package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"sync"
)

type Request struct {
	Op string  `json:"op"`
	X  float64 `json:"x"`
	Y  float64 `json:"y"`
}

type Response struct {
	Op     string  `json:"op"`
	X      float64 `json:"x"`
	Y      float64 `json:"y"`
	Result float64 `json:"result"`
}

var (
	store Response
	mu    sync.Mutex
)

func calculate(op string, x, y float64) (float64, error) {
	switch op {
	case "add":
		return x + y, nil
	case "sub":
		return x - y, nil
	case "mul":
		return x * y, nil
	case "div":
		if y == 0 {
			return 0, fmt.Errorf("division by zero")
		}
		return x / y, nil
	default:
		return 0, fmt.Errorf("unknown operation")
	}
}

func handler(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/NGINX-test:40000" {
		http.NotFound(w, r)
		return
	}

	w.Header().Set("Content-Type", "application/json")

	switch r.Method {

	case http.MethodGet:

		var found bool = false

		mu.Lock()
		if store != (Response{}) {
			found = true
		}
		mu.Unlock()

		if !found {
			http.Error(w, "JSON request not found", http.StatusNotFound)
			return
		}

		json.NewEncoder(w).Encode(store)

	case http.MethodPost:
		var req Request
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, err.Error(), http.StatusBadRequest)
			return
		}

		result, err := calculate(req.Op, req.X, req.Y)
		if err != nil {
			http.Error(w, err.Error(), http.StatusBadRequest)
			return
		}

		resp := Response{
			Op:     req.Op,
			X:      req.X,
			Y:      req.Y,
			Result: result,
		}

		mu.Lock()
		store = resp
		mu.Unlock()

		json.NewEncoder(w).Encode(resp)

	case http.MethodPut:
		var req Request
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			http.Error(w, err.Error(), http.StatusBadRequest)
			return
		}

		var found bool

		mu.Lock()
		if store != (Response{}) {
			found = true
		}
		mu.Unlock()
		if !found {
			http.Error(w, "JSON request not found", http.StatusNotFound)
			return
		}

		result, err := calculate(req.Op, req.X, req.Y)
		if err != nil {
			http.Error(w, err.Error(), http.StatusBadRequest)
			return
		}

		resp := Response{
			Op:     req.Op,
			X:      req.X,
			Y:      req.Y,
			Result: result,
		}

		mu.Lock()
		store = resp
		mu.Unlock()

		json.NewEncoder(w).Encode(resp)

	case http.MethodDelete:

		mu.Lock()
		found := store != Response{}
		if found {
			store = Response{}
		}
		mu.Unlock()

		if !found {
			http.Error(w, "JSON request not found", http.StatusNotFound)
			return
		}

		w.WriteHeader(http.StatusOK)
		w.Write([]byte(`{"message":"Deleted successfully"}`))

	default:
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
	}
}

func main() {
	http.HandleFunc("/NGINX-test:40000", handler)

	fmt.Println("Server running on :40000")
	log.Fatal(http.ListenAndServe(":40000", nil))
}
