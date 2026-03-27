package main

import (
	"bytes"
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"log"
	"math"
	"net/http"
	"sync"

	"github.com/gorilla/mux"
)

const (
	listenAddr = ":3000"
	jsonRPCVer = "2.0"
)

type rpcRequest struct {
	JSONRPC string          `json:"jsonrpc"`
	Method  string          `json:"method"`
	Params  json.RawMessage `json:"params,omitempty"`
	ID      json.RawMessage `json:"id,omitempty"`
}

type rpcResponse struct {
	JSONRPC string      `json:"jsonrpc"`
	Result  any         `json:"result,omitempty"`
	Error   *rpcError   `json:"error,omitempty"`
	ID      interface{} `json:"id"`
}

type rpcError struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
}

type xyParams struct {
	X float64 `json:"x"`
	Y float64 `json:"y"`
}

type precisionParams struct {
	N int `json:"N"`
}

type xyParamMap map[string]json.RawMessage
type precisionParamMap map[string]json.RawMessage

type serverState struct {
	mu        sync.RWMutex
	precision int
}

func (s *serverState) setPrecision(n int) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.precision = n
}

func (s *serverState) getPrecision() int {
	s.mu.RLock()
	defer s.mu.RUnlock()
	return s.precision
}

func main() {
	state := &serverState{precision: 2}

	router := mux.NewRouter()
	router.HandleFunc("/", state.handleJSONRPC).Methods(http.MethodPost)
	router.HandleFunc("/rpc", state.handleJSONRPC).Methods(http.MethodPost)

	log.Printf("GO07_01 listening on http://localhost%s", listenAddr)
	if err := http.ListenAndServe(listenAddr, router); err != nil {
		log.Fatalf("server stopped: %v", err)
	}
}

func (s *serverState) handleJSONRPC(w http.ResponseWriter, r *http.Request) {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		log.Printf("read body error: %v", err)
		writeSingleResponse(w, errorResponse(nil, -32700, "Parse error"))
		return
	}

	log.Printf("request: %s", bytes.TrimSpace(body))

	trimmed := bytes.TrimSpace(body)
	if len(trimmed) == 0 {
		writeSingleResponse(w, errorResponse(nil, -32700, "Parse error"))
		return
	}

	if trimmed[0] == '[' {
		s.handleBatch(w, trimmed)
		return
	}

	var req rpcRequest
	if err := json.Unmarshal(trimmed, &req); err != nil {
		log.Printf("unmarshal request error: %v", err)
		writeSingleResponse(w, errorResponse(nil, -32700, "Parse error"))
		return
	}

	resp, shouldReply := s.processRequest(req)
	if !shouldReply {
		w.WriteHeader(http.StatusNoContent)
		return
	}

	writeSingleResponse(w, resp)
}

func (s *serverState) handleBatch(w http.ResponseWriter, body []byte) {
	var items []json.RawMessage
	if err := json.Unmarshal(body, &items); err != nil {
		log.Printf("unmarshal batch error: %v", err)
		writeSingleResponse(w, errorResponse(nil, -32700, "Parse error"))
		return
	}

	if len(items) == 0 {
		writeSingleResponse(w, errorResponse(nil, -32600, "Invalid Request"))
		return
	}

	responses := make([]rpcResponse, 0, len(items))
	for _, item := range items {
		var req rpcRequest
		if err := json.Unmarshal(item, &req); err != nil {
			responses = append(responses, errorResponse(nil, -32600, "Invalid Request"))
			continue
		}

		resp, shouldReply := s.processRequest(req)
		if shouldReply {
			responses = append(responses, resp)
		}
	}

	if len(responses) == 0 {
		w.WriteHeader(http.StatusNoContent)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	if err := json.NewEncoder(w).Encode(responses); err != nil {
		log.Printf("encode batch response error: %v", err)
	}
}

func (s *serverState) processRequest(req rpcRequest) (rpcResponse, bool) {
	if req.JSONRPC != jsonRPCVer || req.Method == "" {
		return errorResponse(extractID(req.ID), -32600, "Invalid Request"), true
	}

	switch req.Method {
	case "sum", "sub", "mul", "div":
		x, y, err := parseXYParams(req.Params)
		if err != nil {
			return errorResponse(extractID(req.ID), -32602, "Invalid params"), hasID(req.ID)
		}

		var result float64
		switch req.Method {
		case "sum":
			result = x + y
		case "sub":
			result = x - y
		case "mul":
			result = x * y
		case "div":
			if y == 0 {
				return errorResponse(extractID(req.ID), -32602, "Division by zero"), hasID(req.ID)
			}
			result = x / y
		}

		result = round(result, s.getPrecision())
		log.Printf("method=%s x=%v y=%v result=%v", req.Method, x, y, result)

		if !hasID(req.ID) {
			return rpcResponse{}, false
		}

		return rpcResponse{
			JSONRPC: jsonRPCVer,
			Result:  result,
			ID:      extractID(req.ID),
		}, true

	case "pre":
		n, err := parsePrecisionParams(req.Params)
		if err != nil || n < 0 {
			return errorResponse(extractID(req.ID), -32602, "Invalid params"), hasID(req.ID)
		}

		s.setPrecision(n)
		log.Printf("notification=pre precision=%d", n)

		if !hasID(req.ID) {
			return rpcResponse{}, false
		}

		return rpcResponse{
			JSONRPC: jsonRPCVer,
			Result:  "ok",
			ID:      extractID(req.ID),
		}, true

	default:
		return errorResponse(extractID(req.ID), -32601, "Method not found"), hasID(req.ID)
	}
}

func parseXYParams(raw json.RawMessage) (float64, float64, error) {
	if len(raw) == 0 {
		return 0, 0, errors.New("missing params")
	}

	trimmed := bytes.TrimSpace(raw)
	switch {
	case len(trimmed) > 0 && trimmed[0] == '[':
		var arr []float64
		if err := json.Unmarshal(trimmed, &arr); err != nil {
			return 0, 0, err
		}
		if len(arr) != 2 {
			return 0, 0, errors.New("expected two values")
		}
		return arr[0], arr[1], nil

	case len(trimmed) > 0 && trimmed[0] == '{':
		var rawMap xyParamMap
		if err := json.Unmarshal(trimmed, &rawMap); err != nil {
			return 0, 0, err
		}
		if _, ok := rawMap["x"]; !ok {
			return 0, 0, errors.New("missing x")
		}
		if _, ok := rawMap["y"]; !ok {
			return 0, 0, errors.New("missing y")
		}

		var p xyParams
		if err := json.Unmarshal(trimmed, &p); err != nil {
			return 0, 0, err
		}
		return p.X, p.Y, nil
	}

	return 0, 0, errors.New("unsupported params format")
}

func parsePrecisionParams(raw json.RawMessage) (int, error) {
	if len(raw) == 0 {
		return 0, errors.New("missing params")
	}

	var rawMap precisionParamMap
	if err := json.Unmarshal(raw, &rawMap); err != nil {
		return 0, err
	}
	if _, ok := rawMap["N"]; !ok {
		return 0, errors.New("missing N")
	}

	var p precisionParams
	if err := json.Unmarshal(raw, &p); err != nil {
		return 0, err
	}
	return p.N, nil
}

func round(value float64, precision int) float64 {
	factor := math.Pow10(precision)
	return math.Round(value*factor) / factor
}

func hasID(id json.RawMessage) bool {
	return len(bytes.TrimSpace(id)) > 0
}

func extractID(raw json.RawMessage) interface{} {
	if !hasID(raw) {
		return nil
	}

	var id interface{}
	if err := json.Unmarshal(raw, &id); err != nil {
		return nil
	}
	return id
}

func errorResponse(id interface{}, code int, message string) rpcResponse {
	return rpcResponse{
		JSONRPC: jsonRPCVer,
		Error: &rpcError{
			Code:    code,
			Message: message,
		},
		ID: id,
	}
}

func writeSingleResponse(w http.ResponseWriter, resp rpcResponse) {
	w.Header().Set("Content-Type", "application/json")
	if err := json.NewEncoder(w).Encode(resp); err != nil {
		log.Printf("encode response error: %v", err)
		http.Error(w, fmt.Sprintf("encode response error: %v", err), http.StatusInternalServerError)
	}
}
