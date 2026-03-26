package main

import (
	"context"
	"database/sql"
	"encoding/json"
	"errors"
	"log"
	"net/http"
	"os"
	"strconv"
	"strings"
	"time"

	_ "github.com/denisenkom/go-mssqldb"
	"github.com/gorilla/mux"
)

const serverPort = ":3000"

type Celebrity struct {
	Id           int    `json:"id"`
	FullName     string `json:"fullName"`
	Nationality  string `json:"nationality"`
	ReqPhotoPath string `json:"reqPhotoPath"`
}

type ErrorResponse struct {
	Error string `json:"error"`
}

type App struct {
	db     *sql.DB
	logger *log.Logger
}

func main() {
	logger := log.New(os.Stdout, "[GO05_01] ", log.LstdFlags|log.Lshortfile)

	connString := getConnectionString()
	db, err := sql.Open("sqlserver", connString)
	if err != nil {
		logger.Fatalf("sql.Open error: %v", err)
	}
	defer db.Close()

	if err := db.Ping(); err != nil {
		logger.Fatalf("db.Ping error: %v", err)
	}

	logger.Println("Connected to SQL Server successfully")

	app := &App{
		db:     db,
		logger: logger,
	}

	if err := app.ensureTable(); err != nil {
		logger.Fatalf("ensureTable error: %v", err)
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

func (app *App) ensureTable() error {
	query := `
IF NOT EXISTS (
	SELECT 1
	FROM sys.tables
	WHERE name = 'Celebrities'
)
BEGIN
	CREATE TABLE Celebrities (
		Id INT NOT NULL PRIMARY KEY,
		FullName NVARCHAR(200) NOT NULL,
		Nationality NVARCHAR(100) NOT NULL,
		ReqPhotoPath NVARCHAR(500) NOT NULL
	)
END
`
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	_, err := app.db.ExecContext(ctx, query)
	return err
}

func (app *App) loggingMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		app.logger.Printf("%s %s", r.Method, r.URL.Path)
		next.ServeHTTP(w, r)
	})
}

func (app *App) handleGetAll(w http.ResponseWriter, r *http.Request) {
	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	rows, err := app.db.QueryContext(ctx, `
SELECT Id, FullName, Nationality, ReqPhotoPath
FROM Celebrities
ORDER BY Id
`)
	if err != nil {
		app.logger.Printf("handleGetAll query error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to read collection")
		return
	}
	defer rows.Close()

	items := make([]Celebrity, 0)
	for rows.Next() {
		var item Celebrity
		if err := rows.Scan(&item.Id, &item.FullName, &item.Nationality, &item.ReqPhotoPath); err != nil {
			app.logger.Printf("handleGetAll scan error: %v", err)
			writeError(w, http.StatusInternalServerError, "failed to parse collection")
			return
		}
		items = append(items, item)
	}

	if err := rows.Err(); err != nil {
		app.logger.Printf("handleGetAll rows error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to iterate collection")
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

	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	var item Celebrity
	err = app.db.QueryRowContext(ctx, `
SELECT Id, FullName, Nationality, ReqPhotoPath
FROM Celebrities
WHERE Id = @p1
`, id).Scan(&item.Id, &item.FullName, &item.Nationality, &item.ReqPhotoPath)

	if errors.Is(err, sql.ErrNoRows) {
		writeError(w, http.StatusNotFound, "celebrity not found")
		return
	}
	if err != nil {
		app.logger.Printf("handleGetByID query error: %v", err)
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

	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	_, err := app.db.ExecContext(ctx, `
INSERT INTO Celebrities (Id, FullName, Nationality, ReqPhotoPath)
VALUES (@p1, @p2, @p3, @p4)
`, item.Id, item.FullName, item.Nationality, item.ReqPhotoPath)

	if err != nil {
		if isDuplicateKeyError(err) {
			writeError(w, http.StatusConflict, "celebrity with this id already exists")
			return
		}
		app.logger.Printf("handleCreate insert error: %v", err)
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

	var item Celebrity
	if err := decodeJSON(r, &item); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	result, err := app.db.ExecContext(ctx, `
UPDATE Celebrities
SET FullName = @p1,
	Nationality = @p2,
	ReqPhotoPath = @p3
WHERE Id = @p4
`, item.FullName, item.Nationality, item.ReqPhotoPath, id)

	if err != nil {
		app.logger.Printf("handleUpdate update error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to update item")
		return
	}

	affected, err := result.RowsAffected()
	if err != nil {
		app.logger.Printf("handleUpdate rows affected error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to verify update")
		return
	}
	if affected == 0 {
		writeError(w, http.StatusNotFound, "celebrity not found")
		return
	}

	item.Id = id
	writeJSON(w, http.StatusOK, item)
}

func (app *App) handleDelete(w http.ResponseWriter, r *http.Request) {
	id, err := parseID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	ctx, cancel := context.WithTimeout(r.Context(), 5*time.Second)
	defer cancel()

	result, err := app.db.ExecContext(ctx, `
DELETE FROM Celebrities
WHERE Id = @p1
`, id)

	if err != nil {
		app.logger.Printf("handleDelete delete error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to delete item")
		return
	}

	affected, err := result.RowsAffected()
	if err != nil {
		app.logger.Printf("handleDelete rows affected error: %v", err)
		writeError(w, http.StatusInternalServerError, "failed to verify delete")
		return
	}
	if affected == 0 {
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
