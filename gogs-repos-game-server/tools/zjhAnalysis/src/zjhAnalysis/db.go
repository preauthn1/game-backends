package zjhAnalysis

import (
	"encoding/csv"
	"fmt"
	"gopkg.in/mgo.v2"
	"mlog"
	"os"
	"time"
)

// 牌型
const (
	SanPai = iota
	DuiZi
	SunZi
	JiaHua
	SunJin
	BaoZi
	S235
	CardType_Count
)

var map_strTypeString = map[int]string {
	SanPai: "散牌",
	DuiZi:  "對子",
	SunZi:  "順子",
	JiaHua: "金花",
	SunJin: "順金",
	BaoZi:  "豹子",
	S235:	"235",
}

var map_strActionString = map[int]string{
	0: "初始值",
	1: "發牌",
	2: "週期檢查",
}

type MongoDatabaseQueue struct {
	db            *mgo.Database
	coll          *mgo.Collection
	perWriteCount int
	log           *mlog.DefaultLogger
}

const (
	COUNT_User			int = 5
)

func NewMongoDB() *MongoDatabaseQueue {
	db := NewMongoDatabaseQueue(Config.MongoConn.Connstring, Config.MongoConn.Pool, Config.MongoConn.Perwritecount,
		mlog.NewDefaultLogger(), Config.MongoConn.Database, Config.MongoConn.Collection)
	return db
}

func NewMongoDatabaseQueue(connString string, connPool int, perWriteCount int, log *mlog.DefaultLogger, database string, collection string) *MongoDatabaseQueue {
	uri := "mongodb://" + connString
	session, err := mgo.DialWithTimeout(uri, 10*time.Minute)
	if err != nil {
		mlog.Error("MongoDB Error : %v", err.Error())
	}

	db := session.DB(database)
	coll := db.C(collection)

	q := new(MongoDatabaseQueue)
	q.db = db
	q.coll = coll
	q.perWriteCount = perWriteCount
	q.log = log
	return q
}

// 自然機率資料
type ZjhOddsData struct {
	ServerID   int32 `bson:"serverid" csv:"-"` //伺服器編號
	FrameID    int64 `bson:"frameid" csv:"-"`
	LogMessage `bson:"message"`
	TableNo    int32     `bson:"tableno" csv:"-"`
	CreateTime time.Time `bson:"createtime" csv:"-"`
}

// 自然機率紀錄
type LogMessage struct {
	GroupName      			string 		`bson:"groupname" csv:"-"`                   // 自然機率
	RoomNo					int32		`bson:"roomno" csv:"-"`		// 房間編號
	CardSetSN				uint64		`bson:"cardsetsn" csv:"-"`		// 發牌副數
	ActionType				int32		`bson:"actiontype" csv:"-"`		// 執行動作，0: 初始值, 1: 發牌, 2: 週期檢查
	WeightSum				int32		`bson:"weightsum" csv:"-"`		// 權重總和
	CheckPeriod				int32		`bson:"checkperiod" csv:"-"`		// 檢查週期
	TotalUserCount			int			`bson:"totalusercount" csv:"-"`		// 玩家人數
	UserCards				[]string	`bson:"usercards" csv:"-"`		// 玩家手牌資料
	DefaultOdds				[CardType_Count]int32	`bson:"defaultodds" csv:"-"`	// 預設機率
	PeriodicOdds			[CardType_Count]int32	`bson:"periodicodds" csv:"-"`	// 週期機率
	ChangeOdds				[CardType_Count]int32	`bson:"changeodds" csv:"-"`	// 變動機率
	TypeCountNow			[CardType_Count]int32	`bson:"typecountnow" csv:"-"`	// 此次牌型數
	TypeCountInTable		[CardType_Count]int32	`bson:"typecountintable" csv:"-"`	// 牌型紀錄表中原來的紀錄
	Difference				[CardType_Count]int32	`bson:"difference" csv:"-"`	// 正負差
	Variation				[CardType_Count]int32	`bson:"variation" csv:"-"`	// 加扣值
}

func (m *MongoDatabaseQueue) FindOddsData(query interface{}) []ZjhOddsData {

	result := []ZjhOddsData{}
	//result := bson.M{}
	//iter := m.coll.exportCsv(query).Iter()
	//for iter.Next(&result) {
	//	m.writeCSVFile(result)
	//}

	sort := m.getSort()
	m.coll.Find(query).Sort(sort).Limit(Config.MongoConn.Limit).All(&result)

	return result

	//TODO check
}

func (m *MongoDatabaseQueue) getSort() string {
	sort := ""
	if Config.MongoConn.Sort == 1 {
		sort = fmt.Sprintf("message.logtime")
	} else {
		sort = fmt.Sprintf("-message.logtime")
	}
	return sort
}

func (m *MongoDatabaseQueue) WriteCSVOddsData(result []ZjhOddsData, fileName string) {
	if len(result) <= 0 {
		return
	}

	if Exists(fileName + ".csv") {
		err := os.Remove(fileName + ".csv")

		if err != nil {
			mlog.Error("Delete Error")
			return
		}
	}

	clientsFile, err := os.OpenFile(fileName+".csv", os.O_RDWR|os.O_CREATE, os.ModePerm)
	if err != nil {
		panic(err)
	}
	defer clientsFile.Close()

	//mlog.System("result: %v", result)

	var ary_strResult [][]string
	// 設定標題
	ary_strCaption := []string {
		"ServerID",
		"GroupName",
		"房間編號",
		"桌號",
		"發牌副數",
		"動作類別",
		"權重總和",
		"檢查週期",
		"總人次",
		"玩家1手牌",
		"玩家2手牌",
		"玩家3手牌",
		"玩家4手牌",
		"玩家5手牌",
		"預設\n[單張]",
		"預設\n[對子]",
		"預設\n[順子]",
		"預設\n[金花]",
		"預設\n[順金]",
		"預設\n[豹子]",
		"預設\n[235]",
		"週期\n[單張]",
		"週期\n[對子]",
		"週期\n[順子]",
		"週期\n[金花]",
		"週期\n[順金]",
		"週期\n[豹子]",
		"週期\n[235]",
		"變動\n[單張]",
		"變動\n[對子]",
		"變動\n[順子]",
		"變動\n[金花]",
		"變動\n[順金]",
		"變動\n[豹子]",
		"變動\n[235]",
		"現在牌型\n[單張]",
		"現在牌型\n[對子]",
		"現在牌型\n[順子]",
		"現在牌型\n[金花]",
		"現在牌型\n[順金]",
		"現在牌型\n[豹子]",
		"現在牌型\n[235]",
		"紀錄牌型\n[單張]",
		"紀錄牌型\n[對子]",
		"紀錄牌型\n[順子]",
		"紀錄牌型\n[金花]",
		"紀錄牌型\n[順金]",
		"紀錄牌型\n[豹子]",
		"紀錄牌型\n[235]",
		"正負差\n[單張]",
		"正負差\n[對子]",
		"正負差\n[順子]",
		"正負差\n[金花]",
		"正負差\n[順金]",
		"正負差\n[豹子]",
		"正負差\n[235]",
		"加扣值\n[單張]",
		"加扣值\n[對子]",
		"加扣值\n[順子]",
		"加扣值\n[金花]",
		"加扣值\n[順金]",
		"加扣值\n[豹子]",
		"加扣值\n[235]",
		"紀錄時間",
	}

	ary_strResult = append(ary_strResult, ary_strCaption)

	// 填入資料
	for i := 0; i < len(result); i++ {
		var ary_strData []string

		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].ServerID))	// 伺服器編號
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].GroupName))	// GroupName
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].RoomNo))	// 房間編號
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].TableNo))	// 桌號
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].CardSetSN))	// 發牌副數
		ary_strData = append(ary_strData, fmt.Sprintf("%v %v", result[i].ActionType, map_strActionString[int(result[i].ActionType)]))	// 執行動作，0: 初始值, 1: 發牌, 2: 週期檢查
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].WeightSum))	// 權重總和
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].CheckPeriod))	// 檢查週期
		ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].TotalUserCount))	// 玩家總人次
		// 玩家手牌資料
		for j := 0; j < COUNT_User; j++ {
			if j < len(result[i].UserCards) {
				ary_strData = append(ary_strData, result[i].UserCards[j])
			} else {
				ary_strData = append(ary_strData, "")
			}
		}
		// 預設機率
		for j := 0; j < len(result[i].DefaultOdds); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].DefaultOdds[j]))
		}
		// 週期機率
		for j := 0; j < len(result[i].PeriodicOdds); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].PeriodicOdds[j]))
		}
		// 變動機率
		for j := 0; j < len(result[i].ChangeOdds); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].ChangeOdds[j]))
		}
		// 此次牌型數
		for j := 0; j < len(result[i].TypeCountNow); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].TypeCountNow[j]))
		}
		// 牌型紀錄表中原來的紀錄
		for j := 0; j < len(result[i].TypeCountInTable); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].TypeCountInTable[j]))
		}
		// 正負差
		for j := 0; j < len(result[i].Difference); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].Difference[j]))
		}
		// 加扣值
		for j := 0; j < len(result[i].Variation); j++ {
			ary_strData = append(ary_strData, fmt.Sprintf("%v", result[i].Variation[j]))
		}
		// 紀錄時間
		ary_strData = append(ary_strData, result[i].CreateTime.String())

		ary_strResult = append(ary_strResult, ary_strData)
	}

	// 寫成 csv 檔
	csvWriter := csv.NewWriter(clientsFile)
	for i := 0; i < len(ary_strResult); i++ {
		err := csvWriter.Write(ary_strResult[i])
		if err != nil {
			mlog.Error("write csv err %v", err.Error())
			panic(err)
		}
	}
	csvWriter.Flush()

	//gocsv.MarshalString(&result)
	//
	//err = gocsv.MarshalFile(&result, clientsFile)
	//// Use this to save the CSV back to the file
	//if err != nil {
	//	mlog.Error("err %v", err.Error())
	//	panic(err)
	//}
	mlog.System("Done Count[%v]", len(result))
}

func Exists(name string) bool {
	if _, err := os.Stat(name); err != nil {
		if os.IsNotExist(err) {
			return false
		}
	}
	return true
}
