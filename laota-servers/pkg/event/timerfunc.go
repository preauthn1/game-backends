package event

import (
	"time"
	"sync"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	EVTNAME_TIMERFUNC     = "timerfunc" //事件名称
	EVTArgsName_TimerFunc = "timerinfo" //参数名称
)

const (
	TimerStatus_Stop = 0
	TimerStatus_Run  = 1
)

const (
	TimerType_Timer  = 0
	TimerType_Ticker = 1
)

//延迟执行一次的函数
type TimerFunc struct {
	WaitSeconds      int32     //延迟时间
	WaitMilliSeconds int32     //延迟时间
	Func             func()    //处理函数
	Status           int       //状态
	Loop             bool      //是否循环执行
	Name             string    //名称
	BeginTime        time.Time //开始时间
	TimerType        int       //timer类型
}

//执行延迟函数
func (tf *TimerFunc) Exec() {
	defer util.PanicErrStack()
	if tf.Status == TimerStatus_Run {
		//example.invalid("执行timer func %v", tf)
		tf.Func()
	}
}

//消耗的秒数
func (tf *TimerFunc) ElapsedSeconds() int32 {
	return int32(time.Now().Sub(tf.BeginTime).Seconds())
}

func (tf *TimerFunc) Stop() {
	tf.Status = TimerStatus_Stop
}

func (tf *TimerFunc) IsStop() bool {
	return tf.Status == TimerStatus_Stop
}

//延迟函数管理器
type TimerFuncManager struct {
	evtDispatcher *EventDispatcher	
	timerFuncs    sync.Map //key:string value:*TimerFunc
}

//创建延迟函数管理器对象
func NewTimerFuncManager(evtDispatcher *EventDispatcher) *TimerFuncManager {
	tfm := new(TimerFuncManager)
	tfm.evtDispatcher = evtDispatcher

	tfm.registerTimeFuncEvtHandler()

	return tfm
}

func (tfm *TimerFuncManager) NewTimer(wait int32, f func()) *TimerFunc {
	return tfm.NewNamedTimer(wait, f, "")
}

//创建一个timer
func (tfm *TimerFuncManager) NewNamedTimer(wait int32, f func(), timername string) *TimerFunc {
	return tfm.NewNamedMilliSecTimer(wait*1000, f, timername)
}

func (tfm *TimerFuncManager) NewMilliSecTimer(wait int32, f func()) *TimerFunc {
	return tfm.NewNamedMilliSecTimer(wait, f, "")
}

//创建一个毫秒级的timer
func (tfm *TimerFuncManager) NewNamedMilliSecTimer(wait int32, f func(), timername string) *TimerFunc {
	tf := new(TimerFunc)
	tf.WaitSeconds = wait / 1000
	tf.WaitMilliSeconds = wait
	tf.Loop = false
	tf.Status = TimerStatus_Run
	tf.Func = f
	tf.Name = timername
	tf.BeginTime = time.Now()
	tf.TimerType = TimerType_Timer

	//測試模式下的快轉倍數 TimerFastMode
	if conf.TestMode && conf.TimerFastMode > 1 {
		wait = wait / int32(conf.TimerFastMode)
		tf.WaitSeconds = wait / 1000
		tf.WaitMilliSeconds = wait
	}

	go func() {
		time.Sleep(time.Duration(wait) * time.Millisecond)
		if tf.Status == TimerStatus_Stop {
			return
		}
		tfm.publishTimerFuncEvent(tf)
	}()

	if timername != "" {		
		tfm.timerFuncs.Store(timername, tf)
		mlog.Debug("创建timerfunc %s , status %v", timername, tf.Status)
	}

	return tf
}

func (tfm *TimerFuncManager) NewTicker(wait int32, f func()) *TimerFunc {
	return tfm.NewNamedTicker(wait, f, "")
}

//创建一个 tick
func (tfm *TimerFuncManager) NewNamedTicker(wait int32, f func(), timername string) *TimerFunc {
	return tfm.NewNamedMilliSecTicker(wait*1000, f, timername)
}

func (tfm *TimerFuncManager) NewMilliSecTicker(wait int32, f func()) *TimerFunc {
	return tfm.NewNamedMilliSecTicker(wait, f, "")
}

//创建一个毫秒级的 tick
func (tfm *TimerFuncManager) NewNamedMilliSecTicker(wait int32, f func(), timername string) *TimerFunc {
	tf := new(TimerFunc)
	tf.WaitSeconds = wait / 1000
	tf.WaitMilliSeconds = wait
	tf.Loop = true
	tf.Status = TimerStatus_Run
	tf.Func = f
	tf.Name = timername
	tf.BeginTime = time.Now()
	tf.TimerType = TimerType_Ticker
	//測試模式下的快轉倍數 TimerFastMode
	if conf.TestMode && conf.TimerFastMode > 1 {
		wait = wait / int32(conf.TimerFastMode)
		tf.WaitSeconds = wait / 1000
		tf.WaitMilliSeconds = wait
	}
	go func() {
		for {
			time.Sleep(time.Duration(wait) * time.Millisecond)
			if tf.Status == TimerStatus_Stop {
				break
			}

			tfm.publishTimerFuncEvent(tf)
		}
	}()

	if timername != "" {
		tfm.timerFuncs.Store(timername, tf)
	}

	return tf
}

//创建一个timer tick
func (tfm *TimerFuncManager) NewNamedRandomTicker(rndScope *util.IntScope, f func(), timername string) *TimerFunc {
	wait := int32(rndScope.GetRandomValue())
	tf := new(TimerFunc)
	tf.WaitSeconds = rndScope.MaxValue
	tf.WaitMilliSeconds = rndScope.MaxValue * 1000
	tf.Loop = true
	tf.Status = TimerStatus_Run
	tf.Func = f
	tf.Name = timername
	tf.BeginTime = time.Now()
	tf.TimerType = TimerType_Ticker
	//測試模式下的快轉倍數 TimerFastMode
	if conf.TestMode && conf.TimerFastMode > 1 {
		wait = wait / int32(conf.TimerFastMode)
		tf.WaitSeconds = wait / 1000
		tf.WaitMilliSeconds = wait
	}
	go func() {
		for {
			time.Sleep(time.Duration(wait) * time.Second)

			if tf.Status == TimerStatus_Stop {
				break
			}
			tfm.publishTimerFuncEvent(tf)
		}
	}()

	if timername != "" {
		tfm.timerFuncs.Store(timername, tf)
	}

	return tf
}

//获取延迟函数
func (tfm *TimerFuncManager) GetTimerFunc(timername string) *TimerFunc {
	if val, ok := tfm.timerFuncs.Load(timername); ok {
		var tf = val.(*TimerFunc)
		return tf
	}
	return nil
}

//移除timer
func (tfm *TimerFuncManager) RemoveTimer(timername string) *TimerFunc {
	if val, ok := tfm.timerFuncs.Load(timername); ok {
		var tf = val.(*TimerFunc)
		tf.Status = TimerStatus_Stop
		tfm.timerFuncs.Delete(timername)
		return tf
	}
	return nil
}

func (tfm *TimerFuncManager) publishTimerFuncEvent(tf *TimerFunc) {
	tfm.evtDispatcher.OnAsyncEvent(newTimerFuncEvent(tf), nil)
}

func (tfm *TimerFuncManager) registerTimeFuncEvtHandler() {
	h := NewCommonHandler("timerfunc", func(evt *Event, evtret *EventRet) bool {

		tf := getTimerFuncEventArg(evt.Args)

		util.SafeCall(tf.Exec)

		//移除命名的timer
		if tf.TimerType == TimerType_Timer && tf.Name != "" {
			tfm.RemoveTimer(tf.Name)
		}

		return true
	}, EVTNAME_TIMERFUNC)

	tfm.evtDispatcher.PushHandler(h)
}

//创建延迟函数事件
func newTimerFuncEvent(tf *TimerFunc) *Event {
	evt := new(Event)
	evt.Name = EVTNAME_TIMERFUNC
	evt.Args = NewEventArgs()
	evt.Args[EVTArgsName_TimerFunc] = tf

	return evt
}

//获取延迟函数事件参数
func getTimerFuncEventArg(evtArgs EventArgs) *TimerFunc {
	return evtArgs[EVTArgsName_TimerFunc].(*TimerFunc)
}
