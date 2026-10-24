package candyhouse

import (
	"fmt"
	"math/rand"
	"strconv"
	"strings"

	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
)

// BonusGameDetail 翻牌紀錄
type BonusGameDetail struct {
	origin   *protodata.BonusTable
	ID       int32 // 順序 1-9
	Location int32 // 位置
	Money    int64 // 獎勵的錢
}

// BonusGame 翻牌遊戲
type BonusGame struct {
	BonusTable []*protodata.BonusTable
	Size       int32 // 共有幾個選項

	GameCount    int32 // 遊戲次數
	GameBetMoney int64 // 下注金額
	WinMoney     int64 // 中獎的錢

	Process map[int32]*BonusGameDetail
	All     []int64 // 所有翻牌紀錄，把所有12個選項都列出
}

// NewBonusGame 建立翻牌遊戲
func NewBonusGame(BonusTable []*protodata.BonusTable, size, count int32, betMoney int64) *BonusGame {

	game := new(BonusGame)
	game.BonusTable = BonusTable
	game.GameCount = count
	game.GameBetMoney = betMoney
	game.Process = game.generateProcess()
	game.Size = size
	game.All = game.generateLaftProcess(size)

	return game
}

// RandomBonus 決定中了哪個Bonus
func (game *BonusGame) randomBonus() *protodata.BonusTable {

	var total int
	var split []int // 分水嶺

	for _, bonus := range game.BonusTable {
		total += int(bonus.GetPerProb())
		split = append(split, total)
	}

	if total == 0 {
		return nil
	}

	// 隨機產生該總重量
	// rand.Seed(time.Now().UnixNano())
	random := rand.Intn(total)

	// 找出是哪個
	for i, row := range split {
		if random < row {
			return game.BonusTable[i]
		}
	}

	return nil
}

// GenerateProcess 產生翻牌歷程
func (game *BonusGame) generateProcess() map[int32]*BonusGameDetail {

	process := make(map[int32]*BonusGameDetail)

	for i := int32(0); i < game.GameCount; i++ {
		detail := new(BonusGameDetail)
		detail.ID = i + 1
		detail.origin = game.randomBonus()

		switch slot.PayType(detail.origin.GetRewardType()) {
		case slot.PayTypeChip:
			f, _ := strconv.ParseFloat(detail.origin.GetRewardValue(), 64)

			detail.Money = int64(f) * game.GameBetMoney
		}

		game.WinMoney += detail.Money
		process[detail.ID] = detail
	}

	return process
}

// GenerateLaftProcess 產生所有牌局選項
func (game *BonusGame) generateLaftProcess(size int32) []int64 {
	ans := make([]int64, 0, game.Size)

	// 計算剩餘數量
	for _, row := range game.Process {
		ans = append(ans, row.Money)
		size--
	}

	// 把剩餘的數量產出
	for i := 0; i < int(size); i++ {
		bonus := game.randomBonus()
		f, _ := strconv.ParseFloat(bonus.GetRewardValue(), 64)
		ans = append(ans, int64(f)*game.GameBetMoney)
	}

	return ans
}

// Choose 玩家選擇，更新Proccess
func (game *BonusGame) Choose(id, location int32) (*BonusGameDetail, error) {

	// 檢查是否結束
	if game.IsFinished() {
		return nil, fmt.Errorf("BonusGame is over")
	}

	// 檢查選擇次數是否正常
	if id <= 0 {
		return nil, fmt.Errorf("Choose ID should be [1-9]")
	}

	// 檢查前面次數是否有為選擇
	if id > 1 && game.Process[id-1].Location == 0 {
		return nil, fmt.Errorf("Choose ID: %d, Location: %d, But Record[%d] has not choosed", id, location, id-1)
	}

	if !game.Exist(id) {
		return nil, fmt.Errorf("Choose ID %d is not exist", id)
	}

	if game.Process[id].Location != 0 {
		return nil, fmt.Errorf("Choose ID %d has been choosed", id)
	}

	game.Process[id].Location = location

	return game.Process[id], nil
}

// Exist 判斷該ID存不存在
func (game *BonusGame) Exist(id int32) bool {
	if _, ok := game.Process[id]; ok {
		return true
	}
	return false
}

// IsFinished 判斷JackpotGame是否結束, 每個Process位置都不是0則結束
func (game *BonusGame) IsFinished() bool {

	if game.Process == nil {
		return false
	}

	for _, detail := range game.Process {
		if detail.Location == 0 {
			return false
		}
	}

	return true
}

// GetProcessData 取得翻牌歷程紀錄
func (game *BonusGame) GetProcessData() string {

	strs := make([]string, 0, len(game.Process))

	for i := 0; i < len(game.Process); i++ {
		if detail, ok := game.Process[int32(i+1)]; ok {
			strs = append(strs, fmt.Sprintf("%d:%d:%d", detail.ID, detail.Location, detail.Money))
		}
	}

	return strings.Join(strs, ",")
}
