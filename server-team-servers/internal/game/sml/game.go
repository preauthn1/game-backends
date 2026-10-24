package sml

import (
	"fmt"
	"math/rand"
	"sort"
	"sync"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

type Game struct {
	sync.RWMutex

	BetConfig     *BetConfig // 下注設定
	BetMinCarray  int64      // 下注時最小攜帶金額
	UserMaxBet    int64      // 玩家最大下注金額
	RecordMaxSize int        // 記錄保留筆數 Default: 30
	EnableHedge   bool       // 可否對押

	Users       map[int32]*User // 場內玩家
	Records     []Record        // 歷史紀錄
	AreaSummary map[BetType]int // 局數統計

	RoundID         int               // 第幾回合
	Step            GameStep          // 當前階段
	Next            []BetType         // 指定開獎 Queue
	BetSummary      map[BetType]int64 // 下注統計
	UserBetSummary  map[BetType]int64 // 玩家下注統計
	RobotBetSummary map[BetType]int64 // 機器人下注統計

	WaterLineChecker func() bool // 是否低於水線, 低於水線回傳 開最小獎機率(萬分比), true
}

func NewGame(opts ...GameOpt) (*Game, error) {

	game := &Game{
		BetConfig:        NewBetConfig(DefaultBetTypes...),
		RecordMaxSize:    30,
		UserMaxBet:       DefaultUserMaxBet,
		Users:            make(map[int32]*User),
		AreaSummary:      make(map[BetType]int),
		BetSummary:       make(map[BetType]int64),
		UserBetSummary:   make(map[BetType]int64),
		RobotBetSummary:  make(map[BetType]int64),
		WaterLineChecker: func() bool { return false },
	}

	for _, opt := range opts {
		if err := opt.Apply(game); err != nil {
			return nil, err
		}
	}

	return game, nil
}

func (game *Game) String() string {
	game.RLock()
	defer game.RUnlock()

	format := `[%v] 玩家人數[%d] 下注[%d]
		進球   [(Bet) %d][(Odd) %.3f]
		進球-左[(Bet) %d][(Odd) %.3f]
		進球-中[(Bet) %d][(Odd) %.3f]
		進球-右[(Bet) %d][(Odd) %.3f]

		守住   [(Bet) %d][(Odd) %.3f] 
		守住-左[(Bet) %d][(Odd) %.3f] 
		守住-中[(Bet) %d][(Odd) %.3f] 
		守住-右[(Bet) %d][(Odd) %.3f]`

	var bSum int64

	for _, n := range game.BetSummary {
		bSum += n
	}

	return fmt.Sprintf(format, game.Step, len(game.Users), bSum,
		game.BetSummary[Goal], game.BetConfig.Odd(Goal),
		game.BetSummary[GoalLeft], game.BetConfig.Odd(GoalLeft),
		game.BetSummary[GoalMiddle], game.BetConfig.Odd(GoalMiddle),
		game.BetSummary[GoalRight], game.BetConfig.Odd(GoalRight),

		game.BetSummary[Save], game.BetConfig.Odd(Save),
		game.BetSummary[SaveLeft], game.BetConfig.Odd(SaveLeft),
		game.BetSummary[SaveMiddle], game.BetConfig.Odd(SaveMiddle),
		game.BetSummary[SaveRight], game.BetConfig.Odd(SaveRight),
	)
}

func (game *Game) AddUser(gu *gamelogic.GameUser) *User {
	game.Lock()
	defer game.Unlock()

	if u, ok := game.Users[gu.UserID]; ok {
		return u
	}

	u := NewUser(gu)
	game.Users[gu.UserID] = u

	return u
}

func (game *Game) DeleteUser(uid int32) {
	game.Lock()
	defer game.Unlock()

	u := game.Users[uid]
	if u == nil {
		return
	}

	// 移除此人下注
	for t, n := range u.BetSummary {
		game.BetSummary[t] -= n
	}

	delete(game.Users, uid)
}

func (game *Game) Bet(uid int32, t BetType, n int64) error {

	// 檢查當前是否為下注狀態
	if game.Step != WaitBet {
		return &ErrorStepNotAllow{
			Action:   "Bet",
			Now:      game.Step,
			Excepted: WaitBet,
		}
	}

	// 檢查玩家是否在場內
	u := game.Users[uid]
	if u == nil {
		return &ErrorUserNotExist{UserID: uid}
	}

	// 檢查攜帶金額是否低於設定值
	if u.GameUser.Score-u.BetSum() < game.BetMinCarray {
		return &ErrorCarryMoneyNotEnough{Want: game.BetMinCarray, But: u.GameUser.Score}
	}

	// 檢查下注類型是否達到總金額上限
	if game.BetSummary[t]+n > game.BetConfig.MaxBet(t) {
		return &ErrorBetTypeOverLimit{
			Type:  t,
			Limit: game.BetConfig.MaxBet(t),
			Now:   game.BetSummary[t],
		}
	}

	// 檢查下注類型是否達到單人上限
	if u.BetSum()+n > game.UserMaxBet {
		return &ErrorBetTypeOverLimit{
			Type:  t,
			Limit: game.UserMaxBet,
			Now:   u.BetSum(),
		}
	}

	// 檢查下注面額是否合法
	if !game.BetConfig.Allow(n) {
		return &ErrorIllegalBetAmount{
			Amount: n,
		}
	}

	// 玩家下注
	if err := u.Bet(t, n, game.EnableHedge); err != nil {
		return err
	}

	game.Lock()
	defer game.Unlock()

	game.BetSummary[t] += n

	if !u.GameUser.IsRobot() {
		game.UserBetSummary[t] += n
	} else {
		game.RobotBetSummary[t] += n
	}

	return nil
}

func (game *Game) NewRound() {
	game.Lock()
	defer game.Unlock()

	// 如果超過紀錄場數，直接重置紀錄
	if len(game.Records) >= game.RecordMaxSize {
		game.RoundID = 0
		game.Records = []Record{}
		game.AreaSummary = make(map[BetType]int)
	}

	game.Step = Start
	game.RoundID++
	game.BetSummary = make(map[BetType]int64)
	game.UserBetSummary = make(map[BetType]int64)
	game.RobotBetSummary = make(map[BetType]int64)

	for _, u := range game.Users {
		u.Clear()
	}
}

func (game *Game) StartBet() {
	game.Lock()
	defer game.Unlock()

	game.Step = WaitBet
}

func (game *Game) Award() (BetType, error) {

	game.Lock()
	defer game.Unlock()

	game.Step = ShowAward

	var award BetType

	if len(game.Next) > 0 { // 指定開獎
		award = game.Next[0]
		game.Next = game.Next[1:]
	} else if game.WaterLineChecker() { // 當前水池低於水線，且開對系統有利獎
		// 預算所有區域結算金額
		type pre struct {
			t BetType
			w int64
		}

		var arr []pre
		for _, awd := range AwardOptions {
			o := pre{t: awd}
			for t, n := range game.UserBetSummary {
				if t.Match(awd) {
					o.w += int64(float64(n) * game.BetConfig.Odd(t))
				}
			}
			arr = append(arr, o)
		}
		sort.Slice(arr, func(i, j int) bool { return arr[i].w < arr[j].w })

		if len(arr) > 0 {
			var options []pre
			options = append(options, arr[0])
			arr = arr[1:]

			for _, a := range arr {
				if a.w == options[0].w {
					options = append(options, a)
				}
			}

			i := rand.Intn(len(options))
			award = options[i].t
		}
	}

	if award == 0 { // 還沒有決定好 純隨機
		i := rand.Intn(len(AwardOptions))
		award = AwardOptions[i]
	}

	// 判斷是 `進球` 還是 `守住`
	for _, u := range game.Users {
		u.Award(award, *game.BetConfig)
	}

	{ // 添加紀錄
		r := Record{
			RoundID:    game.RoundID,
			Award:      award,
			BetSummary: game.BetSummary,
		}

		game.Records = append(game.Records, r)
		over := len(game.Records) - game.RecordMaxSize
		if over > 0 {
			game.Records = game.Records[over:]
		}
	}

	// 增加統計資料
	game.AreaSummary[award]++

	return award, nil
}

func (game *Game) Finish() {
	game.Lock()
	defer game.Unlock()

	game.Step = Finish
}
