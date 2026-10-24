package command

import (
	"bufio"
	"os"
	"strings"
	"sync"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

var cmg Group
var runing chan bool
var wgCmd sync.WaitGroup

// AddCommand 增加命令
func AddCommand(c Command) error {
	return cmg.AddCommand(c)
}

func init() {
	cmg = make(map[string]Command)
	cmg.AddCommand(new(HelpCommand))
	cmg.AddCommand(new(ClsCommand))
	cmg.AddCommand(new(QuitCommand))
	cmg.AddCommand(new(TitleCommand))
	cmg.AddCommand(new(LogCommand))
	cmg.AddCommand(new(PProfCommand))
	cmg.AddCommand(new(VersionCommand))
	cmg.AddCommand(new(DebugCommand))
}

// Run 运行命令行
func Run() {
	mlog.System("Command runing...")
	wgCmd.Add(1)

	go func() {
		defer util.PanicErrStack()

		for {
			reader := bufio.NewReader(os.Stdin)
			data, _, _ := reader.ReadLine()
			sline := string(data)

			sline = strings.ToLower(sline)

			select {
			case <-runing:
				break
			default:
				sarr := strings.Split(sline, " ")
				cname := sarr[:1][0]
				cargs := sarr[1:]

				cmdret := cmg.Exec(cname, cargs)

				if cmdret != "" {
					mlog.Printf(cmdret)
				}
			}
		}

		wgCmd.Done()
	}()

}

// Stop 停止命令行
func Stop() {
	runing <- false
	go func() {
		defer util.PanicErrStack()

		wgCmd.Wait()

		example.invalid("Command stop...")
	}()
}
