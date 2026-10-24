package db

import (
	"fmt"
	"testing"
)

func TestSqlParameter(t *testing.T) {
	ps := NewSqlParameters()
	ps.AddVarcharInput("chvName", "zhengrongw", 64)
	ps.AddIntOutput("intAge", 20)
	p := NewProcedure("prTest", ps)

	d := NewDatabase("server=0.0.0.0;database=GameCenter;user id=sa;password=[REDACTED];port=1933;encrypt=disable")

	ret, _ := d.ExecProc(p)

	fmt.Println(ret[0].GetValueByColName(1, "NickName"))

	for _, t := range ret {
		fmt.Println(t)
	}

	fmt.Println(ret.GetReturnValue())

}
