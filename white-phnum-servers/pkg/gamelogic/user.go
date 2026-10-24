package gamelogic

import (
	"fmt"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
)

const (
	UserState_Free    = 0 //用户站立状态
	UserState_Queue   = 1 //用户排队状态
	UserState_Sit     = 2 //用户坐下状态
	UserState_Ready   = 3 //用户举手状态
	UserState_Game    = 4 //用户游戏状态
	UserState_Offline = 5 //用户断线状态
)

const (
	UserSex_Girl = iota //性别，女
	UserSex_Boy
)

const (
	UserType_Normal = 1 //用户类型，普通用户
	UserType_Rebot  = 2 //机器人
	UserType_Guest  = 3 //游客
)

const (
	FreeTableNo = -1
	FreeSeatNo  = -1
)

//用户信息
type GameUser struct {
	TableNo             int32             //桌子号
	SeatNo              int32             //椅子号
	UserID              int32             //用户ID
	Score               int64             //当前积分 (當局積分)
	InGamingMoneyChange int64             //遊戲中金額異動 (用於斷線重連時充值金額到帳或遊戲中充值)
	LoginScore          int64             //进入时的积分
	UpdateScore         int64             //变化的积分
	TaxAmount           int64             //税收金额
	NickName            string            //昵称
	UserState           UserState         //用户状态
	ConnInfo            *network.ConnInfo //网络连接信息
	WinCount            int32             //赢
	LoseCount           int32             //输
	DrawCount           int32             //平
	UpdateWinCount      int32             //赢变化
	UpdateLoseCount     int32             //输变化
	UpdateDrawCount     int32             //平变化
	HDCode              string            //设备码
	HDType              int32             //设备类型
	UserSex             int32             //性别
	UserType            int32             //用户类型
	FaceID              int32             //头像ID
	GameTime            int32             //游戏时间
	LoginTime           time.Time         //进入时间
	LastTableNo         int32             //上一次桌子号
	LastSeatNo          int32             //上一次椅子号
	GameBuff            string            //游戏控制参数
	GameCountOnTheTable int32             //当前桌上游戏次数
	TradeID             int64             //每次進入遊戲房間會獲取一個唯一的值
	LastWinLose         int32             //上一次輸贏結果: 0輸 1贏
	SumCharge           int64             //充值金額
	SumConvert          int64             //總兌換
	HasBindPay          bool              //是否綁定支付寶或銀行卡之一
	ExperienceCash      int64             //體驗金幣
	FlowAmount          int64             //流水加總
	LoginName           string
	AlipayAccount       string
	ContestId           int32 //比賽活動ID
	TimeSeq             int64 //用戶登入遊戲時間戳
	Coupon              int64 //禮碼
	UpdateCoupon        int64 // 異動禮碼
	RobotLevel          int32 // 機器人等級
	AvatarFrameID       int32 // VIP 頭像框
	SumWinLoss          int64 //玩家该游戏总输赢
}

//创建用户对象
func NewGameUser() *GameUser {
	ur := new(GameUser)
	ur.UserState = freeState
	ur.TableNo = FreeTableNo
	ur.WinCount = 0
	ur.LoseCount = 0
	ur.DrawCount = 0
	ur.UpdateWinCount = 0
	ur.UpdateLoseCount = 0
	ur.UpdateDrawCount = 0
	ur.UpdateScore = 0
	ur.Score = 0
	ur.InGamingMoneyChange = 0
	ur.LoginScore = 0
	ur.TableNo = FreeTableNo
	ur.SeatNo = FreeSeatNo
	ur.TaxAmount = 0
	ur.GameTime = 0
	ur.LoginTime = time.Now()
	ur.LastTableNo = FreeTableNo
	ur.LastSeatNo = FreeSeatNo
	ur.GameBuff = ""
	ur.GameCountOnTheTable = 0
	ur.TradeID = 0
	ur.LastWinLose = 0
	ur.FlowAmount = 0
	ur.TimeSeq = 0
	ur.Coupon = 0
	ur.RobotLevel = 0
	ur.AvatarFrameID = 0

	return ur
}

//是否空闲
func (ur *GameUser) IsFree() bool {
	return ur.UserState.StateName() == StateName_Free
}

//是否排队
func (ur *GameUser) IsQueue() bool {
	return ur.UserState.StateName() == StateName_Queue
}

//是否坐下
func (ur *GameUser) IsSit() bool {
	return ur.UserState.StateName() == StateName_Sit
}

//是否举手
func (ur *GameUser) IsReady() bool {
	return ur.UserState.StateName() == StateName_Ready
}

//是否游戏中
func (ur *GameUser) IsGameing() bool {
	return ur.UserState.StateName() == StateName_Game
}

//是否断线
func (ur *GameUser) IsOffLine() bool {
	return ur == nil || ur.UserState.StateName() == StateName_Offline
}

//获取网络传输的gameuser 对象
func (user *GameUser) GetProtoBufGameUser() *netproto.GameUser {
	nUser := new(netproto.GameUser)
	nUser.UserID = proto.Int32(user.UserID)
	nUser.NickName = proto.String(user.NickName)
	nUser.Sex = proto.Int32(user.UserSex)
	nUser.FaceID = proto.Int32(user.FaceID)
	nUser.Score = proto.Int64(user.Score + user.Coupon) // 給Client的分數是Score加Coupon
	nUser.WinCount = proto.Int32(user.WinCount)
	nUser.LoseCount = proto.Int32(user.LoseCount)
	nUser.DrawCount = proto.Int32(user.DrawCount)
	nUser.TableNo = proto.Int32(user.TableNo)
	nUser.SeatNo = proto.Int32(user.SeatNo)
	nUser.StateValue = proto.Int32(user.UserState.StateValue())
	nUser.LastWinLose = proto.Int32(user.LastWinLose)
	nUser.Coupon = proto.Int64(user.Coupon)
	nUser.UpdateCoupon = proto.Int64(user.UpdateCoupon)
	nUser.AvatarFrameID = proto.Int32(user.AvatarFrameID)

	return nUser
}

func (ur *GameUser) String() string {
	prefix := ""
	if ur.IsRobot() {
		prefix = "[Robot] "
	}
	return fmt.Sprintf(prefix+"userid:%d nickname:%s,score:%d,updatescore:%d,tax:%d,gametime:%d,logintime:%s,state:%s,tableno:%d,seatno:%d", ur.UserID, ur.NickName, ur.Score, ur.UpdateScore, ur.TaxAmount, ur.GameTime, ur.LoginTime, ur.UserState.StateName(), ur.TableNo, ur.SeatNo)
}

//是否机器人
func (ur *GameUser) IsRobot() bool {
	return ur == nil || ur.UserType == UserType_Rebot
}

//房间用户管理
type RoomUserManager struct {
	*player.PlayerManager
}

//创建房间用户管理器
func NewRoomUserManager(playerManager *player.PlayerManager) *RoomUserManager {
	rm := new(RoomUserManager)
	rm.PlayerManager = playerManager

	return rm
}

//根据连接ID获取用户信息
func (rm *RoomUserManager) GetGameUserByConnID(connid string) *GameUser {
	clt := rm.PlayerManager.GetPlayerByConnID(connid)

	if clt == nil {
		return nil
	}

	if clt.IsGuest() {
		return nil
	}

	return rm.GetGameUserByUserID(clt.UserID)
}

//根据用户ID获取用户信息
func (rm *RoomUserManager) GetGameUserByUserID(userid int32) *GameUser {
	pr := rm.PlayerManager.GetPlayerByUserID(userid)

	if pr == nil {
		return nil
	}

	ur := pr.UserData.(*GameUser)
	ur.ConnInfo = pr.Conn

	return ur
}

//根据桌子号获取用户
func (rm *RoomUserManager) GetGameUserByTableNo(tableno int32) []*GameUser {
	var urs []*GameUser

	for _, pr := range rm.PlayerManager.LogonPlayer() {
		ur := pr.UserData.(*GameUser)

		if ur.TableNo == tableno {
			urs = append(urs, ur)
		}
	}

	return urs
}

//根据桌子号椅子号获取用户
func (rm *RoomUserManager) GetGameUserByTableAndSeat(tableno int32, seatno int32) *GameUser {

	for _, pr := range rm.PlayerManager.LogonPlayer() {
		ur := pr.UserData.(*GameUser)

		if ur.TableNo == tableno && ur.SeatNo == seatno {
			return ur
		}
	}

	return nil
}

//获取所有的房间用户
func (rm *RoomUserManager) GetAllUser() []*GameUser {
	var urs []*GameUser

	for _, pr := range rm.PlayerManager.LogonPlayer() {
		ur := pr.UserData.(*GameUser)

		urs = append(urs, ur)
	}

	return urs
}

//获取房间所有空闲用户
func (rm *RoomUserManager) GetFreeUser() []*GameUser {
	return rm.GetStateUser(UserState_Free)
}

//获取房间所有排队用户
func (rm *RoomUserManager) GetQueueUser() []*GameUser {
	return rm.GetStateUser(UserState_Queue)
}

//获取房间所有游戏用户
func (rm *RoomUserManager) GetGameingUser() []*GameUser {
	return rm.GetStateUser(UserState_Game)
}

//获取房间所有游戏用户
func (rm *RoomUserManager) GetOffLineUser() []*GameUser {
	return rm.GetStateUser(UserState_Offline)
}

//获取房间所有坐下用户
func (rm *RoomUserManager) GetSitUser() []*GameUser {
	return rm.GetStateUser(UserState_Sit)
}

//获取房间所有准备用户
func (rm *RoomUserManager) GetReadyUser() []*GameUser {
	return rm.GetStateUser(UserState_Ready)
}

//获取房间指定状态的用户
func (rm *RoomUserManager) GetStateUser(stateValue int32) []*GameUser {
	var urs []*GameUser
	for _, pr := range rm.PlayerManager.LogonPlayer() {
		ur := pr.UserData.(*GameUser)

		if ur.UserState.StateValue() == stateValue {
			urs = append(urs, ur)
		}
	}

	return urs
}

//用户数
func (rm *RoomUserManager) UserCount() int32 {
	return int32(rm.PlayerManager.UserCount())
}
