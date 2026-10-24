package util

import (
	"sync"
)

//安全的map对象
type SafeMap struct {
	sync.RWMutex
	innerMap map[interface{}]interface{}
}

//创建safemap
func NewSafeMap() *SafeMap {
	sm := new(SafeMap)
	sm.innerMap = make(map[interface{}]interface{})

	return sm
}

func (m *SafeMap) Get(key interface{}) interface{} {
	m.RLock()
	defer m.RUnlock()

	return m.innerMap[key]
}

func (m *SafeMap) Set(key interface{}, value interface{}) {
	m.Lock()
	defer m.Unlock()

	m.innerMap[key] = value
}

func (m *SafeMap) Del(key interface{}) {
	m.Lock()
	defer m.Unlock()

	delete(m.innerMap, key)
}

func (m *SafeMap) Len() int {
	m.RLock()
	defer m.RUnlock()

	return len(m.innerMap)
}

func (m *SafeMap) RLockRange(f func(interface{}, interface{})) {
	m.RLock()
	defer m.RUnlock()

	for k, v := range m.innerMap {
		f(k, v)
	}
}

func (m *SafeMap) LockRange(f func(interface{}, interface{})) {
	m.Lock()
	defer m.Unlock()

	for k, v := range m.innerMap {
		f(k, v)
	}
}
