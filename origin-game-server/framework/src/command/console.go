package command

import (
	"bufio"
	"mlog"
	"os"
	"strings"
	"sync"
	"util"
)

var cmg CommandGroup
var runing chan bool
var wgCmd sync.WaitGroup

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

//运行命令行
func Run() {
	mlog.System("command runing...")
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

//停止命令行
func Stop() {
	runing <- false
	go func() {
		defer util.PanicErrStack()

		wgCmd.Wait()

		example.invalid("command stop...")
	}()
}
