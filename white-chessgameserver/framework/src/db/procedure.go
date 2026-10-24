package db

import (
	"fmt"
)

//存储过程
type Procedure struct {
	Name       string //存储过程名称
	Parameters *Parameters
}

//创建存储过程
func NewProcedure(name string, ps *Parameters) *Procedure {
	p := new(Procedure)
	p.Name = name
	p.Parameters = ps

	return p
}

//获取sql字符串
func (p *Procedure) GetSqlString() string {
	sqlstr := ""

	//输出参数申明
	for idx, pr := range p.Parameters.innerParameters {
		if pr.Direction == ParamDirection_Output {
			sqlstr = sqlstr + fmt.Sprintf("declare @p%d %s;", idx, pr.GetParamType())

			if pr.Value != nil {
				sqlstr += fmt.Sprintf("set @p%d=%s;", idx, pr.GetParameterValue())
			}

		}
	}

	sqlstr += fmt.Sprintf("declare @intProcRet int; exec @intProcRet = %s ", p.Name)
	end := len(p.Parameters.innerParameters)
	for idx, pr := range p.Parameters.innerParameters {

		if pr.Direction == ParamDirection_Input {
			sqlstr += fmt.Sprintf("@%s=%s", pr.Name, pr.GetParameterValue())
		} else {
			sqlstr += fmt.Sprintf("@%s=@p%d output", pr.Name, idx)
		}

		if idx != end-1 {
			sqlstr += ","
		}
	}

	//输出参数申明
	optsize := p.Parameters.OutputParamCount()
	oidx := 0
	if optsize > 0 {
		sqlstr += ";select "

		for idx, pr := range p.Parameters.innerParameters {
			if pr.Direction == ParamDirection_Output {
				sqlstr += fmt.Sprintf("@p%d %s", idx, pr.Name)
				oidx++

				if oidx != optsize {
					sqlstr += ","
				}
			}
		}
	}

	sqlstr += ";select @intProcRet;"
	return sqlstr
}
