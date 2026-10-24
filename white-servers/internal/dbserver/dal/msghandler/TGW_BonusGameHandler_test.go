package msghandler

import (
	"fmt"
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

func TestTGWBonusGameHandler_IsHook(t *testing.T) {
	type args struct {
		bclsid int32
		sclsid int32
	}
	tests := []struct {
		name string
		h    *TGWBonusGameHandler
		args args
		want bool
	}{
		{
			name: "Hooked ReadBonusGame",
			h:    &TGWBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID),
			},
			want: true,
		},
		{
			name: "Hooked ResetBonusGame",
			h:    &TGWBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID),
			},
			want: true,
		},
		{
			name: "Hooked RecordBonusGame",
			h:    &TGWBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID),
			},
			want: true,
		},
		{
			name: "Not Hooked, Wrong BClassID",
			h:    &TGWBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Hall),
				sclsid: int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID),
			},
			want: false,
		},
		{
			name: "Not Hooked, Wrong SClassID",
			h:    &TGWBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.DBServerClassID_BlockIPID),
			},
			want: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			h := &TGWBonusGameHandler{}
			if got := h.IsHook(tt.args.bclsid, tt.args.sclsid); got != tt.want {
				t.Errorf("TGWBonusGameHandler.IsHook() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestTGWBonusGameHandler_OnMessage_ReadBonusGame(t *testing.T) {
	initDBConnection()

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(9936738),
	}

	msgData, _ := proto.Marshal(&netproto.TGW_MSG_ReadBonusGame{
		UserID: proto.Int32(9936738),
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID),
		MsgData:  rpcMessage,
	}

	handler := new(TGWBonusGameHandler)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

	if ret[0].BClassID != int32(netproto.MessageBClassID_Game) {
		t.Errorf("Message BClassID want %d, but %d", int32(netproto.MessageBClassID_Game), ret[0].BClassID)
	}

	if ret[0].SClassID != int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameRetID) {
		t.Errorf("Message SClassID want %d, but %d", int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameRetID), ret[0].SClassID)
	}

	msg, ok := ret[0].MsgData.(*netproto.TGW_MSG_BonusGameRecord)

	if !ok {
		t.Errorf("Return Message is not *netproto.TGW_MSG_BonusGameRecord")
	}

	fmt.Printf("UserID: %d, Records length: %d\n", msg.GetUserID(), len(msg.GetRecords()))
	for _, row := range msg.GetRecords() {
		fmt.Println(row)
	}
}

func TestTGWBonusGameHandler_OnMessage_ResetBonusGame(t *testing.T) {
	initDBConnection()

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(1),
	}

	msgData, _ := proto.Marshal(&netproto.TGW_MSG_ResetBonusGame{
		UserID: proto.Int32(1),
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID),
		MsgData:  rpcMessage,
	}

	handler := new(TGWBonusGameHandler)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

}

func TestTGWBonusGameHandler_OnMessage_RecordBonusGame(t *testing.T) {
	initDBConnection()

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(1),
	}

	msgData, _ := proto.Marshal(&netproto.TGW_MSG_RecordBonusGame{
		UserID: proto.Int32(1),
		Records: &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(1),
			Location:    proto.Int32(0),
			RewareValue: proto.Int64(20),
			BonusID:     proto.Int32(1),
		},
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID),
		MsgData:  rpcMessage,
	}

	handler := new(TGWBonusGameHandler)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

}
