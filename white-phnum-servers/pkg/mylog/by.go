package mylog

import "time"

type MyLogStatement struct {
	sql         string
	args        []interface{}
	monglogargs MongoLogData
}

type MongoLogData struct {
	ServerId   int         `bson:"serverid"`
	UserID     int32       `bson:"userid"`
	FrameId    int64       `bson:"frameid"`
	Message    interface{} `bson:"message"`
	TableNo    int32       `bson:"tableno"`
	CodeStack  interface{} `bson:"codestack"`
	CreateTime time.Time   `bson:"createtime"`
}

func MongoLog(userid int32, frameid int64, tableno int32, message interface{}) {
	//sql := "INSERT INTO bylog (serverid, userid, frameid, message, tableno, codestack) VALUES (?, ?, ?, ?, ?, ?)"

	logData := MongoLogData{
		ServerId:   ServerId,
		UserID:     userid,
		FrameId:    frameid,
		Message:    message,
		TableNo:    tableno,
		CodeStack:  MyDBQueue.getCodeStack(),
		CreateTime: time.Now(),
	}

	query := &MyLogStatement{monglogargs: logData}

	MyDBQueue.addQueue(query)
}

func AddLog(userid int32, tableno int32, message interface{}) {
	logData := MongoLogData{
		ServerId:   ServerId,
		UserID:     userid,
		Message:    message,
		TableNo:    tableno,
		CodeStack:  MyDBQueue.getCodeStack(),
		CreateTime: time.Now(),
	}

	query := &MyLogStatement{monglogargs: logData}

	MyDBQueue.addQueue(query)
}

func BYQuery(query interface{}) interface{} {
	return MyDBQueue.query(query)
}
