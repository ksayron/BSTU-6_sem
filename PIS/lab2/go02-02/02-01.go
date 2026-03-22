package main

import (
	"fmt"
	"net/http"
	"strings"

	go02_02lib "example.com/go02_02/go02-02lib"
)

var A01 = 3

func handler(w http.ResponseWriter, r *http.Request) {

	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}

	if r.Method != http.MethodGet {
		w.WriteHeader(http.StatusMethodNotAllowed)
		w.Write([]byte("Wrong method"))
		return
	}

	w.Header().Set("Content-Type", "text/plain; charset=utf-8")

	fmt.Fprintf(w, "a01=%d,\n", A01)
	fmt.Fprintf(w, "a02=%t\n", A02)
	fmt.Fprintf(w, "a03=%s", strings.ToLower(go02_02lib.A03))
}

func main() {
	http.HandleFunc("/", handler)
	fmt.Println("GO02_02 server running on :4000")
	if err := http.ListenAndServe(":4000", nil); err != nil {
		fmt.Println("ListenAndServe:", err)
	}
}
