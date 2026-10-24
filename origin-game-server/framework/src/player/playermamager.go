package player

import (
	"command"
	"errors"
	"msg"
	"msgserver"
	"network"
	"sync"

	//"sync"
)

//用户管理器
type PlayerManager struct {
	users     map[string]*Player
	msgserver *msgserver.MsgServer
	sync.RWMutex
}

//创建用户管理器对象
func NewPlayerManager(svr *msgserver.MsgServer) *PlayerManager {
	pm := new(PlayerManager)
	pm.users = make(map[string]*Player)
	pm.msgserver = svr
	command.AddCommand(NewPlayerCmd(pm, svr))

	return pm
}

//添加用户
func (pm *PlayerManager) SetPlayer(ur *Player) {
	pm.Lock()
	defer pm.Unlock()
	pm.users[ur.Conn.ConnID()] = ur
}

//删除用户
func (pm *PlayerManager) RemovePlayer(connid string) {
	pm.Lock()
	defer pm.Unlock()
	delete(pm.users, connid)
}

//根据连接获取用户
func (pm *PlayerManager) GetPlayerByConnID(connid string) *Player {
	pm.Lock()
	defer pm.Unlock()
	return pm.users[connid]
}

//根据用户ID获取用户信息
func (pm *PlayerManager) GetPlayerByUserID(userid int32) *Player {
	pm.Lock()
	defer pm.Unlock()
	for _, u := range pm.users {
		if u.UserID == userid {
			return u
		}
	}

	return nil
}

//已登录用户
func (pm *PlayerManager) LogonPlayer() []*Player {
	pm.Lock()
	defer pm.Unlock()
	urs := make([]*Player, 0)
	for _, u := range pm.users {
		if !u.IsGuest() {
			urs = append(urs, u)
		}
	}

	return urs
}

//未登录用户
func (pm *PlayerManager) UnLogonPlayer() []*Player {
	pm.Lock()
	defer pm.Unlock()
	urs := make([]*Player, 0)
	for _, u := range pm.users {
		if u.IsGuest() {
			urs = append(urs, u)
		}
	}

	return urs
}

//用户数量
func (pm *PlayerManager) PlayerCount() int {
	pm.Lock()
	defer pm.Unlock()
	return len(pm.users)
}

//未登录用户数
func (pm *PlayerManager) GuestCount() int {
	return len(pm.UnLogonPlayer())
}

//已登录用户数
func (pm *PlayerManager) UserCount() int {
	return len(pm.LogonPlayer())
}

func (pm *PlayerManager) Range(f func(p *Player)) {
	pm.Lock()
	defer pm.Unlock()
	for _, pr := range pm.users {
		f(pr)
	}
}

func (pm *PlayerManager) SetPlayerData(connid string, setfunc func(pr *Player)) error {
	pm.Lock()
	defer pm.Unlock()
	pr, ok := pm.users[connid]

	if !ok {
		return errors.New("连接不存在")
	}
	setfunc(pr)

	return nil

}

//设置用户断线
func (pm *PlayerManager) SetPlayerOffLine(connid string) {
	pm.Lock()
	defer pm.Unlock()
	pr, ok := pm.users[connid]

	if !ok {
		return
	}

	if pr != nil {
		pr.IsOnline = false
	}
}

//设置用户断线重回
func (pm *PlayerManager) SetPlayerRecover(newconnid string, userid int32) {
	pm.Lock()
	defer pm.Unlock()
	var exid string
	var expr *Player

	exid = ""
	expr = nil
	for id, pr := range pm.users {
		if pr.UserID == userid && pr.IsOffLine() {
			exid = id
			expr = pr
			break
		}
	}

	if exid != "" && expr != nil {
		newplayer, ok := pm.users[newconnid]
		if ok {
			newplayer.UserID = expr.UserID
			newplayer.Cer = expr.Cer
			newplayer.HDCode = expr.HDCode
			newplayer.HDType = expr.HDType
			newplayer.UserData = expr.UserData
			newplayer.Conn.ConnTime = expr.Conn.ConnTime

			//fmt.Printf("set newplayer :%v\r\n", newplayer)
		}

		//删除旧的用户信息
		delete(pm.users, exid)

	}
}

//清除用户信息
func (pm *PlayerManager) ClearPlayerInfo(connid string) {
	pm.Lock()
	defer pm.Unlock()
	expr, ok := pm.users[connid]
	if ok {
		expr.UserData = nil
		expr.UserID = GuestUserID
		expr.Cer = GuestCerData
		expr.HDType = 0
		expr.HDCode = ""
	}
}

func (pm *PlayerManager) SendMessage(clt *network.ConnInfo, msg *msg.Message) {
	pm.msgserver.SendMessage(clt, msg)
}