package gamelog

import (
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

import (
	"fmt"
	"log"
	"os"
	"path"
	"time"
)

//基于文件的日志记录器
type FileGameLog struct {
	*gameLogBase
	logDir   string
	serverid int32
}

//创建基于文件的日志记录器
func NewFileGameLog(logdir string, serverid int32) *FileGameLog {
	flg := new(FileGameLog)
	flg.logDir = logdir
	flg.serverid = serverid
	flg.gameLogBase = NewGameLogBase()

	//检查目录
	flg.ensureFolder()

	return flg
}

func (l *FileGameLog) EndGameLog(uscores map[int32]int64) {
	defer util.PanicErrStack()

	l.gameEndTime = time.Now()

	logdesc := l.getGameLogDesc()

	if logdesc == "" {
		return
	}

	uids := ""
	for _, uid := range l.gameUsers {
		uids += fmt.Sprintf("%d_", uid)
	}

	logfilename := fmt.Sprintf("%s\\%d\\%s%d.log", l.logDir, l.serverid, uids, time.Now().Unix())

	file, err := os.Create(logfilename)

	if err != nil {
		mlog.Error("创建牌局记录文件错误,%v", err)
		return
	}

	innerLogger := log.New(file, "", log.Llongfile)
	innerLogger.Output(3, l.getGameLogDesc())

	file.Close()
	file = nil
	innerLogger = nil

	//清除牌局记录数据
	l.gameLogBase.clearGameStep()
}

func (l *FileGameLog) ensureFolder() {
	defer util.PanicErrStack()

	var err error
	logdirname := path.Join(l.logDir, fmt.Sprintf("%d", l.serverid))

	if _, err = os.Stat(logdirname); os.IsNotExist(err) {
		err = os.MkdirAll(logdirname, os.ModePerm)
	}

	if err != nil {
		mlog.Error("创建牌局记录目录错误,%v", err)
	}

}
