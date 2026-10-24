package dal

import (
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
	"testing"
)



//func TestNotConcurrencySafeSlice(t *testing.T) {
//	list := make([]int, 10000000)
//	counter := 0
//	worker(500, func(id int){
//		list = list[1:]
//		list = append(list, counter)
//		counter++
//	})
//		//parallel(conf, 100)
//
//}
// 註解掉的有 Concurrency Issue, 下面的是正確姿勢
func TestConcurrencySafeSlice(t *testing.T) {
	queue := make(chan int, 100000)
	list := make([]int, 1000)
	counter := 0
	go func() {
		for data := range queue {
			list = list[1:]
			list = append(list, data)
		}
		mlog.Error("History queue over")
	}()

	worker(100, func(id int){
		queue <- counter
		counter++
	})

	//parallel(conf, 100)
}

func worker(n int, Run func(id int)) {
	JobChans := make(chan bool, n*2)
	DoneChans := make(chan bool, n*2)

	for w := 1; w <= n; w++ {
		go func(id int, jChans <-chan bool, dChans chan<- bool) {
			defer util.PanicErrStack()
			for _ = range jChans {
				//mlog.System("Run Worker [%v]", id)
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

	jobsNum := 10000000
	//limiter := time.Tick(time.Duration(10) * time.Microsecond)
	for i := 0; i < jobsNum; i++ {
		//<-limiter
		JobChans <- true
		//mlog.System("============job %d", i)
	}

	close(JobChans)

	//select {}
}