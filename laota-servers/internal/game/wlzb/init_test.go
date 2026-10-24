package wlzb

import (
	"fmt"
	"os"
	"path"
	"runtime"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

var cfg *config.ConfigGroup

// InitCfg 抓出設定檔
func InitCfg() error {
	// 檢查config位置
	var data *config.ConfigGroup
	_, file, _, _ := runtime.Caller(0)
	var defaultPaths = []string{"./config", "../../config", path.Dir(file) + "/config", path.Dir(file) + "/../../config"}

	data = new(config.ConfigGroup)

	for _, path := range defaultPaths {
		fi, err := os.Stat(path)
		if err != nil {
			mlog.Debug("Find config folder %s Error", path)
			continue
		}

		if !fi.IsDir() {
			mlog.Debug("%s is not a folder", path)
			continue
		}

		data.LoadConfig(path, 0)

		if len(data.Configs) > 0 {
			mlog.Debug("Find config folder %s Success", path)
			break
		}
		mlog.Debug("Find config folder %s Error", path)
	}

	if data == nil {
		return fmt.Errorf("Can not find config folder, please put setting files on this folder")
	}

	if len(data.Configs) == 0 {
		return fmt.Errorf("Can not load configs, path: %s", data.DirName)
	}

	cfg = data

	return nil
}

func TestMain(m *testing.M) {
	InitCfg()
	InitProtoData()
	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true
	conf.ClearLog = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	m.Run()
}

func TestProto(t *testing.T) {
	Protodatas.Print()
}
