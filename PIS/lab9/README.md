# GO09_01

## 1) NGINX WebDAV + auth + cadaver (Ubuntu/Debian)

```bash
sudo apt update
sudo apt install -y nginx apache2-utils cadaver
```

Create WebDAV directory and user:

```bash
sudo mkdir -p /var/webdav
sudo chown -R www-data:www-data /var/webdav
sudo htpasswd -c /etc/nginx/webdav.passwd student
```

NGINX config example (`/etc/nginx/sites-available/webdav.conf`):

```nginx
server {
    listen 3000;
    server_name _;

    location / {
        root /var/webdav;
        dav_methods PUT DELETE MKCOL COPY MOVE;
        dav_access user:rw group:rw all:r;
        create_full_put_path on;
        autoindex on;

        auth_basic "WebDAV";
        auth_basic_user_file /etc/nginx/webdav.passwd;
    }
}
```

Enable config and restart:

```bash
sudo ln -s /etc/nginx/sites-available/webdav.conf /etc/nginx/sites-enabled/webdav.conf
sudo nginx -t
sudo systemctl restart nginx
```

Check with cadaver:

```bash
cadaver http://localhost:3000/
```

Inside cadaver:

```text
mkcol docs
put local.txt docs/local.txt
get docs/local.txt
copy docs/local.txt docs/local-copy.txt
move docs/local-copy.txt docs/moved.txt
delete docs/moved.txt
```

## 2) GO09_01c WebDAV client

Module path:

- `cmd/GO09_01c/main.go`

Build:

```bash
go build ./...
```

Examples:

```bash
go run ./cmd/GO09_01c -base http://localhost:3000 -user student -pass YOUR_PASS -method MKCOL -src /docs
go run ./cmd/GO09_01c -base http://localhost:3000 -user student -pass YOUR_PASS -method PUT -src /docs/a.txt -text "hello webdav"
go run ./cmd/GO09_01c -base http://localhost:3000 -user student -pass YOUR_PASS -method GET -src /docs/a.txt
go run ./cmd/GO09_01c -base http://localhost:3000 -user student -pass YOUR_PASS -method COPY -src /docs/a.txt -dst /docs/a-copy.txt
go run ./cmd/GO09_01c -base http://localhost:3000 -user student -pass YOUR_PASS -method MOVE -src /docs/a-copy.txt -dst /docs/moved.txt
go run ./cmd/GO09_01c -base http://localhost:3000 -user student -pass YOUR_PASS -method DELETE -src /docs/moved.txt
```

## 3) GO09_01s WebDAV server

Module path:

- `cmd/GO09_01s/main.go`

Run:

```bash
go run ./cmd/GO09_01s -addr :3000 -root ./storage
```

Run with basic auth:

```bash
go run ./cmd/GO09_01s -addr :3000 -root ./storage -user student -pass 1111
```

Supported methods:

- `MKCOL`
- `PUT`
- `GET`
- `COPY` (uses `Destination` header)
- `MOVE` (uses `Destination` header)
- `DELETE`

Quick test of GO09_01s with GO09_01c:

```bash
go run ./cmd/GO09_01c -base http://localhost:3000 -method MKCOL -src /demo
go run ./cmd/GO09_01c -base http://localhost:3000 -method PUT -src /demo/one.txt -text "abc"
go run ./cmd/GO09_01c -base http://localhost:3000 -method GET -src /demo/one.txt
go run ./cmd/GO09_01c -base http://localhost:3000 -method COPY -src /demo/one.txt -dst /demo/two.txt
go run ./cmd/GO09_01c -base http://localhost:3000 -method MOVE -src /demo/two.txt -dst /demo/moved.txt
go run ./cmd/GO09_01c -base http://localhost:3000 -method DELETE -src /demo/moved.txt
```
