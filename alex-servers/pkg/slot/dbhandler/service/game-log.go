package service

import (
	"fmt"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/db"
	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// GameLogService 玩家紀錄相關
type GameLogService struct{}

// Add 新增遊戲紀錄
func (s *GameLogService) Add(gameType, logType, userID, gameID, serverID int32, currentAmount, currentCoupon, betMoney, betCoupon, winMoney, finalScore, finalCoupon int64, data string) error {

	const ProcedureName = "PrGs_Slot_AddGameLog"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)                  // 玩家ID
	ps.AddIntInput("intGameID", gameID)                  // 遊戲ID
	ps.AddIntInput("intServerID", serverID)              // ServerID
	ps.AddBigIntInput("lngCurrentAmount", currentAmount) // 當前金額
	ps.AddBigIntInput("lngCurrentCoupon", currentCoupon) // 當前Coupon
	ps.AddBigIntInput("lngBetMoney", betMoney)           // 花費金額
	ps.AddBigIntInput("lngBetCoupon", betCoupon)         // 花費Coupon
	ps.AddBigIntInput("lngWinMoney", winMoney)           // 贏得金額
	ps.AddBigIntInput("lngFinalScore", finalScore)       // 最終金額
	ps.AddBigIntInput("lngFinalCoupon", finalCoupon)     // 最終Coupon
	ps.AddIntInput("intGameType", gameType)              // 遊戲類型
	ps.AddIntInput("intLogType", logType)                // 紀錄類型
	ps.AddVarcharInput("chvGameData", data, 100)         // 遊戲資料

	ps.AddVarcharOutput("chvMsg", "")

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	return nil
}

// Search 查詢遊戲紀錄
func (s *GameLogService) Search(userID, gameID, index, size int32, start, end string) (int32, []*netproto.Slot_GameLog, error) {

	const ProcedureName = "PrGs_Slot_GetGameLog"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)           // 玩家ID
	ps.AddIntInput("intGameID", gameID)           // 遊戲ID
	ps.AddVarcharInput("dtStartTime", start, 100) // 開始時間
	ps.AddVarcharInput("dtEndTime", end, 100)     // 結束時間
	ps.AddIntInput("intPageIndex", index)         // 起始筆數
	ps.AddIntInput("intPageSize", size)           // 每頁幾筆
	ps.AddIntOutput("intPageCount", 0)
	ps.AddVarcharOutput("chvMsg", "")

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return 0, nil, fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return 0, nil, fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	pageCount := dbret.GetOutputParamValueInt32("intPageCount")

	var result []*netproto.Slot_GameLog

	for i := 0; i < len(dbret[0].Rows); i++ {
		data := new(netproto.Slot_GameLog)

		if val, ok := dbret[0].GetValueByColName(i, "RowNum").(int64); ok {
			data.RowNum = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "OrderID").(int64); ok {
			data.OrderID = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "CreateTime").(time.Time); ok {
			data.CreateTime = proto.String(val.Format("2006-01-02 15:04:05"))
		}

		if val, ok := dbret[0].GetValueByColName(i, "BetMoney").(int64); ok {
			data.BetMoney = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "FinalScore").(int64); ok {
			data.Profit = proto.Int64(val)
		}

		result = append(result, data)
	}

	return pageCount, result, nil
}
