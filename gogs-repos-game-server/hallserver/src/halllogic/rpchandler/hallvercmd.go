package rpchandler

import (
	"bytes"
	"fmt"
	"strconv"
	"halllogic/hallutil"
)

type HallVerCmd struct {
	hallVerProvider hallutil.HallVerProvider
}

//创建机器人命令
func NewHallVerCmd(verHandler hallutil.HallVerProvider) *HallVerCmd {
	cvcmd := new(HallVerCmd)
	cvcmd.hallVerProvider = verHandler

	return cvcmd
}

func (hv *HallVerCmd) Name() string {
	return "hallver"
}

func (hv *HallVerCmd) Help() string {
	return "客戶端版本號，eg:hallver <areaid>"
}

func (hv *HallVerCmd) Exec(args []string) string {
	if len(args) < 1 {
		return "error,seeref usage"
	}
	areaId, err := strconv.Atoi(args[0])

	if err == nil {
		var buf bytes.Buffer
		ver := hv.hallVerProvider.GetVer(nil, nil, int64(areaId))
		buf.WriteString(fmt.Sprintf("===大廳版本資訊===\nIOS Version：%s\nAndroid Version：%s\n", ver.GetIOSVersion(), ver.GetAndroidVersion()))
		buf.WriteString(fmt.Sprintf("===大廳版本資訊===\nIOS Version2：%s\nAndroid Version2：%s\n", ver.GetIOSVersion2(), ver.GetAndroidVersion2()))
		buf.WriteString(fmt.Sprintf("===配置資訊===\nAreaConfig：%s\n", ver.GetAreaConfig()))
		buf.WriteString(fmt.Sprintf("BillDomain：%s\n", ver.GetBillDomain()))
		buf.WriteString(fmt.Sprintf("ClubDomain：%s\n", ver.GetClubDomain()))
		buf.WriteString(fmt.Sprintf("CustomerDomain：%s\n", ver.GetCustomerDomain()))
		buf.WriteString(fmt.Sprintf("UpdateDomain：%s\n", ver.GetUpdateDomain()))
		return buf.String()
	}

	return "error,seeref usage"
}
