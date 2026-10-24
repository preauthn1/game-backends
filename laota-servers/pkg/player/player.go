package player

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/network"
)

//设备类型
const (
	UnKnown = iota
	HDType_Android
	HDType_IOS
	HDType_PC
	HDType_H5
)

const (
	GuestUserID  = 0  //游客用户ID
	GuestCerData = "" //游客证书数据
)

//游戏玩家
type Player struct {
	Conn     *network.ConnInfo //连接信息
	UserID   int32             //用户ID
	Cer      string            //用户证书
	HDType   int32             //设备类型
	HDCode   string            //设备码
	UserData interface{}       //用户数据
	IsOnline bool              //是否保持连接
}

//创建游客对象
func NewGuest(conn *network.ConnInfo) *Player {
	player := new(Player)
	player.Conn = conn
	player.UserID = GuestUserID
	player.Cer = GuestCerData
	player.IsOnline = true

	return player
}

//是否游客
func (p *Player) IsGuest() bool {
	return p.UserID == GuestUserID && p.Cer == GuestCerData
}

func (p *Player) String() string {
	return fmt.Sprintf("userid:%d,cer:%s,hdtype:%d,hdcode:%s,conn:%v,userdata:%v,offline:%t", p.UserID, p.Cer, p.HDType, p.HDCode, p.Conn, p.UserData, p.IsOffLine())
}

func (p *Player) SimpleString() string {
	return fmt.Sprintf("userid:%d,cer:%s,hdtype:%d,hdcode:%s,conn:%v,offline:%v", p.UserID, p.Cer, p.HDType, p.HDCode, p.Conn, p.IsOffLine())
}

//用户是否断线
func (p *Player) IsOffLine() bool {
	return !p.IsOnline
}
