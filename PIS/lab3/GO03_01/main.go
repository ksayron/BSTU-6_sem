package main

import (
	"log"
	"net/http"
)

func logHandler(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		log.Printf("Method: %s, Path: %s", r.Method, r.URL.Path)
		next(w, r)
	}
}

func emptyHandler(w http.ResponseWriter, r *http.Request) {}

func fallbackHandler(w http.ResponseWriter, r *http.Request) {
	log.Printf("Method: %s, Path: %s", r.Method, r.URL.Path)
}

func main() {
	mux := http.NewServeMux()

	for _, method := range []string{"GET", "POST", "PUT"} {
		for _, path := range []string{"/A", "/A/B"} {
			m, p := method, path
			mux.HandleFunc(m+" "+p, logHandler(emptyHandler))
		}
	}

	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		switch r.Method {
		case "GET", "POST", "PUT":
			log.Printf("Method: %s, Path: %s", r.Method, r.URL.Path)
		}
	})

	log.Println("Server starting on port 3000...")
	log.Fatal(http.ListenAndServe(":3000", mux))
}
