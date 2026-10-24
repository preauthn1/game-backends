package service

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// JackpotNoLinkService Jackpot相關存取DB服務
type JackpotNoLinkService struct{}

// GetJackpotNoLink 取得非連線型Jackpot資訊
func (s *JackpotNoLinkService) GetJackpotNoLink(gameID, flagID, tableNo, levelID int32) ([]*netproto.Slot_JackpotData, error) {

	const ProcedureName = "PrGs_Slot_JackpotPool_NoLink"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intGameID", gameID)   // GameID
	ps.AddIntInput("intFlagID", flagID)   // FlagID
	ps.AddIntInput("intTableNo", tableNo) // 桌子編號
	ps.AddIntInput("intLevelID", levelID) // Jackpot等級
	ps.AddVarcharOutput("chvMsg", "")     // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return nil, fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return nil, fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	var result []*netproto.Slot_JackpotData

	for i := 0; i < len(dbret[0].Rows); i++ {

		data := new(netproto.Slot_JackpotData)

		if val, ok := dbret[0].GetValueByColName(i, "GameID").(int64); ok {
			data.GameID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "FlagID").(int64); ok {
			data.FlagID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "TableNo").(int64); ok {
			data.TableNo = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "LevelID").(int64); ok {
			data.Level = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Contribution").(int64); ok {
			data.Contribution = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "TotalContribution").(int64); ok {
			data.TotalContribution = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "AwardTotal").(int64); ok {
			data.AwardTotal = proto.Int64(val)
		}

		result = append(result, data)
	}

	return result, nil
}

// UpdateJackpotNoLink 取得非連線型Jackpot資訊
func (s *JackpotNoLinkService) UpdateJackpotNoLink(gameID, flagID, tableNo, levelID int32, contribution, totalContribution, awardTotal int64) ([]*netproto.Slot_JackpotData, error) {

	const ProcedureName = "PrGs_Slot_JackpotPool_NoLink"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intGameID", gameID)                          // GameID
	ps.AddIntInput("intFlagID", flagID)                          // FlagID
	ps.AddIntInput("intTableNo", tableNo)                        // 桌子編號
	ps.AddIntInput("intLevelID", levelID)                        // Jackpot等級
	ps.AddBigIntInput("lngContribution", contribution)           // 玩家貢獻值
	ps.AddBigIntInput("lngTotalContribution", totalContribution) // 累積貢獻值
	ps.AddBigIntInput("lngAwardTotal", awardTotal)               // 已發送獎金總和
	ps.AddVarcharOutput("chvMsg", "")                            // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		return nil, fmt.Errorf("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
	}

	msg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		return nil, fmt.Errorf("Procedure %s execute error. %v", ProcedureName, msg)
	}

	var result []*netproto.Slot_JackpotData

	for i := 0; i < len(dbret[0].Rows); i++ {

		data := new(netproto.Slot_JackpotData)

		if val, ok := dbret[0].GetValueByColName(i, "GameID").(int64); ok {
			data.GameID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "FlagID").(int64); ok {
			data.FlagID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "TableNo").(int64); ok {
			data.TableNo = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "LevelID").(int64); ok {
			data.Level = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Contribution").(int64); ok {
			data.Contribution = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "TotalContribution").(int64); ok {
			data.TotalContribution = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "AwardTotal").(int64); ok {
			data.AwardTotal = proto.Int64(val)
		}

		result = append(result, data)
	}

	return result, nil
}
