package dfdclogic

import (
	"config"
	"event"
	"fmt"
	"gamelogic"
	"math"
	"math/rand"
	"mlog"
	"msg"
	"netproto"
	"strconv"
	"strings"
	"util"

	"example.invalid/ahmetb/go-linq"
	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot/protodata"
)

//動畫名稱
const (
	GameStartAnimation  = "gamestart"
	GameKaiJiang_Action = "kaijiang"
	GameEnd_Action      = "end"
)

//階段狀態
const (
	Bet            = iota //下注状态
	KaiJiang              //开奖状态
	KaiJiangAction        //开奖动画
	EndAction             //结算动画
	Wait                  //等待游戏开始
)

//PayType獎勵方式
const (
	CHIP       = "chip"
	FREE_SPINS = "free_spins"
	FREE_GAME  = "free_Game"
	JACKPOT    = "jackpot"
)

//符號定義
const (
	WILD    = "w" //多福多財為fubat
	SCATTER = "s" //多福多財為鑼
)

//賠付線類型
const (
	FIXEDMODE = int32(0)
	ALLMODE   = int32(1)
)

//符號組合模式
const (
	LEFT     = "left"
	ANY      = "any"
	RIGHT    = "right"
	STRAIGHT = "straight"
)

//連線賠付方式
const (
	ALL   = "all"   //全線賠付
	FIXED = "fixed" //指定PayLine

)

//Jackpot獎項分級
const (
	GRAND = 1
	MAJOR = 2
	MINOR = 3
	MINI  = 4
)

//Jackpot獎項分級
const (
	GrandLevel = "Grand"
	MajorLevel = "Major"
	MinorLevel = "Minor"
	MiniLevel  = "Mini"
)

//歷程紀錄類型
const (
	MAIN     = 1 //主遊戲
	BONUS    = 2 //JackpotGame
	FREE     = 3 //FreeGame
	JPNOLINK = 4 //單機 JP
	JPLINK   = 5 //連線 JP
)

//遊戲歷程紀錄類型
const (
	REEL = 1 //輪軸資訊
	OPEN = 2 //翻圖案，三個一樣結束型
)

//遊戲種類
const (
	chip           = "chip"           //直接給錢
	chipXrate      = "chipXrate"      //直接給錢
	free_spins     = "free_spins"     //免費的spin，不換場景
	free_game      = "free_game"      //小遊戲，換場景的free spin
	jackpot        = "jackpot"        //遊戲的彩金
	mysteryjackpot = "mysteryjackpot" //觸發方式為隨機的彩金
	wheel          = "wheel"          //轉盤小遊戲
	miniSlot       = "miniSlot"       //1*N 迷你老虎雞
	pickgame       = "pickgame"       //選獎勵的遊戲
	pickghost      = "pickghost"      //選鬼牌的遊戲
	selectgamemode = "selectgamemode" //選free spin的遊戲
)

type DFDCLogic struct {
	*gamelogic.ComputeGameLogicBase
	DFDCConfig            //多福多財資料設定擋csv
	cfgs                  *config.ConfigGroup
	userdata              []*dfdcuserdata //用户数据
	money                 int64           //当前的钱
	winmoney              int64           //本次进入房间输赢的钱
	coupon                int64           //使用的coupon
	isCoupon              bool
	updateMoney           int64
	betvalue              *betconfig                 //下注配置文件
	action_str            string                     //当前动画
	gamestate             int32                      //游戏状态
	kaiJiang1             []int32                    //第1行符號id (待優化: 未來若做真實輪軸 只會需要kaiJiang)
	kaiJiang2             []int32                    //第2行符號id
	kaiJiang3             []int32                    //第3行符號id
	reelSymbolResult      map[int32][]int32          //map[reelId][]int32{symboleIdMap}取得輪軸所有開獎結果, reel從1開始, symbol index從0開始
	payLineResult         map[int32][]*PayLineResult //map[LineId][]*賠付線結果
	freeSpins             int32                      //可玩幾次freespins
	slotpattern           int32                      //slot類型
	showWildWinMoney      []*int32                   //表演金額
	bigwinrule            []int32                    //client動畫播放階段,以Bet為主
	bigwinnotify          int32                      //當下注16, Total=6400(16*400) 得獎超過6400才廣播.
	endactiontime         int32
	goldenSymbolRate      []int32          //黃金符號賠率
	resultSymbol          map[int][]string //開獎符號[reel][]int{"符號1","符號2","符號3",}
	jackPotGameFlag       bool             //是否觸發jackpotGame
	reelNum               int32            //輪軸數目
	rowNum                int32            //行數
	symbolAttributesGroup int32            //SymbolAttributes的groupId
	mainGameTableGroup    int32            //MainGameTable中的groupId 定義Pattern使用的符號參數
	payTableGroup         int32            //payTableGroup群組(符號群組)
	lineType              string           //payline類型
	payLineGroupId        int32            //payLine群組
	symbolGroup           int32            //符號群組
	kaijiangsymbol        []string         //開獎符號名稱
	symbolIdRes           []int32          //該連線符號ID列表
	lineCount             map[string]int32 //該符號連線數
	payiconCount          map[string]int32 //符號連續幾個
	payRate               map[string]int32 //符號賠率
	freeSpinGroup         int32            //freeSpin群組
	bonusGameRecord       []*BonusRecord   //玩家bonus game紀錄
	freeGameMoney         int64            //玩家freeGame贏錢金額
	jackpotdataDetail     []*JackpotData   //jackpot紀錄
	freespinRecord        FrspRecord       //從DB獲得的free spin資訊紀錄
	bgSetChoose           []*BonusRecord   //此局玩家設定過的bonus game紀錄
	bonusProcess          []int32          //玩家bonus game icon順序
	jackpotAward          int64            //jackpot獎金
	wildPayRate           map[string]int32 //符號賠率
	freeSpinLimit         int32            //Free Spin最多可累積的數量
	freeNow               bool             //此局是否為免費場
	jackpotlevel          int32            //獲得獎金的層級
	jpFlag                bool             //JPGame標記
	jpFlagUser            int32            //JPGame標記
	betlimitmoney         int64            //下注限制
}

//多福多財資料設定擋csv
type DFDCConfig struct {
	payLineGroup          []*PayLineDetail                    //所有賠付線Data
	slotPatternTable      *protodata.SlotPattern              //Slot類型
	symbolAttributesTable []*SymbolAttributes                 //黃金符號設定
	normalSymbolDistTable []*protodata.SlotSymbolDistribution //一般輪軸設定
	payOutSymbole         map[string]*SlotPayOutSymbol        //map[symboleName]SlotPayOutSymbol
	reelAndSymbol         [10]*ReelAndSymbolSetting           //[reelid]的array, 用map方式找symbolid取得symbol name
	jackpotTable          []*protodata.JackpotTable
	bonusTable            []*protodata.BonusTable
	mainGameTable         []*protodata.MainGameTable //主遊戲設定
	freeSpinTable         []*protodata.FreeSpinTable //free spin設定
	pickGameTable         []*protodata.PickGameTable //收集類型設定
	slotPayTable          []*protodata.SlotPayTable  //符號定義內容
}

type SymbolAttributes struct {
	id                int32  //流水號
	groupId           int32  //群組編號
	symbol            string //符號類型
	isGoldSymbol      int32  //是否黃金符號
	goldCondition     string //黃金符號條件
	conditionValue    int32  //符號賠率
	replaceGoldsymbol string //取代的符號
	isWild            int32  //是否百搭
	wildEx            string //百搭例外(不包含該種類別)
	multiplier        int32  //加倍倍數
	isExpand          int32  //擴展符號
	isSticky          int32  //符號
	stickyType        string //定住的方式
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

//賠付獎項資料
type SlotPayOutSymbol struct {
	payOutTable []*protodata.SlotPayTable //所有賠付獎項Data
}

//每個reel的symbolid都是唯一
type ReelAndSymbolSetting struct {
	reelId       int32
	symboleIdMap map[int32]*netproto.DFDC_SymbolGroup
}

//賠付線結果
type PayLineResult struct {
	lineId      int32
	reelId      int32
	SymbolGroup []*netproto.DFDC_SymbolGroup
}

type ReelSymbol struct {
	reelId     int32
	rowId      int32
	symbolId   int32
	symbolName string
}

type BonusRecord struct {
	id    int32
	place int32
	icon  int32
}

type JackpotData struct {
	contribution      int64
	totalContribution int64 //此值須再/10000(10^4) 單位為分數
	awardTotal        int64
	level             int32
}

type FrspRecord struct {
	userBet       int64
	freeSpinGroup int32
	times         int32
	chooseRate    int32
	winMoney      int64
}

//新建一个多福多財遊戲实例
func NewDFDCLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *DFDCLogic {
	glc := new(DFDCLogic)
	glc.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)
	glc.cfgs = cfgs
	glc.getdfdcconfig(cfgs)
	glc.ReloadLogicConfig()
	glc.freeSpins = 0
	//glc.jackpotList = make([]*JackpotPool, 0)
	glc.RegisterBeforeGameStartFunc(func() {
		glc.clear()
	})
	glc.RegisterAfterGameStartFunc(func() {
		glc.readJackpotPool()
		glc.startGame()
	})
	glc.RegisterBeforeGameEndFunc(func() {
	})
	return glc
}

func (lh *DFDCLogic) getdfdcconfig(cfgs *config.ConfigGroup) {
	lh.betvalue = newBetconfig(cfgs)
	scfgs := cfgs.GetConfig("slotconfig")
	lh.endactiontime = int32(scfgs.GetConfigInt("slotsetting", "endactiontime"))
	lh.freeSpinLimit = int32(scfgs.GetConfigInt("slotsetting", "freeSpinLimit"))
	gcfgs := cfgs.GetConfig("gameserver")
	lh.betlimitmoney = int64(gcfgs.GetConfigInt("gameserver", "betlimitmoney"))
	slotpatternstr := scfgs.GetConfigValue("slotsetting", "slotpattern")
	slotpatternarr := strings.Split(slotpatternstr, ",")
	rndPpattern := util.RndInt(0, len(slotpatternarr))
	r, _ := strconv.Atoi(slotpatternarr[rndPpattern])
	lh.slotpattern = int32(r)
	lh.getSlotPatternTable()
	lh.getmainGameTable()
	lh.getSlotSymbolDistribution()
	lh.getSymbolAttributes()
	lh.getPayLineTable()
	lh.getSlotPayTable()
	lh.getJackPotTable()
	lh.getBonusTable()
	lh.getFreeSpinTable()
	lh.getPickGameTable()
	lh.getSlotPayTable2()
	lh.reelNum = int32(lh.mainGameTable[0].GetReelNum())
	lh.rowNum = int32(lh.mainGameTable[0].GetRowNum())
	lh.symbolAttributesGroup = int32(lh.slotPatternTable.GetSymbolAttributesGroupId())
	lh.mainGameTableGroup = int32(lh.slotPatternTable.GetMainGameTableGroupId())
	lh.payTableGroup = int32(lh.slotPatternTable.GetPayTableGroupId())
	lh.lineType = string(lh.slotPatternTable.GetLineType())
	lh.payLineGroupId = int32(lh.slotPatternTable.GetPayLineGroupId())

	// 設定初始Jackpot
	lh.jackpotdataDetail = make([]*JackpotData, 0)

	for _, jpTable := range lh.jackpotTable { //企劃表格
		if jpTable.GetGroupId() != int32(lh.slotPatternTable.GetJackpotGroup()) {
			continue
		}
		jackpotDetail := new(JackpotData)

		checkLevel := int32(0)
		switch jpTable.GetLevel() {
		case GrandLevel:
			checkLevel = GRAND
		case MajorLevel:
			checkLevel = MAJOR
		case MinorLevel:
			checkLevel = MINOR
		case MiniLevel:
			checkLevel = MINI
		}

		jackpotDetail.level = checkLevel

		lh.jackpotdataDetail = append(lh.jackpotdataDetail, jackpotDetail)
	}
}

//取得slot模式
func (lh *DFDCLogic) getSlotPatternTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPattern.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.SlotPattern_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPatternTableArr := slotdata.(*protodata.SlotPattern_ARRAY)
	lh.slotPatternTable = slotPatternTableArr.Items[lh.slotpattern-1]

	//mlog.System("[weihao] slotPatternTable[%+v]", lh.slotPatternTable)
}

//取得slot的輪軸設定
func (lh *DFDCLogic) getSlotSymbolDistribution() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotSymbolDistribution.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.SlotSymbolDistribution_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotSymbolDistTable := slotdata.(*protodata.SlotSymbolDistribution_ARRAY)

	//一般輪軸設定
	for _, v := range slotSymbolDistTable.Items {
		if v.GetReelNum() <= lh.mainGameTable[0].GetReelNum() {
			lh.normalSymbolDistTable = append(lh.normalSymbolDistTable, v)
		}
	}

	//mlog.System("[weihao] csv slot的輪軸設定: %+v", lh.normalSymbolDistTable)
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getJackPotTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("JackPotTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.JackpotTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	jackpotTableArr := slotdata.(*protodata.JackpotTable_ARRAY)
	lh.jackpotTable = jackpotTableArr.GetItems()
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getBonusTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("BonusTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.BonusTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	bonusTableArr := slotdata.(*protodata.BonusTable_ARRAY)
	lh.bonusTable = bonusTableArr.GetItems()
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getSlotPayTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPayTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.SlotPayTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPayTableArr := slotdata.(*protodata.SlotPayTable_ARRAY)
	lh.payOutSymbole = make(map[string]*SlotPayOutSymbol, 0)

	for _, v := range slotPayTableArr.Items {
		//payTableArr := make([]*DataCfgDfdc.SlotPayTable, 0)
		if int32(lh.slotPatternTable.GetPayTableGroupId()) == v.GetGroupId() {
			if arr, ok := lh.payOutSymbole[v.GetSymbol()]; ok {
				arr.payOutTable = append(arr.payOutTable, v)
			} else {
				lh.payOutSymbole[v.GetSymbol()] = new(SlotPayOutSymbol)
				lh.payOutSymbole[v.GetSymbol()].payOutTable = make([]*protodata.SlotPayTable, 0)
				lh.payOutSymbole[v.GetSymbol()].payOutTable = append(lh.payOutSymbole[v.GetSymbol()].payOutTable, v)
			}
		}
	}
}

//取得csv的賠付線設定
func (lh *DFDCLogic) getPayLineTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPayLine.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.SlotPayLine_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPatternTableArr := slotdata.(*protodata.SlotPayLine_ARRAY)
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

}

func (lh *DFDCLogic) clear() {
	lh.money = 0
	lh.coupon = 0
	lh.winmoney = 0
	lh.jackpotlevel = 0
	lh.freeNow = false
	lh.userdata = make([]*dfdcuserdata, 0)
	lh.showWildWinMoney = make([]*int32, 0)
	lh.lineCount = make(map[string]int32)
	lh.payiconCount = make(map[string]int32)
	lh.payRate = make(map[string]int32)
	lh.wildPayRate = make(map[string]int32)
	lh.jackpotAward = 0
	// lh.jackpotdataDetail = make([]*JackpotData, 0)
}

//遊戲要付費或免費
func (lh *DFDCLogic) sendGameFreeOrPayMsg() {
	user := lh.GetUserBySeatNo(0)
	if user == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCFreeOrPayID)

	pdata := new(netproto.DFDC_FreeOrPayMsg)
	//TODO: 讀取freespin次數
	pdata.FreeCount = proto.Int32(lh.freeSpins)
	if lh.freeSpins > 0 {
		pdata.ReelSetting = lh.getFreeReelSetting(int32(lh.getUserBetSymbol()))
	} else {
		pdata.ReelSetting = nil
	}

	pmsg.MsgData = pdata

	lh.SendMessageToUser(user, pmsg)
}

//to client jackpot 資料
func (lh *DFDCLogic) sendJackpotPoolS2C() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotPoolS2CID)
	pdata := new(netproto.DFDC_JackpotPoolS2CMsg)
	jackpot := make([]*netproto.DFDC_JackpotPoolDetail, 0)
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	//DFDC_JackpotPoolDetail
	//required int32 JackpotID=1;	 // 1:巨獎 2:大獎 3:中獎 4:小獎
	//required int64 Syspool = 2;	 // 水池值
	//optional string Level = 3;	 // 獎項名稱:Grand/Major/Minor/Mini
	for index, jackpotTable := range lh.jackpotTable {
		if index < 4 {
			jackpotDetail := new(netproto.DFDC_JackpotPoolDetail)
			id := int32(index + 1)
			jackpotDetail.JackpotID = proto.Int32(id)
			//計算4個值
			jackpotDetail.Syspool = proto.Int64(lh.getJackpotPoolByID(id, int32(jackpotGroupId)))
			jackpotDetail.Level = proto.String(jackpotTable.GetLevel())
			jackpot = append(jackpot, jackpotDetail)
		}
	}
	pdata.JackpotPoolID = jackpot
	pmsg.MsgData = pdata
	lh.BoardcastGameUserMessage(pmsg)
}

//更新資料庫jackpot水池(TODO:Contribution/TotalContribution/AwardTotal)
func (lh *DFDCLogic) updateJackpotPool(contri int64, totalcontri int64, awardtotal int64, level int32) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotJackpotNoLinkID)
	pdata := new(netproto.Slot_JackpotData)
	pdata.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)
	pdata.FlagID = proto.Int32(int32(FlagID))
	pdata.TableNo = proto.Int32(lh.TableManager.TableNo)
	pdata.Contribution = proto.Int64(contri)
	pdata.TotalContribution = proto.Int64(totalcontri)
	pdata.AwardTotal = proto.Int64(awardtotal)
	pdata.Level = proto.Int32(level)
	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)

}

func (lh *DFDCLogic) startGame() {
	lh.GameLog.AddGameStep("遊戲開始")
	lh.RegisterAnimation(GameStartAnimation, int32(1), lh.startBet)
}

//開始下注
func (lh *DFDCLogic) startBet() {
	if lh.jpFlag {
		//需檢查是否還有殘留的局數未表演完
		for _, bonusData := range lh.bonusGameRecord {
			if bonusData.icon != 0 {
				//多福多財BonusGame紀錄殘留
				lh.getRecoverBonusGame()
				break
			}
		}
	}
	lh.setState(Bet)
}

//设置状态
func (lh *DFDCLogic) setState(state int32) {
	lh.gamestate = state
	lh.BoardcastMessage(lh.getStateMsg())

}

//获取状态消息
func (lh *DFDCLogic) getStateMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCStateID)

	pdata := new(netproto.DFDC_GameStatus)
	pdata.State = proto.Int32(lh.gamestate)
	pmsg.MsgData = pdata

	return pmsg
}

//取得jackpot pool
func (lh *DFDCLogic) getJackpotPool() {
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	for _, jackpotObj := range lh.jackpotTable {
		if jackpotObj.GetGroupId() == int32(jackpotGroupId) {
			pmsg := new(msg.Message)
			pmsg.BClassID = int32(netproto.MessageBClassID_Game)
			pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGetJackpotPoolID)
			pdata := new(netproto.DFDC_GetJackpotMsg)
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

//用户进入
func (lh *DFDCLogic) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	lh.ComputeGameLogicBase.UserEnter(gameuser, tableno, seatno)
	// 先傳送等待狀態
	lh.setState(Wait)

	//確認是否有殘留資料
	lh.checkReturn(gameuser)
	lh.sendScene(gameuser)

	lh.GameLog.GameBegin()
}

//发送场景
func (lh *DFDCLogic) sendScene(gameuser *gamelogic.GameUser) {
	lh.readJackpotPool()
	lh.sendGoldenSymbolMsg(gameuser)
	lh.sendSettingMsg(gameuser)
	lh.TimerFuncManager.NewNamedTimer(int32(1), func() {
		lh.sendJackpotPoolS2C()
		lh.sendWaitingDataMsg(gameuser)
	}, fmt.Sprintf("gametimeoutcd_%d", lh.TableNo()))
}

//发送设置
func (lh *DFDCLogic) sendWaitingDataMsg(gameuser *gamelogic.GameUser) {
	lh.newUserData(gameuser)
	//需檢查是否還有殘留的局數未表演完
	for _, bonusData := range lh.bonusGameRecord {
		if bonusData.icon != 0 {
			//多福多財BonusGame紀錄殘留
			lh.getRecoverBonusGame()
			break
		}
	}

	//多福多財FreeSpin紀錄殘留
	if lh.freespinRecord.times > 0 {
		//傳送玩家上局下注資訊
		lh.sendFreeBetDetailMsg(gameuser)
		lh.freeSpins = lh.freespinRecord.times
		lh.freeSpinGroup = lh.freespinRecord.freeSpinGroup
		lh.userdata[0].userbetmoney = lh.freespinRecord.userBet
		lh.userdata[0].userbetsymbol = lh.freespinRecord.chooseRate
		//mlog.System("lh.freespinRecord.chooseRate",lh.freespinRecord.chooseRate)
		lh.freeGameMoney = lh.freespinRecord.winMoney
		lh.freespinRecord.times = 0
		lh.sendGameFreeOrPayMsg()
		lh.updatefreespin(gameuser)
	}
	lh.setState(Bet)
	//lh.SendMessageToUser(gameuser, lh.getStateMsg())
}

//发送设置
func (lh *DFDCLogic) sendSettingMsg(gameuser *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCSettingID)
	pdata := new(netproto.DFDC_Setting)
	pdata.BetCount = lh.betvalue.betcount
	pdata.ReelSetting = lh.getReelSetting()
	pdata.RowCounts = proto.Int32(lh.rowNum)
	pdata.TotalPayLine = proto.Int32(1)
	pdata.BigWinStep = lh.bigwinrule
	if lh.slotPatternTable.GetLineType() == ALL {
		pdata.PayLineMode = proto.Int32(ALLMODE)
	} else {
		pdata.PayLineMode = proto.Int32(FIXEDMODE)
	}
	pdata.SymbolPayTable = lh.getSymbolPayTable()

	pmsg.MsgData = pdata

	lh.SendMessageToUser(gameuser, pmsg)
}

//場景輪軸設定
func (lh *DFDCLogic) getReelSetting() []*netproto.DFDC_ReelSetting {
	result := make([]*netproto.DFDC_ReelSetting, 0)
	symbolGroupID := int32(0)

	//根據betsymbol決定表演第幾軸
	symbolGroupID = 1 //5/30決定群組1做為表演輪軸 後續若要調整可只微調此區

	for reelId := 1; reelId <= int(lh.reelNum); reelId++ {
		reelSetting := new(netproto.DFDC_ReelSetting)
		reelSetting.RealID = proto.Int32(int32(reelId))

		for _, s := range lh.normalSymbolDistTable {
			if int(s.GetReelNum()) == reelId && symbolGroupID == int32(s.GetGroupId()) {
				symbolGroup := new(netproto.DFDC_SymbolGroup)
				symbolId := s.GetId()
				symbolGroup.SymbolID = proto.Int32(int32(symbolId))
				symbolGroup.SymbolName = proto.String(s.GetSymbol())
				//symbolGroup.Order = proto.Int32(int32(s.GetOrder()))
				reelSetting.SymbolGroup = append(reelSetting.SymbolGroup, symbolGroup)
			}
			if int(s.GetReelNum()) == reelId {
				//༼ಢ_ಢ༽
				sGroup := new(netproto.DFDC_SymbolGroup)
				sId := s.GetId()
				sGroup.SymbolID = proto.Int32(int32(sId))
				sGroup.SymbolName = proto.String(s.GetSymbol())
				//symbolGroup.Order = proto.Int32(int32(s.GetOrder()))

				if lh.reelAndSymbol[reelId] == nil {
					reelSymbolObj := new(ReelAndSymbolSetting)
					reelSymbolObj.symboleIdMap = make(map[int32]*netproto.DFDC_SymbolGroup, 0)
					reelSymbolObj.symboleIdMap[int32(sId)] = sGroup
					lh.reelAndSymbol[reelId] = reelSymbolObj
				} else {
					lh.reelAndSymbol[reelId].symboleIdMap[int32(sId)] = sGroup
				}
				lh.reelAndSymbol[reelId].reelId = int32(reelId)

			}
		}
		result = append(result, reelSetting)
	}

	return result
}

//断线重连
func (lh *DFDCLogic) UserRecover(gameuser *gamelogic.GameUser) {
	lh.ComputeGameLogicBase.UserRecover(gameuser)
	lh.newUserData(gameuser)
	lh.checkReturn(gameuser)
	lh.sendScene(gameuser)
}

//用户离开
func (lh *DFDCLogic) UserLeave(gameuser *gamelogic.GameUser) bool {
	lh.clear()
	mlog.Debug("[weihao] UserLeave gamestate[%v] ", lh.gamestate)
	if lh.freeSpins > 0 || lh.jpFlag {
		lh.jpFlag = false
		lh.jpFlagUser = 0
		lh.jackpotAward = 0
		return false
	}
	return lh.ComputeGameLogicBase.UserLeave(gameuser)
}

//获取用户数据通过用户ID
func (lh *DFDCLogic) getUserDataByID(userid int32) *dfdcuserdata {
	for _, ud := range lh.userdata {
		if ud.user.UserID == userid {
			return ud
		}
	}

	return nil
}

//游戏结束
func (lh *DFDCLogic) gameEnd() {
	lh.setState(Wait)
	//若玩家狀態還在遊戲中，不結束遊戲
	stateFlag := true
	for _, user := range lh.AllGameUser() {
		//檢查所有玩家狀態
		if user.UserState.StateName() != gamelogic.StateName_Game {
			stateFlag = false
			break
		}
	}
	if stateFlag {
		lh.GameEndFunc()
	}
}

//更新用戶分數
func (lh *DFDCLogic) updateUserSorce(user *gamelogic.GameUser) {
	if lh.freeSpins <= 0 {
		lh.freeGameMoney = 0
	}
	for _, ud := range lh.userdata {
		if !ud.user.IsRobot() {
			//lh.winmoney:寫分
			lh.UpdateUserScoreFunc(ud.user.UserID, lh.winmoney, 0, -lh.coupon)
		}
	}
}

//用户消息主处理函数
func (lh *DFDCLogic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if !lh.CheckGameUserMessage(user) {
		lh.KickUserFunc(user.UserID, "非法操作")
		return
	}
	//不是给游戏逻辑服务器的消息

	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	lh.CheckUserAnimationCommited(user, gmsg, lh.action_str, nil)

	//遊戲開始
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID) {
		//add goldenSymbol index
		ok := lh.userBet(user, gmsg.MsgData.([]byte))
		if ok {
			lh.startSpin(user)
		}
		return
	}

	// 玩家Bonus Game選擇
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameIconID) {
		lh.recordUserChoose(user, gmsg.MsgData.([]byte))
		return
	}

	// 查詢遊戲歷程紀錄
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCLoadGameHistoryID) {
		lh.loadGameHistory(user, gmsg.MsgData.([]byte))
		return
	}

}

func (lh *DFDCLogic) userBet(user *gamelogic.GameUser, data []byte) bool {
	ust := &netproto.DFDC_StartBet{}
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
		mlog.Debug("[weihao] 沒有free spin, 下注不能0元")
		lh.SendMessageToUser(user, lh.getBetRetMsg(user, -1, ust.GetMoney()))
		return false
	}

	//結構增加golden symbol (需自行計算至userbet)
	//下注金額 = betcount * 黃金符號賠率
	goldenSymbolIndex := ust.GetGoldenSymbol()

	allmoney := int64(0)
	usecoupon := int64(0)

	//freespin等於0的時候再來檢查身上的錢夠不夠
	if lh.freeSpins == 0 {
		allmoney = ust.GetMoney() * int64(goldenSymbolIndex)
		lh.freeNow = false
		lh.isCoupon = false

		if user.Coupon > 0 {
			if user.Coupon >= allmoney {
				usecoupon += allmoney
				allmoney = 0
			} else {
				usecoupon = user.Coupon
				allmoney -= user.Coupon
			}
			lh.isCoupon = true
		}

		if int64(allmoney) > user.Score+user.Coupon {
			lh.SendMessageToUser(user, lh.getBetRetMsg(user, -1, ust.GetMoney()))
			return false
		}
		if user.Score+user.Coupon < lh.betlimitmoney*100 {
			lh.SendMessageToUser(user, lh.getBetRetMsg(user, -6, ust.GetMoney()))
			return false
		}

		lh.updateUserMoney(int64(-allmoney), usecoupon)
	}
	lh.GameLog.AddGameStep("用户 %d; 下注: %d ;選擇符號: %d ;金額: %d; 使用禮碼: %d", user.UserID, ust.GetMoney(), ust.GetGoldenSymbol(), allmoney, usecoupon)
	lh.newUserData(user)
	lh.userdata[0].userbetmoney = int64(ust.GetMoney())
	lh.userdata[0].userbetsymbol = ust.GetGoldenSymbol() //玩家選擇的符號
	lh.userdata[0].allmoney = allmoney                   //玩家實際下注金額(freespin=0)
	lh.SendMessageToUser(user, lh.getBetRetMsg(user, 0, allmoney+usecoupon))

	if lh.freeSpins > 0 {
		lh.freeNow = true
		lh.GameLog.AddGameStep("用户%d ,FreeSpin剩餘次數:%d", user.UserID, lh.freeSpins)
		lh.freeSpins = lh.freeSpins - 1
	} else {
		//沒有免費次數 清空資料
		lh.freeSpins = 0
		lh.freeSpinGroup = 0
	}
	lh.sendJPoolS2C()
	return true
}

//取得下注金額
func (lh *DFDCLogic) getUserBetMoney() int64 {
	return lh.userdata[0].userbetmoney
}

//取得下注金額
func (lh *DFDCLogic) getUserBetSymbol() int64 {
	return int64(lh.userdata[0].userbetsymbol)
}

//下注狀態
func (lh *DFDCLogic) getBetRetMsg(user *gamelogic.GameUser, ret int32, money int64) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCBetRetID)
	pdata := new(netproto.DFDC_BetRet)
	pdata.Money = proto.Int64(money)
	pdata.Ret = proto.Int32(ret)
	pmsg.MsgData = pdata
	mlog.Debug("[weihao] 收到下注 [%+v]", pdata)

	return pmsg
}

//更新玩家的钱
func (lh *DFDCLogic) updateUserMoney(money, coupon int64) {
	lh.money += money
	lh.coupon += coupon
	lh.winmoney += money
	//mlog.System("[weihao] 更新玩家的钱 lh.money[%v]", lh.winmoney)
}

func (lh *DFDCLogic) startSpin(user *gamelogic.GameUser) {
	lh.setState(KaiJiang)
	lh.getResultPicture(user)
	lh.SendMessageToUser(user, lh.getDFDCResultMsg(user))
	lh.updateUserSorce(user)
	lh.action_str = lh.RegisterAnimation(GameEnd_Action, lh.endactiontime, lh.gameEnd)
}

//取得開獎的起始圖
func (lh *DFDCLogic) getResultPicture(user *gamelogic.GameUser) []int32 {
	row1 := make([]int32, 0) //開獎結果
	row2 := make([]int32, 0)
	row3 := make([]int32, 0)
	//根據下注符號開獎
	if lh.freeNow {
		for _, freegameTable := range lh.freeSpinTable {
			if lh.freeSpinGroup == int32(freegameTable.GetGroupId()) {
				conditionValue, _ := strconv.ParseFloat(freegameTable.GetConditionValue(), 64)
				if lh.userdata[0].userbetsymbol == int32(conditionValue) {
					lh.symbolGroup = int32(freegameTable.GetFreeSymbolGroupId())
				}
			}
		}
	} else {
		for _, table := range lh.mainGameTable {
			if int32(table.GetGroupId()) == int32(lh.slotPatternTable.GetMainGameTableGroupId()) {
				value := table.GetConditionValue()
				conditionValue, err := strconv.Atoi(value)
				if err == nil {
					if int32(conditionValue) == lh.userdata[0].userbetsymbol {
						if table.GetGoldCondition() == "betRate" {
							lh.symbolGroup = int32(table.GetSymbolGroupId())
							break
						}
					}
				}
			}
		}
	}
	lh.GameLog.AddGameStep("此局群組ID: %d", lh.symbolGroup)
	for _, slotTable := range lh.normalSymbolDistTable {
		if lh.symbolGroup == int32(slotTable.GetGroupId()) {
			//1.輪軸內隨機一個order 2.根據order權重找出符號
			order1 := int32(0) //order id
			order2 := int32(0)
			order3 := int32(0)
			for reel := int32(1); reel <= lh.reelNum; reel++ {
				//獲得該軸 order id
				order2 = lh.getOrderIdbyGroupAndReel(reel, lh.symbolGroup)
				sym2 := lh.randSymbol(reel, order2, lh.symbolGroup)
				row2 = append(row2, sym2)

				//根據起始order 推測往上一個及往下一個order
				order1 = lh.getPreOrderByGroup(reel, order2, lh.symbolGroup)
				sym1 := lh.randSymbol(reel, order1, lh.symbolGroup)
				row1 = append(row1, sym1)
				order3 = lh.getNextOrderByGroup(reel, order2, lh.symbolGroup)
				sym3 := lh.randSymbol(reel, order3, lh.symbolGroup)
				row3 = append(row3, sym3)
			}
			break
		}
	}

	lh.kaiJiang1 = row1
	lh.kaiJiang2 = row2
	lh.kaiJiang3 = row3

	lh.GameLog.AddGameStep("第一行出現符號:%v ,%v ,%v ,%v ,%v", lh.getSymbolName(1, row1[0]), lh.getSymbolName(2, row1[1]), lh.getSymbolName(3, row1[2]), lh.getSymbolName(4, row1[3]), lh.getSymbolName(5, row1[4]))
	lh.GameLog.AddGameStep("第二行出現符號:%v ,%v ,%v ,%v ,%v", lh.getSymbolName(1, row2[0]), lh.getSymbolName(2, row2[1]), lh.getSymbolName(3, row2[2]), lh.getSymbolName(4, row2[3]), lh.getSymbolName(5, row2[4]))
	lh.GameLog.AddGameStep("第三行出現符號:%v ,%v ,%v ,%v ,%v", lh.getSymbolName(1, row3[0]), lh.getSymbolName(2, row3[1]), lh.getSymbolName(3, row3[2]), lh.getSymbolName(4, row3[3]), lh.getSymbolName(5, row3[4]))

	return row1
}

//取得所有開獎結果
func (lh *DFDCLogic) getDFDCResultMsg(user *gamelogic.GameUser) *msg.Message {
	defer util.PanicErrStack()
	lh.setState(KaiJiangAction)
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameResult)
	pdata := new(netproto.DFDC_GameResultReel)

	//mlog.System("[weihao] 取得第一行開獎結果 [%v]", lh.kaiJiang1)
	//沿用吉祥如意協議，賠付線結果更正
	if lh.slotPatternTable.GetLineType() == ALL {
		pdata.PayLineGroup = nil
		pdata.PayAllGroup = lh.getResultAllGroup()
	} else {
		pdata.PayLineGroup = lh.getResultPayLine()
		pdata.PayAllGroup = nil
	}

	//Symbol ID不等於開獎結果，而是透過Symbol ID骰出該order id 之結果
	pdata.SymbolID = lh.kaiJiang1 //lh.reelSymbolResult

	lh.reelSymbolResultRange(func(obj *ReelSymbol) {
		//mlog.System("[weihao] reelid[%v] 輪軸所有開獎結果 symbole[%v][%v]", obj.reelId, obj.symbolId, obj.symbolName)
		settingObj := new(netproto.DFDC_ReelSetting)
		settingObj.RealID = proto.Int32(obj.reelId)
		settingObj.RowIndex = proto.Int32(obj.rowId)
		symbolObj := new(netproto.DFDC_SymbolGroup)
		symbolObj.SymbolID = proto.Int32(obj.symbolId)
		//特殊符號變色

		symbolObj.SymbolName = proto.String(obj.symbolName)
		settingObj.SymbolGroup = append(settingObj.SymbolGroup, symbolObj)
		pdata.ReelSetting = append(pdata.ReelSetting, settingObj)
	})
	pdata.WinMoney = proto.Int64(lh.getWinMoneyResult(user))
	if lh.freeSpins == 0 {
		pdata.FreeGameWinMoney = proto.Int64(lh.freeGameMoney)
		lh.GameLog.AddGameStep("用户%dFreeSpin獲得:%d", lh.userdata[0].user.UserID, lh.freeGameMoney)
	}
	pmsg.MsgData = pdata

	return pmsg
}

//依照權重取得開獎符號
func (lh *DFDCLogic) randSymbol(reelid int32, orderId int32, groupId int32) int32 {
	//根據輪軸及OrderID獲得流水號及權重
	idSlice := make([]int32, 0)
	idWeight := make([]int32, 0)
	totalWeight := 0     //總tatol
	symbolId := int32(0) //符號ID
	for _, table := range lh.normalSymbolDistTable {
		if reelid == int32(table.GetReelNum()) && orderId == int32(table.GetOrder()) && groupId == int32(table.GetGroupId()) {
			idSlice = append(idSlice, int32(table.GetId()))
			idWeight = append(idWeight, int32(table.GetProportion()))
			totalWeight += int(table.GetProportion())
		}
	}

	jugde := rand.Intn(totalWeight) //隨機判定
	weight := 0                     //判斷權重
	for i := 0; i < len(idSlice); i++ {
		if weight+int(idWeight[i]) > jugde {
			symbolId = idSlice[i]
			break
		} else {
			weight += int(idWeight[i])
		}
	}

	if symbolId == 0 {
		mlog.Fatal("[weihao] 開獎符號 不可以為0")
	}

	return symbolId
}

//取得賠付線
func (lh *DFDCLogic) getResultPayLine() []*netproto.DFDC_PayLineGroup {
	//沿用吉祥如意協議，賠付線結果更正
	result := make([]*netproto.DFDC_PayLineGroup, 0)

	//從kaiJiang1、kaiJiang2、kaiJiang3獲得結果(symbol id)
	lh.reelSymbolResult = lh.getReelSymbolResultArr()

	//log
	lh.reelSymbolResultRange(func(obj *ReelSymbol) {
		//mlog.System("[weihao] reelid[%v] 輪軸所有開獎結果 symbole[%v][%v]", obj.reelId, obj.symbolId, obj.symbolName)
	})

	//根據符號連線數轉換成線數
	lh.payLineResult = lh.getPayLineResult()
	var startSymboleName string
	startSymboleName = fmt.Sprintf("")
	//非全線賠付
	//TODO 重構移到getPayLineResult()裡面
	blink := true
	for lineId, resultArr := range lh.payLineResult {
		startSymboleIsWild := true

		for _, v := range resultArr {
			payLineGroup1 := new(netproto.DFDC_PayLineGroup)
			payLineGroup1.PayLineID = proto.Int32(lineId)

			if v.reelId == 1 {
				blink = true
				//payLineId = lineId
				startSbId := v.SymbolGroup[0].GetSymbolID()
				startSymboleName = lh.getSymbolName(v.reelId, startSbId)
				//mlog.Debug("[weihao] startSymboleName[%resultArr]", startSymboleName)
			}

			for _, symbolVal := range v.SymbolGroup {
				payLineList1 := new(netproto.DFDC_PayLineDetail)
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
					payLineList1.Line = proto.Bool(false)
				} else {
					blink = false
					//沒中獎但要畫線
					payLineList1.Blink = proto.Bool(blink)
					payLineList1.Line = proto.Bool(false)
				}

				payLineGroup1.PayLineDetail = append(payLineGroup1.PayLineDetail, payLineList1)
				//mlog.System("[weihao] 中獎賠付線[%v] 起始symbol[%v] reelid[%v] symbol[%v] Blink[%v] Line[%v] ",
				//	lineId, startSymboleName, v.reelId, nextSymbolName, payLineList1.GetBlink(), payLineList1.GetLine())

			}

			result = append(result, payLineGroup1)
		}

	}

	return result
}

//取得輪軸所有開獎結果 map[reelid][]symbolid
func (lh *DFDCLogic) getReelSymbolResultArr() map[int32][]int32 {
	//map[輪軸]int32{開獎符號}
	reelSymbolResult := make(map[int32][]int32, 0)
	reel := int32(0)
	//開獎時已獲得kaiJiang1、kaiJiang2、kaiJiang3
	for index := int32(0); index < lh.reelNum; index++ {
		//輪軸ID(從0開始,需+1), 開獎符號
		reel = index + 1
		symbolArr := []int32{lh.kaiJiang1[index], lh.kaiJiang2[index], lh.kaiJiang3[index]}
		reelSymbolResult[lh.reelAndSymbol[reel].reelId] = symbolArr
	}
	return reelSymbolResult //輪軸結果
}

//循環開獎結果每個reel的symbol obj
func (lh *DFDCLogic) reelSymbolResultRange(f func(p *ReelSymbol)) {
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

func (lh *DFDCLogic) getSymbolName(reelId, symbolId int32) string {
	var result string

	if v, ok := lh.reelAndSymbol[reelId].symboleIdMap[symbolId]; ok {
		return v.GetSymbolName()
	} else {
		return result
	}

	return result
}

func (lh *DFDCLogic) getOrderNumber(reelid, orderId int32) int32 {
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

//取得結算金額
func (lh *DFDCLogic) getWinMoneyResult(user *gamelogic.GameUser) int64 {
	sumwinmoney := int64(0)
	showmoney := int64(0)

	if lh.slotPatternTable.GetLineType() == ALL {
		if len(lh.kaijiangsymbol) == 0 {
			lh.GameLog.AddGameStep("用户%d未中獎", user.UserID)
		}

		//累計spin貢獻值
		lh.calJackpotPool()

		//任意出現三個或三個以上的Scatter時會進入Scatter Bonus Spin模式
		lh.getFreeSpinResult(LEFT)
		lh.sendGameFreeOrPayMsg()
		//chip:賠率，此數字乘上betCount
		//chipXrate:賠率，此數字乘上betCount，再乘上betRate
		for _, res := range lh.kaijiangsymbol {
			sumwinmoney += lh.getUserBetMoney() * int64(lh.lineCount[res]) * int64(lh.payRate[res])
			showmoney += lh.getUserBetMoney() * int64(lh.lineCount[res]) * int64(lh.payRate[res])
			lh.GameLog.AddGameStep("開獎結果:%s;下注金額:%d;連線數:%d;賠率:%d;贏%d", res, lh.getUserBetMoney(), int64(lh.lineCount[res]), int64(lh.payRate[res]), sumwinmoney)
		}

		//加上歷程紀錄
		if lh.freeNow {
			lh.addHitoryLog(user, showmoney, FREE, REEL, lh.getGameData())
		} else {
			lh.addHitoryLog(user, showmoney, MAIN, REEL, lh.getGameData())
		}

		//mlog.System("sumwinmoney", sumwinmoney)
		lh.updateUserMoney(sumwinmoney, 0)
		if lh.freeNow {
			lh.freeGameMoney += lh.jackpotAward
			lh.freeGameMoney += sumwinmoney
		}

	} else {
		if len(lh.payLineResult) == 0 {
			mlog.Error("沒中獎")
		}

		lh.calJackpotPool()

		//任意出現三個或三個以上的Scatter時會進入Scatter Bonus Spin模式
		lh.getFreeSpinResult(ANY)

		//由左開始的賠付線連線獎金結算
		for lineid, resultArr := range lh.payLineResult {
			jackpotmoney := int64(0)
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
				payout, err := strconv.Atoi(payOut.GetPayout())
				if err != nil {
					payout = 0
				}
				winmoney := int64(payout) * lh.getUserBetMoney()
				sumwinmoney += winmoney
				sumwinmoney += jackpotmoney

				mlog.System("[weihao] done取得結算金額 賠付線id[%v] symbole[%v]  連線數量[%v] 下注[%v] 賠付倍率[%v] 結算金額[%v] jackpotmoney[%v]",
					lineid, payOut.GetSymbol(), payOut.GetCount(), lh.getUserBetMoney(), payOut.GetPayout(), winmoney, jackpotmoney)
			}
		}
		//sumwinmoney贏錢
		lh.updateUserMoney(sumwinmoney, 0)
	}
	if lh.jpFlag {
		lh.jpFlagUser = user.UserID
	}
	if lh.freeNow {
		//FreeSpin存入DB
		lh.updatefreespin(user)
	}

	if !lh.isCoupon {

		var diff float64

		diff = float64(sumwinmoney) - float64(lh.userdata[0].userbetmoney*int64(lh.userdata[0].userbetsymbol))

		user.FlowAmount += int64(math.Abs(diff))
	}

	uscoresRet := make(map[int32]int64)
	uscoresRet[user.UserID] = lh.winmoney
	ucouponRet := make(map[int32]int64)
	ucouponRet[user.UserID] = -lh.coupon
	lh.GameLog.GameEnd(uscoresRet, ucouponRet)

	return showmoney
}

//累計spin貢獻值
func (lh *DFDCLogic) calJackpotPool() {
	// 若是使用Coupon玩的不累計
	if lh.isCoupon {
		return
	}
	//每次Spin貢獻值(contribution)=increment*toltalBet
	//現在水池實際金額(poolNow)=startUp+每次Spin貢獻值+已儲存累積的貢獻值-重置前累積JP出獎金額
	//當重置前累積JP出獎金額大於已儲存累積的貢獻值時 兩個值都歸零
	//玩家實際看到的現在水池實際金額*CurrentBet/MAXBet
	//水池會根據變更投注額即時變動
	jackpotGroupId := int32(lh.slotPatternTable.GetJackpotGroup())
	//contribution := int64(0)
	totalcontri := int64(0)
	for _, jpTable := range lh.jackpotTable { //企劃表格
		if jpTable.GetGroupId() != jackpotGroupId {
			continue
		}

		for _, jpdetail := range lh.jackpotdataDetail { //資料庫拉出來的紀錄
			for i := 0; i < 4; i++ {
				level := int32(i + 1)
				checkLevel := int32(0)
				switch jpTable.GetLevel() {
				case GrandLevel:
					checkLevel = GRAND
				case MajorLevel:
					checkLevel = MAJOR
				case MinorLevel:
					checkLevel = MINOR
				case MiniLevel:
					checkLevel = MINI
				}
				//利用id獲得水池值/貢獻值/累積JP出獎
				//四池增加的比例不一
				//contribution = int64(jpTable.GetIncrement()) * lh.userdata[0].userbetmoney * int64(lh.userdata[0].userbetsymbol)
				if jpdetail.level == level && int32(jackpotGroupId) == jpTable.GetGroupId() && jpdetail.level == checkLevel {
					totalcontri = int64(jpTable.GetIncrement()) * lh.userdata[0].allmoney
					if lh.jackpotlevel == checkLevel {
						lh.GameLog.AddGameStep("水池 %d 增加(N/10000) N=%d , 累積獎金增加 %d", level, totalcontri, lh.jackpotAward)
						contri := (jpdetail.totalContribution + totalcontri) / 10000
						if lh.jackpotAward > contri {
							lh.updateJackpotPool(0, -jpdetail.totalContribution, -jpdetail.awardTotal, level)
						} else {
							lh.updateJackpotPool(0, totalcontri, lh.jackpotAward, level)
						}
					} else {
						lh.GameLog.AddGameStep("水池 %d 增加(N/10000) N=%d , 累積獎金增加 %d", level, totalcontri, 0)
						lh.updateJackpotPool(0, totalcontri, 0, level)
					}

				}
			}
		}

	}

	//TODO linq
	//jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	//for _, jackpotObj := range lh.jackpotTable {
	//	if jackpotObj.GetGroupId() == int32(jackpotGroupId) {
	//		increment := util.Decimal((float64(jackpotObj.GetIncrement()) / 10000), 3) //Jackpot每次Spin增加的比例
	//		betmoney := int32(math.Abs(float64(lh.money)))
	//		spinINC := util.Decimal(float64(increment)*float64(betmoney), 3) //每次Spin貢獻值
	//		mlog.System("[weihao] 每次Spin貢獻值[%v] increment[%v] betmoney[%v] = [%v] ", spinINC, increment, betmoney, (float64(increment) * float64(betmoney)))
	//		jackpotPoolObj := linq.From(lh.jackpotList).
	//			FirstWithT(func(j *JackpotPool) bool {
	//			return j.jackpotId == jackpotObj.GetId()
	//		}).(*JackpotPool)
	//
	//		jackpotPoolObj.level = jackpotObj.GetLevel()
	//		jackpotPoolObj.spinStack += spinINC
	//		//syspool = 0
	//		syspool := int64(jackpotObj.GetStartUp()) + int64(math.Floor(float64(jackpotPoolObj.spinStack))) - jackpotPoolObj.jpAmountOut //現在水池實際金額
	//		mlog.System("[weihao] 計算syspool , syspool[%v] spinStack[%v] jpOut[%v]", syspool,
	//			int64(math.Floor(float64(jackpotPoolObj.spinStack))), jackpotPoolObj.jpAmountOut)
	//		//當計算後的syspool小於startup值就用startup當syspool, 此行為是初始化syspool
	//		if jackpotPoolObj.syspool < int64(jackpotObj.GetStartUp()) {
	//			syspool = int64(jackpotObj.GetStartUp()) + int64(util.Decimal(float64(spinINC), 0))
	//			//jackpotPoolObj.spinStack = 0
	//			//jackpotPoolObj.jpAmountOut = 0
	//		}
	//		jackpotPoolObj.syspool = syspool
	//
	//		mlog.System("[weihao] 計算jp jackpotid[%v] syspool[%v] 累計後spin貢獻值[%v] 本次增加[%v] 下注金額[%v] increment[%v]",
	//			jackpotObj.GetId(), jackpotPoolObj.syspool, jackpotPoolObj.spinStack, spinINC, betmoney, increment)
	//	}
	//}
}

//任意出現三個或三個以上的Scatter時會進入Scatter Bonus Spin模式
func (lh *DFDCLogic) getFreeSpinResult(lineType string) {
	freeNow := false
	if lineType == LEFT {
		ok, payout := lh.getFreeSpinCheck()
		if ok {
			lh.freeSpinGroup = int32(payout)
			freeNow = true
		}
	} else if lineType == ANY {
		scatterCount := int32(0)
		lh.reelSymbolResultRange(func(symbolObj *ReelSymbol) {
			if lh.isEqualSymbole(SCATTER, symbolObj.symbolName) {
				scatterCount++
				payOut, ok := lh.getSymbolPayOut(ANY, FREE_SPINS, symbolObj.symbolName, scatterCount)
				if ok {
					payout, err := strconv.Atoi(payOut.GetPayout())
					if err != nil {
						payout = 0
					} else {
						lh.freeSpinGroup = int32(payout)
						freeNow = true
					}
				}
			}
		})
	}
	if freeNow {
		//讀取free spin 次數
		for _, freeSpinTable := range lh.freeSpinTable {
			conditionValue, _ := strconv.ParseFloat(freeSpinTable.GetConditionValue(), 64)
			if lh.freeSpinGroup == freeSpinTable.GetGroupId() && lh.userdata[0].userbetsymbol == int32(conditionValue) {
				lh.freeSpins += int32(freeSpinTable.GetFreeSpinCount())
				if lh.freeSpinLimit != 0 && lh.freeSpins > lh.freeSpinLimit {
					//mlog.System("超過上限，故重置為%s次", lh.freeSpinLimit)
					lh.GameLog.AddGameStep("增加%d後，總次數為%d，超過上限故重置為%d次", freeSpinTable.GetFreeSpinCount(), lh.freeSpins, lh.freeSpinLimit)
					lh.freeSpins = lh.freeSpinLimit
				}
				lh.updatefreespin(lh.userdata[0].user)
			}
		}
		freeNow = false
	}
}

//取得連線符號的數量
func (lh *DFDCLogic) getSameSymbolCount(linkeType string, r []*PayLineResult, firstSymbol string) int32 {
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

//是否為某個相同特殊符號:WILD, SCATTER
func (lh *DFDCLogic) isEqualSymbole(equalName string, symbolName string) bool {
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

//取得Symbole數量的獎金
func (lh *DFDCLogic) getSymbolPayOut(linkType, payType, symboleName string, count int32) (*protodata.SlotPayTable, bool) {
	result := new(protodata.SlotPayTable)
	okResult := false

	if v, ok := lh.payOutSymbole[symboleName]; ok {
		//取得唯一的payout資料, 指定方向與數量
		result2 := linq.From(v.payOutTable).
			FirstWithT(
				func(arr *protodata.SlotPayTable) bool {
					return arr.GetLinkType() == linkType && arr.GetPayType() == payType && arr.GetCount() == count
				},
			)

		if result2 != nil {
			result = result2.(*protodata.SlotPayTable)
			okResult = true
		}

	}

	return result, okResult
}

//wild開始,當有連續wild, 採用賠率大的
func (lh *DFDCLogic) getFirstWildPayOut(leftFirstSymbol string, resultArr []*PayLineResult, payOut *protodata.SlotPayTable) (int32, *protodata.SlotPayTable, int64) {
	sameCount := int32(0)
	jackpotmoney := int64(0)
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
				} else if sameCount == lh.reelNum { //一條wild,中了jackpot
					jackpotmoney = lh.getJackpotMoney(leftFirstSymbol)
					payOut.Payout = proto.String("")
				}
			}
		}
	}
	return sameCount, payOut, jackpotmoney
}

//Jackpot獎金
func (lh *DFDCLogic) getJackpotMoney(leftFirstSymbol string) int64 {
	payOut2, ok2 := lh.getSymbolPayOut(LEFT, JACKPOT, leftFirstSymbol, lh.reelNum)
	if ok2 {
		return int64(lh.getJackpotPayOut(payOut2))
	}
	return 0
}

//發放jackpot payout獎池
func (lh *DFDCLogic) getJackpotPayOut(payOut *protodata.SlotPayTable) int32 {
	//jackpoObj := linq.From(lh.jackpotList).
	//	FirstWithT(func(j *JackpotPool) bool {
	//	payout, err := strconv.Atoi(payOut.GetPayout())
	//	if err != nil {
	//		payout = 0
	//	}
	//	return j.jackpotId == int32(payout)
	//}).(*JackpotPool)
	//
	//betRate := float32(lh.betrate)
	//jprate := float32(float32(lh.jprate))
	//jackpotMoney := int32(math.Floor(float64(float32(jackpoObj.syspool) * betRate / jprate)))
	//jackpoObj.jpAmountOut += int64(jackpotMoney)
	//
	//jpTable := linq.From(lh.jackpotTable).
	//	FirstWithT(func(j *protodata.JackpotTable) bool {
	//	payout, err := strconv.Atoi(payOut.GetPayout())
	//	if err != nil {
	//		payout = 0
	//	}
	//	return j.GetId() == int32(payout)
	//}).(*protodata.JackpotTable)
	//
	//syspool := jackpoObj.syspool - jackpoObj.jpAmountOut //現在水池實際金額
	//mlog.System("[weihao] 發放jackpot before syspool[%v] jpAmountOut[%v] ", jackpoObj.syspool, jackpoObj.jpAmountOut)
	//////當計算後的syspool小於startup值就用startup當syspool, 此行為是初始化syspool
	//if syspool < int64(jpTable.GetStartUp()) {
	//	syspool = int64(jpTable.GetStartUp())
	//	jackpoObj.syspool = syspool
	//	jackpoObj.jpAmountOut = 0
	//	jackpoObj.spinStack = 0
	//}
	//
	//mlog.System("[weihao] 計算jackpot 每次Spin貢獻值[%v] 累積的貢獻值[%v] 現在水池實際金額[%v] 玩家得獎[%v]",
	//	jackpoObj.spinStack, jackpoObj.spinStack, jackpoObj.syspool, jackpotMoney)

	return 0
}

//wild開始,非連續wild
func (lh *DFDCLogic) getNonContinuWildPayOut(leftFirstSymbol string, resultArr []*PayLineResult, payOut *protodata.SlotPayTable, ok bool) (*protodata.SlotPayTable, bool) {
	if !ok && lh.isEqualSymbole(WILD, leftFirstSymbol) {
		for _, arr := range resultArr {

			pSymbolGroup := linq.From(arr.SymbolGroup).
				FirstWithT(func(pObj *netproto.DFDC_SymbolGroup) bool {
					return !lh.isEqualSymbole(WILD, pObj.GetSymbolName())
				},
				)

			if pSymbolGroup != nil {
				leftFirstSymbol = pSymbolGroup.(*netproto.DFDC_SymbolGroup).GetSymbolName()

				sameCount := lh.getSameSymbolCount(LEFT, resultArr, leftFirstSymbol)
				payOut, ok = lh.getSymbolPayOut(LEFT, CHIP, leftFirstSymbol, sameCount)
				//break
				return payOut, ok
			}
		}
	}

	return payOut, ok
}

//取得中獎結果的賠付線
func (lh *DFDCLogic) getPayLineResult() map[int32][]*PayLineResult {
	payLineResultArr := make(map[int32][]*PayLineResult, 0)
	temp := make([]map[int32][]*PayLineResult, 0)

	for y := 0; y < int(lh.rowNum); y++ { //從左邊開始
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

//檢核賠付線是否符合設定
func (lh *DFDCLogic) checkPayLine(y int, initSymbolName string, payLineResultArr map[int32][]*PayLineResult) (bool, map[int32][]*PayLineResult) {
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

			if startSymbole == nextSymbolName || lh.isEqualSymbole(WILD, startSymbole) || lh.isEqualSymbole(WILD, nextSymbolName) || len(tempResultArr) >= int(2) {
				//mlog.System("[weihao] After檢核賠付線 lineid[%v] symbol[%v] after reeldi[%v]x[%v] y[%v] ",
				//	lineGroup.lineId, nextSymbolName, rid, p.pointX, p.pointY)

				bonusCount[startSymbole]++
				symbol2 := new(netproto.DFDC_SymbolGroup)
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

			if len(tempResultArr) >= int(lh.reelNum) {
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

////賠付現規則, 逐一檢查
func (lh *DFDCLogic) payLineCheckRange(f func(p *PayLineDetail)) {
	for _, lineGroup := range lh.payLineGroup {
		f(lineGroup)
	}
}

//取得黃金符號設定
func (lh *DFDCLogic) getSymbolAttributes() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SymbolAttributes.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.SymbolAttributes_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	symbolAttributesTable := slotdata.(*protodata.SymbolAttributes_ARRAY)
	lh.symbolAttributesTable = make([]*SymbolAttributes, 0)

	for _, v := range symbolAttributesTable.Items {
		attributesTable := new(SymbolAttributes)
		attributesTable.id = int32(v.GetId())                     //流水號
		attributesTable.groupId = int32(v.GetGroupId())           //群組編號
		attributesTable.symbol = v.GetSymbol()                    //符號類型
		attributesTable.isGoldSymbol = int32(v.GetIsGoldsymbol()) //是否黃金符號
		attributesTable.goldCondition = v.GetGoldCondition()      //黃金符號條件
		conditionValue, _ := strconv.ParseFloat(v.GetConditionValue(), 64)
		attributesTable.conditionValue = int32(conditionValue)       //符號賠率
		attributesTable.replaceGoldsymbol = v.GetReplaceGoldsymbol() //取代的符號
		attributesTable.isWild = int32(v.GetIsWild())
		attributesTable.wildEx = v.GetWildEx()
		attributesTable.multiplier = int32(v.GetMultiplier())
		attributesTable.isExpand = int32(v.GetIsExpand())
		attributesTable.isSticky = int32(v.GetIsSticky())
		attributesTable.stickyType = v.GetStickyType()
		lh.symbolAttributesTable = append(lh.symbolAttributesTable, attributesTable)
	}

	//mlog.System("[weihao] csv 黃金符號設定: %+v", lh.symbolAttributesTable)
}

//发送黃金符號设置
func (lh *DFDCLogic) sendGoldenSymbolMsg(gameuser *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGoldenSymbolID)
	pdata := new(netproto.DFDC_GoldenSymbolList)

	for i, symbolAttr := range lh.goldenSymbolRate {
		bdata := new(netproto.DFDC_GoldenSymbolDetail)
		bdata.Symbols = proto.Int32(int32(i + 1))
		bdata.Credits = proto.Int32(symbolAttr)
		pdata.GoldenDetail = append(pdata.GoldenDetail, bdata)
	}

	pmsg.MsgData = pdata

	lh.SendMessageToUser(gameuser, pmsg)
}

func (lh *DFDCLogic) getOrderIdbyGroupAndReel(reel int32, group int32) int32 {
	orderSlice := make([]int32, 0)
	for _, table := range lh.normalSymbolDistTable {
		if reel == int32(table.GetReelNum()) && group == int32(table.GetGroupId()) {
			orderSlice = append(orderSlice, int32(table.GetOrder()))
		}
	}
	//去重覆
	orderSlice = removeRepByLoop(orderSlice)
	randOrder := rand.Intn(len(orderSlice))

	return orderSlice[randOrder]
}

func removeRepByLoop(order []int32) []int32 {
	result := make([]int32, 0) // 存放结果
	for i := range order {
		flag := true
		for j := range result {
			if order[i] == result[j] {
				flag = false // 存在重复元素，标识为false
				break
			}
		}
		if flag { // 标识为false，不添加进结果
			result = append(result, order[i])
		}
	}
	return result
}

func removeStringRepByLoop(order []string) []string {
	result := make([]string, 0) // 存放结果
	for i := range order {
		flag := true
		for j := range result {
			if order[i] == result[j] {
				flag = false // 存在重复元素，标识为false
				break
			}
		}
		if flag { // 标识为false，不添加进结果
			result = append(result, order[i])
		}
	}
	return result
}

//尋找前一個 order id
func (lh *DFDCLogic) getPreOrderByGroup(reel int32, order int32, group int32) int32 {
	preOrder := int32(0)
	orderSlice := make([]int32, 0)
	for _, table := range lh.normalSymbolDistTable {
		if reel == int32(table.GetReelNum()) && group == int32(table.GetGroupId()) {
			//該群組輪軸含有的order
			orderSlice = append(orderSlice, int32(table.GetOrder()))
			//去重覆
			orderSlice = removeRepByLoop(orderSlice)
		}
	}
	for orderSit := 1; orderSit <= (len(orderSlice)); orderSit++ {
		if order == int32(orderSit) {
			//得到order的位置
			preOrder = int32(orderSit - 1)
		}
	}
	if preOrder == int32(0) {
		preOrder = int32(len(orderSlice))
	}

	return orderSlice[preOrder-1]
}

//尋找下一個 order id
func (lh *DFDCLogic) getNextOrderByGroup(reel int32, order int32, group int32) int32 {
	nextOrder := int32(0)
	orderSlice := make([]int32, 0)
	for _, table := range lh.normalSymbolDistTable {
		if reel == int32(table.GetReelNum()) && group == int32(table.GetGroupId()) {
			//該群組輪軸含有的order
			orderSlice = append(orderSlice, int32(table.GetOrder()))
			//去重覆
			orderSlice = removeRepByLoop(orderSlice)
		}
	}
	for orderSit := 1; orderSit <= (len(orderSlice)); orderSit++ {
		if order == int32(orderSit) {
			//得到order的位置
			nextOrder = int32(orderSit + 1)
		}
	}
	if nextOrder > int32(len(orderSlice)) {
		nextOrder = int32(1)
	}

	return orderSlice[nextOrder-1]
}

//判斷中獎連線(由第一軸符號進行判斷)
func (lh *DFDCLogic) getAllLineSymbol() (bool, []string) {
	defer func() {
		err := util.PanicErrStack()
		if err != nil {
			mlog.Fatal("系統掛掉")
		}
	}()

	//透過lh.kaijiang獲得各符號
	resultReel := make(map[int][]string)
	for reel := 0; reel < int(lh.reelNum); reel++ {
		resultReel[reel+1] = []string{lh.getSymbolName(int32(reel+1), lh.kaiJiang1[reel]), lh.getSymbolName(int32(reel+1), lh.kaiJiang2[reel]), lh.getSymbolName(int32(reel+1), lh.kaiJiang3[reel])}
	}
	lh.resultSymbol = resultReel

	//開獎成功結果符號
	result := make([]string, 0)
	lineType := make(map[string]string)
	mysteryjackpot := false
	mysteryCount := int32(0)
	mysterySymbol := ""

	//檢查各輪軸出現符號定義及順序
	for reelIndex := 1; reelIndex <= int(lh.reelNum); reelIndex++ {
		for _, icon := range lh.resultSymbol[reelIndex] {
			mysteryFlag := false
			for _, payTable := range lh.payOutSymbole[icon].payOutTable {
				if payTable.GetGroupId() == int32(lh.slotPatternTable.GetMainGameTableGroupId()) {
					switch string(payTable.GetLinkType()) {
					case LEFT:
						lineType[icon] = LEFT
						break
					case RIGHT:
						lineType[icon] = RIGHT
						break
					case ANY:
						lineType[icon] = ANY
						if payTable.GetPayType() == "mysteryjackpot" {
							mysteryFlag = true
						}
					}
				}
			}
			if lineType[icon] == ANY && mysteryFlag {
				mysteryCount++
				mysterySymbol = icon
				mysteryjackpot = true
			}
		}
	}
	lh.GameLog.AddGameStep("mysteryjackpot符號個數%d", mysteryCount)
	//觸發jackpot , 沒有使用coupon才可進入
	if mysteryjackpot && !lh.isCoupon {
		for _, payTable := range lh.payOutSymbole[mysterySymbol].payOutTable {
			if payTable.GetPayType() == "mysteryjackpot" && mysteryCount == payTable.GetCount() {
				jackpot := rand.Intn(100000000) + 1
				probability, _ := strconv.ParseFloat(payTable.GetPayout(), 64)
				if jackpot <= int(probability) {
					//進入jackpot bonus game 骰定
					//(level int32, isRecover bool, awardRecord int64)
					lh.GameLog.AddGameStep("即將進入JackpotGame")
					//標記下局進入JP
					lh.jpFlag = true
					lh.dfdcJackpotBonusGame(lh.getjackpotLevel())
				}
			}
		}
	}

	//以第一軸為基準
	firstBaida, result := lh.checkLineByReel(resultReel, 1)
	if firstBaida { //若第一軸百搭
		secondBaida, result2 := lh.checkLineByReel(resultReel, 2)
		result = append(result, result2...)
		if secondBaida {
			thirdBaiDa, result3 := lh.checkLineByReel(resultReel, 3)
			result = append(result, result3...)
			if thirdBaiDa {
				fourthBaiDa, result4 := lh.checkLineByReel(resultReel, 4)
				result = append(result, result4...)
				if fourthBaiDa {
					result = append(result, resultReel[5]...)
				}
			}
		}
	}

	result = removeStringRepByLoop(result)
	result = removeWild(result)
	lh.kaijiangsymbol = result

	if len(result) == 0 {
		return false, nil
	}

	return true, result
}

//取得全線賠付內容
func (lh *DFDCLogic) getResultAllGroup() []*netproto.DFDC_PayAllGroup {
	//全線賠付結果
	result := make([]*netproto.DFDC_PayAllGroup, 0)

	//從kaiJiang1、kaiJiang2、kaiJiang3獲得結果(symbol id)
	lh.reelSymbolResult = lh.getReelSymbolResultArr()
	//log
	lh.reelSymbolResultRange(func(obj *ReelSymbol) {
		//lh.GameLog.AddGameStep("reelid [%v] ;輪軸開獎結果 符號: [%v], 符號名稱:[%v]", obj.reelId, obj.symbolId, obj.symbolName)
		//mlog.System("reelid[%v] 輪軸所有開獎結果 symbole[%v][%v]", obj.reelId, obj.symbolId, obj.symbolName)
	})

	//開獎成功的符號(res)
	isWin, res := lh.getAllLineSymbol()
	if isWin {
		result = lh.checkCountAndLine(res)
	} else {
		return nil
	}
	return result
}

//取得全線賠付內容
func (lh *DFDCLogic) checkCountAndLine(res []string) []*netproto.DFDC_PayAllGroup {
	result := make([]*netproto.DFDC_PayAllGroup, 0)
	//res symbolName結果陣列
	payAllDet := new(netproto.DFDC_PayAllGroup)
	payLineDetail := make([]*netproto.DFDC_PayLineDetail, 0)
	//全線賠付細節
	PayAllDetail := make([]*netproto.DFDC_PayAllDetail, 0)

	//連線與得獎符號
	for _, jiang := range res {
		symbolIdRes := make([]int32, 0)
		blinkFlag := true //是否亮燈
		for reel := 1; reel <= int(lh.reelNum); reel++ {
			blink1 := false
			blink2 := false
			blink3 := false
			payLineDetaildata1 := new(netproto.DFDC_PayLineDetail)
			payLineDetaildata1.ReelID = proto.Int32(int32(reel))
			symbol := lh.resultSymbol[reel][0]
			payLineDetaildata1.SymbolID = proto.Int32(lh.kaiJiang1[reel-1])
			if blinkFlag && (symbol == jiang || lh.isEqualSymbole(WILD, symbol)) {
				blink1 = true
				symbolIdRes = append(symbolIdRes, lh.kaiJiang1[reel-1])
			}
			payLineDetaildata1.Blink = proto.Bool(blink1)
			payLineDetaildata1.Line = proto.Bool(false)
			payLineDetail = append(payLineDetail, payLineDetaildata1)

			payLineDetaildata2 := new(netproto.DFDC_PayLineDetail)
			payLineDetaildata2.ReelID = proto.Int32(int32(reel))
			symbol2 := lh.resultSymbol[reel][1]
			payLineDetaildata2.SymbolID = proto.Int32(lh.kaiJiang2[reel-1])
			if blinkFlag && (symbol2 == jiang || lh.isEqualSymbole(WILD, symbol2)) {
				blink2 = true
				symbolIdRes = append(symbolIdRes, lh.kaiJiang2[reel-1])
			}
			payLineDetaildata2.Blink = proto.Bool(blink2)
			payLineDetaildata2.Line = proto.Bool(false)
			payLineDetail = append(payLineDetail, payLineDetaildata2)

			payLineDetaildata3 := new(netproto.DFDC_PayLineDetail)
			payLineDetaildata3.ReelID = proto.Int32(int32(reel))
			symbol3 := lh.resultSymbol[reel][2]
			payLineDetaildata3.SymbolID = proto.Int32(lh.kaiJiang3[reel-1])
			if blinkFlag && (symbol3 == jiang || lh.isEqualSymbole(WILD, symbol3)) {
				blink3 = true
				symbolIdRes = append(symbolIdRes, lh.kaiJiang3[reel-1])
			} else {
				payLineDetaildata3.Blink = proto.Bool(false)
			}
			payLineDetaildata3.Blink = proto.Bool(blink3)
			payLineDetaildata3.Line = proto.Bool(false)
			payLineDetail = append(payLineDetail, payLineDetaildata3)
			if !blink1 && !blink2 && !blink3 {
				blinkFlag = false
			}
		}
		PayAllDetailData := new(netproto.DFDC_PayAllDetail)
		PayAllDetailData.SymbolName = proto.String(jiang)
		//計算該符號賠率
		rate := lh.getSymbolPayRate(jiang, lh.payiconCount[jiang])
		lh.payRate[jiang] = int32(rate)
		PayAllDetailData.PayRate = proto.Int32(lh.payRate[jiang] * int32(lh.userdata[0].userbetmoney) * lh.lineCount[jiang]) //該符號賠率
		PayAllDetailData.PayCount = proto.Int32(lh.payiconCount[jiang])                                                      //符號連續幾個
		PayAllDetailData.WayCount = proto.Int32(lh.lineCount[jiang])                                                         //符號連線總和
		lh.GameLog.AddGameStep("開獎符號: %s ,符號連續 %d 個 ;符號賠率: %d ;連線數總和:%d", jiang, lh.payiconCount[jiang], rate, lh.lineCount[jiang])
		for _, symbol := range symbolIdRes { //該連線符號ID列表
			PayAllDetailData.SymbolIDList = append(PayAllDetailData.SymbolIDList, symbol)
		}
		//TODO:是否觸發特殊事件(Jackpot、freeSpin)
		PayAllDetailData.FeaturePays = proto.Bool(false)
		PayAllDetail = append(PayAllDetail, PayAllDetailData)

	}
	payAllDet.PayLineDetail = payLineDetail
	payAllDet.PayAllDetail = PayAllDetail
	result = append(result, payAllDet)

	return result
}

//特殊符號賠率
func (lh *DFDCLogic) getSpecialSymbolPayRate(symbol string) int {
	wildRate := symbol[1:2]
	payRate := 0
	if wildRate != "j" {
		lenSymbol := len(symbol)
		payRate, _ = strconv.Atoi(symbol[1:lenSymbol])
	} else {
		//觸發jackpot
	}
	return payRate
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getmainGameTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("MainGameTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.MainGameTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	mainGameTableArr := slotdata.(*protodata.MainGameTable_ARRAY)
	lh.mainGameTable = mainGameTableArr.GetItems()
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getFreeSpinTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("FreeSpinTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.FreeSpinTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	freeSpinTableArr := slotdata.(*protodata.FreeSpinTable_ARRAY)
	lh.freeSpinTable = freeSpinTableArr.GetItems()
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getPickGameTable() {
	slotdata, pcfgerr := lh.ReadProtoConfig("PickGameTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.PickGameTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	pickGameTableArr := slotdata.(*protodata.PickGameTable_ARRAY)
	lh.pickGameTable = pickGameTableArr.GetItems()
}

//多福多財bonus遊戲
func (lh *DFDCLogic) dfdcJackpotBonusGame(level string) {
	switch level {
	case GrandLevel:
		lh.jackpotlevel = GRAND
	case MajorLevel:
		lh.jackpotlevel = MAJOR
	case MinorLevel:
		lh.jackpotlevel = MINOR
	case MiniLevel:
		lh.jackpotlevel = MINI
	}
	//取得獎金及分級
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	for _, table := range lh.jackpotTable {
		if int32(jackpotGroupId) == table.GetGroupId() && level == table.GetLevel() {
			//透過level取得水池值
			//實際金額*CurrentBet/MAXBet
			for _, detail := range lh.jackpotdataDetail {
				if lh.jackpotlevel != detail.level {
					continue
				}
				money := lh.userdata[0].userbetmoney * int64(lh.userdata[0].userbetsymbol)                                             // 當前下注金額
				inc := ((detail.totalContribution + int64(table.GetIncrement())*lh.userdata[0].allmoney) / 10000) - detail.awardTotal  // 增加金額，每次Spin貢獻值+已儲存累積的貢獻值-重置前累積JP出獎金額
				total := table.GetStartUp() + inc                                                                                      // 現在水池實際金額
				maxMoney := int64(lh.betvalue.betcount[len(lh.betvalue.betcount)-1] * lh.goldenSymbolRate[len(lh.goldenSymbolRate)-1]) // MAXBet

				lh.jackpotAward = total * money / maxMoney // 玩家實際看到的現在水池實際金額*CurrentBet/MAXBet
			}
		}
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameID)
	//獲得開獎結果
	lh.bonusProcess = lh.getBonusProcess(lh.jackpotlevel)
	bonusGame := new(netproto.DFDC_BonusGame)
	bonusGameDetail := make([]*netproto.DFDC_BonusGameDetail, 0)
	//取得符號
	for i := 0; i < len(lh.bonusProcess); i++ {
		bonusGameDetailData := new(netproto.DFDC_BonusGameDetail)
		bonusGameDetailData.Id = proto.Int32(int32(i + 1))
		bonusGameDetailData.Place = proto.Int32(0)
		bonusGameDetailData.Icon = proto.Int32(lh.bonusProcess[i])
		bonusGameDetail = append(bonusGameDetail, bonusGameDetailData)
	}
	bonusGame.BonusDetail = bonusGameDetail
	bonusGame.Money = proto.Int64(lh.jackpotAward) //Bonus Game結果總金額
	pmsg.MsgData = bonusGame
	lh.BoardcastGameUserMessage(pmsg)
	lh.updateBonusGame(lh.userdata[0].user.UserID)
}

//計算該符號賠率
func (lh *DFDCLogic) getSymbolPayRate(symbol string, payiconCount int32) int32 {
	payRate := int32(0)
	for _, payTable := range lh.payOutSymbole[symbol].payOutTable {
		if payiconCount == int32(payTable.GetCount()) && payTable.GetGroupId() == int32(lh.slotPatternTable.GetPayTableGroupId()) {
			if payTable.GetPayType() == chip {
				payout, _ := strconv.ParseFloat(payTable.GetPayout(), 64)
				payRate = int32(payout)
			} else if payTable.GetPayType() == chipXrate {
				//chipXrate需再乘上黃金符號賠率
				payout, _ := strconv.ParseFloat(payTable.GetPayout(), 64)
				payRate = int32(payout) * lh.userdata[0].userbetsymbol
			}
		}
	}

	return payRate
}

//場景輪軸設定
func (lh *DFDCLogic) getFreeReelSetting(betsymbol int32) []*netproto.DFDC_ReelSetting {
	//TODO:此輪軸設定須包含符號骰定(使用空結構整理輪軸資訊)
	result := make([]*netproto.DFDC_ReelSetting, 0)
	symbolGroupID := int32(0)
	freeReelNum := int32(0)
	//根據betsymbol決定表演第幾軸
	for _, freegameTable := range lh.freeSpinTable {
		conditionValue, _ := strconv.ParseFloat(freegameTable.GetConditionValue(), 64)
		if lh.freeSpinGroup == freegameTable.GetGroupId() && lh.userdata[0].userbetsymbol == int32(conditionValue) {
			freeReelNum = int32(freegameTable.GetFreeReelNum())
			symbolGroupID = int32(freegameTable.GetFreeSymbolGroupId())
		}
	}

	for reelId := 1; reelId <= int(freeReelNum); reelId++ {
		reelSetting := new(netproto.DFDC_ReelSetting)
		reelSetting.RealID = proto.Int32(int32(reelId))

		for _, s := range lh.normalSymbolDistTable {
			if int(s.GetReelNum()) == reelId && symbolGroupID == int32(s.GetGroupId()) {
				symbolGroup := new(netproto.DFDC_SymbolGroup)
				symbolId := s.GetId()
				symbolGroup.SymbolID = proto.Int32(int32(symbolId))
				symbolGroup.SymbolName = proto.String(s.GetSymbol())
				//symbolGroup.Order = proto.Int32(int32(s.GetOrder()))
				reelSetting.SymbolGroup = append(reelSetting.SymbolGroup, symbolGroup)

				if lh.reelAndSymbol[reelId] == nil {
					reelSymbolObj := new(ReelAndSymbolSetting)
					reelSymbolObj.symboleIdMap = make(map[int32]*netproto.DFDC_SymbolGroup, 0)
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

	return result
}

//紀錄玩家選擇設定
func (lh *DFDCLogic) recordUserChoose(user *gamelogic.GameUser, data []byte) {
	ust := &netproto.DFDC_BonusGameDetail{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析用户%vBonusGame內容错误%v", user, err)
	}
	//TODO:斷線重連時，需將此筆紀錄存進DB
	bonusRecord := new(BonusRecord)
	bonusRecord.id = ust.GetId()
	bonusRecord.place = ust.GetPlace()
	bonusRecord.icon = ust.GetIcon()
	lh.GameLog.AddGameStep("用户%d 選擇第: %d次; 選擇位置:%d,圖案:%d", user.UserID, ust.GetId(), ust.GetPlace(), ust.GetIcon())
	for i := 0; i < 9; i++ {
		turn := int32(i + 1)
		if int(turn) <= len(lh.bonusProcess) && ust.GetId() == turn {
			lh.bgSetChoose = append(lh.bgSetChoose, bonusRecord)
		}
	}
	if len(lh.bgSetChoose) < len(lh.bonusProcess) {
		lh.updateBonusGame(user.UserID)
	}
	if len(lh.bgSetChoose) == len(lh.bonusProcess) {
		lh.jpFlag = false
		lh.jpFlagUser = 0
		if lh.jackpotAward > 0 {
			lh.addHitoryLog(user, lh.jackpotAward, JPNOLINK, OPEN, lh.getjpData())
		}
		lh.GameLog.AddGameStep("JackpotGame圖案順序: %d", lh.bonusProcess)
		lh.GameLog.AddGameStep("JackpotGame獎金%d", lh.jackpotAward)
		lh.resetBonusGame(user)
		lh.bgSetChoose = make([]*BonusRecord, 0)
		lh.bonusProcess = make([]int32, 0)
		lh.bonusGameRecord = make([]*BonusRecord, 0)

		//獨立遊戲將視為獨立結算，freespin中當作freegame小遊戲
		//新增流水
		user.FlowAmount += lh.jackpotAward
		lh.updateUserMoney(lh.jackpotAward, 0)
		uscoresRet := make(map[int32]int64)
		uscoresRet[user.UserID] = lh.jackpotAward
		ucouponRet := make(map[int32]int64)
		ucouponRet[user.UserID] = -lh.coupon
		lh.GameLog.GameEnd(uscoresRet, ucouponRet)
		//更新水池
		lh.sendJackpotPoolS2C()
		lh.jackpotAward = 0

		lh.updateUserSorce(user)
		lh.gameEnd()
	}
}

//取得SymbolPayTable
func (lh *DFDCLogic) getSymbolPayTable() []*netproto.DFDC_SymbolPayTable {
	result := make([]*netproto.DFDC_SymbolPayTable, 0)
	for _, slotpayTable := range lh.slotPayTable {
		for i := 1; i <= 51; i++ {
			if i == int(slotpayTable.GetId()) {
				if i <= 45 || i > 48 {
					symbolPayTable := new(netproto.DFDC_SymbolPayTable)
					symbolPayTable.SymbolName = proto.String(slotpayTable.GetSymbol())
					symbolPayTable.Count = proto.Int32(slotpayTable.GetCount())
					rate, _ := strconv.ParseFloat(slotpayTable.GetPayout(), 64)
					symbolPayTable.Rate = proto.Int32(int32(rate))
					result = append(result, symbolPayTable)
				}
			}
		}
	}
	return result
}

//取得csv的賠付獎金設定
func (lh *DFDCLogic) getSlotPayTable2() {
	slotdata, pcfgerr := lh.ReadProtoConfig("SlotPayTable.bytes", func(data []byte) (proto.Message, error) {
		tableinfo := &protodata.SlotPayTable_ARRAY{}
		err := proto.Unmarshal(data, tableinfo)
		return tableinfo, err
	})

	if pcfgerr != nil {
		mlog.Error("解析文件错误%v", pcfgerr)
		return
	}

	slotPayTableArr := slotdata.(*protodata.SlotPayTable_ARRAY)
	lh.slotPayTable = slotPayTableArr.GetItems()
}

//讀取資料庫jackpot水池
func (lh *DFDCLogic) readJackpotPool() {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotJackpotNoLinkID)
	pdata := new(netproto.Slot_JackpotData)
	pdata.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)
	pdata.FlagID = proto.Int32(int32(FlagID))
	pdata.TableNo = proto.Int32(lh.TableManager.TableNo)
	//讀取資料不須填值
	pdata.Contribution = proto.Int64(0)
	pdata.TotalContribution = proto.Int64(0)
	pdata.AwardTotal = proto.Int64(0)
	pdata.Level = proto.Int32(1)
	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)
}

//讀取BonusGame紀錄
func (lh *DFDCLogic) readBonusGame(gameuser *gamelogic.GameUser) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID)
	detail := new(netproto.DFDC_JackpotBonusGameRet)
	detail.UserID = proto.Int32(gameuser.UserID)
	pmsg.MsgData = detail
	lh.SendRPCMessageFunc(true, pmsg)
}

//讀取FreeSpin紀錄
func (lh *DFDCLogic) readFreeSpinTimes(gameuser *gamelogic.GameUser) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotFreeSpinRetID)
	pdata := new(netproto.Slot_FreeSpinRead)
	pdata.UserID = proto.Int32(gameuser.UserID)
	pdata.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)
	pdata.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)
}

//利用id獲得計算後的水池值
func (lh *DFDCLogic) getJackpotPoolByID(id int32, group int32) int64 {
	//每次Spin貢獻值(contribution)=increment*toltalBet
	//現在水池實際金額=startUp+每次Spin貢獻值+已儲存累積的貢獻值-重置前累積JP出獎金額
	//玩家實際看到的現在水池實際金額*CurrentBet/MAXBet
	//水池會根據變更投注額即時變動
	pool := int64(0)
	level := ""
	switch id {
	case GRAND:
		level = GrandLevel
	case MAJOR:
		level = MajorLevel
	case MINOR:
		level = MinorLevel
	case MINI:
		level = MiniLevel
	}
	//jackpot表格
	for _, jpTable := range lh.jackpotTable {
		if group != jpTable.GetGroupId() {
			continue
		}
		for _, jpdetail := range lh.jackpotdataDetail {
			if level == jpTable.GetLevel() && id == jpdetail.level {
				pool = int64(jpTable.GetStartUp()) - jpdetail.awardTotal + (jpdetail.totalContribution / 10000)
			}
		}
		if pool == 0 && level == jpTable.GetLevel() {
			pool = int64(jpTable.GetStartUp())
		}
	}
	return pool
}

//新增玩家
func (lh *DFDCLogic) newUserData(gameuser *gamelogic.GameUser) {
	if len(lh.userdata) == 0 {
		ud := new(dfdcuserdata)
		ud.user = gameuser
		lh.userdata = append(lh.userdata, ud)
	}
}

//獲得japot層級
func (lh *DFDCLogic) getjackpotLevel() string {
	level := ""
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	total := int64(0)
	for _, table := range lh.jackpotTable {
		if int32(jackpotGroupId) == table.GetGroupId() {
			total += int64(table.GetPerProb())
		}
	}
	judge := rand.Int63n(total)
	perprob := int64(0)
	for _, jpTable := range lh.jackpotTable {
		if int32(jackpotGroupId) == jpTable.GetGroupId() {
			perprob += int64(jpTable.GetPerProb())
			if judge < perprob {
				level = jpTable.GetLevel()
				break
			}
		}
	}
	lh.GameLog.AddGameStep("JackpotGame獎金層級: %s", level)
	return level
}

//獲得遊戲過程
func (lh *DFDCLogic) getBonusProcess(level int32) []int32 {
	res := make([]int32, 0)
	all := getallres()
	for i := len(all) - 1; i > 0; i-- {
		j := rand.Intn(len(all))
		all[j], all[i] = all[i], all[j]
	}
	turn := rand.Intn(6) + 3
	for k := 0; k < turn; k++ {
		res = append(res, all[k])
	}
	res = checkCount(level, res)

	return res
}

func getallres() []int32 {
	res := make([]int32, 0)
	for i := int32(0); i < 4; i++ {
		for j := 0; j < 2; j++ {
			res = append(res, i+1)
		}
	}
	return res
}

func checkCount(level int32, res []int32) []int32 {
	levelCount := 0
	//將level結果補足3個
	for n := 0; n < len(res); n++ {
		if res[n] == level {
			levelCount++
		}
	}
	switch levelCount {
	case 0:
		res = append(res, level)
		res = append(res, level)
		res = append(res, level)
	case 1:
		res = append(res, level)
		res = append(res, level)
	case 2:
		res = append(res, level)
	}
	return res
}

//獲得遊戲過程
func (lh *DFDCLogic) getRecoverBonusGame() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameID)
	bonusGame := new(netproto.DFDC_BonusGame)
	bonusGameDetail := make([]*netproto.DFDC_BonusGameDetail, 0)
	//獲得開獎結果
	for _, bgRecord := range lh.bonusGameRecord {
		if bgRecord.icon != 0 {
			bonusGameDetailData := new(netproto.DFDC_BonusGameDetail)
			bonusGameDetailData.Id = proto.Int32(bgRecord.id)
			bonusGameDetailData.Place = proto.Int32(bgRecord.place)
			bonusGameDetailData.Icon = proto.Int32(bgRecord.icon)
			bonusGameDetail = append(bonusGameDetail, bonusGameDetailData)
		}
	}

	bonusGame.BonusDetail = bonusGameDetail
	bonusGame.Money = proto.Int64(lh.jackpotAward) //Bonus Game結果總金額
	pmsg.MsgData = bonusGame
	lh.BoardcastGameUserMessage(pmsg)
}

//FreeSpin存入DB
func (lh *DFDCLogic) updatefreespin(gameuser *gamelogic.GameUser) {
	user := lh.getUserDataByID(gameuser.UserID)
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotFreeSpinID)
	pdata := new(netproto.Slot_FreeSpinDetail)
	pdata.UserID = proto.Int32(gameuser.UserID)
	pdata.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)
	pdata.FlagID = proto.Int32(int32(FlagID))
	pdata.UserBet = proto.Int64(lh.getUserBetMoney())
	pdata.FreeSpinGroup = proto.Int32(lh.freeSpinGroup)
	pdata.Times = proto.Int32(lh.freeSpins)
	pdata.ChooseRate = proto.Int32(user.userbetsymbol)
	pdata.WinMoney = proto.Int64(lh.freeGameMoney)
	pdata.IsCoupon = proto.Bool(lh.isCoupon)
	pmsg.MsgData = pdata

	lh.SendRPCMessageFunc(true, pmsg)
}

//BonusGame存入DB
func (lh *DFDCLogic) updateBonusGame(userID int32) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID)
	pdata := new(netproto.DFDC_JackpotBonusGameRecord)
	pdata.UserID = proto.Int32(userID)
	recordStr := ""

	for i := 0; i < 9; i++ {
		turn := int32(i + 1)
		id := ""
		place := ""
		icon := ""
		if int(turn) <= len(lh.bgSetChoose) {
			id = fmt.Sprintf("%d", lh.bgSetChoose[i].id)
			place = fmt.Sprintf("%d", lh.bgSetChoose[i].place)
			icon = fmt.Sprintf("%d", lh.bgSetChoose[i].icon)
			if turn == 1 {
				recordStr += fmt.Sprintf("%v,%v,%v", id, place, icon)
			} else {
				recordStr += fmt.Sprintf("|%v,%v,%v", id, place, icon)
			}
		} else if int(turn) <= len(lh.bonusProcess) && int(turn) > len(lh.bgSetChoose) {
			id = fmt.Sprintf("%d", turn)
			place = fmt.Sprintf("%d", 0)
			icon = fmt.Sprintf("%d", lh.bonusProcess[i])
			if turn == 1 {
				recordStr += fmt.Sprintf("%v,%v,%v", id, place, icon)
			} else {
				recordStr += fmt.Sprintf("|%v,%v,%v", id, place, icon)
			}
		} else if id == "" {
			id = fmt.Sprintf("%d", turn)
			place = fmt.Sprintf("%d", 0)
			icon = fmt.Sprintf("%d", 0)
			if turn == 1 {
				recordStr += fmt.Sprintf("%v,%v,%v", id, place, icon)
			} else {
				recordStr += fmt.Sprintf("|%v,%v,%v", id, place, icon)
			}
		}
	}

	if recordStr == "" {
		recordStr = "1,0,0|2,0,0|3,0,0|4,0,0|5,0,0|6,0,0|7,0,0|8,0,0|9,0,0"
	}
	pdata.ChvRecordStr = proto.String(recordStr)
	pdata.Winmoney = proto.Int64(lh.jackpotAward)
	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)
}

//BonusGame存入DB
func (lh *DFDCLogic) getFreeSpinCheck() (bool, int32) {
	freeNow := false
	link := int32(0)
	group := int32(0)
	for _, reel1icon := range lh.resultSymbol[1] {
		//檢查第一軸有沒有SCATTER
		if lh.isEqualSymbole(SCATTER, reel1icon) {
			for _, reel2icon := range lh.resultSymbol[2] {
				//檢查第二軸有無跟第一軸相同，有則繼續
				if lh.isEqualSymbole(SCATTER, reel2icon) || lh.isBaiDa(reel2icon, reel1icon) {
					for _, reel3icon := range lh.resultSymbol[3] {
						//檢查第三軸有無跟第二軸相同，有則繼續
						if lh.isEqualSymbole(SCATTER, reel3icon) || lh.isBaiDa(reel3icon, reel2icon) {
							for _, reel4icon := range lh.resultSymbol[4] {
								//檢查第四軸有無跟第三軸相同，有則繼續，沒有則退回三連線記錄
								if lh.isEqualSymbole(SCATTER, reel4icon) || lh.isBaiDa(reel4icon, reel3icon) {
									for _, reel5icon := range lh.resultSymbol[5] {
										//檢查第五軸有無跟第四軸相同，有則在五連線紀錄，沒有則退回四連線紀錄
										if lh.isEqualSymbole(SCATTER, reel5icon) || lh.isBaiDa(reel5icon, reel4icon) {
											//五連線紀錄
											freeNow = true
											link = 5
										} else {
											//第四軸和第五軸不一樣，四連線紀錄
											freeNow = true
											link = 4
										}
									}
								} else {
									//第四軸和第三軸不一樣，三連線紀錄
									freeNow = true
									link = 3
								}
							}
						}
					}
				}
			}
		} else if lh.isBaiDa(reel1icon, "") {
			//如果第一軸是Wild，檢查第二軸有沒有scatter或Wild
			for _, reel2icon := range lh.resultSymbol[2] {
				//檢查第二軸有沒有scatter或Wild
				if lh.isEqualSymbole(SCATTER, reel2icon) || lh.isBaiDa(reel2icon, reel1icon) {
					for _, reel3icon := range lh.resultSymbol[3] {
						//檢查第三軸有沒有scatter或Wild
						if lh.isEqualSymbole(SCATTER, reel3icon) || lh.isBaiDa(reel3icon, reel2icon) {
							for _, reel4icon := range lh.resultSymbol[4] {
								//檢查第四軸有無跟第三軸相同，有則繼續，沒有則退回三連線記錄
								if lh.isEqualSymbole(SCATTER, reel4icon) || lh.isBaiDa(reel4icon, reel3icon) {
									for _, reel5icon := range lh.resultSymbol[5] {
										//檢查第五軸有無跟第四軸相同，有則在五連線紀錄，沒有則退回四連線紀錄
										if lh.isEqualSymbole(SCATTER, reel5icon) || lh.isBaiDa(reel5icon, reel4icon) {
											//第四軸和第三軸不一樣，檢查過去三軸是否有scatter
											if lh.isEqualSymbole(SCATTER, reel1icon) || lh.isEqualSymbole(SCATTER, reel2icon) || lh.isEqualSymbole(SCATTER, reel3icon) || lh.isEqualSymbole(SCATTER, reel4icon) || lh.isEqualSymbole(SCATTER, reel5icon) {
												freeNow = true
												link = 5
											}
										} else {
											//第四軸和第三軸不一樣，檢查過去三軸是否有scatter
											if lh.isEqualSymbole(SCATTER, reel1icon) || lh.isEqualSymbole(SCATTER, reel2icon) || lh.isEqualSymbole(SCATTER, reel3icon) || lh.isEqualSymbole(SCATTER, reel4icon) {
												freeNow = true
												link = 4
											}
										}
									}
								} else {
									//第四軸和第三軸不一樣，檢查過去三軸是否有scatter
									if lh.isEqualSymbole(SCATTER, reel1icon) || lh.isEqualSymbole(SCATTER, reel2icon) || lh.isEqualSymbole(SCATTER, reel3icon) {
										freeNow = true
										link = 3
									}
								}
							}
						}
					}
				}
			}
		}
	}
	if link > 0 {
		for _, payTable := range lh.slotPayTable {
			if payTable.GetSymbol() == "s1" && payTable.GetPayType() == free_spins && payTable.GetGroupId() == int32(lh.slotPatternTable.GetMainGameTableGroupId()) {
				if link == payTable.GetCount() {
					payOut, _ := strconv.ParseFloat(payTable.GetPayout(), 64)
					group = int32(payOut)
				}
			}
		}
	}
	return freeNow, group
}

//是否為百搭符號WILD, SCATTER
func (lh *DFDCLogic) isBaiDa(first string, next string) bool {
	isBaiDa := false

	if first == "" {
		return false
	}

	//先判斷第一個
	for _, attTable := range lh.symbolAttributesTable {
		if first == attTable.symbol && attTable.isWild == 1 {
			//當第一個符號是百搭，檢查第二個符號是不是例外
			wildEx := getWildEx(attTable)
			if len(wildEx) == 0 {
				isBaiDa = true
				lh.wildPayRate[next] = attTable.multiplier
			} else {
				for _, wildEx := range getWildEx(attTable) {
					if next != wildEx {
						isBaiDa = true
						lh.wildPayRate[next] = attTable.multiplier
					}
				}
			}
		}
	}
	return isBaiDa
}

//是否為百搭符號WILD, SCATTER
func getWildEx(attTable *SymbolAttributes) []string {
	wildEx := make([]string, 0)
	attExString := attTable.wildEx
	lastindex := 0
	for index, char := range attExString {
		if char == ',' {
			substr := attExString[lastindex:index]
			lastindex = index + 1
			wildEx = append(wildEx, substr)
		}
	}

	return wildEx
}

//发送场景
func (lh *DFDCLogic) checkReturn(gameuser *gamelogic.GameUser) {
	//從DB撈取玩家資料
	lh.readBonusGame(gameuser)
	lh.readFreeSpinTimes(gameuser)
}

//遊戲歷程紀錄
func (lh *DFDCLogic) addHitoryLog(gameuser *gamelogic.GameUser, win int64, gameType int32, logType int32, gameData string) {
	lh.newUserData(gameuser)
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotAddGameLogID)
	pdata := new(netproto.Slot_HistoryLogData)
	pdata.UserID = proto.Int32(gameuser.UserID)
	pdata.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	pdata.ServerID = proto.Int32(lh.TableManager.Room.ServerID)

	curAmount := int64(0)
	betMoney := int64(0)
	finalScore := int64(0)

	if lh.userdata[0].userbeginmoney != 0 {
		curAmount = lh.userdata[0].userbeginmoney
		lh.userdata[0].userbeginmoney += win
	} else {
		lh.userdata[0].userbeginmoney = gameuser.Score
		curAmount = gameuser.Score
		lh.userdata[0].userbeginmoney += win
	}

	//根據gameType類型來寫入紀錄
	if gameType == MAIN {
		//先給定起始金額，讓其他類型也可從此紀錄讀取
		betMoney = lh.userdata[0].userbetmoney * int64(lh.userdata[0].userbetsymbol)
	}

	pdata.CurrentAmount = proto.Int64(curAmount)
	pdata.CurrentCoupon = proto.Int64(gameuser.Coupon)
	pdata.BetMoney = proto.Int64(betMoney - lh.coupon)
	pdata.BetCoupon = proto.Int64(lh.coupon)

	//贏分分數=起始+贏分
	finalScore = curAmount + win - betMoney + lh.coupon

	pdata.WinMoney = proto.Int64(win)
	pdata.FinalScore = proto.Int64(finalScore)
	pdata.FinalCoupon = proto.Int64(gameuser.Coupon - lh.coupon)
	pdata.GameType = proto.Int32(gameType)
	pdata.LogType = proto.Int32(logType)
	pdata.GameData = proto.String(gameData)
	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)
}

//獲得主遊戲輪軸
func (lh *DFDCLogic) getGameData() string {
	gameData := ""
	for i := 0; i < int(lh.reelNum); i++ {
		reel := i + 1
		if i != 0 {
			gameData += fmt.Sprintf("|%v", "")
		}
		for _, icon := range lh.resultSymbol[reel] {
			gameData += fmt.Sprintf("%v,", icon)
		}
	}
	return gameData
}

//取得翻圖遊戲紀錄
func (lh *DFDCLogic) getjpData() string {
	jpData := ""
	for _, icon := range lh.bonusProcess {
		jpData += fmt.Sprintf("%v,", icon)
	}
	return jpData
}

//BonusGame存入DB
func (lh *DFDCLogic) resetBonusGame(gameuser *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID)
	pdata := new(netproto.DFDC_JackpotBonusGameRecord)
	pdata.UserID = proto.Int32(gameuser.UserID)
	recordStr := "1,0,0|2,0,0|3,0,0|4,0,0|5,0,0|6,0,0|7,0,0|8,0,0|9,0,0"
	pdata.ChvRecordStr = proto.String(recordStr)
	pdata.Winmoney = proto.Int64(0)
	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)
}

//傳送玩家上局下注資訊
func (lh *DFDCLogic) sendFreeBetDetailMsg(gameuser *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCUserRecoverID)

	pdata := new(netproto.DFDC_UserRecover)
	pdata.WinMoey = proto.Int64(lh.freespinRecord.winMoney)
	pdata.BetMoney = proto.Int64(lh.freespinRecord.userBet)
	pdata.GoldenSymbol = proto.Int32(lh.freespinRecord.chooseRate)

	pmsg.MsgData = pdata

	lh.SendMessageToUser(gameuser, pmsg)
}

//以第一軸為基準
func (lh *DFDCLogic) checkLineByReel(resultReel map[int][]string, reel int) (bool, []string) {
	firstBaida := false
	result := make([]string, 0)
	reelOneSlice := make([]string, 0)
	for reelIndex := 1; reelIndex <= int(lh.reelNum); reelIndex++ {
		if reelIndex == reel {
			reelOneSlice = resultReel[reelIndex]
		} else {
			//reelOneSlice compare resultReel[reelIndex]
			for _, iconReel1 := range reelOneSlice {
				countSlice := make([]int, 0)
				for reelIndex := 1; reelIndex <= int(lh.reelNum); reelIndex++ {
					count := 0
					for _, icon := range lh.resultSymbol[reelIndex] {
						//從第二軸開始比對
						if icon == iconReel1 {
							count++
						} else if lh.isBaiDa(icon, iconReel1) {
							//確認兩符號是否百搭
							count++
						} else if lh.isBaiDa(iconReel1, "") {
							//第一個符號百搭 確認該軸的下一軸是否有符號一樣
							firstBaida = true
						}
					}
					countSlice = append(countSlice, count)
				}

				//獲得第一輪軸的符號iconReel1在各輪軸的個數 countSlice
				lineCount := 0 //總線數
				for reelCount, line := range countSlice {
					if reelCount == 0 && line != 0 {
						lineCount = 1
						lineCount = lineCount * line
					} else if line != 0 {
						lineCount = lineCount * line
						lh.payiconCount[iconReel1] = int32(reelCount + 1)
					} else if reelCount <= 2 && line == 0 {
						lineCount = 0
						break
					} else {
						break
					}
				}
				if lineCount != 0 {
					lh.lineCount[iconReel1] = int32(lineCount)
					result = append(result, iconReel1)
				}
			}
		}
	}
	return firstBaida, result
}

func removeWild(order []string) []string {
	result := make([]string, 0) // 存放结果
	for i, icon := range order {
		flag := true
		if icon == "w1" {
			flag = false // 存在wild元素，标识为false
		}
		if flag { // 标识为false，不添加进结果
			result = append(result, order[i])
		}
	}
	return result
}

//to client jackpot 資料
func (lh *DFDCLogic) sendJPoolS2C() {
	if lh.isCoupon {
		return
	}
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotPoolS2CID)
	pdata := new(netproto.DFDC_JackpotPoolS2CMsg)
	jackpot := make([]*netproto.DFDC_JackpotPoolDetail, 0)
	jackpotGroupId := lh.slotPatternTable.GetJackpotGroup()
	//DFDC_JackpotPoolDetail
	//required int32 JackpotID=1;	 // 1:巨獎 2:大獎 3:中獎 4:小獎
	//required int64 Syspool = 2;	 // 水池值
	//optional string Level = 3;	 // 獎項名稱:Grand/Major/Minor/Mini
	for index, jackpotTable := range lh.jackpotTable {
		if index < 4 {
			totalcontri := int64(jackpotTable.GetIncrement()) * lh.userdata[0].allmoney
			jackpotDetail := new(netproto.DFDC_JackpotPoolDetail)
			id := int32(index + 1)
			jackpotDetail.JackpotID = proto.Int32(id)
			//計算4個值
			jackpotDetail.Syspool = proto.Int64(lh.getJackpotPoolByID(id, int32(jackpotGroupId)) + (totalcontri / 10000))
			jackpotDetail.Level = proto.String(jackpotTable.GetLevel())
			jackpot = append(jackpot, jackpotDetail)
		}
	}
	pdata.JackpotPoolID = jackpot
	pmsg.MsgData = pdata
	lh.BoardcastGameUserMessage(pmsg)
}

//查詢遊戲歷程紀錄
func (lh *DFDCLogic) loadGameHistory(user *gamelogic.GameUser, data []byte) {
	lh.newUserData(user)
	ust := &netproto.DFDC_GetGameLog{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析用户%v遊戲歷程紀錄內容错误%v", user, err)
	}
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotGetGameLogID)
	pdata := new(netproto.Slot_LoadHistoryData)
	pdata.UserID = proto.Int32(user.UserID)
	pdata.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	pdata.StartTime = proto.String(ust.GetStartTime())
	pdata.EndTime = proto.String(ust.GetEndTime())
	pdata.PageIndex = proto.Int32(ust.GetPageIndex())
	pdata.PageSize = proto.Int32(ust.GetPageSize())
	pmsg.MsgData = pdata
	lh.SendRPCMessageFunc(true, pmsg)
}
