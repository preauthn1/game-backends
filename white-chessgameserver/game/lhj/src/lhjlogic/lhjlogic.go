package lhjlogic

import (
	"DataCfgLhj"
	"config"
	"event"
	"fmt"
	"gamelogic"
	"example.invalid/ahmetb/go-linq"
	"example.invalid/golang/protobuf/proto"
	"math"
	"mlog"
	"msg"
	"netproto"
	"rpcmsg"
	"strconv"
	"strings"
	"util"
)

//PayType獎勵方式
const (
	CHIP       = "chip"
	FREE_SPINS = "free_spins"
	FREE_GAME  = "free_Game"
	JACKPOT    = "jackpot"
)

//符號組合模式
const (
	LEFT = "left"
	ANY  = "any"
)

const (
	WILD    = "w"
	SCATTER = "s"
)

type LhjLogic struct {
	*gamelogic.ComputeGameLogicBase
	LhjConfig        //老虎機資料設定擋csv
	cfgs             *config.ConfigGroup
	userdata         []*lhjuserdata //用户数据
	money            int64          //当前的钱
	winmoney         int64          //本次进入房间输赢的钱
	updateMoney      int64
	betvalue         *betconfig //下注配置文件
	action_str       string     //当前动画
	gamestate        int32      //游戏状态
	reelRow          int32
	kaiJiang         []int32                    //開獎起始符號id
	reelSymbolResult map[int32][]int32          //map[reelId][]int32{symboleIdMap}取得輪軸所有開獎結果, reel從1開始, symbol index從0開始
	payLineResult    map[int32][]*PayLineResult //map[LineId][]*賠付線結果
	freeSpins        int32                      //可玩幾次freespins
	slotpattern      int32                      //slot類型
	showWildWinMoney []*int32                   //表演金額
	betrate          int32                      //jackpot計算使用
	jprate           int32                      //jackpot計算使用
	jackpotList      []*JackpotPool             //Jackpot明細的內容
	bigwinrule       []int32                    //client動畫播放階段,以Bet為主
	bigwinnotify     int32                      //當下注16, Total=6400(16*400) 得獎超過6400才廣播.
	endactiontime    int32
}

type JackpotPool struct {
	jackpotId   int32
	syspool     int64   //累積獎池
	spinStack   float32 //累積的貢獻值
	jpAmountOut int64   //重置前累積JP出獎金額
	level       string
}

//賠付線結果
type PayLineResult struct {
	lineId      int32
	reelId      int32
	SymbolGroup []*netproto.LHJ_SymbolGroup
}

type SymboleOrder struct {
	SymbolID   int32
	SymbolName string
	Order      int32
}

//老虎機資料設定擋csv
type LhjConfig struct {
	payLineGroup          []*PayLineDetail                     //所有賠付線Data
	slotPatternTable      *DataCfgLhj.SlotPattern              //Slot類型
	normalSymbolDistTable []*DataCfgLhj.SlotSymbolDistribution //一般輪軸設定
	payOutSymbole         map[string]*SlotPayOutSymbol         //map[symboleName]SlotPayOutSymbol
	reelAndSymbol         [10]*ReelAndSymbolSetting            //[reelid]的array, 用map方式找symbolid取得symbol name
	jackpotTable          []*DataCfgLhj.JackpotTable
	//TODO free spin 輪軸設定
}

//每個reel的symbolid都是唯一
type ReelAndSymbolSetting struct {
	reelId       int32
	symboleIdMap map[int32]*netproto.LHJ_SymbolGroup
}

type ReelSymbol struct {
	reelId     int32
	rowId      int32
	symbolId   int32
	symbolName string
}

//賠付獎項資料
type SlotPayOutSymbol struct {
	payOutTable []*DataCfgLhj.SlotPayTable //所有賠付獎項Data
}

type PayLineDetail struct {
	id        int32        //流水號
	groupId   int32        //群組的ID
	lineId    int32        //中獎線編號
	payLineXY []*PayLineXY //所有賠付線Data
}

type PayLineXY struct {
	pointX int32
	pointY int32
}

const (
	GameStartAnimation  = "gamestart"
	GameKaiJiang_Action = "kaijiang"
	GameEnd_Action      = "end"
)

const (
	Bet            = iota //下注状态
	KaiJiang              //开奖状态
	KaiJiangAction        //开奖动画
	EndAction             //结算动画
	Wait                  //等待游戏开始
)

//新建一个龙虎斗实例
func NewLHJLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *LhjLogic {
	glc := new(LhjLogic)
	glc.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)
	glc.cfgs = cfgs
	glc.getlhjconfig(cfgs)
	glc.ReloadLogicConfig()
	glc.freeSpins = 0
	//glc.jackpotList = make([]*JackpotPool, 0)
	glc.RegisterBeforeGameStartFunc(func() {
		glc.clear()
	})
	glc.RegisterAfterGameStartFunc(func() {
		glc.sendGameFreeOrPayMsg()
		glc.startAnimation()
	})
	glc.RegisterBeforeGameEndFunc(func() {
		glc.sendJackpotPoolS2C()
		glc.updateJackpotPool()
	})
	return glc
}

//to client jackpot 資料
func (lh *LhjLogic) sendJackpotPoolS2C() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJJackpotPoolS2CID)
	pdata := new(netproto.LHJ_JackpotPoolS2CMsg)

	for _, v := range lh.jackpotList {
		jackpot := new(netproto.LHJ_JackpotPoolDetail)
		jackpot.JackpotID = proto.Int32(v.jackpotId)
		jackpot.Syspool = proto.Int64(v.syspool)
		jackpot.Level = proto.String(v.level)
		pdata.JackpotPoolID = append(pdata.JackpotPoolID, jackpot)
		mlog.System("[weihao] send S2C jackpot[%+v]", jackpot)
	}
	pmsg.MsgData = pdata

	lh.BoardcastMessage(pmsg)
}

//更新資料庫jackpot水池
func (lh *LhjLogic) updateJackpotPool() {
	for _, v := range lh.jackpotList {
		pmsg := new(msg.Message)
		pmsg.BClassID = int32(netproto.MessageBClassID_Game)
		pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJUpdateJackpotPoolID)
		pdata := new(netproto.LHJ_GetJackpotRet)
		pdata.JackpotID = proto.Int32(v.jackpotId)
		pdata.ServerID = proto.Int32(lh.TableManager.Room.ServerID)
		flagid, _ := strconv.Atoi(lh.TableManager.Room.Flag)
		pdata.FlagID = proto.Int32(int32(flagid))
		pdata.TableNo = proto.Int32(lh.TableNo())
		spinStack := int64(util.Decimal(float64(v.spinStack*1000), 0))
		pdata.SpinStack = proto.Int64(spinStack)
		pdata.PoolAmount = proto.Int64(v.syspool)
		pdata.JPAmountOut = proto.Int64(v.jpAmountOut)
		mlog.System("[weihao] 更新資料庫jackpot水池[%+v] ", v)
		pmsg.MsgData = pdata
		lh.SendRPCMessageFunc(true, pmsg)
	}
}

//取得jackpot pool
func (lh *LhjLogic) getJackpotPool() {
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	for _, jackpotObj := range lh.jackpotTable {
		if jackpotObj.GetGroupId() == int32(jackpotGroupId) {
			pmsg := new(msg.Message)
			pmsg.BClassID = int32(netproto.MessageBClassID_Game)
			pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJGetJackpotPoolID)
			pdata := new(netproto.LHJ_GetJackpotMsg)
			pdata.ServerID = proto.Int32(lh.TableManager.Room.ServerID)
			flagid, _ := strconv.Atoi(lh.TableManager.Room.Flag)
			pdata.FlagID = proto.Int32(int32(flagid))
			pdata.TableNo = proto.Int32(lh.TableManager.TableNo)
			pdata.JackpotID = proto.Int32(jackpotObj.GetId())
			pmsg.MsgData = pdata

			lh.SendRPCMessageFunc(true, pmsg)
		}
	}
}

//收到RPC消息解析处理消息
func (lh *LhjLogic) OnRecvRPCMessage(m *msg.Message) {
	rm, ok := m.MsgData.(*rpcmsg.RPCMessage)
	if !ok {
		return
	}
	mmbyte := rm.MsgData.([]byte)
	if len(mmbyte) == 0 {
		return
	}

	if m.SClassID == int32(netproto.LHJ_GameMessageClassID_LHJGetJackpotPoolRetID) {
		lh.setJackpool(m)
		lh.sendJackpotPoolS2C()
	}

}

func (lh *LhjLogic) setJackpool(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.LHJ_GetJackpotRet{}
		return mm
	})
	detail := rmm.(*netproto.LHJ_GetJackpotRet)

	obj := linq.From(lh.jackpotList).
		FirstWithT(func(v *JackpotPool) bool {
			return v.jackpotId == detail.GetJackpotID()
		})

	jackpotObj := new(JackpotPool)
	spinStack := util.Decimal(float64(detail.GetSpinStack())/1000, 3)
	if obj != nil {
		jackpotObj.spinStack = spinStack
		jackpotObj.syspool = detail.GetPoolAmount()
		jackpotObj.jpAmountOut = detail.GetJPAmountOut()
	} else {
		jackpotObj := new(JackpotPool)
		jackpotObj.jackpotId = detail.GetJackpotID()
		jackpotObj.spinStack = spinStack
		jackpotObj.syspool = detail.GetPoolAmount()
		jackpotObj.jpAmountOut = detail.GetJPAmountOut()
		lh.jackpotList = append(lh.jackpotList, jackpotObj)
	}

	//log
	for _, v := range lh.jackpotList {
		mlog.System("[weihao] setJackpool jackpotObj[%+v] ", v)
	}

	lh.sendJackpotPoolS2C()
}

func (lh *LhjLogic) clear() {
	lh.money = 0
	lh.winmoney = 0
	lh.userdata = make([]*lhjuserdata, 0)
	lh.showWildWinMoney = make([]*int32, 0)
}

func (lh *LhjLogic) startAnimation() {
	lh.RegisterAnimation(GameStartAnimation, int32(1), lh.startBet)
}

func (lh *LhjLogic) startBet() {
	lh.setState(Bet)
	//lh.BoardcastMessage(lh.getStateMsg())
}

//设置状态
func (lh *LhjLogic) setState(state int32) {
	lh.gamestate = state
	lh.BoardcastMessage(lh.getStateMsg())
}

//获取状态消息
func (lh *LhjLogic) getStateMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJStateID)

	pdata := new(netproto.LHJ_GameStatus)
	pdata.State = proto.Int32(lh.gamestate)
	pmsg.MsgData = pdata

	//mlog.System("[weihao] 获取状态消息[%v] ", lh.gamestate)
	return pmsg
}

func (lh *LhjLogic) getlhjconfig(cfgs *config.ConfigGroup) {
	lh.betvalue = newBetconfig(cfgs)
	lh.reelRow = 3

	lh.getSlotPatternTable()
	lh.getSlotSymbolDistribution()
	lh.getPayLineTable()
	lh.getSlotPayTable()
	lh.getJackPotTable()

	//scfgs := cfgs.GetConfig("gameserver")
	//lh.bettime = int32(scfgs.GetConfigInt("gameserver", "bettime"))
	//lh.kaijiangactiontime = int32(scfgs.GetConfigInt("gameserver", "kaijiangactiontime"))

	//lh.readytime = int32(scfgs.GetConfigInt("gameserver", "readywaittime"))
	//lh.zhuanglimit = int64(scfgs.GetConfigInt("gameserver", "zhuanglimit"))
	//lh.maxturn = int32(scfgs.GetConfigInt("gameserver", "zhuangturn"))
	//lh.serverid = strconv.Itoa(int(lh.GetServerConfig().ServerID))
	//lh.maxclearcount = int32(scfgs.GetConfigInt("gameserver", "maxclearcount"))
	//lh.zhushui = int32(scfgs.GetConfigInt("gameserver", "zhushui"))
	//lh.maxrobotzhuang = int32(scfgs.GetConfigInt("gameserver", "maxrobotzhuang"))
	//lh.robotxiazhuang = int32(scfgs.GetConfigInt("gameserver", "robotxiazhuang"))
	//lh.robotzhuang = int32(scfgs.GetConfigInt("gameserver", "robotzhuang"))
	//lh.winline = int64(scfgs.GetConfigInt("gameserver", "winline"))
	//lh.sametimebetmode = int64(scfgs.GetConfigInt("gameserver", "sametimebetmode"))
	//lh.betlimitmoney = int64(scfgs.GetConfigInt("gameserver", "betlimitmoney"))
	//lh.publiczhuang = int64(scfgs.GetConfigInt("gameserver", "publiczhuang"))
	//lh.luckystarround = int64(scfgs.GetConfigInt("gameserver", "luckystarround"))
	//betcountstr := scfgs.GetConfigValue("gameserver", "betcount")
	//lastindex := 0
	//for index, char := range betcountstr {
	//	if char == ',' {
	//		substr := betcountstr[lastindex:index]
	//		lastindex = index + 1
	//		count, _ := strconv.ParseInt(substr, 10, 32)
	//		lh.betcount = append(lh.betcount, int32(count))
	//	}
	//}
}

//取得slot模式
func (lh *LhjLogic) getSlotPatternTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPattern.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &DataCfgLhj.SlotPattern_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPatternTableArr := slotdata.(*DataCfgLhj.SlotPattern_ARRAY)
	lh.slotPatternTable = slotPatternTableArr.Items[lh.slotpattern]

	mlog.System("[weihao] slotPatternTable[%+v]", lh.slotPatternTable)
}

//取得slot的輪軸設定
func (lh *LhjLogic) getSlotSymbolDistribution() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotSymbolDistribution.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &DataCfgLhj.SlotSymbolDistribution_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotSymbolDistTable := slotdata.(*DataCfgLhj.SlotSymbolDistribution_ARRAY)

	//一般輪軸設定
	for _, v := range slotSymbolDistTable.Items {
		if v.GetGroupId() == lh.slotPatternTable.GetSymbolGroupId() &&
			v.GetReelNum() <= lh.slotPatternTable.GetReelNum() {
			lh.normalSymbolDistTable = append(lh.normalSymbolDistTable, v)
		}
	}

	mlog.System("[weihao] csv slot的輪軸設定: %+v", lh.normalSymbolDistTable)
}

//取得csv的賠付獎金設定
func (lh *LhjLogic) getJackPotTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("JackPotTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &DataCfgLhj.JackpotTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	jackpotTableArr := slotdata.(*DataCfgLhj.JackpotTable_ARRAY)
	lh.jackpotTable = jackpotTableArr.GetItems()
}

//取得csv的賠付獎金設定
func (lh *LhjLogic) getSlotPayTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPayTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &DataCfgLhj.SlotPayTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPayTableArr := slotdata.(*DataCfgLhj.SlotPayTable_ARRAY)
	lh.payOutSymbole = make(map[string]*SlotPayOutSymbol, 0)

	for _, v := range slotPayTableArr.Items {
		//payTableArr := make([]*DataCfgLhj.SlotPayTable, 0)
		if int32(lh.slotPatternTable.GetPayTableGroupId()) == v.GetGroupId() {
			if arr, ok := lh.payOutSymbole[v.GetSymbol()]; ok {
				arr.payOutTable = append(arr.payOutTable, v)
			} else {
				lh.payOutSymbole[v.GetSymbol()] = new(SlotPayOutSymbol)
				lh.payOutSymbole[v.GetSymbol()].payOutTable = make([]*DataCfgLhj.SlotPayTable, 0)
				lh.payOutSymbole[v.GetSymbol()].payOutTable = append(lh.payOutSymbole[v.GetSymbol()].payOutTable, v)
			}
		}
	}

	//for symbolName, v := range lh.payOutSymbole {
	//	for _, payOut := range v.payOutTable {
	//		mlog.System("[weihao] 賠付獎金設定 map[%v] val[%+v] ", symbolName, payOut )
	//	}
	//}

}

//取得csv的賠付線設定
func (lh *LhjLogic) getPayLineTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPayLine.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &DataCfgLhj.SlotPayLine_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPatternTableArr := slotdata.(*DataCfgLhj.SlotPayLine_ARRAY)
	lh.payLineGroup = make([]*PayLineDetail, 0)

	for _, v := range slotPatternTableArr.Items {
		if v.GetGroupId() == lh.slotPatternTable.GetPayLineGroupId() {
			payLineDetail := new(PayLineDetail)
			payLineDetail.id = int32(v.GetId())
			payLineDetail.groupId = int32(v.GetGroupId())
			payLineDetail.lineId = int32(v.GetLineId())
			payLineDetail.payLineXY = make([]*PayLineXY, 0)

			xystr := strings.Split(v.GetContent(), ",")
			for _, s1 := range xystr {
				payLineG1 := new(PayLineXY)
				xyarr := strings.Split(s1, "_")
				x, _ := strconv.Atoi(xyarr[0])
				y, _ := strconv.Atoi(xyarr[1])
				payLineG1.pointX = int32(x)
				payLineG1.pointY = int32(y - 1)
				payLineDetail.payLineXY = append(payLineDetail.payLineXY, payLineG1)
			}

			lh.payLineGroup = append(lh.payLineGroup, payLineDetail)
		}
	}

	//for k, v := range lh.payLineGroup {
	//	for _, p := range v.payLineXY {
	//		mlog.System("[weihao] 賠付線設定k[%v] lineId[%v] xy[%+v] ", k, v.lineId, p)
	//	}
	//}

}

//用户进入
func (lh *LhjLogic) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	lh.ComputeGameLogicBase.UserEnter(gameuser, tableno, seatno)
	lh.getJackpotPool()
	lh.sendScene(gameuser)
}

//发送场景
func (lh *LhjLogic) sendScene(gameuser *gamelogic.GameUser) {

	lh.sendSettingMsg(gameuser)
	lh.SendMessageToUser(gameuser, lh.getStateMsg())
}

//发送设置
func (lh *LhjLogic) sendSettingMsg(gameuser *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJSettingID)
	pdata := new(netproto.LHJ_Setting)
	pdata.BetCount = lh.betvalue.betcount
	pdata.ReelSetting = lh.getReelSetting()
	pdata.RowCounts = proto.Int32(int32(lh.slotPatternTable.GetRowNum()))
	totalPayLine := len(lh.payLineGroup)
	pdata.TotalPayLine = proto.Int32(int32(totalPayLine))
	pdata.BigWinStep = lh.bigwinrule

	pmsg.MsgData = pdata

	lh.SendMessageToUser(gameuser, pmsg)
}

//場景輪軸設定
func (lh *LhjLogic) getReelSetting() []*netproto.LHJ_ReelSetting {
	result := make([]*netproto.LHJ_ReelSetting, 0)

	for reelId := 1; reelId <= int(lh.slotPatternTable.GetReelNum()); reelId++ {
		reelSetting := new(netproto.LHJ_ReelSetting)
		reelSetting.RealID = proto.Int32(int32(reelId))

		for _, s := range lh.normalSymbolDistTable {
			if int(s.GetReelNum()) == reelId {
				symbolGroup := new(netproto.LHJ_SymbolGroup)
				symbolId := s.GetId()
				symbolGroup.SymbolID = proto.Int32(int32(symbolId))
				symbolGroup.SymbolName = proto.String(s.GetSymbol())
				symbolGroup.Order = proto.Int32(int32(s.GetOrder()))
				reelSetting.SymbolGroup = append(reelSetting.SymbolGroup, symbolGroup)

				if lh.reelAndSymbol[reelId] == nil {
					reelSymbolObj := new(ReelAndSymbolSetting)
					reelSymbolObj.symboleIdMap = make(map[int32]*netproto.LHJ_SymbolGroup, 0)
					reelSymbolObj.symboleIdMap[int32(symbolId)] = symbolGroup
					lh.reelAndSymbol[reelId] = reelSymbolObj
				} else {
					lh.reelAndSymbol[reelId].symboleIdMap[int32(symbolId)] = symbolGroup
				}
				lh.reelAndSymbol[reelId].reelId = int32(reelId)

			}
		}
		result = append(result, reelSetting)
	}

	//log
	//for key, r := range lh.reelAndSymbol {
	//	if r == nil {
	//		continue
	//	}
	//	for skey, s := range r.symboleIdMap {
	//		mlog.System("[weihao] 場景輪軸設定 reelAndSymbol reel[%v] skey[%v] symbol[%v] ", key, skey, s.GetSymbolName())
	//	}
	//}

	return result
}

//断线重连
func (lh *LhjLogic) UserRecover(gameuser *gamelogic.GameUser) {
	//mlog.Debug("UserRecover")
	//lh.ComputeGameLogicBase.UserRecover(gameuser)
	//ud := lh.getUserDataByID(gameuser.UserID)
	//if ud != nil {
	//	ud.money += ud.user.Score - ud.moneylast
	//	ud.moneylast = ud.user.Score
	//}
	//
	//lh.sendScene(gameuser)
}

//用户离开
func (lh *LhjLogic) UserLeave(gameuser *gamelogic.GameUser) bool {
	mlog.System("[weihao] UserLeave gamestate[%v] ", lh.gamestate)
	lh.freeSpins = 0

	//if lh.gamestate != Wait {
	//	if lh.getUserDataByID(gameuser.UserID) != nil {
	//		mlog.System("用户在玩游戏还不能离开")
	//		return false
	//	}
	//}

	lh.gameEnd()

	lh.ChangeUserStateEnd(gameuser)
	return lh.ComputeGameLogicBase.UserLeave(gameuser)
}

//获取用户数据通过用户ID
func (lh *LhjLogic) getUserDataByID(userid int32) *lhjuserdata {
	for _, ud := range lh.userdata {
		if ud.user.UserID == userid {
			return ud
		}
	}

	return nil
}

//游戏结束
func (lh *LhjLogic) gameEnd() {
	lh.updateUserSorce()
	lh.setState(Wait)
	//if lh.zhuangID != 0 {
	//	user := lh.GetUserByUserID(lh.zhuangID)
	//	if user.IsOffLine() {
	//		lh.zhuangID = 0
	//	}
	//}
	lh.GameEndFunc()
}

//用户消息主处理函数
func (lh *LhjLogic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if !lh.CheckGameUserMessage(user) {
		lh.KickUserFunc(user.UserID, "非法操作")
		return
	}
	//不是给游戏逻辑服务器的消息

	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	lh.CheckUserAnimationCommited(user, gmsg, lh.action_str, nil)

	if gmsg.SClassID == int32(netproto.LHJ_GameMessageClassID_LHJGameStartID) {
		ok := lh.userBet(user, gmsg.MsgData.([]byte))
		if ok {
			lh.startSpin(user)
		}
		return
	}
}

func (lh *LhjLogic) userBet(user *gamelogic.GameUser, data []byte) bool {
	ust := &netproto.LHJ_StartBet{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析用户%v下注错误%v", user, err)
		return false
	}

	if lh.gamestate != Bet {
		lh.SendMessageToUser(user, lh.getBetRetMsg(user, -5, ust.GetMoney()))
		return false
	}

	if lh.freeSpins == 0 && ust.GetMoney() == 0 {
		mlog.System("[weihao] 沒有free spin, 下注不能0元")
		lh.SendMessageToUser(user, lh.getBetRetMsg(user, -6, ust.GetMoney()))
		return false
	}

	allmoney := ust.GetMoney() * int32(len(lh.payLineGroup))
	if int64(allmoney) > user.Score {
		lh.SendMessageToUser(user, lh.getBetRetMsg(user, -4, ust.GetMoney()))
		return false
	}

	//TODO weihao add log
	//lh.GameLog.AddGameStep("用户%d下注:%s;金額:%d, %v", user.UserID, lh.getBetTypeString(int(b.GetBetType())), betmon, b)

	lh.updateUserMoney(-allmoney)
	//lh.SendMessageToUser(user, lh.getBetRetMsg(user, 0, allmoney))
	ud := new(lhjuserdata)
	ud.user = user
	ud.userbetmoney = ust.GetMoney()
	lh.userdata = append(lh.userdata, ud)

	lh.SendMessageToUser(user, lh.getBetRetMsg(user, 0, ust.GetMoney()))

	return true
}

//取得下注金額
func (lh *LhjLogic) getUserBetMoney() int32 {
	return lh.userdata[0].userbetmoney
}

//下注狀態
func (lh *LhjLogic) getBetRetMsg(user *gamelogic.GameUser, ret int32, money int32) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJBetRetID)
	pdata := new(netproto.LHJ_BetRet)
	pdata.Money = proto.Int64(int64(money))
	pdata.Ret = proto.Int32(ret)
	pmsg.MsgData = pdata

	mlog.System("[weihao] 收到下注 [%+v]", pdata)

	return pmsg
}

func (lh *LhjLogic) startSpin(user *gamelogic.GameUser) {
	lh.getResultPicture()
	lh.setState(KaiJiang)
	lh.SendMessageToUser(user, lh.getLHJResultMsg())
	//lh.gameEnd()
	lh.action_str = lh.RegisterAnimation(GameEnd_Action, lh.endactiontime, lh.gameEnd)
}

//取得所有開獎結果
func (lh *LhjLogic) getLHJResultMsg() *msg.Message {
	defer util.PanicErrStack()

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJGameResult)
	pdata := new(netproto.LHJ_GameResultReel)

	mlog.System("[weihao] 取得所有開獎結果 [%v]", lh.kaiJiang)
	pdata.PayLineGroup = lh.getResultPayLine()
	//pdata.SymbolID = lh.kaiJiang	//lh.reelSymbolResult

	lh.reelSymbolResultRange(func(obj *ReelSymbol) {
		//mlog.System("[weihao] reelid[%v] 輪軸所有開獎結果 symbole[%v][%v]", obj.reelId, obj.symbolId, obj.symbolName)
		settingObj := new(netproto.LHJ_ReelSetting)
		settingObj.RealID = proto.Int32(obj.reelId)
		settingObj.RowIndex = proto.Int32(obj.rowId)
		symbolObj := new(netproto.LHJ_SymbolGroup)
		symbolObj.SymbolID = proto.Int32(obj.symbolId)
		symbolObj.SymbolName = proto.String(obj.symbolName)
		settingObj.SymbolGroup = append(settingObj.SymbolGroup, symbolObj)
		pdata.ReelSetting = append(pdata.ReelSetting, settingObj)
	})

	pdata.WinMoney = proto.Int32(lh.getWinMoneyResult())

	//betRet := new(netproto.LHJ_BetRet)
	//betRet.Ret = proto.Int32(0)
	//betRet.Money = proto.Int64(int64(lh.getUserBetMoney()))
	//pdata.BetRet = betRet
	pmsg.MsgData = pdata

	//mlog.System("[weihao] 取得所有開獎結果[%+v]", pdata)

	//for _, v := range pdata.PayLineGroup {
	//	for _, p := range v.PayLineDetail {
	//		mlog.System("[weihao] 取得所有開獎結果 group[%v] [%+v]", v.GetGroupId(), p)
	//	}
	//}

	return pmsg
}

//取得結算金額
func (lh *LhjLogic) getWinMoneyResult() int32 {
	sumwinmoney := int32(0)

	if len(lh.payLineResult) == 0 {
		mlog.Error("沒中獎")
	}

	lh.calJackpotPool()

	//任意出現三個或三個以上的Scatter時會進入Scatter Bonus Spin模式
	lh.getFreeSpinResult()

	//由左開始的賠付線連線獎金結算
	for lineid, resultArr := range lh.payLineResult {
		jackpotmoney := int32(0)
		//從左開始
		leftFirstSymbol := resultArr[0].SymbolGroup[0].GetSymbolName()
		sameCount := lh.getSameSymbolCount(LEFT, resultArr, leftFirstSymbol)
		payOut, ok := lh.getSymbolPayOut(LEFT, CHIP, leftFirstSymbol, sameCount)

		//wild開始,當有連續wild, 採用賠率大的
		sameCount, payOut, jackpotmoney = lh.getFirstWildPayOut(leftFirstSymbol, resultArr, payOut)

		//wild開始,非連續wild
		payOut, ok = lh.getNonContinuWildPayOut(leftFirstSymbol, resultArr, payOut, ok)

		if ok || jackpotmoney > 0 {
			if len(lh.userdata) == 0 {
				mlog.Error("沒有玩家")
			}
			winmoney := payOut.GetPayout() * lh.getUserBetMoney()
			sumwinmoney += winmoney
			sumwinmoney += jackpotmoney

			mlog.System("[weihao] done取得結算金額 賠付線id[%v] symbole[%v]  連線數量[%v] 下注[%v] 賠付倍率[%v] 結算金額[%v] jackpotmoney[%v]",
				lineid, payOut.GetSymbol(), payOut.GetCount(), lh.getUserBetMoney(), payOut.GetPayout(), winmoney, jackpotmoney)
		}
	}

	mlog.System("[weihao] 總結算金額[%v]", sumwinmoney)

	lh.updateUserMoney(sumwinmoney)
	return sumwinmoney
}

func (lh *LhjLogic) getJackpotMoney(leftFirstSymbol string) int32 {
	payOut2, ok2 := lh.getSymbolPayOut(LEFT, JACKPOT, leftFirstSymbol, int32(lh.slotPatternTable.GetReelNum()))
	if ok2 {
		return lh.getJackpotPayOut(payOut2)
	}
	return 0
}

//wild開始,當有連續wild, 採用賠率大的
func (lh *LhjLogic) getFirstWildPayOut(leftFirstSymbol string, resultArr []*PayLineResult, payOut *DataCfgLhj.SlotPayTable) (int32, *DataCfgLhj.SlotPayTable, int32) {
	sameCount := int32(0)
	jackpotmoney := int32(0)
	if lh.isEqualSymbole(WILD, leftFirstSymbol) {
		done := false
		sameCount = int32(0)
		newSymbolName := ""
		for _, group := range resultArr {
			for _, v := range group.SymbolGroup {

				if !done {
					newSymbolName = v.GetSymbolName()
				}

				if !lh.isEqualSymbole(WILD, v.GetSymbolName()) && v.GetSymbolName() != newSymbolName {
					newSymbolName = ""
					break
				}

				sameCount++
				if !lh.isEqualSymbole(WILD, newSymbolName) {
					payOut2, ok2 := lh.getSymbolPayOut(LEFT, CHIP, newSymbolName, sameCount)
					if ok2 && payOut.GetPayout() < payOut2.GetPayout() &&
						(lh.isEqualSymbole(WILD, payOut.GetSymbol()) || payOut.GetSymbol() == payOut2.GetSymbol()) {
						payOut = payOut2
						done = true
					} else {
						done = true
					}
				} else if sameCount == int32(lh.slotPatternTable.GetReelNum()) { //一條wild,中了jackpot
					jackpotmoney = lh.getJackpotMoney(leftFirstSymbol)
					payOut.Payout = proto.Int32(0)
				}
			}
		}
	}
	return sameCount, payOut, jackpotmoney
}

//發放jackpot payout獎池
func (lh *LhjLogic) getJackpotPayOut(payOut *DataCfgLhj.SlotPayTable) int32 {
	jackpoObj := linq.From(lh.jackpotList).
		FirstWithT(func(j *JackpotPool) bool {
			return j.jackpotId == payOut.GetPayout()
		}).(*JackpotPool)

	betRate := float32(lh.betrate)
	jprate := float32(float32(lh.jprate))
	jackpotMoney := int32(math.Floor(float64(float32(jackpoObj.syspool) * betRate / jprate)))
	jackpoObj.jpAmountOut += int64(jackpotMoney)

	jpTable := linq.From(lh.jackpotTable).
		FirstWithT(func(j *DataCfgLhj.JackpotTable) bool {
			return j.GetId() == payOut.GetPayout()
		}).(*DataCfgLhj.JackpotTable)

	syspool := jackpoObj.syspool - jackpoObj.jpAmountOut //現在水池實際金額
	mlog.System("[weihao] 發放jackpot before syspool[%v] jpAmountOut[%v] ", jackpoObj.syspool, jackpoObj.jpAmountOut)
	////當計算後的syspool小於startup值就用startup當syspool, 此行為是初始化syspool
	if syspool < int64(jpTable.GetStartUp()) {
		syspool = int64(jpTable.GetStartUp())
		jackpoObj.syspool = syspool
		jackpoObj.jpAmountOut = 0
		jackpoObj.spinStack = 0
	}

	mlog.System("[weihao] 計算jackpot 每次Spin貢獻值[%v] 累積的貢獻值[%v] 現在水池實際金額[%v] 玩家得獎[%v]",
		jackpoObj.spinStack, jackpoObj.spinStack, jackpoObj.syspool, jackpotMoney)

	return jackpotMoney
}

//累計spin貢獻值
func (lh *LhjLogic) calJackpotPool() {
	//TODO linq
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	for _, jackpotObj := range lh.jackpotTable {
		if jackpotObj.GetGroupId() == int32(jackpotGroupId) {
			increment := util.Decimal((float64(jackpotObj.GetIncrement()) / 10000), 3) //Jackpot每次Spin增加的比例
			betmoney := int32(math.Abs(float64(lh.money)))
			spinINC := util.Decimal(float64(increment)*float64(betmoney), 3) //每次Spin貢獻值
			mlog.System("[weihao] 每次Spin貢獻值[%v] increment[%v] betmoney[%v] = [%v] ", spinINC, increment, betmoney, (float64(increment) * float64(betmoney)))
			jackpotPoolObj := linq.From(lh.jackpotList).
				FirstWithT(func(j *JackpotPool) bool {
					return j.jackpotId == jackpotObj.GetId()
				}).(*JackpotPool)

			jackpotPoolObj.level = jackpotObj.GetLevel()
			jackpotPoolObj.spinStack += spinINC
			//syspool = 0
			syspool := int64(jackpotObj.GetStartUp()) + int64(math.Floor(float64(jackpotPoolObj.spinStack))) - jackpotPoolObj.jpAmountOut //現在水池實際金額
			mlog.System("[weihao] 計算syspool , syspool[%v] spinStack[%v] jpOut[%v]", syspool,
				int64(math.Floor(float64(jackpotPoolObj.spinStack))), jackpotPoolObj.jpAmountOut)
			//當計算後的syspool小於startup值就用startup當syspool, 此行為是初始化syspool
			if jackpotPoolObj.syspool < int64(jackpotObj.GetStartUp()) {
				syspool = int64(jackpotObj.GetStartUp()) + int64(util.Decimal(float64(spinINC), 0))
				//jackpotPoolObj.spinStack = 0
				//jackpotPoolObj.jpAmountOut = 0
			}
			jackpotPoolObj.syspool = syspool

			mlog.System("[weihao] 計算jp jackpotid[%v] syspool[%v] 累計後spin貢獻值[%v] 本次增加[%v] 下注金額[%v] increment[%v]",
				jackpotObj.GetId(), jackpotPoolObj.syspool, jackpotPoolObj.spinStack, spinINC, betmoney, increment)
		}
	}
}

//wild開始,非連續wild
func (lh *LhjLogic) getNonContinuWildPayOut(leftFirstSymbol string, resultArr []*PayLineResult, payOut *DataCfgLhj.SlotPayTable, ok bool) (*DataCfgLhj.SlotPayTable, bool) {
	if !ok && lh.isEqualSymbole(WILD, leftFirstSymbol) {
		for _, arr := range resultArr {

			pSymbolGroup := linq.From(arr.SymbolGroup).
				FirstWithT(func(pObj *netproto.LHJ_SymbolGroup) bool {
					return !lh.isEqualSymbole(WILD, pObj.GetSymbolName())
				},
				)

			if pSymbolGroup != nil {
				leftFirstSymbol = pSymbolGroup.(*netproto.LHJ_SymbolGroup).GetSymbolName()

				sameCount := lh.getSameSymbolCount(LEFT, resultArr, leftFirstSymbol)
				payOut, ok = lh.getSymbolPayOut(LEFT, CHIP, leftFirstSymbol, sameCount)
				//break
				return payOut, ok
			}
		}
	}

	return payOut, ok
}

//任意出現三個或三個以上的Scatter時會進入Scatter Bonus Spin模式
func (lh *LhjLogic) getFreeSpinResult() {
	scatterCount := int32(0)

	lh.reelSymbolResultRange(func(symbolObj *ReelSymbol) {
		if lh.isEqualSymbole(SCATTER, symbolObj.symbolName) {
			scatterCount++
			payOut, ok := lh.getSymbolPayOut(ANY, FREE_SPINS, symbolObj.symbolName, scatterCount)
			if ok {
				lh.freeSpins = payOut.GetPayout()
			}
		}
	})
}

//循環開獎結果每個reel的symbol obj
func (lh *LhjLogic) reelSymbolResultRange(f func(p *ReelSymbol)) {
	for reelId, symbolArr := range lh.reelSymbolResult {
		for rowId, symboldId := range symbolArr {
			reelSymbol := new(ReelSymbol)
			reelSymbol.reelId = reelId
			reelSymbol.rowId = int32(rowId + 1)
			reelSymbol.symbolId = symboldId
			symbolName := lh.getSymbolName(reelId, symboldId)
			reelSymbol.symbolName = symbolName

			f(reelSymbol)
		}
	}
}

//取得Symbole數量的獎金
func (lh *LhjLogic) getSymbolPayOut(linkType, payType, symboleName string, count int32) (*DataCfgLhj.SlotPayTable, bool) {
	result := new(DataCfgLhj.SlotPayTable)
	okResult := false

	if v, ok := lh.payOutSymbole[symboleName]; ok {
		//取得唯一的payout資料, 指定方向與數量
		result2 := linq.From(v.payOutTable).
			FirstWithT(
				func(arr *DataCfgLhj.SlotPayTable) bool {
					return arr.GetLinkType() == linkType && arr.GetPayType() == payType && arr.GetCount() == count
				},
			)

		if result2 != nil {
			result = result2.(*DataCfgLhj.SlotPayTable)
			okResult = true
		}

	}

	return result, okResult
}

//取得連線符號的數量
func (lh *LhjLogic) getSameSymbolCount(linkeType string, r []*PayLineResult, firstSymbol string) int32 {
	sameCount := int32(0)
	for _, v := range r {
		isWild := lh.isEqualSymbole(WILD, v.SymbolGroup[0].GetSymbolName())
		if (firstSymbol == v.SymbolGroup[0].GetSymbolName()) || isWild {
			sameCount++
			//if isWild {
			//	//TODO Wild_Mutiplier獎金加倍,情境: p1,w2,w2,p4,p5 (公式:p1連續3個的賠率40 * 下注金額 * (w2的數量=2+2)
			//}
		} else {
			break
		}
	}
	return sameCount
}

//取得開獎的起始圖
func (lh *LhjLogic) getResultPicture() []int32 {
	result := make([]int32, 0)

	for i := int32(1); i <= int32(lh.slotPatternTable.GetReelNum()); i++ {
		result = append(result, lh.randSymbol(i, 1))
	}

	lh.kaiJiang = result
	return result
}

//
func (lh *LhjLogic) getOrderNumber(reelid ,orderId int32) int32 {
	hasOrder := false
	orderId = orderId + 1
	for _, v := range lh.normalSymbolDistTable {
		if int32(v.GetOrder()) == orderId && reelid == int32(v.GetReelNum()) {
			hasOrder = true
			break
		}
	}
	if !hasOrder {
		orderId = 1
	}
	return orderId
}

//依照權重取得開講符號
func (lh *LhjLogic) randSymbol(reelid int32, orderId int32) int32 {
	result := int32(0)

	totalWeight := 0
	startIndex := 0
	hasOrder := false

	for _, v := range lh.normalSymbolDistTable {
		if int32(v.GetOrder()) == orderId && reelid == int32(v.GetReelNum()) {
			hasOrder = true
			break
		}
	}
	if !hasOrder {
		orderId = 1
	}


	for _, v := range lh.normalSymbolDistTable {
		if int32(v.GetOrder()) != orderId {
			continue
		}
		if startIndex == 0 {
			startIndex = int(v.GetId())
		}
		if reelid == int32(v.GetReelNum()) {
			totalWeight += int(v.GetProportion())
		}
	}


	endIndex := startIndex + totalWeight

	if startIndex == 0 && endIndex == 0 {
		mlog.Fatal("is zero")
	}

	rnd := util.RndInt(startIndex, endIndex)

	checkRnd := startIndex
	for _, v := range lh.normalSymbolDistTable {
		if int32(v.GetOrder()) != orderId {
			continue
		}
		if reelid == int32(v.GetReelNum()) {
			checkRnd += int(v.GetProportion())
			if checkRnd >= rnd {
				return int32(v.GetId())
			}
		}
	}

	if result == 0 {
		mlog.Fatal("[weihao] 開講符號 不可以為0")
	}

	return result
}

//取得賠付線
func (lh *LhjLogic) getResultPayLine() []*netproto.LHJ_PayLineGroup {
	//mlog.System("[weihao] B取得開獎的起始圖[%resultArr] ", lh.kaiJiang)

	result := make([]*netproto.LHJ_PayLineGroup, 0)

	lh.reelSymbolResult = lh.getReelSymbolResultArr()

	//log
	lh.reelSymbolResultRange(func(obj *ReelSymbol) {
		mlog.System("[weihao] reelid[%v] 輪軸所有開獎結果 symbole[%v][%v]", obj.reelId, obj.symbolId, obj.symbolName)
	})

	lh.payLineResult = lh.getPayLineResult()
	//payLineId := int32(0)
	var startSymboleName string
	startSymboleName = fmt.Sprintf("")

	//TODO 重構移到getPayLineResult()裡面
	blink := true
	for lineId, resultArr := range lh.payLineResult {
		startSymboleIsWild := true

		for _, v := range resultArr {
			payLineGroup1 := new(netproto.LHJ_PayLineGroup)
			payLineGroup1.PayLineID = proto.Int32(lineId)

			if v.reelId == 1 {
				blink = true
				//payLineId = lineId
				startSbId := v.SymbolGroup[0].GetSymbolID()
				startSymboleName = lh.getSymbolName(v.reelId, startSbId)
				//mlog.Debug("[weihao] startSymboleName[%resultArr]", startSymboleName)
			}

			for _, symbolVal := range v.SymbolGroup {
				payLineList1 := new(netproto.LHJ_PayLineDetail)
				payLineList1.ReelID = proto.Int32(v.reelId)
				payLineList1.SymbolID = proto.Int32(symbolVal.GetSymbolID())
				nextSymbolName := lh.getSymbolName(v.reelId, symbolVal.GetSymbolID())

				//第一軸的是wild, 就將第一軸的符號相同第二軸的下一個symbol
				if startSymboleIsWild && lh.isEqualSymbole(WILD, startSymboleName) && !lh.isEqualSymbole(WILD, nextSymbolName) && v.reelId >= 2 {
					startSymboleName = nextSymbolName
					//startSymboleIsWild = false
				} else if startSymboleIsWild && lh.isEqualSymbole(WILD, nextSymbolName) && v.reelId >= 2 {
					if !lh.isEqualSymbole(WILD, nextSymbolName) || startSymboleName != nextSymbolName {
						startSymboleIsWild = false
					}
					startSymboleName = nextSymbolName
				}

				if startSymboleName == nextSymbolName {
					payLineList1.Blink = proto.Bool(blink)
					payLineList1.Line = proto.Bool(true)
				} else {
					blink = false
					//沒中獎但要畫線
					payLineList1.Blink = proto.Bool(blink)
					payLineList1.Line = proto.Bool(true)
				}

				payLineGroup1.PayLineDetail = append(payLineGroup1.PayLineDetail, payLineList1)
				mlog.System("[weihao] 中獎賠付線[%v] 起始symbol[%v] reelid[%v] symbol[%v] Blink[%v] Line[%v] ",
					lineId, startSymboleName, v.reelId, nextSymbolName, payLineList1.GetBlink(), payLineList1.GetLine())

			}

			result = append(result, payLineGroup1)
			//mlog.System("[weihao] symbol[%resultArr] 中獎賠付線 : %+resultArr",
			//	startSymboleName, payLineGroup1.PayLineDetail)
		}

	}

	return result
}

//取得輪軸所有開獎結果 map[reelid][]symbolie
func (lh *LhjLogic) getReelSymbolResultArr() map[int32][]int32 {
	reelSymbolResult := make(map[int32][]int32, 0)
	for reelid, kaiJiangSymbolID := range lh.kaiJiang {
		reelid += 1

		if symbolObj, ok := lh.reelAndSymbol[reelid].symboleIdMap[kaiJiangSymbolID]; ok {
			symbolArr := make([]int32, 0)
			symbolArr = append(symbolArr, symbolObj.GetSymbolID())

			mlog.System("[weihao] TODO A 輪軸[%v] 開獎符號id[%v][%v] symbolKey[%v] ",
				reelid, kaiJiangSymbolID, symbolObj.GetSymbolName(), symbolObj.GetSymbolID())

			newOrder := lh.getOrderNumber(int32(reelid), symbolObj.GetOrder())
			symbolArr = append(symbolArr, lh.randSymbol(int32(reelid), newOrder))
			newOrder = lh.getOrderNumber(int32(reelid), newOrder)
			symbolArr = append(symbolArr, lh.randSymbol(int32(reelid), newOrder))
			reelSymbolResult[lh.reelAndSymbol[reelid].reelId] = symbolArr
		}
	}
	return reelSymbolResult
}

//取得row 0 之後的開講項目
func (lh *LhjLogic) getContinuRowSymbol(symbolObj *netproto.LHJ_SymbolGroup, reelid int, symbolArr []int32) []int32 {
	testIndex := 0
	for i := 1; i < int(lh.reelRow); i++ {

		newSymbolIndex := symbolObj.GetSymbolID() + int32(i)
		newSymbolIndex, testIndex = symbolIndexOutOfRange(newSymbolIndex, lh, reelid, i, testIndex)

		//mlog.Error("[weihao] 確認, reel[%v] newSymbolIndex[%v] len[%v] val[%v] ",
		//	reelid, newSymbolIndex, len(lh.reelsetting[reelid].SymbolGroup), lh.reelsetting[reelid])

		//symbolArr = append(symbolArr, lh.reelsetting[reelid].SymbolGroup[newSymbolIndex].GetSymbolID())
		symbolArr = append(symbolArr, lh.reelAndSymbol[reelid].symboleIdMap[newSymbolIndex].GetSymbolID())

		mlog.System("[weihao] TODO B 輪軸[%v] 開獎符號id[%v][%v] ",
			lh.reelAndSymbol[reelid].reelId, lh.reelAndSymbol[reelid].symboleIdMap[int32(newSymbolIndex)].GetSymbolID(),
			lh.reelAndSymbol[reelid].symboleIdMap[int32(newSymbolIndex)].GetSymbolName())
	}
	return symbolArr
}

//輪軸讀取超過最後項目就循環第一個
func symbolIndexOutOfRange(newSymbolIndex int32, lh *LhjLogic, reelid int, i int, testIndex int) (int32, int) {
	if v, ok := lh.reelAndSymbol[reelid].symboleIdMap[newSymbolIndex]; ok {
		return v.GetSymbolID(), testIndex
	}

	min := int32(9999)

	for _, v := range lh.reelAndSymbol[reelid].symboleIdMap {
		if min > v.GetSymbolID() {
			min = v.GetSymbolID()
		}
	}

	v, _ := lh.reelAndSymbol[reelid].symboleIdMap[min+int32(testIndex)]
	testIndex++

	return v.GetSymbolID(), testIndex
}

//取得中獎結果的賠付線
func (lh *LhjLogic) getPayLineResult() map[int32][]*PayLineResult {
	payLineResultArr := make(map[int32][]*PayLineResult, 0)
	temp := make([]map[int32][]*PayLineResult, 0)
	mlog.Error("[weihao] TODO 1.需要的符號數量相同才中獎 未完成.")
	mlog.Error("[weihao] TODO 2.符號的組合模式 未完成方向起始點.")
	for y := 0; y < int(lh.reelRow); y++ { //從左邊開始
		leftReelId := lh.reelAndSymbol[1].reelId
		initSymbolId := lh.reelSymbolResult[leftReelId][y]
		initSymbolName := lh.getSymbolName(leftReelId, initSymbolId)
		//mlog.System("[weihao] 起始符號[%v][%v] reeldId[%v]y[%v] ", initSymbolId, initSymbolName, leftReelId, y)

		ok, tempResultArr := lh.checkPayLine(0, initSymbolName, payLineResultArr)
		//payLineResultArr = append(payLineResultArr, tempResultArr...)
		//payLineResultArr[leftReelId] = tempResultArr[leftReelId]
		if ok {
			temp = append(temp, tempResultArr)
		}
	}

	//array轉換map[lineid]格式
	for _, arr := range temp {
		for lineid, v := range arr {
			payLineResultArr[lineid] = v
		}
	}

	//log
	for _, arr := range payLineResultArr {
		for _, v := range arr {
			for _, s := range v.SymbolGroup {
				if v.reelId == 1 {
					mlog.System("[weihao] 取得中獎結果的賠付線 LineId[%v] reel[%v] symbole[%v][%v]",
						v.lineId, v.reelId, s.GetSymbolID(), lh.getSymbolName(v.reelId, s.GetSymbolID()))
				}
			}
		}
	}

	return payLineResultArr
}

func (lh *LhjLogic) getSymbolName(reelId, symbolId int32) string {
	var result string

	if v, ok := lh.reelAndSymbol[reelId].symboleIdMap[symbolId]; ok {
		return v.GetSymbolName()
	} else {
		return result
	}

	return result
}

//是否為某個相同特殊符號:WILD, SCATTER
func (lh *LhjLogic) isEqualSymbole(equalName string, symbolName string) bool {
	result := false

	if symbolName == "" {
		//mlog.Error("[weihao] iswile symename[%v] is empty.", symbolName)
		return false
	}

	name := symbolName[0:1]
	if name == equalName {
		result = true
	}

	return result
}

//賠付現規則, 逐一檢查
func (lh *LhjLogic) payLineCheckRange(f func(p *PayLineDetail)) {
	for _, lineGroup := range lh.payLineGroup {
		f(lineGroup)
	}
}

//檢核賠付線是否符合設定
func (lh *LhjLogic) checkPayLine(y int, initSymbolName string, payLineResultArr map[int32][]*PayLineResult) (bool, map[int32][]*PayLineResult) {
	//defer util.PanicErrStack()
	defer func() {
		err := util.PanicErrStack()
		if err != nil {
			mlog.Fatal("系統掛掉")
		}
	}()

	bonusCount := make(map[string]int, 0)
	resultArr := make(map[int32][]*PayLineResult, 0)

	lh.payLineCheckRange(func(lineGroup *PayLineDetail) {
		tempResultArr := make([]*PayLineResult, 0)
		startSymbole := initSymbolName
		startSymboleIsWild := true

		//hasPayLine := exsitPayLine(payLineResultArr, lineGroup)
		//if hasPayLine {
		//	//break
		//	//return
		//	mlog.Error("[weihao] exsitPayLine")
		//}

		for _, p := range lineGroup.payLineXY {
			rid := lh.reelAndSymbol[p.pointX].reelId
			//mlog.System("[weihao] 查錯 : len[%v] reel[%v] y[%v] ridval[%+v]",
			//	len(lh.reelSymbolResult), rid, p.pointY, lh.reelSymbolResult[rid])

			if lh.reelSymbolResult[rid] == nil || len(lh.reelSymbolResult[rid]) == 0 {
				mlog.System("stop")
			}

			symbolId := lh.reelSymbolResult[rid][p.pointY] //TODO Check
			nextSymbolName := lh.getSymbolName(rid, symbolId)

			if rid == 1 && startSymbole != nextSymbolName {
				break
			}

			//mlog.System("[weihao] before lineId[%v]檢核賠付線是否符合設定 symbol[%v] lineid[%v] after reelid[%v]x[%v]y[%v]  ",
			//	lineGroup.lineId, nextSymbolName, lineGroup.lineId, rid, p.pointX, p.pointY)
			//payOut, ok := lh.getSymbolPayOut(LEFT, CHIP, firstSymbol, sameCount)

			if startSymbole == nextSymbolName || lh.isEqualSymbole(WILD, startSymbole) || lh.isEqualSymbole(WILD, nextSymbolName) || len(tempResultArr) >= int(2) {
				//mlog.System("[weihao] After檢核賠付線 lineid[%v] symbol[%v] after reeldi[%v]x[%v] y[%v] ",
				//	lineGroup.lineId, nextSymbolName, rid, p.pointX, p.pointY)

				bonusCount[startSymbole]++
				symbol2 := new(netproto.LHJ_SymbolGroup)
				symbol2.SymbolID = proto.Int32(symbolId)
				symbol2.SymbolName = proto.String(lh.getSymbolName(rid, symbolId))
				payLine2 := new(PayLineResult)
				payLine2.lineId = lineGroup.lineId
				payLine2.reelId = rid
				payLine2.SymbolGroup = append(payLine2.SymbolGroup, symbol2)
				tempResultArr = append(tempResultArr, payLine2)
			} else {
				mlog.Debug("[weihao] 無連線, 結束檢核lineid[%v] ", lineGroup.lineId)
				break
			}

			//第一軸的是wild, 就將第一軸的符號相同第二軸的下一個symbol
			if startSymboleIsWild && lh.isEqualSymbole(WILD, startSymbole) && !lh.isEqualSymbole(WILD, nextSymbolName) && rid >= 2 {
				startSymbole = nextSymbolName
				startSymboleIsWild = false
			}

			if len(tempResultArr) >= int(lh.slotPatternTable.GetReelNum()) {
				sameCount := lh.getSameSymbolCount(LEFT, tempResultArr, startSymbole)
				_, ok := lh.getSymbolPayOut(LEFT, CHIP, startSymbole, sameCount)
				if ok {
					resultArr[lineGroup.lineId] = tempResultArr
					//mlog.System("[weihao] [%v]成功結束檢核lineid[%v] sameCount[%v]", startSymbole, lineGroup.lineId, sameCount)
				} else {
					_, ok := lh.getSymbolPayOut(LEFT, JACKPOT, startSymbole, sameCount)
					if ok {
						resultArr[lineGroup.lineId] = tempResultArr
					}
				}
				startSymbole = initSymbolName
			}
		}
	})

	return true, resultArr
}

//已經存在payline就不需要在檢查是否payline符合
func exsitPayLine(payLineResultArr map[int32][]*PayLineResult, lineGroup *PayLineDetail) (hasPayLine bool) {
	if _, ok := payLineResultArr[lineGroup.lineId]; ok {
		mlog.System("[weihao] hasPayLine")
		return true
	}

	return false
}

//換牌
func (lh *LhjLogic) ExchangeMaxCard(userid int32) {

}

//更新玩家的钱
func (lh *LhjLogic) updateUserMoney(money int32) {
	lh.money += int64(money)
	lh.winmoney += int64(money)
	//mlog.System("[weihao] 更新玩家的钱 lh.money[%v]", lh.money)
}

//更新用戶分數
func (lh *LhjLogic) updateUserSorce() {
	for _, ud := range lh.userdata {
		if lh.winmoney != 0 {
			if !ud.user.IsRobot() {
				lh.UpdateUserScoreFunc(ud.user.UserID, lh.winmoney, 0)
			}
		}
	}
}

//遊戲要付費或免費
func (lh *LhjLogic) sendGameFreeOrPayMsg() {
	user := lh.GetUserBySeatNo(0)
	if user == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJFreeOrPayID)

	pdata := new(netproto.LHJ_FreeOrPayMsg)
	pdata.FreeCount = proto.Int32(lh.freeSpins)

	pmsg.MsgData = pdata

	lh.SendMessageToUser(user, pmsg)
}
