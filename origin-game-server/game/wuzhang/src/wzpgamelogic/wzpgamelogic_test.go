package wzpgamelogic

import (
	"conf"
	"config"
	"fmt"
	"gamelogic"
	"path"
	"poker"
	"runtime"
	"strconv"
	"strings"
	"testing"
)

func initConfig() {
	betamounts := "200,300,500,800,1500"
	gameconfig = new(wzpGameConfig)
	gameconfig.baseAmount = 100
	gameconfig.betTimeOut = 8
	betAmountArr := strings.Split(betamounts, ",")
	for _, ba := range betAmountArr {
		iba, _ := strconv.Atoi(ba)
		gameconfig.betAmount = append(gameconfig.betAmount, int32(iba))
	}

}

//隨機下注
func Test_getBetAmount(t *testing.T) {
	conf.CLogLevel = 99 //disable_log
	initConfig()
	lgc := new(WZPRobotLogic)
	betScale := []int32{0, 2, 2, 0, 0}
	chaitem := NewRobotChaItem(true, true, 0, betScale, false, 0, false)

	for i := 0; i < 100; i++ {
		val := lgc.getBetAmount(200, chaitem)
		if int32(val) != gameconfig.betAmount[1] && int32(val) != gameconfig.betAmount[2] {
			t.Error("Robot 隨機下注 failure") // 如果不是如预期的那么就报错
		}
	}
	t.Log("#1 隨機下注 success")
}

//隨機發捭, 梭哈
func TestFapai(t *testing.T) {
	var p poker.Poker
	user1 := gamelogic.NewGameUser()
	user1.UserID = 1
	allTableUsers := []*gamelogic.GameUser{user1}
	p = poker.NewRandomPoker(false, false, 1)
	//洗牌分牌
	p = poker.NewFixedPoker(allTableUsers, p.(*poker.RandomPoker), 5)
	fp := p.(*poker.FixedPoker)

	cards := fp.GetUserCards(1)
	t.Log("初始化後用戶將會拿到的牌", cards)
	origin := strings.Split(strings.TrimRight(cards.String(), ","), ",")

	foo := p.Deal(1, 2)
	t.Log("獲取兩張牌", foo)
	get2 := strings.Split(strings.TrimRight(foo.String(), ","), ",")

	cards = fp.GetUserCards(1)
	t.Log("用戶剩下的牌", cards)
	other := strings.Split(strings.TrimRight(cards.String(), ","), ",")

	for i, v := range get2 {
		if v != origin[i] {
			t.Errorf(fmt.Sprintf("比對原始牌與獲取的兩張有所不同, %v : 獲取:%v!=原始:%v", i, v, origin[i]))
		}
	}

	for i, v := range other {
		if v != origin[i+2] {
			t.Errorf("取兩張牌後, 比對原始牌與剩餘牌有所不同, %v, %v", origin, other)
		}
	}

	t.Log("#2 隨機發牌 success")
}

//固定發牌
func TestFapai2(t *testing.T) {
	_, filename, _, _ := runtime.Caller(0)

	//加载配置信息
	configDir := path.Dir(filename) + "/../../config"
	cfgs := new(config.ConfigGroup)
	cfgs.LoadConfig(configDir, 0)
	//t.Log(cfgs)
	// 需要三個 user
	var allTableUsers []*gamelogic.GameUser
	for i := 0; i < 3; i++ {
		user := gamelogic.NewGameUser()
		user.UserID = int32(i)
		allTableUsers = append(allTableUsers, user)
	}

	var p poker.Poker
	p = poker.NewFilePoker(cfgs, allTableUsers)
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	valid_cards := []string{"红桃A,红桃8,红桃Q,红桃J,红桃10,", "黑桃A,黑桃7,黑桃Q,黑桃J,黑桃10,", "梅花A,梅花6,梅花Q,梅花J,梅花10,"}
	for i := 0; i < 3; i++ {
		cards := fp.GetUserCards(int32(i))
		if cards.String() != valid_cards[i] {
			t.Errorf("固定發牌結果與設定檔有所不同, 實際發牌:%v, 發牌設定:%v", cards, valid_cards[i])
		}
		t.Log("user ", i, " ", cards)
	}

	t.Log("#3 固定發牌 success")
}

//同花大顺 1 同花顺 2 四条 3 葫芦 4 同花 5 顺子 6 三条 7 两对 8 一对 9 散牌 10 弃牌 11
func TestRandomFapai(t *testing.T) {
	var allTableUsers []*gamelogic.GameUser
	for i := 0; i < 3; i++ {
		user := gamelogic.NewGameUser()
		user.UserID = int32(i)
		allTableUsers = append(allTableUsers, user)
	}

	//結果統計
	result := make(map[int]int)
	var p poker.Poker
	for step := 0; step < 100; step++ {
		p = poker.NewRandomPoker(false, false, 1)
		//洗牌分牌
		p = poker.NewFixedPoker(allTableUsers, p.(*poker.RandomPoker), 5)

		fp := p.(*poker.FixedPoker)
		for i := 0; i < 3; i++ {
			cards := fp.GetUserCards(int32(i))
			size := getCardSize(cards)
			result[int(size)] += 1
		}
	}
	if len(result) < 5 {
		t.Errorf("隨機發牌產生牌型少於5種, %v", result)
	}
	t.Log("#4 隨機發牌統計牌型 success")
}
