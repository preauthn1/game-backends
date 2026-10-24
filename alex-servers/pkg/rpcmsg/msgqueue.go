package rpcmsg

import (
	"fmt"
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	MsgSendedWaitReturn = iota //已发送等待返回结果
	MsgDroped                  //已丢弃，超时未收到返回结果
	MsgSendSuccess             //发送并处理成功
)

//队列消息信息
type QueuedMsgInfo struct {
	Message      *msg.Message //消息内容
	MsgID        int          //队列ID
	PushTime     time.Time    //加入时间
	RetryTimes   int          //重试次数
	LastSendTime time.Time    //最后一次发送时间
	Status       int          //状态
	Safe         bool         //是否安全消息，即永不丢弃
}

//队列消息字符串描述
func (msg *QueuedMsgInfo) String() string {
	statusdesc := ""
	if msg.Status == MsgSendedWaitReturn {
		statusdesc = "已发送"
	} else if msg.Status == MsgDroped {
		statusdesc = "已丢弃"
	} else if msg.Status == MsgSendSuccess {
		statusdesc = "发送成功"
	}

	return fmt.Sprintf("protoid:%s,msgid:%d,pushtime:%s,retrytimes:%d,lastsendtime:%s,status:%s,safe:%s", msg.Message, msg.MsgID, msg.PushTime, msg.RetryTimes, msg.LastSendTime, statusdesc, msg.Safe)
}

//消息队列
type MsgQueue struct {
	maxSize          int                    //最大消息长度
	innerWaitMsgList map[int]*QueuedMsgInfo //等待处理消息
	OnMessageSig     chan *QueuedMsgInfo    //消息通道
	waitmsgLocker    *sync.RWMutex          //等待处理消息锁
	currentMsgID     int                    //当前消息ID
	stopSig          chan bool              //停止信号
	retryInterval    int                    //重发时间间隔
	maxRetryTimes    int                    //最大重发次数
}

//创建消息队列
func NewMsgQueue(maxSize int, throttle int, retryInterval int, maxRetryTimes int) *MsgQueue {
	mq := new(MsgQueue)
	mq.maxSize = maxSize
	mq.currentMsgID = 0
	mq.waitmsgLocker = &sync.RWMutex{}
	mq.OnMessageSig = make(chan *QueuedMsgInfo, throttle)
	mq.stopSig = make(chan bool, 1)
	mq.retryInterval = retryInterval
	mq.maxRetryTimes = maxRetryTimes
	mq.innerWaitMsgList = make(map[int]*QueuedMsgInfo)

	return mq
}

//启动
func (mq *MsgQueue) Start() {
	go func() {
		defer util.PanicErrStack()

		for {

			//停止信号
			select {
			case <-mq.stopSig:
				break
			default:

			}

			time.Sleep(time.Second * time.Duration(31))

			//需要丢弃的消息
			dealmsg := make([]int, 0)
			//需要再次发送的消息
			msgtosend := make([]*QueuedMsgInfo, 0)

			mq.waitmsgLocker.Lock()

			for _, msg := range mq.innerWaitMsgList {
				if msg.RetryTimes >= mq.maxRetryTimes && !msg.Safe {
					//丢弃消息
					mlog.Error("消息%v超出最大重试次数,丢弃", msg)

					//					go func() {
					//						mq.DealMsg(msg.MsgID, MsgDroped)
					//					}()

					dealmsg = append(dealmsg, msg.MsgID)

					continue
				}

				if int(time.Now().Sub(msg.LastSendTime).Seconds()) < mq.retryInterval {
					continue
				}

				msg.LastSendTime = time.Now()
				msg.RetryTimes++

				msgtosend = append(msgtosend, msg)
			}
			mq.waitmsgLocker.Unlock()

			//处理丢弃消息
			for _, msgid := range dealmsg {
				mq.DealMsg(msgid, MsgDroped)
			}
			//处理需要再次发送的消息
			for _, m := range msgtosend {
				mq.OnMessageSig <- m
			}

		}

		mlog.Warn("消息队列退出...")
	}()
}

//停止
func (mq *MsgQueue) Stop() {
	mq.stopSig <- true
}

//队列数量
func (mq *MsgQueue) Size() int {
	mq.waitmsgLocker.RLock()
	defer mq.waitmsgLocker.RUnlock()

	return len(mq.innerWaitMsgList)
}

//添加消息
func (mq *MsgQueue) PushMsg(msg *msg.Message, safe bool) {
	mq.waitmsgLocker.Lock()
	if len(mq.innerWaitMsgList) >= mq.maxSize {
		mq.waitmsgLocker.Unlock()
		mlog.Error("排队消息超出最大长度")
		return
	}

	//消息ID
	msgid := mq.currentMsgID + 1
	mq.currentMsgID++

	//队列信息
	msgqinfo := new(QueuedMsgInfo)
	msgqinfo.MsgID = msgid
	msgqinfo.PushTime = time.Now()
	msgqinfo.Message = msg
	msgqinfo.RetryTimes = 1
	msgqinfo.LastSendTime = time.Now()
	msgqinfo.Status = MsgSendedWaitReturn
	msgqinfo.Safe = safe

	//重试机制,进入队列
	if mq.maxRetryTimes > 0 || safe {
		//加入排队队列
		mq.innerWaitMsgList[msgid] = msgqinfo
	}
	mq.waitmsgLocker.Unlock()

	//通知队列消息
	mq.OnMessageSig <- msgqinfo

}

//处理消息
func (mq *MsgQueue) DealMsg(msgid int, status int) {
	mq.waitmsgLocker.Lock()
	delete(mq.innerWaitMsgList, msgid)
	mq.waitmsgLocker.Unlock()
}

//查找消息
func (mq *MsgQueue) Find(msgid int) int {
	mq.waitmsgLocker.RLock()
	defer mq.waitmsgLocker.RUnlock()

	if _, ok := mq.innerWaitMsgList[msgid]; ok {
		return MsgSendedWaitReturn
	}

	return MsgSendSuccess
}
