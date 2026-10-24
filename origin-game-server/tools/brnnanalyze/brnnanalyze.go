package main

import (
	"brnnanalyze"
	"gopkg.in/mgo.v2/bson"
)

func main() {
	db := brnnanalyze.NewMongoDB()

	groupName := "syspool_all"
	findGroup(groupName, db)
}

func findGroup(groupName string, db *brnnanalyze.MongoDatabaseQueue) {
	findData := bson.M{"message.groupname": groupName}

	switch groupName {
	case "syspool_all":
		result := db.FindSysPool(findData)
		db.WriteCSVSysPool(result, groupName)
	}

}
