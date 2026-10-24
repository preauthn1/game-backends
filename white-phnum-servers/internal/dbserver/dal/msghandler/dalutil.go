package msghandler

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//数据库连接字符串
var ConnString string
var DealLogConnString string
var ClubConnString string
var PoolSize int = 100
var database *db.Database
var dealLogDataBase *db.Database
var clubDataBase *db.Database
var wildCatDataBase *db.Database
var SMSWebAPI string
var DbUtil db.DatabaseEngine
var DealLogDbUtil db.DatabaseEngine
var ClubDbUtil db.DatabaseEngine
var WildCatDbUtil db.DatabaseEngine
var HDCodeMap *util.SafeMap
var FastLogonStat bool

func InitDatabase() {
	//dao := new(db.Database)
	//dao.Connstring = ConnString
	//dao.Connpool = db.NewConnPool(PoolSize, ConnString)
	database = db.NewDatabase(ConnString, PoolSize)
	dealLogDataBase = db.NewDatabase(DealLogConnString, PoolSize)
	clubDataBase = db.NewDatabase(ClubConnString, PoolSize)
	DbUtil = db.NewDatabaseEngine(database)
	DealLogDbUtil = db.NewDatabaseEngine(dealLogDataBase)

	ClubDbUtil = db.NewDatabaseEngine(clubDataBase)
	WildCatDbUtil = db.NewDatabaseEngine(wildCatDataBase)

	HDCodeMap = util.NewSafeMap()
	FastLogonStat = false
	command.AddCommand(NewDBUtilCmd())
}

//获取提示消息
func GetErrMsgValue(set db.RecordSet) string {
	rlen := len(set)

	return set[rlen-2].GetValueByColName(0, "chvErrMsg").(string)
}

//获取数据库执行结果消息
func GetDBRetMsg(retCode int32) *msg.Message {
	ret := new(netproto.DBServerRet)
	ret.Code = proto.Int32(retCode)

	msg := new(msg.Message)
	msg.BClassID = int32(netproto.MessageBClassID_DBServer)
	msg.SClassID = int32(netproto.DBServerClassID_DBRetID)
	msg.MsgData = ret

	return msg
}

//解析用户登录结果
func parseUserLogonRet(ret db.RecordSet) *netproto.UserLoginRet {

	logret := &netproto.UserLoginRet{}

	//充值返利
	if ret.GetRetTableCount() >= 7 {
		grt := ret[5]
		if len(grt.Rows) < 1 {
			mlog.Error("充值返利信息為為空, userid=%d", logret.GetUserID())
			FlowAmount := new(netproto.FlowAmountInfo)
			FlowAmount.Status = proto.Int32(0)
			FlowAmount.UnionPlatform = proto.Int32(0)
			FlowAmount.FlowAmountNow = proto.Int32(0)
			FlowAmount.FlowAmountLimit = proto.Int32(0)
			FlowAmount.Achievement = proto.Int32(0)
			FlowAmount.FlowAmountRate = proto.Int32(0)
			FlowAmount.BonusRate = proto.Int32(0)
			FlowAmount.CustomerDomain = proto.String("")
			logret.FlowInfo = FlowAmount
		} else {
			grt.ForEachRow(func(r db.RecordRow) {
				FlowAmount := new(netproto.FlowAmountInfo)
				FlowAmount.Status = proto.Int32(int32(grt.GetValueByColNameInt64(0, "Status")))
				FlowAmount.UnionPlatform = proto.Int32(int32(grt.GetValueByColNameInt64(0, "UnionPlatform")))
				FlowAmount.FlowAmountNow = proto.Int32(int32(grt.GetValueByColNameInt64(0, "FlowAmountNow")))
				FlowAmount.FlowAmountLimit = proto.Int32(int32(grt.GetValueByColNameInt64(0, "FlowAmountLimit")))
				FlowAmount.Achievement = proto.Int32(int32(grt.GetValueByColNameInt64(0, "OpenStatus")))
				FlowAmount.FlowAmountRate = proto.Int32(int32(grt.GetValueByColNameInt64(0, "FlowAmountRate")))
				FlowAmount.BonusRate = proto.Int32(int32(grt.GetValueByColNameInt64(0, "BonusRate")))
				FlowAmount.CustomerDomain = proto.String(grt.GetSingleValueString("CustomerDomain"))
				logret.FlowInfo = FlowAmount
			})
		}
	}

	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("解析用户登录结果发生错误%v, %s", err, errorStr)
			panic(err)
		}
	}()

	code := 0
	if ret.GetReturnValue() == 1 {
		code = 1
	} else {
		// 登入失败
		errmsg := ret.GetOutputParamValue("chvErrMsg").(string)
		userid := ret.GetOutputParamValue("intUserId").(int64)
		mlog.Warn("parseUserLogon Error, code %d, userid %d, errmsg %s", ret.GetReturnValue(), userid, errmsg)
	}

	if ret.GetReturnValue() == -2 {
		mlog.Error("邀請碼錯誤")
		logret.Code = proto.Int32(-2)
		logret.Message = proto.String("游戏重要数据不完整，请重新扫码下载")
		return logret
	}

	if len(ret[0].Rows) < 1 { //找不到用戶
		mlog.Error("缺少了用戶資料")
		logret.Code = proto.Int32(0)
		logret.Message = proto.String("服务器错误.")
		return logret
	}

	logret.Code = proto.Int32(int32(code))
	logret.Message = proto.String(ret.GetOutputParamValue("chvErrMsg").(string))

	if code == 1 {
		logret.UserID = proto.Int32(int32(ret.GetOutputParamValue("intUserID").(int64)))
		logret.Cer = proto.String(ret.GetOutputParamValue("chvCertification").(string))

		userdata := new(netproto.UserHallLogonData)
		userdata.LoginName = proto.String(ret[0].GetSingleValueString("LoginName"))
		userdata.NickName = proto.String(ret[0].GetSingleValueString("NickName"))
		userdata.UserType = proto.Int32(ret[0].GetSingleValueInt32("UserType"))
		userdata.Sex = proto.Int32(ret[0].GetSingleValueInt32("Sex"))
		userdata.HeadID = proto.Int32(ret[0].GetSingleValueInt32("HeadID"))
		userdata.IsTopWindow = proto.Bool(ret[0].GetSingleValueInt32("IsTopWindow") == 1)
		userdata.CashAmount = proto.Int64(ret[0].GetSingleValueInt64("CashAmount"))
		userdata.BankAmount = proto.Int64(ret[0].GetSingleValueInt64("BankAmount"))
		userdata.IsGaming = proto.Bool(ret[0].GetSingleValueBool("IsGaming"))
		userdata.ServerAddr = proto.String(ret[0].GetSingleValueString("ServerAddr"))
		userdata.ServerName = proto.String(ret[0].GetSingleValueString("ServerName"))
		userdata.IsKick = proto.Bool(ret[0].GetSingleValueBool("IsKick"))
		userdata.IsBindGuest = proto.Bool(ret[0].GetSingleValueInt32("IsBindGuest") == 1)
		userdata.IsBindZhifubao = proto.Bool(ret[0].GetSingleValueInt32("IsBindZhifubao") == 1)
		userdata.Zhifubao = proto.String(ret[0].GetSingleValueString("AlipayAccount"))
		userdata.RealName = proto.String(ret[0].GetSingleValueString("RealName"))
		userdata.UserLevel = proto.Int32(ret[0].GetSingleValueInt32("UserLevel"))
		userdata.LevelKey = proto.String(ret[0].GetSingleValueString("LevelKey"))
		userdata.AnnMsg = proto.String(ret[0].GetSingleValueString("AnnMsg"))
		userdata.ConvertRateTipMsg = proto.String(ret[0].GetSingleValueString("ConvertRateTipMsg"))
		userdata.BankPwdTipMsg = proto.String(ret[0].GetSingleValueString("BankPwdTipMsg"))
		userdata.UIFlag = proto.String(ret[0].GetSingleValueString("UIFlag"))
		userdata.PaySort = proto.String(ret[0].GetSingleValueString("PaySort"))
		userdata.PayAmountConfig = proto.String(ret[0].GetSingleValueString("PayAmountConfig"))
		userdata.PayNotifyMsg = proto.String(ret[0].GetSingleValueString("PayNotifyMsg"))
		userdata.PayTips = proto.String(ret[0].GetSingleValueString("PayTips"))
		userdata.NotifyFlag = proto.String(ret[0].GetSingleValueString("NotifyFlag"))
		userdata.UpGradeMsg = proto.String(ret[0].GetSingleValueString("UpGradeMsg"))
		userdata.LockGameID = proto.Int32(ret[0].GetSingleValueInt32("LockGameID"))
		userdata.IsBindBankCard = proto.Bool(ret[0].GetSingleValueInt32("IsBindUnionpayBank") == 1)
		userdata.BankCode = proto.String(ret[0].GetSingleValueString("UnionpayBank"))
		userdata.BankCardNum = proto.String(ret[0].GetSingleValueString("UnionpayAccount"))
		userdata.BankName = proto.String(ret[0].GetSingleValueString("UnionpayBankName"))
		userdata.BankRealName = proto.String(ret[0].GetSingleValueString("UnionpayRealName"))
		userdata.UISort = proto.String(ret[0].GetSingleValueString("UISort"))
		userdata.SumCharge = proto.Int32(ret[0].GetSingleValueInt32("SumCharge"))
		userdata.ShowConvertLimit = proto.Int32(ret[0].GetSingleValueInt32("ShowConvertLimit"))
		userdata.ShowAllGameLimit = proto.Int32(ret[0].GetSingleValueInt32("ShowAllGameLimit"))
		userdata.QuestionContactTip = proto.String(ret[0].GetSingleValueString("QuestionContactTip"))
		userdata.AnnMsgShow = proto.Int32(ret[0].GetSingleValueInt32("AnnMsgShow"))
		userdata.ClubOpen = proto.Bool(ret[0].GetSingleValueBool("ClubOpen"))
		userdata.ConvertBankShow = proto.Int32(ret[0].GetSingleValueInt32("ConvertBankShow"))
		userdata.ConvertAlipayShow = proto.Int32(ret[0].GetSingleValueInt32("ConvertAlipayShow"))
		userdata.ShowVIPAgent = proto.Int64(ret[0].GetSingleValueInt64("ShowVIPAgent"))
		userdata.ShopNameID = proto.Int32(ret[0].GetSingleValueInt32("ShopNameID"))
		userdata.AgentNameID = proto.Int32(ret[0].GetSingleValueInt32("AgentNameID"))
		userdata.ShowCS = proto.Int64(ret[0].GetSingleValueInt64("ShowCS"))
		userdata.PlaytimeCS = proto.Int32(ret[0].GetSingleValueInt32("PlaytimeCS"))
		userdata.PlaytimeConvert = proto.Int32(ret[0].GetSingleValueInt32("PlaytimeConvert"))
		userdata.PlaytimeVIPAgent = proto.Int32(ret[0].GetSingleValueInt32("PlaytimeVIPAgent"))
		userdata.ShowConvertBindPhone = proto.Int32(ret[0].GetSingleValueInt32("ShowConvertBindPhone"))
		userdata.ShowAllGameBindPhone = proto.Int32(ret[0].GetSingleValueInt32("ShowAllGameBindPhone"))
		userdata.ShowVIPAgentBindPhone = proto.Int32(ret[0].GetSingleValueInt32("ShowVIPAgentBindPhone"))
		userdata.ShowCSBindPhone = proto.Int32(ret[0].GetSingleValueInt32("ShowCSBindPhone"))
		userdata.ShowConvertPopup = proto.Int32(ret[0].GetSingleValueInt32("ShowConvertPopup"))
		userdata.ConvertPopupText = proto.String(ret[0].GetSingleValueString("ConvertPopupText"))
		userdata.CouponAmount = proto.Int64(ret[0].GetSingleValueInt64("CouponAmount"))
		userdata.AvatarFrame = proto.Int32(ret[0].GetSingleValueInt32("AvatarFrame"))
		userdata.GiftMoney = proto.Int32(ret[0].GetSingleValueInt32("GiftMoney"))
		userdata.ShowCampaignAtLogin = proto.Int32(ret[0].GetSingleValueInt32("ShowCampaignAtLogin"))
		userdata.ShowFirstDepositAtLogin = proto.Int32(ret[0].GetSingleValueInt32("ShowFirstDepositAtLogin"))

		//游戏
		if ret.GetRetTableCount() >= 3 {
			grt := ret[1]

			grt.ForEachRow(func(r db.RecordRow) {
				gid := r[0].(int64)
				popular := r[1].(int64)
				iconName := r[2].(string)
				aliasName := r[3].(string)
				iconTitle := r[4].(string)
				skin := r[5].(string)
				userdata.GameList = append(userdata.GameList, int32(gid))
				//新的GameList 協議
				gameList2 := new(netproto.GameList2)
				gameList2.GameID = proto.Int32(int32(gid))
				gameList2.Popular = proto.Int32(int32(popular))
				gameList2.IconName = proto.String(iconName)
				gameList2.AliasName = proto.String(aliasName)
				gameList2.IconTitle = proto.String(iconTitle)
				gameList2.Skin = proto.String(skin)
				userdata.GameList2 = append(userdata.GameList2, gameList2)
			})
		}

		//支付方式最大值及最小值
		if ret.GetRetTableCount() >= 4 {
			payAmonutRange := ""
			grt := ret[2]
			grt.ForEachRow(func(r db.RecordRow) {
				payAmonutRange += fmt.Sprintf("%v:%v.%v", r[0], r[1], r[2]) + "|"
			})
			if len(payAmonutRange) > 0 {
				userdata.PayAmountRange = proto.String(payAmonutRange[:len(payAmonutRange)-1])
			}
		}

		//輪播圖
		if ret.GetRetTableCount() >= 5 {
			grt := ret[3]
			if len(grt.Rows) < 1 {
				mlog.Error("輪播圖資訊為空, userid=%d", logret.GetUserID())
			}
			grt.ForEachRow(func(r db.RecordRow) {
				picture := r[0].(string)
				link := r[1].(string)
				behavior := r[2].(int64)
				banner := new(netproto.Banner)
				banner.Picture = proto.String(picture)
				banner.Link = proto.String(link)
				banner.Behavior = proto.Int32(int32(behavior))
				userdata.Banner = append(userdata.Banner, banner)
			})
		}

		//支付信息
		if ret.GetRetTableCount() >= 6 {
			grt := ret[4]
			if len(grt.Rows) < 1 {
				mlog.Error("支付信息為空, userid=%d", logret.GetUserID())
			}
			grt.ForEachRow(func(r db.RecordRow) {
				gwId := r[0].(int64)
				typeId := r[1].(int64)
				typeName := r[2].(string)
				amountSort := r[3].(string)
				paymentInfo := new(netproto.PaymentInfo)
				paymentInfo.GWID = proto.Int32(int32(gwId))
				paymentInfo.TypeID = proto.Int32(int32(typeId))
				paymentInfo.TypeName = proto.String(typeName)
				paymentInfo.AmountSort = proto.String(amountSort)
				userdata.PaymentInfoList = append(userdata.PaymentInfoList, paymentInfo)
			})
		}

		logret.UserData = userdata
	}

	return logret
}

func updateProtectNodeStatus(userid int32, token string, status int32) {
	ps := db.NewSqlParameters()
	ps.AddVarcharInput("chvToken", token, 10)
	ps.AddIntInput("intLoginUserID", userid)
	ps.AddIntInput("intStatus", status)
	proc := db.NewProcedure("PrPs_UpdateProtectNodeStatus", ps)
	ret, err := DbUtil.ExecProc(proc)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	//fmt.Println(proc.GetSqlString())
	if err != nil {
		panic(err)
	}
}

func getRetMsg(code int32, msgContent string) *netproto.RetMessage {
	retmsg := new(netproto.RetMessage)
	retmsg.Code = proto.Int32(code)
	retmsg.Message = proto.String(msgContent)

	return retmsg
}
