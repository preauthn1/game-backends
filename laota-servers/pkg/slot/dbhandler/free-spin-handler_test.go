package dbhandler

import (
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

func TestFreeSpinHander_IsHook(t *testing.T) {
	type args struct {
		bclsid int32
		sclsid int32
	}
	tests := []struct {
		name string
		h    *FreeSpinHander
		args args
		want bool
	}{
		{
			name: "Hooked",
			h:    &FreeSpinHander{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID),
			},
			want: true,
		},
		{
			name: "Hooked",
			h:    &FreeSpinHander{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID),
			},
			want: true,
		},
		{
			name: "Not Hooked, Wrong BClassID",
			h:    &FreeSpinHander{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Hall),
				sclsid: int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID),
			},
			want: false,
		},
		{
			name: "Not Hooked, Wrong SClassID",
			h:    &FreeSpinHander{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.DBServerClassID_BlockIPID),
			},
			want: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			h := &FreeSpinHander{}
			if got := h.IsHook(tt.args.bclsid, tt.args.sclsid); got != tt.want {
				t.Errorf("FreeSpinHander.IsHook() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestFreeSpinHander_OnMessage_Read(t *testing.T) {

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(9936738),
	}

	msgData, _ := proto.Marshal(&netproto.Slot_MSG_FreeGameRead{
		UserID: proto.Int32(1),
		GameID: proto.Int32(2),
		FlagID: proto.Int32(3),
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID),
		MsgData:  rpcMessage,
	}

	handler := new(FreeSpinHander)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

	if ret[0].BClassID != int32(netproto.MessageBClassID_Game) {
		t.Errorf("Message BClassID want %d, but %d", int32(netproto.MessageBClassID_Game), ret[0].BClassID)
	}

	if ret[0].SClassID != int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID) {
		t.Errorf("Message SClassID want %d, but %d", int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID), ret[0].SClassID)
	}

	msg, ok := ret[0].MsgData.(*netproto.Slot_MSG_FreeGameRecord)

	if !ok {
		t.Errorf("Return Message is not *netproto.Slot_MSG_FreeGameRecord")
	}

	t.Log(msg)
}

func TestFreeSpinHander_OnMessage_Record(t *testing.T) {

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(1),
	}

	msgData, _ := proto.Marshal(&netproto.Slot_MSG_FreeGameRecord{
		Info: &netproto.Slot_FreeSpinDetail{
			UserID:        proto.Int32(1),
			GameID:        proto.Int32(2),
			FlagID:        proto.Int32(3),
			OriginID:      proto.Int64(4),
			FreeSpinGroup: proto.Int32(5),
			Times:         proto.Int32(6),
			UserBet:       proto.Int64(7),
			ChooseRate:    proto.Int32(8),
			WinMoney:      proto.Int64(9),
			WinTimes:      proto.Int32(10),
		},
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID),
		MsgData:  rpcMessage,
	}

	handler := new(FreeSpinHander)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

}

func TestFreeSpinHander_OnMessage_Reset(t *testing.T) {

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(1),
	}

	msgData, _ := proto.Marshal(&netproto.Slot_MSG_FreeGameRecord{
		Info: &netproto.Slot_FreeSpinDetail{
			UserID:        proto.Int32(1),
			GameID:        proto.Int32(2),
			FlagID:        proto.Int32(3),
			OriginID:      proto.Int64(0),
			FreeSpinGroup: proto.Int32(0),
			Times:         proto.Int32(0),
			UserBet:       proto.Int64(0),
			ChooseRate:    proto.Int32(0),
			WinMoney:      proto.Int64(0),
			WinTimes:      proto.Int32(0),
		},
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID),
		MsgData:  rpcMessage,
	}

	handler := new(FreeSpinHander)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

}
