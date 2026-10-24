package serverdefense

import (
	"fmt"
	"mlog"
	"msg"
	"network"
	"sync"
	"time"
	"util"
)

//cc防护
type CCDefense struct {
	*DefenserBase
	ipConnNumLimit    int            //同IP连接限制
	connectTimesLimit *DefenserLimit //连接次数
	sendDataLenLimit  *DefenserLimit //发送数据包总长度
	sendMsgLimit      *DefenserLimit //发送消息次数

	connTimesDefData   map[string]*DefenserData //连接次数防护数据
	sendDataLenDefData map[string]*DefenserData //发送数据大小防护数据
	sendMsgDefData     map[string]*DefenserData //发送消息数量防护数据

	connTimesLocker   *sync.RWMutex //连接次数防护锁
	sendDataLenLocker *sync.RWMutex //发送数据包长度防护锁
	sendMsgLocker     *sync.RWMutex //发送消息防护数锁
}

//创建cc防护对象
func NewCCDefense(ipConnNumLimit int, connTimesLimit *DefenserLimit, sendDataLenLimit *DefenserLimit, sendMsgLimit *DefenserLimit) *CCDefense {
	ccdef := new(CCDefense)
	ccdef.connectTimesLimit = connTimesLimit
	ccdef.sendDataLenLimit = sendDataLenLimit
	ccdef.sendMsgLimit = sendMsgLimit
	ccdef.ipConnNumLimit = ipConnNumLimit

	ccdef.connTimesDefData = make(map[string]*DefenserData)
	ccdef.sendDataLenDefData = make(map[string]*DefenserData)
	ccdef.sendMsgDefData = make(map[string]*DefenserData)

	ccdef.connTimesLocker = &sync.RWMutex{}
	ccdef.sendDataLenLocker = &sync.RWMutex{}
	ccdef.sendMsgLocker = &sync.RWMutex{}

	ccdef.DefenserBase = NewDefenserBase()

	//5秒数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(5) * time.Second)

			//连接次数
			ccdef.connTimesLocker.Lock()
			for _, d := range ccdef.connTimesDefData {
				d.ClearSecData_5()
			}
			ccdef.connTimesLocker.Unlock()

			//数据包长度
			ccdef.sendDataLenLocker.Lock()
			for _, d := range ccdef.sendDataLenDefData {
				d.ClearSecData_5()
			}
			ccdef.sendDataLenLocker.Unlock()

			//消息数
			ccdef.sendMsgLocker.Lock()
			for _, d := range ccdef.sendMsgDefData {
				d.ClearSecData_5()
			}
			ccdef.sendMsgLocker.Unlock()

		}
	}()

	//10秒数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(10) * time.Second)

			//连接次数
			ccdef.connTimesLocker.Lock()
			for _, d := range ccdef.connTimesDefData {
				d.ClearSecData_10()
			}
			ccdef.connTimesLocker.Unlock()

			//数据包长度
			ccdef.sendDataLenLocker.Lock()
			for _, d := range ccdef.sendDataLenDefData {
				d.ClearSecData_10()
			}
			ccdef.sendDataLenLocker.Unlock()

			//消息数
			ccdef.sendMsgLocker.Lock()
			for _, d := range ccdef.sendMsgDefData {
				d.ClearSecData_10()
			}
			ccdef.sendMsgLocker.Unlock()
		}

	}()

	//30秒数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(30) * time.Second)

			//连接次数
			ccdef.connTimesLocker.Lock()
			for _, d := range ccdef.connTimesDefData {
				d.ClearSecData_30()
			}
			ccdef.connTimesLocker.Unlock()

			//数据包长度
			ccdef.sendDataLenLocker.Lock()
			for _, d := range ccdef.sendDataLenDefData {
				d.ClearSecData_30()
			}
			ccdef.sendDataLenLocker.Unlock()

			//消息数
			ccdef.sendMsgLocker.Lock()
			for _, d := range ccdef.sendMsgDefData {
				d.ClearSecData_30()
			}
			ccdef.sendMsgLocker.Unlock()
		}

	}()

	//60秒数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(60) * time.Second)

			//连接次数
			ccdef.connTimesLocker.Lock()
			for _, d := range ccdef.connTimesDefData {
				d.ClearSecData_60()
			}
			ccdef.connTimesLocker.Unlock()

			//数据包长度
			ccdef.sendDataLenLocker.Lock()
			for _, d := range ccdef.sendDataLenDefData {
				d.ClearSecData_60()
			}
			ccdef.sendDataLenLocker.Unlock()

			//消息数
			ccdef.sendMsgLocker.Lock()
			for _, d := range ccdef.sendMsgDefData {
				d.ClearSecData_60()
			}
			ccdef.sendMsgLocker.Unlock()
		}

	}()

	//300秒数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(300) * time.Second)

			//连接次数
			ccdef.connTimesLocker.Lock()
			for _, d := range ccdef.connTimesDefData {
				d.ClearSecData_300()
			}
			ccdef.connTimesLocker.Unlock()

			//数据包长度
			ccdef.sendDataLenLocker.Lock()
			for _, d := range ccdef.sendDataLenDefData {
				d.ClearSecData_300()
			}
			ccdef.sendDataLenLocker.Unlock()

			//消息数
			ccdef.sendMsgLocker.Lock()
			for _, d := range ccdef.sendMsgDefData {
				d.ClearSecData_300()
			}
			ccdef.sendMsgLocker.Unlock()
		}

	}()

	//15分钟,清理所有数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(60*15) * time.Second)

			//连接次数
			ccdef.connTimesLocker.Lock()
			ccdef.connTimesDefData = make(map[string]*DefenserData)
			ccdef.connTimesLocker.Unlock()

			//数据包长度
			ccdef.sendDataLenLocker.Lock()
			ccdef.sendDataLenDefData = make(map[string]*DefenserData)
			ccdef.sendDataLenLocker.Unlock()

			//消息数
			ccdef.sendMsgLocker.Lock()
			ccdef.sendMsgDefData = make(map[string]*DefenserData)
			ccdef.sendMsgLocker.Unlock()
		}
	}()

	return ccdef
}

func (d *CCDefense) OnClientConnect(clt *network.ConnInfo) {
	ip := clt.IPAddress
	cn := d.GetConnNumByIP(ip)

	//ip白名单
	if d.checkWhiteIPList(ip) {
		return
	}

	//如果IP连接数超出上线
	if cn >= d.ipConnNumLimit {
		d.CloseConn(clt)

		example.invalid("IP%s并发连接数达到%d,加入黑名单", ip, cn)
		//封锁IP
		d.BlackIPList.BlockIP(ip, d.BlockIPTime, fmt.Sprintf("并发连接数达到%d", cn))

		return
	}

	var dd *DefenserData

	d.connTimesLocker.Lock()
	//获取连接次数计数
	if defdata, ok := d.connTimesDefData[ip]; ok {
		dd = defdata
	} else {
		dd = NewDefenserData()
		d.connTimesDefData[ip] = dd
	}
	d.connTimesLocker.Unlock()

	//如果连接次数超出设定
	if dd.CheckLimit(d.connectTimesLimit) {
		d.CloseConn(clt)

		example.invalid("ip %s连接频率超出设定,加入黑名单.%s", ip, dd)
		d.BlackIPList.BlockIP(ip, d.BlockIPTime, fmt.Sprintf("连接次数超出设定,%s", dd))
	} else {
		//增加连接计数
		dd.Increase(1)
	}
}

func (d *CCDefense) OnClientDisconnect(clt *network.ConnInfo) {

}

func (d *CCDefense) OnClientData(data *network.ConnData) {
	//ip白名单
	if d.checkWhiteIPList(data.Conn.IPAddress) {
		return
	}

	var dd *DefenserData

	connsign := data.Conn.IPAddress

	d.sendDataLenLocker.Lock()
	//获取发送数据包大小计数
	if defdata, ok := d.sendDataLenDefData[connsign]; ok {
		dd = defdata
	} else {
		dd = NewDefenserData()
		d.sendDataLenDefData[connsign] = dd
	}
	d.sendDataLenLocker.Unlock()

	//增加计数
	if data.Data != nil {
		dd.Increase(len(data.Data))
	}

	if dd.CheckLimit(d.sendDataLenLimit) {
		d.CloseConn(data.Conn)

		ip := data.Conn.IPAddress

		example.invalid("ip%s发送数据包长度超出设定,加入黑名单.%s", ip, dd)
		d.BlackIPList.BlockIP(ip, d.BlockIPTime, fmt.Sprintf("单位时间内,发送数据包大小超出设定,%s", dd))
	}
}

func (d *CCDefense) OnClientMessage(clt *network.ConnInfo, msg *msg.Message) {
	//ip白名单
	if d.checkWhiteIPList(clt.IPAddress) {
		return
	}

	var dd *DefenserData

	connsign := clt.IPAddress

	d.sendMsgLocker.Lock()
	//获取发送消息
	if defdata, ok := d.sendMsgDefData[connsign]; ok {
		dd = defdata
	} else {
		dd = NewDefenserData()
		d.sendMsgDefData[connsign] = dd
	}
	d.sendMsgLocker.Unlock()

	//增加消息数量计数
	dd.Increase(1)

	if dd.CheckLimit(d.sendMsgLimit) {
		d.CloseConn(clt)

		ip := clt.IPAddress

		example.invalid("ip%s发送消息频率超出设定,加入黑名单%s", ip, dd)
		d.BlackIPList.BlockIP(ip, d.BlockIPTime, fmt.Sprintf("单位时间内,发送消息数量超出设定,%s", dd))
	}
}

//解析连接次数防护数据
func (d *CCDefense) ParseConnTimesLimit(str string) {
	lmt := d.parseDefenseLimit(str)
	if lmt == nil {
		mlog.Error("解析连接频率防护数据错误,%s", str)
		return
	}

	d.connectTimesLimit = lmt
}

//解析发送数据包长度防护数据
func (d *CCDefense) ParseSendDataLimit(str string) {
	lmt := d.parseDefenseLimit(str)
	if lmt == nil {
		mlog.Error("解析发送数据包长度防护数据错误,%s", str)
		return
	}

	d.sendDataLenLimit = lmt
}

//解析发送消息防护数据
func (d *CCDefense) ParseSendMsgLimit(str string) {
	lmt := d.parseDefenseLimit(str)
	if lmt == nil {
		mlog.Error("解析发送协议频率防护数据错误,%s", str)
		return
	}

	d.sendMsgLimit = lmt
}
