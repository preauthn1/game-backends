package RunnerConfig

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

type Server struct {
	ServerName string   `yaml:"ServerName"`
	Path       []string `yaml:"Path"`
	Execute    string   `yaml:"Execute"`
}

type RunnerConfig struct {
	DelayTime      []int    `yaml:"DelayTime"`
	MainServerList []Server `yaml:"MainServerList"`
	GameServerList []Server `yaml:"GameServerList"`
}

var Config *RunnerConfig

func init() {
	cfData, err := GetConfig("serverlist")
	if err != nil {
		mlog.Error("runner config err : %v", err)
	}
	runnerStruct := RunnerConfig{}
	err2 := yaml.Unmarshal([]byte(cfData), &runnerStruct)
	if err2 != nil {
		mlog.Error("cannot unmarshal data: %v", err2)
	}
	Config = &runnerStruct
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
