package robotaction

import (
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//机器人主动入座处理器
type RobotAutoSitHandler struct {
}

//创建机器人主动入座处理器
func NewRobotAutoSitHandler() *RobotAutoSitHandler {
	rash := new(RobotAutoSitHandler)

	return rash
}

func (rash *RobotAutoSitHandler) Start(ctx *RobotActionHandlerCtx) {
	scope := util.NewIntScope(ctx.RobotCfg.AutoSitWaitSpanMin, ctx.RobotCfg.AutoSitWaitSpanMax)
	ctx.TfManager.NewNamedRandomTicker(scope, func() {
		if ctx.ServerStatus.IsServerRuning() {
			rash.robotAutoSit(ctx)
		}
	}, "RobotAutoSitTicker")
}

func (rash *RobotAutoSitHandler) robotAutoSit(ctx *RobotActionHandlerCtx) {
	//example.invalid("机器人主动入座检查开始...")

	//获取一个空闲的机器人
	freerobot := ctx.RobotManager.GetFreeRobot(gamelogic.FreeTableNo)

	if freerobot == nil {
		mlog.Debug("机器人主动入座检查完成.没有空闲的机器人.")
		return
	}

	tables := ctx.Room.GetRobotSortedTable()

	for _, table := range tables {
		//获取一个空闲的座位
		st := table.GetFreeSeat()
		if st == nil {
			continue
		}
		//AgainstGameing => 1 => false , 不可以觀戰
		//AgainstGameing => 0 => true , 可以觀戰
		if !ctx.RobotCfg.AgainstGameing && table.GameLogic.IsGaming() {
			continue
		}

		urcount := int32(table.TableRealUserCount())

		//检查桌子上的人数是否匹配撮合条件
		if urcount < ctx.RobotCfg.TableMinUserCount || urcount >= ctx.RobotCfg.TableMaxUserCount {
			continue
		}

		ctx.ActionController.UserSit(freerobot.UserInfo, table.TableNo, st.SeatNo)
		ctx.ActionController.UserReady(freerobot.UserInfo)

		//example.invalid("机器人主动入座检查完成.")
		return
	}

	mlog.Debug("机器人主动入座检查完成.没有空闲的桌子")

}
