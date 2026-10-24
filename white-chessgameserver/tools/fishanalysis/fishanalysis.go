package main

import (
	"fishanalysis"
	"gopkg.in/mgo.v2/bson"
	"mlog"
)

func main() {
	db := fishanalysis.NewMongoDB()

	groupName := "syspool_all"
	findGroup(groupName, db)

	groupName = "personal_kill"
	findGroup(groupName, db)

	groupName = "userquery"
	findGroup(groupName, db)

}

func findGroup(groupName string, db *fishanalysis.MongoDatabaseQueue) {
	findData := bson.M{"message.groupname": groupName}
	mlog.System("[weihao] start %v", groupName)

	switch groupName {
	case "syspool_all":
		result := db.FindSysPool(findData)
		db.WriteCSVSysPool(result, groupName)
	case "personal_kill":
		//findData = bson.M{"userid":8784407}
		result := db.FindPlayerPool(findData)
		db.WriteCSVPlayerPool(result, groupName)
	case "userquery":
		findData := bson.M{"userid": fishanalysis.UserCfg.QueryData.UserId}
		result := db.FindUserDate(findData)
		db.WriteCSVUserDataLog(result, groupName)
	}

}
