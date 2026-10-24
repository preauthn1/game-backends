package module

import (
	"fmt"
)

//模块命令
type ModuleCmd struct {
	moduleGroup *ModuleGroup
}

//创建模块命令对象
func NewModuleCmd(moduleGroup *ModuleGroup) *ModuleCmd {
	cmd := new(ModuleCmd)
	cmd.moduleGroup = moduleGroup

	return cmd
}

func (cmd *ModuleCmd) Name() string {
	return "module"
}

func (cmd *ModuleCmd) Help() string {
	return "模块管理，eg:moudle list | module stop *** "
}

func (cmd *ModuleCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error,usage:moudle list | module stop ***"
	}

	cmdname := args[0]
	switch cmdname {
	case "list":
		str := ""
		for _, m := range cmd.moduleGroup.modules {
			str += m.Name()
			str += "\r\n"
		}

		return str
	case "stop":
		if len(args) != 2 {
			return "error,usage:module stop ***"
		}
		mname := args[2]

		um := cmd.moduleGroup.UnLoad(mname)

		if um == nil {
			return "error,模块名称不存在"
		}

		return fmt.Sprintf("module %s stop.", mname)

	default:
		return "error,usage:module list"
	}
}
