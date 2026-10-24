package main

import (
	"gopkg.in/mgo.v2/bson"
	"zjhAnalysis"
)

func main() {
	db := zjhAnalysis.NewMongoDB()

	groupName := "NaturalOdds"
	findGroup(groupName, db)
}

func findGroup(groupName string, db *zjhAnalysis.MongoDatabaseQueue) {
	findData := bson.M{"message.groupname": groupName}

	switch groupName {
	case "NaturalOdds":
		result := db.FindOddsData(findData)
		db.WriteCSVOddsData(result, groupName)
	}

}
