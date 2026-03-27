package main

import (
	"log"
	"net/http"
	"path/filepath"

	"github.com/gorilla/websocket"
)

const listenAddr = ":3000"

var upgrader = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool {
		return true
	},
}

func main() {
	mux := http.NewServeMux()
	mux.HandleFunc("/", serveIndex)
	mux.HandleFunc("/ws", handleWS)

	log.Printf("GO08_01s listening on http://localhost%s", listenAddr)
	if err := http.ListenAndServe(listenAddr, mux); err != nil {
		log.Fatalf("server stopped: %v", err)
	}
}

func serveIndex(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}

	http.ServeFile(w, r, filepath.Join("web", "index.html"))
}

func handleWS(w http.ResponseWriter, r *http.Request) {
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("upgrade error: %v", err)
		return
	}
	defer conn.Close()

	log.Printf("client connected: %s", r.RemoteAddr)

	for {
		msgType, data, err := conn.ReadMessage()
		if err != nil {
			if websocket.IsCloseError(err, websocket.CloseNormalClosure, websocket.CloseGoingAway) {
				log.Printf("client closed connection: %s", r.RemoteAddr)
			} else if websocket.IsUnexpectedCloseError(err, websocket.CloseAbnormalClosure) {
				log.Printf("client disconnected unexpectedly: %s: %v", r.RemoteAddr, err)
			} else {
				log.Printf("client disconnected: %s", r.RemoteAddr)
			}
			return
		}

		text := string(data)
		log.Printf("received from %s: %s", r.RemoteAddr, text)

		reply := "from server " + text
		if err := conn.WriteMessage(msgType, []byte(reply)); err != nil {
			log.Printf("write error for %s: %v", r.RemoteAddr, err)
			return
		}
	}
}
