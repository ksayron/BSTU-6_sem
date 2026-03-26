package main

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"
	"os"
	"strconv"
	"strings"

	"github.com/gorilla/mux"
	"gorm.io/driver/sqlserver"
	"gorm.io/gorm"
)

const serverPort = ":3001"

type Celebrity struct {
	Id           int    `json:"id" gorm:"column:Id;primaryKey;autoIncrement:false"`
	FullName     string `json:"fullName" gorm:"column:FullName;type:nvarchar(200);not null"`
	Nationality  string `json:"nationality" gorm:"column:Nationality;type:nvarchar(100);not null"`
	ReqPhotoPath string `json:"reqPhotoPath" gorm:"column:ReqPhotoPath;type:nvarchar(500);not null"`
}

func (Celebrity) TableName() string {
	return "Celebrities"
}

type ErrorResponse struct {
	Error string `json:"error"`
}

type App struct {
	db     *gorm.DB
	logger *log.Logger
}

func main() {
	logger := log.New(os.Stdout, "[GO06_01] ", log.LstdFlags|log.Lshortfile)

	dsn := getConnectionString()
	db, err := gorm.Open(sqlserver.Open(dsn), &gorm.Config{})
	if err != nil {
		logger.Fatalf("gorm.Open error: %v", err)
	}

	logger.Println("Connected to SQL Server successfully")

	if err := db.AutoMigrate(&Celebrity{}); err != nil {
		logger.Fatalf("AutoMigrate error: %v", err)
	}

	app := &App{
		db:     db,
		logger: logger,
	}

	router := mux.NewRouter()
	router.Use(app.loggingMiddleware)

	router.HandleFunc("/Celebrities/All", app.handleGetAll).Methods(http.MethodGet)
	router.HandleFunc("/Celebrities/{id:[0-9]+}", app.handleGetByID).Methods(http.MethodGet)
	router.HandleFunc("/Celebrities", app.handleCreate).Methods(http.MethodPost)
	router.HandleFunc("/Celebrities/{id:[0-9]+}", app.handleUpdate).Methods(http.MethodPut)
	router.HandleFunc("/Celebrities/{id:[0-9]+}", app.handleDelete).Methods(http.MethodDelete)

	logger.Printf("Server started on port %s", serverPort)
	if err := http.ListenAndServe(serverPort, router); err != nil {
		logger.Fatalf("ListenAndServe error: %v", err)
	}
}

func getConnectionString() string {
	if value := os.Getenv("MSSQL_CONN_STRING"); value != "" {
		return value
	}

	host := getEnv("DB_HOST", "WIN-UCLB12VI625")
	port := getEnv("DB_PORT", "14625")
	user := getEnv("DB_USER", "UniversityUser")
	password := getEnv("DB_PASSWORD", "1111")
	database := getEnv("DB_NAME", "Celebrities")

	return "sqlserver://" + user + ":" + password + "@" + host + ":" + port +
		"?database=" + database + "&encrypt=disable&trustservercertificate=true"
}

func getEnv(name, fallback string) string {
	if value := os.Getenv(name); value != "" {
		return value
	}
	return fallback
}

func (app *App) loggingMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		app.logger.Printf("%s %s", r.Method, r.URL.Path)
		next.ServeHTTP(w, r)
	})
}

func (app *App) handleGetAll(w http.ResponseWriter, r *http.Request) {
	var items []Celebrity

	if err := app.db.Order("Id").Find(&items).Error; err != nil {
		app.logger.Printf("handleGetAll error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to read collection")
		return
	}

	writeJSON(w, http.StatusOK, items)
}

func (app *App) handleGetByID(w http.ResponseWriter, r *http.Request) {
	id, err := parseID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	var item Celebrity
	err = app.db.First(&item, "Id = ?", id).Error
	if errors.Is(err, gorm.ErrRecordNotFound) {
		writeError(w, http.StatusNotFound, "celebrity not found")
		return
	}
	if err != nil {
		app.logger.Printf("handleGetByID error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to read item")
		return
	}

	writeJSON(w, http.StatusOK, item)
}

func (app *App) handleCreate(w http.ResponseWriter, r *http.Request) {
	var item Celebrity
	if err := decodeJSON(r, &item); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	if err := app.db.Create(&item).Error; err != nil {
		if isDuplicateKeyError(err) {
			writeError(w, http.StatusConflict, "celebrity with this id already exists")
			return
		}
		app.logger.Printf("handleCreate error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to create item")
		return
	}

	writeJSON(w, http.StatusCreated, item)
}

func (app *App) handleUpdate(w http.ResponseWriter, r *http.Request) {
	id, err := parseID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	var input Celebrity
	if err := decodeJSON(r, &input); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	var existing Celebrity
	err = app.db.First(&existing, "Id = ?", id).Error
	if errors.Is(err, gorm.ErrRecordNotFound) {
		writeError(w, http.StatusNotFound, "celebrity not found")
		return
	}
	if err != nil {
		app.logger.Printf("handleUpdate precheck error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to load item")
		return
	}

	existing.FullName = input.FullName
	existing.Nationality = input.Nationality
	existing.ReqPhotoPath = input.ReqPhotoPath

	if err := app.db.Save(&existing).Error; err != nil {
		app.logger.Printf("handleUpdate save error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to update item")
		return
	}

	writeJSON(w, http.StatusOK, existing)
}

func (app *App) handleDelete(w http.ResponseWriter, r *http.Request) {
	id, err := parseID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	result := app.db.Delete(&Celebrity{}, "Id = ?", id)
	if result.Error != nil {
		app.logger.Printf("handleDelete error: %v", result.Error)
		writeError(w, http.StatusInternalServerError, "failed to delete item")
		return
	}

	if result.RowsAffected == 0 {
		writeError(w, http.StatusNotFound, "celebrity not found")
		return
	}

	writeJSON(w, http.StatusOK, map[string]string{
		"message": "deleted successfully",
	})
}

func parseID(r *http.Request) (int, error) {
	vars := mux.Vars(r)
	return strconv.Atoi(vars["id"])
}

func decodeJSON(r *http.Request, dst any) error {
	contentType := r.Header.Get("Content-Type")
	if contentType != "" && !strings.HasPrefix(contentType, "application/json") {
		return errors.New("Content-Type must be application/json")
	}

	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()

	if err := decoder.Decode(dst); err != nil {
		return err
	}

	return nil
}

func isDuplicateKeyError(err error) bool {
	msg := strings.ToLower(err.Error())
	return strings.Contains(msg, "2627") ||
		strings.Contains(msg, "2601") ||
		strings.Contains(msg, "duplicate key") ||
		strings.Contains(msg, "primary key constraint")
}

func writeJSON(w http.ResponseWriter, status int, payload any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(payload)
}

func writeError(w http.ResponseWriter, status int, message string) {
	writeJSON(w, status, ErrorResponse{Error: message})
}
