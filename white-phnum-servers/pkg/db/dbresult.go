package db

import (
	"fmt"
	"strings"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

//行集
type RecordRow []interface{}

//数据表
type RecordTable struct {
	Rows []RecordRow
	Cols []string
}

//结果集
type RecordSet []*RecordTable

//根据列名获取值
func (t *RecordTable) GetValueByColName(rowindex int, name string) interface{} {
	if name == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	if rowindex >= len(t.Rows) || rowindex < 0 {
		data := fmt.Sprintf("超過資料行數, 查詢rowindex:%v , 錯誤欄位:%v", rowindex, name)
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	colidx := -1
	name = strings.ToLower(name)

	for i, n := range t.Cols {
		if strings.ToLower(n) == name {
			colidx = i
			break
		}
	}

	if colidx == -1 {
		data := fmt.Sprintf("Table無此欄位:%v", name)
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	if colidx >= len(t.Cols) {
		data := fmt.Sprintf("超過資料行數, 錯誤欄位:%v", name)
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	if t.Rows[rowindex][colidx] == nil {
		data := fmt.Sprintf("Table錯誤欄位:%v 資料為Null ", name)
		mlog.NewDefaultLogger().SetCallDepLen(5).SetData(data).Warn("SP")
		return nil
	}

	return t.Rows[rowindex][colidx]
}

func (t *RecordTable) GetValueByColNameInt32(iRowIndex int, strColName string) int32 {
	if strColName == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return -1
	}

	if iRowIndex >= len(t.Rows) || iRowIndex < 0 {
		mlog.Error("超過資料行數, iRowIndex:%v , 錯誤欄位:%v", iRowIndex, strColName)
		return -1
	}

	pRet := t.GetValueByColName(iRowIndex, strColName)
	if pRet == nil {
		return -1
	}

	return int32(pRet.(int64))
}

func (t *RecordTable) GetValueByColNameInt64(iRowIndex int, strColName string) int64 {
	if strColName == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return -1
	}

	if iRowIndex >= len(t.Rows) || iRowIndex < 0 {
		mlog.Error("超過資料行數, iRowIndex:%v , 錯誤欄位:%v", iRowIndex, strColName)
		return -1
	}

	pRet := t.GetValueByColName(iRowIndex, strColName)
	if pRet == nil {
		return -1
	}

	return pRet.(int64)
}

func (t *RecordTable) GetValueByColNameBool(iRowIndex int, strColName string) bool {
	if strColName == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return false
	}

	if iRowIndex >= len(t.Rows) || iRowIndex < 0 {
		mlog.Error("超過資料行數, iRowIndex:%v , 錯誤欄位:%v", iRowIndex, strColName)
		return false
	}

	pRet := t.GetValueByColName(iRowIndex, strColName)
	if pRet == nil {
		return false
	}

	return pRet.(bool)
}

func (t *RecordTable) GetValueByColNameString(iRowIndex int, strColName string) string {
	if strColName == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return ""
	}

	if iRowIndex >= len(t.Rows) || iRowIndex < 0 {
		mlog.Error("超過資料行數, iRowIndex:%v , 錯誤欄位:%v", iRowIndex, strColName)
		return ""
	}

	pRet := t.GetValueByColName(iRowIndex, strColName)
	if pRet == nil {
		return ""
	}

	return pRet.(string)
}

// string to time
func (t *RecordTable) GetValueByColNameTime(iRowIndex int, strColName string) time.Time {
	timeStr := time.Now().Format("2006-01-02 15:04:05")
	formatTime, _ := time.Parse("2006-01-02 15:04:05", timeStr)

	if strColName == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return formatTime
	}

	if iRowIndex >= len(t.Rows) || iRowIndex < 0 {
		mlog.Error("超過資料行數, iRowIndex:%v , 錯誤欄位:%v", iRowIndex, strColName)
		return formatTime
	}

	pRet := t.GetValueByColName(iRowIndex, strColName)
	return pRet.(time.Time)
}

func (t *RecordTable) ForEachRow(f func(r RecordRow)) {
	for _, tr := range t.Rows {
		f(tr)
	}
}

func (t *RecordTable) GetSingleValue(name string) interface{} {
	if name == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	return t.GetValueByColName(0, name)
}

func (t *RecordTable) GetSingleValueInt32(name string) int32 {
	if name == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return -1
	}

	pRet := t.GetSingleValue(name)
	if pRet == nil {
		return -1
	}

	return int32(pRet.(int64))
}

func (t *RecordTable) GetSingleValueInt64(name string) int64 {
	if name == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return -1
	}

	pRet := t.GetSingleValue(name)
	if pRet == nil {
		return -1
	}

	return pRet.(int64)
}

func (t *RecordTable) GetSingleValueBool(name string) bool {
	if name == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return false
	}

	pRet := t.GetSingleValue(name)
	if pRet == nil {
		return false
	}

	return pRet.(bool)
}

func (t *RecordTable) GetSingleValueString(name string) string {
	if name == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return ""
	}

	pRet := t.GetSingleValue(name)
	if pRet == nil {
		return ""
	}

	return pRet.(string)
}

func (rs RecordSet) GetReturnValue() int64 {
	rlen := len(rs)
	if rlen < 1 {
		data := fmt.Sprintf("RecordSet 長度為 %d", rlen)
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return -1
	}

	return rs[rlen-1].Rows[0][0].(int64)
}

func (rs RecordSet) GetOutputParamValues() RecordRow {
	rlen := len(rs)
	if rlen < 2 {
		data := fmt.Sprintf("RecordSet 長度為 %d", rlen)
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	return rs[rlen-2].Rows[0]
}

func (rs RecordSet) GetOutputParamValue(pname string) interface{} {
	if pname == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	rlen := len(rs)
	if rlen < 2 {
		data := fmt.Sprintf("RecordSet 長度為 %d", rlen)
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return nil
	}

	rt := rs[rlen-2]

	return rt.GetValueByColName(0, pname)
}

func (rs RecordSet) GetOutputParamValueInt32(pname string) int32 {
	if pname == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return -1
	}

	pRet := rs.GetOutputParamValue(pname)
	if pRet == nil {
		return -1
	}

	return int32(pRet.(int64))
}

func (rs RecordSet) GetOutputParamValueInt64(pname string) int64 {
	if pname == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return -1
	}

	pRet := rs.GetOutputParamValue(pname)
	if pRet == nil {
		return -1
	}

	return pRet.(int64)
}

func (rs RecordSet) GetOutputParamValueBool(pname string) bool {
	if pname == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return false
	}

	pRet := rs.GetOutputParamValue(pname)
	if pRet == nil {
		return false
	}

	return pRet.(bool)
}

func (rs RecordSet) GetOutputParamValueString(pname string) string {
	if pname == "" {
		data := fmt.Sprintf("傳入的欄位名稱為空字串")
		mlog.NewDefaultLogger().SetCallDepLen(7).SetData(data).Error("SP")
		return ""
	}

	pRet := rs.GetOutputParamValue(pname)
	if pRet == nil {
		return "'"
	}

	return pRet.(string)
}

//结果集长度
//返回值以select方式返回.输出参数也会作为结果集返回.调用方需要根据是否使用了输出参数来判断结果集的长度
func (rs RecordSet) GetRetTableCount() int {
	return len(rs) - 1
}
