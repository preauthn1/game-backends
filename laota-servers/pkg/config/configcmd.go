package config

import (
	"fmt"
)

//配置命令
type ConfigCmd struct {
	configgroup *ConfigGroup
}

//创建命令对象
func NewConfigCmd(configgroup *ConfigGroup) *ConfigCmd {
	cmd := new(ConfigCmd)
	cmd.configgroup = configgroup

	return cmd
}

func (cmd *ConfigCmd) Name() string {
	return "config"
}

func (cmd *ConfigCmd) Help() string {
	return "配置数据，eg:config display 配置文件名 | config getsection 配置文件名 节点名 | config getvalue 配置文件名 节点名 键名"
}

func (cmd *ConfigCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error,seeref usage."
	}

	cmdname := args[0]
	switch cmdname {
	case "display":
		if len(args) == 1 {
			return cmd.configgroup.String()
		} else {
			cname := args[1]
			cfg := cmd.configgroup.GetConfig(cname)

			if cfg == nil {
				return "配置文件不存在"
			}

			return cfg.String()
		}
	case "getsection":
		if len(args) != 3 {
			return "error,参数格式不正确.see useage"
		}
		fname := args[1]
		sname := args[2]

		return fmt.Sprintf("%v", cmd.configgroup.GetConfigSection(fname, sname))
	case "getvalue":
		if len(args) != 4 {
			return "error,参数格式不正确.see useage"
		}
		fname := args[1]
		sname := args[2]
		kname := args[3]

		return cmd.configgroup.GetConfigValue(fname, sname, kname)
	default:
		return "error,seeref usage."
	}
}
