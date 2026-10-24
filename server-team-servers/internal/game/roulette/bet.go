package roulette

import (
	"sort"
)

// BetType 下注類型
type BetType uint32

const (
	Odd         BetType = iota + 1 // 單 1:1
	Even                           // 雙 1:1
	Red                            // 紅  1:1
	Black                          // 黑 1:1
	Small                          // 小 1-18 1:1
	Big                            // 大 19-36 1:1
	First12                        // 1-12 1:2
	Second12                       // 13-24 1:2
	Third12                        // 25-36 1:2
	FirstRow                       // 1-34 1-4-7-10-13-16-19-22-25-28-31-34 1:2
	SecondRow                      // 2-35 2-5-8-11-14-17-20-23-26-29-32-35 1:2
	ThirdRow                       // 3-36 3-6-9-12-15-18-21-24-27-30-33-36 1:2
	OneNumber                      // 單一號碼 1:35
	TwoNumber                      // 兩個號碼 1:17
	ThreeNumber                    // 三個號碼 1:11
	FourNumber                     // 四個號碼 1:8
	SixNumber                      // 六個號碼 1:5
	SuperZero                      // 超級0 1:47
)

var DefaultBetTypeMap = map[BetType]BetTypeConfig{
	Odd:         {Type: Odd, Odd: 1, MaxBet: 100000},
	Even:        {Type: Even, Odd: 1, MaxBet: 100000},
	Red:         {Type: Red, Odd: 1, MaxBet: 100000},
	Black:       {Type: Black, Odd: 1, MaxBet: 100000},
	Small:       {Type: Small, Odd: 1, MaxBet: 100000},
	Big:         {Type: Big, Odd: 1, MaxBet: 100000},
	First12:     {Type: First12, Odd: 2, MaxBet: 100000},
	Second12:    {Type: Second12, Odd: 2, MaxBet: 100000},
	Third12:     {Type: Third12, Odd: 2, MaxBet: 100000},
	FirstRow:    {Type: FirstRow, Odd: 2, MaxBet: 100000},
	SecondRow:   {Type: SecondRow, Odd: 2, MaxBet: 100000},
	ThirdRow:    {Type: ThirdRow, Odd: 2, MaxBet: 100000},
	OneNumber:   {Type: OneNumber, Odd: 35, MaxBet: 100000},
	TwoNumber:   {Type: TwoNumber, Odd: 17, MaxBet: 100000},
	ThreeNumber: {Type: ThreeNumber, Odd: 11, MaxBet: 100000},
	FourNumber:  {Type: FourNumber, Odd: 8, MaxBet: 100000},
	SixNumber:   {Type: SixNumber, Odd: 5, MaxBet: 100000},
	SuperZero:   {Type: SuperZero, Odd: 47, MaxBet: 100000},
}

var DefaultBetTypes []BetTypeConfig

var DefaultUserMaxBet int64 = 1000000

// 0 - 36
var AwardOptions = []uint32{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36}
var BetAreas = []BetType{Odd, Even, Red, Black, Small, Big, First12, Second12, Third12, FirstRow, SecondRow, ThirdRow, OneNumber, TwoNumber, ThreeNumber, FourNumber, SixNumber, SuperZero}

func init() {
	for _, v := range DefaultBetTypeMap {
		DefaultBetTypes = append(DefaultBetTypes, v)
	}
	sort.Slice(DefaultBetTypes, func(i, j int) bool {
		return DefaultBetTypes[i].Type < DefaultBetTypes[j].Type
	})
}

func (t BetType) String() string {
	switch t {
	default:
		return ""
	case Odd:
		return "單"
	case Even:
		return "雙"
	case Red:
		return "紅"
	case Black:
		return "黑"
	case Small:
		return "小"
	case Big:
		return "大"
	case First12:
		return "1ST12"
	case Second12:
		return "2ND12"
	case Third12:
		return "3RD12"
	case FirstRow:
		return "1ST ROW"
	case SecondRow:
		return "2ND ROW"
	case ThirdRow:
		return "3RD ROW"
	case OneNumber:
		return "單一號碼"
	case TwoNumber:
		return "兩個號碼"
	case ThreeNumber:
		return "三個號碼"
	case FourNumber:
		return "四個號碼"
	case SixNumber:
		return "六個號碼"
	case SuperZero:
		return "超級0"
	}
}

func (t BetType) Match(m BetType) bool {
	if t%16 == 0 {
		return t&m == t
	}
	return t == m
}

// Bet 下注
type Bet struct {
	Type    BetType  // 下注類型
	Numbers []uint32 // 下注號碼 1-36
	Count   int64    // 下注數量
	Win     int64    // 中獎數量
	Rtn     int64    // 返還數量
}

// Valid 檢查下注是否合法
// 盤面配置如下
// |   | 3 | 6 | 9 | 12 | 15 | 18 | 21 | 24 | 27 | 30 | 33 | 36 | 3RD ROW |
// | 0 | 2 | 5 | 8 | 11 | 14 | 17 | 20 | 23 | 26 | 29 | 32 | 35 | 2ND ROW |
// |   | 1 | 4 | 7 | 10 | 13 | 16 | 19 | 22 | 25 | 28 | 31 | 34 | 1ST ROW |
// |   |     1ST12      |      2ND12        |       3RD12       |         |
// |   |  1-18  | EVEN  |   RED   |  BLACK  |   ODD   |  19-36  |         |
func (b Bet) Valid() error {

	// 先檢查下注號碼是否合法
	for _, n := range b.Numbers {
		if n > 36 {
			return ErrorIllegalBetNumber{Number: n}
		}
	}

	// 排序 b.Number
	sort.Slice(b.Numbers, func(i, j int) bool {
		return b.Numbers[i] < b.Numbers[j]
	})

	switch b.Type {
	default:
		return &ErrorIllegalBetType{Type: b.Type}
	case Odd, Even, Red, Black, Small, Big, SuperZero:
		if len(b.Numbers) != 0 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 0}
		}
	case First12, Second12, Third12, FirstRow, SecondRow, ThirdRow:
		if len(b.Numbers) != 0 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 0}
		}
	case OneNumber:
		if len(b.Numbers) != 1 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 1}
		}
		if b.Numbers[0] == 0 {
			return &ErrorIllegalBetType{Type: b.Type}
		}
	case TwoNumber:
		if len(b.Numbers) != 2 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 2}
		}
		// 特例 0-1 0-2 0-3
		if b.Numbers[0] == 0 && b.Numbers[1] == 1 {
			return nil
		}
		if b.Numbers[0] == 0 && b.Numbers[1] == 2 {
			return nil
		}
		if b.Numbers[0] == 0 && b.Numbers[1] == 3 {
			return nil
		}
		// 相差 3 給過
		if b.Numbers[0]+3 == b.Numbers[1] {
			return nil
		}
		// 如果不連續, 又不是相差 3 就不給過
		if b.Numbers[0]+1 != b.Numbers[1] {
			return &ErrorIllegalBetNumber{Number: 0}
		}
		// 如果是連續, 必須是同一列, 例如 1-2 2-3 4-5 5-6
		if b.Numbers[0]%3 != 1 && b.Numbers[1]%3 != 0 {
			return &ErrorIllegalBetNumber{Number: 0}
		}
	case ThreeNumber:
		if len(b.Numbers) != 3 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 3}
		}
		// 特例 0-1-2 0-2-3
		if b.Numbers[0] == 0 && b.Numbers[1] == 1 && b.Numbers[2] == 2 {
			return nil
		}
		if b.Numbers[0] == 0 && b.Numbers[1] == 2 && b.Numbers[2] == 3 {
			return nil
		}
		// 三個數字必須連續
		if b.Numbers[0]+1 != b.Numbers[1] || b.Numbers[1]+1 != b.Numbers[2] {
			return &ErrorIllegalBetNumber{Number: 0}
		}
		// 三個數字必須在同一列
		if b.Numbers[0]%3 != 1 || b.Numbers[1]%3 != 2 || b.Numbers[2]%3 != 0 {
			return &ErrorIllegalBetNumber{Number: 0}
		}
	case FourNumber:
		if len(b.Numbers) != 4 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 4}
		}
		// 1 2 位置的數字必須差 1, 3 4 位置的數字必須差 1
		// 1 3 位置的數字必須差 3, 2 4 位置的數字必須差 3
		if b.Numbers[0]+1 != b.Numbers[1] || b.Numbers[2]+1 != b.Numbers[3] {
			return &ErrorIllegalBetNumber{Number: 0}
		}
		if b.Numbers[0]+3 != b.Numbers[2] || b.Numbers[1]+3 != b.Numbers[3] {
			return &ErrorIllegalBetNumber{Number: 0}
		}
	case SixNumber:
		if len(b.Numbers) != 6 {
			return &ErrorIllegalBetNumberCount{Count: len(b.Numbers), Want: 6}
		}
		// 六個數字必須連續
		for i := 0; i < 5; i++ {
			if b.Numbers[i]+1 != b.Numbers[i+1] {
				return &ErrorIllegalBetNumber{Number: 0}
			}
		}
		// 前三個數字必須在同一列
		if b.Numbers[0]%3 != 1 || b.Numbers[1]%3 != 2 || b.Numbers[2]%3 != 0 {
			return &ErrorIllegalBetNumber{Number: 0}
		}
		// 後三個數字必須在同一列
		if b.Numbers[3]%3 != 1 || b.Numbers[4]%3 != 2 || b.Numbers[5]%3 != 0 {
			return &ErrorIllegalBetNumber{Number: 0}
		}
	}

	return nil
}

func (b Bet) Match(n uint32) bool {
	switch b.Type {
	default:
		return false
	case Odd:
		if n == 0 {
			return false
		}
		return n%2 == 1
	case Even:
		if n == 0 {
			return false
		}
		return n%2 == 0
	case Red:
		accept := []uint32{1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36}
		for _, a := range accept {
			if a == n {
				return true
			}
		}
		return false
	case Black:
		accept := []uint32{2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35}
		for _, a := range accept {
			if a == n {
				return true
			}
		}
		return false
	case Small:
		return n >= 1 && n <= 18
	case Big:
		return n >= 19 && n <= 36
	case First12:
		return n >= 1 && n <= 12
	case Second12:
		return n >= 13 && n <= 24
	case Third12:
		return n >= 25 && n <= 36
	case FirstRow:
		return n%3 == 1
	case SecondRow:
		return n%3 == 2
	case ThirdRow:
		if n == 0 {
			return false
		}
		return n%3 == 0
	case OneNumber, TwoNumber, ThreeNumber, FourNumber, SixNumber:
		for _, a := range b.Numbers {
			if a == n {
				return true
			}
		}
		return false
	case SuperZero:
		return n == 0
	}
}

func (b *Bet) Payout(conf BetConfig, dryRun bool) (rtn int64, win int64) {
	odd := conf.Odd(b.Type)

	win = int64(float64(b.Count) * odd)

	if !dryRun {
		b.Win = win
		b.Rtn = b.Count
	}

	return b.Count, win
}

// BetTypeConfig 下注類型設定
type BetTypeConfig struct {
	Type   BetType // 下注類型
	Odd    float64 // 賠率
	MaxBet int64   // 下注上限，2024-02-29 改為 `個人` 的 `同區域` `同號碼`` 上限
}

// BetConfig 下注設定
type BetConfig struct {
	Area         map[BetType]BetTypeConfig // 下注區域
	AllowAmounts map[int64]bool            // 允許的下注金額(籌碼)
}

func NewBetConfig(confs ...BetTypeConfig) *BetConfig {
	cfg := &BetConfig{
		Area:         make(map[BetType]BetTypeConfig),
		AllowAmounts: make(map[int64]bool),
	}
	cfg.AddBetType(confs...)
	return cfg
}

func (cfg *BetConfig) AddBetType(confs ...BetTypeConfig) {
	for _, c := range confs {
		cfg.Area[c.Type] = c
	}
}

func (cfg *BetConfig) AddAllowAmounts(amounts ...int64) {
	for _, n := range amounts {
		cfg.AllowAmounts[n] = true
	}
}

func (cfg *BetConfig) Odd(t BetType) float64 {
	if c, ok := cfg.Area[t]; ok {
		return c.Odd
	}
	return 0
}

func (cfg *BetConfig) MaxBet(t BetType) int64 {
	if c, ok := cfg.Area[t]; ok {
		return c.MaxBet
	}
	return 0
}

// 只要金額可以被允許的籌碼完整湊成, 就可以下注
// 例如允許的籌碼有 5 10 50 100
// 金額 165 可以湊成 100 50 10 5 這樣就可以下注
func (cfg *BetConfig) Allow(n int64) bool {

	var arr []int64

	for k := range cfg.AllowAmounts {
		arr = append(arr, k)
	}

	// 降冪排序
	sort.Slice(arr, func(i, j int) bool { return arr[i] > arr[j] })

	// 依序減去最大的籌碼
	for _, a := range arr {
		for n >= a {
			n -= a
		}
	}

	// 如果最後 n 為 0, 表示可以湊成
	return n == 0
}
