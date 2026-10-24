package fixtures

import (
	"path"
	"runtime"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/mylog"
)

func MongoInitByTest() {
	_, filename, _, _ := runtime.Caller(1)
	mlog.System("[weihao] filename[%v]", filename)
	configDir := path.Dir(filename) + "/../../config"
	mlog.System("[weihao] configdir[%v]", configDir)
	cfgs := new(config.ConfigGroup)
	cfgs.LoadConfig(configDir, 0)
	serverInfo := cfgs.GetConfig(gamelogic.CONFIGFILENAME_SERVERINFO)
	ConnString := serverInfo.GetConfigValue("mongoconn", "connstring")
	database := serverInfo.GetConfigValue("mongoconn", "database")
	collection := serverInfo.GetConfigValue("mongoconn", "collection")
	InitMongoDatabaseQueue(ConnString, 100, 100, 0, mlog.NewDefaultLogger(), database, collection)
}

func InitMongoDatabaseQueue(connString string, connPool int, perWriteCount int, serverId int, log *mlog.DefaultLogger, database string, collection string) {
	mylog.ServerId = serverId
	if connString != "" && database != "" && collection != "" {
		mylog.MyDBQueue = mylog.NewMongoDatabaseQueue(connString, connPool, perWriteCount, log, database, collection)
	} else {
		mylog.MyDBQueue = mylog.NewEmptyDatabaseQueue()
	}
	mylog.MyDBQueue.Start()
}
