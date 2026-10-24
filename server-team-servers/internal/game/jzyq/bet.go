package jzyq

// BetType 下注類型
type BetType uint32

const (
	Inside  BetType = iota + 0x01 // 線內
	Outside                       // 線外
)

const (
	Goal        BetType = iota + 0x10 // 得分
	GoalInside                        // 得分-線內
	GoalOutside                       // 得分-線外
)
const (
	Lost        BetType = iota + 0x20 // 失分
	LostInside                        // 失分-線內
	LostOutside                       // 失分-線外
)

var AwardOptions = []BetType{GoalInside, GoalOutside, LostInside, LostOutside}
var BetAreas = []BetType{Goal, Lost, GoalInside, GoalOutside, LostInside, LostOutside}

func (t BetType) String() string {
	switch t {
	default:
		return ""
	case Goal:
		return "進球"
	case GoalInside:
		return "進球-線內"
	case GoalOutside:
		return "進球-線外"
	case Lost:
		return "失球"
	case LostInside:
		return "失球-線內"
	case LostOutside:
		return "失球-線外"
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
	{Type: GoalInside, Odd: 3.8, MaxBet: 5000},
	{Type: GoalOutside, Odd: 3.8, MaxBet: 5000},
	{Type: Lost, Odd: 2.0, MaxBet: 10000},
	{Type: LostInside, Odd: 3.8, MaxBet: 5000},
	{Type: LostOutside, Odd: 3.8, MaxBet: 5000},
}

var DefaultBetTypeMap = map[BetType]BetTypeConfig{
	Goal:        {Type: Goal, Odd: 2, MaxBet: 10000},
	GoalInside:  {Type: GoalInside, Odd: 3.8, MaxBet: 5000},
	GoalOutside: {Type: GoalOutside, Odd: 3.8, MaxBet: 5000},
	Lost:        {Type: Lost, Odd: 2, MaxBet: 10000},
	LostInside:  {Type: LostInside, Odd: 3.8, MaxBet: 5000},
	LostOutside: {Type: LostOutside, Odd: 3.8, MaxBet: 5000},
}

var DefaultBetAllowAmounts = []int64{100, 500, 1000, 2000, 5000, 10000}

var DefaultUserMaxBet int64 = 100000

// BetTypeConfig 下注類型設定
type BetTypeConfig struct {
	Type   BetType // 下注類型
	Odd    float64 // 賠率
	MaxBet int64   // 下注上限
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
