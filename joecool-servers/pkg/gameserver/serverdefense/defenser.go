package serverdefense

import (
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverdefense/blackip"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

//客户端连接上下文
type ConnListProvider interface {
	RangeConn(f func(conn *network.ConnInfo))
}

//服务器客户端连接遍历包装类
type ServerConnListProvider struct {
	msgsvr *msgserver.MsgServer
}

//创建服务器连接容器对象
func NewServerConnListProvider(msgsvr *msgserver.MsgServer) *ServerConnListProvider {
	connprovider := new(ServerConnListProvider)
	connprovider.msgsvr = msgsvr

	return connprovider
}

func (svrconnlst *ServerConnListProvider) RangeConn(f func(conn *network.ConnInfo)) {
	svrconnlst.msgsvr.RangeConn(f)
}

//防护接口
type Defenser interface {
	msgserver.MsgServerLogicHandler

	//设置黑名单列表容器
	SetIPListProvider(iplist blackip.BlackIPListProvider)
	//设置客户端连接容器
	SetConnProvider(connprovider ConnListProvider)
}

//防护基类
type DefenserBase struct {
	BlackIPList   blackip.BlackIPListProvider //ip黑名单
	ConnProvider  ConnListProvider            //客户端连接容器
	CloseConnFunc func(clt *network.ConnInfo) //断开客户端连接
	BlockIPTime   int                         //封锁时间
	WhiteIPList   string                      //ip白名单
}

func NewDefenserBase() *DefenserBase {
	dd := new(DefenserBase)
	dd.BlockIPTime = 15 * 60

	return dd
}

func (db *DefenserBase) SetIPListProvider(iplist blackip.BlackIPListProvider) {
	db.BlackIPList = iplist
}

func (db *DefenserBase) SetConnProvider(connprovider ConnListProvider) {
	db.ConnProvider = connprovider
}

func (db *DefenserBase) CloseClientFunc(f func(clt *network.ConnInfo)) {
	db.CloseConnFunc = f
}

//踢出用户
func (db *DefenserBase) CloseConn(clt *network.ConnInfo) {
	db.CloseConnFunc(clt)
}

//根据IP汇总连接数
func (db *DefenserBase) GetConnNumByIP(ip string) int {
	num := 0

	db.ConnProvider.RangeConn(func(conn *network.ConnInfo) {
		if conn.IPAddress == ip {
			num = num + 1
		}
	})

	return num
}

func (dd *DefenserBase) parseDefenseLimit(limit string) *DefenserLimit {
	arr := strings.Split(limit, ",")

	if len(arr) != 5 {
		mlog.Error("解析防护配置错误,%s", limit)
		return nil
	}

	seclimit_5, err := strconv.Atoi(arr[0])

	if err != nil {
		mlog.Error("解析防护配置错误,%s", limit)
		return nil
	}

	seclimit_10, err := strconv.Atoi(arr[1])
	if err != nil {
		mlog.Error("解析防护配置错误,%s", limit)
		return nil
	}

	seclimit_30, err := strconv.Atoi(arr[2])
	if err != nil {
		mlog.Error("解析防护配置错误,%s", limit)
		return nil
	}

	seclimit_60, err := strconv.Atoi(arr[3])
	if err != nil {
		mlog.Error("解析防护配置错误,%s", limit)
		return nil
	}

	seclimit_300, err := strconv.Atoi(arr[4])
	if err != nil {
		mlog.Error("解析防护配置错误,%s", limit)
		return nil
	}

	return NewDefenserLimit(seclimit_5, seclimit_10, seclimit_30, seclimit_60, seclimit_300)
}

func (dd *DefenserBase) checkWhiteIPList(ip string) bool {
	return strings.Index(dd.WhiteIPList, ip+",") > 0
}
