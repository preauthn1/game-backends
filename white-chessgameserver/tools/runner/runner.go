package main

import (
	"RunnerConfig"
	"mlog"
	"os/exec"
	"time"
)

func main() {
	for _, server := range RunnerConfig.Config.MainServerList {
		ServerStart(server)
	}
	mlog.System("MainServer finish")
	time.Sleep(time.Duration(RunnerConfig.Config.DelayTime[0]) * time.Millisecond)
	for _, server := range RunnerConfig.Config.GameServerList {
		ServerStart(server)
	}
	mlog.System("GameServer finish")
	time.Sleep(time.Duration(30) * time.Second)
}

func ServerStart(server RunnerConfig.Server) {
	for _, path := range server.Path {
		cmd := exec.Command("cmd.exe", "/c", "start "+path+"\\"+server.Execute)
		cmd.Dir = path
		err := cmd.Run()
		if err != nil {
			mlog.Error("啟動發生錯誤 %v", err)
		} else {
			mlog.System("%s 服務器啟動", server.ServerName)
		}
		time.Sleep(time.Duration(RunnerConfig.Config.DelayTime[1]) * time.Millisecond)
	}
}
