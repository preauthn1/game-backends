package sml

// BetType 下注類型
type BetType uint32

const (
	Left   BetType = iota + 0x01 // 左
	Middle                       // 中
	Right                        // 右
)

const (
	Goal       BetType = iota + 0x10 // 進球
	GoalLeft                         // 進球 左
	GoalMiddle                       // 進球 中
	GoalRight                        // 進球 右
)
const (
	Save       BetType = iota + 0x20 // 守住
	SaveLeft                         // 守住 左
	SaveMiddle                       // 守住 中
	SaveRight                        // 守住 右
)

var AwardOptions = []BetType{GoalLeft, GoalMiddle, GoalRight, SaveLeft, SaveMiddle, SaveRight}
var BetAreas = []BetType{Goal, GoalLeft, GoalMiddle, GoalRight, Save, SaveLeft, SaveMiddle, SaveRight}

func (t BetType) String() string {
	switch t {
	default:
		return ""
	case Goal:
		return "進球"
	case GoalLeft:
		return "進球-左"
	case GoalMiddle:
		return "進球-中"
	case GoalRight:
		return "進球-右"
	case Save:
		return "守住"
	case SaveLeft:
		return "守住-左"
	case SaveMiddle:
		return "守住-中"
	case SaveRight:
		return "守住-右"
	}
}

// AllowBet 是否允許下注, 在 BetAreas 內的才允許下注
func (t BetType) AllowBet() bool {
	for _, v := range BetAreas {
		if t == v {
			return true
		}
	}
	return false
}

func (t BetType) Match(m BetType) bool {
	if t%16 == 0 {
		return t&m == t
	}
	return t == m
}

var DefaultBetTypes = []BetTypeConfig{
	{Type: Goal, Odd: 2.0, MaxBet: 10000},
	{Type: GoalLeft, Odd: 5.8, MaxBet: 5000},
	{Type: GoalMiddle, Odd: 5.8, MaxBet: 5000},
	{Type: GoalRight, Odd: 5.8, MaxBet: 5000},
	{Type: Save, Odd: 2.0, MaxBet: 10000},
	{Type: SaveLeft, Odd: 5.8, MaxBet: 5000},
	{Type: SaveMiddle, Odd: 5.8, MaxBet: 5000},
	{Type: SaveRight, Odd: 5.8, MaxBet: 5000},
}

var DefaultBetTypeMap = map[BetType]BetTypeConfig{
	Goal:       {Type: Goal, Odd: 1.95, MaxBet: 10000},
	GoalLeft:   {Type: GoalLeft, Odd: 5.8, MaxBet: 5000},
	GoalMiddle: {Type: GoalMiddle, Odd: 5.8, MaxBet: 5000},
	GoalRight:  {Type: GoalRight, Odd: 5.8, MaxBet: 5000},
	Save:       {Type: Save, Odd: 1.95, MaxBet: 10000},
	SaveLeft:   {Type: SaveLeft, Odd: 5.8, MaxBet: 5000},
	SaveMiddle: {Type: SaveMiddle, Odd: 5.8, MaxBet: 5000},
	SaveRight:  {Type: SaveRight, Odd: 5.8, MaxBet: 5000},
}

var DefaultBetAllowAmounts = []int64{100, 500, 1000, 2000, 5000, 10000}

var DefaultUserMaxBet int64 = 100000

// BetTypeConfig 下注類型設定
type BetTypeConfig struct {
	Type   BetType // 下注類型
	Odd    float64 // 賠率
	MaxBet int64   // 總下注上限
}

// BetConfig 下注設定
type BetConfig struct {
	Area         map[BetType]BetTypeConfig
	AllowAmounts map[int64]bool
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

func (cfg *BetConfig) Allow(n int64) bool {
	return cfg.AllowAmounts[n]
}
