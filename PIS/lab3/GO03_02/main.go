package main

import (
	"GO03_02/P03_02"
	"log"
	"net/http"
)

var stats = &P03_02.Stats{}

func handleS(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	case http.MethodGet:
		stats.PlusGet()
		log.Printf("GET /S: incremented GET counter")
	case http.MethodPost:
		stats.PlusPost()
		log.Printf("POST /S: incremented POST counter")
	default:
		http.Error(w, "Method Not Allowed", http.StatusMethodNotAllowed)
	}
}

func handleG(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, "Method Not Allowed", http.StatusMethodNotAllowed)
		return
	}
	w.Header().Set("Content-Type", "text/plain")
	result := stats.GenStr()
	log.Printf("GET /G: %s", result)
	w.Write([]byte(result))
}

func main() {
	mux := http.NewServeMux()

	mux.HandleFunc("/S", handleS)
	mux.HandleFunc("/G", handleG)

	log.Println("Server starting on port 3000...")
	log.Fatal(http.ListenAndServe(":3000", mux))
}
