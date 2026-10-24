package slot

import "testing"
import "example.invalid/ServerTeam/Servers/pkg/slot/protodata"
import "example.invalid/golang/protobuf/proto"

func TestBackgroundResult(t *testing.T) {

	bgr := new(BackgroundResult)

	bgr.AddDistribution(&protodata.ReelBgDistribution{
		ReelNum: proto.Uint32(1),
		BgEnum:  proto.String("bg0"),
	})

	bgr.AddDistribution(&protodata.ReelBgDistribution{
		ReelNum: proto.Uint32(2),
		BgEnum:  proto.String("bg1"),
	})

	bgr.AddDistribution(&protodata.ReelBgDistribution{
		ReelNum: proto.Uint32(3),
		BgEnum:  proto.String("bg2"),
	})

	bgr.AddDistribution(&protodata.ReelBgDistribution{
		ReelNum: proto.Uint32(4),
		BgEnum:  proto.String("bg3"),
	})

	bgr.AddDistribution(&protodata.ReelBgDistribution{
		ReelNum: proto.Uint32(5),
		BgEnum:  proto.String("bg4"),
	})

	t.Log(bgr)
}
