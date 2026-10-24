package module

import (
	"command"
	"mlog"
)

//模块组
type ModuleGroup struct {
	modules map[string]Module
}

func NewModuleGroup() *ModuleGroup {
	mg := new(ModuleGroup)
	mg.modules = make(map[string]Module)

	//添加模块命令
	cmd := NewModuleCmd(mg)
	command.AddCommand(cmd)

	return mg
}

//注册模块
func (mg *ModuleGroup) Register(m Module) {
	if _, ok := mg.modules[m.Name()]; ok {
		mlog.Printf("模块%s已经注册", m.Name())
		return
	}

	mg.modules[m.Name()] = m

	return
}

//卸载模块
func (mg *ModuleGroup) UnLoad(mname string) Module {
	m := mg.GetModule(mname)
	if m == nil {
		return nil
	}

	m.Stop()

	return m
}

//获取模块
func (mg *ModuleGroup) GetModule(name string) Module {
	if m, ok := mg.modules[name]; ok {
		return m
	}

	return nil
}

//遍历所有模块
func (mg *ModuleGroup) Range(f func(m Module)) {
	for _, m := range mg.modules {
		f(m)
	}
}

//运行
func (mg *ModuleGroup) Run(ctx *MouduleCtx) error {
	for _, m := range mg.modules {
		err := m.Run(ctx)

		if err != nil {
			return err
		}
	}

	return nil
}

//停止
func (mg *ModuleGroup) Stop(ctx *MouduleCtx) {
	for _, m := range mg.modules {
		m.Stop()
	}
}

//名称
func (mg *ModuleGroup) Name() string {
	return "modulegroup"
}
