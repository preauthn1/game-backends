package blackip

import (
	"time"
)

//黑名单信息
type BlackIPInfo struct {
	IpAddr       string    //ip地址
	BlackEndTime time.Time //黑名单截止时间
	BlackMemo    string    //加入黑名单原因
}

//是否过期
func (bip *BlackIPInfo) IsExpire() bool {
	return time.Now().After(bip.BlackEndTime)
}
