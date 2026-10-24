package roulette

import (
	"context"
	"sync"
	"time"

	apipb "example.invalid/ServerTeam/Servers/internal/api-service/pb/apisvc"
	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

func (logic *Logic) MakeDashboard() *pb.CJLHLPTableDashboard {
	m := new(pb.CJLHLPTableDashboard)

	m.TableID = logic.TableNo()
	m.Step = pb.CJLHLP_Step(logic.Game.Step)
	if len(logic.Game.Next) > 0 {
		m.NextAward = pb.CJLHLP_BetType(logic.Game.Next[0])
	}
	m.SysPool = logic.SysPool.GetPool()
	m.SysPoolWinLine = logic.SysPool.GetWinLine()
	m.TotalBetAmount = logic.TotalBetAmount
	m.TotalWinLossAmount = logic.TotalWinLossAmount
	m.TotalValidAmount = logic.TotalValidAmount

	m.BetSummary = &pb.CJLHLPBetAreaInfo{
		Total:           logic.Game.UserBetSum() + logic.Game.RobotBetSum(),
		PlayerCount:     logic.Game.UserCount(),
		PlayerBetAmount: logic.Game.UserBetSum(),
		RobotCount:      logic.Game.RobotCount(),
		RobotBetAmount:  logic.Game.RobotBetSum(),
	}

	{ // 玩家資訊
		for _, u := range logic.Game.Users {
			{ // 玩家
				p := &pb.CJLHLPPlayerInfo{
					PID:       u.UserID,
					IsRobot:   u.GameUser.IsRobot(),
					BetAmount: u.BetSum(),
				}
				m.Players = append(m.Players, p)
			}
		}
	}

	{ // 歷史資訊
		for _, h := range logic.Game.Records {
			m.History = append(m.History, h.Award)
		}
	}

	m.Cheatable = logic.Game.Step == WaitBet && len(logic.Game.Next) == 0 && logic.Cheating == 0 && logic.SpecialRewardCount == 0
	m.Cheating = logic.Cheating > 0
	m.SpecialRewardCount = logic.SpecialRewardCount
	m.SpecialRewardRate = logic.SpecailRewardRate

	m.NextAwardList = append(m.NextAwardList, logic.Game.Next...)

	m.TableDisabled = logic.Disabled

	return m
}

func (logic *Logic) SubscripEvent(ctx context.Context) <-chan *pb.CJLHLPTableDashboard {
	ch := make(chan *pb.CJLHLPTableDashboard, 10)

	logic.BroadcastSubscriper.Store(ctx, ch)
	go func(ctx context.Context, ch chan *pb.CJLHLPTableDashboard) {
		<-ctx.Done()
		logic.BroadcastSubscriper.Delete(ctx)
		time.Sleep(time.Second)
		close(ch)
	}(ctx, ch)

	return ch
}

func (logic *Logic) EventWorker(c chan *pb.CJLHLPTableDashboard, m *sync.Map) {
	for e := range c {
		SubscriperLen := 0
		m.Range(func(key, value interface{}) bool {
			SubscriperLen++
			ch := value.(chan *pb.CJLHLPTableDashboard)
			select {
			default:
				mlog.Warn("send a closed chan")
			case ch <- e:
			}
			return true
		})
		mlog.Debug("牌桌[%d] Broadcast to Subscripers Len %d", logic.TableNo(), SubscriperLen)
	}
}

func (logic *Logic) queryBetSummary() {
	if err := app.Context.Invoke(func(c apipb.GameBetServiceClient) error {
		req := new(apipb.SummaryReq)
		req.ServerID = logic.TableManager.Room.ServerID

		loc := time.FixedZone("UTC+8", 60*60*8)
		now := time.Now().In(loc)
		begin := time.Date(now.Year(), now.Month(), now.Day(), 0, 0, 0, 0, now.Location())
		end := begin.AddDate(0, 0, 1)

		req.Begin = begin.UTC().Format("2006-01-02 15:04:05")
		req.End = end.UTC().Format("2006-01-02 15:04:05")

		example.invalid("查詢本日下注, %v", req)

		res, err := c.Summary(context.Background(), req)
		if err != nil {
			return err
		}

		logic.TotalBetAmount = res.BetAmount
		logic.TotalWinLossAmount = res.WinLossAmount
		logic.TotalValidAmount = res.ValidAmount

		example.invalid("更新 本日下注總計: %d, 本日輸贏總計: %d, 本日流水總計: %d", logic.TotalBetAmount, logic.TotalWinLossAmount, logic.TotalValidAmount)

		return nil
	}); err != nil {
		mlog.Error("Bet Summary Error, %v", err)
	}
}
