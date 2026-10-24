package service

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	dbutil "example.invalid/ServerTeam/Servers/pkg/db/dbutil"
	"example.invalid/ServerTeam/Servers/pkg/mlog"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// BonusGameService 玩家紀錄相關
type BonusGameService struct{}

// Read 讀取BonusGame
func (s *BonusGameService) Read(userID, gameID, flagID int32) ([]*netproto.Slot_BonusGameDetail, error) {

	const ProcedureName = "PrGs_Slot_BonusGameRead"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID) // UserID
	ps.AddIntInput("intGameID", gameID) // GameID
	ps.AddIntInput("intFlagID", flagID) // FlagID
	ps.AddVarcharOutput("chvMsg", "")   // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return nil, fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return nil, fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	var result []*netproto.Slot_BonusGameDetail

	for i := 0; i < len(dbret[0].Rows); i++ {

		data := new(netproto.Slot_BonusGameDetail)

		if val, ok := dbret[0].GetValueByColName(i, "UserID").(int64); ok {
			data.UserID = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read UserID error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "GameID").(int64); ok {
			data.GameID = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read GameID error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "FlagID").(int64); ok {
			data.FlagID = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read FlagID error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "OriginID").(int64); ok {
			data.OriginID = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read OriginID error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "TurnID").(int64); ok {
			data.TurnID = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read TurnID error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "Location").(int64); ok {
			data.Location = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read Location error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "BetMoney").(int64); ok {
			data.BetMoney = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read BetMoney error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "BetRate").(int64); ok {
			data.BetRate = proto.Int32(int32(val))
		} else {
			mlog.Warn("Procedure %s Read BetRate error", ProcedureName)
		}

		if val, ok := dbret[0].GetValueByColName(i, "Data").(string); ok {
			data.Data = proto.String(val)
		} else {
			mlog.Warn("Procedure %s Read Data error", ProcedureName)
		}

		result = append(result, data)
	}

	return result, nil
}

// Record 紀錄BonusGame
func (s *BonusGameService) Record(userID, gameID, flagID, originID, turnID, location, betMoney, betRate int32, data string) error {
	const ProcedureName = "PrGs_Slot_BonusGameRecord"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)      // UserID
	ps.AddIntInput("intGameID", gameID)      // GameID
	ps.AddIntInput("intFlagID", flagID)      // FlagID
	ps.AddIntInput("intOriginID", originID)  // OriginID
	ps.AddIntInput("intTurnID", turnID)      // TurnID
	ps.AddIntInput("intLocation", location)  // Location
	ps.AddIntInput("intBetMoney", betMoney)  // BetMoney
	ps.AddIntInput("intBetRate", betRate)    // BetRate
	ps.AddVarcharInput("txtData", data, 255) // Data
	ps.AddVarcharOutput("chvMsg", "")        // chvMsg的輸出

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

// Reset 重置BonusGame
func (s *BonusGameService) Reset(userID, gameID, flagID, originID int32) error {
	const ProcedureName = "PrGs_Slot_BonusGameReset"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)     // UserID
	ps.AddIntInput("intGameID", gameID)     // GameID
	ps.AddIntInput("intFlagID", flagID)     // FlagID
	ps.AddIntInput("intOriginID", originID) // OriginID
	ps.AddVarcharOutput("chvMsg", "")       // chvMsg的輸出

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
