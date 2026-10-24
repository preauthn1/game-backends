package msg

import (
	"fmt"
	"testing"
	"time"
)

func TestLogger(t *testing.T) {
	mq := NewMsgQueue(10, 10, 10, 3)
	mq.Start()

	go func() {
		for {
			msg := <-mq.OnMessageSig

			fmt.Println("recv msg...", msg.Message.MsgData)
		}
	}()

	for i := 0; i < 2; i++ {
		time.Sleep(time.Duration(3) * time.Second)
		msg := NewMessage(0, 0, fmt.Sprintf("abc_%d", i))
		qm, err := mq.PushMsg(msg)

		if err != nil {
			fmt.Println(err)
		} else {
			fmt.Println(fmt.Sprintf("msg %d queued!", qm.MsgID))

			//			go func() {
			//				time.Sleep(time.Second * time.Duration(1))
			//				mq.DealMsg(qm.MsgID)
			//				fmt.Println(fmt.Sprintf("msg %d dealed!", qm.MsgID))
			//			}()
		}
	}

	time.Sleep(time.Second * time.Duration(100))
	fmt.Println(mq.innerDealedMsgList)
	fmt.Println(mq.innerWaitMsgList)

}
