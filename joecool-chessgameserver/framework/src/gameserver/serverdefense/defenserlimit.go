package serverdefense

import (
	"fmt"
)

//防护信息
type DefenserLimit struct {
	SecLimit_5   int //5秒阀值
	SecLimit_10  int //10秒阀值
	SecLimit_30  int //30秒阀值
	SecLimit_60  int //60秒阀值
	SecLimit_300 int //5分钟阀值
}

//创建防护信息对象
func NewDefenserLimit(secLimit_5 int, secLimit_10 int, secLimit_30 int, secLimit_60 int, secLimit_300 int) *DefenserLimit {
	definfo := new(DefenserLimit)
	definfo.SecLimit_5 = secLimit_5
	definfo.SecLimit_10 = secLimit_10
	definfo.SecLimit_30 = secLimit_30
	definfo.SecLimit_60 = secLimit_60
	definfo.SecLimit_300 = secLimit_300

	return definfo
}

func (limit *DefenserLimit) String() string {
	return fmt.Sprintf("5:%d,10:%d,30:%d,60:%d,300:%d", limit.SecLimit_5, limit.SecLimit_10, limit.SecLimit_30, limit.SecLimit_60, limit.SecLimit_300)
}
