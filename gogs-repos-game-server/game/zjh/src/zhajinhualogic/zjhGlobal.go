package zhajinhualogic

import (
	"conf"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"io/ioutil"
	"os"
)

//--------------------------------------------------------------------------
// 所有通用的 Global 變數或程序均放置於此
//--------------------------------------------------------------------------

//读取proto配置文件
func ReadProtoConfig(fName string, unmarshFunc func(data []byte) (proto.Message, error)) (proto.Message, error) {
	protoConfigFolderName := "protoconfig"

	if conf.TestMode {
		protoConfigFolderName = "../../protoconfig"
	}

	fpath := fmt.Sprintf("%s/%s", protoConfigFolderName, fName)
	fi, err := os.Open(fpath)

	if err != nil {
		return nil, err
	}
	defer fi.Close()

	fd, err := ioutil.ReadAll(fi)
	if err != nil {
		return nil, err
	}

	return unmarshFunc(fd)
}
