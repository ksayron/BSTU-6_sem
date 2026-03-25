package main

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"strconv"
	"sync"

	"github.com/gorilla/mux"
)

const (
	serverPort = ":3000"
	dataFile   = "Celebrities.json"
)

type Celebrity struct {
	Id           int    `json:"id"`
	FullName     string `json:"fullName"`
	Nationality  string `json:"nationality"`
	ReqPhotoPath string `json:"reqPhotoPath"`
}

type Store struct {
	mu   sync.RWMutex
	path string
	data []Celebrity
}

func NewStore(path string) (*Store, error) {
	s := &Store{path: path}
	if err := s.load(); err != nil {
		return nil, err
	}
	return s, nil
}

func (s *Store) load() error {
	s.mu.Lock()
	defer s.mu.Unlock()

	if _, err := os.Stat(s.path); errors.Is(err, os.ErrNotExist) {
		s.data = []Celebrity{}
		return s.saveLocked()
	}

	bytes, err := os.ReadFile(s.path)
	if err != nil {
		return err
	}

	if len(bytes) == 0 {
		s.data = []Celebrity{}
		return nil
	}

	var items []Celebrity
	if err := json.Unmarshal(bytes, &items); err != nil {
		return err
	}

	s.data = items
	return nil
}

func (s *Store) saveLocked() error {
	bytes, err := json.MarshalIndent(s.data, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(s.path, bytes, 0644)
}

func (s *Store) GetAll() []Celebrity {
	s.mu.RLock()
	defer s.mu.RUnlock()

	result := make([]Celebrity, len(s.data))
	copy(result, s.data)
	return result
}

func (s *Store) GetByID(id int) (Celebrity, bool) {
	s.mu.RLock()
	defer s.mu.RUnlock()

	for _, item := range s.data {
		if item.Id == id {
			return item, true
		}
	}
	return Celebrity{}, false
}

func (s *Store) Add(item Celebrity) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, existing := range s.data {
		if existing.Id == item.Id {
			return ErrConflict
		}
	}

	s.data = append(s.data, item)
	return s.saveLocked()
}

func (s *Store) Update(id int, item Celebrity) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for i, existing := range s.data {
		if existing.Id == id {
			item.Id = id
			s.data[i] = item
			return s.saveLocked()
		}
	}

	return ErrNotFound
}

func (s *Store) Delete(id int) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for i, item := range s.data {
		if item.Id == id {
			s.data = append(s.data[:i], s.data[i+1:]...)
			return s.saveLocked()
		}
	}

	return ErrNotFound
}

var (
	ErrNotFound = errors.New("celebrity not found")
	ErrConflict = errors.New("celebrity with this id already exists")
)

type API struct {
	store  *Store
	logger *log.Logger
}

func main() {
	logger := log.New(os.Stdout, "[GO04_01] ", log.LstdFlags|log.Lshortfile)

	exePath, err := os.Executable()
	if err != nil {
		logger.Fatalf("failed to determine executable path: %v", err)
	}

	dataPath := filepath.Join(filepath.Dir(exePath), dataFile)
	store, err := NewStore(dataPath)
	if err != nil {
		logger.Fatalf("failed to initialize store: %v", err)
	}

	api := &API{
		store:  store,
		logger: logger,
	}

	router := mux.NewRouter()
	router.Use(api.loggingMiddleware)

	router.HandleFunc("/Celebrities/All", api.handleGetAll).Methods(http.MethodGet)
	router.HandleFunc("/Celebrities/{id:[0-9]+}", api.handleGetByID).Methods(http.MethodGet)
	router.HandleFunc("/Celebrities", api.handleCreate).Methods(http.MethodPost)
	router.HandleFunc("/Celebrities/{id:[0-9]+}", api.handleUpdate).Methods(http.MethodPut)
	router.HandleFunc("/Celebrities/{id:[0-9]+}", api.handleDelete).Methods(http.MethodDelete)

	logger.Printf("server started on port %s", serverPort)
	if err := http.ListenAndServe(serverPort, router); err != nil {
		logger.Fatalf("server stopped with error: %v", err)
	}
}

func (api *API) loggingMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		api.logger.Printf("%s %s", r.Method, r.URL.Path)
		next.ServeHTTP(w, r)
	})
}

func (api *API) handleGetAll(w http.ResponseWriter, r *http.Request) {
	writeJSON(w, http.StatusOK, api.store.GetAll())
}

func (api *API) handleGetByID(w http.ResponseWriter, r *http.Request) {
	id, err := getID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	item, ok := api.store.GetByID(id)
	if !ok {
		writeError(w, http.StatusNotFound, ErrNotFound.Error())
		return
	}

	writeJSON(w, http.StatusOK, item)
}

func (api *API) handleCreate(w http.ResponseWriter, r *http.Request) {
	var item Celebrity
	if err := decodeJSON(r, &item); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	if err := api.store.Add(item); err != nil {
		if errors.Is(err, ErrConflict) {
			writeError(w, http.StatusConflict, err.Error())
			return
		}
		writeError(w, http.StatusInternalServerError, "failed to save item")
		return
	}

	writeJSON(w, http.StatusCreated, item)
}

func (api *API) handleUpdate(w http.ResponseWriter, r *http.Request) {
	id, err := getID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	var item Celebrity
	if err := decodeJSON(r, &item); err != nil {
		writeError(w, http.StatusBadRequest, err.Error())
		return
	}

	if err := api.store.Update(id, item); err != nil {
		if errors.Is(err, ErrNotFound) {
			writeError(w, http.StatusNotFound, err.Error())
			return
		}
		writeError(w, http.StatusInternalServerError, "failed to update item")
		return
	}

	item.Id = id
	writeJSON(w, http.StatusOK, item)
}

func (api *API) handleDelete(w http.ResponseWriter, r *http.Request) {
	id, err := getID(r)
	if err != nil {
		writeError(w, http.StatusBadRequest, "invalid id")
		return
	}

	if err := api.store.Delete(id); err != nil {
		if errors.Is(err, ErrNotFound) {
			writeError(w, http.StatusNotFound, err.Error())
			return
		}
		writeError(w, http.StatusInternalServerError, "failed to delete item")
		return
	}

	writeJSON(w, http.StatusOK, map[string]string{
		"message": "deleted successfully",
	})
}

func getID(r *http.Request) (int, error) {
	vars := mux.Vars(r)
	return strconv.Atoi(vars["id"])
}

func decodeJSON(r *http.Request, target any) error {
	if r.Header.Get("Content-Type") != "" && r.Header.Get("Content-Type") != "application/json" {
		return errors.New("Content-Type must be application/json")
	}

	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()

	if err := decoder.Decode(target); err != nil {
		return err
	}

	return nil
}

func writeJSON(w http.ResponseWriter, status int, payload any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(payload)
}

func writeError(w http.ResponseWriter, status int, message string) {
	writeJSON(w, status, map[string]string{
		"error": message,
	})
}
