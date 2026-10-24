package msghandler

import (
	"fmt"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"testing"

	proto "example.invalid/golang/protobuf/proto"
)

func TestDfdcBonusGameHandler_IsHook(t *testing.T) {
	type args struct {
		bclsid int32
		sclsid int32
	}
	tests := []struct {
		name string
		fsp  *DfdcBonusGameHandler
		args args
		want bool
	}{
		{
			name: "Hooked",
			fsp:  &DfdcBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID),
			},
			want: true,
		},
		{
			name: "Hooked Ret",
			fsp:  &DfdcBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID),
			},
			want: true,
		},
		{
			name: "Not Hooked, Wrong BClassID",
			fsp:  &DfdcBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Hall),
				sclsid: int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID),
			},
			want: false,
		},
		{
			name: "Not Hooked, Wrong SClassID",
			fsp:  &DfdcBonusGameHandler{},
			args: args{
				bclsid: int32(netproto.MessageBClassID_Game),
				sclsid: int32(netproto.DBServerClassID_BlockIPID),
			},
			want: false,
		},
	}
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			fsp := &DfdcBonusGameHandler{}
			if got := fsp.IsHook(tt.args.bclsid, tt.args.sclsid); got != tt.want {
				t.Errorf("DfdcBonusGameHandler.IsHook() = %v, want %v", got, tt.want)
			}
		})
	}
}

func TestDfdcBonusGameHandler_OnMessage_Record(t *testing.T) {
	initDBConnection()

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(9936738),
	}

	msgData, _ := proto.Marshal(&netproto.DFDC_JackpotBonusGameRecord{
		UserID:       proto.Int32(9936738),
		Winmoney:     proto.Int64(200),
		ChvRecordStr: proto.String("1,0,0|2,0,0|3,0,0|4,0,0|5,0,0|6,0,0|7,0,0|8,0,0|9,0,0"),
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID),
		MsgData:  rpcMessage,
	}

	handler := new(DfdcBonusGameHandler)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

	if ret[0].BClassID != int32(netproto.MessageBClassID_DBServer) {
		t.Errorf("Message BClassID want %d, but %d", int32(netproto.MessageBClassID_DBServer), ret[0].BClassID)
	}

	if ret[0].SClassID != int32(netproto.DBServerClassID_DBRetID) {
		t.Errorf("Message SClassID want %d, but %d", int32(netproto.DBServerClassID_DBRetID), ret[0].SClassID)
	}

	msg, ok := ret[0].MsgData.(*netproto.DBServerRet)

	if !ok {
		t.Errorf("Return Message is not *netproto.DBServerRet")
	}

	fmt.Printf("Code: %d\n", msg.GetCode())
}

func TestDfdcBonusGameHandler_OnMessage_Read(t *testing.T) {
	initDBConnection()

	rpcInfo := &netproto.RPCInfo{
		RouteServerID: proto.Int32(1),
		ConnID:        proto.String(""),
		IPAddress:     proto.String("local"),
		Cer:           proto.String(""),
		UserID:        proto.Int32(9936738),
	}

	msgData, _ := proto.Marshal(&netproto.DFDC_JackpotBonusGameRet{
		UserID: proto.Int32(9936738),
	})

	rpcMessage := &rpcmsg.RPCMessage{
		RPCInfo: rpcInfo,
		MsgData: msgData,
	}

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID),
		MsgData:  rpcMessage,
	}

	handler := new(DfdcBonusGameHandler)

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)

	ret := handler.OnMessage(ctx, clt, m)

	if len(ret) == 0 {
		t.Errorf("No Message Return")
	}

	if ret[0].BClassID != int32(netproto.MessageBClassID_Game) {
		t.Errorf("Message BClassID want %d, but %d", int32(netproto.MessageBClassID_Game), ret[0].BClassID)
	}

	if ret[0].SClassID != int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID) {
		t.Errorf("Message SClassID want %d, but %d", int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID), ret[0].SClassID)
	}

	msg, ok := ret[0].MsgData.(*netproto.DFDC_JackpotBonusGame)

	if !ok {
		t.Errorf("Return Message is not *netproto.DFDC_JackpotBonusGame")
	}

	fmt.Printf("Money: %d, Detail length: %d\n", msg.GetMoney(), len(msg.GetBonusDetail()))
	for _, row := range msg.GetBonusDetail() {
		fmt.Printf("Detail: %+v\n", row)
	}
}
