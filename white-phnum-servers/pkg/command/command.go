package command

import (
	"strconv"

	"example.invalid/ServerTeam/Servers/pkg/event"
)

//命令接口
type Command interface {
	Name() string              //名称
	Help() string              //说明
	Exec(args []string) string //执行命令
}

//外部命令
type ExternalCommand struct {
	name         string
	help         string
	cmdEvtServer *event.EventDispatcher
}

func (exc *ExternalCommand) Name() string {
	return exc.name
}

func (exc *ExternalCommand) Help() string {
	return exc.help
}

func (exc *ExternalCommand) Exec(args []string) string {
	evtArgs := event.NewEventArgs()

	if args != nil && len(args) > 0 {
		for i, a := range args {
			argname := "cmd_arg_" + strconv.Itoa(i)
			evtArgs[argname] = a
		}
	}
	evt := event.NewEvent(exc.name, evtArgs)

	ret := exc.cmdEvtServer.OnEvent(evt)
	return ret.ToString()
}

//创建外部命令对象
func NewExternalCommand(name, help string, evtServer *event.EventDispatcher) *ExternalCommand {
	exc := new(ExternalCommand)
	exc.name = name
	exc.help = help
	exc.cmdEvtServer = evtServer

	return exc
}
