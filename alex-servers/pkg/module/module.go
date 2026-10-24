package module

//模块接口定义
//组织一个功能项,提供基础的功能组件
type Module interface {
	Run(ctx *MouduleCtx) error //启动模块
	Stop()                     //停止模块
	Name() string              //模块名称
}
