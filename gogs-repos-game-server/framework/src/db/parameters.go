package db

import "mlog"

//参数数组
type Parameters struct {
	innerParameters []*SqlParameter
}

//创建Sql参数集合
func NewSqlParameters() *Parameters {
	ps := new(Parameters)

	return ps
}

//添加参数
func (ps *Parameters) AddParameter(p *SqlParameter) *Parameters {
	if p == nil {
		mlog.Error("傳入的參數為 nil")
		return  nil
	}

	ps.innerParameters = append(ps.innerParameters, p)

	return ps
}

//添加int参数
func (ps *Parameters) AddIntInput(name string, value int32) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlIntParameter(name, value, 4)

	ps.AddParameter(p)

	return ps
}

//添加int输出参数
func (ps *Parameters) AddIntOutput(name string, value int32) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlOutputParameter(name, value, SqlDbType_Int, 4)

	ps.AddParameter(p)

	return ps
}

//添加tinyint参数
func (ps *Parameters) AddTinyIntInput(name string, value byte) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlParameter(name, value, SqlDbType_Tinyint, ParamDirection_Input, 1)

	ps.AddParameter(p)

	return ps
}

//添加tinyint输出参数
func (ps *Parameters) AddTinyIntOutput(name string, value byte) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlOutputParameter(name, value, SqlDbType_Tinyint, 1)

	ps.AddParameter(p)

	return ps
}

//添加smallint参数
func (ps *Parameters) AddSmallIntInput(name string, value int16) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlParameter(name, value, SqlDbType_Smallint, ParamDirection_Input, 2)

	ps.AddParameter(p)

	return ps
}

//添加smallint输出参数
func (ps *Parameters) AddSmallIntOutput(name string, value int16) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlOutputParameter(name, value, SqlDbType_Smallint, 2)

	ps.AddParameter(p)

	return ps
}

//添加bigint参数
func (ps *Parameters) AddBigIntInput(name string, value int64) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlParameter(name, value, SqlDbType_Bigint, ParamDirection_Input, 8)

	ps.AddParameter(p)

	return ps
}

//添加bigint输出参数
func (ps *Parameters) AddBigIntOutput(name string, value int64) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlOutputParameter(name, value, SqlDbType_Bigint, 8)

	ps.AddParameter(p)

	return ps
}

//添加varchar参数
func (ps *Parameters) AddVarcharInput(name string, value string, size int32) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlParameter(name, value, SqlDbType_Varchar, ParamDirection_Input, size)

	ps.AddParameter(p)

	return ps
}

//添加varchar输出参数
func (ps *Parameters) AddVarcharOutput(name string, value string) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlOutputParameter(name, value, SqlDbType_Varchar, 256)

	ps.AddParameter(p)

	return ps
}

//添加varchar输出参数
func (ps *Parameters) AddVarcharSizeOutput(name string, value string, size int32) *Parameters {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return  nil
	}

	p := NewSqlOutputParameter(name, value, SqlDbType_Varchar, size)

	ps.AddParameter(p)

	return ps
}

//输出参数数量
func (ps *Parameters) OutputParamCount() int {
	cn := 0

	for _, p := range ps.innerParameters {
		if p.Direction == ParamDirection_Output {
			cn++
		}
	}

	return cn
}
