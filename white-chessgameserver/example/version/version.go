package main

import (
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"msg"
	"net"
	"netproto"
	"os"
	"reflect"
	"unsafe"
	"util"
)

func main() {

	fmt.Println("開始取得版號")

	serverip := "0.0.0.0:10000"
	//serverip := "0.0.0.0:8087"
	StartClient(serverip)

	select {}

}

//客户端启动函数
//      远程ip地址和端口 tcpaddr
func StartClient(tcpaddr string) {
	tcpAddr, err := net.ResolveTCPAddr("tcp4", tcpaddr)
	checkError(err, "ResolveTCPAddr")
	conn, err := net.DialTCP("tcp", nil, tcpAddr)
	checkError(err, "DialTCP")
	// 启动客户端发送线程
	go chatSend(conn)

	go readSend(conn)

}

//接收數據
func readSend(conn net.Conn) {
	// 接收数据
	buf := make([]byte, 1024)
	for {
		lenght, err := conn.Read(buf)
		if checkError(err, "Connection") == false {
			fmt.Println("read error")
		}

		fmt.Println("read", lenght)
		fmt.Println(string(buf[:lenght-1]))

		//UnMarshal
		msgparser := msg.NewMsgParserGroup()
		msgparser.RegisterParser(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_HallVerInfoID), msg.NewProtoMsgParser())
		data := buf[4:]
		if len(data) < 8 {
			fmt.Println("消息不正确")
		}

		msg := new(msg.Message)
		msg.BClassID = util.BytesToInt(data[:4], true)
		msg.SClassID = util.BytesToInt(data[4:8], true)

		msg.MsgData = data[8:]

		//解析MsgData詳細格式與內容
		hallVerData := &netproto.HallVerInfo{}
		byteMsgData := msg.MsgData.([]byte) //須將interface轉[]byte
		errMsgData := proto.Unmarshal(byteMsgData, hallVerData)

		if errMsgData != nil {
			fmt.Println("解析MsgData 失敗")
		}

		fmt.Println("BClassID => %v", msg.BClassID)
		fmt.Println("SClassID => %v", msg.SClassID)
		fmt.Println("MsgData => ", hallVerData)

	}

}

func BytesToString(b []byte) string {
	bh := (*reflect.SliceHeader)(unsafe.Pointer(&b))
	sh := reflect.StringHeader{bh.Data, bh.Len}
	return *(*string)(unsafe.Pointer(&sh))
}

//客户端发送线程
//      发送连接 conn
func chatSend(conn net.Conn) {

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestHallVerID), nil)

	msgparser := msg.NewMsgParserGroup()
	msgparser.RegisterParser(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_RequestHallVerID), msg.NewProtoMsgParser())

	protoHeader := int(netproto.HallMsgClassID_value["ServerListDataID"])
	byteHeader := util.IntToBytes(int32(protoHeader), true)

	data, err := msgparser.Marshal(retmsg)
	if checkError(err, "ProtoMarshal") == false {
		conn.Close()
		os.Exit(0)
	}

	sendData := append(byteHeader, data...)

	// 发送给服务端
	lens, err := conn.Write(sendData)
	if lens > 0 {
	}
	if err != nil {
		fmt.Println(err.Error())
		conn.Close()
		//break
	}

}

//错误检查
func checkError(err error, info string) (res bool) {
	if err != nil {
		fmt.Println(info + "  " + err.Error())
		return false
	}
	return true
}
