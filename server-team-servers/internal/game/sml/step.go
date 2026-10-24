package sml

type GameStep int

const (
	Prepare   GameStep = iota // 初始狀態
	Start                     // 遊戲開始表演動畫
	WaitBet                   // 等待下注
	ShowAward                 // 開獎動畫
	Finish                    // 遊戲結束狀態
)

func (step GameStep) String() string {
	switch step {
	default:
		return ""
	case Prepare:
		return "準備中"
	case Start:
		return "遊戲開始表演動畫中"
	case WaitBet:
		return "等待下注中"
	case ShowAward:
		return "開獎中"
	case Finish:
		return "遊戲結束"
	}
}
