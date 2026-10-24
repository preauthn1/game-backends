package main

import (
	"context"
	"flag"
	"log"
	"time"

	"example.invalid/dustin/go-humanize"
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

type user struct {
	uid int32
	cer string
}

var u1 = &user{uid: 1015669, cer: "eyJhbGciOiJBRVMiLCJ0eXAiOiJKV1QifQ==.eyJMb2dpblVzZXJJRCI6MTAxNTY2OSwiTG9naW5OYW1lIjoiZGVtby0xNjA3OTMxODMwMjA5IiwiTWVyY2hhbnRJRCI6MTc1MDEzLCJMYW5nIjoiRU4tVVMiLCJUb2tlbkV4cGlyZWRUaW1lIjoxNjA4Nzk1ODg4LCJSbmRJRCI6IjJiOTRkM2FmLTU4ZTAtNDliNS1iNzJlLWIwZGYwMzZhODc0OSIsIlNpZ25LZXkiOiJhOTQ1MWQ4YWUyYjQwNjg1MGVkZGZlNjAxNzUyNzM0ZSIsIlRva2VuVmVyc2lvbiI6MTAwMH0=.Ob3fFSoNyH1NPXWFYDOOUe+bTDFIZ1dC3fPEXelRt9A+T/s1+7siu8S0oOir8NZj3tupr1G8+hutlV9/dQQee1747JjH+4BYIeFjFMVNJxuYbME9qhEDBxc5H5Q+blgh0QfwQmwlkR+yQQJImQ+0s44ljFliNkOusCJOjR0dFfpW7xlW5B7PyaX0XqT8jOzFfTWXLbg/sD/YD2X6cG/Op2jCFajdZ3/pI0VFhYvnD+zdbNzYwLmZO8GS66qlUPP2z54c2aQZJSNDaBE1dfEapYok4tWtzl+fcpXsDJB3EFt7knKhTW3T+FGo8LiyBB5Dv2EbLxEN+LEIhPgyqQHHBUHpOrcICNEYgA0nMINhjHbEOTJC4+lKM0GK7mrcgbxuf65p+ZiLLUG5Gda1DmxVf/HA0LiDOIRadHjDLyuy0b/WorM6mN5UFXk3SjvL4JlXfMij76EeCLCYYiW13ScpSg=="}
var u2 = &user{uid: 1015671, cer: "eyJhbGciOiJBRVMiLCJ0eXAiOiJKV1QifQ==.eyJMb2dpblVzZXJJRCI6MTAxNTY3MSwiTG9naW5OYW1lIjoiZGVtby0xNjA3OTMxOTc1Mzc1IiwiTWVyY2hhbnRJRCI6MTc1MDEzLCJMYW5nIjoiRU4tVVMiLCJUb2tlbkV4cGlyZWRUaW1lIjoxNjA4Nzk1OTc4LCJSbmRJRCI6IjQwNGYzYTZkLTQyZjQtNDRiYS05ZGRkLTAwOTIyZGJkMGEyZCIsIlNpZ25LZXkiOiI0ZGIxNWFlNTQ5ZWRmN2U5ZWNlNGJjNTc1YmNjMmQ0MCIsIlRva2VuVmVyc2lvbiI6MTAwMH0=.Ob3fFSoNyH1NPXWFYDOOUe+bTDFIZ1dC3fPEXelRt9A+T/s1+7siu8S0oOir8NZj3tupr1G8+hutlV9/dQQee3HA+3T9V45Y3kZdqM2vuQGYbME9qhEDBxc5H5Q+blghSvxRbcv8LalwV4adVu+1hI4ljFliNkOusCJOjR0dFfpW7xlW5B7PyaX0XqT8jOzFfTWXLbg/sD/YD2X6cG/Op2jCFajdZ3/pI0VFhYvnD+zdbNzYwLmZO8GS66qlUPP2E0+uB6gdjkz8ZoLaWi5MmqjL5ocquvpXNuhZMDGEnbXsRQVIX0bkkFsIPuUAqocHRrVe4XC7mXt3kswaMtPZ3++NNVN8z/zcThwZApO63n0KC9ja5zSVN/6s8pZDsy1p/73ifgtGfn1gO+TOxj9OAP/IdVft/P1FUXDm6KjSyPvWorM6mN5UFXk3SjvL4JlXfMij76EeCLCYYiW13ScpSg=="}
var pid int

func init() {
	flag.IntVar(&pid, "p", 0, "user1 ort user2")
}

// UserStateNameByValue ?
func UserStateNameByValue(v int) string {
	// 用戶狀態
	switch v {
	default:
		return ""
	case gamelogic.UserStateFree:
		return gamelogic.StateNameFree
	case gamelogic.UserStateQueue:
		return gamelogic.StateNameQueue
	case gamelogic.UserStateSit:
		return gamelogic.StateNameSit
	case gamelogic.UserStateReady:
		return gamelogic.StateNameReady
	case gamelogic.UserStateGame:
		return gamelogic.StateNameGame
	case gamelogic.UserStateOffline:
		return gamelogic.StateNameOffline
	}
}

func main() {

	var u *user

	flag.Parse()

	switch pid {
	case 1:
		u = u1
	case 2:
		u = u2
	}

	if u == nil {
		log.Fatal("Not right user")
	}

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	packer := network.NewHeadMsgPacker(conf.MSGPACKERBUF, true)
	evt := msgserver.NewMsgEvtChan(conf.EVTCHANSIZE)
	parser := msg.NewProtoMsgParser()

	client := msgserver.NewTCPMsgClient(packer, evt, parser, false)

	if err := client.Connect(":3000"); err != nil {
		log.Fatal(err)
	}
	defer client.Close()

	go func() {
		for {
			time.Sleep(2 * time.Second)
			m := new(msg.Message)
			m.BClassID = int32(netproto.MessageBClassID_Common)
			m.SClassID = int32(netproto.PlatformCommonClassID_HeartBeatID)
			if err := client.SendMessage(m); err != nil {
				log.Fatal(err)
			}
		}
	}()

	logined := make(chan bool, 1)
	go func() {
		for m := range evt.MsgSig {
			// fmt.Println(m)
			if m.BClassID == int32(netproto.MessageBClassID_Common) && m.SClassID == int32(netproto.PlatformCommonClassID_TipMessageID) {
				data := new(netproto.TipMessage)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("你被踢了... Message: %v, Type: %d, Param: %d", data.GetMessage(), data.GetMsgType(), data.GetParam())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_GameRoom) && m.SClassID == int32(netproto.GameRoomClassID_LoginRoomRetID) {
				data := new(netproto.LoginGameRoomRet)
				proto.Unmarshal(m.MsgData.([]byte), data)
				if data.GetCode() != 1 {
					log.Fatalf("登入失敗 %s", data.GetMessage())
				}
				logined <- true
				log.Printf("%d(%s) 登入成功", data.GetUserData().GetUserID(), data.GetUserData().GetNickName())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.GameMessageClassID_UserEnterID) {
				data := new(netproto.UserEnter)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("玩家[%d] 進入 桌子[%d] 座位[%d] 狀態[%s] 金幣[%s]", data.User.GetUserID(), data.User.GetTableNo(), data.User.GetSeatNo(), UserStateNameByValue(int(data.User.GetStateValue())), humanize.Commaf(float64(data.User.GetScore())/100))
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.GameMessageClassID_GameStartID) {
				log.Printf("=== 遊戲開始 ===")
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.GameMessageClassID_GameEndID) {
				log.Printf("=== 遊戲結束 ===")
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.GameMessageClassID_UserStateChangeID) {
				data := new(netproto.UserStateChange)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("玩家[%d] 狀態[%s] 金幣[%s]", data.User.GetUserID(), UserStateNameByValue(int(data.User.GetStateValue())), humanize.Commaf(float64(data.User.GetScore())/100))
				continue
			}
		}
	}()

	{ // 登入
		m := new(msg.Message)
		m.BClassID = int32(netproto.MessageBClassID_GameRoom)
		m.SClassID = int32(netproto.GameRoomClassID_LoginRoomID)
		data := new(netproto.LoginGameRoomInfo)
		data.UserID = proto.Int32(u.uid)
		data.Cer = proto.String(u.cer)
		data.HDCode = proto.String("")
		data.HDType = proto.Int32(0)
		m.MsgData, _ = proto.Marshal(data)
		if err := client.SendMessage(m); err != nil {
			log.Fatal(err)
		}
	}

	<-logined

	select {
	case <-ctx.Done():
		log.Printf("結束")
	}
}
