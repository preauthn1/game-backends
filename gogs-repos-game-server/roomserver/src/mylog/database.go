package mylog

import (
	"database/sql"
	"encoding/json"
	_ "example.invalid/go-sql-driver/mysql"
	"gopkg.in/mgo.v2"
	"mlog"
	"time"
	"util"
)

var MyDBQueue IDatabaseQueue
var ServerId int

type IDatabaseQueue interface {
	addQueue(s *MyLogStatement)
	Start()
	writeDB(stmts []*MyLogStatement)
	getCodeStack() interface{}
	query(in interface{}) interface{}
}

func InitDatabaseQueue(connString string , connPool int, perWriteCount int, serverId int, log *mlog.DefaultLogger) {
	ServerId = serverId
	if connString != "" {
		MyDBQueue = NewDatabaseQueue(connString, connPool, perWriteCount, log)
	} else {
		MyDBQueue = NewEmptyDatabaseQueue()
	}
	MyDBQueue.Start()
}

func InitMongoDatabaseQueue(connString string , connPool int, perWriteCount int, serverId int, log *mlog.DefaultLogger, database string, collection string) {
	ServerId = serverId
	if connString != "" && database != "" && collection != "" {
		MyDBQueue = NewMongoDatabaseQueue(connString, connPool, perWriteCount, log, database, collection)
	}else {
		MyDBQueue = NewEmptyDatabaseQueue()
	}
	MyDBQueue.Start()
}

type databaseQueue struct {
	db *sql.DB
	queue chan *MyLogStatement
	perWriteCount int
	log *mlog.DefaultLogger
}

type mongoDatabaseQueue struct {
	db *mgo.Database
	coll *mgo.Collection
	queue chan *MyLogStatement
	perWriteCount int
	log *mlog.DefaultLogger
}

func NewDatabaseQueue(connString string , connPool int, perWriteCount int, log *mlog.DefaultLogger) *databaseQueue {
	db, _ := sql.Open("mysql", connString)
	db.SetMaxOpenConns(connPool)
	db.SetMaxIdleConns((connPool / 2) + 1)
	q := new(databaseQueue)
	q.db = db
	q.queue = make(chan *MyLogStatement, 10000)
	q.perWriteCount = perWriteCount
	q.log = log
	return q
}

func NewMongoDatabaseQueue(connString string , connPool int, perWriteCount int, log *mlog.DefaultLogger, database string, collection string) *mongoDatabaseQueue {
	uri := "mongodb://" + connString
	session, err := mgo.Dial(uri)
	if err != nil {
		mlog.Error("MongoDB Error : %v", err.Error())
	}

	db := session.DB(database)
	coll := db.C(collection)

	q := new(mongoDatabaseQueue)
	q.db = db
	q.coll = coll
	q.queue = make(chan *MyLogStatement, 10000)
	q.perWriteCount = perWriteCount
	q.log = log
	return q
}

func(q *databaseQueue) addQueue(s *MyLogStatement) {

	//code stack 程式func呼叫堆疊來源
	dataStr, _ := json.Marshal(q.log.GetCallStack())

	s.args = append(s.args, dataStr)
	q.queue <- s
	q.log.Reset()
}

func(q *databaseQueue) Start() {
	go func() {
		defer util.PanicErrStack()
		for {
			stmts := make([]*MyLogStatement, 0)
		FORLOOP:
			for i:=0;i<q.perWriteCount;i++ {

				select {
				case mystmt := <-q.queue:
					stmts = append(stmts, mystmt)
				default:
					break FORLOOP
				}
			}
			go func(){
				if len(stmts)>0 {
					q.writeDB(stmts)
				}
			}()
			if len(stmts) >= q.perWriteCount {
				mlog.Error("bylog 每秒己經超過 %d 條, 還有 %d 條", q.perWriteCount, len(q.queue))
				continue
			}
			time.Sleep(1 * time.Second)
		}
	}()
}

func(q *databaseQueue) writeDB(stmts []*MyLogStatement) {
	tx, err := q.db.Begin()
	if err != nil {
		mlog.Error("mylog error %s", err)
		return
	}

	for _, stmt := range stmts {
		tx.Exec(stmt.sql, stmt.args...)
	}
	tx.Commit()
}

func(q *databaseQueue) getCodeStack() interface{} {
	return q.log.GetCallStack()
}
func(q *databaseQueue) query(in interface{}) interface{} {
	return nil
}

// empty databasequeue
type emptyDatabaseQueue struct {}
func(q *emptyDatabaseQueue) addQueue(s *MyLogStatement) {}
func(q *emptyDatabaseQueue) Start() {}
func(q *emptyDatabaseQueue) writeDB(stmts []*MyLogStatement) {}
func(q *emptyDatabaseQueue) getCodeStack() interface{} {
	return nil
}
func(q *emptyDatabaseQueue) query(in interface{}) interface{} {
	return nil
}
func NewEmptyDatabaseQueue() *emptyDatabaseQueue { return new(emptyDatabaseQueue) }

//MongoDB
func (q *mongoDatabaseQueue) addQueue(s *MyLogStatement) {
	//code stack 程式func呼叫堆疊來源
	dataStr := q.log.GetCallStack()
	s.args = append(s.args, dataStr)
	q.queue <- s
	q.log.Reset()
}
func (q *mongoDatabaseQueue) getCodeStack() interface{}{
	cloneLog := new(mlog.DefaultLogger)
	cloneLog.StdLog = q.log.StdLog
	cloneLog.CallDepLen = q.log.CallDepLen
	cloneLog.CallDepStart = q.log.CallDepStart
	return cloneLog.GetCallStack()
}

func (q *mongoDatabaseQueue) Start(){
	go func() {
		defer util.PanicErrStack()
		for {
			stmts := make([]*MyLogStatement, 0)
		FORLOOP:
			for i:=0;i<q.perWriteCount;i++ {

				select {
				case mystmt := <-q.queue:
					stmts = append(stmts, mystmt)
				default:
					break FORLOOP
				}
			}
			go func(){
				if len(stmts)>0 && q.coll.Database.Session != nil {
					q.writeDB(stmts)
				}
			}()
			if len(stmts) >= q.perWriteCount {
				mlog.Error("bylog 每秒己經超過 %d 條, 還有 %d 條", q.perWriteCount, len(q.queue))
				continue
			}
			time.Sleep(1 * time.Second)
		}
	}()
}
func (q *mongoDatabaseQueue) writeDB(stmts []*MyLogStatement){
	defer util.PanicErrStack()
	//mlog.System(ConnString)
	c := q.coll

	for _, stmt := range stmts {
		err := c.Insert(stmt.monglogargs)
		if err != nil {
			c.Database.Session.Refresh()
			panic(err)
		}
	}
}

func (q *mongoDatabaseQueue) query(query interface{}) interface{} {
	result := QueryMessage{}
	c := q.coll
	c.Find(query).Sort("-frameid").One(&result)
	return result
}

type QueryMessage struct {
	ServerId   int32 `bson:"serverid"`
	UserId     int32 `bson:"userid"`
	FrameId    int64 `bson:"frameid"`
	Msg `bson:"message"`
	TableNo    int32     `bson:"tableno"`
	CreateTime time.Time `bson:"createtime"`
}
type Msg struct {
	Seq int64 `bson:"seq"`
}