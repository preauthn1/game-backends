package jlbpk

import (
	"fmt"
	"os"
	"path"
	"runtime"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

var cfg *config.ConfigGroup

func init() {

	// 檢查config位置
	if err := initCfg(); err != nil {
		fmt.Fprintf(os.Stderr, "Load config files error. %v\n", err)
		os.Exit(1)
	}

}

func initCfg() error {
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
