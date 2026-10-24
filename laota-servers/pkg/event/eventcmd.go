package event

import (
	"fmt"
)

//事件命令
type EventCmd struct {
	evtdispatcher *EventDispatcher
}

//创建事件命令对象
func NewEventCmd(evtdispatcher *EventDispatcher) *EventCmd {
	cmd := new(EventCmd)
	cmd.evtdispatcher = evtdispatcher

	return cmd
}

func (cmd *EventCmd) Name() string {
	return "event"
}

func (cmd *EventCmd) Help() string {
	return "事件处理器，eg:event stat | event listhandler"
}

func (cmd *EventCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error,seeref usage"
	}

	evtname := args[0]
	switch evtname {
	case "stat":
		etype := "async"
		if cmd.evtdispatcher.asyncEvents == nil {
			etype = "sync"
		}

		return fmt.Sprintf("hander:%d,type:%s:event channel len:%d", len(cmd.evtdispatcher.innerHandlers), etype, len(cmd.evtdispatcher.asyncEvents))
	case "listhandler":
		str := ""
		for _, h := range cmd.evtdispatcher.innerHandlers {
			str += h.Handler.HandlerName()
			str += ","
		}
		return str
	default:
		return "error,seeref usage."
	}

}
