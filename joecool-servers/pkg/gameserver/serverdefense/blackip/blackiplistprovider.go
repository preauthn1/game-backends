package blackip

//ip黑名单
type BlackIPListProvider interface {
	//加入黑名单
	BlockIP(ip string, sec int, memo string)

	//检查IP是否被拦截
	CheckIP(ip string) bool
}
