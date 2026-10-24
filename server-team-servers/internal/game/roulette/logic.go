package roulette

import (
	"context"
	"fmt"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"

	"example.invalid/golang/protobuf/proto"

	apipb "example.invalid/ServerTeam/Servers/internal/api-service/pb/apisvc"
	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/roomutil"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	AnimateStart     = "AnimateStart"
	AniamteWaitBet   = "AniamteWaitBet"
	AniamteShowAward = "AniamteShowAward"
	AnimateFinish    = "AnimateFinish"
)

type Logic struct {
	*gamelogic.ComputeGameLogicBase

	Game        *Game     // 遊戲主體
	GameOptions []GameOpt // 遊戲初始設定參數

	AutoStart bool // 是否自動開始

	GameStepTimeout map[GameStep]time.Duration    // 每個 GameStep 的延遲時間設定
	GameStepTimer   map[GameStep]*event.TimerFunc // 每個 GameStep 的 Timer
	RichestCount    int                           // 富豪榜人數
	WinListCount    int                           // 贏錢排行人數

	FlowCalctor FlowCalctor // 流水計算器 Default: LossOnlyFlowCalctor

	HooksBefore map[GameStep][]func() // 每個 GameStep 被註冊改做的事 (前)
	HooksAfter  map[GameStep][]func() // 每個 GameStep 被註冊改做的事 (後)

	Richests []*gamelogic.GameUser // 當局富豪榜列表，每局開始時抓取一次，中間進入的人不被計算進去

	EventChan           chan *pb.CJLHLPTableDashboard
	BroadcastSubscriper *sync.Map
	Cheating            int     // 作弊要踢的人數
	blocklist           []int32 // 暫時不要讓他進來的人

	SpecialRewardCount int32 // 特殊獎勵次數
	SpecailRewardRate  int32 // 特殊獎勵機率 Default: 70

	TotalBetAmount     int64 // 今日下注總額
	TotalWinLossAmount int64 // 今日輸贏總額
	TotalValidAmount   int64 // 今日輸贏總額

	Disabled bool // 是否關閉遊戲
}

func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.Group) *Logic {

	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)

	logic.GameStepTimeout = make(map[GameStep]time.Duration)
	logic.GameStepTimer = make(map[GameStep]*event.TimerFunc)
	logic.HooksBefore = map[GameStep][]func(){}
	logic.HooksAfter = map[GameStep][]func(){}
	logic.EventChan = make(chan *pb.CJLHLPTableDashboard, 10)
	logic.BroadcastSubscriper = new(sync.Map)
	logic.SpecailRewardRate = 70

	logic.initGameOptions(cfgs)

	game, err := NewGame(logic.GameOptions...)
	if err != nil {
		mlog.Fatal("Create Game Error, %v", err)
	}

	logic.Game = game

	logic.RegisterAfterGameStartFunc(func() {
		if logic.Disabled {
			for _, user := range logic.AllGameUser() {
				if !user.IsRobot() {
					logic.KickUserParamFunc(user.UserID, "", roomutil.MsgParamTableDisabled)
				}
			}
		}
		logic.queryBetSummary()
		logic.Start()
	})

	logic.RegisterBeforeGameEndFunc(func() {
		logic.GameLog.GameEnd()
		for _, gu := range logic.AllGameUser() {
			if gu.IsOffLine() {
				logic.Game.DeleteUser(gu.UserID)
			}
		}
	})

	go logic.EventWorker(logic.EventChan, logic.BroadcastSubscriper)

	if logic.AutoStart {
		go func() {
			ctx, cancel := context.WithCancel(context.TODO())
			defer cancel()

			for {
				select {
				case <-ctx.Done():
					return
				case <-time.After(500 * time.Millisecond):
					logic.GameStartFunc()
					cancel()
				}
			}
		}()
	}

	return logic
}

func (logic *Logic) initGameOptions(cfgs *config.Group) {
	{ // 下注區域設定
		section := cfgs.GetConfigSection(gamelogic.ConfigFileNameGameServer, "bet")

		parse := func(t BetType) (float64, int64, error) {
			k := strconv.Itoa(int(t))
			s := section[k]
			s = strings.TrimSpace(s)
			strs := strings.Split(s, ",")
			if len(strs) != 2 {
				return 0, 0, fmt.Errorf("BetType %v config error, want %v args but %v", t, 2, len(strs))
			}
			odd, err := strconv.ParseFloat(strings.TrimSpace(strs[0]), 64)
			if err != nil {
				return 0, 0, fmt.Errorf("BetType %v config, odd parse error, %v", t, err)
			}
			max, err := strconv.ParseInt(strings.TrimSpace(strs[1]), 0, 64)
			if err != nil {
				return 0, 0, fmt.Errorf("BetType %v config, max bet parse error, %v", t, err)
			}
			return odd, max, nil
		}

		conf := NewBetConfig()
		for _, t := range BetAreas {
			odd, max, err := parse(t)
			if err != nil {
				conf.AddBetType(DefaultBetTypeMap[t])
				continue
			}
			conf.AddBetType(BetTypeConfig{Type: t, Odd: odd, MaxBet: max})
		}

		opt := &BetConfigOpt{Conf: conf}

		logic.GameOptions = append(logic.GameOptions, opt)
	}

	{ // 下注面額設定
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameServer, "betcount")
		strs := strings.Split(strings.TrimSpace(v), ",")
		opt := &BetAmountOpt{}
		for _, str := range strs {
			i, err := strconv.ParseInt(strings.TrimSpace(str), 0, 64)
			if err != nil {
				mlog.Warn("Config betcount parse error, %v", err)
				continue
			}
			opt.Amounts = append(opt.Amounts, i)
		}
		logic.GameOptions = append(logic.GameOptions, opt)
	}

	{ // 下注時最小攜帶金額設定
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameInfo, "betmincarry")
		if i, err := strconv.ParseInt(strings.TrimSpace(v), 0, 64); err == nil {
			opt := &BetMinCarryMoneyOpt{Amount: i}
			logic.GameOptions = append(logic.GameOptions, opt)
		}
	}

	{ // 下注時玩家最大下注金額設定
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameInfo, "usermaxbet")
		if i, err := strconv.ParseInt(strings.TrimSpace(v), 0, 64); err == nil {
			opt := &UserMaxBetOpt{Amount: i}
			logic.GameOptions = append(logic.GameOptions, opt)
		} else {
			opt := &UserMaxBetOpt{Amount: DefaultUserMaxBet}
			logic.GameOptions = append(logic.GameOptions, opt)
		}
	}

	{ // 紀錄保留筆數
		section := cfgs.GetConfigSection(gamelogic.ConfigFileNameGameServer, "record")
		size, err := strconv.Atoi(section["maxsize"])
		if err != nil {
			opt := &RecordMaxSizeOpt{Size: 30}
			logic.GameOptions = append(logic.GameOptions, opt)
		} else {
			opt := &RecordMaxSizeOpt{Size: size}
			logic.GameOptions = append(logic.GameOptions, opt)
		}
	}

	{ // 水線開最小獎機率
		opt := &WaterLineCheckerOpt{
			Func: func() bool {
				b := logic.SysPool.IsCtrlSystemWin()
				logic.GameLog.AddGameStep("%s and IsCtrl=%v", logic.SysPool.ToString(), b)
				return b
			},
		}
		logic.GameOptions = append(logic.GameOptions, opt)
	}

	{ // 動畫時間
		section := cfgs.GetConfigSection(gamelogic.ConfigFileNameGameServer, "animate")
		{ // start=3s            // 遊戲開始動畫時間 Default: 3s
			d, err := time.ParseDuration(strings.TrimSpace(section["start"]))
			if err != nil {
				d = 3 * time.Second
			}
			logic.GameStepTimeout[Start] = d
		}
		{ // bet=10s             // 下注時間 Default: 10s
			d, err := time.ParseDuration(strings.TrimSpace(section["bet"]))
			if err != nil {
				d = 10 * time.Second
			}
			logic.GameStepTimeout[WaitBet] = d
		}
		{ // award=5s            // 表演開獎時間 Default: 5s
			d, err := time.ParseDuration(strings.TrimSpace(section["award"]))
			if err != nil {
				d = 5 * time.Second
			}
			logic.GameStepTimeout[ShowAward] = d
		}
		{ // finish=2s           // 遊戲結束時間 Default: 2s
			d, err := time.ParseDuration(strings.TrimSpace(section["finish"]))
			if err != nil {
				d = 2 * time.Second
			}
			logic.GameStepTimeout[Finish] = d
		}
	}

	{ // 富豪榜人數 Default: 7
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameInfo, "richestcount")
		i, err := strconv.Atoi(v)
		if err != nil {
			logic.RichestCount = 7
		} else {
			logic.RichestCount = i
		}
	}

	{ // 贏錢排行人數 Default: 3
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameInfo, "winlistcount")
		i, err := strconv.Atoi(v)
		if err != nil {
			logic.WinListCount = 3
		} else {
			logic.WinListCount = i
		}
	}

	{ // 是否自動開始 Default: false
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameInfo, "autostart")
		i, err := strconv.ParseBool(v)
		if err != nil {
			logic.AutoStart = false
		} else {
			logic.AutoStart = i
		}
	}

	{ // 流水計算器 Default: LossOnlyFlowCalctor
		// 0 or 1, 0: LossOnlyFlowCalctor, 1: EffectiveBetFlowCalctor
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameInfo, "flowtype")
		i, err := strconv.Atoi(v)
		if err != nil {
			logic.FlowCalctor = LossOnlyFlowCalctor{}
		} else {
			switch i {
			case 0:
				logic.FlowCalctor = LossOnlyFlowCalctor{}
			case 1:
				logic.FlowCalctor = EffectiveBetFlowCalctor{}
			default:
				logic.FlowCalctor = LossOnlyFlowCalctor{}
			}
		}
	}
}

// PiSaiMessage 遊戲封包
func (logic *Logic) PiSaiMessage(user *gamelogic.GameUser, m *msg.Message) {}

// GameMessage 遊戲封包
func (logic *Logic) GameMessage(user *gamelogic.GameUser, m *msg.Message) {
	if !logic.CheckGameUserMessage(user) {
		logic.KickUserFunc(user.UserID, "非法操作")
		return
	}
	if m.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	switch m.SClassID {
	case int32(netproto.Roulette_GameMessageClassID_RouletteBetID):
		logic.bet(user, m)
	case int32(netproto.Roulette_GameMessageClassID_RouletteBetClearID):
		logic.undo(user)
	}
}

// Start 遊戲階段 開始遊戲
func (logic *Logic) Start() {

	for _, f := range logic.HooksBefore[Start] {
		f()
	}

	logic.blocklist = []int32{}
	logic.Richests = logic.richest(logic.RichestCount)

	logic.BoardcastMessage(logic.mRichestList())
	logic.BoardcastMessage(logic.mLuckyStarList())

	logic.Game.NewRound()

	example.invalid("牌桌[%d] %v", logic.TableNo(), logic.Game.Step)
	logic.GameLog.AddGameStep("牌桌[%d] %v", logic.TableNo(), logic.Game.Step)

	for _, f := range logic.HooksAfter[Start] {
		f()
	}

	logic.GameStepTimer[Start] = logic.TimerFuncManager.NewNamedTimer(int32(logic.GameStepTimeout[Start]/time.Second), logic.WaitBet, AnimateStart)
	logic.BoardcastMessage(logic.mStep())

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// WaitBet 遊戲階段 等待下注
func (logic *Logic) WaitBet() {

	for _, f := range logic.HooksBefore[WaitBet] {
		f()
	}

	logic.Game.StartBet()

	example.invalid("牌桌[%d] %v", logic.TableNo(), logic.Game.Step)
	logic.GameLog.AddGameStep("牌桌[%d] %v", logic.TableNo(), logic.Game.Step)

	for _, f := range logic.HooksAfter[WaitBet] {
		f()
	}

	logic.GameStepTimer[WaitBet] = logic.TimerFuncManager.NewNamedTimer(int32(logic.GameStepTimeout[WaitBet]/time.Second), logic.ShowAward, AniamteWaitBet)
	logic.BoardcastMessage(logic.mStep())

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// ShowAward 遊戲階段 開獎動畫
func (logic *Logic) ShowAward() {

	for _, f := range logic.HooksBefore[ShowAward] {
		f()
	}

	if len(logic.Game.Next) > 0 {
		logic.GameLog.AddGameStep("牌桌[%d] 指定開獎 %v", logic.TableNo(), logic.Game.Next[0])
	}

	award, err := logic.Game.Award()
	if err != nil {
		mlog.Error("牌桌[%d] 開獎錯誤, %v", logic.TableNo(), err)
		logic.GameEndFunc()
		return
	}

	example.invalid("牌桌[%d] %v %v", logic.TableNo(), logic.Game.Step, award)
	logic.GameLog.AddGameStep("牌桌[%d] %v %v", logic.TableNo(), logic.Game.Step, award)

	// 計算所有人金幣
	logic.calcAward(award)

	for _, f := range logic.HooksAfter[ShowAward] {
		f()
	}

	logic.GameStepTimer[ShowAward] = logic.TimerFuncManager.NewNamedTimer(int32(logic.GameStepTimeout[ShowAward]/time.Second), logic.Finish, AniamteShowAward)
	logic.BoardcastMessage(logic.mStep())

	logic.BoardcastMessage(logic.mGameResult())
	logic.BoardcastMessage(logic.mRound())

	for _, u := range logic.Game.Users {
		if u.GameUser == nil {
			continue
		}
		logic.SendMessageToUser(u.GameUser, logic.mGameResultWin(u))
	}

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// Finish 遊戲階段 結束遊戲
func (logic *Logic) Finish() {

	for _, f := range logic.HooksBefore[Finish] {
		f()
	}

	logic.Game.Finish()

	example.invalid("牌桌[%d] 結束遊戲", logic.TableNo())
	logic.GameLog.AddGameStep("牌桌[%d] %v", logic.TableNo(), logic.Game.Step)

	// 如果玩家不在線上把玩家清理掉
	for _, u := range logic.AllGameUser() {
		if u.IsOffLine() {
			if u, found := logic.Game.Users[u.UserID]; !found || u.BetSum() <= 0 {
				logic.GameLog.AddGameStep("牌桌[%d] 玩家[%d] 不在線上且沒有押注，移除玩家", logic.TableNo(), u.UserID)
				logic.Game.DeleteUser(u.UserID)
			}
		}
	}

	// 移除沒有押注的 gamelog 玩家
	for uid := range logic.GameLog.Context().Users {
		if u, found := logic.Game.Users[uid]; !found || u.BetSum() <= 0 {
			logic.GameLog.Context().RemoveUser(uid)
		}
	}

	for _, f := range logic.HooksAfter[Finish] {
		f()
	}

	logic.GameStepTimer[Finish] = logic.TimerFuncManager.NewNamedTimer(int32(logic.GameStepTimeout[Finish]/time.Second), logic.GameEndFunc, AnimateFinish)
	logic.BoardcastMessage(logic.mStep())

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// UserEnter 玩家進入
func (logic *Logic) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {

	if logic.Disabled && !user.IsRobot() {
		logic.KickUserParamFunc(user.UserID, "", roomutil.MsgParamTableDisabled)
		user.ConnInfo.Close()
		return
	}

	mlog.Debug("牌桌[%d] 玩家[%d] 進入", logic.TableNo(), user.UserID)

	logic.ComputeGameLogicBase.UserEnter(user, tableno, seatno)

	// 添加玩家
	logic.Game.AddUser(user)

	// 發送場景
	logic.SendScene(user)

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// UserLeave 玩家離開
func (logic *Logic) UserLeave(user *gamelogic.GameUser) bool {

	if logic.IsGaming() {
		if logic.Game.Step == WaitBet {
			u := logic.Game.Users[user.UserID]
			if u != nil && u.BetSum() > 0 {
				mlog.Debug("牌桌[%d] 玩家[%d] 有下注還不能離開", logic.TableNo(), user.UserID)
				return false
			}
		} else {
			return false
		}

		logic.ChangeUserStateEnd(user)
		logic.Game.DeleteUser(user.UserID)

		// 更新牌桌資訊
		logic.EventChan <- logic.MakeDashboard()
	}

	return logic.ComputeGameLogicBase.UserLeave(user)
}

// UserRecover 玩家斷線重連
func (logic *Logic) UserRecover(user *gamelogic.GameUser) {
	logic.ComputeGameLogicBase.UserRecover(user)

	// 添加玩家
	logic.Game.AddUser(user)

	// 發送場景
	logic.SendScene(user)

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// RegistHooksBefore 註冊某遊戲階段前該做的事
func (logic *Logic) RegistHooksBefore(step GameStep, f func()) {
	logic.HooksBefore[step] = append(logic.HooksBefore[step], f)
}

// RegistHooksAfter 註冊某遊戲階段後該做的事
func (logic *Logic) RegistHooksAfter(step GameStep, f func()) {
	logic.HooksAfter[step] = append(logic.HooksAfter[step], f)
}

// 計算輸贏獎勵
func (logic *Logic) calcAward(award uint32) {

	var uBet, uWin, uRtn, uTax, uFlow int64 // 玩家總下注，總贏錢，總稅收，總流水
	var rBet, rWin int64                    // 機器人總下注，總贏錢

	logic.GameLog.AddGameStep("牌桌[%d] 流水模式: %v", logic.TableNo(), logic.FlowCalctor)

	ctx := logic.GameLog.Context()
	ctx.SettleTime = time.Now()
	wg := sync.WaitGroup{}

	for _, u := range logic.Game.Users {
		bet := u.BetSum()                               // 總下注額
		r, win := u.Award(award, *logic.Game.BetConfig) // 總贏金額

		if bet == 0 && win == 0 {
			ctx.RemoveUser(u.UserID)
			continue
		}

		tax := int64(float64(win) * (float64(logic.GetServerConfig().TaxScale) / 100)) // 總稅金
		rtn := win - tax + r                                                           // 返還金額
		update := rtn - bet                                                            // 更新金額
		flow, err := logic.FlowCalctor.Calc(u.Bets)                                    // 流水
		if err != nil {
			mlog.Error("牌桌[%d] 玩家[%d] 計算流水錯誤, %v", logic.TableNo(), u.UserID, err)
		}

		ctxU := ctx.Users[u.UserID]
		ctxU.BetScore = bet
		ctxU.PayoutScore = win + r
		ctxU.TaxAmount = tax
		ctxU.ValidScore = flow
		ctxU.UpdateScore = update

		u.GameUser.BetAmount += bet
		u.GameUser.FlowAmount += flow
		uid := u.UserID

		defer func() {
			mlog.Debug("玩家[%d] 下注[%d] 贏錢[%d] 稅金[%d] 流水[%d] 返還金額[%d] 更新金額[%d] 最終金額[%d]", uid, bet, win, tax, flow, rtn, update, u.GameUser.Score)
		}()

		if u.GameUser.IsRobot() {
			rBet += bet
			rWin += win

			// 寫分
			logic.UpdateUserScoreFunc(u.UserID, update, tax)
		} else {
			uBet += bet
			uWin += win
			uTax += tax
			uFlow += flow
			uRtn += rtn

			format := `玩家[%d] 下注[%d] 贏錢[%d] 稅金[%d] 流水[%d] 返還金額[%d] 更新金額[%d]`

			logic.GameLog.AddGameStep(format,
				u.UserID, bet, win, tax, flow, rtn, update,
			)
			example.invalid(format,
				u.UserID, bet, win, tax, flow, rtn, update,
			)

			for _, b := range u.Bets {
				logic.GameLog.AddGameStep("    [%v] 下注[%d] 贏[%d]退[%d] 號碼%v", b.Type, b.Count, b.Win, b.Rtn, b.Numbers)
				example.invalid("    [%v] 下注[%d] 贏[%d]退[%d] 號碼%v", b.Type, b.Count, b.Win, b.Rtn, b.Numbers)
			}

			// 寫分
			wg.Add(1)
			go func(u *User, t time.Time) {
				defer wg.Done()
				var transactionNo string

				if err := app.Context.Invoke(func(c apipb.GameBetServiceClient) error {
					p := logic.TableManager.Room.RoomUserManager.GetPlayerByUserID(u.UserID)
					if p == nil {
						return fmt.Errorf("player %d not fount", u.UserID)
					}
					req := new(apipb.PlaceBetReq)
					req.AccessToken = [REDACTED]
					req.ServerID = logic.TableManager.Room.ServerID
					req.TradeID = u.GameUser.TradeID
					req.BetAmount = bet

					example.invalid("牌桌[%d] API 下注 請求 玩家[%d] BetAmount: %d", logic.TableNo(), u.UserID, req.BetAmount)

					res, err := c.PlaceBet(context.Background(), req)
					if err != nil {
						return err
					}
					example.invalid("牌桌[%d] API 下注 回覆 玩家[%d] TransactionNo: %s, AfterAmount: %d", logic.TableNo(), u.UserID, res.TransactionNo, res.AfterAmount)

					transactionNo = res.TransactionNo
					ctxU.TransactionNo = transactionNo

					return nil
				}); err != nil {
					mlog.Error("牌桌[%d] Place Bet Error, %v", logic.TableNo(), err)
					logic.KickUserFunc(u.UserID, "System Error ...")
					uBet -= bet
					uWin -= win
					ctx.RemoveUser(u.UserID)
					return
				}

				if len(transactionNo) == 0 {
					ctx.RemoveUser(u.UserID)
					return
				}

				if err := app.Context.Invoke(func(c apipb.GameResultServiceClient) error {

					req := new(apipb.SettlementReq)
					req.PlayerID = u.UserID
					req.ServerID = logic.TableManager.Room.ServerID
					req.TradeID = u.GameUser.TradeID
					req.UpdateAmount = rtn
					req.TransactionNo = transactionNo
					req.BetAmount = bet
					req.ValidAmount = flow
					req.PayoutAmount = rtn
					req.TaxAmount = tax
					req.SettleTime = util.FormatTimeStr(t.UTC())

					example.invalid("牌桌[%d] API 結算 請求 玩家[%d] PlayerID: %d ServerID: %d TradeID: %d UpdateAmount: %d TransactionNo: %s BetAmount: %d ValidAmount: %d PayoutAmount: %d TaxAmount: %d", logic.TableNo(), u.UserID,
						req.PlayerID,
						req.ServerID,
						req.TradeID,
						req.UpdateAmount,
						req.TransactionNo,
						req.BetAmount,
						req.ValidAmount,
						req.PayoutAmount,
						req.TaxAmount)

					res, err := c.SettlementV2(context.Background(), req)
					if err != nil {
						mlog.Error("牌桌[%d] 更新玩家金幣錯誤 SettlemmentV2 Error, %v", logic.TableNo(), err)
						return err
					}
					example.invalid("牌桌[%d] API 結算 回覆 玩家[%d] AfterAmount: %d", logic.TableNo(), u.UserID, res.AfterAmount)

					// 原先 UpdateUserScoreFunc 內做得要加回來
					u.GameUser.UpdateScore += update

					// 原先在 roomactionmanager.go 內做得要加回來
					if update > 0 {
						u.GameUser.WinCount++
						u.GameUser.UpdateWinCount++
						u.GameUser.LastWinLose = 1
					} else {
						u.GameUser.LoseCount++
						u.GameUser.UpdateLoseCount++
						u.GameUser.LastWinLose = 0
					}

					u.GameUser.Score = res.AfterAmount

					return nil
				}); err != nil {
					mlog.Error("牌桌[%d] 更新玩家金幣錯誤 %v", logic.TableNo(), err)
					logic.KickUserFunc(u.UserID, "System Error ...")
					uBet -= bet
					uWin -= win
					ctx.RemoveUser(u.UserID)
					return
				}
			}(u, ctx.SettleTime)

		}
	}

	wg.Wait()

	example.invalid("牌桌[%d] 開獎耗時 %v", logic.TableNo(), time.Since(ctx.SettleTime))

	if rBet > 0 {
		format := `機器人 下注[%d] 贏錢[%d]`

		logic.GameLog.AddGameStep(format, rBet, rWin)
	}

	// 系統輸贏
	sysWin := uBet - uRtn
	log := logic.SysPool.UpdateSysPoolAndTax(sysWin, uTax, uFlow)
	logic.GameLog.AddGameStep(log)
}

// 取得最有錢的 n 個人
func (logic *Logic) richest(n int) []*gamelogic.GameUser {

	var richests []*gamelogic.GameUser

	richests = append(richests, logic.AllTableUser()...)

	sort.Slice(richests, func(i, j int) bool { return richests[i].Score > richests[j].Score })

	if len(richests) > n {
		richests = richests[:n]
	}

	return richests
}

// 玩家下注
func (logic *Logic) bet(user *gamelogic.GameUser, m *msg.Message) {

	data := new(netproto.Roulette_BetRequest)

	if err := proto.Unmarshal(m.MsgData.([]byte), data); err != nil {
		mlog.Warn("牌桌[%d] 玩家[%d]下注 解析 proto 錯誤, %v", logic.TableNo(), user.UserID, err)

		returnM := new(msg.Message)
		returnM.BClassID = int32(netproto.MessageBClassID_Game)
		returnM.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetRetID)

		returnData := new(netproto.Roulette_BetResponse)
		returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeInvalidArgument.Enum()

		returnM.MsgData = returnData

		logic.SendMessageToUser(user, returnM)
		return
	}

	u := logic.Game.Users[user.UserID]
	if u == nil {
		u = logic.Game.AddUser(user)
	}

	var successCnt int

	for _, bet := range data.Bet {
		if err := logic.Game.Bet(user.UserID, Bet{
			Type:    ReverseBetType(bet.GetType()),
			Count:   bet.GetMoney(),
			Numbers: bet.GetNumbers(),
		}); err != nil {

			// 移除這次下注成功的, user.Undo()
			for i := 0; i < successCnt; i++ {
				u.Undo()
			}

			returnM := new(msg.Message)
			returnM.BClassID = int32(netproto.MessageBClassID_Game)
			returnM.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetRetID)

			returnData := new(netproto.Roulette_BetResponse)

			if e, ok := err.(*ErrorStepNotAllow); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 非下注狀態, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeNotBetStep.Enum()
			} else if e, ok := err.(*ErrorMoneyNotEnough); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 用戶金額不足, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeMoneyNotEnough.Enum()
			} else if e, ok := err.(*ErrorUserNotExist); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 玩家不在遊戲內, %v", logic.TableNo(), user.UserID, e)
				return
			} else if e, ok := err.(*ErrorCarryMoneyNotEnough); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 攜帶金額不夠多, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeCarryMoneyNotEnough.Enum()
			} else if e, ok := err.(*ErrorBetTypeOverLimit); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 此類下注已達上限, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeBetTypeOverLimit.Enum()
			} else if e, ok := err.(*ErrorIllegalBetAmount); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 非法的下注金額, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalBetMoney.Enum()
			} else if e, ok := err.(*ErrorAlreadyBetOtherSide); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 不能對壓, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeAlreadyBetOtherSide.Enum()
			} else if e, ok := err.(*ErrorIllegalBetType); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 非法下注類型, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalBetType.Enum()
			} else if e, ok := err.(*ErrorIllegalBetNumber); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 非法下注號碼組合, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalNumbers.Enum()
			} else if e, ok := err.(*ErrorIllegalBetNumberCount); ok {
				mlog.Debug("牌桌[%d] 玩家[%d]下注 非法下注號碼數量, %v", logic.TableNo(), user.UserID, e)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalNumbers.Enum()
			} else {
				mlog.Warn("牌桌[%d] 玩家[%d]下注 未知錯誤, %v", logic.TableNo(), user.UserID, err)
				returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeInternalError.Enum()
			}

			returnM.MsgData = returnData

			logic.SendMessageToUser(user, returnM)
			return
		}

		successCnt++
	}

	{ // 下注結果
		returnM := new(msg.Message)
		returnM.BClassID = int32(netproto.MessageBClassID_Game)
		returnM.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetRetID)

		returnData := new(netproto.Roulette_BetResponse)

		returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeSuccess.Enum()
		returnData.Money = proto.Int64(user.Score - u.BetSum()) // 預計算玩家的錢

		for _, bet := range u.Bets {
			b := new(netproto.Roulette_Bet)
			b.Type = TransforBetType(bet.Type).Enum()
			b.Money = proto.Int64(bet.Count)
			b.Numbers = append(b.Numbers, bet.Numbers...)
			returnData.Bets = append(returnData.Bets, b)
		}

		returnM.MsgData = returnData

		logic.SendMessageToUser(user, returnM)
	}

	// 廣播玩家下注
	logic.BoardcastMessage(logic.mBoardcastBetSummary(user.UserID))

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}

// 玩家取消下注
func (logic *Logic) undo(user *gamelogic.GameUser) {

	u := logic.Game.Users[user.UserID]
	if u == nil {
		u = logic.Game.AddUser(user)
	}

	if err := logic.Game.Undo(user.UserID); err != nil {
		returnM := new(msg.Message)
		returnM.BClassID = int32(netproto.MessageBClassID_Game)
		returnM.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetClearRetID)

		returnData := new(netproto.Roulette_BetResponse)

		if e, ok := err.(*ErrorStepNotAllow); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 非下注狀態, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeNotBetStep.Enum()
		} else if e, ok := err.(*ErrorMoneyNotEnough); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 用戶金額不足, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeMoneyNotEnough.Enum()
		} else if e, ok := err.(*ErrorUserNotExist); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 玩家不在遊戲內, %v", logic.TableNo(), user.UserID, e)
			return
		} else if e, ok := err.(*ErrorCarryMoneyNotEnough); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 攜帶金額不夠多, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeCarryMoneyNotEnough.Enum()
		} else if e, ok := err.(*ErrorBetTypeOverLimit); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 此類下注已達上限, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeBetTypeOverLimit.Enum()
		} else if e, ok := err.(*ErrorIllegalBetAmount); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 非法的下注金額, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalBetMoney.Enum()
		} else if e, ok := err.(*ErrorAlreadyBetOtherSide); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 不能對壓, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeAlreadyBetOtherSide.Enum()
		} else if e, ok := err.(*ErrorIllegalBetType); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 非法下注類型, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalBetType.Enum()
		} else if e, ok := err.(*ErrorIllegalBetNumber); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 非法下注號碼組合, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalNumbers.Enum()
		} else if e, ok := err.(*ErrorIllegalBetNumberCount); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 非法下注號碼數量, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeIllegalNumbers.Enum()
		} else if e, ok := err.(*ErrorNoBet); ok {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 無下注紀錄, %v", logic.TableNo(), user.UserID, e)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeInternalError.Enum()
		} else {
			mlog.Debug("牌桌[%d] 玩家[%d]下注 未知錯誤, %v", logic.TableNo(), user.UserID, err)
			returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeInternalError.Enum()
		}

		returnM.MsgData = returnData

		logic.SendMessageToUser(user, returnM)
		return
	}

	{ // 下注結果
		returnM := new(msg.Message)
		returnM.BClassID = int32(netproto.MessageBClassID_Game)
		returnM.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetClearRetID)

		returnData := new(netproto.Roulette_BetResponse)

		returnData.Code = netproto.Roulette_ResultCode_Roulette_ResultCodeSuccess.Enum()
		returnData.Money = proto.Int64(user.Score - u.BetSum()) // 預計算玩家的錢

		for _, bet := range u.Bets {
			b := new(netproto.Roulette_Bet)
			b.Type = TransforBetType(bet.Type).Enum()
			b.Money = proto.Int64(bet.Count)
			returnData.Bets = append(returnData.Bets, b)
		}

		returnM.MsgData = returnData

		logic.SendMessageToUser(user, returnM)
	}

	// 廣播玩家下注
	logic.BoardcastMessage(logic.mBoardcastBetSummary(user.UserID))

	// 更新牌桌資訊
	logic.EventChan <- logic.MakeDashboard()
}
