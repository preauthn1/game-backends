package service

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/db"
	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
)

// LightningService Lightning相關功能
type LightningService struct{}

// Read 查詢Lightning資料
func (s *LightningService) Read(userID, gameID, flagID int32) (string, error) {

	const ProcedureName = "PrGs_Slot_LightningGameRead"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID) // 玩家ID
	ps.AddIntInput("intGameID", gameID) // 遊戲ID
	ps.AddIntInput("intFlagID", flagID) // FlagID

	ps.AddVarcharOutput("chvMsg", "")

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return "", fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return "", fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	var data string
	for i := 0; i < len(dbret[0].Rows); i++ {
		if val, ok := dbret[0].GetValueByColName(i, "Data").(string); ok {
			data = val
		}
	}

	return data, nil
}

// Record 更新Lightning資料
func (s *LightningService) Record(userID, gameID, flagID int32, data string) error {

	const ProcedureName = "PrGs_Slot_LightningGameRecord"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", userID)          // 玩家ID
	ps.AddIntInput("intGameID", gameID)          // 遊戲ID
	ps.AddIntInput("intFlagID", flagID)          // FlagID
	ps.AddVarcharInput("txtData", data, 1<<16-1) // Data

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
