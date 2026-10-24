package service

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	dbutil "example.invalid/ServerTeam/Servers/pkg/db/dbutil"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// DeferredAwardService 延遲獎勵
type DeferredAwardService struct{}

// Read 讀取 DeferredAward
func (s *DeferredAwardService) Read(userID, gameID, flagID int32, awardType string) ([]*netproto.Slot_DeferredAward, error) {

	const ProcedureName = "PrGs_Slot_DeferredAwardRead"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)                // UserID
	ps.AddIntInput("intGameID", gameID)                // GameID
	ps.AddIntInput("intFlagID", flagID)                // FlagID
	ps.AddVarcharInput("chvAwardType", awardType, 100) // Award Type
	ps.AddVarcharOutput("chvMsg", "")                  // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return nil, fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return nil, fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	var result []*netproto.Slot_DeferredAward

	for i := 0; i < len(dbret[0].Rows); i++ {

		data := new(netproto.Slot_DeferredAward)

		if val, ok := dbret[0].GetValueByColName(i, "AwardType").(string); ok {
			data.AwardType = proto.String(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "OriginID").(int64); ok {
			data.OriginID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Data").(string); ok {
			data.Data = proto.String(val)
		}

		result = append(result, data)
	}

	return result, nil
}

// Record 新增獎勵紀錄
func (s *DeferredAwardService) Record(userID, gameID, flagID int32, awardType string, originID int32, data string) error {

	const ProcedureName = "PrGs_Slot_DeferredAwardRecord"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)                // 玩家ID
	ps.AddIntInput("intGameID", gameID)                // 遊戲ID
	ps.AddIntInput("intFlagID", flagID)                // FlagID
	ps.AddVarcharInput("chvAwardType", awardType, 100) // Award Type
	ps.AddIntInput("intOriginID", originID)            // OriginID
	ps.AddVarcharInput("txtData", data, 255)           // Data

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

// Pop 取用一筆 DeferredAward
func (s *DeferredAwardService) Pop(userID, gameID, flagID int32, awardType string) (*netproto.Slot_DeferredAward, error) {

	const ProcedureName = "PrGs_Slot_DeferredAwardPop"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)                // UserID
	ps.AddIntInput("intGameID", gameID)                // GameID
	ps.AddIntInput("intFlagID", flagID)                // FlagID
	ps.AddVarcharInput("chvAwardType", awardType, 100) // Award Type
	ps.AddVarcharOutput("chvMsg", "")                  // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return nil, fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return nil, fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	var result []*netproto.Slot_DeferredAward

	for i := 0; i < len(dbret[0].Rows); i++ {

		data := new(netproto.Slot_DeferredAward)

		if val, ok := dbret[0].GetValueByColName(i, "AwardType").(string); ok {
			data.AwardType = proto.String(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "OriginID").(int64); ok {
			data.OriginID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Data").(string); ok {
			data.Data = proto.String(val)
		}

		result = append(result, data)
	}

	if len(result) == 0 {
		return nil, nil
	}

	return result[0], nil
}
