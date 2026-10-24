package service

import (
	"testing"

	dbutil "example.invalid/ServerTeam/Servers/pkg/db/dbutil"
)

var Conn_DEV = "server=0.0.0.0;database=CenterDB;user id=sa;password=[REDACTED];port=1433;encrypt=disable;"           //中心库数据库连接字符串
var Conn_DEALLOG_DEV = "server=0.0.0.0;database=PerGameLog;user id=sa;password=[REDACTED];port=1433;encrypt=disable;" //牌局记录库数据库连接字符串
var Conn_CLUB_DEV = "server=0.0.0.0;database=Club;user id=sa;password=[REDACTED];port=1433;encrypt=disable;"          //Club数据库连接字符串
var SmsWebAPI_DEV = "0.0.0.0:80/SMS/Send"
var Semaphore = 5
var Conn_POOL = 200

func TestMain(m *testing.M) {

	dbutil.ConnString = Conn_DEV
	dbutil.DealLogConnString = Conn_DEALLOG_DEV
	dbutil.ClubConnString = Conn_CLUB_DEV

	dbutil.SMSWebAPI = SmsWebAPI_DEV
	dbutil.PoolSize = Conn_POOL

	dbutil.InitDatabase()

	m.Run()
}
