FROM golang:1.22 AS builder

WORKDIR /app

COPY go.mod ./
COPY go.sum ./
RUN go mod download

COPY . .
RUN go build -o GO04_01_linux .

FROM debian:bookworm-slim

WORKDIR /app

COPY --from=builder /app/GO04_01_linux .
COPY --from=builder /app/Celebrities.json .

EXPOSE 3000

CMD ["./GO04_01_linux"]