package portrouter

//端口映射加载器
type PortMapLoader interface {
	LoadPortMap() PortMap
}
