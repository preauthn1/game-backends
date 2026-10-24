package network

import (
	"errors"
	"fmt"
	"mlog"
	"util"
)

//数据包处理接口定义
type MsgPacker interface {
	Packet(data []byte) []byte
	UnPack(data []byte) ([]byte, error)
}

//基于包头包体方式的分包
type HeadMsgPacker struct {
	maxMsgLen int32  //最大消息长度
	bigEndian bool   //是否大头字节序
	dataBuf   []byte //缓冲数据流
}

func NewHeadMsgPacker(maxMsgLen int32, bigEndian bool) *HeadMsgPacker {
	p := new(HeadMsgPacker)
	p.maxMsgLen = maxMsgLen
	p.bigEndian = bigEndian

	return p
}

//包头长度
const (
	MSGHEADLEN int32 = 4
)

//打包
func (packer *HeadMsgPacker) Packet(data []byte) []byte {
	msgLen := int32(len(data))
	return append(util.IntToBytes(msgLen, packer.bigEndian), data...)
}

//解包
func (packer *HeadMsgPacker) UnPack(data []byte) ([]byte, error) {
	var buf []byte
	if data != nil && len(data) > 0 {
		buf = append(packer.dataBuf, data...)
	} else {
		buf = packer.dataBuf
	}

	if int32(len(buf)) <= MSGHEADLEN {
		packer.dataBuf = buf
		return nil, nil
	}

	msglen := util.BytesToInt(buf[:MSGHEADLEN], packer.bigEndian)

	if msglen >= packer.maxMsgLen || msglen <= 0 {
		mlog.Error("消息长度超出范围,%d", msglen)
		return nil, errors.New(fmt.Sprintf("消息长度超出范围,当前消息长度%d", msglen))
	}

	if int32(len(buf)) < MSGHEADLEN+msglen {
		packer.dataBuf = buf
		return nil, nil
	}

	packer.dataBuf = buf[MSGHEADLEN+msglen:]
	return buf[MSGHEADLEN : MSGHEADLEN+msglen], nil
}

//空的数据包处理器
type EmptyMsgPacker struct {
}

func (packer *EmptyMsgPacker) Packet(data []byte) []byte {
	return data
}

func (packer *EmptyMsgPacker) UnPack(data []byte) ([]byte, error) {
	return nil, nil
}

//数据包处理器组
type MsgPackerGroup struct {
	innerPackers []MsgPacker
}

func (packer *MsgPackerGroup) Packet(data []byte) []byte {
	ndata := data
	for _, p := range packer.innerPackers {
		ndata = p.Packet(ndata)
	}

	return ndata
}

func (packer *MsgPackerGroup) UnPack(data []byte) ([]byte, error) {
	dlen := len(packer.innerPackers)
	ndata := data
	var err error

	for i := dlen - 1; i >= 0; i-- {
		var nerr error
		p := packer.innerPackers[i]
		ndata, nerr = p.UnPack(ndata)

		if nerr != nil {
			err = nerr
		}
	}

	return ndata, err
}

func (packer *MsgPackerGroup) AppendPacker(p MsgPacker) *MsgPackerGroup {
	packer.innerPackers = append(packer.innerPackers, p)

	return packer
}
