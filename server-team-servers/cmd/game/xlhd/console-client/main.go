package main

import (
	"bufio"
	"context"
	"errors"
	"log"
	"os"
	"strconv"
	"strings"
	"time"

	"example.invalid/dustin/go-humanize"
	"example.invalid/golang/protobuf/proto"
	"example.invalid/spf13/cobra"

	"example.invalid/ServerTeam/Servers/internal/game/lhd"
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

var u1 = &user{uid: 1020239, cer: "eyJhbGciOiJBRVMiLCJ0eXAiOiJKV1QifQ==.eyJMb2dpblVzZXJJRCI6MTAyMDIzOSwiTG9naW5OYW1lIjoiZGVtby0xNjc4ODg3NjkzNTk1IiwiTWVyY2hhbnRJRCI6MTc1MDEzLCJTcGVjaWZpY0dhbWVJRCI6MjIzMiwiTGFuZyI6ImVuLXVzIiwiVG9rZW5FeHBpcmVkVGltZSI6MTY5MzE0MzE3NSwiUm5kSUQiOiIxNjE2YTZlMC1jOTIyLTRiZGMtODUzNi04MmUxOWFmYTQxZGMiLCJTZXNzaW9uVG9rZW5SYXciOm51bGwsIlNlc3Npb25Ub2tlblBHUyI6IjQ4NmRkOWJjLTEyN2YtNDc3Yy1hODQ5LWRiOGJhM2FlMDA2NSIsIlNpZ25LZXkiOiI0MWY2ZDVkY2Q1ZWQ0MjgyNTFhMDAyNTY5Y2NkYWMxMyIsIlRva2VuVmVyc2lvbiI6MTAwMSwiSnd0VG9rZW5UeXBlIjoxfQ==.Ob3fFSoNyH1NPXWFYDOOUe+bTDFIZ1dC3fPEXelRt9A+T/s1+7siu8S0oOir8NZjlxNefrm16HXvZ3ERHP8xVIP6/vdLJ+PBNAEb5bp/dYcHfKOwy8fDBKLsJs4TEbksg0rFNT8lOS5949KKSYq+Wo4ljFliNkOusCJOjR0dFfruYBLywrkJdj/KrWXT/O659jBKZMBCNh199z63Q91aa0boV3cFJiXxjfkwPBJ3O4t9RZDHHvi2tTQGuas0NK/9AzcDIPVQnQqqg19qKDmam0kNl3unhGDmpuWn2owTvXraLoQ+pPeFIBmFy7+HUFuiUeivrVymB0KFx+PBzzrnb5U52dGUVWkCPfEJKY1+TPpk+T9ySgO+ndmnTzc6F6RMl9MWitA6fJAAGK8mfDjLlzus11Qxy4kJjqEL1T8VEygq7OBIDki8nrsOHTpX46wSUoRee7c6N9YQUPYtEJl957uRYdE7HlY8ivhs7qkGdWY0KpviA8ojlEeLWsS0Pjy70ZT3JNxYeGAuhxE4GZI4dNHnvV5KOaqczp9/RFof8CdBOzw29/XTGmAyay9UdhVeC9S4rZF59asZYGZTdlQimKvst4gDHm1vM9KqyAFFr9DeDGi7TAHhPq7cO9oa6TLBmYpnQI6eFR6tbMfyZHfS05ObOxHVkvZCeFLokTtx7q4="}

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

var rootCommand = &cobra.Command{
	Use:   "",
	Short: "",
	Long:  "",
	RunE: func(cmd *cobra.Command, args []string) error {
		return nil
	},
}

var betCommand = &cobra.Command{
	Use:   "bet [args]",
	Short: "bet",
	Long:  "bet",
	RunE: func(cmd *cobra.Command, args []string) error {
		if len(args) != 2 {
			return errors.New("args error")
		}

		var t int32
		var n int64
		{
			v, err := strconv.ParseUint(args[0], 0, 32)
			if err != nil {
				return err
			}

			t = int32(v)

			switch int(t) {
			default:
				return errors.New("bet type error")
			case lhd.LONG, lhd.HU, lhd.HE:
			}
		}
		{
			v, err := strconv.ParseInt(args[1], 0, 64)
			if err != nil {
				return err
			}
			n = v
		}

		{ // 下注
			m := new(msg.Message)
			m.BClassID = int32(netproto.MessageBClassID_Game)
			m.SClassID = int32(netproto.LHD_GameMessageClassID_LHDBetID)
			data := new(netproto.LHD_BetArr)
			data.BetArr = append(data.BetArr, &netproto.LHD_Bet{
				BetType: proto.Int32(t),
				Count:   proto.Int32(int32(n)),
			})

			m.MsgData, _ = proto.Marshal(data)

			log.Println("-->", m)

			if err := client.SendMessage(m); err != nil {
				return err
			}
		}

		return nil
	},
}

var packer = network.NewHeadMsgPacker(conf.MSGPACKERBUF, true)
var evt = msgserver.NewMsgEvtChan(conf.EVTCHANSIZE)
var parser = msg.NewProtoMsgParser()
var client = msgserver.NewTCPMsgClient(packer, evt, parser, false)

func init() {
	rootCommand.AddCommand(betCommand)
}

func main() {

	u := u1

	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	if err := client.Connect(":3000"); err != nil {
		log.Fatal(err)
	}
	defer client.Close()

	go func() {
		for {
			select {
			default:
				r := bufio.NewReader(os.Stdin)
				line, _, err := r.ReadLine()
				if err != nil {
					log.Fatal(err)
				}
				str := string(line)
				str = strings.ToLower(str)
				str = strings.TrimSpace(str)

				rootCommand.SetArgs(strings.Split(str, " "))

				if err := rootCommand.Execute(); err != nil {
					log.Println(err)
				}

			case <-ctx.Done():
			}
		}
	}()

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
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDSettingID) {
				data := new(netproto.LHD_Setting)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲設定 下注最低攜帶金額[%d] 籌碼面額 %v , 最大下注額 %v, 玩家最大下注 %v, 莊Limit[%d], 下注區域[%d]", m, data.GetBetlimitmoney(), data.GetBetCount(), data.GetBetMax(), data.GetUserMaxBet(), data.GetZhuangLimit(), len(data.GetBetAreas()))
				for _, v := range data.GetBetAreas() {
					log.Printf("下注區域[%d] 最低下注[%d] 最高下注[%d] 賠率[%.2f]", v.GetType(), v.GetMin(), v.GetMax(), v.GetOdds())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDRoundID) {
				data := new(netproto.LHD_RoundID)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 回合資訊 RoundID[%d] 龍[%d] 和[%d] 虎[%d]", m, data.GetRoundID(), data.GetLongWin(), data.GetHeWin(), data.GetHuWin())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDResultCardListID) {
				data := new(netproto.LHD_ResultCardList)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲紀錄 共 %d 筆", m, len(data.GetCardList()))
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDRichestListID) {
				data := new(netproto.LHD_RichestList)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 富豪榜 共 %d 筆", m, len(data.GetRichestList()))
				for _, user := range data.GetRichestList() {
					log.Printf("    玩家[%d] 金幣[%s]", user.GetUserID(), humanize.Commaf(float64(user.GetMoney())/100))
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDLuckyStarListID) {
				data := new(netproto.LHD_LuckyStar)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 幸運星 玩家[%d]", m, data.GetUserID())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDZhuangInfoID) {
				data := new(netproto.LHD_ZhuangInfo)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 莊家資訊 莊家[%d] LastTurn[%d] Users[%d]", m, data.GetUserID(), data.GetLastTurn(), len(data.GetUserInfo()))
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDStateID) {
				data := new(netproto.LHD_Sate)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲階段 %v 剩 %d 秒", m, data.GetState(), data.GetTime())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDGameResultPokerID) {
				data := new(netproto.LHD_GameResultPoker)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲結果 第 %d 回合", m, data.GetRoundID())
				for _, card := range data.GetPoker() {
					log.Printf("    花色: %v 數字: %v", card.GetColor(), card.GetNumber())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDGameResultWinID) {
				data := new(netproto.LHD_GameResultUserMoney)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 玩家贏錢資訊 自己贏[%d] 莊贏[%d] 贏家數[%d] 標記人數[%d]", m, data.GetSelfWin(), data.GetZhuangWin(), len(data.GetWinUserInfo()), len(data.GetStageUerInfo()))
				for _, user := range data.GetWinUserInfo() {
					log.Printf("    贏家[%s] 贏[%d]", user.GetUserName(), user.GetWin())
				}
				for _, user := range data.GetStageUerInfo() {
					log.Printf("    標記人[%d] 贏[%d]", user.GetUserID(), user.GetWin())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDJackpotID) {
				data := new(netproto.LHD_JackpotInfo)
				proto.Unmarshal(m.MsgData.([]byte), data)

				log.Printf("%v -> 獎池資訊 金額: %d, 階段: %d, 階段比: %d", m, data.GetMoney(), data.GetPhase(), data.GetPhaseRate())

				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDJackpotSettingID) {
				data := new(netproto.LHD_JackpotSetting)
				proto.Unmarshal(m.MsgData.([]byte), data)

				log.Printf("獎池設定:")
				for _, setting := range data.GetMoney() {
					log.Printf("    %v 彩金: %d", setting.GetType().Enum().String(), setting.GetMoney())
				}
				for _, setting := range data.GetAllowdBet() {
					log.Printf("    %v 下注限制: %d", setting.GetType().Enum().String(), setting.GetMoney())
				}

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

	<-ctx.Done()
	log.Printf("結束")
}
