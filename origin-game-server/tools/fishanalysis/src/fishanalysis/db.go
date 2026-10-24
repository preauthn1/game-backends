package fishanalysis

import (
	"fmt"
	"example.invalid/gocsv"
	"gopkg.in/mgo.v2"
	"mlog"
	"os"
	"time"
)

type MongoDatabaseQueue struct {
	db            *mgo.Database
	coll          *mgo.Collection
	perWriteCount int
	log           *mlog.DefaultLogger
}

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

//系統水池
type SysPoolLogData struct {
	ServerId   int32 `bson:"serverid" csv:"serverid"`
	UserId     int32 `bson:"userid" csv:"-"`
	FrameId    int64 `bson:"frameid" csv:"-"`
	SysMessage `bson:"message"`
	TableNo    int32     `bson:"tableno"`
	CreateTime time.Time `bson:"createtime"`
}

//系統水池
type SysMessage struct {
	Seq                          int64   `bson:"seq" csv:"Seq"`
	GroupName                    string  `bson:"groupname"`                                     //系統水池
	SumBullet                    int64   `bson:"sumbullet" csv:"魚場內子彈ID"`                       //魚場內子彈ID
	BoomAvgBet                   float32 `bson:"boomavgbet" csv:"魚場內玩家總平均押注"`                   //魚場內玩家總平均押注
	BoomAvgBetArea               float32 `bson:"boomavgbetarea" csv:"魚場水位線基準值"`                 //魚場水位線基準值
	CalAreaUp                    float32 `bson:"calareaup" csv:"淨分上限"`                          //淨分上限
	CalAreaDown                  float32 `bson:"calareadown" csv:"淨分下限"`                        //淨分下限
	ConditionOdds                int32   `bson:"conditionodds" csv:"淨分區間"`                      //淨分區間
	InitRate                     float32 `bson:"initrate" csv:"原子彈威力"`                          //原子彈威力
	SyspoolAdjustRate            float32 `bson:"syspooladjustrate" csv:"應加扣加威力"`                //應加扣加威力
	AfterSyspoolAdjustRate       float32 `bson:"aftersyspooladjustrate" csv:"更改後子彈威力"`          //更改後子彈威力
	SysSumWinLoss                int64   `bson:"syssumwinloss" csv:"系統總輸贏"`                     //系統總輸贏
	HsiTungHuoLiChouShui         int64   `bson:"hsitunghuolichoushui" csv:"系統獲利抽水後總輸贏"`         //系統獲利抽水後總輸贏
	HsiTungHuoLiBulletNumber     int32   `bson:"hsitunghuolibulletnumber" csv:"系統獲利每N發抽水"`      //系統獲利每N發抽水
	HsiTungHuoLiIRate            float32 `bson:"hsitunghuoliirate" csv:"系統獲利每次抽N%"`             //系統獲利每次抽N%
	HsiTungHuoLiChouShuiChingFen int64   `bson:"hsitunghuolichoushuichingfen" csv:"系統獲利當次抽水淨分"` //系統獲利當次抽水淨分
	Allrealusersumfish           float32 `bson:"allrealusersumfish" csv:"玩家總得分"`                //玩家總得分
	Allrealuserbetmoney          float32 `bson:"allrealuserbetmoney" csv:"玩家總押分"`               //玩家總押分
}

//個人水池
type PlayerPoolLogData struct {
	ServerId      int32 `bson:"serverid" csv:"serverid"`
	UserId        int32 `bson:"userid" csv:"-"`
	FrameId       int64 `bson:"frameid" csv:"-"`
	PlayerMessage `bson:"message"`
	TableNo       int32     `bson:"tableno"`
	CreateTime    time.Time `bson:"createtime"`
}
type PlayerMessage struct {
	Seq              int64   `bson:"seq" csv:"Seq"`
	GroupName        string  `bson:"groupname" csv:"GroupName"`
	FrameId          int64   `bson:"frameid" csv:"-"`
	UserId           int32   `bson:"userid" csv:"此玩家賬號ID"`              //此玩家賬號ID
	Bid              int32   `bson:"bid" csv:"此玩家子彈ID"`                 //此玩家子彈ID
	PlayerPoolStatus int     `bson:"playerpoolstatus" csv:"單一玩家超端狀態"`   //單一玩家超端狀態
	AttackRate       float32 `bson:"attackrate" csv:"單一玩家應加扣威力"`        //單一玩家應加扣威力
	AfterAttackRate  float32 `bson:"afterattackrate" csv:"單一玩家更改後子彈威力"` //單一玩家更改後子彈威力
	PlayerWinLoss    float32 `bson:"playerwinloss" csv:"單一玩家獲利區間"`      //單一玩家獲利區間
	HitMoney         float32 `bson:"hitmoney" csv:"單一玩家得分"`             //單一玩家得分
	SpendMoney       float32 `bson:"spendmoney" csv:"單一玩家押分"`           //單一玩家押分
	HitSpendScale    float32 `bson:"hitspendscale" csv:"單一玩家獲利比例"`      //單一玩家獲利比例
}

type UserLogData struct {
	ServerId   int32     `bson:"serverid" csv:"serverid"`
	UserId     int32     `bson:"userid" csv:"UserId"`
	FrameId    int64     `bson:"frameid" csv:"FrameId"`
	Message    string    `bson:"message"`
	TableNo    int32     `bson:"tableno"`
	CreateTime time.Time `bson:"createtime"`
}

func (m *MongoDatabaseQueue) FindSysPool(query interface{}) []SysPoolLogData {

	result := []SysPoolLogData{}
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

func (m *MongoDatabaseQueue) FindPlayerPool(query interface{}) []PlayerPoolLogData {

	result := []PlayerPoolLogData{}
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

func (m *MongoDatabaseQueue) FindUserDate(query interface{}) []UserLogData {
	result := []UserLogData{}
	//result := bson.M{}
	//iter := m.coll.exportCsv(query).Iter()
	//for iter.Next(&result) {
	//	m.writeCSVFile(result)
	//}

	sort := m.getSort()
	m.coll.Find(query).Sort(sort).Limit(UserCfg.QueryData.Limit).All(&result)

	return result
}

func (m *MongoDatabaseQueue) getSort() string {
	sort := ""
	if Config.MongoConn.Sort == 1 {
		sort = fmt.Sprintf("frameid")
	} else {
		sort = fmt.Sprintf("-frameid")
	}
	return sort
}

func (m *MongoDatabaseQueue) WriteCSVSysPool(result []SysPoolLogData, fileName string) {
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
	gocsv.MarshalString(&result)

	err = gocsv.MarshalFile(&result, clientsFile)
	// Use this to save the CSV back to the file
	if err != nil {
		mlog.Error("err %v", err.Error())
		panic(err)
	}
	mlog.System("Done Count[%v]", len(result))
}

func (m *MongoDatabaseQueue) WriteCSVPlayerPool(result []PlayerPoolLogData, fileName string) {
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
	gocsv.MarshalString(&result)

	err = gocsv.MarshalFile(&result, clientsFile)
	// Use this to save the CSV back to the file
	if err != nil {
		mlog.Error("err %v", err.Error())
		panic(err)
	}
	mlog.System("Done Count[%v]", len(result))
}

func (m *MongoDatabaseQueue) WriteCSVUserDataLog(result []UserLogData, fileName string) {
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
	gocsv.MarshalString(&result)

	err = gocsv.MarshalFile(&result, clientsFile)
	// Use this to save the CSV back to the file
	if err != nil {
		mlog.Error("err %v", err.Error())
		panic(err)
	}
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
