package jzyq

import (
	"context"
	"math/rand"
	"sort"
	"sync"
	"time"

	apipb "example.invalid/ServerTeam/Servers/internal/api-service/pb/apisvc"
	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/roomutil"
)

func (logic *Logic) MakeDashboard() *pb.JZYQTableDashboard {
	m := new(pb.JZYQTableDashboard)

	m.TableID = logic.TableNo()
	m.Step = pb.JZYQ_Step(logic.Game.Step)
	if len(logic.Game.Next) > 0 {
		m.NextAward = pb.JZYQ_BetType(logic.Game.Next[0])
	}
	m.SysPool = logic.SysPool.GetPool()
	m.SysPoolWinLine = logic.SysPool.GetWinLine()
	m.TotalBetAmount = logic.TotalBetAmount
	m.TotalWinLossAmount = logic.TotalWinLossAmount
	m.TotalValidAmount = logic.TotalValidAmount

	{ // 下注區域
		m.Goal = &pb.JZYQBetAreaInfo{
			Total:           logic.Game.BetSummary[Goal],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[Goal],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[Goal],
		}
		m.GoalInside = &pb.JZYQBetAreaInfo{
			Total:           logic.Game.BetSummary[GoalInside],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[GoalInside],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[GoalInside],
		}
		m.GoalOutside = &pb.JZYQBetAreaInfo{
			Total:           logic.Game.BetSummary[GoalOutside],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[GoalOutside],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[GoalOutside],
		}
		m.Lost = &pb.JZYQBetAreaInfo{
			Total:           logic.Game.BetSummary[Lost],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[Lost],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[Lost],
		}
		m.LostInside = &pb.JZYQBetAreaInfo{
			Total:           logic.Game.BetSummary[LostInside],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[LostInside],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[LostInside],
		}
		m.LostOutside = &pb.JZYQBetAreaInfo{
			Total:           logic.Game.BetSummary[LostOutside],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[LostOutside],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[LostOutside],
		}
	}

	{ // 玩家資訊
		for _, u := range logic.Game.Users {
			{ // 下注區域
				if u.BetSummary[Goal] > 0 {
					if !u.GameUser.IsRobot() {
						m.Goal.PlayerCount++
					} else {
						m.Goal.RobotCount++
					}
				}
				if u.BetSummary[GoalInside] > 0 {
					if !u.GameUser.IsRobot() {
						m.GoalInside.PlayerCount++
					} else {
						m.GoalInside.RobotCount++
					}
				}
				if u.BetSummary[GoalOutside] > 0 {
					if !u.GameUser.IsRobot() {
						m.GoalOutside.PlayerCount++
					} else {
						m.GoalOutside.RobotCount++
					}
				}
				if u.BetSummary[Lost] > 0 {
					if !u.GameUser.IsRobot() {
						m.Lost.PlayerCount++
					} else {
						m.Lost.RobotCount++
					}
				}
				if u.BetSummary[LostInside] > 0 {
					if !u.GameUser.IsRobot() {
						m.LostInside.PlayerCount++
					} else {
						m.LostInside.RobotCount++
					}
				}
				if u.BetSummary[LostOutside] > 0 {
					if !u.GameUser.IsRobot() {
						m.LostOutside.PlayerCount++
					} else {
						m.LostOutside.RobotCount++
					}
				}
			}
			{ // 玩家
				p := &pb.JZYQPlayerInfo{
					PID:         u.UserID,
					IsRobot:     u.GameUser.IsRobot(),
					BetAmount:   u.BetSum(),
					Goal:        u.BetSummary[Goal],
					GoalInside:  u.BetSummary[GoalInside],
					GoalOutside: u.BetSummary[GoalOutside],
					Lost:        u.BetSummary[Lost],
					LostInside:  u.BetSummary[LostInside],
					LostOutside: u.BetSummary[LostOutside],
				}
				m.Players = append(m.Players, p)
			}
		}
	}

	{ // 歷史資訊
		for _, h := range logic.Game.Records {
			m.History = append(m.History, pb.JZYQ_BetType(h.Award))
		}
	}

	m.Cheatable = logic.Game.Step == WaitBet && len(logic.Game.Next) == 0 && logic.Cheating == 0 && logic.SpecialRewardCount == 0
	m.Cheating = logic.Cheating > 0
	m.SpecialRewardCount = logic.SpecialRewardCount
	m.SpecialRewardRate = logic.SpecailRewardRate

	for _, n := range logic.Game.Next {
		m.NextAwardList = append(m.NextAwardList, pb.JZYQ_BetType(n))
	}
	m.TableDisabled = logic.Disabled

	return m
}

func (logic *Logic) Cheat(cnt int) {
	cheatable := logic.Game.Step == WaitBet && len(logic.Game.Next) == 0 && logic.Cheating == 0

	if !cheatable {
		return
	}

	logic.Cheating = cnt
	logic.EventChan <- logic.MakeDashboard()
}

func (logic *Logic) SubscripEvent(ctx context.Context) <-chan *pb.JZYQTableDashboard {
	ch := make(chan *pb.JZYQTableDashboard, 10)

	logic.BroadcastSubscriper.Store(ctx, ch)
	go func(ctx context.Context, ch chan *pb.JZYQTableDashboard) {
		<-ctx.Done()
		logic.BroadcastSubscriper.Delete(ctx)
		time.Sleep(time.Second)
		close(ch)
	}(ctx, ch)

	return ch
}

func (logic *Logic) EventWorker(c chan *pb.JZYQTableDashboard, m *sync.Map) {
	for e := range c {
		SubscriperLen := 0
		m.Range(func(key, value interface{}) bool {
			SubscriperLen++
			ch := value.(chan *pb.JZYQTableDashboard)
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

func (logic *Logic) checkBlock(u *gamelogic.GameUser) bool {
	for _, id := range logic.blocklist {
		if id == u.UserID {
			logic.KickUserParamFunc(id, "", roomutil.MsgParamUserLocked)
			return true
		}
	}
	return false
}

func (logic *Logic) cheat() {

	type BetArea struct {
		Total   int64
		UserCnt int
		UserIDs map[int32]*User
	}

	userBetSummary := func(game *Game) map[BetType]*BetArea {
		data := make(map[BetType]*BetArea)
		// init BetType
		for _, t := range BetAreas {
			data[t] = &BetArea{
				Total:   0,
				UserCnt: 0,
				UserIDs: make(map[int32]*User),
			}
		}

		for _, u := range game.Users {
			if u.GameUser.IsRobot() {
				continue
			}
			for _, t := range BetAreas {
				data[t].Total += u.BetSummary[t]
				if u.BetSummary[t] > 0 {
					data[t].UserCnt++
					data[t].UserIDs[u.UserID] = u
				}
			}
		}

		return data
	}

	format := `
			作弊第 %d 階段:
				得分[金額: %d 人數: %d]
				得分-線內[金額: %d 人數: %d]
				得分-線外[金額: %d 人數: %d]

				失分[金額: %d 人數: %d]
				失分-線內[金額: %d 人數: %d]
				失分-線外[金額: %d 人數: %d]`

	summary1 := userBetSummary(logic.Game)

	example.invalid(format, 1,
		summary1[Goal].Total, summary1[Goal].UserCnt,
		summary1[GoalInside].Total, summary1[GoalInside].UserCnt,
		summary1[GoalOutside].Total, summary1[GoalOutside].UserCnt,

		summary1[Lost].Total, summary1[Lost].UserCnt,
		summary1[LostInside].Total, summary1[LostInside].UserCnt,
		summary1[LostOutside].Total, summary1[LostOutside].UserCnt,
	)

	logic.GameLog.AddGameStep(format, 1,
		summary1[Goal].Total, summary1[Goal].UserCnt,
		summary1[GoalInside].Total, summary1[GoalInside].UserCnt,
		summary1[GoalOutside].Total, summary1[GoalOutside].UserCnt,

		summary1[Lost].Total, summary1[Lost].UserCnt,
		summary1[LostInside].Total, summary1[LostInside].UserCnt,
		summary1[LostOutside].Total, summary1[LostOutside].UserCnt,
	)

	// 決定要踢哪邊的玩家
	// * 踢下注最多金額的那邊
	// * 下注金額一樣，踢人數最多那邊
	// * 人數一樣多，就隨機
	var t1 BetType
	if summary1[Goal].Total > summary1[Lost].Total {
		t1 = Goal
	} else if summary1[Goal].Total < summary1[Lost].Total {
		t1 = Lost
	} else if summary1[Goal].UserCnt > summary1[Lost].UserCnt {
		t1 = Goal
	} else if summary1[Goal].UserCnt < summary1[Lost].UserCnt {
		t1 = Lost
	} else {
		if rand.Intn(2) == 0 {
			t1 = Goal
		} else {
			t1 = Lost
		}
	}

	if t1 != 0 {
		defer func() {
			logic.EventChan <- logic.MakeDashboard()
		}()

		area := summary1[t1]

		if area.UserCnt == 0 {
			example.invalid("作弊: 沒有玩家不踢")
			logic.GameLog.AddGameStep("作弊: 沒有玩家不踢")
			return
		}

		// 要踢2個人
		for i := 0; i < logic.Cheating; i++ {

			var users []int32
			for uid := range area.UserIDs {
				users = append(users, uid)
			}

			if len(users) == 0 {
				example.invalid("作弊: 已經沒有玩家不踢")
				logic.GameLog.AddGameStep("作弊: 沒有玩家不踢")
				continue
			}

			idx := rand.Intn(len(users))
			uid := users[idx]

			u := area.UserIDs[uid]
			if u != nil {
				// 刪除用戶數據之前先清除用戶下注數據
				logic.Game.DeleteUser(u.UserID)

				logic.KickUserParamFunc(uid, "", roomutil.MsgParamUserLocked)
				logic.UserLeave(u.GameUser)
				logic.blocklist = append(logic.blocklist, uid)

				example.invalid("作弊: 踢除玩家[%d]", uid)

				logic.GameLog.AddGameStep("作弊: 踢除玩家[%d]", uid)
				delete(area.UserIDs, uid)

				logic.BoardcastMessage(logic.mBoardcastBetSummary(uid))
			}
		}

		summary2 := userBetSummary(logic.Game)

		example.invalid(format, 2,
			summary2[Goal].Total, summary2[Goal].UserCnt,
			summary2[GoalInside].Total, summary2[GoalInside].UserCnt,
			summary2[GoalOutside].Total, summary2[GoalOutside].UserCnt,

			summary2[Lost].Total, summary2[Lost].UserCnt,
			summary2[LostInside].Total, summary2[LostInside].UserCnt,
			summary2[LostOutside].Total, summary2[LostOutside].UserCnt,
		)

		logic.GameLog.AddGameStep(format, 2,
			summary2[Goal].Total, summary2[Goal].UserCnt,
			summary2[GoalInside].Total, summary2[GoalInside].UserCnt,
			summary2[GoalOutside].Total, summary2[GoalOutside].UserCnt,

			summary2[Lost].Total, summary2[Lost].UserCnt,
			summary2[LostInside].Total, summary2[LostInside].UserCnt,
			summary2[LostOutside].Total, summary2[LostOutside].UserCnt,
		)

		var t2 BetType

		compenyWin := rand.Intn(100) < 70

		var sWin string
		switch compenyWin {
		case true:
			sWin = "公司贏"
		case false:
			sWin = "公司輸"
		}

		example.invalid("作弊第二階段: 公司贏機率[7/10] 公司輸機率[3/10] 結果[%s]", sWin)
		logic.GameLog.AddGameStep("作弊第二階段: 公司贏機率[7/10] 公司輸機率[3/10] 結果[%s]", sWin)

		if compenyWin { // 公司贏
			if summary2[Goal].Total > summary2[Lost].Total { // 進大於守 開 失分
				t2 = Lost
			} else if summary2[Goal].Total < summary2[Lost].Total { // 守大於進 開 得分
				t2 = Goal
			}
		} else { // 公司輸
			if summary2[Goal].Total > summary2[Lost].Total { // 進大於守 開 得分
				t2 = Goal
			} else if summary2[Goal].Total < summary2[Lost].Total { // 守大於進 開 失分
				t2 = Lost
			}
		}

		// 還沒找到就隨機
		if t2 == 0 {
			if rand.Intn(2) == 0 {
				t2 = Goal
			} else {
				t2 = Lost
			}
		}

		type option struct {
			Type   BetType
			Amount int64
		}
		// 找到哪邊就找左中右哪個賠最少
		switch t2 {
		case Goal:
			options := []option{
				{Type: GoalInside, Amount: logic.Game.UserBetSummary[GoalInside]},
				{Type: GoalOutside, Amount: logic.Game.UserBetSummary[GoalOutside]},
			}
			sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			t2 = options[0].Type
		case Lost:
			options := []option{
				{Type: LostInside, Amount: logic.Game.UserBetSummary[LostInside]},
				{Type: LostOutside, Amount: logic.Game.UserBetSummary[LostOutside]},
			}
			sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			t2 = options[0].Type
		}

		example.invalid("作弊: 指定開獎[%s]", t2)
		logic.GameLog.AddGameStep("作弊: 指定開獎[%s]", t2)

		logic.Game.Next = append(logic.Game.Next, t2)
	} else {
		example.invalid("作弊: 不指定開獎")
		logic.GameLog.AddGameStep("作弊: 不指定開獎")
	}
}

func (logic *Logic) specialReward() {

	format := `
		特殊開獎 機率 %d%% 剩餘 %d 次:
			Total[%d] 得分[%d]/得分-線內[%d]/得分-線外[%d]
			Total[%d] 失分[%d]/失分-線內[%d]/失分-線外[%d]`

	example.invalid(format, logic.SpecailRewardRate, logic.SpecialRewardCount,
		logic.Game.BetSummary[Goal]+logic.Game.BetSummary[GoalInside]+logic.Game.BetSummary[GoalOutside],
		logic.Game.BetSummary[Goal],
		logic.Game.BetSummary[GoalInside],
		logic.Game.BetSummary[GoalOutside],
		logic.Game.BetSummary[Lost]+logic.Game.BetSummary[LostInside]+logic.Game.BetSummary[LostOutside],
		logic.Game.BetSummary[Lost],
		logic.Game.BetSummary[LostInside],
		logic.Game.BetSummary[LostOutside],
	)

	logic.GameLog.AddGameStep(format, logic.SpecailRewardRate, logic.SpecialRewardCount,
		logic.Game.BetSummary[Goal]+logic.Game.BetSummary[GoalInside]+logic.Game.BetSummary[GoalOutside],
		logic.Game.BetSummary[Goal],
		logic.Game.BetSummary[GoalInside],
		logic.Game.BetSummary[GoalOutside],
		logic.Game.BetSummary[Lost]+logic.Game.BetSummary[LostInside]+logic.Game.BetSummary[LostOutside],
		logic.Game.BetSummary[Lost],
		logic.Game.BetSummary[LostInside],
		logic.Game.BetSummary[LostOutside],
	)

	// 骰出要不要特殊開獎
	if int32(rand.Intn(100)) < logic.SpecailRewardRate {

		// 計算押注最少的區域 得分/失分
		var t1 BetType
		goalGroup := logic.Game.BetSummary[Goal] + logic.Game.BetSummary[GoalInside] + logic.Game.BetSummary[GoalOutside]
		loseGroup := logic.Game.BetSummary[Lost] + logic.Game.BetSummary[LostInside] + logic.Game.BetSummary[LostOutside]
		if goalGroup > loseGroup {
			t1 = Lost
		} else if goalGroup < loseGroup {
			t1 = Goal
		} else {
			if rand.Intn(2) == 0 {
				t1 = Goal
			} else {
				t1 = Lost
			}
		}

		// type option struct {
		// 	Type   BetType
		// 	Amount int64
		// }

		// 計算押注最少的區域 左/中/右
		var t2 BetType
		switch t1 {
		case Goal:
			betType2 := []BetType{GoalInside, GoalOutside}
			t2 = betType2[rand.Intn(len(betType2))]
			// options := []option{
			// 	{Type: GoalInside, Amount: logic.Game.BetSummary[GoalInside]},
			// 	{Type: GoalOutside, Amount: logic.Game.BetSummary[GoalOutside]},
			// }
			// sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			// t2 = options[0].Type
		case Lost:
			betType2 := []BetType{LostInside, LostOutside}
			t2 = betType2[rand.Intn(len(betType2))]
			// options := []option{
			// 	{Type: LostInside, Amount: logic.Game.BetSummary[LostInside]},
			// 	{Type: LostOutside, Amount: logic.Game.BetSummary[LostOutside]},
			// }
			// sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			// t2 = options[0].Type
		}

		example.invalid("特殊開獎: 指定開獎[%s]", t2)
		logic.GameLog.AddGameStep("特殊開獎: 指定開獎[%s]", t2)

		logic.Game.Next = append(logic.Game.Next, t2)

	} else {
		example.invalid("特殊開獎: 不開獎")
		logic.GameLog.AddGameStep("特殊開獎: 不開獎")
	}

	logic.SpecialRewardCount--
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
