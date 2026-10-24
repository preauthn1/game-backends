package service

import (
	"testing"

	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
)

var Conn_DEV = "server=0.0.0.0;database=CenterDB;user id=gameuser;password=[REDACTED];port=1433;encrypt=disable;"
var Conn_DEALLOG_DEV = "server=0.0.0.0;database=PerGameLog;user id=gameuser;password=[REDACTED];port=1433;encrypt=disable;"
var Conn_CLUB_DEV = "server=0.0.0.0;database=Club;user id=gameuser;password=[REDACTED];port=1433;encrypt=disable;"
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
