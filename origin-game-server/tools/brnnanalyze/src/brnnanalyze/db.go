package brnnanalyze

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
	ServerId   int32 `bson:"serverid" csv:"serverid"` //伺服器編號
	FrameId    int64 `bson:"frameid" csv:"-"`
	SysMessage `bson:"message"`
	TableNo    int32     `bson:"tableno"`
	CreateTime time.Time `bson:"createtime"`
}

//系統水池
type SysMessage struct {
	GroupName      string `bson:"groupname"`                   //系統水池
	StartSyspool   int64  `bson:"startsyspool" csv:"原始水池值"`    //原始水池值
	ChangeSysValue int64  `bson:"changesysvalue" csv:"異動水池值"`  //異動水池值
	SyspoolNow     int64  `bson:"syspoolnow" csv:"更新後水池值"`     //更新後水池值
	JiangType      int32  `bson:"jiangtype" csv:"開獎表名稱"`       //開獎表名稱
	JiangWeight    int32  `bson:"jiangweight" csv:"贏分類型(通殺賠)"` //贏分類型(通殺賠)
	ZhuangPoint    int32  `bson:"zhuangpoint" csv:"莊家牌點"`      //莊家牌點
	Xian1Point     int32  `bson:"xian1point" csv:"閒家1牌點"`      //閒家1牌點
	Xian2Point     int32  `bson:"xian2point" csv:"閒家2牌點"`      //閒家2牌點
	Xian3Point     int32  `bson:"xian3point" csv:"閒家3牌點"`      //閒家3牌點
	Xian4Point     int32  `bson:"xian4point" csv:"閒家4牌點"`      //閒家4牌點
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

func (m *MongoDatabaseQueue) getSort() string {
	sort := ""
	if Config.MongoConn.Sort == 1 {
		sort = fmt.Sprintf("createtime")
	} else {
		sort = fmt.Sprintf("-createtime")
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

func Exists(name string) bool {
	if _, err := os.Stat(name); err != nil {
		if os.IsNotExist(err) {
			return false
		}
	}
	return true
}
