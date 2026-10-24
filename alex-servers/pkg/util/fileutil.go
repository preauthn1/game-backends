package util

import (
	"errors"
	"fmt"
	"io/ioutil"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
)

//检查目录
func MustDirExists(dirname string) {
	_, err := os.Stat(dirname)

	if err == nil {
		return
	}

	if os.IsNotExist(err) {
		os.MkdirAll(dirname, 0777)
	}
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

//读取文件
func ReadFile(filename string) string {
	f, err := os.Open(filename)
	defer f.Close()

	if err != nil {
		return fmt.Sprintf("%v", err)
	}

	data, err := ioutil.ReadAll(f)
	if err != nil {
		return fmt.Sprintf("%v", err)
	}

	return string(data)

}

func FilePutContents(filename string, content string) error {
	bytes := []byte(content)
	fp, err := os.OpenFile(filename, os.O_RDWR|os.O_CREATE|os.O_APPEND, os.ModePerm)

	if err != nil {
		return err
	}

	defer fp.Close()

	_, err = fp.Write(bytes)

	return err
}

func FilePutContentsNA(filename string, content string) error {
	bytes := []byte(content)
	fp, err := os.OpenFile(filename, os.O_WRONLY|os.O_CREATE, os.ModePerm)

	if err != nil {
		return err
	}

	defer fp.Close()
	fp.Truncate(0)
	fp.Seek(0, 0)
	_, err = fp.Write(bytes)

	return err
}

func WriteErrorLog(content string) {
	filepath := "./log/error.log"
	FilePutContents(filepath, content+"\r\n")
}
