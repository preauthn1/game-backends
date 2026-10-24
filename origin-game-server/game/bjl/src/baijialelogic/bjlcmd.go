package baijialelogic

import (
	//	"mlog"
	"strconv"
	"util"
)

type bjlCmd struct {
	bjllogic *BaiJialeLogic
}

//创建用户命令
func NewBjlCmd(bjl *BaiJialeLogic) *bjlCmd {
	cmd := new(bjlCmd)
	cmd.bjllogic = bjl

	return cmd
}

func (bc *bjlCmd) Name() string {
	return "syspool"
}

func (bc *bjlCmd) Help() string {
	return "百家乐.usage:syspool change (talbeid) (Money)/syspool listpool (talbeid)/syspool listpool/syspool zhushui (talbeid) (int32)"
}

func (bc *bjlCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error.seeref usage"
	}

	cmdname := args[0]

	switch cmdname {
	case "change":
		if len(args) < 2 {
			return "error.eg kick 10001"
		}
		if len(args) == 3 {
			if !util.IsNumber(args[1]) {
				return "error.房间必须为数字"
			}

			if !util.IsNumber(args[2]) {
				return "error.改变后的钱必须为数字"
			}

			tableno, _ := strconv.Atoi(args[1])
			money, _ := strconv.Atoi(args[2])
			t := bc.bjllogic.GetTable().Room.GetTableByNo(int32(tableno))
			bjl := t.GameLogic.(*BaiJialeLogic)
			bjl.changeSysPool(int64(money))
		} else {
			money, _ := strconv.Atoi(args[1])
			for i := 0; i < len(bc.bjllogic.GetTable().Room.Tables); i++ {
				t := bc.bjllogic.GetTable().Room.Tables[i]
				bjl := t.GameLogic.(*BaiJialeLogic)
				bjl.changeSysPool(int64(money))
			}
		}

		return "change success"
	case "listpool":
		if len(args) == 2 {
			if !util.IsNumber(args[1]) {
				return "error.房间必须为数字"
			}

			tableno, _ := strconv.Atoi(args[1])
			t := bc.bjllogic.GetTable().Room.GetTableByNo(int32(tableno))
			bjl := t.GameLogic.(*BaiJialeLogic)
			bjl.checkSysPool()
		} else {
			alltable := bc.bjllogic.GetTable().Room.Tables
			for _, t := range alltable {
				bjl := t.GameLogic.(*BaiJialeLogic)
				bjl.checkSysPool()
			}
		}

		return "listpool success"
	case "zhushui":
		if len(args) < 2 {
			return "error.eg kick 10001"
		}
		if len(args) == 3 {
			if !util.IsNumber(args[1]) {
				return "error.房间必须为数字"
			}

			if !util.IsNumber(args[2]) {
				return "error.改变后的注水百分比必须为数字"
			}

			tableno, _ := strconv.Atoi(args[1])
			zhushui, _ := strconv.Atoi(args[2])
			t := bc.bjllogic.GetTable().Room.GetTableByNo(int32(tableno))
			bjl := t.GameLogic.(*BaiJialeLogic)
			bjl.changeZhuShui(int32(zhushui))
		} else {
			zhushui, _ := strconv.Atoi(args[1])
			for i := 0; i < len(bc.bjllogic.GetTable().Room.Tables); i++ {
				t := bc.bjllogic.GetTable().Room.Tables[i]
				bjl := t.GameLogic.(*BaiJialeLogic)
				bjl.changeZhuShui(int32(zhushui))
			}

		}

		return "zhushui success"
	}

	return "nil"
}
