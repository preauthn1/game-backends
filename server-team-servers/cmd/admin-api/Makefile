.PHONY: build

build:
	GOARCH=amd64 GOOS=linux CGO_ENABLED=0 go build -o admin-api .
	docker build --platform linux/amd64 -t example.invalid/admin-api .
