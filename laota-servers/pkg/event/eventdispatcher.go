package event

import (
	"sort"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//事件处理器信息
type EventHandlerInfo struct {
	Handler EventHandler
	Sort    int
}

//事件处理器组
type EventDispatcher struct {
	innerHandlers []*EventHandlerInfo  //处理器集合
	asyncEvents   chan *AsyncEventInfo //异步事件
	throttle      int                  //并发数
}

//异步事件信息
type AsyncEventInfo struct {
	Evt      *Event
	Callback CallbackFun
}

//创建事件处理器组
func NewEventDispatcher() *EventDispatcher {
	evg := new(EventDispatcher)

	//evg.asyncEvents = make(chan *AsyncEventInfo, 1000)

	return evg
}

//创建异步事件处理器组
func NewAsyncEventDispatcher(throttle int) *EventDispatcher {
	evg := new(EventDispatcher)
	evg.asyncEvents = make(chan *AsyncEventInfo, 10000)
	evg.throttle = throttle

	for i := 0; i < throttle; i++ {
		//开启处理异步事件线程
		go func() {
			defer util.PanicErrStack()

			for {
				asinfo := <-evg.asyncEvents
				if asinfo != nil {
					ret := evg.OnEvent(asinfo.Evt)

					if asinfo.Callback != nil {
						callbackf := func() {
							asinfo.Callback(asinfo.Evt, ret)
						}

						util.SafeCall(callbackf)
					}
				}
			}
		}()
	}

	return evg
}

//sort 接口实现
func (evg *EventDispatcher) Len() int {
	return len(evg.innerHandlers)
}

func (evg *EventDispatcher) Less(i, j int) bool {
	ih := evg.innerHandlers[i]
	jh := evg.innerHandlers[j]

	return ih.Sort > jh.Sort
}

func (evg *EventDispatcher) Swap(i, j int) {
	evg.innerHandlers[i], evg.innerHandlers[j] = evg.innerHandlers[j], evg.innerHandlers[i]
}

//添加事件处理器,可控制排序
func (evg *EventDispatcher) AddHandler(h EventHandler, sortIndex int) *EventDispatcher {
	if h == nil {
		panic("处理器不能为空")
	}

	hi := &EventHandlerInfo{
		Handler: h,
		Sort:    sortIndex}
	evg.innerHandlers = append(evg.innerHandlers, hi)

	//排序
	sort.Sort(evg)

	return evg
}

//添加事件处理器
func (evg *EventDispatcher) PushHandler(h EventHandler) *EventDispatcher {
	return evg.AddHandler(h, 0)
}

//遍历处理器
func (evg *EventDispatcher) RangeSort(f func(int, EventHandler) bool) {
	for _, v := range evg.innerHandlers {
		ret := f(v.Sort, v.Handler)

		if !ret {
			return
		}
	}
}

//遍历处理器
func (evg *EventDispatcher) Range(f func(EventHandler) bool) {
	for _, v := range evg.innerHandlers {
		ret := f(v.Handler)

		if !ret {
			return
		}
	}
}

//同步触发事件
func (evg *EventDispatcher) OnEvent(evt *Event) (ret *EventRet) {
	ret = NewEventRet()

	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetCallDepLen(8).Error("触发同步事件%v发生错误%v, errstack:%s", evt, err, errorStr)
			ret.Error = err
		}
	}()

	for _, h := range evg.innerHandlers {
		goon := h.Handler.OnEvent(evt, ret)

		if !goon {
			return
		}
	}

	return
}

//异步触发事件
func (evg *EventDispatcher) OnAsyncEvent(evt *Event, callback CallbackFun) {
	asinfo := new(AsyncEventInfo)
	asinfo.Evt = evt
	asinfo.Callback = callback

	evg.asyncEvents <- asinfo
}
