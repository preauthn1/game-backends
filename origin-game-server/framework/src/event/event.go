package event

import (
	"fmt"
)

//事件参数
type EventArgs map[string]interface{}

//创建事件参数
func NewEventArgs() EventArgs {
	args := make(map[string]interface{})

	return args
}

//事件结果
type EventRet struct {
	Ret   map[string]interface{}
	Error interface{}
}

//创建事件结果
func NewEventRet() *EventRet {
	ret := &EventRet{
		Ret:   make(map[string]interface{}),
		Error: nil}

	return ret
}

//结果是否有错误
func (ret *EventRet) HasError() bool {
	return ret.Error != nil
}

//是否空的结果
func (ret *EventRet) IsEmpty() bool {
	return ret.Ret == nil || len(ret.Ret) == 0
}

func (ret *EventRet) mustNotEmpty() {
	if ret.Ret == nil {
		panic("结果集为空")
	}
}

//从结果中获取字符串
func (ret *EventRet) GetString(key string) string {
	ret.mustNotEmpty()

	return ret.Ret[key].(string)
}

//从结果中获取int
func (ret *EventRet) GetInt(key string) int {
	ret.mustNotEmpty()

	return ret.Ret[key].(int)
}

//从结果中获取int64
func (ret *EventRet) GetInt64(key string) int64 {
	ret.mustNotEmpty()

	return ret.Ret[key].(int64)
}

//从结果中获取float32
func (ret *EventRet) GetFloat32(key string) float32 {
	ret.mustNotEmpty()

	return ret.Ret[key].(float32)
}

//从结果中获取float64
func (ret *EventRet) GetFloat64(key string) float64 {
	ret.mustNotEmpty()

	return ret.Ret[key].(float64)
}

//获取bool值
func (ret *EventRet) GetBool(key string) bool {
	ret.mustNotEmpty()

	return ret.Ret[key].(bool)
}

//设置结果
func (ret *EventRet) SetResult(key string, value interface{}) {
	ret.mustNotEmpty()

	ret.Ret[key] = value
}

const (
	SINGLERESULTKEYMAME = "result"
	SINGLEARGSKEYNAME   = "arg"
)

//设置单一结果，用于返回结果只有一个值的情况
func (ret *EventRet) SetSingleResult(value interface{}) {
	ret.SetResult(SINGLERESULTKEYMAME, value)
}

//获取单一结果
func (ret *EventRet) GetSingleResult() interface{} {
	return ret.Ret[SINGLERESULTKEYMAME]
}

//结果转变为字符串
func (ret *EventRet) ToString() string {
	if ret.HasError() {
		return fmt.Sprintf("%v", ret.Error)
	}

	retstr := ""
	for _, v := range ret.Ret {
		retstr += v.(string) + "."
	}

	return retstr
}

//事件
type Event struct {
	Name string    //事件名称
	Args EventArgs //事件参数
}

//创建事件对象
func NewEvent(name string, args EventArgs) *Event {
	evt := new(Event)
	evt.Name = name
	evt.Args = args

	return evt
}

func (evt *Event) GetSingleArgValue() interface{} {
	return evt.Args[SINGLEARGSKEYNAME]
}

//获取string参数
func (evt *Event) GetString(key string) string {
	return evt.Args[key].(string)
}

//获取int参数
func (evt *Event) GetInt(key string) int {
	return evt.Args[key].(int)
}

//获取int64参数
func (evt *Event) GetInt64(key string) int64 {
	return evt.Args[key].(int64)
}

//获取float32参数
func (evt *Event) GetFloat32(key string) float32 {
	return evt.Args[key].(float32)
}

//获取float64参数
func (evt *Event) GetFloat64(key string) float64 {
	return evt.Args[key].(float64)
}

//获取bool参数
func (evt *Event) GetBool(key string) bool {
	return evt.Args[key].(bool)
}
