package nodemonitor

import (
	"db"
)
import (
	_ "example.invalid/mattn/go-adodb"
)

//var database *db.Database
//var DbUtil db.DatabaseEngine
var NodeDBUtil *db.Mssql2000

func InitDatabase() {
	//dao := new(db.Database)
	//dao.Connstring = ConnString
	//dao.Connpool = db.NewConnPool(PoolSize, ConnString)
	//database = db.NewDatabase(Config.GameDB.ConnString, int(Config.GameDB.Pool))
	//DbUtil = db.NewDatabaseEngine(database)
	NodeDBUtil = db.NewMssql2000(Config.NodeDB.Host, Config.NodeDB.Port, Config.NodeDB.User, Config.NodeDB.Password, Config.NodeDB.Database)
}
