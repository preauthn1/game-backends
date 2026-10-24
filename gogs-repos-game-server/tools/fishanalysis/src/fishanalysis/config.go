package fishanalysis

import (
	"bufio"
	"bytes"
	"conf"
	"example.invalid/golang/yaml"
	"io"
	"io/ioutil"
	"mlog"
	"os"
	"util"
)

var Config *MongoConn
var UserCfg *UserQuery

type MongoConn struct {
	MongoConn MongoConnInfo `yaml:"MongoConn"`
}

type UserQuery struct {
	QueryData UserQueryInfo `yaml:"UserQuery"`
}

type MongoConnInfo struct {
	Connstring    string `yaml:"Connstring"`
	Database      string `yaml:"Database"`
	Collection    string `yaml:"Collection"`
	Pool          int    `yaml:"Pool"`
	Perwritecount int    `yaml:"Perwritecount"`
	Sort          int    `yaml:"Sort"`
	Limit         int    `yaml:"Limit"`
}

type UserQueryInfo struct {
	UserId int `yaml:"UserId"`
	Sort   int `yaml:"Sort"`
	Limit  int `yaml:"Limit"`
}

func init() {
	cfData, err := GetConfig("config")
	if err != nil {
		mlog.Error("runner config err : %v", err)
	}

	config := MongoConn{}
	err2 := yaml.Unmarshal([]byte(cfData), &config)
	if err2 != nil {
		mlog.Error("cannot unmarshal MongoConn data: %v", err2)
	}
	Config = &config

	usercfg := UserQuery{}
	err2 = yaml.Unmarshal([]byte(cfData), &usercfg)
	if err2 != nil {
		mlog.Error("cannot unmarshal usercfg data: %v", err2)
	}
	UserCfg = &usercfg
}

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
