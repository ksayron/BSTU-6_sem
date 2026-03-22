package main

import (
	"fmt"
	"net/http"

	go02_01lib "example.com/go02_01/go02-01lib"
)

const C01 = 3.14

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

	fmt.Fprintf(w, "c01=%0.6e,\n", C01)
	fmt.Fprintf(w, "c02=%0.6e,\n", C02)
	fmt.Fprintf(w, "c03=%0.6e", go02_01lib.C03)
}

func main() {
	http.HandleFunc("/", handler)
	fmt.Println("GO02_01 server running on :3000")
	if err := http.ListenAndServe(":3000", nil); err != nil {
		fmt.Println("ListenAndServe:", err)
	}
}
