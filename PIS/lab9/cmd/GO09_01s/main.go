package main

import (
	"errors"
	"flag"
	"io"
	"log"
	"net/http"
	"net/url"
	"os"
	"path/filepath"
	"strings"
)

type server struct {
	root   string
	user   string
	pass   string
	logger *log.Logger
}

func main() {
	addr := flag.String("addr", ":3000", "listen address")
	root := flag.String("root", "./storage", "storage root directory")
	user := flag.String("user", "", "basic auth username (optional)")
	pass := flag.String("pass", "", "basic auth password (optional)")
	flag.Parse()

	if err := os.MkdirAll(*root, 0o755); err != nil {
		log.Fatalf("create root dir error: %v", err)
	}

	s := &server{
		root:   *root,
		user:   *user,
		pass:   *pass,
		logger: log.New(os.Stdout, "[GO09_01s] ", log.LstdFlags|log.Lshortfile),
	}

	s.logger.Printf("server root: %s", *root)
	s.logger.Printf("listen on http://localhost%s", *addr)

	if err := http.ListenAndServe(*addr, s); err != nil {
		s.logger.Fatalf("listen error: %v", err)
	}
}

func (s *server) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	s.logger.Printf("%s %s", r.Method, r.URL.Path)

	if !s.authorized(r) {
		w.Header().Set("WWW-Authenticate", `Basic realm="webdav"`)
		http.Error(w, "unauthorized", http.StatusUnauthorized)
		return
	}

	switch r.Method {
	case "MKCOL":
		s.handleMKCOL(w, r)
	case http.MethodPut:
		s.handlePUT(w, r)
	case http.MethodGet:
		s.handleGET(w, r)
	case "COPY":
		s.handleCOPY(w, r)
	case "MOVE":
		s.handleMOVE(w, r)
	case http.MethodDelete:
		s.handleDELETE(w, r)
	default:
		w.Header().Set("Allow", "MKCOL, PUT, GET, COPY, MOVE, DELETE")
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
	}
}

func (s *server) authorized(r *http.Request) bool {
	if s.user == "" && s.pass == "" {
		return true
	}
	u, p, ok := r.BasicAuth()
	return ok && u == s.user && p == s.pass
}

func (s *server) handleMKCOL(w http.ResponseWriter, r *http.Request) {
	target, err := s.resolvePath(r.URL.Path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	if _, err := os.Stat(target); err == nil {
		http.Error(w, "collection exists", http.StatusMethodNotAllowed)
		return
	}

	if err := os.MkdirAll(target, 0o755); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	w.WriteHeader(http.StatusCreated)
}

func (s *server) handlePUT(w http.ResponseWriter, r *http.Request) {
	target, err := s.resolvePath(r.URL.Path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	parent := filepath.Dir(target)
	if err := os.MkdirAll(parent, 0o755); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	f, err := os.Create(target)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	defer f.Close()

	if _, err := io.Copy(f, r.Body); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusCreated)
}

func (s *server) handleGET(w http.ResponseWriter, r *http.Request) {
	target, err := s.resolvePath(r.URL.Path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}
	http.ServeFile(w, r, target)
}

func (s *server) handleCOPY(w http.ResponseWriter, r *http.Request) {
	src, err := s.resolvePath(r.URL.Path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	dst, err := s.resolveDestination(r)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	if err := copyPath(src, dst); err != nil {
		if errors.Is(err, os.ErrNotExist) {
			http.Error(w, "source not found", http.StatusNotFound)
			return
		}
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusCreated)
}

func (s *server) handleMOVE(w http.ResponseWriter, r *http.Request) {
	src, err := s.resolvePath(r.URL.Path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	dst, err := s.resolveDestination(r)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	if err := os.MkdirAll(filepath.Dir(dst), 0o755); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	if err := os.Rename(src, dst); err != nil {
		if errors.Is(err, os.ErrNotExist) {
			http.Error(w, "source not found", http.StatusNotFound)
			return
		}
		if err := copyPath(src, dst); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		if err := os.RemoveAll(src); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
	}

	w.WriteHeader(http.StatusCreated)
}

func (s *server) handleDELETE(w http.ResponseWriter, r *http.Request) {
	target, err := s.resolvePath(r.URL.Path)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	if err := os.RemoveAll(target); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}

func (s *server) resolveDestination(r *http.Request) (string, error) {
	destRaw := r.Header.Get("Destination")
	if destRaw == "" {
		return "", errors.New("missing Destination header")
	}

	u, err := url.Parse(destRaw)
	if err != nil {
		return "", err
	}

	path := u.Path
	if path == "" {
		path = destRaw
	}

	return s.resolvePath(path)
}

func (s *server) resolvePath(requestPath string) (string, error) {
	cleaned := filepath.Clean("/" + requestPath)
	cleaned = strings.TrimPrefix(cleaned, "/")
	full := filepath.Join(s.root, cleaned)

	rootAbs, err := filepath.Abs(s.root)
	if err != nil {
		return "", err
	}
	fullAbs, err := filepath.Abs(full)
	if err != nil {
		return "", err
	}

	if fullAbs != rootAbs && !strings.HasPrefix(fullAbs, rootAbs+string(filepath.Separator)) {
		return "", errors.New("invalid path")
	}

	return fullAbs, nil
}

func copyPath(src, dst string) error {
	info, err := os.Stat(src)
	if err != nil {
		return err
	}

	if info.IsDir() {
		return copyDir(src, dst)
	}
	return copyFile(src, dst)
}

func copyDir(srcDir, dstDir string) error {
	if err := os.MkdirAll(dstDir, 0o755); err != nil {
		return err
	}

	entries, err := os.ReadDir(srcDir)
	if err != nil {
		return err
	}

	for _, entry := range entries {
		srcPath := filepath.Join(srcDir, entry.Name())
		dstPath := filepath.Join(dstDir, entry.Name())
		if entry.IsDir() {
			if err := copyDir(srcPath, dstPath); err != nil {
				return err
			}
			continue
		}
		if err := copyFile(srcPath, dstPath); err != nil {
			return err
		}
	}
	return nil
}

func copyFile(src, dst string) error {
	if err := os.MkdirAll(filepath.Dir(dst), 0o755); err != nil {
		return err
	}

	in, err := os.Open(src)
	if err != nil {
		return err
	}
	defer in.Close()

	out, err := os.Create(dst)
	if err != nil {
		return err
	}
	defer out.Close()

	if _, err := io.Copy(out, in); err != nil {
		return err
	}
	return out.Sync()
}
