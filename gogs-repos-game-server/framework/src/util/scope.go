package util

import (
	"math/rand"
	"sync"
	"time"
)

//表示一个范围
type Scope struct {
	MinValue interface{}
	MaxValue interface{}
}

type IntScoper interface {
	GetRandomValue() int32
}

//表示一个int范围
type IntScope struct {
	MinValue int32
	MaxValue int32
}

//创建int范围对象
func NewIntScope(min int32, max int32) *IntScope {
	s := new(IntScope)

	s.MinValue = min
	s.MaxValue = max

	return s
}

//获取一个范围内的int随机值
func (s *IntScope) GetRandomValue() int32 {
	r := rand.New(RndSeed)

	v := r.Intn(int(s.MaxValue - s.MinValue))

	return s.MinValue + int32(v)
}

//动态的int范围
type DynamicIntScope struct {
	*IntScope

	valueLock      *sync.RWMutex
	changeInterval int32
}

//创建动态int范围对象
func NewDynamicIntScope(f func() (int32, int32), changeinterval int32) *DynamicIntScope {
	dynacScope := new(DynamicIntScope)

	fminv, fmaxv := f()
	dynacScope.IntScope = NewIntScope(fminv, fmaxv)

	dynacScope.valueLock = &sync.RWMutex{}
	dynacScope.changeInterval = changeinterval

	go func() {
		for {
			time.Sleep(time.Duration(dynacScope.changeInterval) * time.Second)

			fminv, fmaxv := f()
			dynacScope.valueLock.Lock()
			dynacScope.IntScope = NewIntScope(fminv, fmaxv)
			dynacScope.valueLock.Unlock()
		}
	}()

	return dynacScope

}

func (s *DynamicIntScope) GetRandomValue() int32 {
	s.valueLock.Lock()
	defer s.valueLock.Unlock()

	return s.IntScope.GetRandomValue()
}
