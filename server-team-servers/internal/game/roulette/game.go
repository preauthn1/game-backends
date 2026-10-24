package roulette

import (
	"log"
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

	Users       map[int32]*User // 場內玩家
	Records     []Record        // 歷史紀錄
	AreaSummary map[uint32]int  // 局數統計

	RoundID int      // 第幾回合
	Step    GameStep // 當前階段
	Next    []uint32 // 指定開獎 Queue

	WaterLineChecker func() bool // 是否低於水線, 低於水線回傳 開最小獎機率(萬分比), true
}

func NewGame(opts ...GameOpt) (*Game, error) {

	game := &Game{
		BetConfig:        NewBetConfig(DefaultBetTypes...),
		RecordMaxSize:    30,
		UserMaxBet:       DefaultUserMaxBet,
		Users:            make(map[int32]*User),
		AreaSummary:      make(map[uint32]int),
		WaterLineChecker: func() bool { return false },
	}

	for _, opt := range opts {
		if err := opt.Apply(game); err != nil {
			return nil, err
		}
	}

	return game, nil
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

	delete(game.Users, uid)
}

func (game *Game) Bet(uid int32, b Bet) error {

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

	// 檢查下注區域是否合法
	if err := b.Valid(); err != nil {
		return err
	}

	// 檢查下注類型是否達到總金額上限
	sum := u.BetTypeSum(b)
	if sum+b.Count > game.BetConfig.MaxBet(b.Type) {
		return &ErrorBetTypeOverLimit{
			Type:    b.Type,
			Numbers: b.Numbers,
			Limit:   game.BetConfig.MaxBet(b.Type),
			Now:     sum,
		}
	}

	// 檢查下注類型是否達到單人上限
	if u.BetSum()+b.Count > game.UserMaxBet {
		return &ErrorBetTypeOverPersionalLimit{
			Type:  b.Type,
			Limit: game.UserMaxBet,
			Now:   u.BetSum(),
		}
	}

	// 檢查下注面額是否合法
	if !game.BetConfig.Allow(b.Count) {
		return &ErrorIllegalBetAmount{
			Amount: b.Count,
		}
	}

	// 玩家下注
	if err := u.Bet(b); err != nil {
		return err
	}

	game.Lock()
	defer game.Unlock()

	return nil
}

func (game *Game) Undo(uid int32) error {

	// 檢查當前是否為下注狀態
	if game.Step != WaitBet {
		return &ErrorStepNotAllow{
			Action:   "Undo",
			Now:      game.Step,
			Excepted: WaitBet,
		}
	}

	// 檢查玩家是否在場內
	u := game.Users[uid]
	if u == nil {
		return &ErrorUserNotExist{UserID: uid}
	}

	// 玩家下注
	if err := u.Undo(); err != nil {
		return err
	}

	return nil
}

func (game *Game) BetTypeSum(t BetType) int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		for _, b := range u.Bets {
			if b.Type == t {
				sum += b.Count
			}
		}
	}

	return sum
}

func (game *Game) UserCount() int64 {
	game.RLock()
	defer game.RUnlock()

	var count int64

	for _, u := range game.Users {
		if !u.GameUser.IsRobot() {
			count++
		}
	}

	return count
}

func (game *Game) UserBetSum() int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if !u.GameUser.IsRobot() {
			sum += u.BetSum()
		}
	}

	return sum
}

func (game *Game) UserWinSum() int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if !u.GameUser.IsRobot() {
			sum += u.WinSum()
		}
	}

	return sum
}

func (game *Game) UserRtnSum() int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if !u.GameUser.IsRobot() {
			sum += u.RtnSum()
		}
	}

	return sum
}

func (game *Game) RobotCount() int64 {
	game.RLock()
	defer game.RUnlock()

	var count int64

	for _, u := range game.Users {
		if u.GameUser.IsRobot() {
			count++
		}
	}

	return count
}

func (game *Game) RobotBetSum() int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if u.GameUser.IsRobot() {
			sum += u.BetSum()
		}
	}

	return sum
}

func (game *Game) RobotWinSum() int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if u.GameUser.IsRobot() {
			sum += u.WinSum()
		}
	}

	return sum
}

func (game *Game) UserBetTypeSum(t BetType) int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if !u.GameUser.IsRobot() {
			for _, b := range u.Bets {
				if b.Type == t {
					sum += b.Count
				}
			}
		}
	}

	return sum
}

func (game *Game) UserWinTypeSum(t BetType) int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if !u.GameUser.IsRobot() {
			for _, b := range u.Bets {
				if b.Type == t {
					sum += b.Win
				}
			}
		}
	}

	return sum
}

func (game *Game) RobotBetTypeSum(t BetType) int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if u.GameUser.IsRobot() {
			for _, b := range u.Bets {
				if b.Type == t {
					sum += b.Count
				}
			}
		}
	}

	return sum
}

func (game *Game) RobotWinTypeSum(t BetType) int64 {
	game.RLock()
	defer game.RUnlock()

	var sum int64

	for _, u := range game.Users {
		if u.GameUser.IsRobot() {
			for _, b := range u.Bets {
				if b.Type == t {
					sum += b.Win
				}
			}
		}
	}

	return sum
}

func (game *Game) NewRound() {
	game.Lock()
	defer game.Unlock()

	// 如果超過紀錄場數，直接重置紀錄
	if len(game.Records) >= game.RecordMaxSize {
		game.RoundID = 0
		game.Records = []Record{}
		game.AreaSummary = make(map[uint32]int)
	}

	game.Step = Start
	game.RoundID++

	for _, u := range game.Users {
		u.Clear()
	}
}

func (game *Game) StartBet() {
	game.Lock()
	defer game.Unlock()

	game.Step = WaitBet
}

func (game *Game) Award() (uint32, error) {

	game.Lock()
	defer game.Unlock()

	game.Step = ShowAward

	var award uint32

	if len(game.Next) > 0 { // 指定開獎
		award = game.Next[0]
		game.Next = game.Next[1:]
	} else if game.WaterLineChecker() { // 當前水池低於水線，且開對系統有利獎
		type Report struct {
			Award uint32
			Win   int64
		}

		// 計算所有號碼的輸贏金額
		var (
			winList  []Report
			winUList []Report
			winRList []Report
		)

		type CalcRole uint32
		const (
			CalcAll CalcRole = iota
			CalcUser
			CalcRobot
		)

		// n 為開獎號碼
		// t 統計類型 0:
		calc := func(n uint32, t CalcRole) int64 {

			var win int64

			for _, u := range game.Users {
				for _, b := range u.Bets {
					if u.GameUser.IsRobot() && t == CalcUser {
						continue
					}
					if !u.GameUser.IsRobot() && t == CalcRobot {
						continue
					}
					if b.Match(n) {
						_, w := b.Payout(*game.BetConfig, true)
						win += w
					}
				}
			}

			return win
		}

		for _, n := range AwardOptions {
			if n == 0 {
				continue
			}
			{
				w := calc(n, CalcAll)
				winList = append(winList, Report{Award: n, Win: w})
			}
			{
				w := calc(n, CalcUser)
				winUList = append(winUList, Report{Award: n, Win: w})
			}
			{
				w := calc(n, CalcRobot)
				winRList = append(winRList, Report{Award: n, Win: w})
			}
		}

		// 找出所有輸最少的
		sort.Slice(winList, func(i, j int) bool { return winList[i].Win < winList[j].Win })
		sort.Slice(winUList, func(i, j int) bool { return winUList[i].Win < winUList[j].Win })
		sort.Slice(winRList, func(i, j int) bool { return winRList[i].Win < winRList[j].Win })

		var (
			selected  []Report
			selectedU []Report
			selectedR []Report
		)

		// 找出最少的
		for _, r := range winList {
			if r.Win == winList[0].Win {
				selected = append(selected, r)
			}
		}
		for _, r := range winUList {
			if r.Win == winUList[0].Win {
				selectedU = append(selectedU, r)
			}
		}
		for _, r := range winRList {
			if r.Win == winRList[0].Win {
				selectedR = append(selectedR, r)
			}
		}

		// log.Printf("開出系統有利獎 選項: %v", selected)
		log.Printf("開出系統有利獎 選項: %v", selectedU)
		// log.Printf("開出系統有利獎 選項: %v", selectedR)

		// 隨機選一個
		// i := rand.Intn(len(selected))
		j := rand.Intn(len(selectedU))
		// k := rand.Intn(len(selectedR))

		// award = selected[i].Award
		award = selectedU[j].Award
		// award = selectedR[k].Award
	} else { // 隨機開獎
		award = AwardOptions[rand.Intn(len(AwardOptions))]
	}

	// 計算獎金
	for _, u := range game.Users {
		u.Award(award, *game.BetConfig)
	}

	var bets []*Bet
	for _, u := range game.Users {
		bets = append(bets, u.Bets...)
	}

	{ // 添加紀錄
		r := Record{
			RoundID: game.RoundID,
			Award:   award,
			Bets:    bets,
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
