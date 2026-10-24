package sml

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

func (logic *Logic) MakeDashboard() *pb.SMLTableDashboard {
	m := new(pb.SMLTableDashboard)

	m.TableID = logic.TableNo()
	m.Step = pb.SML_Step(logic.Game.Step)
	if len(logic.Game.Next) > 0 {
		m.NextAward = pb.SML_BetType(logic.Game.Next[0])
	}
	m.SysPool = logic.SysPool.GetPool()
	m.SysPoolWinLine = logic.SysPool.GetWinLine()
	m.TotalBetAmount = logic.TotalBetAmount
	m.TotalWinLossAmount = logic.TotalWinLossAmount
	m.TotalValidAmount = logic.TotalValidAmount

	{ // 下注區域
		m.Goal = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[Goal],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[Goal],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[Goal],
		}
		m.GoalLeft = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[GoalLeft],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[GoalLeft],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[GoalLeft],
		}
		m.GoalMiddle = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[GoalMiddle],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[GoalMiddle],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[GoalMiddle],
		}
		m.GoalRight = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[GoalRight],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[GoalRight],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[GoalRight],
		}
		m.Save = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[Save],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[Save],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[Save],
		}
		m.SaveLeft = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[SaveLeft],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[SaveLeft],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[SaveLeft],
		}
		m.SaveMiddle = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[SaveMiddle],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[SaveMiddle],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[SaveMiddle],
		}
		m.SaveRight = &pb.SMLBetAreaInfo{
			Total:           logic.Game.BetSummary[SaveRight],
			PlayerCount:     0,
			PlayerBetAmount: logic.Game.UserBetSummary[SaveRight],
			RobotCount:      0,
			RobotBetAmount:  logic.Game.RobotBetSummary[SaveRight],
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
				if u.BetSummary[GoalLeft] > 0 {
					if !u.GameUser.IsRobot() {
						m.GoalLeft.PlayerCount++
					} else {
						m.GoalLeft.RobotCount++
					}
				}
				if u.BetSummary[GoalMiddle] > 0 {
					if !u.GameUser.IsRobot() {
						m.GoalMiddle.PlayerCount++
					} else {
						m.GoalMiddle.RobotCount++
					}
				}
				if u.BetSummary[GoalRight] > 0 {
					if !u.GameUser.IsRobot() {
						m.GoalRight.PlayerCount++
					} else {
						m.GoalRight.RobotCount++
					}
				}
				if u.BetSummary[Save] > 0 {
					if !u.GameUser.IsRobot() {
						m.Save.PlayerCount++
					} else {
						m.Save.RobotCount++
					}
				}
				if u.BetSummary[SaveLeft] > 0 {
					if !u.GameUser.IsRobot() {
						m.SaveLeft.PlayerCount++
					} else {
						m.SaveLeft.RobotCount++
					}
				}
				if u.BetSummary[SaveMiddle] > 0 {
					if !u.GameUser.IsRobot() {
						m.SaveMiddle.PlayerCount++
					} else {
						m.SaveMiddle.RobotCount++
					}
				}
				if u.BetSummary[SaveRight] > 0 {
					if !u.GameUser.IsRobot() {
						m.SaveRight.PlayerCount++
					} else {
						m.SaveRight.RobotCount++
					}
				}
			}
			{ // 玩家
				p := &pb.SMLPlayerInfo{
					PID:        u.UserID,
					IsRobot:    u.GameUser.IsRobot(),
					BetAmount:  u.BetSum(),
					Goal:       u.BetSummary[Goal],
					GoalLeft:   u.BetSummary[GoalLeft],
					GoalMiddle: u.BetSummary[GoalMiddle],
					GoalRight:  u.BetSummary[GoalRight],
					Save:       u.BetSummary[Save],
					SaveLeft:   u.BetSummary[SaveLeft],
					SaveMiddle: u.BetSummary[SaveMiddle],
					SaveRight:  u.BetSummary[SaveRight],
				}
				m.Players = append(m.Players, p)
			}
		}
	}

	{ // 歷史資訊
		for _, h := range logic.Game.Records {
			m.History = append(m.History, pb.SML_BetType(h.Award))
		}
	}

	m.Cheatable = logic.Game.Step == WaitBet && len(logic.Game.Next) == 0 && logic.Cheating == 0 && logic.SpecialRewardCount == 0
	m.Cheating = logic.Cheating > 0
	m.SpecialRewardCount = logic.SpecialRewardCount
	m.SpecialRewardRate = logic.SpecailRewardRate

	for _, n := range logic.Game.Next {
		m.NextAwardList = append(m.NextAwardList, pb.SML_BetType(n))
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

func (logic *Logic) SubscripEvent(ctx context.Context) <-chan *pb.SMLTableDashboard {
	ch := make(chan *pb.SMLTableDashboard, 10)

	logic.BroadcastSubscriper.Store(ctx, ch)
	go func(ctx context.Context, ch chan *pb.SMLTableDashboard) {
		<-ctx.Done()
		logic.BroadcastSubscriper.Delete(ctx)
		time.Sleep(time.Second)
		close(ch)
	}(ctx, ch)

	return ch
}

func (logic *Logic) EventWorker(c chan *pb.SMLTableDashboard, m *sync.Map) {
	for e := range c {
		SubscriperLen := 0
		m.Range(func(key, value interface{}) bool {
			SubscriperLen++
			ch := value.(chan *pb.SMLTableDashboard)
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
			進球[金額: %d 人數: %d]
			進球-左[金額: %d 人數: %d]
			進球-中[金額: %d 人數: %d]
			進球-右[金額: %d 人數: %d]

			守住[金額: %d 人數: %d]
			守住-左[金額: %d 人數: %d]
			守住-中[金額: %d 人數: %d]
			守住-右[金額: %d 人數: %d]`

	summary1 := userBetSummary(logic.Game)

	example.invalid(format, 1,
		summary1[Goal].Total, summary1[Goal].UserCnt,
		summary1[GoalLeft].Total, summary1[GoalLeft].UserCnt,
		summary1[GoalMiddle].Total, summary1[GoalMiddle].UserCnt,
		summary1[GoalRight].Total, summary1[GoalRight].UserCnt,

		summary1[Save].Total, summary1[Save].UserCnt,
		summary1[SaveLeft].Total, summary1[SaveLeft].UserCnt,
		summary1[SaveMiddle].Total, summary1[SaveMiddle].UserCnt,
		summary1[SaveRight].Total, summary1[SaveRight].UserCnt,
	)

	logic.GameLog.AddGameStep(format, 1,
		summary1[Goal].Total, summary1[Goal].UserCnt,
		summary1[GoalLeft].Total, summary1[GoalLeft].UserCnt,
		summary1[GoalMiddle].Total, summary1[GoalMiddle].UserCnt,
		summary1[GoalRight].Total, summary1[GoalRight].UserCnt,

		summary1[Save].Total, summary1[Save].UserCnt,
		summary1[SaveLeft].Total, summary1[SaveLeft].UserCnt,
		summary1[SaveMiddle].Total, summary1[SaveMiddle].UserCnt,
		summary1[SaveRight].Total, summary1[SaveRight].UserCnt,
	)

	// 決定要踢哪邊的玩家
	// * 踢下注最多金額的那邊
	// * 下注金額一樣，踢人數最多那邊
	// * 人數一樣多，就隨機
	var t1 BetType
	if summary1[Goal].Total > summary1[Save].Total {
		t1 = Goal
	} else if summary1[Goal].Total < summary1[Save].Total {
		t1 = Save
	} else if summary1[Goal].UserCnt > summary1[Save].UserCnt {
		t1 = Goal
	} else if summary1[Goal].UserCnt < summary1[Save].UserCnt {
		t1 = Save
	} else {
		if rand.Intn(2) == 0 {
			t1 = Goal
		} else {
			t1 = Save
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
			summary2[GoalLeft].Total, summary2[GoalLeft].UserCnt,
			summary2[GoalMiddle].Total, summary2[GoalMiddle].UserCnt,
			summary2[GoalRight].Total, summary2[GoalRight].UserCnt,

			summary2[Save].Total, summary2[Save].UserCnt,
			summary2[SaveLeft].Total, summary2[SaveLeft].UserCnt,
			summary2[SaveMiddle].Total, summary2[SaveMiddle].UserCnt,
			summary2[SaveRight].Total, summary2[SaveRight].UserCnt,
		)

		logic.GameLog.AddGameStep(format, 2,
			summary2[Goal].Total, summary2[Goal].UserCnt,
			summary2[GoalLeft].Total, summary2[GoalLeft].UserCnt,
			summary2[GoalMiddle].Total, summary2[GoalMiddle].UserCnt,
			summary2[GoalRight].Total, summary2[GoalRight].UserCnt,

			summary2[Save].Total, summary2[Save].UserCnt,
			summary2[SaveLeft].Total, summary2[SaveLeft].UserCnt,
			summary2[SaveMiddle].Total, summary2[SaveMiddle].UserCnt,
			summary2[SaveRight].Total, summary2[SaveRight].UserCnt,
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
			if summary2[Goal].Total > summary2[Save].Total { // 進大於守 開守
				t2 = Save
			} else if summary2[Goal].Total < summary2[Save].Total { // 守大於進 開進
				t2 = Goal
			}
		} else { // 公司輸
			if summary2[Goal].Total > summary2[Save].Total { // 進大於守 開進
				t2 = Goal
			} else if summary2[Goal].Total < summary2[Save].Total { // 守大於進 開守
				t2 = Save
			}
		}

		// 還沒找到就隨機
		if t2 == 0 {
			if rand.Intn(2) == 0 {
				t2 = Goal
			} else {
				t2 = Save
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
				{Type: GoalLeft, Amount: logic.Game.UserBetSummary[GoalLeft]},
				{Type: GoalMiddle, Amount: logic.Game.UserBetSummary[GoalMiddle]},
				{Type: GoalRight, Amount: logic.Game.UserBetSummary[GoalRight]},
			}
			sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			t2 = options[0].Type
		case Save:
			options := []option{
				{Type: SaveLeft, Amount: logic.Game.UserBetSummary[SaveLeft]},
				{Type: SaveMiddle, Amount: logic.Game.UserBetSummary[SaveMiddle]},
				{Type: SaveRight, Amount: logic.Game.UserBetSummary[SaveRight]},
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
			Total[%d] 進球[金額: %d]/進球-左[金額: %d]/進球-中[金額: %d]/進球-右[金額: %d]

			Total[%d] 守住[金額: %d]/守住-左[金額: %d]/守住-中[金額: %d]/守住-右[金額: %d]`

	example.invalid(format, logic.SpecailRewardRate, logic.SpecialRewardCount,
		logic.Game.BetSummary[Goal]+logic.Game.BetSummary[GoalLeft]+logic.Game.BetSummary[GoalMiddle]+logic.Game.BetSummary[GoalRight],
		logic.Game.BetSummary[Goal],
		logic.Game.BetSummary[GoalLeft],
		logic.Game.BetSummary[GoalMiddle],
		logic.Game.BetSummary[GoalRight],
		logic.Game.BetSummary[Save]+logic.Game.BetSummary[SaveLeft]+logic.Game.BetSummary[SaveMiddle]+logic.Game.BetSummary[SaveRight],
		logic.Game.BetSummary[Save],
		logic.Game.BetSummary[SaveLeft],
		logic.Game.BetSummary[SaveMiddle],
		logic.Game.BetSummary[SaveRight],
	)

	logic.GameLog.AddGameStep(format, logic.SpecailRewardRate, logic.SpecialRewardCount,
		logic.Game.BetSummary[Goal]+logic.Game.BetSummary[GoalLeft]+logic.Game.BetSummary[GoalMiddle]+logic.Game.BetSummary[GoalRight],
		logic.Game.BetSummary[Goal],
		logic.Game.BetSummary[GoalLeft],
		logic.Game.BetSummary[GoalMiddle],
		logic.Game.BetSummary[GoalRight],
		logic.Game.BetSummary[Save]+logic.Game.BetSummary[SaveLeft]+logic.Game.BetSummary[SaveMiddle]+logic.Game.BetSummary[SaveRight],
		logic.Game.BetSummary[Save],
		logic.Game.BetSummary[SaveLeft],
		logic.Game.BetSummary[SaveMiddle],
		logic.Game.BetSummary[SaveRight],
	)

	// 骰出要不要特殊開獎
	if int32(rand.Intn(100)) < logic.SpecailRewardRate {

		// 計算押注最少的區域 進球/守住
		var t1 BetType
		goalGroup := logic.Game.BetSummary[Goal] + logic.Game.BetSummary[GoalLeft] + logic.Game.BetSummary[GoalMiddle] + logic.Game.BetSummary[GoalRight]
		saveGroup := logic.Game.BetSummary[Save] + logic.Game.BetSummary[SaveLeft] + logic.Game.BetSummary[SaveMiddle] + logic.Game.BetSummary[SaveRight]

		if goalGroup > saveGroup {
			t1 = Save
		} else if goalGroup < saveGroup {
			t1 = Goal
		} else {
			if rand.Intn(2) == 0 {
				t1 = Goal
			} else {
				t1 = Save
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
			// options := []option{
			// 	{Type: GoalLeft, Amount: logic.Game.BetSummary[GoalLeft]},
			// 	{Type: GoalMiddle, Amount: logic.Game.BetSummary[GoalMiddle]},
			// 	{Type: GoalRight, Amount: logic.Game.BetSummary[GoalRight]},
			// }
			// sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			// t2 = options[0].Type
			betType2 := []BetType{GoalLeft, GoalMiddle, GoalRight}
			t2 = betType2[rand.Intn(len(betType2))]
		case Save:
			// options := []option{
			// 	{Type: SaveLeft, Amount: logic.Game.BetSummary[SaveLeft]},
			// 	{Type: SaveMiddle, Amount: logic.Game.BetSummary[SaveMiddle]},
			// 	{Type: SaveRight, Amount: logic.Game.BetSummary[SaveRight]},
			// }
			// sort.Slice(options, func(i, j int) bool { return options[i].Amount < options[j].Amount })
			// t2 = options[0].Type
			betType2 := []BetType{SaveLeft, SaveMiddle, SaveRight}
			t2 = betType2[rand.Intn(len(betType2))]
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
