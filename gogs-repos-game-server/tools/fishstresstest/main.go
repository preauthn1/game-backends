package main

import (
	"conf"
	"fmt"
	"monitorhandler"
	"msgserver"
	"time"
	"util"

	//"mlog"
	"mlog"
	"monitorutil"
	"example.invalid/golang/yaml"
)

type ConfigStruct struct {
	GameId	int32	`yaml:"GameId"`
	ServerId int32	`yaml:"ServerId"`
	WorkerCount int `yaml:"WorkerCount"`
	HallServerIp string `yaml:"HallServerIp"`
	SelfIP string `yaml:"SelfIP"`
	TfAPI string `yaml:"TfAPI"`
	SiteId int32 `yaml:"SiteId"`
}

var Config *ConfigStruct

func init() {
	conf.CLogLevel = 3
	conf.ClearLog = false
	monitorhandler.ReTryCount = 3
	monitorhandler.ReTryInterval = 3
	loadConfig()
}
func main() {
	worker(Config.WorkerCount)
	//parallel(conf, 100)
}

func worker(n int) {
	JobChans := make(chan bool, n*2)
	DoneChans := make(chan bool, n*2)

	for w := 1; w <= n; w++ {
		go func(id int, jChans <-chan bool, dChans chan<- bool) {

			for _ = range jChans {
				mlog.System("Run Worker [%v]", id)
				Run(id)
				dChans <- true
			}
		}(w, JobChans, DoneChans)
	}
	go func() {
		for {
			<-DoneChans
		}
	}()

	jobsNum := 1000000
	limiter := time.Tick(time.Duration(10) * time.Millisecond)
	for i := 0; i < jobsNum; i++ {
		<-limiter
		JobChans <- true
		//mlog.System("============job %d", i)
	}

	close(JobChans)

	select {}
}

func parallel(n int) {
	for {
		var ExecChans = make(chan bool, n)
		var DoneChans = make(chan bool, 1)
		limiter := time.Tick(time.Duration(1) * time.Millisecond)
		for i := 1; i <= n; i++ {
			<-limiter
			go func() {
				ExecChans <- true
				Run(i)
				DoneChans <- true
			}()
		}
		for i := 0; i < n; i++ {
			<-DoneChans
			<-ExecChans
		}
	}

	select {}
}

func Run(guestID int) {

	tfData := monitorutil.BuildTfData(Config.TfAPI, 2)
	tfQuery := fmt.Sprintf(Config.TfAPI, tfData)
	mlog.System(tfQuery)
	// fetch hallserver
	err := util.Retry(monitorhandler.ReTryCount, time.Duration(monitorhandler.ReTryInterval)*time.Second, func() error {
		var e error
		tfData, e = monitorutil.Get(tfQuery) //fix for loop
		if e != nil {
			mlog.Error("%v", e)
			return e
		}
		return nil
	})
	if err != nil {
		errMsg := "fetch url [" + tfQuery + "] error msg [" + err.Error() + "]"
		mlog.System(errMsg)
		return
	}
	mlog.System(fmt.Sprintf("fetch url [%s], body [%s]", tfQuery, tfData))
	ret, _ := msgserver.NodeDefDecryptor(tfData)

	mlog.System("%v", ret)

	//mlog.System("Connect HallServer [%s], selfip [%s]", serverIP, selfIP)
	err = monitorhandler.FishClient(ret, tfData, Config.SiteId, int32(guestID))
	if err != nil {
		errMsg := "error : " + err.Error()
		//monitorutil.Push(conf.NotifyWebHook, errMsg, time.Since(start))
		mlog.Error("ErrorMsg %v", errMsg)
	}
}

func loadConfig() {
	cfData, err := monitorutil.GetConfig("game")
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
