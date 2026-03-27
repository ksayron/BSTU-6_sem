package main

import (
	"fmt"
	"log"
	"time"

	"github.com/gorilla/websocket"
)

const wsURL = "ws://localhost:3000/ws"

func main() {
	conn, _, err := websocket.DefaultDialer.Dial(wsURL, nil)
	if err != nil {
		log.Fatalf("dial error: %v", err)
	}
	defer conn.Close()

	log.Printf("connected to %s", wsURL)

	for i := 1; i <= 5; i++ {
		message := fmt.Sprintf("message %d", i)
		log.Printf("send: %s", message)

		if err := conn.WriteMessage(websocket.TextMessage, []byte(message)); err != nil {
			log.Fatalf("write error: %v", err)
		}

		_, reply, err := conn.ReadMessage()
		if err != nil {
			log.Fatalf("read error: %v", err)
		}
		log.Printf("recv: %s", string(reply))

		time.Sleep(time.Second)
	}

	if err := conn.WriteMessage(websocket.CloseMessage, websocket.FormatCloseMessage(websocket.CloseNormalClosure, "client done")); err != nil {
		log.Printf("close frame send error: %v", err)
	}

	log.Print("client finished")
}
