package dfdc

import (
	"math/rand"
	"strconv"
	"strings"
	"time"
)

// JackpotGameDetail 翻牌紀錄
type JackpotGameDetail struct {
	ID    int32 // 順序 1-9
	Place int32 // 位置
	Icon  int32 // 符號(Level) 1: Grand, 2: Major, 3: Minor, 4: Mini
}

// JackpotGame 翻牌遊戲
type JackpotGame struct {
	Jackpots []*Jackpot // 當前所有獎池

	MaxBetRate  int32 // 最大黃金倍率
	MaxBetMoney int32 // 最大押注數量

	GameBetRate  int32 // 下注符號倍率
	GameBetMoney int64 // 下注金額

	WinJackpot *Jackpot // 中獎的Jackpot
	WinMoney   int64    // 中獎的錢

	Process map[int32]*JackpotGameDetail
}

// NewJackpotGame 建立翻牌遊戲
func NewJackpotGame(jackpots []*Jackpot, maxRate, maxMoney, rate int32, money int64) *JackpotGame {
	return &JackpotGame{
		Jackpots:     jackpots,
		MaxBetRate:   maxRate,
		MaxBetMoney:  maxMoney,
		GameBetRate:  rate,
		GameBetMoney: money,
	}
}

// RandomJackpot 決定中了哪個Jackpot
func (game *JackpotGame) RandomJackpot() *Jackpot {

	var total int
	var split []int // 分水嶺

	for _, jackpot := range game.Jackpots {
		total += int(jackpot.origin.GetPerProb())
		split = append(split, total)
	}

	if total == 0 {
		return nil
	}

	// 隨機產生該總重量
	rand.Seed(time.Now().UnixNano())
	random := rand.Intn(total)

	// 找出是哪個
	for i, row := range split {
		if random < row {
			return game.Jackpots[i]
		}
	}

	return nil
}

// GenerateProcess 產生翻牌歷程
func (game *JackpotGame) GenerateProcess() map[int32]*JackpotGameDetail {

	process := make(map[int32]*JackpotGameDetail)

	rand.Seed(time.Now().UnixNano())

	if game.WinJackpot == nil {
		return process
	}

	// 檢視所有Jackpot, 知道要開哪些符號
	levels := make([]JackpotLevel, 0, len(game.Jackpots))
	for _, jackpot := range game.Jackpots {
		levels = append(levels, jackpot.Level)
	}
	levels = append(levels, levels...)

	// 洗亂
	for i, l := 0, len(levels); i < l; i++ {
		j := rand.Intn(l - 1)
		levels[i], levels[j] = levels[j], levels[i]
	}

	// 隨機長度 3-9, 最少第3張就中獎, 最多第9張中獎
	length := rand.Intn(6) + 3
	procLevels := make([]JackpotLevel, 0, length)
	for i := 0; i < length; i++ {
		procLevels = append(procLevels, levels[i])
	}

	// 最後補中獎的level
	cnt := 0
	for _, level := range procLevels {
		if level == game.WinJackpot.Level {
			cnt++
		}
	}

	for i := 3; i > cnt; i-- {
		procLevels = append(procLevels, game.WinJackpot.Level)
	}

	// 組合
	for idx, level := range procLevels {
		detail := &JackpotGameDetail{
			ID:    int32(idx) + 1,
			Place: 0,
			Icon:  level.Int(),
		}

		process[detail.ID] = detail
	}

	return process
}

// Play 直接選出一個Jackpot
func (game *JackpotGame) Play() {

	game.WinJackpot = game.RandomJackpot()
	game.Process = game.GenerateProcess()

	currentBet := int64(game.GameBetRate) * game.GameBetMoney
	maxBet := int64(game.MaxBetRate) * int64(game.MaxBetMoney)

	game.WinMoney = game.WinJackpot.GetCurrentPoolByBet(currentBet, maxBet)
}

// Choose 玩家選擇，更新Proccess
func (game *JackpotGame) Choose(id, place, icon int32) {

	detail := &JackpotGameDetail{
		ID:    id,
		Place: place,
		Icon:  icon,
	}

	game.Process[id] = detail
}

// IsFinished 判斷JackpotGame是否結束, 每個Process位置都不是0則結束
func (game *JackpotGame) IsFinished() bool {

	if game.Process == nil {
		return false
	}

	for _, detail := range game.Process {
		if detail.Place == 0 {
			return false
		}
	}

	return true
}

// GetProcessData 取得翻牌歷程紀錄
func (game *JackpotGame) GetProcessData() string {

	strs := make([]string, 0, len(game.Process))

	for i := 0; i < len(game.Process); i++ {
		if detail, ok := game.Process[int32(i+1)]; ok {
			strs = append(strs, strconv.Itoa(int(detail.Icon)))
		}
	}

	return strings.Join(strs, ",")
}
