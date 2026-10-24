package main

import (
	"example.invalid/gin-gonic/gin"
	"log"
	"gopkg.in/yaml.v2"
	"io/ioutil"
	"deployer"
	"os/exec"
	"os"
	"path/filepath"
	"strings"
	"example.invalid/pkg/errors"
)

var DB = make(map[string]string)



func setupRouter() *gin.Engine {
	deploy := deployer.NewDeploy(Config)
	// Disable Console Color
	// gin.DisableConsoleColor()
	r := gin.Default()

	// Ping test
	r.GET("/ping", func(c *gin.Context) {
		c.String(200, "pong")
	})

	// Get user value
	// {error_code, error_message}
	r.GET("/deploy/:build_name", func(c *gin.Context) {
		buildNum := c.Params.ByName("build_name")
		h := deploy.ActionDeploy(buildNum)
		c.JSON(200, h)
		//user := c.Params.ByName("name")
		//value, ok := DB[user]
		//if ok {
		//	c.JSON(200, gin.H{"user": user, "value": value})
		//} else {
		//	c.JSON(200, gin.H{"user": user, "status": "no value"})
		//}
	})

	// Authorized group (uses gin.BasicAuth() middleware)
	// Same than:
	// authorized := r.Group("/")
	// authorized.Use(gin.BasicAuth(gin.Credentials{
	//	  "foo":  "bar",
	//	  "manu": "123",
	//}))
	//authorized := r.Group("/", gin.BasicAuth(gin.Accounts{
	//	"foo":  "bar", // user:foo password:[REDACTED]
	//	"manu": "123", // user:manu password:[REDACTED]
	//}))
	//
	//authorized.POST("admin", func(c *gin.Context) {
	//	user := c.MustGet(gin.AuthUserKey).(string)
	//
	//	// Parse JSON
	//	var json struct {
	//		Value string `json:"value" binding:"required"`
	//	}
	//
	//	if c.Bind(&json) == nil {
	//		DB[user] = json.Value
	//		c.JSON(200, gin.H{"status": "ok"})
	//	}
	//})

	return r
}



var Config = deployer.Config{}


func init(){
	currentPath, _ := GetCurrentPath()
	data, _ := ioutil.ReadFile(currentPath + "/config/deploy.yml")
	log.Println(currentPath)
	err := yaml.Unmarshal([]byte(data), &Config)
	if err != nil {
		log.Fatalf("error: %v", err)
	}
}


func main() {
	r := setupRouter()
	// Listen and Server in 0.0.0.0:8080
	r.Run(":" + Config.Server.Port)
}

//获取当前程序执行目录
func GetCurrentPath() (string, error) {
	file, err := exec.LookPath(os.Args[0])
	if err != nil {
		return "", err
	}
	path, err := filepath.Abs(file)
	if err != nil {
		return "", err
	}
	i := strings.LastIndex(path, "/")
	if i < 0 {
		i = strings.LastIndex(path, "\\")
	}
	if i < 0 {
		return "", errors.New(`error: Can't find "/" or "\".`)
	}
	return string(path[0 : i+1]), nil
}