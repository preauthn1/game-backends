package serverdefense

import (
	"fmt"
	"sync"
)

//防护数据
type DefenserData struct {
	SecData_5   int //5秒数据
	SecData_10  int //10秒数据
	SecData_30  int //30秒数据
	SecData_60  int //60秒数据
	SecData_300 int //5分钟数据

	innerLocker *sync.RWMutex
}

//创建防护数据对象
func NewDefenserData() *DefenserData {
	defdata := new(DefenserData)
	defdata.SecData_5 = 0
	defdata.SecData_10 = 0
	defdata.SecData_30 = 0
	defdata.SecData_60 = 0
	defdata.SecData_300 = 0

	defdata.innerLocker = &sync.RWMutex{}

	return defdata
}

//增加计数器
func (dd *DefenserData) Increase(num int) {
	dd.innerLocker.Lock()
	defer dd.innerLocker.Unlock()

	dd.SecData_5 += num
	dd.SecData_10 += num
	dd.SecData_30 += num
	dd.SecData_60 += num
	dd.SecData_300 += num
}

//清除计数器
func (dd *DefenserData) Clear() {
	dd.innerLocker.Lock()
	defer dd.innerLocker.Unlock()

	dd.SecData_5 = 0
	dd.SecData_10 = 0
	dd.SecData_30 = 0
	dd.SecData_60 = 0
	dd.SecData_300 = 0
}

//检查防护数据是否超出设定
func (d *DefenserData) CheckLimit(limit *DefenserLimit) bool {
	d.innerLocker.RLock()
	defer d.innerLocker.RUnlock()

	if d.SecData_5 >= limit.SecLimit_5 {
		return true
	}
	if d.SecData_10 >= limit.SecLimit_10 {
		return true
	}
	if d.SecData_30 >= limit.SecLimit_30 {
		return true
	}
	if d.SecData_60 >= limit.SecLimit_60 {
		return true
	}
	if d.SecData_300 >= limit.SecLimit_300 {
		return true
	}

	return false
}

func (d *DefenserData) ClearSecData_5() {
	d.innerLocker.Lock()
	defer d.innerLocker.Unlock()

	d.SecData_5 = 0
}

func (d *DefenserData) ClearSecData_10() {
	d.innerLocker.Lock()
	defer d.innerLocker.Unlock()

	d.SecData_10 = 0
}

func (d *DefenserData) ClearSecData_30() {
	d.innerLocker.Lock()
	defer d.innerLocker.Unlock()

	d.SecData_30 = 0
}

func (d *DefenserData) ClearSecData_60() {
	d.innerLocker.Lock()
	defer d.innerLocker.Unlock()

	d.SecData_60 = 0
}

func (d *DefenserData) ClearSecData_300() {
	d.innerLocker.Lock()
	defer d.innerLocker.Unlock()

	d.SecData_300 = 0
}

func (d *DefenserData) String() string {
	d.innerLocker.RLock()
	d.innerLocker.RUnlock()

	return fmt.Sprintf("5:%d,10:%d,30:%d,60:%d,300:%d", d.SecData_5, d.SecData_10, d.SecData_30, d.SecData_60, d.SecData_300)
}
