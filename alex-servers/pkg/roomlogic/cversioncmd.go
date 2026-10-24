package roomlogic

import (
	"bytes"
	"fmt"

	"example.invalid/golang/protobuf/proto"
)

type CVersionCmd struct {
	verprovider GameVerProvider
}

//创建机器人命令
func NewCVersionCmd(verprovider GameVerProvider) *CVersionCmd {
	cvcmd := new(CVersionCmd)
	cvcmd.verprovider = verprovider

	return cvcmd
}

func (cv *CVersionCmd) Name() string {
	return "cversion"
}

func (cv *CVersionCmd) Help() string {
	return "客戶端版本號，eg:cversion list | cversion ios 1.0.0 | cversion android 1.0.0"
}

func (cv *CVersionCmd) Exec(args []string) string {
	if len(args) < 1 {
		return "error,seeref usage"
	}

	cmdname := args[0]

	if cmdname == "list" {
		var buf bytes.Buffer
		buf.WriteString(fmt.Sprintf("===客戶端版本資訊===\nIOS Version：%s\nAndroid Version：%s", cv.verprovider.GetVer().GetIOSVersion(), cv.verprovider.GetVer().GetAndroidVersion()))
		return buf.String()
	}

	if cmdname == "ios" {
		fmt.Println(len(args))
		fmt.Println(args[1])
		if len(args) > 1 && args[1] != "" {
			cv.verprovider.(*RPCGameVerProvider).gsreg.gameServerRegRet.IOSVersion = proto.String(args[1])
			return "變更版號成功"
		}
	}

	if cmdname == "android" {
		if len(args) > 1 && args[1] != "" {
			cv.verprovider.(*RPCGameVerProvider).gsreg.gameServerRegRet.AndroidVersion = proto.String(args[1])
			return "變更版號成功"
		}
	}

	return "error,seeref usage"
}
