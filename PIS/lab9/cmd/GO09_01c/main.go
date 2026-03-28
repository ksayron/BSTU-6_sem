package main

import (
	"flag"
	"fmt"
	"io"
	"log"
	"net/http"
	"net/url"
	"os"
	"strings"
)

const (
	testDir       = "/go09_test_data/"
	originalFile  = testDir + "original.txt"
	copiedFile    = testDir + "copied.txt"
	movedFile     = testDir + "moved.txt"
	testFileBody  = "GO09_01c test content"
	defaultServer = "http://localhost:3000"
)

type step struct {
	name   string
	method string
	src    string
	dst    string
	body   string
	check  func(status int, responseBody string) error
}

func main() {
	base := flag.String("base", defaultServer, "webdav base url")
	user := flag.String("user", "", "basic auth username")
	pass := flag.String("pass", "", "basic auth password")
	flag.Parse()

	logger := log.New(os.Stdout, "[GO09_01c] ", log.LstdFlags|log.Lshortfile)
	client := &http.Client{}

	logger.Printf("test run started, server=%s", *base)
	if err := runScenario(client, logger, *base, *user, *pass); err != nil {
		logger.Fatalf("test run failed: %v", err)
	}

	logger.Println("all tests passed")
}

func runScenario(client *http.Client, logger *log.Logger, base, user, pass string) error {
	cleanupPlan := []string{movedFile, copiedFile, originalFile, testDir}
	// Best effort pre-cleanup so repeated runs do not fail with 409 Conflict.
	cleanup(client, logger, base, user, pass, cleanupPlan)
	defer cleanup(client, logger, base, user, pass, cleanupPlan)

	steps := []step{
		{
			name:   "MKCOL test directory",
			method: "MKCOL",
			src:    testDir,
			check: func(status int, _ string) error {
				if status >= 200 && status < 300 {
					return nil
				}
				if status == http.StatusConflict || status == http.StatusMethodNotAllowed {
					return nil
				}
				return fmt.Errorf("expected 2xx/409/405 status, got %d", status)
			},
		},
		{
			name:   "PUT original file",
			method: http.MethodPut,
			src:    originalFile,
			body:   testFileBody,
			check:  expect2xx,
		},
		{
			name:   "GET original file",
			method: http.MethodGet,
			src:    originalFile,
			check: func(status int, responseBody string) error {
				if err := expect2xx(status, responseBody); err != nil {
					return err
				}
				if responseBody != testFileBody {
					return fmt.Errorf("unexpected GET content: %q", responseBody)
				}
				return nil
			},
		},
		{
			name:   "COPY original -> copied",
			method: "COPY",
			src:    originalFile,
			dst:    copiedFile,
			check:  expect2xx,
		},
		{
			name:   "MOVE copied -> moved",
			method: "MOVE",
			src:    copiedFile,
			dst:    movedFile,
			check:  expect2xx,
		},
		{
			name:   "GET moved file",
			method: http.MethodGet,
			src:    movedFile,
			check: func(status int, responseBody string) error {
				if err := expect2xx(status, responseBody); err != nil {
					return err
				}
				if responseBody != testFileBody {
					return fmt.Errorf("unexpected moved file content: %q", responseBody)
				}
				return nil
			},
		},
		{
			name:   "DELETE moved file",
			method: http.MethodDelete,
			src:    movedFile,
			check:  expect2xx,
		},
		{
			name:   "DELETE original file",
			method: http.MethodDelete,
			src:    originalFile,
			check:  expect2xx,
		},
		{
			name:   "DELETE test directory",
			method: http.MethodDelete,
			src:    testDir,
			check: func(status int, _ string) error {
				if status >= 200 && status < 300 {
					return nil
				}
				if status == http.StatusConflict {
					return nil
				}
				return fmt.Errorf("expected 2xx/409 status, got %d", status)
			},
		},
	}

	for i, st := range steps {
		status, responseBody, err := doRequest(client, st.method, base, st.src, st.dst, st.body, user, pass)
		if err != nil {
			return fmt.Errorf("step %d (%s) request error: %w", i+1, st.name, err)
		}
		if err := st.check(status, responseBody); err != nil {
			return fmt.Errorf("step %d (%s) failed: %w; status=%d body=%q", i+1, st.name, err, status, responseBody)
		}
		logger.Printf("step %d OK: %s (status=%d)", i+1, st.name, status)
	}

	return nil
}

func cleanup(client *http.Client, logger *log.Logger, base, user, pass string, paths []string) {
	for _, p := range paths {
		status, _, err := doRequest(client, http.MethodDelete, base, p, "", "", user, pass)
		if err != nil {
			logger.Printf("cleanup request error for %s: %v", p, err)
			continue
		}
		if status >= 200 && status < 300 {
			logger.Printf("cleanup removed: %s", p)
		}
	}
}

func doRequest(client *http.Client, method, base, src, dst, requestBody, user, pass string) (int, string, error) {
	reqURL, err := joinURL(base, src)
	if err != nil {
		return 0, "", err
	}

	var body io.Reader
	if method == http.MethodPut {
		body = strings.NewReader(requestBody)
	}

	req, err := http.NewRequest(method, reqURL, body)
	if err != nil {
		return 0, "", err
	}

	if user != "" || pass != "" {
		req.SetBasicAuth(user, pass)
	}

	if method == http.MethodPut {
		req.Header.Set("Content-Type", "text/plain")
	}

	if method == "COPY" || method == "MOVE" {
		if dst == "" {
			return 0, "", fmt.Errorf("destination is required for %s", method)
		}
		destURL, err := joinURL(base, dst)
		if err != nil {
			return 0, "", err
		}
		req.Header.Set("Destination", destURL)
	}

	resp, err := client.Do(req)
	if err != nil {
		return 0, "", err
	}
	defer resp.Body.Close()

	respData, err := io.ReadAll(resp.Body)
	if err != nil {
		return 0, "", err
	}

	return resp.StatusCode, string(respData), nil
}

func expect2xx(status int, _ string) error {
	if status < 200 || status >= 300 {
		return fmt.Errorf("expected 2xx status, got %d", status)
	}
	return nil
}

func joinURL(base, p string) (string, error) {
	u, err := url.Parse(base)
	if err != nil {
		return "", err
	}

	if !strings.HasPrefix(p, "/") {
		p = "/" + p
	}

	u.Path = strings.TrimRight(u.Path, "/") + p
	return u.String(), nil
}
