package lhd

import (
	"context"
	"fmt"
	"math"
	"math/rand"
	"net/http"
	"os"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"

	"example.invalid/golang/protobuf/proto"

	apipb "example.invalid/ServerTeam/Servers/internal/api-service/pb/apisvc"
	pb "example.invalid/ServerTeam/Servers/internal/cli-api-svc/pb/cliapisvc"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/poker"
	"example.invalid/ServerTeam/Servers/pkg/prob"
	"example.invalid/ServerTeam/Servers/pkg/roomutil"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	Bet            = iota //下注状态
	KaiJiang              //开奖状态
	KaiJiangAction        //开奖动画
	EndAction             //结算动画
	Wait                  //等待游戏开始
)

const (
	LONG = 1 //龙
	HE   = 2 //和
	HU   = 3 //虎
)

// 第一个用来占位
var resultstring = []string{"", "龙", "和", "虎"}

const (
	GameStartAnimation  = "gamestart"
	GameKaiJiang_Action = "kaijiang"
	GameEnd_Action      = "end"
)

const (
	HEandKill     = 0
	HEandBack     = 1
	HEandKillHalf = 2
)

const (
	CtrlBetLimitDefault int64 = 100 // 監控名單預設注額
)

type card struct {
	color  int32
	number int32
}

type userScore struct {
	userdata []*gamelogic.GameUser
}

type LongHudouLogic struct {
	*gamelogic.ComputeGameLogicBase
	serverfile         *os.File
	opttimer           *event.TimerFunc
	bettotal           int64                        //下注总额
	bettotalrobot      int64                        //机器人下注总额
	bettypetotal       []*bettype                   //下注分类汇总
	bettyperobot       []*bettype                   //机器人下注分类汇总
	betvalue           *betconfig                   //下注配置文件
	userdata           []*longhudouuserdata         //用户数据
	cardresult         []*card                      //开奖的牌
	gamestate          int32                        //游戏状态
	bettime            int32                        //下注时间
	jiangtype          int32                        //开奖类型
	action_str         string                       //当前动画
	kaijiangactiontime int32                        //开奖动画超时
	endactiontime      int32                        //用户结算动画超时
	readytime          int32                        //用户准备时间
	zhuangID           int32                        //当前莊家ID
	zhuanglimit        int64                        //上莊限制
	lastturn           int32                        //莊剩余的局数
	maxturn            int32                        //上莊的最大局数
	zhuanglist         []int32                      //上莊列表
	betcount           []int32                      //下注数量
	zhuangWin          int64                        //莊家赢錢
	serverid           string                       //服务器ID
	winuserinfo        []int32                      //赢錢最多的前三家userid
	resultcardlist     *netproto.LHD_ResultCardList //开奖的历史牌
	resultlist         []int32                      //开奖的历史
	zhushui            int32                        //注水率
	maxclearcount      int32                        //清理数据上限
	maxrobotzhuang     int32                        //最多有好多个机器人上莊
	robotxiazhuang     int32                        //机器人下莊概率
	robotzhuang        int32                        //没有人当莊的时候机器人当莊的概率
	zhushuimoney       int64                        //注水的錢
	he                 int32                        //下和的概率百分比
	startsyspool       int64                        //开始时的金币数量
	startmoney         int64                        //上莊时的金币
	currtax            int64                        //当前抽水
	nextjiangtype      int32                        //下次开奖类型
	systotalwin        int64                        //系统收入
	syslastwin         int64                        //上一局收益
	sametimebetmode    int64                        //是否開啟莊閒同時下注
	systaxincome       int64                        //系統稅收, 非該局稅收
	userwinlose        int64                        //玩家輸贏
	robotwinlose       int64                        //機器人輸贏
	uservalidamount    int64                        //玩家有效下注
	usertaxamount      int64                        //玩家稅收
	betlimitmoney      int64                        //下注限制
	publiczhuang       int64                        //是否開放用戶上莊
	userscore          *userScore                   //用户数据
	luckystar          int32                        //幸運星玩家id
	luckyfaceid        int32                        //幸運星玩家頭像id
	luckystarround     int64                        //幸運星持續局數
	luckystartimes     int64                        //幸運星第幾局
	notbettimes        int64                        //沒下注持續局數
	richestuserinfo    []int32                      //富豪玩家的userid
	richestList        []*netproto.LHD_Richest      //富豪榜
	stageuserinfo      []int32                      //標誌玩家的userid
	roundid            int32                        //當局局數
	longwin            int32                        //龍贏局數
	hewin              int32                        //和局
	huwin              int32                        //虎贏局數
	newcard            bool                         //重新發牌
	poker              poker.Poker                  //整副牌
	cards              *poker.CardGroup             //所有的牌
	lastGameUserBet    []*longhudouuserdata         //上局下注玩家
	//bowProvider        *gamelogic.BlackOrWhiteUserProvider   //需要放分或殺分的列表
	assignRewardDetail  *netproto.SuperToolAssignRewardDetail //指定開獎
	execAssignReward    bool                                  //true 執行指定開獎
	fangFenTarget       int64                                 //放分金額
	fangFenComplete     int64                                 //己放分金額
	execFangFen         bool                                  //true 執行系統放分
	SuperToolService    *gamelogic.SuperToolBrGameServiceBase
	HEandKill           int32 //開和通殺
	ctrlbetlimit        int64 //監控名單會控制的最低注額
	EventChan           chan *pb.LHDTableDashboard
	BroadcastSubscriper *sync.Map
	CheatFlag           bool    // 是否要作弊
	cheatKickCnt        int     // 踢人數
	blockList           []int32 // 作弊後踢出的玩家，重連後要踢出

	SpecialRewardCount int32 // 特殊獎勵次數
	SpecailRewardRate  int32 // 特殊獎勵機率 Default: 70

	totalValidAmount   int64
	totalBetAmount     int64
	totalWinLossAmount int64
	FlowType           int32 // 流水統計方式 0: 輸分流水 1: 有效投注流水 Default: 0

	cheatrate int32 //作弊概率
	Disabled  bool  // 是否關閉遊戲

	Jackpot *Jackpot // 獎池
}

// 新建一个龙虎斗实例
func NewLongHuDouLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.Group) *LongHudouLogic {
	glc := new(LongHudouLogic)
	glc.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)
	glc.getlhdconfig(cfgs)
	glc.SuperToolService = glc.GetSuperToolService().(*gamelogic.SuperToolBrGameServiceBase)
	glc.gamestate = Wait
	glc.zhuangID = 0
	glc.he = 0
	glc.nextjiangtype = 0
	glc.syslastwin = 0
	glc.systotalwin = 0
	glc.roundid = 0
	glc.longwin = 0
	glc.hewin = 0
	glc.huwin = 0
	glc.newcard = false
	glc.resultcardlist = new(netproto.LHD_ResultCardList)
	glc.startsyspool = glc.SysPool.GetPool()
	glc.EventChan = make(chan *pb.LHDTableDashboard, 10)
	glc.BroadcastSubscriper = new(sync.Map)
	glc.SpecailRewardRate = 70

	{ // 解析流水模式
		v := cfgs.GetConfigValue(gamelogic.ConfigFileNameGameServer, gamelogic.ConfigSectionNameGameServer, "flowtype")
		if n, err := strconv.ParseInt(v, 10, 32); err != nil {
			glc.FlowType = 0
		} else {
			glc.FlowType = int32(n)
		}
	}

	{ // 解析 Jackpot 設定
		jp := NewJackpot()

		sec := cfgs.GetConfigSection(gamelogic.ConfigFileNameGameServer, "jackpot")
		if sec != nil {
			if ok, err := strconv.ParseBool(sec["enabled"]); err != nil {
				mlog.Warn("Jackpot enabled parse error: %v", err)
			} else {
				jp.Enabled = ok
			}

			if ok, err := strconv.ParseBool(sec["awardpause"]); err != nil {
				mlog.Warn("Jackpot enabled parse error: %v", err)
			} else {
				jp.AwardPause = ok
			}

			if v, err := strconv.ParseInt(sec["basepool"], 10, 64); err != nil {
				mlog.Warn("Jackpot basepool parse error: %v", err)
			} else {
				jp.BasePool = v
			}

			if v, err := strconv.ParseInt(sec["contribute"], 10, 64); err != nil {
				mlog.Warn("Jackpot contribute parse error: %v", err)
			} else {
				jp.ContributeRate = v
			}

			if v, err := strconv.ParseInt(sec["awardpool"], 10, 64); err != nil {
				mlog.Warn("Jackpot awardpool parse error: %v", err)
			} else {
				jp.AwardLine = v
			}

			{ // phase level
				strs := strings.Split(sec["phaselevel"], ",")
				var arr []int64
				for _, str := range strs {
					str = strings.TrimSpace(str)
					if v, err := strconv.ParseInt(str, 10, 64); err != nil {
						mlog.Warn("Jackpot phaselevel parse error: %v", err)
					} else {
						arr = append(arr, v)
					}
				}

				jp.PhaseLevel = arr
			}

			if v, err := strconv.ParseInt(sec["award_weight_none"], 10, 64); err != nil {
				mlog.Warn("Jackpot award_weight_none parse error: %v", err)
			} else {
				jp.AwardWeight[JackpotLevelNone] = v
			}

			if v, err := strconv.ParseInt(sec["award_weight_mini"], 10, 64); err != nil {
				mlog.Warn("Jackpot award_weight_mini parse error: %v", err)
			} else {
				jp.AwardWeight[JackpotLevelMini] = v
			}

			if v, err := strconv.ParseInt(sec["award_weight_minor"], 10, 64); err != nil {
				mlog.Warn("Jackpot award_weight_minor parse error: %v", err)
			} else {
				jp.AwardWeight[JackpotLevelMinor] = v
			}

			if v, err := strconv.ParseInt(sec["award_weight_grand"], 10, 64); err != nil {
				mlog.Warn("Jackpot award_weight_grand parse error: %v", err)
			} else {
				jp.AwardWeight[JackpotLevelGrand] = v
			}

			if v, err := strconv.ParseInt(sec["award_amount_mini"], 10, 64); err != nil {
				mlog.Warn("Jackpot award_amount_mini parse error: %v", err)
			} else {
				jp.AwardAmount[JackpotLevelMini] = v
			}

			if v, err := strconv.ParseInt(sec["award_amount_minor"], 10, 64); err != nil {
				mlog.Warn("Jackpot award_amount_minor parse error: %v", err)
			} else {
				jp.AwardAmount[JackpotLevelMinor] = v
			}

			if v, err := strconv.ParseInt(sec["allowed_bet_mini"], 10, 64); err != nil {
				mlog.Warn("Jackpot allowed_bet_mini parse error: %v", err)
			} else {
				jp.AllowedBetAmount[JackpotLevelMini] = v
			}

			if v, err := strconv.ParseInt(sec["allowed_bet_minor"], 10, 64); err != nil {
				mlog.Warn("Jackpot allowed_bet_minor parse error: %v", err)
			} else {
				jp.AllowedBetAmount[JackpotLevelMinor] = v
			}

			if v, err := strconv.ParseInt(sec["allowed_bet_grand"], 10, 64); err != nil {
				mlog.Warn("Jackpot allowed_bet_grand parse error: %v", err)
			} else {
				jp.AllowedBetAmount[JackpotLevelGrand] = v
			}
		}

		glc.Jackpot = jp
	}

	go func(c chan *pb.LHDTableDashboard, m *sync.Map) {
		for e := range c {
			SubscriperLen := 0
			m.Range(func(key, value interface{}) bool {
				SubscriperLen++
				ch := value.(chan *pb.LHDTableDashboard)
				select {
				case ch <- e:

				default:
					mlog.Error("Dashboard subscriper channel blocking, subscriper channel length %d", len(ch))
				}
				return true
			})
			mlog.Debug("BroadcastSubscriper length %d", SubscriperLen)
		}
	}(glc.EventChan, glc.BroadcastSubscriper)

	glc.addCmd()
	glc.RegisterAfterGameStartFunc(func() {

		if glc.Disabled {
			for _, user := range glc.AllGameUser() {
				if !user.IsRobot() {
					glc.KickUserParamFunc(user.UserID, "", roomutil.MsgParamTableDisabled)
				}
			}
		}

		glc.ReadJackpotPoolNoLink()

		glc.clearGameData() //清除数据

		glc.queryBetSummary()

		glc.getAssignReward()
		//glc.VipUserService.UpdateVIPUser()
		//glc.getTableBlackUser()
		//glc.getTableWhiteUser()
		glc.fapai()
		glc.huangZhuang() //6/12 移除上莊機制
		glc.getLuckyStar()
		glc.getRichestList()
		glc.getGameRound()
		if glc.publiczhuang == 1 {
			glc.GameLog.AddGameStep("莊家%d，剩余%d", glc.zhuangID, glc.lastturn)
		}
		glc.zhuShui()
		glc.broadCastZhuangInfo()
		glc.startAnimation()
	})
	glc.RegisterBeforeGameEndFunc(func() {

	})
	glc.RegisterAfterGameEndFunc(func() {
		glc.saveLastGameUserBet()
	})

	// go func() {

	// 	ctx, cancel := context.WithCancel(context.Background())
	// 	defer cancel()

	// 	for {
	// 		select {
	// 		case <-time.After(100 * time.Millisecond):
	// 			if glc.GameStartFunc != nil {
	// 				mlog.Debug("Table[%d] First Game Start Round[%d]\n", glc.TableManager.TableNo, glc.roundid)
	// 				glc.GameStartFunc()
	// 				cancel()
	// 			}
	// 		case <-ctx.Done():
	// 			return
	// 		}
	// 	}
	// }()

	return glc
}

// 強制设为新牌局 重新发牌
func (lh *LongHudouLogic) forceNewRoundFapai() {
	lh.roundid = 0
	lh.newcard = true
	lh.fapai()
}

// 发牌
func (lh *LongHudouLogic) fapai() {
	mlog.Debug("roundID:%d", lh.roundid)
	if lh.roundid == 0 || lh.newcard {
		lh.poker = poker.NewRandomPoker(false, false, 2) //初始化一副牌
		lh.poker.Shuffle()
		//直接发所有的牌
		lh.cards = lh.poker.Deal(0, 104)
		lh.newcard = false //發完牌了，重置狀態
	} else {
		lh.poker.Shuffle()
	}
	mlog.Debug("len(card):%d", len(lh.cards.Cards))
}

// 添加命令行
func (lh *LongHudouLogic) addCmd() {
	command.AddCommand(NewLhdCmd(lh))
}

// 广播莊家、大富豪、幸運星
func (lh *LongHudouLogic) broadCastZhuangInfo() {
	lh.BoardcastMessage(lh.getZhuangInfoMsg())
	lh.BoardcastMessage(lh.getRichestListMsg())
	lh.BoardcastMessage(lh.getLuckyStarMsg())
	lh.BoardcastMessage(lh.getJackpotMsg())
}

// 注水到下注额
func (lh *LongHudouLogic) zhuShui() {
	lh.zhushuimoney = 0
	lh.GameLog.AddGameStep("系統水池%d, 贏錢線%d", lh.SysPool.GetPool(), lh.SysPool.GetWinLine())
}

// 用户离开
func (lh *LongHudouLogic) UserLeave(gameuser *gamelogic.GameUser) bool {
	if lh.gamestate != Wait {
		if lh.getUserDataByID(gameuser.UserID) != nil {
			mlog.Debug("用户在玩游戏还不能离开")
			return false
		}

		if lh.zhuangID == gameuser.UserID {
			mlog.Debug("用户在当莊，所以用户不能离开")
			return false
		}
	}

	lh.delFromZhuangList(gameuser.UserID)
	lh.ChangeUserStateEnd(gameuser)
	lh.EventChan <- lh.MakeDashboard()
	return lh.ComputeGameLogicBase.UserLeave(gameuser)
}

// 注销用户实例
func (lh *LongHudouLogic) UserFree(gameuser *gamelogic.GameUser) {
	if lh.gamestate != Wait {
		if lh.getUserDataByID(gameuser.UserID) != nil {
			mlog.Debug("用户在玩游戏还不能离开")
			return
		}

		if lh.zhuangID == gameuser.UserID {
			mlog.Debug("用户在当莊，所以用户不能离开")
			return
		}
	} else {
		if lh.zhuangID == gameuser.UserID {
			lh.zhuangID = 0
		}
	}

	lh.delFromZhuangList(gameuser.UserID)

	lh.ChangeUserStateEnd(gameuser)
	lh.ComputeGameLogicBase.UserFree(gameuser)
	lh.EventChan <- lh.MakeDashboard()
}

// 断线重连
func (lh *LongHudouLogic) UserRecover(gameuser *gamelogic.GameUser) {
	mlog.Debug("UserRecover")
	lh.ComputeGameLogicBase.UserRecover(gameuser)
	ud := lh.getUserDataByID(gameuser.UserID)
	if ud != nil {
		ud.money += ud.user.Score - ud.moneylast
		ud.moneylast = ud.user.Score
	}

	lh.sendScene(gameuser)
	lh.EventChan <- lh.MakeDashboard()
}

// 用户进入
func (lh *LongHudouLogic) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	if lh.Disabled && !gameuser.IsRobot() {
		lh.KickUserParamFunc(gameuser.UserID, "", roomutil.MsgParamTableDisabled)
		gameuser.ConnInfo.Close()
		return
	}
	lh.checkBlock(gameuser)
	lh.ComputeGameLogicBase.UserEnter(gameuser, tableno, seatno)
	mlog.Debug("UserEnter")
	lh.sendScene(gameuser)
	lh.EventChan <- lh.MakeDashboard()

}

// 发送场景
func (lh *LongHudouLogic) sendScene(gameuser *gamelogic.GameUser) {
	//发送设置

	if gameuser.IsRobot() {
		mlog.Debug("给机器人%d发送场景消息", gameuser.UserID)
	} else {
		mlog.Debug("给玩家%d发送场景消息", gameuser.UserID)
	}
	lh.sendSetting(gameuser)
	lh.SendMessageToUser(gameuser, lh.getRoundMsg())
	lh.SendMessageToUser(gameuser, lh.getResultCardListMsg())
	lh.SendMessageToUser(gameuser, lh.getRichestListMsg())
	lh.SendMessageToUser(gameuser, lh.getLuckyStarMsg())
	lh.SendMessageToUser(gameuser, lh.getJackpotMsg())
	lh.SendMessageToUser(gameuser, lh.getJackpotSettingMsg())
	if lh.gamestate == Wait {
		return
	}
	//发送阶段
	lh.SendMessageToUser(gameuser, lh.getStateMsg())
	lh.SendMessageToUser(gameuser, lh.getZhuangInfoMsg())
	lh.SendMessageToUser(gameuser, lh.getBetTotalMsg(0))
	lh.sendUserBetInfo(gameuser)

	if lh.gamestate == KaiJiangAction {
		//发送开奖信息
		lh.SendMessageToUser(gameuser, lh.getGameResultPokerMsg())
	} else if lh.gamestate == EndAction {
		//发送用户下注输赢信息如果有
		ud := lh.getUserDataByID(gameuser.UserID)
		if ud != nil {
			lh.SendMessageToUser(gameuser, lh.getUserWinMsg(ud.getShowWinLose(), lh.zhuangWin, ud))
		}
	}
}

// 发送设置
func (lh *LongHudouLogic) sendSetting(gameuser *gamelogic.GameUser) {
	mlog.Debug("sendSetting")

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDSettingID)

	pdata := new(netproto.LHD_Setting)
	pdata.BetCount = append(pdata.BetCount, lh.betcount...)
	pdata.BetMoney = proto.Int32(int32(lh.betvalue.getMoneyByType(1)))
	pdata.ZhuangLimit = proto.Int64(lh.zhuanglimit)
	pdata.Betlimitmoney = proto.Int64(lh.betlimitmoney)
	pdata.UserMaxBet = proto.Int64(lh.betvalue.getUserMaxBet())

	for i := 0; i < len(lh.betvalue.betvalue); i++ {
		pdata.BetMax = append(pdata.BetMax, lh.betvalue.betvalue[i].maxmoney)

		var odd float32 = 1 + (float32(lh.betvalue.betvalue[i].odd)-1)*float32(100-lh.GetServerConfig().TaxScale)/100

		pdata.BetAreas = append(pdata.BetAreas, &netproto.BetArea{
			Type: proto.Int32(lh.betvalue.betvalue[i].longhudoutype),
			Min:  proto.Int64(lh.betlimitmoney),
			Max:  proto.Int64(lh.betvalue.betvalue[i].maxmoney),
			Odds: proto.Float32(odd),
		})
	}

	pmsg.MsgData = pdata
	lh.SendMessageToUser(gameuser, pmsg)
}

// 发送用户的下注信息
func (lh *LongHudouLogic) sendUserBetInfo(gameuser *gamelogic.GameUser) {
	ud := lh.getUserDataByID(gameuser.UserID)
	if ud == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDBetInfoID)

	pdata := new(netproto.LHD_UserBetInfo)
	pdata.UserMoney = proto.Int64(ud.money)

	lucky := lh.getUserDataByID(lh.luckystar) //獲取明燈

	for _, v := range ud.userbet {
		betinfo := new(netproto.LHD_BroadCastBet)
		betinfo.Type = proto.Int32(v.longhudoutype)
		betinfo.Money = proto.Int64(v.money)
		betinfo.LuckyStar = proto.Bool(false)
		if lucky != nil && lucky.getUserBetByType(v.longhudoutype) > 0 {
			betinfo.LuckyStar = proto.Bool(true)
		}
		pdata.BetInfo = append(pdata.BetInfo, betinfo)
	}
	pmsg.MsgData = pdata
	lh.SendMessageToUser(gameuser, pmsg)
}

// 清除数据
func (lh *LongHudouLogic) clearGameData() {
	lh.userdata = make([]*longhudouuserdata, 0)
	lh.bettypetotal = make([]*bettype, 0)
	lh.bettyperobot = make([]*bettype, 0)
	lh.cardresult = make([]*card, 0)
	lh.winuserinfo = make([]int32, 0)
	lh.stageuserinfo = make([]int32, 0)
	lh.richestuserinfo = make([]int32, 0)
	lh.richestList = make([]*netproto.LHD_Richest, 0)
	lh.bettotal = 0
	lh.bettotalrobot = 0
	lh.zhuangWin = 0
	lh.currtax = 0
	lh.robotwinlose = 0
	lh.userwinlose = 0
	lh.uservalidamount = 0
	lh.usertaxamount = 0
	lh.jiangtype = 0
	lh.assignRewardDetail = nil
	lh.execAssignReward = false
	lh.execFangFen = false
	rand.Seed(time.Now().UnixNano())
	zlen := len(lh.zhuanglist)
	if zlen == 0 {
		lh.robotZhuang()
	}

	lh.robotXiaZhuang()

	lh.CheatFlag = false
	lh.blockList = []int32{}

	if lh.Jackpot != nil {
		lh.Jackpot.Assignment = nil
	}
}

// 机器人下莊
func (lh *LongHudouLogic) robotXiaZhuang() {
	if lh.zhuangID == 0 {
		return
	}

	ur := lh.GetUserByUserID(lh.zhuangID)
	if ur != nil && ur.IsRobot() {
		if rand.Int31n(100) < lh.robotxiazhuang {
			lh.zhuangID = 0
		}
	}
}

// 机器人上莊
func (lh *LongHudouLogic) robotZhuang() {
	if lh.publiczhuang == 1 {
		//概率当莊
		if rand.Int31n(100) > lh.robotzhuang {
			return
		}
		//统计机器人个数
		robotarr := make([]int32, 0)
		for _, ur := range lh.AllTableUser() {
			if ur.IsRobot() {
				robotarr = append(robotarr, ur.UserID)
			}
		}

		//沒有機器人或沒有設定最多幾個機器人上莊
		if len(robotarr) <= 0 || lh.maxrobotzhuang <= 0 {
			return
		}

		//随机有几个人当莊
		n := rand.Int31n(lh.maxrobotzhuang) + 1

		for i := int32(0); i < n; i++ {
			rcn := len(robotarr)
			if rcn <= 0 {
				break
			}
			index := rand.Int31n(int32(rcn))
			lh.addToZhuangList(robotarr[index])
			robotarr = append(robotarr[:index], robotarr[index+1:]...)
		}
	} else {
		mlog.Debug("未開啟上莊機制，機器人不用上莊")
		return
	}
}
func (lh *LongHudouLogic) startAnimation() {
	//開始下注前+2秒
	lh.EventChan <- lh.MakeDashboard()
	lh.RegisterAnimation(GameStartAnimation, int32(2), lh.startBet)
}

// 开始下注
func (lh *LongHudouLogic) startBet() {
	lh.opttimer = lh.TimerFuncManager.NewTimer(lh.bettime, lh.kaiJiang)

	lh.setState(Bet)
	lh.BoardcastMessage(lh.getStateMsg())
}

// 开始开奖
func (lh *LongHudouLogic) kaiJiang() {

	if lh.CheatFlag {
		lh.cheat(lh.cheatKickCnt)
		lh.CheatFlag = false
		lh.cheatKickCnt = 0
	} else if lh.SpecialRewardCount > 0 {
		lh.specialReward()
	}

	// 統計所有玩家下注
	for _, ud := range lh.userdata {
		ud.betAmount = 0

		betMap := make(map[int32]int64)
		for _, bet := range ud.userbet {
			betMap[bet.longhudoutype] += bet.money
			ud.betAmount += bet.money
		}

		isRobot := ""
		if ud.user.IsRobot() {
			isRobot = "[機器人]"
			continue
		}

		lh.GameLog.AddGameStep("%s玩家[%d] 押注總計: %d, 龍: %d, 虎: %d, 和: %d", isRobot, ud.user.UserID, ud.betAmount, betMap[LONG], betMap[HU], betMap[HE])
	}

	lh.setState(KaiJiang)

	if lh.zhuangID != 0 {
		if lh.GetUserByUserID(lh.zhuangID).IsRobot() {
			lh.removeRobotBet()
		}
	} else {
		lh.removeRobotBet()
	}

	lh.getResultType()
	lh.countGameRound()
	lh.getPokerByType()

	if lh.Jackpot != nil && lh.Jackpot.Assignment != nil && lh.Jackpot.Assignment.Type != AssignTypeNone {
		switch lh.Jackpot.Assignment.Type {
		case AssignTypeRandom: // 隨機玩家獲得
			// 決定隨機玩家
			// 玩家下注未超過限制則不派發
			var uList []int32
			for _, ud := range lh.userdata {
				if ud.user.IsRobot() {
					continue
				}

				var betAmt int64
				for _, bet := range ud.userbet {
					betAmt += bet.money
				}

				if betAmt >= lh.Jackpot.AllowedBetAmount[lh.Jackpot.Assignment.Level] {
					uList = append(uList, ud.user.UserID)
				}
			}

			example.invalid("符合下注條件玩家共 %d 人", len(uList))

			if len(uList) == 0 {
				mlog.Warn("[龍虎斗] 無玩家下注超過限制，不派發彩金")
			} else {
				lh.Jackpot.Assignment.UID = uList[rand.Intn(len(uList))]
				example.invalid("指定隨機玩家[%d]獲得 %v ", lh.Jackpot.Assignment.UID, lh.Jackpot.Assignment.Level)
			}

		case AssignTypeRecycle: // 回收
			award := lh.Jackpot.AwardAmount[lh.Jackpot.Assignment.Level]
			if lh.Jackpot.Assignment.Level == JackpotLevelGrand {
				award = lh.Jackpot.Current()
			}
			contribute := lh.Jackpot.Contribute
			lh.Jackpot.Contribute -= award * 10000
			if lh.Jackpot.Contribute < 0 {
				lh.Jackpot.Reset()
			}
			lh.UpdateJackpotPoolNoLink(lh.Jackpot.Contribute - contribute)
			example.invalid("[龍虎斗] 回收彩金[%v] %d", lh.Jackpot.Assignment.Level, award)
		}
	}

	wg := new(sync.WaitGroup)
	now := time.Now()
	ctx := lh.GameLog.Context()
	ctx.SettleTime = now
	// start := time.Now().Unix()
	for _, ud := range lh.userdata {
		lh.sendUserResult(ud, wg, now)
		example.invalid("[龍虎斗] 玩家[%d] 總押注: %d, 總贏分: %d, 總有效投注: %d", ud.user.UserID, ud.betAmount, ud.winAmount, ud.flowAmount)
	}
	wg.Wait()
	// end := time.Now().Unix()
	// example.invalid("開獎耗時 %d seconds", end-start)
	// fmt.Println(end-start, "seconds")
	example.invalid("[龍虎斗] 開獎耗時 %v", time.Since(now))

	mlog.Debug("getPokerByType done")
	lh.BoardcastMessage(lh.getGameResultPokerMsg())
	mlog.Debug("getGameResultPokerMsg done")
	lh.BoardcastMessage(lh.getRoundMsg())
	mlog.Debug("getRoundMsg done")
	lh.setState(KaiJiangAction)
	mlog.Debug("setState done")
	lh.addResultCardList()
	mlog.Debug("addResultCardList done")

	lh.action_str = lh.RegisterAnimation(GameKaiJiang_Action, lh.kaijiangactiontime, lh.sendResult)
}

// 計算回合
func (lh *LongHudouLogic) countGameRound() {
	if lh.roundid >= 30 {
		lh.roundid = 1
	} else {
		lh.roundid += 1
		if lh.roundid == 30 {
			lh.newcard = true //30場了，重新發牌
			mlog.Debug("重新發牌:%t", lh.newcard)
		}
	}
	if lh.jiangtype == LONG {
		lh.longwin += 1
	} else if lh.jiangtype == HE {
		lh.hewin += 1
	} else if lh.jiangtype == HU {
		lh.huwin += 1
	}
	mlog.Debug("开奖类型：%v", lh.jiangtype)
}

// 添加开奖到历史列表
func (lh *LongHudouLogic) addResultCardList() {
	cards := new(netproto.LHD_Cards)
	for i := 0; i < len(lh.cardresult); i++ {
		card := new(netproto.LHD_Poker)
		card.Color = proto.Int32(lh.cardresult[i].color)
		card.Number = proto.Int32(lh.cardresult[i].number)
		cards.Cards = append(cards.Cards, card)
	}
	cards.RoundID = proto.Int32(lh.roundid)

	if lh.assignRewardDetail != nil {
		example.invalid("===========%v", lh.jiangtype)
		lh.assignRewardDetailResult(fmt.Sprintf("%d", lh.jiangtype))
	}
	lh.resultcardlist.CardList = append(lh.resultcardlist.CardList, cards)
	if len(lh.resultcardlist.CardList) > 100 {
		lh.resultcardlist.CardList = append(lh.resultcardlist.CardList[:0], lh.resultcardlist.CardList[1:]...)
	}
	lh.resultlist = append(lh.resultlist, lh.jiangtype)
	if len(lh.resultlist) > 100 {
		lh.resultlist = append(lh.resultlist[:0], lh.resultlist[1:]...)
	}
}

// 获取开奖历史列表的消息
func (lh *LongHudouLogic) getResultCardListMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDResultCardListID)

	pdata := new(netproto.LHD_ResultCardList)
	if len(lh.resultcardlist.CardList) > 30 {
		pdata.CardList = append(pdata.CardList, lh.resultcardlist.CardList[len(lh.resultcardlist.CardList)-30:]...)
	} else {
		pdata.CardList = append(pdata.CardList, lh.resultcardlist.CardList...)
	}

	//mlog.Debug("getResultCardListMsg:%v", pdata.CardList)
	pmsg.MsgData = pdata
	return pmsg
}

// 获取开奖结果扑克牌消息
func (lh *LongHudouLogic) getGameResultPokerMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDGameResultPokerID)

	pdata := new(netproto.LHD_GameResultPoker)
	for _, v := range lh.cardresult {
		cards := new(netproto.LHD_Poker)
		cards.Color = proto.Int32(v.color)
		cards.Number = proto.Int32(v.number)
		pdata.Poker = append(pdata.Poker, cards)
	}
	pdata.RoundID = proto.Int32(lh.roundid)

	pmsg.MsgData = pdata
	return pmsg
}

// 获取大富豪列表的消息
func (lh *LongHudouLogic) getRichestListMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDRichestListID)
	pdata := new(netproto.LHD_RichestList)
	pdata.RichestList = lh.richestList
	//如果當下沒有玩家 回傳空值
	pmsg.MsgData = pdata
	return pmsg
}

// 获取幸運星的消息
func (lh *LongHudouLogic) getLuckyStarMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDLuckyStarListID)
	luckyStarList := new(netproto.LHD_LuckyStar)
	luckyStarList.UserID = proto.Int32(lh.luckystar)
	luckyStarList.UserFaceID = proto.Int32(lh.luckyfaceid)
	pmsg.MsgData = luckyStarList
	return pmsg
}

func (lh *LongHudouLogic) getJackpotMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDJackpotID)

	pdata := new(netproto.LHD_JackpotInfo)
	pdata.Money = proto.Int64(lh.Jackpot.Current())

	phase, rate := lh.Jackpot.Phase()
	pdata.Phase = proto.Int32(int32(phase))
	pdata.PhaseRate = proto.Int32(int32(rate))

	pmsg.MsgData = pdata

	return pmsg
}

func (lh *LongHudouLogic) getJackpotSettingMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDJackpotSettingID)

	pdata := new(netproto.LHD_JackpotSetting)
	pdata.Enabled = proto.Bool(lh.Jackpot.Enabled)

	pdata.Money = append(pdata.Money, &netproto.LHD_JackpotOption{
		Type:  netproto.LHD_JackpotType_LHD_JackpotType_Mini.Enum(),
		Money: proto.Int64(lh.Jackpot.AwardAmount[JackpotLevelMini]),
	})
	pdata.Money = append(pdata.Money, &netproto.LHD_JackpotOption{
		Type:  netproto.LHD_JackpotType_LHD_JackpotType_Minor.Enum(),
		Money: proto.Int64(lh.Jackpot.AwardAmount[JackpotLevelMinor]),
	})
	pdata.Money = append(pdata.Money, &netproto.LHD_JackpotOption{
		Type:  netproto.LHD_JackpotType_LHD_JackpotType_Grand.Enum(),
		Money: proto.Int64(-1),
	})

	pdata.AllowdBet = append(pdata.AllowdBet, &netproto.LHD_JackpotOption{
		Type:  netproto.LHD_JackpotType_LHD_JackpotType_Mini.Enum(),
		Money: proto.Int64(lh.Jackpot.AllowedBetAmount[JackpotLevelMini]),
	})
	pdata.AllowdBet = append(pdata.AllowdBet, &netproto.LHD_JackpotOption{
		Type:  netproto.LHD_JackpotType_LHD_JackpotType_Minor.Enum(),
		Money: proto.Int64(lh.Jackpot.AllowedBetAmount[JackpotLevelMinor]),
	})
	pdata.AllowdBet = append(pdata.AllowdBet, &netproto.LHD_JackpotOption{
		Type:  netproto.LHD_JackpotType_LHD_JackpotType_Grand.Enum(),
		Money: proto.Int64(lh.Jackpot.AllowedBetAmount[JackpotLevelGrand]),
	})

	pmsg.MsgData = pdata

	return pmsg
}

// 获取开奖类型。
func (lh *LongHudouLogic) getResultType() {
	//lh.bowProvider = gamelogic.NewBlackOrWhiteUserProvider(lh.VipUserProvider)
	superToolHandler := lh.SuperToolService.GetBrGameSuperToolHandler()

	//指定开奖
	if lh.nextjiangtype != 0 {
		lh.jiangtype = lh.nextjiangtype
		example.invalid("指定開獎:%s", resultstring[lh.jiangtype])
		lh.GameLog.AddGameStep("指定開獎[%v]", resultstring[lh.jiangtype])
		lh.nextjiangtype = 0
		return
	} else if ok, rettype := lh.isAssignReward(); ok {
		lh.execAssignReward = true
		lh.jiangtype = rettype
		return
	} else if lh.fangFenTarget > 0 && lh.fangFenComplete < lh.fangFenTarget { //系統放分
		lh.execFangFen = true
		alltype := lh.getFangFenKaiJiangType()
		lh.jiangtype = lh.betvalue.getJiangType(alltype)
		lh.GameLog.AddGameStep("系統放分 : target %d, already %d", lh.fangFenTarget, lh.fangFenComplete)
		return
	} else if len(lh.getBowBet()) > 0 { //監控放殺分
		if lh.bowJiangType() {
			example.invalid("執行監控放殺分策略 : %s", superToolHandler.GetProiorityListString())
			lh.GameLog.AddGameStep("執行監控放殺分策略 : %s", superToolHandler.GetProiorityListString())
			return
		}
	}

	t := 0
	//玩家上莊且機器人下注
	if lh.zhuangID != 0 && !lh.GetUserByUserID(lh.zhuangID).IsRobot() && len(lh.bettyperobot) > int(0) {
		if lh.SysPool.GetPool() < lh.SysPool.GetWinLine() {
			eat := util.RndInt(1, 5)
			mlog.NewDefaultLogger().System("水池小於贏錢線就1/2吃玩家, rand 1-4[%d] 3以上吃", eat)
			if eat >= 3 {
				t = 1
			}
		} else {
			eat := util.RndInt(1, 4)
			mlog.NewDefaultLogger().System("水池大於贏錢線就1/3吃玩家, rand 1-3[%d] 3以上吃", eat)
			if eat >= 3 {
				t = 1
			}
		}

	}

	//机器人和系统当莊的时候方案一
	if t == 0 {
		var alltype []int32

		//水池低於贏錢線60％開系統有利獎
		if lh.SysPool.IsCtrlSystemWin() {
			alltype = lh.getMinKaiJiangType()
			lh.GameLog.AddGameStep("水線之下, 開獎:%v", alltype)
			mlog.Warn("水線之下, 開獎:%v", alltype)
		}
		if len(alltype) == 0 {
			alltype = lh.getAllKaiJiangType()
		}
		lh.jiangtype = lh.betvalue.getJiangType(alltype)
	} else {
		mlog.NewDefaultLogger().System("吃玩家錢")
		lh.jiangtype = lh.getMaxRobotJiang()
	}

}

// 获取机器人赢最多的开奖方案
func (lh *LongHudouLogic) getMaxRobotJiang() int32 {
	maxtype := int32(0)
	maxmoney := int64(0)
	for _, rtd := range lh.bettyperobot {
		money := int64(lh.betvalue.getOddByType(rtd.longhudoutype)) * rtd.money
		if money >= maxmoney {
			maxtype = rtd.longhudoutype
			maxmoney = money
		}
	}

	return maxtype
}

// 获取机器人赢錢
func (lh *LongHudouLogic) getRobotWin() int64 {
	win := int64(0)
	for _, rtd := range lh.bettyperobot {
		if rtd.longhudoutype == lh.jiangtype {
			win = rtd.money * int64(lh.betvalue.getOddByType(lh.jiangtype))
			break
		}
	}

	return win
}

// 给玩家发送结果
func (lh *LongHudouLogic) sendResult() {
	lh.calWinLose()

	//lh.writePoolToFile()
	lh.updateVipUserWin()

	lh.action_str = lh.RegisterAnimation(GameEnd_Action, lh.endactiontime, lh.gameEnd)
}

// 計算下注輸贏結果
func (lh *LongHudouLogic) calWinLose() {
	sysTaxIncomeBefore := lh.systaxincome
	lh.syslastwin = 0
	lh.setState(EndAction)

	var flowtypestr string
	switch lh.FlowType {
	case 0:
		flowtypestr = "輸錢流水"
	case 1:
		flowtypestr = "有效投注流水"
	}
	lh.GameLog.AddGameStep("流水模式: %s", flowtypestr)

	// 做一個 map 紀錄一下
	uDataMap := make(map[int32]bool)
	for _, ud := range lh.userdata {
		uDataMap[ud.user.UserID] = true
	}
	// 移除不存在 userdata 中的 gamelog 玩家
	for uid := range lh.GameLog.Context().Users {
		if !uDataMap[uid] {
			lh.GameLog.Context().RemoveUser(uid)
		}
	}

	changeSysPoolValue := int64(0)
	if lh.zhuangID == 0 {
		//系統小妹當莊的時候，玩家贏錢水池減少
		if lh.userwinlose > 0 {
			//玩家贏錢，水線下降
			//lh.syspool -= lh.userwinlose - lh.zhushuimoney
			changeSysPoolValue = -lh.userwinlose
		} else {
			//玩家輸錢，水線上升(稅)
			//lh.syspool -= int64(math.Floor(float64(lh.userwinlose)*lh.GetTax())) - lh.zhushuimoney
			changeSysPoolValue = -lh.userwinlose
		}
	} else {
		//這裡計算莊家輸贏及稅收
		var income, tax int64
		if lh.bettotal > int64(0) { //賺錢要抽稅
			income, tax = lh.GetUserWinAndTax(lh.bettotal)
			lh.systaxincome += tax
			lh.GameLog.AddGameStep("莊家贏錢 %d, 抽稅 %d", income, tax)

		} else {
			income = lh.bettotal
		}

		//這裡計算水池, 系統小妹當莊
		lh.systotalwin += income
		lh.syslastwin += income
		lh.zhuangWin += income

		ur := lh.GetUserByUserID(lh.zhuangID)
		if ur != nil {
			finalBetTotal := lh.bettotal + lh.bettotalrobot
			income, tax = lh.GetUserWinAndTax(finalBetTotal)
			//機器人當莊的時候
			if ur.IsRobot() {
				lh.zhuangWin = income
				lh.systaxincome = tax
				lh.systotalwin = income
				//玩家贏錢水池減少，並將注出的水倒回水池
				if lh.userwinlose > 0 {
					//玩家贏錢，水線下降
					//lh.syspool -= lh.userwinlose - lh.zhushuimoney
					changeSysPoolValue = -lh.userwinlose
				} else {
					//玩家輸錢，水線上升(稅)
					//lh.syspool -= int64(math.Floor(float64(lh.userwinlose)*lh.GetTax())) - lh.zhushuimoney
					changeSysPoolValue = -lh.userwinlose
				}
			} else {
				//玩家當莊的時候，機器人贏錢水池就添加，並將注出的水倒回水池
				if lh.robotwinlose > 0 {
					//機器人贏錢 水位上升(稅)
					//lh.syspool += int64(math.Floor(float64(lh.robotwinlose)*lh.GetTax())) - lh.zhushuimoney
					changeSysPoolValue = lh.robotwinlose
				} else {
					//lh.syspool += lh.robotwinlose - lh.zhushuimoney
					changeSysPoolValue = lh.robotwinlose
				}
			}
			lh.UpdateUserScoreFunc(ur.UserID, income, tax) // 更新金幣異動
			lh.SendMessageToUser(ur, lh.getUserWinMsg(lh.zhuangWin, lh.zhuangWin, nil))
		} else {
			mlog.Debug("莊家数据为空")
		}
	}
	if !lh.execFangFen {
		//example.invalid("系统水池異動, from %d, change %d", lh.SysPool.GetPool(), changeSysPoolValue)
		//lh.GameLog.AddGameStep("系统水池異動, from %d, change %d", lh.SysPool.GetPool(), changeSysPoolValue)
		log := lh.SysPool.UpdateSysPoolAndTax(changeSysPoolValue, lh.usertaxamount, lh.uservalidamount)
		lh.GameLog.AddGameStep(log)
	} else {
		before := lh.fangFenComplete
		changeSysPoolValue = -changeSysPoolValue //水池下降代表玩家贏錢
		if changeSysPoolValue > 0 {              //玩家贏錢要抽稅
			changeSysPoolValue = int64(math.Floor(float64(changeSysPoolValue) * lh.GetTax()))
		} else { //玩家輸錢要把稅算回來
			changeSysPoolValue = int64(math.Floor(float64(changeSysPoolValue) / lh.GetTax()))
		}
		lh.fangFenComplete += changeSysPoolValue
		lh.GameLog.AddGameStep("系統放分完成, target %d, already %d-%d", lh.fangFenTarget, before, lh.fangFenComplete)
		example.invalid("系統放分完成, target %d, already %d-%d", lh.fangFenTarget, before, lh.fangFenComplete)
	}
	mlog.Debug("莊家赢錢：%v", lh.zhuangWin)
	lh.GameLog.AddGameStep("本局稅收 %d", lh.systaxincome-sysTaxIncomeBefore)
	//發送沒下注的user 莊家結果
	for _, u2 := range lh.AllTableUser() {
		sendBool := true
		for _, ud := range lh.userdata {
			if u2.UserID == ud.user.UserID {
				sendBool = false
				break
			}
		}
		if sendBool {
			lh.SendMessageToUser(u2, lh.getUserWinMsg(0, lh.zhuangWin, nil))
			if u2.UserID == lh.luckystar {
				if lh.getUserDataByID(lh.luckystar) == nil {
					lh.notbettimes += 1
				}
			}
		}
	}
	//發送下注user結算結果
	uscoresRet := make(map[int32]int64)
	for _, ud := range lh.userdata {
		lh.SendMessageToUser(ud.user, lh.getUserWinMsg(ud.getShowWinLose(), lh.zhuangWin, ud))
		uscoresRet[ud.user.UserID] = ud.updatemoney
	}
	uscoresRet[lh.zhuangID] = lh.zhuangWin
	lh.GameLog.GameEnd()
	lh.bettotal = 0
}

// 给每个玩家发送开奖结果
func (lh *LongHudouLogic) sendUserResult(ud *longhudouuserdata, wg *sync.WaitGroup, settleTime time.Time) {
	var updateAmount, flowAmount, betAmount, rtnAmount, winAmount, taxAmount, effectiveBetAmount int64

	betMap := make(map[int32]int64)
	for _, bet := range ud.userbet {
		betMap[bet.longhudoutype] += bet.money
	}

	// 依押注類型計算
	for bettype, money := range betMap {
		betAmount += money
		effectiveBetAmount += money
		var isWin, isReturn bool
		if bettype == lh.jiangtype { // 此注類型有開
			isWin = true
			isReturn = true
			win := int64(lh.betvalue.getOddByType(bettype))*money - money // 設定中的賠率包含押注金額
			income, tax := lh.GetUserWinAndTax(win)

			winAmount += win
			rtnAmount += money
			rtnAmount += income
			taxAmount += tax
		} else if lh.jiangtype == HE && (bettype == LONG || bettype == HU) { // 開和 [莊] [閒]

			switch lh.HEandKill {
			case HEandBack: // 開和全退
				isReturn = true
				rtnAmount += money
				effectiveBetAmount -= money
				if lh.FlowType == 1 {
					flowAmount -= money
				}
			case HEandKillHalf: // 開和退一半
				isReturn = true
				rtnAmount += money / 2
				flowAmount += money / 2
				effectiveBetAmount -= money / 2
				if lh.FlowType == 1 {
					flowAmount -= money
				}
			}
		}

		switch lh.FlowType {
		case 0: // 輸錢流水
			// 沒贏沒退 = 輸 算流水
			if !isWin && !isReturn {
				flowAmount += money
			}
		case 1: // 有效投注流水
			flowAmount += money
		}
	}

	var (
		jplv       JackpotLevel
		jpamt      int64
		contribute int64
		jpmsg      string
	)

	// Check Jackpot Award
	if lh.Jackpot != nil && !ud.user.IsRobot() && lh.Jackpot.Enabled && !lh.Jackpot.AwardPause && lh.Jackpot.Awardable() {
		example.invalid("Jackpot.Assignment %+v", lh.Jackpot.Assignment)
		if lh.Jackpot.Assignment == nil {
			jplv, jpamt, contribute = lh.Jackpot.Award(betAmount, lh.betvalue.getUserMaxBet())
			example.invalid("Award jplv: %v, jpamt: %v, contribute: %v", jplv, jpamt, contribute)
			ud.JackpotLevel = jplv
			ud.JackpotAmount = jpamt
			lh.UpdateJackpotPoolNoLink(contribute)
		} else if lh.Jackpot.Assignment.UID == ud.user.UserID {
			jplv, jpamt, contribute = lh.Jackpot.AwardByAssigned(lh.Jackpot.Assignment.Level)
			example.invalid("AwardByAssigned jplv: %v, jpamt: %v, contribute: %v", jplv, jpamt, contribute)
			ud.JackpotLevel = jplv
			ud.JackpotAmount = jpamt
			lh.UpdateJackpotPoolNoLink(contribute)
		}
		jpmsg = fmt.Sprintf("Jackpot[%v] %d, ", jplv, jpamt)
	}

	rtnAmount += jpamt

	updateAmount = rtnAmount - betAmount

	mlog.Debug("User[%d] UpdateAmount: %d, BetAmount: %d, WinAmount: %d, ReturnAmount: %d, FlowAmount: %d, TaxAmount: %d", ud.user.UserID, updateAmount, betAmount, winAmount, rtnAmount, flowAmount, taxAmount)

	// for i := 0; i < len(ud.userbet); i++ {
	// 	betAmount += util.ABSInt64(ud.userbet[i].money)
	// 	betlosemoney += ud.userbet[i].money
	// 	if ud.userbet[i].longhudoutype == lh.jiangtype {
	// 		win := int64(lh.betvalue.getOddByType(lh.jiangtype)) * ud.userbet[i].money
	// 		//机器人不计算奖池里面的錢
	// 		if lh.zhuangID == 0 {
	// 			if !ud.user.IsRobot() {
	// 				lh.bettotal -= win
	// 			}
	// 		} else {
	// 			ur := lh.GetUserByUserID(lh.zhuangID)
	// 			if ur != nil && ur.IsRobot() && ud.user.IsRobot() {
	// 				lh.bettotalrobot -= win
	// 			} else {
	// 				lh.bettotal -= win
	// 			}
	// 		}

	// 		ud.money += win
	// 	} else if lh.jiangtype == int32(HE) {
	// 		if lh.HEandKill == HEandBack {
	// 			//開和全退
	// 			win := ud.userbet[i].money
	// 			ud.win = win
	// 			//机器人不计算奖池里面的錢
	// 			if lh.zhuangID == 0 {
	// 				if !ud.user.IsRobot() {
	// 					lh.bettotal -= win
	// 				}
	// 			} else {
	// 				ur := lh.GetUserByUserID(lh.zhuangID)
	// 				if ur != nil && ur.IsRobot() && ud.user.IsRobot() {
	// 					lh.bettotalrobot -= win
	// 				} else {
	// 					lh.bettotal -= win
	// 				}
	// 			}
	// 			ud.money += win
	// 		} else if lh.HEandKill == HEandKillHalf {
	// 			//開和退一半
	// 			win := ud.userbet[i].money / 2
	// 			//机器人不计算奖池里面的錢
	// 			if lh.zhuangID == 0 {
	// 				if !ud.user.IsRobot() {
	// 					lh.bettotal -= win
	// 				}
	// 			} else {
	// 				ur := lh.GetUserByUserID(lh.zhuangID)
	// 				if ur != nil && ur.IsRobot() && ud.user.IsRobot() {
	// 					lh.bettotalrobot -= win
	// 				} else {
	// 					lh.bettotal -= win
	// 				}
	// 			}
	// 			ud.money += win
	// 		}
	// 	}
	// }

	// money := ud.money - ud.user.Score

	lh.systaxincome += taxAmount
	ud.win = rtnAmount

	// 將機器人及玩家輸贏減掉稅
	if ud.user.IsRobot() {
		lh.robotwinlose += updateAmount
	} else {
		lh.userwinlose += updateAmount
		lh.usertaxamount += taxAmount
		lh.uservalidamount += flowAmount
	}

	// var income, tax int64
	// if money > int64(0) {
	// 	income, tax = lh.GetUserWinAndTax(money)
	// 	ud.win = income + betlosemoney
	// 	lh.systaxincome += tax
	// } else {
	// 	income = money
	// 	ud.win = income + betlosemoney
	// }
	// if income < 0 {
	// 	flowAmount = betAmount
	// } else {
	// 	flowAmount = 0
	// }
	// payAmount = betAmount + income

	ud.updatemoney = updateAmount
	stepmsg := "玩家[%d] " + jpmsg + " 下注贏錢: %d, 抽稅: %d, 流水: %d, 有效投注金額: %d, 返還金額: %d, 寫分狀態: %s"
	if ud.user.IsRobot() {
		stepmsg = "[機器人]" + stepmsg
	}

	var scorestate = "成功"

	ud.betAmount = betAmount
	ud.winAmount = rtnAmount + taxAmount
	ud.flowAmount = flowAmount

	ud.user.FlowAmount += flowAmount
	ud.user.BetAmount += betAmount
	ud.user.PayAmount += rtnAmount

	lh.GameLog.Context().UserUpdateScore(ud.user.UserID, updateAmount)
	lh.GameLog.Context().UserBetScore(ud.user.UserID, betAmount)
	lh.GameLog.Context().UserPayoutScore(ud.user.UserID, rtnAmount+taxAmount)
	lh.GameLog.Context().UserValidScore(ud.user.UserID, flowAmount)
	lh.GameLog.Context().UserTaxAmount(ud.user.UserID, taxAmount)

	// Contribute Jackpot
	if lh.Jackpot != nil && lh.Jackpot.Enabled && !ud.user.IsRobot() {
		contribute := lh.Jackpot.AddBet(effectiveBetAmount)
		lh.UpdateJackpotPoolNoLink(contribute)
	}

	//lh.GameLog.AddGameStep("用户%d系統稅收%d", ud.user.UserID, lh.systaxincome)
	// lh.UpdateUserScoreFunc(ud.user.UserID, updateAmount, taxAmount)
	wg.Add(1)
	go func() {
		defer wg.Done()
		if ud.user.IsRobot() { //機器人用舊的結算流程
			lh.UpdateUserScoreFunc(ud.user.UserID, updateAmount, taxAmount)
		} else {

			var transactionNo string

			if err := app.Context.Invoke(func(c apipb.GameBetServiceClient, pm *player.Manager) error {
				p := pm.GetPlayerByUserID(ud.user.UserID)
				req := new(apipb.PlaceBetReq)
				req.AccessToken = [REDACTED]
				req.ServerID = lh.TableManager.Room.ServerID
				req.TradeID = ud.user.TradeID
				req.BetAmount = betAmount

				example.invalid("API 下注 請求 玩家[%d] BetAmount: %d", ud.user.UserID, req.BetAmount)

				res, err := c.PlaceBet(context.Background(), req)
				if err != nil {
					return err
				}
				example.invalid("API 下注 回覆 玩家[%d] TransactionNo: %s, AfterAmount: %d", ud.user.UserID, res.TransactionNo, res.AfterAmount)

				transactionNo = res.TransactionNo
				u := lh.GameLog.Context().Users[ud.user.UserID]
				u.TransactionNo = transactionNo

				return nil
			}); err != nil {
				mlog.Error("Place Bet Error, %v", err)
				lh.KickUserFunc(ud.user.UserID, "System Error ...")
				ud.clearData()
				//lh.delUserData(ud.user) // range slice 裡不能刪除 item 會出問題
				lh.UserLeave(ud.user)
				scorestate = "失敗"
				lh.userwinlose -= updateAmount
				lh.usertaxamount -= taxAmount
				lh.uservalidamount -= flowAmount
			}

			if len(transactionNo) == 0 {
				lh.GameLog.AddGameStep(stepmsg, ud.user.UserID, updateAmount-jpamt, taxAmount, flowAmount, betAmount, rtnAmount, scorestate)
				lh.GameLog.Context().RemoveUser(ud.user.UserID)
				return
			}

			if err := app.Context.Invoke(func(c apipb.GameResultServiceClient) error {

				req := new(apipb.SettlementReq)
				req.PlayerID = ud.user.UserID
				req.ServerID = lh.TableManager.Room.ServerID
				req.TradeID = ud.user.TradeID
				req.UpdateAmount = rtnAmount
				req.TransactionNo = transactionNo
				req.BetAmount = betAmount
				req.ValidAmount = flowAmount
				req.PayoutAmount = rtnAmount
				req.TaxAmount = taxAmount
				req.SettleTime = util.FormatTimeStr(settleTime.UTC())

				example.invalid("API 結算 請求 玩家[%d] PlayerID: %d ServerID: %d TradeID: %d UpdateAmount: %d TransactionNo: %s BetAmount: %d ValidAmount: %d PayoutAmount: %d TaxAmount: %d", ud.user.UserID,
					req.PlayerID,
					req.ServerID,
					req.TradeID,
					req.UpdateAmount,
					req.TransactionNo,
					req.BetAmount,
					req.ValidAmount,
					req.PayoutAmount,
					req.TaxAmount)
				var afterAmount int64
				res, err := c.SettlementV2(context.Background(), req)
				if err != nil { // Marshall 說 settlement 失敗不是真的失敗，不用中斷流程 API 自己會持續跟錢包API 重試 settlement
					mlog.Error("更新玩家金幣錯誤 SettlemmentV2 Error, %v", err)
					lh.GameLog.AddGameStep("更新玩家 [%d] 金幣錯誤 SettlemmentV2 Error, %v", ud.user.UserID, err)
					afterAmount = ud.user.Score + updateAmount
					//return err
				} else {
					afterAmount = res.AfterAmount
				}
				example.invalid("API 結算 回覆 玩家[%d] AfterAmount: %d", ud.user.UserID, afterAmount)

				// 原先 UpdateUserScoreFunc 內做得要加回來
				ud.user.UpdateScore += updateAmount

				// 原先在 roomactionmanager.go 內做得要加回來
				if updateAmount > 0 {
					ud.user.WinCount++
					ud.user.UpdateWinCount++
					ud.user.LastWinLose = 1
				} else if updateAmount == 0 {
					ud.user.DrawCount++
					ud.user.UpdateDrawCount++
					ud.user.LastWinLose = 0
				} else {
					ud.user.LoseCount++
					ud.user.UpdateLoseCount++
					ud.user.LastWinLose = 0
				}

				ud.user.Score = afterAmount

				return nil
			}); err != nil {
				mlog.Error("更新玩家金幣錯誤 %v", err)
				lh.KickUserFunc(ud.user.UserID, "System Error ...")
				ud.clearData()
				//lh.delUserData(ud.user) //range slice 裡不能刪除 item 會出問題
				lh.UserLeave(ud.user)

				scorestate = "失敗"
				lh.userwinlose -= updateAmount
				lh.usertaxamount -= taxAmount
				lh.uservalidamount -= flowAmount
			}
		}

		if !ud.user.IsRobot() {
			lh.GameLog.AddGameStep(stepmsg, ud.user.UserID, updateAmount-jpamt, taxAmount, flowAmount, betAmount, rtnAmount, scorestate)
		}

		//这里写入赢家排名列表
		winSort(ud, lh)
	}()
}

// 这里写入赢家排名列表
func winSort(ud *longhudouuserdata, lhd *LongHudouLogic) {
	badd := false
	if ud.win > 0 {
		if len(lhd.winuserinfo) > 0 {
			for i := 0; i < len(lhd.winuserinfo); i++ {
				u := lhd.getUserDataByID(lhd.winuserinfo[i])
				if u != nil {
					if ud.win > u.win {
						if len(lhd.winuserinfo) >= 3 {
							if i == 0 {
								lhd.winuserinfo[2] = lhd.winuserinfo[1]
								lhd.winuserinfo[1] = lhd.winuserinfo[0]
								lhd.winuserinfo[0] = ud.user.UserID
							} else if i == 1 {
								lhd.winuserinfo[2] = lhd.winuserinfo[1]
								lhd.winuserinfo[1] = ud.user.UserID
							} else {
								lhd.winuserinfo[2] = ud.user.UserID
							}
						} else if len(lhd.winuserinfo) == 2 {
							lhd.winuserinfo = append(lhd.winuserinfo, ud.user.UserID)
							if i == 0 {
								lhd.winuserinfo[2] = lhd.winuserinfo[1]
								lhd.winuserinfo[1] = lhd.winuserinfo[0]
								lhd.winuserinfo[0] = ud.user.UserID
							} else if i == 1 {
								lhd.winuserinfo[2] = lhd.winuserinfo[1]
								lhd.winuserinfo[1] = ud.user.UserID
							}
						} else {
							lhd.winuserinfo = append(lhd.winuserinfo, ud.user.UserID)
							lhd.winuserinfo[1] = lhd.winuserinfo[0]
							lhd.winuserinfo[0] = ud.user.UserID
						}

						badd = true
						break
					}
				}
			}
		}

		if len(lhd.winuserinfo) < 3 && !badd {
			lhd.winuserinfo = append(lhd.winuserinfo, ud.user.UserID)
		}
	}
}

// 获取玩家赢錢消息
func (lh *LongHudouLogic) getUserWinMsg(selfwin int64, zhuangwin int64, ud *longhudouuserdata) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDGameResultWinID)

	pdata := new(netproto.LHD_GameResultUserMoney)
	pdata.ZhuangWin = proto.Int64(zhuangwin)
	pdata.SelfWin = proto.Int64(selfwin)
	for i := 0; i < len(lh.winuserinfo); i++ {
		ud := lh.getUserDataByID(lh.winuserinfo[i])
		if ud == nil {
			continue
		}
		userwin := new(netproto.LHD_WinUerInfo)
		userid := strconv.Itoa(int(ud.user.UserID))
		userwin.UserName = proto.String(userid)
		userwin.Win = proto.Int64(ud.getShowWinLose())
		pdata.WinUserInfo = append(pdata.WinUserInfo, userwin)
	}
	for j := 0; j < len(lh.stageuserinfo); j++ {
		ud := lh.getUserDataByID(lh.stageuserinfo[j])
		if ud == nil {
			continue
		}
		stageuserwin := new(netproto.LHD_StageUerInfo)
		stageuserwin.UserID = proto.Int32(ud.user.UserID)
		stageuserwin.Win = proto.Int64(ud.getShowWinLose())
		pdata.StageUerInfo = append(pdata.StageUerInfo, stageuserwin)
	}

	getJackpotType := func(lv JackpotLevel) netproto.LHD_JackpotType {
		switch lv {
		default:
			return netproto.LHD_JackpotType_LHD_JackpotType_None
		case JackpotLevelNone:
			return netproto.LHD_JackpotType_LHD_JackpotType_None
		case JackpotLevelMini:
			return netproto.LHD_JackpotType_LHD_JackpotType_Mini
		case JackpotLevelMinor:
			return netproto.LHD_JackpotType_LHD_JackpotType_Minor
		case JackpotLevelGrand:
			return netproto.LHD_JackpotType_LHD_JackpotType_Grand
		}
	}

	if ud != nil {
		pdata.JackpotType = getJackpotType(ud.JackpotLevel).Enum()
		pdata.JackpotMoney = proto.Int64(ud.JackpotAmount)
	}
	pmsg.MsgData = pdata
	return pmsg
}

// 游戏结束
func (lh *LongHudouLogic) gameEnd() {
	lh.setState(Wait)
	if lh.zhuangID != 0 {
		user := lh.GetUserByUserID(lh.zhuangID)
		if user.IsOffLine() {
			lh.zhuangID = 0
		}
	}
	lh.GameEndFunc()
}

// 获取开奖的扑克信息
func (lh *LongHudouLogic) getPokerByType() {

	index := int32(0)
	card1 := lh.getCardbyIndex(index)
	lh.delCard(card1)

	//card1.color = lh.getCardColor(-1)
	//card1.number = lh.getCardNumber(0)
	card2 := lh.getCardbyIndex(index)
	lh.delCard(card2)
	//card2.color = lh.getCardColor(-1)
	//card2.number = lh.getCardNumber(0)

	if lh.jiangtype == LONG {
		//结果是龙
		if card2.number == card1.number {
			card2 = lh.getDifferentCardByNumber(card1.number)
			lh.delCard(card2)
		}

		if card1.number > card2.number {
			lh.cardresult = append(lh.cardresult, card1)
			lh.cardresult = append(lh.cardresult, card2)
		} else {
			lh.cardresult = append(lh.cardresult, card2)
			lh.cardresult = append(lh.cardresult, card1)
		}
	} else if lh.jiangtype == HE {
		//和
		if card1.number != card2.number {
			card2 = lh.getCardByNumber(card1.number)
			lh.delCard(card2)
		}

		//card2.number = card1.number
		lh.cardresult = append(lh.cardresult, card1)
		lh.cardresult = append(lh.cardresult, card2)
	} else if lh.jiangtype == HU {
		//虎
		if card2.number == card1.number {
			card2 = lh.getDifferentCardByNumber(card1.number)
			lh.delCard(card2)
		}

		if card1.number < card2.number {
			lh.cardresult = append(lh.cardresult, card1)
			lh.cardresult = append(lh.cardresult, card2)
		} else {
			lh.cardresult = append(lh.cardresult, card2)
			lh.cardresult = append(lh.cardresult, card1)
		}
	}

	c1 := poker.NewPokerCard(lh.cardresult[0].color, lh.cardresult[0].number)
	c2 := poker.NewPokerCard(lh.cardresult[1].color, lh.cardresult[1].number)
	mlog.Debug("开奖结果是%s,扑克牌是%s,%s", lh.getResultString(), c1.String(), c2.String())
	lh.GameLog.AddGameStep("开奖结果是%s,扑克牌是%s,%s", lh.getResultString(), c1.String(), c2.String())
}

// 获取牌的颜色
func (lh *LongHudouLogic) getCardColor(c int32) int32 {
	for i := 0; i < 10; i++ {
		time.Sleep(1 * time.Nanosecond)
		rand.Seed(time.Now().UnixNano())
		v := rand.Int31n(4)
		if v != c {
			return v
		}
	}
	if c < 3 {
		return c + 1
	}

	return 0
}

// 获取一张扑克牌
func (lh *LongHudouLogic) getCardNumber(n int32) int32 {
	for i := 0; i < 10; i++ {
		time.Sleep(1 * time.Nanosecond)
		rand.Seed(time.Now().UnixNano())
		v := rand.Int31n(13) + 1
		if v != n {
			return v
		}
	}

	return n + 1
}

// 获取所有的开奖结果
func (lh *LongHudouLogic) getAllKaiJiangType() []int32 {
	alltype := make([]int32, 0)

	for i := 0; i < 3; i++ {
		alltype = append(alltype, int32(i+1))
	}

	//檢查上莊是否為系統或機器人
	var iszhuangrobot bool = lh.isZhuangRobotAndSystem()

	if !iszhuangrobot { //是真人上莊所有獎都可以開
		return alltype
	}

	return alltype
}

// 上莊是為系統或機器人
func (lh *LongHudouLogic) isZhuangRobotAndSystem() bool {
	var iszhuangrobot bool = false
	ur := lh.GetUserByUserID(lh.zhuangID)

	if ur != nil && ur.IsRobot() {
		iszhuangrobot = true
	}

	if lh.zhuangID == 0 {
		iszhuangrobot = true
	}

	return iszhuangrobot
}

// 获取赔偿最小的开奖结果
func (lh *LongHudouLogic) getMinKaiJiangType() []int32 {
	if lh.getBetMoneyByType(LONG) > lh.getBetMoneyByType(HU) {
		return []int32{HU}
	} else if lh.getBetMoneyByType(LONG) < lh.getBetMoneyByType(HU) {
		return []int32{LONG}
	}
	return []int32{}

}

// 获取放分开奖结果
func (lh *LongHudouLogic) getFangFenKaiJiangType() []int32 {
	//放分開和條件及機率
	r := (lh.fangFenTarget * 15 / 10) - lh.fangFenComplete
	hepay := lh.getBetMoneyByType(HE) * int64(lh.betvalue.getOddByType(lh.jiangtype))
	if r > hepay {
		rnd := util.RndInt(0, 1000)
		if rnd < 75 {
			return []int32{HE}
		}
	}

	if lh.getBetMoneyByType(LONG) > lh.getBetMoneyByType(HU) {
		return []int32{LONG}
	} else if lh.getBetMoneyByType(LONG) < lh.getBetMoneyByType(HU) {
		return []int32{HU}
	}

	return []int32{LONG, HU}
}

// 设置状态
func (lh *LongHudouLogic) setState(state int32) {
	lh.gamestate = state
	lh.EventChan <- lh.MakeDashboard()
	//lh.BoardcastMessage(lh.getStateMsg())
}

// 用户消息主处理函数
func (lh *LongHudouLogic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if !lh.CheckGameUserMessage(user) {
		lh.KickUserFunc(user.UserID, "非法操作")
		return
	}
	//不是给游戏逻辑服务器的消息
	mlog.Debug("当前游戏状态为：%v", lh.gamestate)
	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	lh.CheckUserAnimationCommited(user, gmsg, lh.action_str, nil)

	if gmsg.SClassID == int32(netproto.LHD_GameMessageClassID_LHDGetShangZhuangListID) {
		lh.SendMessageToUser(user, lh.getZhuangListMsg())
		return
	}

	if lh.gamestate != Bet {
		return
	}

	if gmsg.SClassID == int32(netproto.LHD_GameMessageClassID_LHDBetID) {
		lh.userBet(user, gmsg.MsgData.([]byte))
		return
	}

	if gmsg.SClassID == int32(netproto.LHD_GameMessageClassID_LHDShangZhuangID) {
		lh.shangZhuang(user, gmsg.MsgData.([]byte))
		return
	}

	if gmsg.SClassID == int32(netproto.LHD_GameMessageClassID_LHDBetClearID) && lh.gamestate == Bet {
		lh.clearUserBet(user)
		return
	}
}

// 获取配置表
func (lh *LongHudouLogic) getlhdconfig(cfgs *config.Group) {
	lh.betvalue = newBetconfig(cfgs)
	scfgs := cfgs.GetConfig("gameserver")
	lh.bettime = int32(scfgs.GetConfigInt("gameserver", "bettime"))
	lh.kaijiangactiontime = int32(scfgs.GetConfigInt("gameserver", "kaijiangactiontime"))
	lh.endactiontime = int32(scfgs.GetConfigInt("gameserver", "endactiontime"))
	lh.readytime = int32(scfgs.GetConfigInt("gameserver", "readywaittime"))
	lh.zhuanglimit = int64(scfgs.GetConfigInt("gameserver", "zhuanglimit"))
	lh.maxturn = int32(scfgs.GetConfigInt("gameserver", "zhuangturn"))
	lh.serverid = strconv.Itoa(int(lh.GetServerConfig().ServerID))
	lh.maxclearcount = int32(scfgs.GetConfigInt("gameserver", "maxclearcount"))
	lh.zhushui = int32(scfgs.GetConfigInt("gameserver", "zhushui"))
	lh.maxrobotzhuang = int32(scfgs.GetConfigInt("gameserver", "maxrobotzhuang"))
	lh.robotxiazhuang = int32(scfgs.GetConfigInt("gameserver", "robotxiazhuang"))
	lh.robotzhuang = int32(scfgs.GetConfigInt("gameserver", "robotzhuang"))
	lh.sametimebetmode = int64(scfgs.GetConfigInt("gameserver", "sametimebetmode"))
	lh.betlimitmoney = int64(scfgs.GetConfigInt("gameserver", "betlimitmoney"))
	lh.publiczhuang = int64(scfgs.GetConfigInt("gameserver", "publiczhuang"))
	lh.luckystarround = int64(scfgs.GetConfigInt("gameserver", "luckystarround"))
	lh.HEandKill = int32(scfgs.GetConfigInt("gameserver", "HEandKill"))
	lh.cheatrate = int32(scfgs.GetConfigInt("gameserver", "cheatrate"))
	if lh.cheatrate <= 0 {
		lh.cheatrate = 80
	}
	betcountstr := scfgs.GetConfigValue("gameserver", "betcount")
	lastindex := 0
	for index, char := range betcountstr {
		if char == ',' {
			substr := betcountstr[lastindex:index]
			lastindex = index + 1
			count, _ := strconv.ParseInt(substr, 10, 32)
			lh.betcount = append(lh.betcount, int32(count))
		}
	}

	lh.ctrlbetlimit = int64(scfgs.GetConfigInt("supertool", "ctrlbetlimit"))
	if lh.ctrlbetlimit <= 0 {
		lh.ctrlbetlimit = CtrlBetLimitDefault
	}
}

// 清除玩家下注信息
func (lh *LongHudouLogic) clearUserBet(user *gamelogic.GameUser) {
	ud := lh.getUserDataByID(user.UserID)
	if ud == nil {
		return
	}

	mlog.Debug("ud.clearcount=%v", ud.clearcount)
	if ud.clearcount >= lh.maxclearcount {
		lh.SendMessageToUser(user, lh.getClearRetMsg(ud.money, -1))
		return
	}

	ud.clearcount++
	for _, ub := range ud.getUserBet() {
		lh.updateBet(ub.longhudoutype, -ub.money)
	}

	lh.GameLog.AddGameStep("用户%d點擊清除", user.UserID)
	ud.clearData()
	//lh.bettotal -= ud.user.Score - ud.money
	//lh.delUserData(user)
	lh.SendMessageToUser(user, lh.getClearRetMsg(user.Score, 0))
	lh.BoardcastMessage(lh.getBetTotalMsg(user.UserID))
}

// 获取清除结果消息
func (lh *LongHudouLogic) getClearRetMsg(money int64, ret int32) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDBetClearRetID)

	pdata := new(netproto.LHD_BetClearRet)
	pdata.Money = proto.Int64(money)
	pdata.Ret = proto.Int32(ret)
	pmsg.MsgData = pdata

	return pmsg
}

func (lh *LongHudouLogic) getUserBetByType(ud *longhudouuserdata, t int32) int64 {
	money := int64(0)
	for _, bet := range ud.userbet {
		if bet.longhudoutype == t {
			money += bet.money
		}
	}
	return money
}

// 用户下注处理函数
func (lh *LongHudouLogic) userBet(user *gamelogic.GameUser, data []byte) {
	ud := lh.getUserDataByID(user.UserID)
	if ud == nil {
		ud = lh.addUserData(user)
	}

	ust := &netproto.LHD_BetArr{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Debug("解析用户%v下注錯誤%v", user, err)
		return
	}

	//莊家不能下注
	if lh.zhuangID == user.UserID {
		return
	}

	money := int32(0)
	for _, b := range ust.BetArr {
		*b.BetType = int32(math.Abs(float64(*b.BetType)))
		*b.Count = int32(math.Abs(float64(*b.Count)))

		//玩家身上金額小於下注限制
		if user.Score < lh.betlimitmoney*100 {
			for i, u := range lh.userdata {
				if u.user.UserID == ud.user.UserID {
					lh.userdata = append(lh.userdata[:i], lh.userdata[i+1:]...)
				}
			}
			lh.SendMessageToUser(user, lh.getBetRetMsg(ud, -6, *b.BetType, ust.BetArr))
			return
		}

		// 玩家下注金額超過上限
		for _, u := range lh.userdata {
			if u.user.UserID == ud.user.UserID {
				if u.getUserBetTotal()+int64(b.GetCount()) > lh.betvalue.getUserMaxBet() {
					lh.SendMessageToUser(user, lh.getBetRetMsg(ud, -4, *b.BetType, ust.BetArr))
					return
				}
			}
		}

		if lh.sametimebetmode != 1 { //關閉莊閒同時下注的配置
			//莊閒不能同時下注
			if (*b.BetType == int32(LONG) && lh.getUserBetByType(ud, HU) > 0) ||
				(*b.BetType == int32(HU) && lh.getUserBetByType(ud, LONG) > 0) {
				lh.SendMessageToUser(user, lh.getBetRetMsg(ud, -5, *b.BetType, ust.BetArr))
				return
			}
		}

		tmpmon := lh.getMoneyByType(*b.BetType) * (*b.Count)

		// 系統下注上限
		if (int64(tmpmon) + lh.getBetMoneyByType(*b.BetType)) > lh.betvalue.getMaxMoneyByType(*b.BetType) {
			lh.SendMessageToUser(user, lh.getBetRetMsg(ud, -4, *b.BetType, ust.BetArr))
			return
		}

		// 不是系統當莊, 則壓注上限為莊家身上的錢所能夠賠付的錢
		if lh.zhuangID != 0 {
			ur := lh.GetUserByUserID(lh.zhuangID)
			odd := int64(0)
			if odd = int64(lh.betvalue.getOddByType(b.GetBetType())) - 1; odd < 0 {
				odd = 0
			}

			allmoney := (int64(tmpmon) + lh.getBetMoneyByType(*b.BetType)) * odd
			if allmoney > ur.Score {
				lh.SendMessageToUser(user, lh.getBetRetMsg(ud, -4, *b.BetType, ust.BetArr))
				return
			}
		}

		money += tmpmon
	}

	ret := int32(0)
	if int64(money) > ud.money {
		ret = -1
	} else {
		for _, b := range ust.BetArr {
			betmon := lh.getMoneyByType(*b.BetType) * (*b.Count)
			ud.setUserBet(*b.BetType, betmon)
			lh.updateBet(*b.BetType, int64(betmon))
			if user.IsRobot() {
				lh.updaterobotBet(*b.BetType, int64(betmon))
			}
			if user.UserID == lh.luckystar {
				lh.notbettimes = 0
			}
			// lh.GameLog.AddGameStep("用户%d下注:%s;金額:%d, %v", user.UserID, lh.getBetTypeString(int(b.GetBetType())), betmon, b)
		}

		lh.BoardcastMessage(lh.getBetTotalMsg(user.UserID))
		lh.EventChan <- lh.MakeDashboard()
	}

	lh.SendMessageToUser(user, lh.getBetRetMsg(ud, ret, 0, ust.BetArr))
}

// 获取此类型的下注总额
func (lh *LongHudouLogic) getBetMoneyByType(t int32) int64 {
	for i := 0; i < len(lh.bettypetotal); i++ {
		if lh.bettypetotal[i].longhudoutype == t {
			return lh.bettypetotal[i].money
		}
	}

	return 0
}

// 获取此类型的机器人下注总额
func (lh *LongHudouLogic) getRobotBetMoneyByType(t int32) int64 {
	for i := 0; i < len(lh.bettyperobot); i++ {
		if lh.bettyperobot[i].longhudoutype == t {
			return lh.bettyperobot[i].money
		}
	}

	return 0
}

// 获取此类型一次下注额
func (lh *LongHudouLogic) getMoneyByType(t int32) int32 {
	return lh.betvalue.getMoneyByType(t)
}

// 更新下注錢
func (lh *LongHudouLogic) updateBet(t int32, money int64) {
	lh.bettotal += int64(money)
	mlog.Debug("BetType=%v,money=%v,lh.bettotal=%v", t, money, lh.bettotal)
	for _, v := range lh.bettypetotal {
		if v.longhudoutype == t {
			v.money += int64(money)
			if v.money < 0 {
				mlog.Debug("錢小于0了！")
				v.money = 0
			}
			return
		}
	}

	b := new(bettype)
	b.longhudoutype = t
	b.money = int64(money)
	if b.money < 0 {
		mlog.Debug("錢小于0了！")
		b.money = 0
	}
	lh.bettypetotal = append(lh.bettypetotal, b)
}

// 更新机器人下注额
func (lh *LongHudouLogic) updaterobotBet(t int32, money int64) {
	lh.bettotalrobot += int64(money)
	for _, v := range lh.bettyperobot {
		if v.longhudoutype == t {
			v.money += money
			if v.money < 0 {
				v.money = 0
			}
			mlog.Debug("下注类型%d，下注的总錢数为%d", t, v.money)
			return
		}
	}

	b := new(bettype)
	b.longhudoutype = t
	b.money = int64(money)
	if b.money < 0 {
		b.money = 0
	}

	lh.bettyperobot = append(lh.bettyperobot, b)
}

// 移除机器人下注额
func (lh *LongHudouLogic) removeRobotBet() {
	for _, v := range lh.bettyperobot {
		lh.updateBet(v.longhudoutype, -v.money)
	}
}

// 获取状态消息
func (lh *LongHudouLogic) getStateMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDStateID)

	pdata := new(netproto.LHD_Sate)
	pdata.State = proto.Int32(lh.gamestate)
	pdata.Time = proto.Int32(lh.getBetTime())
	pmsg.MsgData = pdata
	return pmsg
}

// 获取剩餘時間
func (lh *LongHudouLogic) getBetTime() int32 {
	time := int32(0)
	if lh.opttimer != nil {
		time = lh.opttimer.WaitSeconds - lh.opttimer.ElapsedSeconds()
	}
	return time
}

// 获取用户数据通过用户ID
func (lh *LongHudouLogic) getUserDataByID(userid int32) *longhudouuserdata {
	for _, ud := range lh.userdata {
		if ud.user.UserID == userid {
			return ud
		}
	}

	return nil
}

// 添加一个用户数据
func (lh *LongHudouLogic) addUserData(user *gamelogic.GameUser) *longhudouuserdata {
	ud := newLongHuDouUserData(user)
	lh.userdata = append(lh.userdata, ud)
	return ud
}

// 删除一个用户数据
func (lh *LongHudouLogic) delUserData(user *gamelogic.GameUser) {
	for i := 0; i < len(lh.userdata); i++ {
		if lh.userdata[i].user.UserID == user.UserID {
			lh.userdata = append(lh.userdata[:i], lh.userdata[i+1:]...)
			break
		}
	}
}

// 获取莊家信息消息
func (lh *LongHudouLogic) getZhuangInfoMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDZhuangInfoID)

	pdata := new(netproto.LHD_ZhuangInfo)
	pdata.UserID = proto.Int32(0)
	pdata.LastTurn = proto.Int32(0)
	ur := lh.GetUserByUserID(lh.zhuangID)
	if ur != nil {
		pdata.UserInfo = append(pdata.UserInfo, ur.GetProtoBufGameUser())
		pdata.UserID = proto.Int32(ur.UserID)
		pdata.LastTurn = proto.Int32(lh.lastturn)
	}
	pmsg.MsgData = pdata
	return pmsg
}

// 获取上莊列表消息
func (lh *LongHudouLogic) getZhuangListMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDShangZhuangListInfoID)

	pdata := new(netproto.LHD_ZhuangList)
	for i := 0; i < len(lh.zhuanglist); i++ {
		ur := lh.GetUserByUserID(lh.zhuanglist[i])
		pdata.UserInfo = append(pdata.UserInfo, ur.GetProtoBufGameUser())
	}
	pmsg.MsgData = pdata
	return pmsg
}

// 检测用户是否能上莊
func (lh *LongHudouLogic) checkUserShangZhuang(userid int32) bool {
	ur := lh.GetUserByUserID(userid)
	if ur != nil && ur.Score >= lh.zhuanglimit {
		return true
	}

	return false
}

// 换莊
func (lh *LongHudouLogic) huangZhuang() {
	if lh.publiczhuang == 1 {
		if lh.zhuangID == 0 {
			//莊是系统直接换莊
			lh.setNextZhuang()
			return
		}
		if lh.lastturn <= 0 {
			//已经到最后一局了
			lh.setNextZhuang()
			return
		}
		ur := lh.GetUserByUserID(lh.zhuangID)
		if ur != nil && ur.Score < lh.zhuanglimit {
			lh.setNextZhuang()
			return
		}

		if ur == nil {
			lh.setNextZhuang()
			return
		}

		lh.lastturn--
		mlog.Debug("1015发送莊家剩余局数%d", lh.lastturn)
	} else {
		mlog.Debug("未開啟上莊機制，不用換莊")
		return
	}
}

// 检测下一个用户是否能上莊
func (lh *LongHudouLogic) checkNetUserCanShangZhuang() {
	if len(lh.zhuanglist) > 0 {
		for {
			user := lh.GetUserByUserID(lh.zhuanglist[0])
			if user != nil && user.Score >= int64(lh.zhuanglimit) {
				break
			}

			lh.delFromZhuangList(lh.zhuanglist[0])
			if len(lh.zhuanglist) <= 0 {
				break
			}
		}
	}
}

// 设置下一个上莊的玩家
func (lh *LongHudouLogic) setNextZhuang() {
	if lh.publiczhuang == 1 {
		lh.checkNetUserCanShangZhuang()
		if len(lh.zhuanglist) > 0 {
			lh.zhuangID = lh.zhuanglist[0]
			lh.lastturn = lh.maxturn
			lh.lastturn--
			lh.delFromZhuangList(lh.zhuangID)
			user := lh.GetUserByUserID(lh.zhuangID)
			lh.startmoney = user.Score
		} else {
			lh.zhuangID = 0
			lh.startmoney = lh.startsyspool
		}
	} else {
		mlog.Error("未開啟上莊機制，不得上莊")
		return
	}
}

// 获取下注消息
func (lh *LongHudouLogic) getBetRetMsg(ud *longhudouuserdata, ret int32, t int32, betinfo []*netproto.LHD_Bet) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDBetRetID)

	pdata := new(netproto.LHD_BetRet)
	pdata.Money = proto.Int64(ud.money)
	pdata.Ret = proto.Int32(ret)
	pdata.Type = proto.Int32(t)
	pdata.BetArr = betinfo
	pmsg.MsgData = pdata
	return pmsg
}

// 获取下注总额消息
func (lh *LongHudouLogic) getBetTotalMsg(userid int32) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDBroadCastBetID)

	pdata := new(netproto.LHD_BroadCastBetArr)
	pdata.UserID = proto.Int32(userid)

	lucky := lh.getUserDataByID(lh.luckystar) //獲取明燈

	for _, v := range lh.betvalue.alltype {
		betinfo := new(netproto.LHD_BroadCastBet)
		betinfo.Type = proto.Int32(v)
		betinfo.Money = proto.Int64(lh.getBetMoneyByType(v))
		betinfo.LuckyStar = proto.Bool(false)
		if lucky != nil && lucky.getUserBetByType(v) > 0 {
			betinfo.LuckyStar = proto.Bool(true)
		}
		pdata.BroadCastBetArr = append(pdata.BroadCastBetArr, betinfo)
	}

	pmsg.MsgData = pdata

	return pmsg
}

// 获取上莊列表索引通过用户ID
func (lh *LongHudouLogic) getZhuangListIndexByUserId(userid int32) int32 {
	for i := 0; i < len(lh.zhuanglist); i++ {
		if lh.zhuanglist[i] == userid {
			return int32(i)
		}
	}

	return -1
}

// 添加一个用户到上莊列表
func (lh *LongHudouLogic) addToZhuangList(userid int32) {
	lh.zhuanglist = append(lh.zhuanglist, userid)
}

// 删除一个用户从上莊列表
func (lh *LongHudouLogic) delFromZhuangList(userid int32) {
	for i := 0; i < len(lh.zhuanglist); i++ {
		if lh.zhuanglist[i] == userid {
			lh.zhuanglist = append(lh.zhuanglist[:i], lh.zhuanglist[i+1:]...)
			break
		}
	}
}

// 获取上莊结果消息
func (lh *LongHudouLogic) getShangzhuangRetMsg(ret int32) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDShangZhuangRetID)

	pdata := new(netproto.LHD_ShangzhuangRet)
	pdata.Ret = proto.Int32(ret)
	pmsg.MsgData = pdata
	return pmsg
}

// 处理用户上莊请求
func (lh *LongHudouLogic) shangZhuang(user *gamelogic.GameUser, data []byte) {
	ust := &netproto.LHD_ShangZhuang{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Debug("解析用户%v上莊錯誤%v", user, err)
		return
	}

	if lh.getZhuangListIndexByUserId(user.UserID) != -1 {
		lh.SendMessageToUser(user, lh.getShangzhuangRetMsg(-2))
		return
	}

	if user.Score < lh.zhuanglimit {
		lh.SendMessageToUser(user, lh.getShangzhuangRetMsg(-1))
		return
	}

	if *ust.IsShangZhuang {
		if lh.publiczhuang == 1 {
			lh.GameLog.AddGameStep("用户%d上莊", user.UserID)
			lh.addToZhuangList(user.UserID)
			lh.SendMessageToUser(user, lh.getShangzhuangRetMsg(0))
		} else {
			mlog.Error("未開啟上莊機制，不得上莊")
			return
		}
	} else {
		lh.GameLog.AddGameStep("用户%d下莊", user.UserID)
		lh.delFromZhuangList(user.UserID)
		lh.SendMessageToUser(user, lh.getShangzhuangRetMsg(1))
		lh.lastturn = -1
	}
}

//获取上一次水池的錢
/*
func (lh *LongHudouLogic) getLastPool() {
	var folderName = "lastpool"
	if _, err := os.Stat(folderName); os.IsNotExist(err) {
		err = os.MkdirAll(folderName, os.ModePerm)
	}

	var tablenostr = strconv.Itoa(int(lh.TableNo()))
	var filename = lh.serverid + "_" + tablenostr + ".conf"
	filename = path.Join(folderName, filename)
	_, err := os.Stat(filename)
	var f *os.File = nil
	if os.IsNotExist(err) {
		f, err = os.Create(filename)
	} else {
		f, err = os.OpenFile(filename, os.O_RDWR, 0)
	}

	if err != nil {
		mlog.Error("%v文件不存在", filename)
		panic(err)
	} else {
		lh.serverfile = f
		defer lh.serverfile.Close()
		lh.readPoolFromFile()
	}
}

//读取水池文件
func (lh *LongHudouLogic) readPoolFromFile() {
	buff := make([]byte, 1024)
	n, _ := lh.serverfile.Read(buff)
	if n > 0 {
		poolstr := string(buff)[0:n]
		//pool, err := strconv.ParseInt(poolstr, 10, 32)
		pool, _ := strconv.Atoi(poolstr)
		lh.syspool = int64(pool)
	}
}

//将水池的錢写入文件
func (lh *LongHudouLogic) writePoolToFile() {
	buff := strconv.Itoa(int(lh.syspool))
	//bufio.ReadWriter.Peek()
	tablenostr := strconv.Itoa(int(lh.TableNo()))
	filename := "./lastpool/" + lh.serverid + "_" + tablenostr + ".conf"
	_, err := os.Stat(filename)
	var f *os.File = nil
	f, err = os.Create(filename)
	if err != nil {
		mlog.Error("%v文件不存在", filename)
		panic(err)
	} else {
		f.WriteString(buff)
		defer f.Close()
	}
}

//添加系统水池的錢
func (lh *LongHudouLogic) addSysPool(value int64) {
	lh.syspool += value
}

//减去系统水池的錢
func (lh *LongHudouLogic) subtractSysPool(value int64) {
	lh.syspool -= value
}
*/
//改变水池的錢
func (lh *LongHudouLogic) changeSysPool(value int64) {
	change := value - lh.SysPool.GetPool()
	lh.SysPool.UpdateSysPool(change)
}

// 改变水池注水率
func (lh *LongHudouLogic) changeZhuShui(value int32) {
	if value >= 0 && value <= 100 {
		lh.zhushui = value
	}
}

// 改变机器人的和的下注概率
func (lh *LongHudouLogic) changeHe(value int32) {
	if value >= 0 && value <= 100 {
		lh.he = value
	}
}

func (lh *LongHudouLogic) checkSysPool() {
	mlog.Printf("桌子號：%v，系统水池：%v，注水概率：%v", lh.TableNo(), lh.SysPool.GetPool(), lh.zhushui)
}

// 占位用防止崩溃
func (lh *LongHudouLogic) ExchangeMaxCard(userid int32) {

}

func (lh *LongHudouLogic) OnRecvHTTPMessage(req *http.Request) proto.Message {
	action := req.PostFormValue("action")

	if action == "getserverinfo" {
		return lh.getTableListMsg()
	} else if action == "gettableinfo" {
		tid, _ := strconv.ParseInt(req.PostFormValue("tableno"), 10, 32)
		return lh.getTableDetailMsg(int32(tid))
	} else if action == "modresult" {
		tid, _ := strconv.ParseInt(req.PostFormValue("tableno"), 10, 32)
		ret, _ := strconv.ParseInt(req.PostFormValue("result"), 10, 32)
		adminuser := req.PostFormValue("adminuser")
		return lh.setNextJiangType(int32(tid), int32(ret), adminuser)
	} else if action == "modpoolamount" {
		tid, _ := strconv.ParseInt(req.PostFormValue("tableno"), 10, 64)
		typeid, _ := strconv.ParseInt(req.PostFormValue("type"), 10, 32)
		pool, _ := strconv.ParseInt(req.PostFormValue("poolamount"), 10, 64)
		operator := req.PostFormValue("operator")
		if typeid == 1 {
			return lh.setSysPool(int32(tid), pool, operator)
		} else {
			return lh.setFangFen(int32(tid), pool, operator)
		}
	} else if action == "modpopwaterrate" {
		tid, _ := strconv.ParseInt(req.PostFormValue("tableno"), 10, 32)
		zhushui, _ := strconv.ParseInt(req.PostFormValue("popwaterrate"), 10, 32)
		return lh.setZhuShui(int32(tid), int32(zhushui))
	} else if action == "history" {
		pdata := new(netproto.LHD_ResultCardList)

		if len(lh.resultcardlist.CardList) > 20 {
			pdata.CardList = lh.resultcardlist.CardList[len(lh.resultcardlist.CardList)-20 : len(lh.resultcardlist.CardList)]
		} else {
			pdata = lh.resultcardlist
		}

		return pdata
	}

	return nil
}

// 获取桌子列表数据
func (lh *LongHudouLogic) getTableListMsg() proto.Message {
	pdata := new(netproto.Http_LHD_GameInfo)
	for _, table := range lh.GetTable().Room.Tables {
		pTable := new(netproto.Http_LHD_TableList)
		logic := table.GameLogic.(*LongHudouLogic)
		pTable.TableNo = proto.Int32(logic.TableNo())
		pTable.Status = proto.Int32(logic.gamestate)
		robotcount := int32(0)
		realusercount := int32(0)
		for _, user := range logic.AllGameUser() {
			if user.IsRobot() {
				robotcount++
			} else {
				realusercount++
			}
		}
		pTable.GameUserCount = proto.Int32(realusercount)
		pTable.RobotCount = proto.Int32(robotcount)
		pTable.PoolCurrentAmount = proto.Int64(logic.SysPool.GetPool())
		lastturn := logic.lastturn
		if lastturn < 0 {
			lastturn = 0
		}
		pTable.SumEarnAmount = proto.Int64(logic.systotalwin)
		pTable.LastEarnAmount = proto.Int32(int32(logic.syslastwin))
		pTable.SysTaxIncome = proto.Int64(logic.systaxincome)

		pdata.TableList = append(pdata.TableList, pTable)
	}

	return pdata
}

// 获取桌子详细数据
func (lh *LongHudouLogic) getTableDetailMsg(tableid int32) proto.Message {
	t := lh.GetTable().Room.GetTableByNo(tableid)
	pTable := new(netproto.Http_LHD_TableDetail)
	if t != nil {
		logic := t.GameLogic.(*LongHudouLogic)
		pTable.TableNo = proto.Int32(logic.TableNo())
		pTable.Status = proto.Int32(logic.gamestate)
		//		pTable.PoolInitAmount = proto.Int64(logic.startsyspool)
		pTable.PoolCurrentAmount = proto.Int64(logic.SysPool.GetPool())
		robotcount := int32(0)
		realusercount := int32(0)
		for _, user := range logic.AllGameUser() {
			if user.IsRobot() {
				robotcount++
			} else {
				realusercount++
			}
		}
		pTable.GameUserCount = proto.Int32(realusercount)
		pTable.RobotCount = proto.Int32(robotcount)
		//		pTable.PoolTaxAmount = proto.Int64(logic.totaltax)
		pTable.PopWaterRate = proto.Int32(logic.zhushui)
		zhuang := logic.GetUserByUserID(logic.zhuangID)
		zhuangtype := int32(0)
		nickname := ""
		currmoney := int64(0)
		if zhuang != nil {
			if zhuang.IsRobot() {
				zhuangtype = 1
			} else {
				zhuangtype = 2
				currmoney = zhuang.Score
			}
			nickname = zhuang.NickName
		}
		pTable.ZhuangType = proto.Int32(zhuangtype)
		pTable.ZhuangNickName = proto.String(nickname)
		pTable.ZhuangUserID = proto.Int32(logic.zhuangID)
		lastturn := logic.lastturn
		if lastturn < 0 {
			lastturn = 0
		}
		pTable.ZhuangSurplusTotal = proto.Int32(lastturn)
		pTable.ZhuangInitMoneyAmount = proto.Int64(logic.startmoney)
		pTable.ZhuangCurrentMoneyAmount = proto.Int64(currmoney)
		pTable.SumEarnAmount = proto.Int64(logic.systotalwin)
		pTable.LastEarnAmount = proto.Int32(int32(logic.syslastwin))
		pTable.SysTaxIncome = proto.Int64(logic.systaxincome)
		pTable.OpenResultDragon = proto.Int64(logic.getRealUserWinByType(LONG))
		pTable.OpenResultTiger = proto.Int64(logic.getRealUserWinByType(HU))
		pTable.OpenResultDraw = proto.Int64(logic.getRealUserWinByType(HE))
		pTable.BetTime = proto.Int32(lh.getBetTime())
		if logic.gamestate == KaiJiangAction {
			pTable.CurentOpenResult = proto.String(logic.getResultString())
			pTable.CurentOpenResultEarn = proto.Int64(logic.getRealUserWinByType(logic.jiangtype))
		}
		pTable.UserInfo = logic.responseCurrentUserBetList()
		pTable.LastGameUserBet = logic.responseLastGameUserBetList()
		btwrate := logic.getBetTypeWinRate()
		pTable.DragonWinRate = proto.Int32(btwrate[0])
		pTable.DrawWinRate = proto.Int32(btwrate[1])
		pTable.TigerWinRate = proto.Int32(btwrate[2])
		pTable.WinLine = proto.Int64(logic.SysPool.GetWinLine())
		pTable.FanFengTarget = proto.Int64(logic.fangFenTarget)
		pTable.FanFengComplete = proto.Int64(logic.fangFenComplete)
	}

	return pTable
}

func (lh *LongHudouLogic) getResultString() string {
	var res string
	res += resultstring[lh.jiangtype]
	return res
}

func (lh *LongHudouLogic) getBetTypeString(idx int) string {
	if idx > 0 && idx < len(resultstring) {
		return resultstring[idx]
	}
	return ""
}

func (lh *LongHudouLogic) getRetMsg(ret bool, msg string) proto.Message {
	pdata := new(netproto.Http_Result)
	pdata.Result = proto.Bool(ret)
	pdata.Message = proto.String(msg)
	return pdata
}

func (lh *LongHudouLogic) getRoundMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDRoundID)
	round := new(netproto.LHD_RoundID)
	round.RoundID = proto.Int32(lh.roundid)
	round.LongWin = proto.Int32(lh.longwin)
	round.HeWin = proto.Int32(lh.hewin)
	round.HuWin = proto.Int32(lh.huwin)
	pmsg.MsgData = round
	return pmsg
}

// 设置下次开奖结果
func (lh *LongHudouLogic) setNextJiangType(tableid int32, jiang int32, adminuser string) proto.Message {
	t := lh.GetTable().Room.GetTableByNo(tableid)

	if t != nil {
		logic := t.GameLogic.(*LongHudouLogic)
		if adminuser != "" {
			logic.GameLog.SetNextJiangRecord(adminuser, fmt.Sprintf("%d", jiang))
		}
		logic.nextjiangtype = jiang
		return lh.getRetMsg(true, "修改开奖结果成功")
	}

	return lh.getRetMsg(false, "修改开奖结果失败")
}

func (lh *LongHudouLogic) setZhuShui(tableid int32, zhushui int32) proto.Message {
	if tableid == -1 {
		for _, table := range lh.GetTable().Room.Tables {
			logic := table.GameLogic.(*LongHudouLogic)
			logic.changeZhuShui(zhushui)
		}
	} else {
		t := lh.GetTable().Room.GetTableByNo(tableid)

		if t != nil {
			logic := t.GameLogic.(*LongHudouLogic)
			logic.changeZhuShui(zhushui)
		} else {
			return lh.getRetMsg(false, "修改注水率失败")
		}
	}

	return lh.getRetMsg(true, "修改注水率成功")
}

// 获取系统赢錢数
func (lh *LongHudouLogic) getRealUserWinByType(t int32) int64 {
	win := int64(0)
	odd := int64(lh.betvalue.getOddByType(t))
	user := lh.GetUserByUserID(lh.zhuangID)
	if user != nil && !user.IsRobot() {
		_, tax := lh.GetUserWinAndTax(lh.bettotal)
		if lh.currtax != 0 {
			tax = lh.currtax
		}

		robotwin := odd * lh.getRobotBetMoneyByType(t)
		win = lh.bettotalrobot - robotwin - tax
		return -win
	}

	for _, ud := range lh.userdata {
		if !ud.user.IsRobot() {
			win += odd * ud.getUserBetByType(t)
			win += ud.money - ud.user.Score
		}
	}

	mlog.Debug("玩家赢錢%d", win)
	return -win
}

// 排序身上攜帶的錢大到小
func (lh *LongHudouLogic) getMoneyRanking() {
	sort.Sort(lh.userscore)
}
func (sbg *userScore) Len() int {
	return len(sbg.userdata)
}
func (sbg *userScore) Swap(i, j int) {
	sbg.userdata[i], sbg.userdata[j] = sbg.userdata[j], sbg.userdata[i]
}
func (sbg *userScore) Less(i, j int) bool {
	return sbg.userdata[i].Score > sbg.userdata[j].Score
}

// 獲取幸運星玩家
func (lh *LongHudouLogic) getLuckyStar() {
	t := 0
	gameuser := lh.AllGameUser()
	if lh.luckystar == 0 || lh.notbettimes >= 3 {
		//如果沒有幸運星、連續三場沒下注或是幸運星的持續局數到了，隨機一個玩家
		t = 1
	} else {
		//檢查玩家還在不在場
		for _, u := range gameuser {
			if u.UserID == lh.luckystar {
				lh.luckystartimes += 1
				break
			} else {
				t = 1
			}
		}
	}
	if t == 1 || lh.luckystartimes == lh.luckystarround {
		if len(gameuser) != 0 {
			userIndex := rand.Intn(len(gameuser))
			lh.luckystar = gameuser[userIndex].UserID
			lh.luckyfaceid = gameuser[userIndex].FaceID
		} else {
			//如果沒有明燈，須和client確認要傳什麼
			lh.luckystar = 0
			lh.luckyfaceid = 0
		}
		lh.luckystartimes = 0
		lh.notbettimes = 0
	}
}

// 獲取富豪玩家
func (lh *LongHudouLogic) getRichestList() {
	userscore := new(userScore)
	userscore.userdata = lh.AllTableUser()
	sort.Sort(userscore)
	//計算分數最高的7個
	for index, user := range userscore.userdata {
		if index < 7 {
			rich := new(netproto.LHD_Richest)
			rich.Ranking = proto.Int32(int32(index + 1))
			rich.UserID = proto.Int32(user.UserID)
			rich.UserFaceID = proto.Int32(user.FaceID)
			rich.Money = proto.Int64(user.Score)
			lh.richestList = append(lh.richestList, rich)
			lh.richestuserinfo = append(lh.richestuserinfo, user.UserID)
		} else {
			break
		}
	}
	lh.stageuserinfo = append(lh.richestuserinfo, lh.luckystar)
}
func (lh *LongHudouLogic) getGameRound() {
	if (lh.roundid == 30 && lh.longwin+lh.hewin+lh.huwin == 30) || lh.roundid == 0 {
		lh.longwin = 0
		lh.hewin = 0
		lh.huwin = 0
	}
}

// 通过索引获取一张牌
func (lh *LongHudouLogic) getCardbyIndex(index int32) *card {
	card := new(card)
	if index < int32(len(lh.cards.Cards)) {
		card.number = lh.cards.Cards[index].CardNumber
		card.color = lh.cards.Cards[index].Color
	} else {
		//mlog.Fatal("已经没牌了")
		lh.forceNewRoundFapai() //強制设为新牌局 重新发牌
		return lh.getCardbyIndex(index)
	}

	if card.number == 14 {
		card.number = 1
	}

	return card
}

// 通过扑克点数获取一张扑克点数相同的牌
func (lh *LongHudouLogic) getCardByNumber(no int32) *card {
	card := new(card)
	tmpno := no
	if tmpno == 1 {
		tmpno = 14
	}

	var isCardFound = false
	for i := 0; i < len(lh.cards.Cards); i++ {
		if lh.cards.Cards[i].CardNumber == tmpno {
			isCardFound = true
			card.number = lh.cards.Cards[i].CardNumber
			card.color = lh.cards.Cards[i].Color
			break
		}
	}

	// 剩馀牌库找不到相同点数
	if !isCardFound {
		lh.forceNewRoundFapai() //強制设为新牌局 重新发牌
		return lh.getCardByNumber(no)
	}

	if card.number == 14 {
		card.number = 1
	}

	return card
}

// 删除一张牌
func (lh *LongHudouLogic) delCard(c *card) {
	delcard := poker.NewEmptyPokerCard()
	delcard.CardNumber = c.number
	delcard.Color = c.color
	if delcard.CardNumber == 1 {
		delcard.CardNumber = 14
	}
	mlog.Debug("roundID:%d", lh.roundid)
	lh.cards.DeleteCardForPokers(delcard)
}

// 通过扑克点数获取一张扑克点数不相同的牌
func (lh *LongHudouLogic) getDifferentCardByNumber(no int32) *card {
	card := new(card)
	tmpno := no
	if tmpno == 1 {
		tmpno = 14
	}
	for i := 0; i < len(lh.cards.Cards); i++ {
		if lh.cards.Cards[i].CardNumber != tmpno {
			card.number = lh.cards.Cards[i].CardNumber
			card.color = lh.cards.Cards[i].Color
			break
		}
	}

	if card.number == 14 {
		card.number = 1
	}

	return card
}

// 收到RPC消息解析處理
func (lh *LongHudouLogic) OnRecvRPCMessage(m *msg.Message) {
	rm, ok := m.MsgData.(*rpcmsg.RPCMessage)
	if !ok {
		return
	}
	mmbyte := rm.MsgData.([]byte)
	if len(mmbyte) == 0 {
		return
	}

	if m.SClassID == int32(netproto.GameMessageClassID_SuperToolGetAssignRewardDetailRetID) {
		lh.setAssignReward(m)
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadRetID) {
		lh.RecvJackpotData(m)
	}
}

func (lh *LongHudouLogic) SubscripEvent(ctx context.Context) <-chan *pb.LHDTableDashboard {
	ch := make(chan *pb.LHDTableDashboard, 10)
	lh.BroadcastSubscriper.Store(ctx, ch)
	go func(ctx context.Context, ch chan *pb.LHDTableDashboard) {
		<-ctx.Done()
		lh.BroadcastSubscriper.Delete(ctx)
		time.Sleep(time.Second)
		close(ch)
	}(ctx, ch)
	return ch
}

func (lh *LongHudouLogic) queryBetSummary() {
	if err := app.Context.Invoke(func(c apipb.GameBetServiceClient) error {
		req := new(apipb.SummaryReq)
		req.ServerID = lh.TableManager.Room.ServerID

		loc := time.FixedZone("UTC+8", 60*60*8)
		now := time.Now().In(loc)
		begin := time.Date(now.Year(), now.Month(), now.Day(), 0, 0, 0, 0, now.Location())
		end := begin.AddDate(0, 0, 1)

		req.Begin = begin.UTC().Format("2006-01-02 15:04:05")
		req.End = end.UTC().Format("2006-01-02 15:04:05")

		example.invalid("查詢本日下注, %v", req)

		res, err := c.Summary(context.Background(), req)
		if err != nil {
			return err
		}

		lh.totalBetAmount = res.BetAmount
		lh.totalWinLossAmount = res.WinLossAmount
		lh.totalValidAmount = res.ValidAmount

		example.invalid("更新 本日下注總計: %d, 本日輸贏總計: %d, 本日流水總計: %d", lh.totalBetAmount, lh.totalWinLossAmount, lh.totalValidAmount)

		return nil
	}); err != nil {
		mlog.Error("Bet Summary Error, %v", err)
	}
}

func (lh *LongHudouLogic) MakeDashboard() *pb.LHDTableDashboard {

	m := new(pb.LHDTableDashboard)

	m.TableID = lh.TableNo()
	m.Long = new(pb.LHDBetAreaInfo)
	m.Hu = new(pb.LHDBetAreaInfo)
	m.Tie = new(pb.LHDBetAreaInfo)
	m.SysPool = lh.SysPool.GetPool()
	m.SysPoolWinLine = lh.SysPool.GetWinLine()
	m.TotalBetAmount = lh.totalBetAmount
	m.TotalWinLossAmount = lh.totalWinLossAmount
	m.TotalValidAmount = lh.totalValidAmount
	if lh.Jackpot != nil {
		m.JackpotInfo = new(pb.LHDJackpotInfo)
		m.JackpotInfo.Enabled = lh.Jackpot.Enabled
		m.JackpotInfo.Awardpause = lh.Jackpot.AwardPause
		m.JackpotInfo.Current = lh.Jackpot.Current()
		m.JackpotInfo.BasePool = lh.Jackpot.BasePool
		m.JackpotInfo.AwardLine = lh.Jackpot.AwardLine

		phase, rate := lh.Jackpot.Phase()
		m.JackpotInfo.Phase = int32(phase)
		m.JackpotInfo.PhaseRate = int32(rate)

		if lh.Jackpot.Assignment != nil {
			m.JackpotInfo.Assignment = &pb.LHDJackpotAssignment{
				Level:      pb.LHDJackpotLevel(lh.Jackpot.Assignment.Level),
				AssignType: pb.LHDJackpotAssignType(lh.Jackpot.Assignment.Type),
			}
		}

	}

	// 統計所有玩家下注
	for _, user := range lh.AllGameUser() {

		mp := new(pb.LHDPlayerInfo)
		mp.PID = user.UserID
		mp.IsRobot = user.IsRobot()

		if ud := lh.getUserDataByID(user.UserID); ud != nil {
			for _, bet := range ud.userbet {
				switch bet.longhudoutype {
				case LONG:
					mp.Long += bet.money
					m.Long.Total += bet.money
					if ud.user.IsRobot() {
						m.Long.RobotBetAmount += bet.money
						m.Long.RobotCount++
					} else {
						m.Long.PlayerBetAmount += bet.money
						m.Long.PlayerCount++
					}
				case HU:
					mp.Hu += bet.money
					m.Hu.Total += bet.money
					if ud.user.IsRobot() {
						m.Hu.RobotBetAmount += bet.money
						m.Hu.RobotCount++
					} else {
						m.Hu.PlayerBetAmount += bet.money
						m.Hu.PlayerCount++
					}
				case HE:
					mp.Tie += bet.money
					m.Tie.Total += bet.money
					if ud.user.IsRobot() {
						m.Tie.RobotBetAmount += bet.money
						m.Tie.RobotCount++
					} else {
						m.Tie.PlayerBetAmount += bet.money
						m.Tie.PlayerCount++
					}
				}
				mp.BetAmount += bet.money
			}
		}

		m.Players = append(m.Players, mp)
	}

	switch lh.nextjiangtype {
	default:
		m.NextAward = pb.LHDAward_LHDAward_NONE
	case LONG:
		m.NextAward = pb.LHDAward_LHDAward_LONG
	case HU:
		m.NextAward = pb.LHDAward_LHDAward_HU
	case HE:
		m.NextAward = pb.LHDAward_LHDAward_TIE
	}

	for _, r := range lh.resultlist {
		switch r {
		case LONG:
			m.History = append(m.History, pb.LHDAward_LHDAward_LONG)
		case HU:
			m.History = append(m.History, pb.LHDAward_LHDAward_HU)
		case HE:
			m.History = append(m.History, pb.LHDAward_LHDAward_TIE)
		}
	}

	switch lh.gamestate {
	default:
		m.State = pb.LHDState_LHDState_Prepare
	case Bet:
		m.State = pb.LHDState_LHDState_Bet
	case KaiJiang:
		m.State = pb.LHDState_LHDState_KaiJiang
	case KaiJiangAction:
		m.State = pb.LHDState_LHDState_KaiJiangAction
	case EndAction:
		m.State = pb.LHDState_LHDState_EndAction
	}

	m.Cheatable = lh.nextjiangtype == 0 && lh.gamestate == 0 && lh.SpecialRewardCount == 0
	m.Cheating = lh.CheatFlag
	m.SpecialRewardCount = lh.SpecialRewardCount
	m.SpecialRewardRate = lh.SpecailRewardRate

	m.TableDisabled = lh.Disabled

	mlog.Debug("Dashboard: %v EventChan len %d", m, len(lh.EventChan))

	return m
}

func (lh *LongHudouLogic) Cheat(cnt int) {
	cheatable := lh.nextjiangtype == 0 && lh.gamestate == 0

	if !cheatable {
		return
	}

	lh.CheatFlag = true
	lh.cheatKickCnt = cnt
	lh.EventChan <- lh.MakeDashboard()
}

func (lh *LongHudouLogic) cheat(cnt int) {

	type BetArea struct {
		Total   int64
		UserCnt int
		UserIDs map[int32]bool
	}

	betMap1 := map[int32]*BetArea{
		LONG: {
			Total:   0,
			UserCnt: 0,
			UserIDs: make(map[int32]bool),
		},
		HE: {
			Total:   0,
			UserCnt: 0,
			UserIDs: make(map[int32]bool),
		},
		HU: {
			Total:   0,
			UserCnt: 0,
			UserIDs: make(map[int32]bool),
		},
	}

	// 統計所有玩家下注
	for _, ud := range lh.userdata {
		if ud.user.IsRobot() {
			continue
		}
		for _, bet := range ud.userbet {
			area := betMap1[bet.longhudoutype]
			area.Total += bet.money
			if ok := area.UserIDs[ud.user.UserID]; !ok {
				area.UserIDs[ud.user.UserID] = true
				area.UserCnt++
			}
		}
	}

	example.invalid("作弊第一階段: 龍[金額: %d 人數: %d] 和[金額: %d 人數: %d] 虎[金額: %d 人數: %d]",
		betMap1[LONG].Total, betMap1[LONG].UserCnt,
		betMap1[HE].Total, betMap1[HE].UserCnt,
		betMap1[HU].Total, betMap1[HU].UserCnt,
	)

	lh.GameLog.AddGameStep("作弊第一階段: 龍[金額: %d 人數: %d] 和[金額: %d 人數: %d] 虎[金額: %d 人數: %d]",
		betMap1[LONG].Total, betMap1[LONG].UserCnt,
		betMap1[HE].Total, betMap1[HE].UserCnt,
		betMap1[HU].Total, betMap1[HU].UserCnt,
	)

	target1 := int32(0)

	// 龍大於虎
	if betMap1[LONG].Total > betMap1[HU].Total {
		target1 = LONG
	} else if betMap1[LONG].Total < betMap1[HU].Total {
		target1 = HU
	} else {
		if rand.Intn(2) == 0 {
			target1 = LONG
		} else {
			target1 = HU
		}
	}

	if target1 != 0 {

		defer func() {
			lh.EventChan <- lh.MakeDashboard()
		}()

		area := betMap1[target1]

		if area.UserCnt == 0 {
			example.invalid("作弊: 沒有玩家不踢")
			lh.GameLog.AddGameStep("作弊: 沒有玩家不踢")
			return
		}

		// 要踢2個人
		for i := 0; i < cnt; i++ {

			var users []int32
			for uid := range area.UserIDs {
				users = append(users, uid)
			}

			if len(users) == 0 {
				example.invalid("作弊: 已經沒有玩家不踢")
				lh.GameLog.AddGameStep("作弊: 沒有玩家不踢")
				continue
			}

			idx := rand.Intn(len(users))
			uid := users[idx]

			u := lh.GetUserByUserID(uid)
			if u != nil {
				//刪除用戶數據之前先清除用戶下注數據
				ud := lh.getUserDataByID(uid)
				if ud == nil {
					continue
				}
				for _, ub := range ud.getUserBet() {
					lh.updateBet(ub.longhudoutype, -ub.money)
				}
				ud.clearData()

				lh.delUserData(u)
				lh.KickUserParamFunc(uid, "", roomutil.MsgParamUserLocked)
				lh.UserLeave(u)
				lh.blockList = append(lh.blockList, uid)
				example.invalid("作弊: 踢除玩家[%d]", uid)
				lh.GameLog.AddGameStep("作弊: 踢除玩家[%d]", uid)
				delete(area.UserIDs, uid)
				lh.BoardcastMessage(lh.getBetTotalMsg(uid))
			}
		}

		betMap2 := map[int32]*BetArea{
			LONG: {
				Total:   0,
				UserCnt: 0,
				UserIDs: make(map[int32]bool),
			},
			HE: {
				Total:   0,
				UserCnt: 0,
				UserIDs: make(map[int32]bool),
			},
			HU: {
				Total:   0,
				UserCnt: 0,
				UserIDs: make(map[int32]bool),
			},
		}

		// 統計所有玩家下注
		for _, ud := range lh.userdata {
			if ud.user.IsRobot() {
				continue
			}
			for _, bet := range ud.userbet {
				area := betMap2[bet.longhudoutype]
				area.Total += bet.money
				if ok := area.UserIDs[ud.user.UserID]; !ok {
					area.UserIDs[ud.user.UserID] = true
					area.UserCnt++
				}
			}
		}

		example.invalid("作弊第二階段: 龍[金額: %d 人數: %d] 和[金額: %d 人數: %d] 虎[金額: %d 人數: %d]",
			betMap2[LONG].Total, betMap2[LONG].UserCnt,
			betMap2[HE].Total, betMap2[HE].UserCnt,
			betMap2[HU].Total, betMap2[HU].UserCnt,
		)
		lh.GameLog.AddGameStep("作弊第二階段: 龍[金額: %d 人數: %d] 和[金額: %d 人數: %d] 虎[金額: %d 人數: %d]",
			betMap2[LONG].Total, betMap2[LONG].UserCnt,
			betMap2[HE].Total, betMap2[HE].UserCnt,
			betMap2[HU].Total, betMap2[HU].UserCnt,
		)

		target2 := int32(0)

		weight := 100
		r := new(prob.Random)
		r.Add(true, int(lh.cheatrate))
		r.Add(false, weight-int(lh.cheatrate))
		compenyWin := r.Choose().(bool)

		var sWin string
		switch compenyWin {
		case true:
			sWin = "公司贏"
		case false:
			sWin = "公司輸"
		}

		cheatMsg := fmt.Sprintf("作弊第二階段: 公司贏機率[%d/%d] 公司輸機率[%d/%d] 結果[%s]", lh.cheatrate, weight, weight-int(lh.cheatrate), weight, sWin)
		example.invalid(cheatMsg)
		lh.GameLog.AddGameStep(cheatMsg)

		if compenyWin { // 公司贏
			if betMap2[LONG].Total > betMap2[HU].Total { // 龍大於虎 開虎
				target2 = HU
			} else if betMap2[LONG].Total < betMap2[HU].Total { // 龍小於虎 開龍
				target2 = LONG
			}
		} else { // 公司輸
			if betMap2[LONG].Total > betMap2[HU].Total { // 龍大於虎 開龍
				target2 = LONG
			} else if betMap2[LONG].Total < betMap2[HU].Total { // 龍大於虎 開龍
				target2 = HU
			}
		}

		// 還沒找到就隨機
		if target2 == 0 {
			if rand.Intn(2) == 0 {
				target2 = LONG
			} else {
				target2 = HU
			}
		}

		s := ""
		switch target2 {
		case LONG:
			s = "龍"
		case HU:
			s = "虎"
		}
		example.invalid("作弊: 指定開獎[%s]", s)
		lh.GameLog.AddGameStep("作弊: 指定開獎[%s]", s)

		lh.setNextJiangType(lh.TableNo(), target2, "Cheat")
	} else {
		example.invalid("作弊: 不指定開獎")
		lh.GameLog.AddGameStep("作弊: 不指定開獎")
	}

}

func (lh *LongHudouLogic) checkBlock(u *gamelogic.GameUser) {
	for _, id := range lh.blockList {
		if id == u.UserID {
			lh.KickUserParamFunc(id, "", roomutil.MsgParamUserLocked)
			u.ConnInfo.Close()
		}
	}
}

func (lh *LongHudouLogic) specialReward() {

	type BetArea struct {
		Total   int64
		UserCnt int
		UserIDs map[int32]bool
	}

	betMap1 := map[int32]*BetArea{
		LONG: {
			Total:   0,
			UserCnt: 0,
			UserIDs: make(map[int32]bool),
		},
		HE: {
			Total:   0,
			UserCnt: 0,
			UserIDs: make(map[int32]bool),
		},
		HU: {
			Total:   0,
			UserCnt: 0,
			UserIDs: make(map[int32]bool),
		},
	}

	// 統計所有玩家下注
	for _, ud := range lh.userdata {
		for _, bet := range ud.userbet {
			area := betMap1[bet.longhudoutype]
			mlog.Debug("betMap1[%d] = %v", bet.longhudoutype, area)
			area.Total += bet.money
			if ok := area.UserIDs[ud.user.UserID]; !ok {
				area.UserIDs[ud.user.UserID] = true
				area.UserCnt++
			}
		}
	}

	format := `特殊開獎 機率 %d%% 剩餘 %d 次 龍[金額: %d] 和[金額: %d] 虎[金額: %d]`

	example.invalid(format, lh.SpecailRewardRate, lh.SpecialRewardCount, betMap1[LONG].Total, betMap1[HE].Total, betMap1[HU].Total)
	lh.GameLog.AddGameStep(format, lh.SpecailRewardRate, lh.SpecialRewardCount, betMap1[LONG].Total, betMap1[HE].Total, betMap1[HU].Total)

	// 骰出要不要特殊開獎
	if int32(rand.Intn(100)) < lh.SpecailRewardRate {

		// 計算押注最少的區域 龍/虎
		var t1 int32
		if betMap1[LONG].Total > betMap1[HU].Total {
			t1 = HU
		} else if betMap1[LONG].Total < betMap1[HU].Total {
			t1 = LONG
		} else {
			if rand.Intn(2) == 0 {
				t1 = LONG
			} else {
				t1 = HU
			}
		}

		example.invalid("特殊開獎: 指定開獎[%s]", resultstring[t1])
		lh.GameLog.AddGameStep("特殊開獎: 指定開獎[%s]", resultstring[t1])

		lh.setNextJiangType(lh.TableNo(), t1, "特殊開獎")

	} else {
		example.invalid("特殊開獎: 不開獎")
		lh.GameLog.AddGameStep("特殊開獎: 不開獎")
	}

	lh.SpecialRewardCount--
}

func (lh *LongHudouLogic) SetNextJiangType(t int32) {
	lh.nextjiangtype = t
}
