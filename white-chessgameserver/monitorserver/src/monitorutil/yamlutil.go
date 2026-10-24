package monitorutil

import (
	"bufio"
	"bytes"
	"conf"
	"io"
	"io/ioutil"
	"os"
	"util"
)

func GetConfig(fileName string) ([]byte, error) {
	var confData []byte
	fileName = fileName + ".yaml"

	pathname, err := util.GetCurrentPath()

	if err != nil {
		panic(err)
	}

	dirname := pathname + conf.ConfigDirName

	flist, err := ioutil.ReadDir(dirname)

	if err != nil {
		return nil, err
	}

	for _, fi := range flist {
		if fi.Name() == fileName {
			filePath := dirname + "\\" + fi.Name()
			confData, err = LoadConfig(filePath)
		}
	}
	return confData, err
}

func LoadConfig(filePath string) ([]byte, error) {
	f, err := os.Open(filePath)
	defer f.Close()

	if err != nil {
		return nil, err
	}

	r := bufio.NewReader(f)

	//var cursec ConfigSection

	var buf bytes.Buffer

	for {
		sbytes, _, err := r.ReadLine()

		if err != nil {
			if err == io.EOF {
				break
			}

			return nil, err
		}

		sline := string(sbytes)
		buf.WriteString(sline + "\n")
	}

	return buf.Bytes(), nil
}
