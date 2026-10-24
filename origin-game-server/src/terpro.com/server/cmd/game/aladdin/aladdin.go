package main

import (
	"conf"
	"config"
	"event"
	"fmt"
	"gamelogic"
	"mlog"
	"os"
	"path"
	"roomserver"
	"rpcmsg"
	"runtime"

	"example.invalid/server/internal/game/aladdin"
)

func main() {

	// conf.CLogLevel = 0

	_, file, _, _ := runtime.Caller(0)
	var defaultPaths = []string{"./config", "../../config", path.Dir(file) + "/config", path.Dir(file) + "/../../config"}
	var cfgDir string

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

		cfgDir = path
		break
	}

	if cfgDir == "" {
		fmt.Fprintln(os.Stderr, "Can not find config folder, please put setting files on this folder")
		os.Exit(1)
	}

	conf.ConfigDirName = cfgDir

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return aladdin.NewLogic(t, tfManager, cfgs)
	}

	roomserver.SuperToolCreator = func(room *gamelogic.Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) gamelogic.ISuperToolService {
		return gamelogic.NewBrGameSuperToolServiceBase(room, tfm, dbMsgQueue)
	}

	roomserver.StartGameLogicServer()
}
