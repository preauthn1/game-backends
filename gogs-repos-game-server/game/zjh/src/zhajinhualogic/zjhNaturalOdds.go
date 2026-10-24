package zhajinhualogic

import (
	"ZJHDataCfg"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"mylog"
	"poker"
	"time"
	"util"
)

//--------------------------------------------------------------------------
// 自然機率變動設定
//--------------------------------------------------------------------------
// 基本設定
type OddsBasicSetting struct {
	I32WeightSum		int32		// 權重總和
	I32CheckPeriod		int32		// 每發 N 副牌讀一次表
}

// 調整值對照表
type Adjustment struct {
	I32Difference		int32		// 每 N 局判斷機率正負差之間（萬分次數）
	I32Variation		int32		// 加扣值（萬分比）
}

type AdjustmentTable struct {
	PData		[]*Adjustment
}

// 牌型判斷範圍
type CardTypeOddsRange struct {
	OddsRange		[CardType_Count]int32
}

// 出牌紀錄
type CardTypeRecords struct {
	TypeCount		[CardType_Count]int32
}

// Mongo Log 資料
type NaturalOddsMongoData struct {
	UI64CardSetSN				uint64		// 發牌副數
	I32ActionType				int32		// 執行動作
	I32WeightSum				int32		// 權重總和
	I32PeriodicCheck			int32		// 檢查週期
	Ary_strUserCards			[]string	// 玩家手牌資料
	Ary_i32DefaultOdds			[CardType_Count]int32		// 預設機率表
	Ary_i32PeriodicOdds			[CardType_Count]int32		// 週期修訂機率表
	Ary_i32ChangeOdds			[CardType_Count]int32		// 變動機率表
	Ary_i32TypeCountNow			[CardType_Count]int32		// 此次發牌紀錄
	Ary_i32TypeCountInTable		[CardType_Count]int32		// 牌型紀錄表中原來的紀錄
	Ary_i32Difference			[CardType_Count]int32		// 正負差
	Ary_i32Variation			[CardType_Count]int32		// 加扣值
}

// 房間
const (
	RoomNo_1			= iota + 1		// 初級場
	RoomNo_2							// 中級場
	RoomNo_3							// 高級場
	RoomNo_4							// 富豪場
	Room_NextNo
)

// 自然機率執行動作
const (
	NOAction_Init				= iota	// 初始值
	NOAction_FaPai						// 發牌
	NOAction_PeriodicCheck				// 週期檢查
)

//--------------------------------------------------------------------------
var map_pBasicSetting map[int32]*OddsBasicSetting	// map[房間編號] - 基本設定值
var map_pAdjustmentTable map[int32]map[int32]*AdjustmentTable	// map[房間編號]map[牌型編號] - 加扣值對照表

var map_pDefaultOdds map[int32]map[int32]int32	// map[房間編號]map[牌型編號] - 預設牌型機率表
var map_pPeriodicOdds map[int32]map[int32]int32	// map[房間編號]map[牌型編號] - 週期修訂機率表，發牌時以此表機率為依據
var map_pChangeOdds map[int32]map[int32]int32	// map[房間編號]map[牌型編號] - 變動牌型機率表

var map_pCardTypeOddsRange map[int32]*CardTypeOddsRange	// map[房間編號] - 各牌型機率亂數範圍值
var map_pCardTypeRecords map[int32]map[int32]*CardTypeRecords	// map[房間編號]map[第幾副牌] - 各副牌所發出牌型紀錄表
var map_ui64CardSetPointer map[int32]uint64	// map[房間編號] - 牌組框架目前位置
var map_ui64TotalUserCount map[int32]uint64	// map[房間編號] - 遊戲人次

//--------------------------------------------------------------------------
//
// 讀取自然機率變動設定值
func GetNaturalOddsSetting() {
	map_pBasicSetting = make(map[int32]*OddsBasicSetting)
	map_pDefaultOdds = make(map[int32]map[int32]int32)
	map_pPeriodicOdds = make(map[int32]map[int32]int32)
	map_pChangeOdds = make(map[int32]map[int32]int32)
	map_pAdjustmentTable = make(map[int32]map[int32]*AdjustmentTable)
	map_pCardTypeOddsRange = make(map[int32]*CardTypeOddsRange)

	getBaiscSetting()
	getProbabilityTable()
	getAdjustmentTable_1()
	getAdjustmentTable_2()
	getAdjustmentTable_3()
	getAdjustmentTable_4()

	initData()
	
	mlog.System("讀取自然機率變動設定值成功")
}

// 讀取基本設定值
func getBaiscSetting()  {
	pTable, err := ReadProtoConfig("SettingTable.bytes", func(data []byte) (proto.Message, error) {
		pData := &ZJHDataCfg.SettingTable_ARRAY{}
		err := proto.Unmarshal(data, pData)
		return pData, err
	})

	if err != nil {
		mlog.Error("SettingTable.bytes 解析文件錯誤%v", err)
		return
	}

	ary_pData := pTable.(*ZJHDataCfg.SettingTable_ARRAY)
	for _, pData := range ary_pData.GetItems() {
		if pData != nil {
			pInfo := new(OddsBasicSetting)
			pInfo.I32WeightSum = int32(pData.GetWeightSum())
			pInfo.I32CheckPeriod = int32(pData.GetCheckPeriod())

			map_pBasicSetting[int32(pData.GetRoomNumber())] = pInfo
		}
	}

	for i := RoomNo_1; i < Room_NextNo; i++ {
		mlog.Debug("[Joecool] Room[%d], Count[%d], Period[%d]", i,
			map_pBasicSetting[int32(i)].I32WeightSum, map_pBasicSetting[int32(i)].I32CheckPeriod)
	}
}

// 讀取牌型預設機率表，並同時設定週期修訂機率表及變動牌型機率表
func getProbabilityTable() {
	pTable, err := ReadProtoConfig("ProbabilityTable.bytes", func(data []byte) (proto.Message, error) {
		pData := &ZJHDataCfg.ProbabilityTable_ARRAY{}
		err := proto.Unmarshal(data, pData)
		return pData, err
	})

	if err != nil {
		mlog.Error("ProbabilityTable.bytes 解析文件錯誤%v", err)
		return
	}

	ary_pData := pTable.(*ZJHDataCfg.ProbabilityTable_ARRAY)
	for _, pData := range ary_pData.GetItems() {
		if pData != nil {
			if map_pDefaultOdds[int32(pData.GetRoomNumber())] == nil {
				map_pDefaultOdds[int32(pData.GetRoomNumber())] = make(map[int32]int32)
			}

			map_pDefaultOdds[int32(pData.GetRoomNumber())][int32(pData.GetCardType())] = int32(pData.GetProbability())
		}
	}

	// 設定週期修訂機率表
	for i := RoomNo_1; i < Room_NextNo; i++ {
		if map_pPeriodicOdds[int32(i)] == nil {
			map_pPeriodicOdds[int32(i)] = make(map[int32]int32)
		}

		for j := SanPai; j < CardType_Count; j++ {
			map_pPeriodicOdds[int32(i)][int32(j)] = map_pDefaultOdds[int32(i)][int32(j)]
		}

		if map_pCardTypeOddsRange[int32(i)] == nil {
			map_pCardTypeOddsRange[int32(i)] = new(CardTypeOddsRange)
		}
		setCardTypeOddsRange(int32(i))
	}

	// 設定變動牌型機率表
	for i := RoomNo_1; i < Room_NextNo; i++ {
		if map_pChangeOdds[int32(i)] == nil {
			map_pChangeOdds[int32(i)] = make(map[int32]int32)
		}

		for j := SanPai; j < CardType_Count; j++ {
			map_pChangeOdds[int32(i)][int32(j)] = map_pDefaultOdds[int32(i)][int32(j)]
		}
	}
}

// 讀取初級場加扣值對照表
func getAdjustmentTable_1() {
	pTable, err := ReadProtoConfig("Adjustment_1_Table.bytes", func(data []byte) (proto.Message, error) {
		pData := &ZJHDataCfg.Adjustment_1_Table_ARRAY{}
		err := proto.Unmarshal(data, pData)
		return pData, err
	})

	if err != nil {
		mlog.Error("Adjustment_1_Table.bytes 解析文件錯誤%v", err)
		return
	}

	ary_pData := pTable.(*ZJHDataCfg.Adjustment_1_Table_ARRAY)
	for _, pData := range ary_pData.GetItems() {
		if pData != nil {
			if map_pAdjustmentTable[RoomNo_1] == nil {
				map_pAdjustmentTable[RoomNo_1] = make(map[int32]*AdjustmentTable)

				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				pATable := new(AdjustmentTable)
				pATable.PData = append(pATable.PData, pAdjust)

				map_pAdjustmentTable[RoomNo_1][int32(pData.GetCardType())] = pATable
			} else {
				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				if map_pAdjustmentTable[RoomNo_1][int32(pData.GetCardType())] == nil {
					pATable := new(AdjustmentTable)
					map_pAdjustmentTable[RoomNo_1][int32(pData.GetCardType())] = pATable
				}

				map_pAdjustmentTable[RoomNo_1][int32(pData.GetCardType())].PData =
				 	append(map_pAdjustmentTable[RoomNo_1][int32(pData.GetCardType())].PData, pAdjust)
			}
		}
	}

	sortDifference(RoomNo_1)
}

// 讀取中級場加扣值對照表
func getAdjustmentTable_2() {
	pTable, err := ReadProtoConfig("Adjustment_2_Table.bytes", func(data []byte) (proto.Message, error) {
		pData := &ZJHDataCfg.Adjustment_2_Table_ARRAY{}
		err := proto.Unmarshal(data, pData)
		return pData, err
	})

	if err != nil {
		mlog.Error("Adjustment_2_Table.bytes 解析文件錯誤%v", err)
		return
	}

	ary_pData := pTable.(*ZJHDataCfg.Adjustment_2_Table_ARRAY)
	for _, pData := range ary_pData.GetItems() {
		if pData != nil {
			if map_pAdjustmentTable[RoomNo_2] == nil {
				map_pAdjustmentTable[RoomNo_2] = make(map[int32]*AdjustmentTable)

				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				pATable := new(AdjustmentTable)
				pATable.PData = append(pATable.PData, pAdjust)

				map_pAdjustmentTable[RoomNo_2][int32(pData.GetCardType())] = pATable
			} else {
				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				if map_pAdjustmentTable[RoomNo_2][int32(pData.GetCardType())] == nil {
					pATable := new(AdjustmentTable)
					map_pAdjustmentTable[RoomNo_2][int32(pData.GetCardType())] = pATable
				}

				map_pAdjustmentTable[RoomNo_2][int32(pData.GetCardType())].PData =
				 	append(map_pAdjustmentTable[RoomNo_2][int32(pData.GetCardType())].PData, pAdjust)
			}
		}
	}

	sortDifference(RoomNo_2)
}

// 讀取高級場加扣值對照表
func getAdjustmentTable_3() {
	pTable, err := ReadProtoConfig("Adjustment_3_Table.bytes", func(data []byte) (proto.Message, error) {
		pData := &ZJHDataCfg.Adjustment_3_Table_ARRAY{}
		err := proto.Unmarshal(data, pData)
		return pData, err
	})

	if err != nil {
		mlog.Error("Adjustment_3_Table.bytes 解析文件錯誤%v", err)
		return
	}

	ary_pData := pTable.(*ZJHDataCfg.Adjustment_3_Table_ARRAY)
	for _, pData := range ary_pData.GetItems() {
		if pData != nil {
			if map_pAdjustmentTable[RoomNo_3] == nil {
				map_pAdjustmentTable[RoomNo_3] = make(map[int32]*AdjustmentTable)

				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				pATable := new(AdjustmentTable)
				pATable.PData = append(pATable.PData, pAdjust)

				map_pAdjustmentTable[RoomNo_3][int32(pData.GetCardType())] = pATable
			} else {
				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				if map_pAdjustmentTable[RoomNo_3][int32(pData.GetCardType())] == nil {
					pATable := new(AdjustmentTable)
					map_pAdjustmentTable[RoomNo_3][int32(pData.GetCardType())] = pATable
				}

				map_pAdjustmentTable[RoomNo_3][int32(pData.GetCardType())].PData =
				 	append(map_pAdjustmentTable[RoomNo_3][int32(pData.GetCardType())].PData, pAdjust)
			}
		}
	}

	sortDifference(RoomNo_3)
}

// 讀取富豪場加扣值對照表
func getAdjustmentTable_4() {
	pTable, err := ReadProtoConfig("Adjustment_4_Table.bytes", func(data []byte) (proto.Message, error) {
		pData := &ZJHDataCfg.Adjustment_4_Table_ARRAY{}
		err := proto.Unmarshal(data, pData)
		return pData, err
	})

	if err != nil {
		mlog.Error("Adjustment_4_Table.bytes 解析文件錯誤%v", err)
		return
	}

	ary_pData := pTable.(*ZJHDataCfg.Adjustment_4_Table_ARRAY)
	for _, pData := range ary_pData.GetItems() {
		if pData != nil {
			if map_pAdjustmentTable[RoomNo_4] == nil {
				map_pAdjustmentTable[RoomNo_4] = make(map[int32]*AdjustmentTable)

				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				pATable := new(AdjustmentTable)
				pATable.PData = append(pATable.PData, pAdjust)

				map_pAdjustmentTable[RoomNo_4][int32(int32(pData.GetCardType()))] = pATable
			} else {
				pAdjust := new(Adjustment)
				pAdjust.I32Difference = pData.GetDifference()
				pAdjust.I32Variation = pData.GetVariation()

				if map_pAdjustmentTable[RoomNo_4][int32(pData.GetCardType())] == nil {
					pATable := new(AdjustmentTable)
					map_pAdjustmentTable[RoomNo_4][int32(pData.GetCardType())] = pATable
				}

				map_pAdjustmentTable[RoomNo_4][int32(pData.GetCardType())].PData =
				 	append(map_pAdjustmentTable[RoomNo_4][int32(pData.GetCardType())].PData, pAdjust)
			}
		}
	}

	sortDifference(RoomNo_4)
}

// 將 pAdjust 依 I32Difference 由大到小排序，以防止 config 資料未按大小順序排列而影響後面的操作
func sortDifference(i32GameFlagID int32) {
	if i32GameFlagID < RoomNo_1 || i32GameFlagID > Room_NextNo {
		return
	}

	for _, pData := range map_pAdjustmentTable[i32GameFlagID] {
		if pData != nil {
			iLen := len(pData.PData)
			pTemp := new(Adjustment)
			for i := 0; i < iLen; i++ {
				for j := i; j < iLen; j++ {
					iDiff_1 := pData.PData[i].I32Difference
					iDiff_2 := pData.PData[j].I32Difference
					if iDiff_1 < iDiff_2 {
						pTemp = pData.PData[i]
						pData.PData[i] = pData.PData[j]
						pData.PData[j] = pTemp
					}
				}
			}
		}
	}
}

// 設定各牌型機率範圍值
func setCardTypeOddsRange(i32GameFlagID int32) {
	if i32GameFlagID < int32(RoomNo_1) || i32GameFlagID >= int32(Room_NextNo) {
		mlog.Debug("無此房間編號[%d]", i32GameFlagID)
		return
	}

	var map_iCardOdds [CardType_Count]int32

	for i := SanPai; i < CardType_Count; i++ {
		map_iCardOdds[i] = map_pPeriodicOdds[i32GameFlagID][int32(i)]
	}

	str := fmt.Sprintf("[Joecool] 房間編號[%d]機率表: ", i32GameFlagID)
	for i := SanPai; i < CardType_Count; i++ {
		str += fmt.Sprintf("[%s] %d, ", typeStringMap[i], map_iCardOdds[i])
	}
	mlog.Debug("%s", str)

	// 設定範圍
	str = "[Joecool] OddsRange: "
	for i := SanPai; i < CardType_Count; i++ {
		map_pCardTypeOddsRange[i32GameFlagID].OddsRange[i] = 0
		for j := 0; j <= i; j++ {
			map_pCardTypeOddsRange[i32GameFlagID].OddsRange[i] += map_iCardOdds[j]
		}
		str += fmt.Sprintf("[%s] %d, ", typeStringMap[i], map_pCardTypeOddsRange[i32GameFlagID].OddsRange[i])
	}
	mlog.Debug("%s", str)
}

func initData() {
	initCardTypeRecords()
	initBasicSetting()
}

// 初始化各副牌所發出牌型紀錄表
func initCardTypeRecords() {
	map_pCardTypeRecords = make(map[int32]map[int32]*CardTypeRecords)

	for iID := RoomNo_1; iID < Room_NextNo; iID++ {
		if map_pCardTypeRecords[int32(iID)] == nil {
			map_pCardTypeRecords[int32(iID)] = make(map[int32]*CardTypeRecords)
		}

		for i := 0; i < int(map_pBasicSetting[int32(iID)].I32CheckPeriod); i++ {
			pData := new(CardTypeRecords)
			map_pCardTypeRecords[int32(iID)][int32(i)] = pData
		}
	}
}

// 初始化基本設定值
func initBasicSetting() {
	map_ui64CardSetPointer = make(map[int32]uint64)
	map_ui64TotalUserCount = make(map[int32]uint64)

	for iID := RoomNo_1; iID < Room_NextNo; iID++ {
		map_ui64CardSetPointer[int32(iID)] = uint64(0)
		map_ui64TotalUserCount[int32(iID)] = uint64(0)
	}
}

// 依機率表發牌，傳入【房間編號】【玩家人數】【發牌用牌組】，回傳【所有玩家手牌】【發剩下的牌】
func FapaiByOddsTable(i32GameFlagID, i32TableNo int32, iUserCount int, pCardSet *poker.CardGroup) ([]*poker.CardGroup, *poker.CardGroup) {
	mlog.Debug("[Joecool] ===== FapaiByOddsTable() Room[%d] =====", i32GameFlagID)
	if pCardSet == nil {
		mlog.Debug("無牌組資料")
		return nil, pCardSet
	}

	if iUserCount <= 0 || iUserCount > pCardSet.Len() / COUNT_HandCards {
		mlog.Debug("人數不正確, iUserCount[%d], 或是人數超過可發出的牌組數[%d]", iUserCount, pCardSet.Len() / COUNT_HandCards)
		return nil, pCardSet
	}

	if i32GameFlagID < int32(RoomNo_1) || i32GameFlagID >= int32(Room_NextNo) {
		mlog.Debug("無此房間編號[%d]", i32GameFlagID)
		return nil, pCardSet
	}

	// 依機率表發牌
	pCardTypeRecords := new(CardTypeRecords)
	var ary_pUserCards []*poker.CardGroup	// 要發的玩家手牌
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iCardType := SanPai
	for i := 0; i < iUserCount; i++ {
		pUserCards := new(poker.CardGroup)
		// 亂數取牌型
		iRnd := util.RndInt(1, int(map_pBasicSetting[i32GameFlagID].I32WeightSum + 1))
		for iType := SanPai; iType < CardType_Count; iType++ {
			if iRnd - int(map_pCardTypeOddsRange[i32GameFlagID].OddsRange[iType]) <= 0 {
				iCardType = iType
				break
			}
		}
		mlog.Debug("[Joecool] ----- User[%d], Rnd[%d], iCardType[%d]%s -----", i, iRnd, iCardType, typeStringMap[iCardType])
		pUserCards, iCardType, pRest = GetCardsByCardType(iCardType, pRest)
		mlog.Debug("[Joecool] UserCards: %s, Type[%s]", pUserCards.String(), typeStringMap[iCardType])
		mlog.Debug("[Joecool] RestCard: %s", pRest.String())

		pCardTypeRecords.TypeCount[iCardType]++

		ary_pUserCards = append(ary_pUserCards, pUserCards)
	}

	str := ""
	for i := SanPai; i < CardType_Count; i++ {
		str += fmt.Sprintf("[%s]%d, ", typeStringMap[i], pCardTypeRecords.TypeCount[i])
	}
	mlog.Debug("[Joecool] %s", str)

	naturalOddsProcess(i32GameFlagID, i32TableNo, iUserCount, *pCardTypeRecords, ary_pUserCards)

	return ary_pUserCards, pCardSet
}

// 機率表變動處理
func naturalOddsProcess(i32GameFlagID, i32TableNo int32, iUserCount int, pCardTypeRecords CardTypeRecords, ary_pUserCards []*poker.CardGroup) {
	mlog.Debug("[Joecool] ===== naturalOddsProcess() Room[%d] =====", i32GameFlagID)

	if i32GameFlagID < int32(RoomNo_1) || i32GameFlagID >= int32(Room_NextNo) {
		mlog.Debug("無此房間編號[%d]", i32GameFlagID)
		return
	}

	pLogData := new(NaturalOddsMongoData)
	str := ""

	if map_ui64CardSetPointer[i32GameFlagID] <= 0 {
		for iNo := RoomNo_1; iNo < Room_NextNo; iNo++ {
			pLogData := new(NaturalOddsMongoData)
			pLogData.UI64CardSetSN = map_ui64CardSetPointer[int32(iNo)]
			pLogData.I32ActionType = NOAction_Init
			pLogData.I32WeightSum = map_pBasicSetting[int32(iNo)].I32WeightSum
			pLogData.I32PeriodicCheck = map_pBasicSetting[int32(iNo)].I32CheckPeriod

			// 機率表
			for i := SanPai; i < CardType_Count; i++ {
				pLogData.Ary_i32DefaultOdds[i] = map_pDefaultOdds[int32(iNo)][int32(i)]
				pLogData.Ary_i32PeriodicOdds[i] = map_pPeriodicOdds[int32(iNo)][int32(i)]
				pLogData.Ary_i32ChangeOdds[i] = map_pChangeOdds[int32(iNo)][int32(i)]
			}

			naturalOddsLog(int32(iNo), i32TableNo, pLogData)
		}
	}

	map_ui64TotalUserCount[i32GameFlagID] += uint64(iUserCount)

	// ----- MongoDB 記錄用 -----
	// 權重總和
	pLogData.I32WeightSum = map_pBasicSetting[i32GameFlagID].I32WeightSum

	// 檢查週期
	pLogData.I32PeriodicCheck = map_pBasicSetting[i32GameFlagID].I32CheckPeriod

	// 發出牌型數
	for i := SanPai; i < CardType_Count; i++ {
		pLogData.Ary_i32TypeCountNow[i] = pCardTypeRecords.TypeCount[i]
	}

	// 玩家手牌
	for i := 0; i < len(ary_pUserCards); i++ {
		iType := GetType(ary_pUserCards[i])
		str = fmt.Sprintf("%s [%s](%d)", ary_pUserCards[i].String(), typeStringMap[int(iType)], iType)
		pLogData.Ary_strUserCards = append(pLogData.Ary_strUserCards, str)
	}
	// ----- MongoDB 記錄用 -----

	i32CheckPeriod := map_pBasicSetting[i32GameFlagID].I32CheckPeriod
	i32CardSetPointer := int32(map_ui64CardSetPointer[i32GameFlagID] % uint64(map_pBasicSetting[i32GameFlagID].I32CheckPeriod))

	mlog.Debug("[Joecool]--------------------------------------------------------------------------------")
	mlog.Debug("[Joecool] 記錄發出的牌型數量，並調整變動牌型機率表")
	mlog.Debug("[Joecool] CardSetPointer[%d]", i32CardSetPointer)
	// [Joecool] for debug begin
	str = ""
	strNew := ""
	strOriginal := ""
	for i := SanPai; i < CardType_Count; i++ {
		str += fmt.Sprintf("[%s]%d, ", typeStringMap[i], map_pChangeOdds[i32GameFlagID][int32(i)])
		strNew += fmt.Sprintf("[%s]%d, ", typeStringMap[i], pCardTypeRecords.TypeCount[int32(i)])
		strOriginal += fmt.Sprintf("[%s]%d, ", typeStringMap[i], map_pCardTypeRecords[i32GameFlagID][i32CardSetPointer].TypeCount[i])
	}
	mlog.Debug("[Joecool] 改變前 ChangeOdds: %s", str)
	mlog.Debug("[Joecool] New Card Record: %s", strNew)
	mlog.Debug("[Joecool] Original Record: %s", strOriginal)
	// [Joecool] for debug end

	// 記錄發出的牌型數量，並調整變動牌型機率表
	for i := SanPai; i < CardType_Count; i++ {
		// 原牌型機率 + 此次牌型發出數 - i32CardSetPointer 所指向的牌型數
		map_pChangeOdds[i32GameFlagID][int32(i)] +=
			pCardTypeRecords.TypeCount[int32(i)] - map_pCardTypeRecords[i32GameFlagID][i32CardSetPointer].TypeCount[i]

		// 更新牌型數量紀錄表
		pLogData.Ary_i32TypeCountInTable[i] = map_pCardTypeRecords[i32GameFlagID][i32CardSetPointer].TypeCount[i]	// Mongo Log 記錄用
		map_pCardTypeRecords[i32GameFlagID][i32CardSetPointer].TypeCount[i] = pCardTypeRecords.TypeCount[int32(i)]
	}

	i32CardSetPointer++
	map_ui64CardSetPointer[i32GameFlagID]++

	// [Joecool] for debug begin
	str = ""
	for i := SanPai; i < CardType_Count; i++ {
		str += fmt.Sprintf("[%s]%d, ", typeStringMap[i], map_pChangeOdds[i32GameFlagID][int32(i)])
	}
	mlog.Debug("[Joecool] 改變後 ChangeOdds: %s", str)
	mlog.Debug("[Joecool]--------------------------------------------------------------------------------")
	// [Joecool] for debug end

	// 機率表及發出牌型數
	for i := SanPai; i < CardType_Count; i++ {
		pLogData.Ary_i32DefaultOdds[i] = map_pDefaultOdds[i32GameFlagID][int32(i)]
		pLogData.Ary_i32PeriodicOdds[i] = map_pPeriodicOdds[i32GameFlagID][int32(i)]
		pLogData.Ary_i32ChangeOdds[i] = map_pChangeOdds[i32GameFlagID][int32(i)]
	}
	pLogData.I32ActionType = NOAction_FaPai
	pLogData.UI64CardSetSN = map_ui64CardSetPointer[i32GameFlagID]

	naturalOddsLog(i32GameFlagID, i32TableNo, pLogData)

	// 若是已到達週期檢查機率表的時候，則進行機率表調整
	if i32CardSetPointer % i32CheckPeriod == 0 {
		// 到達調整機率表週期，進行調整
		mlog.Debug("[Joecool]================================================================================")
		mlog.Debug("[Joecool] 到達調整機率表週期：%d 次，進行調整", i32CheckPeriod)

		// [Joecool] for debug begin
		str = "[Joecool] 改變前 PeriodicOdds: "
		for i := SanPai; i < CardType_Count; i++ {
			str += fmt.Sprintf("[%s]%d ", typeStringMap[i], map_pPeriodicOdds[i32GameFlagID][int32(i)])
		}
		mlog.Debug("%s", str)
		// [Joecool] for debug end

		// 清除與週期檢查無關的資料
		for i := SanPai; i < CardType_Count; i++ {
			pLogData.Ary_i32TypeCountNow[int32(i)] = 0
			pLogData.Ary_i32TypeCountInTable[int32(i)] = 0
		}

		str = "[Joecool] 正負差: "
		ary_i32Diff := new([CardType_Count]int32)
		for i := SanPai; i < CardType_Count; i++ {
			ary_i32Diff[i] = map_pChangeOdds[i32GameFlagID][int32(i)] - map_pDefaultOdds[i32GameFlagID][int32(i)]
			pLogData.Ary_i32Difference[int32(i)] = ary_i32Diff[i]
			str += fmt.Sprintf("[%s]%d ", typeStringMap[i], ary_i32Diff[i])
		}
		mlog.Debug("%s", str)

		i32TotalOdds := int32(0)
		for iType := DuiZi; iType < CardType_Count; iType++ {
			i32Variation := int32(0)
			mlog.Debug("[Joecool] ---------- %s ----------", typeStringMap[iType])
			for i := 0; i < len(map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData); i++ {
				if i == 0 {
					if ary_i32Diff[iType] >= map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i].I32Difference {
						i32Variation = map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i].I32Variation
					}
				} else if i == len(map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData) - 1 {
					if ary_i32Diff[iType] < map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i-1].I32Difference {
						i32Variation = map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i].I32Variation
					}
				} else {
					if ary_i32Diff[iType] >= map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i].I32Difference &&
						ary_i32Diff[iType] < map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i-1].I32Difference {
						i32Variation = map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i].I32Variation
					}
				}
				mlog.Debug("[Joecool] i:%d  Diff[%s]%d, Difference[%d], Variation[%d]",	i, typeStringMap[iType], ary_i32Diff[iType],
					map_pAdjustmentTable[i32GameFlagID][int32(iType)].PData[i].I32Difference, i32Variation)
			}

			pLogData.Ary_i32Variation[int32(iType)] = i32Variation // Mongo Log 記錄用

			map_pPeriodicOdds[i32GameFlagID][int32(iType)] = map_pDefaultOdds[i32GameFlagID][int32(iType)] + i32Variation
			i32TotalOdds += map_pPeriodicOdds[i32GameFlagID][int32(iType)]

			if map_pPeriodicOdds[i32GameFlagID][int32(iType)] < 0 {
				mlog.Error("牌型[%s] 差異值小於 0, 值 = %d, 正負差[%d]",
					typeStringMap[iType], map_pPeriodicOdds[i32GameFlagID][int32(iType)], ary_i32Diff[iType])

				map_pPeriodicOdds[i32GameFlagID][int32(iType)] = 0
			}
		}
		pLogData.Ary_i32Variation[int32(SanPai)] = (map_pBasicSetting[i32GameFlagID].I32WeightSum - i32TotalOdds) - map_pPeriodicOdds[i32GameFlagID][int32(SanPai)] 	// Mongo Log 記錄用
		map_pPeriodicOdds[i32GameFlagID][int32(SanPai)] = map_pBasicSetting[i32GameFlagID].I32WeightSum - i32TotalOdds
		
		// [Joecool] for debug begin
		str = "[Joecool] 加扣值: "
		for i := SanPai; i < CardType_Count; i++ {
			str += fmt.Sprintf("[%s]%d ", typeStringMap[i], pLogData.Ary_i32Variation[int32(i)])
		}
		mlog.Debug("%s", str)
		// [Joecool] for debug end

		// 機率表及發出牌型數
		for i := SanPai; i < CardType_Count; i++ {
			pLogData.Ary_i32DefaultOdds[i] = map_pDefaultOdds[i32GameFlagID][int32(i)]
			pLogData.Ary_i32PeriodicOdds[i] = map_pPeriodicOdds[i32GameFlagID][int32(i)]
			pLogData.Ary_i32ChangeOdds[i] = map_pChangeOdds[i32GameFlagID][int32(i)]
		}
		pLogData.I32ActionType = NOAction_PeriodicCheck

		naturalOddsLog(i32GameFlagID, i32TableNo, pLogData)

		// [Joecool] for debug begin
		str = "[Joecool] 改變後 PeriodicOdds: "
		for i := SanPai; i < CardType_Count; i++ {
			str += fmt.Sprintf("[%s]%d ", typeStringMap[i], map_pPeriodicOdds[i32GameFlagID][int32(i)])
		}
		mlog.Debug("%s", str)
		// [Joecool] for debug end
		mlog.Debug("[Joecool]--------------------------------------------------------------------------------")

		setCardTypeOddsRange(i32GameFlagID)

		mlog.Debug("[Joecool]================================================================================")
	}
}

// 自然機率 log
func naturalOddsLog(i32GameFlagID, i32TableNo int32, pMongoData *NaturalOddsMongoData) {
	if pMongoData == nil {
		return
	}

	type LogData struct {
		GroupName				string
		RoomNo					int32		// 房間編號
		CardSetSN				uint64		// 發牌副數
		ActionType				int32		// 執行動作，0: 初始值, 1: 發牌, 2: 週期檢查
		WeightSum				int32		// 權重總和
		CheckPeriod				int32		// 檢查週期
		TotalUserCount			uint64		// 玩家總人次
		UserCards				[]string	// 玩家手牌資料
		DefaultOdds				[CardType_Count]int32		// 預設機率
		PeriodicOdds			[CardType_Count]int32		// 週期機率
		ChangeOdds				[CardType_Count]int32		// 變動機率
		TypeCountNow			[CardType_Count]int32		// 此次發牌數
		TypeCountInTable		[CardType_Count]int32		// 牌型紀錄表中原來的紀錄
		Difference				[CardType_Count]int32		// 正負差
		Variation				[CardType_Count]int32		// 加扣值
		LogTime					string	// log 時間
	}

	msg := LogData{
		GroupName:      		"NaturalOdds",
		RoomNo:					i32GameFlagID,
		CardSetSN:				pMongoData.UI64CardSetSN,
		ActionType:				pMongoData.I32ActionType,
		WeightSum:				pMongoData.I32WeightSum,
		CheckPeriod:			pMongoData.I32PeriodicCheck,
		TotalUserCount:			map_ui64TotalUserCount[i32GameFlagID],
		UserCards:				pMongoData.Ary_strUserCards,
		DefaultOdds:			pMongoData.Ary_i32DefaultOdds,
		PeriodicOdds:			pMongoData.Ary_i32PeriodicOdds,
		ChangeOdds:				pMongoData.Ary_i32ChangeOdds,
		TypeCountNow:			pMongoData.Ary_i32TypeCountNow,
		TypeCountInTable:		pMongoData.Ary_i32TypeCountInTable,
		Difference:				pMongoData.Ary_i32Difference,
		Variation:				pMongoData.Ary_i32Variation,
		LogTime:				time.Now().Format("2006-01-02 15:04:05.000Z"),
	}

	mylog.MongoLog(-1, 0, i32TableNo, msg)
}
