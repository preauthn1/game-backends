package aladdin

import (
	"fmt"
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestLogic_Spin_WithRespin(t *testing.T) {

	conf.CLogLevel = 4
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	logic.SlotPatternID = uint32(1)
	logic.SlotPattern = &protodata.SlotPattern{Id: proto.Uint32(1), MainGameTableGroupId: proto.Uint32(1)}

	logic.MainGameTable = []*protodata.MainGameTable{
		&protodata.MainGameTable{GroupId: proto.Uint32(1), SymbolGroupId: proto.Uint32(1)},
	}

	logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},

		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},

		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},

		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
	}

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(1), PayType: proto.String("respin"), Payout: proto.String("1")},
		&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(2), PayType: proto.String("respin"), Payout: proto.String("2")},
		&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(3), PayType: proto.String("respin"), Payout: proto.String("3")},
		&protodata.SlotPayTable{Symbol: proto.String("p1"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p2"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p3"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p1"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p2"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p3"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
	}

	logic.RespinTable = []*protodata.RespinTable{
		&protodata.RespinTable{GroupId: proto.Int32(1), RespinCount: proto.Uint32(1), AppearReel: proto.String("2"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(2)},
		&protodata.RespinTable{GroupId: proto.Int32(2), RespinCount: proto.Uint32(2), AppearReel: proto.String("2,4"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(3)},
		&protodata.RespinTable{GroupId: proto.Int32(3), RespinCount: proto.Uint32(3), AppearReel: proto.String("2,3,4"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(4)},
	}

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	t.Run("Spin", func(tt *testing.T) {

		fmt.Println(logic.Bet(user, 10))

		logic.Spin(1, false, 1, nil)

		for _, row := range logic.StepResults {
			fmt.Println(row.Order)
		}
	})

}

func TestLogic_Bet(t *testing.T) {

	paylines := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
		&protodata.SlotPayLine{},
	}

	type fields struct {
		SlotLogicBase *baselogic.SlotLogicBase
		StepResults   []*StepResult
	}
	type args struct {
		user  *gamelogic.GameUser
		money int64
	}
	tests := []struct {
		name         string
		fields       fields
		args         args
		wantOk       bool
		wantRetcode  int32
		wantBetmoney int64
	}{
		{
			name: "Bet Success",
			fields: fields{
				SlotLogicBase: &baselogic.SlotLogicBase{
					MinCarryMoney: 5000,
					SlotPayLine:   paylines,
					GameState:     slot.GameStateBet,
					BetCounts:     []int32{100},
				},
				StepResults: nil,
			},
			args: args{
				user:  &gamelogic.GameUser{Score: 50000},
				money: 100,
			},
			wantOk:       true,
			wantRetcode:  slot.BetStateSuccess,
			wantBetmoney: 1000,
		},
		{
			name: "Status Not Bet",
			fields: fields{
				SlotLogicBase: &baselogic.SlotLogicBase{
					MinCarryMoney: 5000,
					SlotPayLine:   paylines,
					GameState:     slot.GameStateWait,
					BetCounts:     []int32{100},
				},
				StepResults: nil,
			},
			args: args{
				user:  &gamelogic.GameUser{Score: 50000},
				money: 100,
			},
			wantOk:       false,
			wantRetcode:  slot.BetStateStepError,
			wantBetmoney: 0,
		},
		{
			name: "User Money Not Enough For Bet",
			fields: fields{
				SlotLogicBase: &baselogic.SlotLogicBase{
					MinCarryMoney: 500,
					SlotPayLine:   paylines,
					GameState:     slot.GameStateBet,
					BetCounts:     []int32{100},
				},
				StepResults: nil,
			},
			args: args{
				user:  &gamelogic.GameUser{Score: 800},
				money: 100,
			},
			wantOk:       false,
			wantRetcode:  slot.BetStateMoneyNotEnough,
			wantBetmoney: 1000,
		},
		{
			name: "User Money Not Enough For Carry",
			fields: fields{
				SlotLogicBase: &baselogic.SlotLogicBase{
					MinCarryMoney: 5000,
					SlotPayLine:   paylines,
					GameState:     slot.GameStateBet,
					BetCounts:     []int32{100},
				},
				StepResults: nil,
			},
			args: args{
				user:  &gamelogic.GameUser{Score: 3000},
				money: 100,
			},
			wantOk:       false,
			wantRetcode:  slot.BetStateMoneyLessMin,
			wantBetmoney: 1000,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			logic := &Logic{
				SlotLogicBase: tt.fields.SlotLogicBase,
				StepResults:   tt.fields.StepResults,
			}
			gotOk, gotRetcode, gotBetmoney := logic.Bet(tt.args.user, tt.args.money)
			if gotOk != tt.wantOk {
				t.Errorf("Logic.Bet() gotOk = %v, want %v", gotOk, tt.wantOk)
			}
			if gotRetcode != tt.wantRetcode {
				t.Errorf("Logic.Bet() gotRetcode = %v, want %v", gotRetcode, tt.wantRetcode)
			}
			if gotBetmoney != tt.wantBetmoney {
				t.Errorf("Logic.Bet() gotBetmoney = %v, want %v", gotBetmoney, tt.wantBetmoney)
			}
		})
	}
}

func TestLogic_CalcGameResult(t *testing.T) {

	conf.CLogLevel = 4
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	t.Run("Spin", func(tt *testing.T) {

		for _, row := range logic.SlotPayLine {
			fmt.Println(row)
		}

		result := slot.GameResult{
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			},
		}

		manager, _ := slot.NewGameResultManager(5, 3)
		manager.GameResult = result

		step := NewStepResult(1, false, 6, 60, logic.SymbolAttributes, manager, nil)

		logic.CalcGameResult(step)

		fmt.Println(step.GamePayResult)
		step.ExpandResult.RowMode()
		fmt.Println(step.ExpandResult)

	})
}
