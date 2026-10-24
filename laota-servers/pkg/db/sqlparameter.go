package db

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

const (
	ParamDirection_Input = iota
	ParamDirection_Output
)

const (
	SqlDbType_Varchar = iota
	SqlDbType_NVarchar
	SqlDbType_Int
	SqlDbType_Tinyint
	SqlDbType_Bigint
	SqlDbType_Smallint
	SqlDbType_Char
	SqlDbType_NChar
	SqlDbType_Decimal
	SqlDbType_DateTime
)

//sql参数
type SqlParameter struct {
	Name      string      //参数名
	Value     interface{} //参数值
	DbType    int32       //参数类型
	Direction byte        //方向
	Size      int32
}

//获取参数类型描述字符串
func (p *SqlParameter) GetParamType() string {
	ptype := ""

	if p.DbType == SqlDbType_Bigint {
		ptype = "bigint"
	} else if p.DbType == SqlDbType_Char {
		ptype = fmt.Sprintf("char(%d)", p.Size)
	} else if p.DbType == SqlDbType_DateTime {
		ptype = "datetime"
	} else if p.DbType == SqlDbType_Decimal {
		ptype = "decimal(18,2)"
	} else if p.DbType == SqlDbType_Int {
		ptype = "int"
	} else if p.DbType == SqlDbType_NChar {
		ptype = fmt.Sprintf("nchar(%d)", p.Size)
	} else if p.DbType == SqlDbType_NVarchar {
		ptype = fmt.Sprintf("nvarchar(%d)", p.Size)
	} else if p.DbType == SqlDbType_Smallint {
		ptype = "smallint"
	} else if p.DbType == SqlDbType_Tinyint {
		ptype = "tinyint"
	} else if p.DbType == SqlDbType_Varchar {
		ptype = fmt.Sprintf("varchar(%d)", p.Size)
	}

	return ptype
}

//获取参数值描述字符串
func (p *SqlParameter) GetParameterValue() string {
	pvalue := ""

	if p.DbType == SqlDbType_Bigint || p.DbType == SqlDbType_Decimal || p.DbType == SqlDbType_Int || p.DbType == SqlDbType_Smallint || p.DbType == SqlDbType_Tinyint {
		pvalue = fmt.Sprintf("%v", p.Value)
	} else {
		pvalue = fmt.Sprintf("'%v'", p.Value)
	}

	return pvalue
}

//创建sql参数
func NewSqlParameter(name string, value interface{}, dbtype int32, direction byte, size int32) *SqlParameter {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return nil
	}

	p := new(SqlParameter)
	p.Name = name
	p.Value = value
	p.DbType = dbtype
	p.Direction = direction
	p.Size = size

	return p
}

//创建sql输入参数
func NewSqlInputParameter(name string, value interface{}, dbtype int32, size int32) *SqlParameter {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return nil
	}

	p := new(SqlParameter)
	p.Name = name
	p.Value = value
	p.DbType = dbtype
	p.Direction = ParamDirection_Input
	p.Size = size

	return p
}

//创建sql输出参数
func NewSqlOutputParameter(name string, value interface{}, dbtype int32, size int32) *SqlParameter {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return nil
	}

	p := new(SqlParameter)
	p.Name = name
	p.Value = value
	p.DbType = dbtype
	p.Direction = ParamDirection_Output
	p.Size = size

	return p
}

//创建sql varchar参数
func NewSqlVarcharParameter(name string, value string, size int32) *SqlParameter {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return nil
	}

	p := new(SqlParameter)
	p.Name = name
	p.Value = value
	p.Direction = ParamDirection_Input
	p.DbType = SqlDbType_Varchar
	p.Size = size

	return p
}

//创建sql int类型参数
func NewSqlIntParameter(name string, value int32, size int32) *SqlParameter {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return nil
	}

	p := new(SqlParameter)
	p.Name = name
	p.Value = value
	p.Direction = ParamDirection_Input
	p.DbType = SqlDbType_Int
	p.Size = size

	return p
}

//创建sql datetime类型参数
func NewSqlDateTimeParameter(name string, value string, size int32) *SqlParameter {
	if name == "" {
		mlog.Error("傳入的欄位名稱為空字串")
		return nil
	}

	p := new(SqlParameter)
	p.Name = name
	p.Value = value
	p.Direction = ParamDirection_Input
	p.DbType = SqlDbType_DateTime
	p.Size = size

	return p
}
