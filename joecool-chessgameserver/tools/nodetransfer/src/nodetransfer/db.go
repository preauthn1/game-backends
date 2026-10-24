package nodetransfer

import (
	"db"
)
import (
	_ "example.invalid/mattn/go-adodb"
)

var NodeDBUtil *db.Mssql2000

func InitDatabase() {
	NodeDBUtil = db.NewMssql2000(Config.NodeDB.Host, Config.NodeDB.Port, Config.NodeDB.User, Config.NodeDB.Password, Config.NodeDB.Database)
}
