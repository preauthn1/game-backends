package command

import (
	"fmt"
	"os"
	"runtime/pprof"
	"strconv"
	"time"
)

//性能分析命令
type PProfCommand struct {
}

func (pcmd *PProfCommand) Name() string {
	return "pprof"
}

func (pcmd *PProfCommand) Help() string {
	return "追蹤pprof，eg:pprof cpu 10(moniter second) | mem "
}

func (pcmd *PProfCommand) Exec(args []string) string {
	if len(args) == 0 {
		return "error,seeref usage."
	}
	arg := args[0]

	if arg == "mem" {
		fm, err := os.Create("mem.dat")
		if err != nil {
			return fmt.Sprintf("create mem dump file error.%v", err)
		}
		pprof.WriteHeapProfile(fm)
		fm.Close()

		return "create mem dump file success.file name:mem.dat."
	}

	if arg == "cpu" {
		if len(args) != 2 {
			return "error,seeref usage."
		}

		monitertime := args[1]
		mt, err := strconv.Atoi(monitertime)
		if err != nil {
			return "error,seeref usage."
		}

		fm, err := os.Create("cpu.dat")
		if err != nil {
			return fmt.Sprintf("create cpu dump file error.%v", err)
		}
		pprof.StartCPUProfile(fm)

		go func() {
			time.Sleep(time.Duration(time.Second) * time.Duration(mt))

			pprof.StopCPUProfile()
			fm.Close()

			fmt.Println("cpu dump watch file close.")

		}()

		return fmt.Sprintf("create cpu dump watch file success.file name:cpu.dat.please wait %d sec", mt)
	}

	return "error.seeref usage."
}
