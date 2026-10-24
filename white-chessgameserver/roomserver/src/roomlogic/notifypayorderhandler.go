package roomlogic

import (
	"gamelogic"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"player"
	"scorestorage"
	"strconv"
	"util"
)

//刷新版本号处理器
type NotifyPayOrderHandler struct {
	roomuserManager *gamelogic.RoomUserManager
	sstorage scorestorage.UserScoreStorage
	gameid      int32
	serverid    int32
}

//创建版本号刷新处理器
func NewNotifyPayOrderHandler(gameid int32, serverid int32, roomuserManager *gamelogic.RoomUserManager, sstorage scorestorage.UserScoreStorage) *NotifyPayOrderHandler {

	handler := new(NotifyPayOrderHandler)
	handler.roomuserManager = roomuserManager
	handler.sstorage = sstorage
	handler.gameid = gameid
	handler.serverid = serverid

	return handler
}

func (h *NotifyPayOrderHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	defer util.PanicErrStack()

	mmbyte := m.MsgData.([]byte)
	mm := &netproto.PayOrderList{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		mlog.Error("解析錯誤 HostAndBackUp")
	}
	if mm.GetServerID() == h.serverid {
		for _, info := range mm.GetPayOrderList() {
			if gameuser := h.roomuserManager.GetGameUserByUserID(info.GetUserID()); gameuser != nil {
				if len(strconv.Itoa(int(gameuser.TimeSeq))) != 17 || len(strconv.Itoa(int(info.GetTimeSeq()))) != 17{
					mlog.Error("TimeSeq 長度有問題, gameuser.TimeSeq = %d, info.TimeSeq = %d", gameuser.TimeSeq, info.GetTimeSeq())
				}

				if gameuser.TimeSeq < info.GetTimeSeq() {
					h.sstorage.OnUserMoneyAdd(gameuser, info.GetAmount())
					example.invalid("用戶 %d 到帳金額 %d 完成", info.GetUserID(), info.GetAmount())
				}
			} else {
				mlog.Warn("通知用戶到帳、但 %d 用戶不存在", info.GetUserID())
			}
		}
	} else {
		mlog.Error("NotifyServer 發錯服務器了")
	}
}

func (h *NotifyPayOrderHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_PayOrderID)
}
