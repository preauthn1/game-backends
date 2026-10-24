package nodemonitor

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

type NodeStruct struct {
	Host      string `yaml:"Host"`
	Port      int32  `yaml:"Port"`
	Database  string `yaml:"Database"`
	User      string `yaml:"User"`
	Password  string `yaml:"Password"`
	Encrypt   string `yaml:"ConnString"`
	Semaphore int32  `yaml:"Semaphore"`
	Pool      int32  `yaml:"Pool"`
}

type ConfigStruct struct {
	Interval        int32      `yaml:"Interval"`
	AlertConnLimit  int32      `yaml:"AlertConnLimit"`
	NodeConnTimeout int32      `yaml:"NodeConnTimeout"`
	AlertBotAPI     string     `yaml:"AlertBotAPI"`
	NodeDB          NodeStruct `yaml:"NodeDB"`
	WhiteList       []string   `yaml:"WhiteList"`
}

var Config *ConfigStruct

func init() {
	cfData, err := GetConfig("monitor")
	if err != nil {
		mlog.Error("runner config err : %v", err)
	}
	config := ConfigStruct{}
	err2 := yaml.Unmarshal([]byte(cfData), &config)
	if err2 != nil {
		mlog.Error("cannot unmarshal data: %v", err2)
	}
	Config = &config
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
