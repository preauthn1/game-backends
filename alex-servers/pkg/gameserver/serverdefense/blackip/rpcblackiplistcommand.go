package blackip

import (
	"fmt"
)

//rpc ip 黑名单命令行
type RPCBlackIPListCommand struct {
	rpcipLst *RPCBlackIPList
}

//创建RPCIP黑名单命令行
func NewRPCBlackIPListCommand(rpcipLst *RPCBlackIPList) *RPCBlackIPListCommand {
	rpcipcmd := new(RPCBlackIPListCommand)
	rpcipcmd.rpcipLst = rpcipLst

	return rpcipcmd
}

func (exc *RPCBlackIPListCommand) Name() string {
	return "rpciplist"
}

func (exc *RPCBlackIPListCommand) Help() string {
	return "黑名單，eg:rpciplist list | find ipaddress | clear  "
}

func (exc *RPCBlackIPListCommand) Exec(args []string) string {
	if len(args) == 1 {
		if args[0] == "list" {
			str := ""

			exc.rpcipLst.innerLocker.Lock()
			for _, sip := range exc.rpcipLst.blackIPList {
				str = str + fmt.Sprintf("%s,", sip)
			}
			exc.rpcipLst.innerLocker.Unlock()

			return str
		} else if args[0] == "clear" {
			exc.rpcipLst.Clear()
			return "ip list clear."
		} else {
			return "error,seeref usage"
		}
	} else if len(args) == 2 {
		if args[0] == "find" {
			findip := args[1]
			find := false

			exc.rpcipLst.innerLocker.Lock()
			for _, sip := range exc.rpcipLst.blackIPList {
				if sip == findip {
					find = true
					break
				}
			}
			exc.rpcipLst.innerLocker.Unlock()

			if find {
				return fmt.Sprintf("ip %s in black list.", findip)
			} else {
				return fmt.Sprintf("ip %s not in black list.", findip)
			}

		} else {
			return "error,seeref usage"
		}
	} else {
		return "error,seeref usage"
	}

}
