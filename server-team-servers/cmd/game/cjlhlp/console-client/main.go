package main

import (
	"bufio"
	"context"
	"errors"
	"log"
	"math/rand"
	"os"
	"strconv"
	"strings"
	"time"

	"example.invalid/dustin/go-humanize"
	"example.invalid/golang/protobuf/proto"
	"example.invalid/spf13/cobra"

	"example.invalid/ServerTeam/Servers/internal/game/roulette"
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

var u1 = &user{uid: 1020239, cer: "eyJhbGciOiJBRVMiLCJ0eXAiOiJKV1QifQ==.eyJMb2dpblVzZXJJRCI6MTAyMDIzOSwiTG9naW5OYW1lIjoiZGVtby0xNjc4ODg3NjkzNTk1IiwiTWVyY2hhbnRJRCI6MTc1MDEzLCJTcGVjaWZpY0dhbWVJRCI6MjIzMiwiTGFuZyI6ImVuLXVzIiwiVG9rZW5FeHBpcmVkVGltZSI6MTY5NjA4MjkwMywiUm5kSUQiOiI1NmQxNzcxMS1hZWEzLTQxMzYtYjgwMC0zMmY1ZTEyNTdmYjQiLCJTZXNzaW9uVG9rZW5SYXciOm51bGwsIlNlc3Npb25Ub2tlblBHUyI6ImJkYmJmNTZiLTdjYTYtNDNjZi05YzU0LTkyMDBhNzQ5ZTk1YiIsIlNpZ25LZXkiOiJiNDZhYWUxOTA1ZjVhYzdmYmMzZGY4OWNhYjhiYWIwMSIsIlRva2VuVmVyc2lvbiI6MTAwMSwiSnd0VG9rZW5UeXBlIjoxfQ==.Ob3fFSoNyH1NPXWFYDOOUe+bTDFIZ1dC3fPEXelRt9A+T/s1+7siu8S0oOir8NZjlxNefrm16HXvZ3ERHP8xVIP6/vdLJ+PBNAEb5bp/dYcHfKOwy8fDBKLsJs4TEbksg0rFNT8lOS5949KKSYq+Wo4ljFliNkOusCJOjR0dFfruYBLywrkJdj/KrWXT/O659jBKZMBCNh199z63Q91aa0boV3cFJiXxjfkwPBJ3O4t9RZDHHvi2tTQGuas0NK/9AzcDIPVQnQqqg19qKDmam+eP/wsv5cNrttA4LXLA/1ytYl0o9+ECE97txXtGc1WGaTw3jTaDsZjvlkg2Dd0ISavUVnT/j4doBaDCsXsMmcnkbdtnwt6KjeQYgbnuPWIGl9MWitA6fJAAGK8mfDjLlzus11Qxy4kJjqEL1T8VEygq7OBIDki8nrsOHTpX46wS8H8mItFxKKF8cpn7aFL6wTMbYXauq6GDnHdz/gpTkd31uCVc2cmDT4qzujSmu85O9jGwEPcLlObH8YazAsC0tngRffdUP1aKNveK+LC6xdUDAwyKU3E8Qjy+NYDWFHmnF9KUQPUuQiCLHlN6hXPyHB9aPhbQfOinK7cnaPY7HFzeDGi7TAHhPq7cO9oa6TLBmYpnQI6eFR6tbMfyZHfS05ObOxHVkvZCeFLokTtx7q4="}

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
	Use:   "bet {bet type} {bet money}",
	Short: "bet by type and money, it will be auto generate numbers by type",
	Long: `bet by type and money, it will be auto generate numbers by type.
  type: 
    1: Odd
    2: Even
    3: Red
    4: Black
    5: Small
    6: Big
    7: First12
    8: Second12
    9: Third12
    10: FirstRow
    11: SecondRow
    12: ThirdRow
    13: OneNumber
    14: TwoNumber
    15: ThreeNumber
    16: FourNumber
    17: SixNumber
    18: SuperZero`,
	Example: `bet 1 100
bet 5 100
bet 13 100`,
	RunE: func(cmd *cobra.Command, args []string) error {
		if len(args)%2 != 0 {
			return errors.New("args error")
		}

		var bets []*netproto.Roulette_Bet

		var t roulette.BetType
		var n int64
		var numbers []uint32

		for i := 0; i < len(args); i += 2 {
			{
				v, err := strconv.ParseUint(args[i], 0, 32)
				if err != nil {
					return err
				}

				t = roulette.BetType(uint32(v))

				switch t {
				default:
					return errors.New("bet type error")
				case roulette.Odd,
					roulette.Even,
					roulette.Red,
					roulette.Black,
					roulette.Small,
					roulette.Big,
					roulette.First12,
					roulette.Second12,
					roulette.Third12,
					roulette.FirstRow,
					roulette.SecondRow,
					roulette.ThirdRow,
					roulette.OneNumber,
					roulette.TwoNumber,
					roulette.ThreeNumber,
					roulette.FourNumber,
					roulette.SixNumber,
					roulette.SuperZero:
				}
			}
			{
				v, err := strconv.ParseInt(args[i+1], 0, 64)
				if err != nil {
					return err
				}
				n = v
			}
			{
				numberSet := roulette.RobotBetAreasNumbers[t]
				if len(numberSet) > 0 {
					numbers = numberSet[rand.Intn(len(numberSet))]
				}
			}
			bets = append(bets, &netproto.Roulette_Bet{
				Type:    roulette.TransforBetType(t).Enum(),
				Money:   proto.Int64(n),
				Numbers: numbers,
			})
		}

		{ // 下注
			m := new(msg.Message)
			m.BClassID = int32(netproto.MessageBClassID_Game)
			m.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetID)
			data := new(netproto.Roulette_BetRequest)
			data.Bet = append(data.Bet, bets...)

			m.MsgData, _ = proto.Marshal(data)

			log.Println("-->", m, data)

			if err := client.SendMessage(m); err != nil {
				return err
			}
		}

		return nil
	},
}

var undoCommand = &cobra.Command{
	Use:   "undo",
	Short: "undo last bet",
	RunE: func(cmd *cobra.Command, args []string) error {

		{ // 下注
			m := new(msg.Message)
			m.BClassID = int32(netproto.MessageBClassID_Game)
			m.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetClearID)

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
	rootCommand.AddCommand(undoCommand)
}

func main() {

	u := u1

	if u == nil {
		log.Fatal("Not right user")
	}

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
				log.Printf("玩家[%d] 進入 桌子[%d] 座位[%d] 狀態[%s] 金幣[%d][%s]", data.User.GetUserID(), data.User.GetTableNo(), data.User.GetSeatNo(), UserStateNameByValue(int(data.User.GetStateValue())), data.User.GetScore(), humanize.Commaf(float64(data.User.GetScore())/100))
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
				log.Printf("玩家[%d] 狀態[%s] 金幣[%d][%s]", data.User.GetUserID(), UserStateNameByValue(int(data.User.GetStateValue())), data.User.GetScore(), humanize.Commaf(float64(data.User.GetScore())/100))
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteSettingID) {
				data := new(netproto.Roulette_Setting)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲設定 下注最低攜帶金額[%d] 籌碼面額 %v 單一玩家最高下注[%d]", m, data.GetBetMinCarry(), data.GetBetCount(), data.GetUserMaxBet())
				for _, betArea := range data.GetBetAreas() {
					log.Printf("    下注區域[%v] 最低下注[%d] 最高下注[%d] 賠率[%.3f]", roulette.ReverseBetType(betArea.GetType()), betArea.GetMinBet(), betArea.GetMaxBet(), betArea.GetOdd())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteRoundID) {
				data := new(netproto.Roulette_RoundInfo)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 回合資訊 RoundID[%d]", m, data.GetRoundID())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteRichestListID) {
				data := new(netproto.Roulette_RichestList)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 富豪榜 共 %d 筆", m, len(data.GetUsers()))
				for _, user := range data.GetUsers() {
					log.Printf("    玩家[%d] 金幣[%s]", user.GetUserID(), humanize.Commaf(float64(user.GetMoney())/100))
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteLuckyStarListID) {
				data := new(netproto.Roulette_LuckyStarList)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 幸運星 共 %d 筆", m, len(data.GetUsers()))
				for _, user := range data.GetUsers() {
					log.Printf("    玩家[%d] 金幣[%s]", user.GetUserID(), humanize.Commaf(float64(user.GetMoney())/100))
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteRecordsID) {
				data := new(netproto.Roulette_GameRecords)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲紀錄 共 %d 筆", m, len(data.GetResults()))
				for _, result := range data.GetResults() {
					log.Printf("    第 %d 回合 %v 號", result.GetRoundID(), result.GetResult())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteStepID) {
				data := new(netproto.Roulette_StepInfo)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲階段 %v 剩 %d 秒", m, roulette.GameStep(data.GetStep()), data.GetTime())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteGameResultID) {
				data := new(netproto.Roulette_GameResult)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 遊戲結果 第 %d 回合 %v 號", m, data.GetRoundID(), data.GetResult())
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteGameResultWinID) {
				data := new(netproto.Roulette_GameResultWin)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 玩家贏錢資訊 贏[%d] 贏家數[%d] 標記人數[%d]", m, data.GetWin(), len(data.GetWinUserInfo()), len(data.GetStageUserInfo()))
				for _, user := range data.GetWinUserInfo() {
					log.Printf("    贏家[%d] 贏[%d]", user.GetUserID(), user.GetWin())
				}
				for _, user := range data.GetStageUserInfo() {
					log.Printf("    標記人[%d] 贏[%d]", user.GetUserID(), user.GetWin())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteBetRetID) {
				data := new(netproto.Roulette_BetResponse)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 玩家下注結果 [%v] 身上剩餘: %d", m, data.GetCode(), data.GetMoney())
				for _, bet := range data.GetBets() {
					log.Printf("    下注類型[%v] 下注金額[%d] 下注號碼 %v", roulette.ReverseBetType(bet.GetType()), bet.GetMoney(), bet.GetNumbers())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteBetClearRetID) {
				data := new(netproto.Roulette_BetResponse)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 玩家回收結果 [%v] 身上剩餘: %d", m, data.GetCode(), data.GetMoney())
				for _, bet := range data.GetBets() {
					log.Printf("    下注類型[%v] 下注金額[%d] 下注號碼 %v", roulette.ReverseBetType(bet.GetType()), bet.GetMoney(), bet.GetNumbers())
				}
				continue
			}
			if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteBoardCastBetID) {
				data := new(netproto.Roulette_BoardCastBet)
				proto.Unmarshal(m.MsgData.([]byte), data)
				log.Printf("%v -> 玩家[%d] 下注 [%d]", m, data.GetUserID(), len(data.Bets))
				for _, bet := range data.GetBets() {
					log.Printf("    下注類型[%v] 下注金額[%d] 下注號碼 %v", roulette.ReverseBetType(bet.GetType()), bet.GetMoney(), bet.GetNumbers())
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
