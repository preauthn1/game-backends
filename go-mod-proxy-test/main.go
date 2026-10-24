package main

import (
	"example.invalid/gin-gonic/gin"
)

func main() {
	srv := gin.Default()

	srv.Run(":8080")
}
