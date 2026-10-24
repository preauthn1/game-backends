package db

import (
	"strings"
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

	colidx := -1
	name = strings.ToLower(name)

	if rowindex >= len(t.Rows) || rowindex < 0 {
		return nil
	}

	for i, n := range t.Cols {
		if strings.ToLower(n) == name {
			colidx = i
			break
		}
	}

	if colidx == -1 {
		return nil
	}

	if colidx >= len(t.Cols) {
		return nil
	}

	return t.Rows[rowindex][colidx]
}

func (t *RecordTable) ForEachRow(f func(r RecordRow)) {
	for _, tr := range t.Rows {
		f(tr)
	}
}

func (t *RecordTable) GetSingleValue(name string) interface{} {
	return t.GetValueByColName(0, name)
}

func (t *RecordTable) GetSingleValueInt32(name string) int32 {
	v := int32(t.GetSingleValue(name).(int64))

	return v
}

func (t *RecordTable) GetSingleValueInt64(name string) int64 {
	v := t.GetSingleValue(name).(int64)

	return v
}

func (t *RecordTable) GetSingleValueBool(name string) bool {
	v := t.GetSingleValue(name).(bool)

	return v
}

func (t *RecordTable) GetSingleValueString(name string) string {
	v := t.GetSingleValue(name).(string)

	return v
}

func (rs RecordSet) GetReturnValue() int64 {
	rlen := len(rs)

	return rs[rlen-1].Rows[0][0].(int64)
}

func (rs RecordSet) GetOutputParamValues() RecordRow {
	rlen := len(rs)
	return rs[rlen-2].Rows[0]
}

func (rs RecordSet) GetOutputParamValue(pname string) interface{} {
	rlen := len(rs)
	rt := rs[rlen-2]

	return rt.GetValueByColName(0, pname)
}

//结果集长度
//返回值以select方式返回.输出参数也会作为结果集返回.调用方需要根据是否使用了输出参数来判断结果集的长度
func (rs RecordSet) GetRetTableCount() int {
	return len(rs) - 1
}
