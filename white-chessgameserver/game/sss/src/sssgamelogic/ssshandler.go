package sssgamelogic

import (
	"gamelogic"
	"mlog"
	"msg"
	"netproto"
	"poker"
	"sort"

	"example.invalid/golang/protobuf/proto"
)

//注册各阶段超时处理函数
func (s *SSSTable) regist() {
	//游戏开始发牌播放动画等
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepBegin, s.gameBeginTimeOut)
	//牌型确定超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepCardTypeChoose, s.CardTypeChooseTimeOut)
	//组牌超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepCombine, s.CombineCardTimeOut)
	//比牌开始动画
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepAnimotion, s.AnimotionBegin)
	//头墩比较超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepCompareH, s.CompareHTimeOut)
	//中墩比较超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepCompareB, s.CompareBTimeOut)
	//后墩比较超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepCompareF, s.CompareFTimeOut)
	//特殊牌型比较超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepCompareSpecial, s.CompareSpecialTimeOut)
	//打枪阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepUserGun, s.UserGunTimeOut)
	//结算超时阶段
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepSettlement, s.SettlementTimeOut)
	//游戏结束
	s.RegistCallBackFnc(netproto.SSSGameStep_SSSGameStepEnd, s.GameEnd)
}

//是否直接进入特殊牌型比较阶段
func (s *SSSTable) IsCompareDirect() bool {
	return false
	n := 0
	for _, v := range s.roles {
		if !v.act.isSpecial {
			n++
		}
	}

	if n >= s.GetCurrentGameUserCount() {
		return true
	}

	return false
}

//比牌开始动画
func (s *SSSTable) AnimotionBegin() {
	mlog.Debug("比牌开始动画")
	if s.IsCompareDirect() {
		mlog.Debug("跳过特殊比牌阶段")
		s.SetJump(netproto.SSSGameStep_SSSGameStepCompareSpecial)
	}

	if s.GetSpacialCount() >= int32(s.GetCurrentGameUserCount()) {
		s.SetJump(netproto.SSSGameStep_SSSGameStepCompareH)
	}
	s.stopLoop()
	s.animotionCur = s.RegisterAnimation(AnimotinBeginCompare, animotionTimeOut, s.ResumeLoop)
}

//头墩比牌超时回调
func (s *SSSTable) CompareHTimeOut() {
	mlog.Debug("比较头墩牌超时")
	s.CompareNormal()
	if s.GetSpacialCount() >= int32(s.GetCurrentGameUserCount()) {
		mlog.Debug("跳过此阶段")
		s.SetJump(netproto.SSSGameStep_SSSGameStepCompareB)
	} else {
		s.broadcastUserCards(netproto.SSSDunType_DunTypeHead)
		s.animotionCur = s.RegisterAnimation(AnimotionHeadComb, animotionTimeOut, s.ResumeLoop)
	}
	s.stopLoop()
}

//中墩比牌超时回调
func (s *SSSTable) CompareBTimeOut() {
	mlog.Debug("比较中墩牌超时")
	if s.GetSpacialCount() >= int32(s.GetCurrentGameUserCount()) {
		mlog.Debug("跳过此阶段")
		s.SetJump(netproto.SSSGameStep_SSSGameStepCompareF)
	} else {
		s.broadcastUserCards(netproto.SSSDunType_DunTypeBody)
		s.animotionCur = s.RegisterAnimation(AnimotionBodyComb, animotionTimeOut, s.ResumeLoop)
	}
	s.stopLoop()
}

//后墩比牌超时回调
func (s *SSSTable) CompareFTimeOut() {
	mlog.Debug("比较后墩牌超时")
	s.stopLoop()
	if s.GetSpacialCount() < int32(s.GetCurrentGameUserCount()) {
		s.broadcastUserCards(netproto.SSSDunType_DunTypeFoot)
		s.animotionCur = s.RegisterAnimation(AnimotionFootComb, animotionTimeOut, s.ResumeLoop)
	}
}

func (s *SSSTable) specialWinTree() {
	size := len(s.roles)
	for i := 0; i < size; i++ {
		if s.roles[i].act.isSpecial && len(s.roles[i].act.sctWinInfo) == 3 {
			s.roles[i].act.sctWin *= 2
			for j := 0; j < 3; j++ {
				r := s.getRoleInfoBySeat(s.roles[i].act.sctWinInfo[j].seatNo)
				r.act.sctLose -= s.roles[i].act.sctWinInfo[j].win
			}
		}
	}
}

//特殊牌型比较超时回调
func (s *SSSTable) CompareSpecialTimeOut() {
	mlog.Debug("比较特殊牌型阶段超时")
	if s.GetSpacialCount() > 0 {
		s.CompareSpeacil()
		s.CompareSpeacilAndNormal()
		s.specialWinTree()
		s.broadcastSpecialCompareResult()
		s.stopLoop()
		s.animotionCur = s.RegisterAnimation(AnimotionSpecialComb, 10, s.ResumeLoop)
		mlog.Debug("比较特殊牌型阶段超时")
	}
}

//打枪阶段
func (s *SSSTable) UserGunTimeOut() {
	mlog.Debug("进入打枪阶段")
	if !s.IsCompareDirect() {
		for _, v := range s.roles {
			if len(v.gun) > 0 {
				s.broadcastUserGun()
				s.stopLoop()
				s.animotionCur = s.RegisterAnimation(AnimotionGun, animotionTimeOut, s.ResumeLoop)
				break
			}
		}
	}
}

//结算超时回调
func (s *SSSTable) SettlementTimeOut() {
	mlog.Debug("进入结算阶段")
	s.CaculateOddsAndGun()
	s.broadCastSettlement()
	s.stopLoop()
	s.animotionCur = s.RegisterAnimation(AnimotionEnd, animotionTimeOut, s.ResumeLoop)
}

func (s *SSSTable) GameEnd() {
	s.GameEndFunc()
}

func cc(c1 *netproto.SCard, c2 *netproto.SCard) int32 {
	if c1.GetNumber() > c2.GetNumber() {
		return 1
	} else if c1.GetNumber() == c2.GetNumber() {
		return 0
	}
	return -1
}

func getSanTiao(c []*netproto.SCard) int32 {
	l := len(c)
	cardNumber1 := int32(0)
	count := 0
	for i := 0; i < l-2; i++ {
		if c[i].GetNumber() == c[i+1].GetNumber() {
			cardNumber1 = c[i].GetNumber()
			count++
		} else {
			count = 0
		}

		if count == 2 {
			break
		}
	}

	return cardNumber1
}

func getMaxDuiZi(c []*netproto.SCard) int32 {
	l := len(c)
	cardNumber1 := int32(0)
	for i := l - 1; i > 0; i-- {
		if c[i].GetNumber() == c[i-1].GetNumber() {
			cardNumber1 = c[i].GetNumber()
			break
		}
	}

	return cardNumber1
}

func getMinDuiZi(c []*netproto.SCard) int32 {
	l := len(c)
	cardNumber1 := int32(0)
	for i := 0; i < l-2; i++ {
		if c[i].GetNumber() == c[i+1].GetNumber() {
			cardNumber1 = c[i].GetNumber()
			break
		}
	}

	return cardNumber1
}

func compareHuLu(c1 []*netproto.SCard, c2 []*netproto.SCard) int32 {
	cardNumber1 := getSanTiao(c1)
	cardNumber2 := getSanTiao(c2)
	mlog.Debug("compareHuLu:c1=%v-c2=%v", c1, c2)
	mlog.Debug("compareHuLu:cardNumber1=%v,cardNumber2=%v", cardNumber1, cardNumber2)
	if cardNumber1 > cardNumber2 {
		return 1
	} else if cardNumber1 == cardNumber2 {
		return 0
	}
	return -1
}

func compareDuiZi(c1 []*netproto.SCard, c2 []*netproto.SCard) int32 {
	cardNumber1 := getMaxDuiZi(c1)
	cardNumber2 := getMaxDuiZi(c2)

	mlog.Debug("对子比较c1：%v,c2:%v", c1, c2)
	mlog.Debug("获取的对子牌为：cardNumber1=%v，cardNumber2=%v", cardNumber1, cardNumber2)
	if cardNumber1 > cardNumber2 {
		return 1
	} else if cardNumber1 == cardNumber2 {
		cardNumber3 := getMinDuiZi(c1)
		cardNumber4 := getMinDuiZi(c2)
		if cardNumber3 > cardNumber4 {
			return 1
		} else if cardNumber3 == cardNumber4 {
			return 0
		}

		return -1
	}
	return -1
}

func compareDun(d1 *netproto.Dun, d2 *netproto.Dun) int32 {
	if d1.GetCt() != d2.GetCt() {
		return -1 //墩类型不匹配 不能进行对比
	}

	if d1.GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
		return compareHuLu(d1.Cards, d2.Cards)
	}

	if d1.GetCt() == int32(netproto.SSSDunCardsNomalType_LiangDui) || d1.GetCt() == int32(netproto.SSSDunCardsNomalType_YiDui) {
		r := compareDuiZi(d1.Cards, d2.Cards)
		if r != 0 {
			return r
		}
	}

	l := len(d1.Cards)
	for i := l - 1; i >= 0; i-- {
		r := cc(d1.Cards[i], d2.Cards[i])
		if r != 0 {
			return r
		}
	}
	return 0
}

//dt 墩类型
func (s *SSSTable) CompareNormal() {
	size := len(s.roles)
	for i := 0; i < size; i++ {
		for n := 0; n < 3; n++ {
			s.roles[i].ris[n].Ct = netproto.SSSDunCardsNomalType(s.roles[i].act.ct[n])
		}
	}
	for dt := 0; dt < 3; dt++ {
		for i := 0; i < size; i++ {
			if s.roles[i].act.isSpecial == true {
				continue
			}
			for j := i + 1; j < size; j++ {
				if s.roles[j].act.isSpecial == true {
					continue
				}
				var shui int32 = 1
				if (dt + 1) == int(netproto.SSSDunType_DunTypeHead) { //头墩
					if s.roles[i].act.recommandCt.GetHead().GetCt() > s.roles[j].act.recommandCt.GetHead().GetCt() {
						if s.roles[j].act.recommandCt.GetHead().GetCt() == int32(netproto.SSSDunCardsNomalType_ChongSan) {
							shui = s.GetNsOdds(ChongSan)
						}
						s.roles[i].ris[dt].result[s.roles[j].seat] -= shui
						s.roles[j].ris[dt].result[s.roles[i].seat] += shui
					} else if s.roles[i].act.recommandCt.GetHead().GetCt() == s.roles[j].act.recommandCt.GetHead().GetCt() {
						r := compareDun(s.roles[i].act.recommandCt.GetHead(), s.roles[j].act.recommandCt.GetHead())
						if s.roles[i].act.recommandCt.GetHead().GetCt() == int32(netproto.SSSDunCardsNomalType_ChongSan) {
							shui = s.GetNsOdds(ChongSan)
						}
						if r > 0 {
							s.roles[i].ris[dt].result[s.roles[j].seat] += shui
							s.roles[j].ris[dt].result[s.roles[i].seat] -= shui
						} else if r < 0 {
							s.roles[i].ris[dt].result[s.roles[j].seat] -= shui
							s.roles[j].ris[dt].result[s.roles[i].seat] += shui
						} else {
							s.roles[i].ris[dt].result[s.roles[j].seat] -= 0
							s.roles[j].ris[dt].result[s.roles[i].seat] += 0
						}
					} else {
						if s.roles[i].act.recommandCt.GetHead().GetCt() == int32(netproto.SSSDunCardsNomalType_ChongSan) {
							shui = s.GetNsOdds(ChongSan)
						}
						s.roles[i].ris[dt].result[s.roles[j].seat] += shui
						s.roles[j].ris[dt].result[s.roles[i].seat] -= shui
					}
				} else if (dt + 1) == int(netproto.SSSDunType_DunTypeBody) { //中墩
					if s.roles[i].act.recommandCt.GetBody().GetCt() > s.roles[j].act.recommandCt.GetBody().GetCt() {
						if s.roles[j].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
							shui = s.GetNsOdds(ZhongDunTongHuaShun)
						} else if s.roles[j].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
							shui = s.GetNsOdds(ZhongDunZhaDan)
						} else if s.roles[j].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
							shui = s.GetNsOdds(ZhongDunHuLu)
						}
						s.roles[i].ris[dt].result[s.roles[j].seat] -= shui
						s.roles[j].ris[dt].result[s.roles[i].seat] += shui
					} else if s.roles[i].act.recommandCt.GetBody().GetCt() == s.roles[j].act.recommandCt.GetBody().GetCt() {
						if s.roles[j].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
							shui = s.GetNsOdds(ZhongDunTongHuaShun)
						} else if s.roles[j].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
							shui = s.GetNsOdds(ZhongDunZhaDan)
						} else if s.roles[j].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
							shui = s.GetNsOdds(ZhongDunHuLu)
						}
						r := compareDun(s.roles[i].act.recommandCt.GetBody(), s.roles[j].act.recommandCt.GetBody())
						if r > 0 {
							s.roles[i].ris[dt].result[s.roles[j].seat] += shui
							s.roles[j].ris[dt].result[s.roles[i].seat] -= shui
						} else if r < 0 {
							s.roles[i].ris[dt].result[s.roles[j].seat] -= shui
							s.roles[j].ris[dt].result[s.roles[i].seat] += shui
						} else {
							s.roles[i].ris[dt].result[s.roles[j].seat] -= 0
							s.roles[j].ris[dt].result[s.roles[i].seat] += 0
						}
					} else {
						if s.roles[i].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
							shui = s.GetNsOdds(ZhongDunTongHuaShun)
						} else if s.roles[i].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
							shui = s.GetNsOdds(ZhongDunZhaDan)
						} else if s.roles[i].act.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
							shui = s.GetNsOdds(ZhongDunHuLu)
						}
						s.roles[i].ris[dt].result[s.roles[j].seat] += shui
						s.roles[j].ris[dt].result[s.roles[i].seat] -= shui
					}
				} else { //后墩
					if s.roles[i].act.recommandCt.GetFoot().GetCt() > s.roles[j].act.recommandCt.GetFoot().GetCt() {
						if s.roles[j].act.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
							shui = s.GetNsOdds(DiDunTongHuaShun)
						} else if s.roles[j].act.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
							shui = s.GetNsOdds(DiDunZhaDan)
						}
						s.roles[i].ris[dt].result[s.roles[j].seat] -= shui
						s.roles[j].ris[dt].result[s.roles[i].seat] += shui
					} else if s.roles[i].act.recommandCt.GetFoot().GetCt() == s.roles[j].act.recommandCt.GetFoot().GetCt() {
						if s.roles[j].act.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
							shui = s.GetNsOdds(DiDunTongHuaShun)
						} else if s.roles[j].act.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
							shui = s.GetNsOdds(DiDunZhaDan)
						}
						r := compareDun(s.roles[i].act.recommandCt.GetFoot(), s.roles[j].act.recommandCt.GetFoot())
						if r > 0 {
							s.roles[i].ris[dt].result[s.roles[j].seat] += shui
							s.roles[j].ris[dt].result[s.roles[i].seat] -= shui
						} else if r < 0 {
							s.roles[i].ris[dt].result[s.roles[j].seat] -= shui
							s.roles[j].ris[dt].result[s.roles[i].seat] += shui
						}
					} else {
						if s.roles[i].act.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
							shui = s.GetNsOdds(DiDunTongHuaShun)
						} else if s.roles[i].act.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
							shui = s.GetNsOdds(DiDunZhaDan)
						}
						s.roles[i].ris[dt].result[s.roles[j].seat] += shui
						s.roles[j].ris[dt].result[s.roles[i].seat] -= shui
					}
				}
			}
		}
	}
	//检测是否打枪
	for _, v := range s.roles {
		if v.act.isSpecial {
			continue
		}
		for i := 0; i < len(s.roles); i++ {
			if v.seat == s.roles[i].seat {
				continue
			}
			isGun := true
			n := 0
			for dt := 0; dt < 3; dt++ {
				if v.ris[int32(dt)].result[s.roles[i].seat] < 0 {
					isGun = false
					mlog.Debug("没有打枪：%v", v.seat)
					break
				}

				if v.ris[int32(dt)].result[s.roles[i].seat] == 0 {
					n++
				}
				mlog.Debug("seat:%v,打枪信息：%v", v.seat, v.ris[int32(dt)].result[s.roles[i].seat])
			}

			if n == 3 {
				isGun = false
			}
			if isGun {
				v.gun = append(v.gun, s.roles[i].seat)
			}
		}
	}
}

func (s *SSSTable) GetSpacialCount() int32 {
	var n int32 = 0
	size := len(s.roles)
	for i := 0; i < size; i++ {
		if s.roles[i].act.isSpecial {
			n++
		}
	}
	return n
}

//特殊牌型比较
func (s *SSSTable) CompareSpeacil() {
	size := len(s.roles)
	for i := 0; i < size; i++ {
		if !s.roles[i].act.isSpecial {
			mlog.Debug("s.roles[%v]:%v不是特殊牌型", i, s.roles[i].roleid)
			continue
		}

		for j := i + 1; j < size; j++ {
			if !s.roles[j].act.isSpecial {
				mlog.Debug("s.roles[%v]:%v不是特殊牌型", j, s.roles[j].roleid)
				continue
			}

			info := new(WinInfo)
			if s.roles[i].act.sct < s.roles[j].act.sct {
				s.roles[i].act.sctWin += s.getSpecialShui(s.roles[i].act.sct)
				s.roles[j].act.sctLose -= s.getSpecialShui(s.roles[i].act.sct)
				info.seatNo = s.roles[j].seat
				info.win = s.getSpecialShui(s.roles[i].act.sct)
				s.roles[i].act.sctWinInfo = append(s.roles[i].act.sctWinInfo, info)
			} else if s.roles[i].act.sct > s.roles[j].act.sct {
				s.roles[i].act.sctLose -= s.getSpecialShui(s.roles[j].act.sct)
				s.roles[j].act.sctWin += s.getSpecialShui(s.roles[j].act.sct)
				info.seatNo = s.roles[i].seat
				info.win = s.getSpecialShui(s.roles[j].act.sct)
				s.roles[j].act.sctWinInfo = append(s.roles[j].act.sctWinInfo, info)
			}

			mlog.Debug("获取到的特殊水数:%v", info.win)
			mlog.Debug("userid[%v].sctWin=%v---userid[%v].sctLose=%v", s.roles[i].seat, s.roles[i].act.sctWin, s.roles[j].seat, s.roles[j].act.sctLose)
		}
	}
}

//特殊牌型与普通牌型之间的比较
func (s *SSSTable) CompareSpeacilAndNormal() {
	size := len(s.roles)
	for i := 0; i < size; i++ {
		if !s.roles[i].act.isSpecial {
			continue
		}
		for j := 0; j < size; j++ {
			if s.roles[j].act.isSpecial {
				continue
			}
			s.roles[i].act.sctWin += s.getSpecialShui(s.roles[i].act.sct)
			s.roles[j].act.sctLose -= s.getSpecialShui(s.roles[i].act.sct)

			info := new(WinInfo)
			info.seatNo = s.roles[j].seat
			info.win = s.getSpecialShui(s.roles[i].act.sct)
			s.roles[i].act.sctWinInfo = append(s.roles[i].act.sctWinInfo, info)
		}
	}
}

//组牌超时回调
func (s *SSSTable) CombineCardTimeOut() {
	mlog.Debug("组牌超时回调")
}

//选择是特殊牌型还是普通牌型超时回调
func (s *SSSTable) CardTypeChooseTimeOut() {
	mlog.Debug("牌型确定阶段超时")
	for _, v := range s.roles {
		if v.act == nil {
			s.RadomCardType(v.seat)
		}
	}
	if s.IsCompareDirect() {
		s.SetJump(netproto.SSSGameStep_SSSGameStepAnimotion)
	}
}

//创建牌型消息超时回调
func (s *SSSTable) getPokerMsg(upk *poker.UserPoker) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_UserPokerID)
	pmsg.MsgData = upk.GetNetMessageData()
	return pmsg
}

func (s *SSSTable) ResumeLoop() {
	s.restartLoop()
	s.logicEngine()
}

//给扑克排一次序
func (s *SSSTable) sortPokerByNumber(cards *poker.CardGroup) {
	arrlen := len(cards.Cards)
	for i := 0; i < arrlen; i++ {
		for j := 0; j+1 < arrlen-i; j++ {
			card1 := cards.Cards[j].CardNumber*10 + cards.Cards[j].Color
			card2 := cards.Cards[j+1].CardNumber*10 + cards.Cards[j+1].Color
			if card1 > card2 {
				cards.Swap(j, j+1)
			}
		}
	}
}

//发牌
func (s *SSSTable) faPai(count int) {
	s.poker.Shuffle()
	s.stopLoop()
	//	hasSpecialType := false
	for _, ur := range s.AllGameUser() {
		cards := s.poker.Deal(ur.UserID, count)
		s.sortPokerByNumber(cards)
		upk := poker.NewUserPoker(ur, cards)
		s.gamePoker.AddUserPoker(upk)
		s.SendMessageToUser(ur, s.getPokerMsg(upk))
		s.GameLog.Fapai(ur.UserID, cards.String())
	}

	s.animotionCur = s.RegisterAnimation(AnimotionFaPai, animotionTimeOut, s.ResumeLoop)
}

//游戏开始超时回调
func (s *SSSTable) gameBeginTimeOut() {
	s.faPai(13)
	mlog.Debug("游戏开始阶段超时")
}

//牌型选择请求
func (s *SSSTable) OnChooseSpecialCardType(user *gamelogic.GameUser, data []byte) {
	if s.step != netproto.SSSGameStep_SSSGameStepCardTypeChoose && s.step != netproto.SSSGameStep_SSSGameStepCombine {
		s.responseChooseSpecialCardTypeRet(1, "不是牌型选择阶段", user.SeatNo, 0)
		mlog.Error("不是牌型选择阶段 seat=%d", user.SeatNo)
		return
	}
	ust := &netproto.ChooseSpecialCardType{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析玩家出牌消息错误,%v", err)
		s.KickUserFunc(user.UserID, "非法操作")
		return
	}
	u := s.getRoleInfoBySeat(user.SeatNo)
	u.act = NewAutoCardType()
	//	s.choosespecialcount++
	if ust.GetSpecial() == false {
		s.broadcastUserCardTypeChoose(user, false)
		if ust.GetHcs() != nil {
			//mlog.Debug("玩家选择普通牌型，获取推荐牌型%s", ust.GetHcs().String())
			if !s.verifyUserCardType(user, ust.GetHcs(), false, 0) {
				//mlog.Error("489玩家发送上来的手牌错误,%v", err)
				s.KickUserFunc(user.UserID, "玩家发送上来的手牌错误,非法操作")
				u.act = nil
				return
			}
			s.GameLog.AddGameStep("玩家%d选择普通牌型%s", user.UserID, ust.GetHcs().String())
			u.act.recommandCt = ust.GetHcs()
			u.act.isSpecial = false
			u.act.Hc2Cgs()
		}
	} else {
		if !s.verifyUserCardType(user, u.act.recommandCt, true, ust.GetSpecialType()) {
			//mlog.Error("499玩家发送上来的手牌错误,%v", err)
			s.KickUserFunc(user.UserID, "玩家发送上来的手牌错误,非法操作")
			u.act = nil
			return
		}
		s.GameLog.AddGameStep("玩家%d选择特殊牌型%d", user.UserID, ust.GetSpecialType())
		s.broadcastUserCardTypeChoose(user, true)
		//mlog.Debug("玩家选择特殊牌型")
		u.act.isSpecial = true
		u.act.sct = ust.GetSpecialType()
		u.isZuPai = true
	}
	//其他阶段为推荐牌型只有特殊牌型才判断超时
	if s.step == netproto.SSSGameStep_SSSGameStepCardTypeChoose {
		u.isDo = true
		jumpstep := true
		//4个都选择了特殊牌型
		for _, v := range s.roles {
			if v.act == nil || !v.act.isSpecial {
				jumpstep = false
				break
			}
		}

		if jumpstep {
			//跳过组牌阶段
			s.SetJump(netproto.SSSGameStep_SSSGameStepCombine)
		}

		if s.checkAllRoleDo() {
			s.timeOutCallBack()
		}
	}
}

func (s *SSSTable) CheckSpecailDo() bool {
	n := 0
	for _, v := range s.roles {
		if v.isDo || (v.act != nil && v.act.isSpecial) {
			n++
			continue
		}

		if s.step == netproto.SSSGameStep_SSSGameStepCombine {
			if v.isZuPai {
				n++
				mlog.Debug("有一个组牌完成")
			}
		}
	}

	mlog.Debug("n=%v,len(s.roles)= %v", n, len(s.roles))
	return n >= len(s.roles)
}

//组牌完成请求
func (s *SSSTable) OnCompleteCombine(user *gamelogic.GameUser, data []byte) {
	if s.step != netproto.SSSGameStep_SSSGameStepCombine {
		s.responseCompleteCardCombRet(user, 1, "不是组牌阶段")
		mlog.Error("不是组牌阶段 seat=%d", user.SeatNo)
		return
	}
	ust := &netproto.CompleteCardComb{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析玩家出牌消息错误")
		s.KickUserFunc(user.UserID, "非法操作")
		return
	}
	u := s.getRoleInfoBySeat(user.SeatNo)
	if !s.verifyUserCardType(user, ust.GetHcs(), ust.GetIsSpecial(), ust.GetSt()) {
		mlog.Error("提交的牌型异常")
		return
	}
	//保存客户端发上来的牌型
	for i := 0; i < 3; i++ {
		u.act.cgs[i] = poker.NewCardGroup(nil)
	}
	for _, v := range ust.GetHcs().Head.Cards {
		c := new(poker.PokerCard)
		c.CardNumber = v.GetNumber()
		c.Color = v.GetColor()
		u.act.cgs[0].AddCard(c)
	}
	s.GameLog.AddGameStep("玩家%d选择头敦牌型%s", u.roleid, u.act.cgs[0].String())
	u.act.ct[0] = ust.GetHcs().Head.GetCt()
	u.act.Cg2Dun(ust.GetHcs().Head, 0, 3)
	for _, v := range ust.GetHcs().Body.Cards {
		c := new(poker.PokerCard)
		c.CardNumber = v.GetNumber()
		c.Color = v.GetColor()
		u.act.cgs[1].AddCard(c)
	}

	s.GameLog.AddGameStep("玩家%d选择中敦牌型%s", u.roleid, u.act.cgs[1].String())
	u.act.ct[1] = ust.GetHcs().Body.GetCt()
	u.act.Cg2Dun(ust.GetHcs().Body, 1, 5)
	for _, v := range ust.GetHcs().Foot.Cards {
		c := new(poker.PokerCard)
		c.CardNumber = v.GetNumber()
		c.Color = v.GetColor()
		u.act.cgs[2].AddCard(c)
	}

	s.GameLog.AddGameStep("玩家%d选择后敦牌型%s", u.roleid, u.act.cgs[2].String())
	u.act.ct[2] = ust.GetHcs().Foot.GetCt()
	u.act.Cg2Dun(ust.GetHcs().Foot, 2, 5)
	if ust.GetIsSpecial() {
		u.act.isSpecial = true
		u.act.sct = ust.GetSt()
	}
	mlog.Debug("dun types =%v", u.act.ct)
	u.act.Cg2Hc()
	s.responseCompleteCardCombRet(user, 0, "Ok")
	s.broadcastUserCardTypeChoose(user, ust.GetIsSpecial())
	u.isDo = true
	u.isZuPai = true
	if s.CheckSpecailDo() {
		s.timeOutCallBack()
		//機器人邏輯在此階段結束後會多跑一次GameMessage(SendGameMessageNow)
	}
}

//断线重连
func (s *SSSTable) UserRecover(gameuser *gamelogic.GameUser) {
	s.DynamicGameLogicBase.UserRecover(gameuser)
	s.PushGameCfg(gameuser)
	if s.IsGaming() {
		sendmsg := false
		if s.step == netproto.SSSGameStep_SSSGameStepCombine || s.step == netproto.SSSGameStep_SSSGameStepCardTypeChoose {
			v := s.getRoleInfoBySeat(gameuser.SeatNo)
			if v != nil && v.isDo == false {
				sendmsg = true
			}

			if v != nil {
				v.isDo = true
				v.isZuPai = true
			}
		}
		s.PushGameInfo(gameuser)
		if sendmsg {
			s.broadcastUserCardTypeChoose(gameuser, false)
			if s.CheckSpecailDo() {
				s.timeOutCallBack()
			}
		}
	}
}

func (s *SSSTable) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	s.DynamicGameLogicBase.UserEnter(gameuser, tableno, seatno)
	s.PushGameCfg(gameuser)
	if s.IsGaming() && !s.IsGameUser(gameuser) { //旁观
		s.PushGameInfo(gameuser)
	}
}

func (s *SSSTable) UserOffLine(gameuser *gamelogic.GameUser) {
	sendmsg := false
	if s.step == netproto.SSSGameStep_SSSGameStepCombine || s.step == netproto.SSSGameStep_SSSGameStepCardTypeChoose {
		v := s.getRoleInfoBySeat(gameuser.SeatNo)
		if v != nil && v.isDo == false {
			v.isDo = true
			v.isZuPai = true
			sendmsg = true
		}
	}

	if sendmsg {
		s.broadcastUserCardTypeChoose(gameuser, false)
		if s.CheckSpecailDo() {
			s.timeOutCallBack()
		}
	}

	s.DynamicGameLogicBase.UserOffLine(gameuser)
}

//客户端消息处理函数
func (s *SSSTable) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	//动画等待
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.GameMessageClassID_UserActionCommitID) {
		s.CheckUserAnimationCommited(user, gmsg, s.animotionCur, nil)
	}
	//选择牌型
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.SSS_GameMessageClassID_ChooseSpecialCardTypeId) {
		s.OnChooseSpecialCardType(user, gmsg.MsgData.([]byte))
	}
	//选择牌型
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.SSS_GameMessageClassID_CompleteCardCombId) {
		s.OnCompleteCombine(user, gmsg.MsgData.([]byte))
	}
}

func (s *SSSTable) ExchangeMaxCard(userid int32) {
	s.GameLogicBase.ExchangeMaxCard(userid)
	var maxRetUserID int32

	act1 := NewAutoCardType()
	specialtype := int32(0)
	fpoker := s.poker.(*poker.FixedPoker)
	for _, ur := range s.AllTableUser() {
		if ur.IsRobot() && ur.UserID != userid {
			continue
		}

		cg := fpoker.GetUserCards(ur.UserID)
		if maxRetUserID == 0 {
			maxRetUserID = ur.UserID
			specialtype = s.getSpecialType(cg)
			if specialtype == 0 {
				s.getNormalType(cg, act1)
			}
			mlog.Debug("userid=%d, ur.UserID= %d", userid, ur.UserID)
			continue
		}
		//比较牌型
		t := s.getSpecialType(cg)
		if t != 0 {
			if specialtype != 0 {
				if t < specialtype {
					maxRetUserID = ur.UserID
					specialtype = t
				}
			} else {

				maxRetUserID = ur.UserID
				specialtype = t
			}
		} else if specialtype == 0 {
			act2 := NewAutoCardType()
			s.getNormalType(cg, act2)
			if s.compareNormal(act2, act1) {
				maxRetUserID = ur.UserID
				act1 = act2
				mlog.Debug("userid=%d, ur.UserID= %d", userid, ur.UserID)
			}
		}
	}

	if maxRetUserID == 0 {
		return
	}

	example.invalid("交换用户%d和用户%d牌型", userid, maxRetUserID)
	s.GameLog.AddGameStep("交换用户%d和用户%d牌型", userid, maxRetUserID)
	fpoker.ExChange(userid, maxRetUserID)
}

func (s *SSSTable) getSpecialType(usercards *poker.CardGroup) int32 {
	if s.CheckTongHuaShiSanShui(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_TongHuaShiSanShui)
	}
	if s.CheckShiSanShui(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_ShiSanShui)
	}

	if s.CheckShiErHuangZu(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_ShiErHuangZu)
	}

	if s.CheckSanTongHuaShun(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_SanTongHuaShun)
	}

	if s.CheckSanFenTianXia(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_SanFenTianXia)
	}

	if s.CheckQuanDa(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_QuanDa)
	}

	if s.CheckQuanXiao(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_QuanXiao)
	}

	if s.CheckCouYiSe(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_CouYiSe)
	}

	if s.CheckSiTaoSanTiao(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_SiTaoSantiao)
	}

	if s.CheckWuDuiSanTiao(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_WuDuiSanTiao)
	}

	if s.CheckLiuDuiBan(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_LiuDuiBan)
	}

	if s.CheckSanTongHua(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_SanTongHua)
	}

	if s.CheckSanShunZi(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_SanShunZi)
	}

	if s.CheckShuangGuaiChongSan(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_ShuangGuaiChongSan)
	}

	if s.CheckSanChongJiaDan(usercards) {
		return int32(netproto.SSSDunCardsSpecialType_SanChongJiaDan)
	}

	return 0
}

func (s *SSSTable) getNormalType(usercards *poker.CardGroup, act *AutoCardType) {
	cg := poker.NewCardGroup(usercards.Cards)
	scg := CardGroupSortNumber{cg, false}
	sort.Sort(&scg)
	for i := SSSDunMax - 1; i >= 0; i-- {
		c := 0
		if i == SSSDunHead {
			c = 3
		} else {
			c = 5
		}
		ret := s.GetTongHuaShun(c, scg.CardGroup)
		if ret != nil {
			act.cgs[i] = ret
			s.sortPokerByNumber(act.cgs[i])
			if c == 3 {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_SanPai)
			} else {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_TongHuaShun)
			}
			continue
		}
		ret = s.Get4(cg)
		if ret != nil {
			act.cgs[i] = ret
			act.ct[i] = int32(netproto.SSSDunCardsNomalType_ZhaDan)
			act.cgs[i].AddCardGroup(s.getSPforCards(1, cg))
			continue
		}
		ret = s.GetHL(cg)
		if ret != nil {
			act.cgs[i] = ret
			act.ct[i] = int32(netproto.SSSDunCardsNomalType_HuLu)
			continue
		}
		tmpcg := poker.NewCardGroup(cg.Cards)
		s.sortCardsByColor(tmpcg)
		ret = s.GetTH(c, tmpcg)
		if ret != nil {
			act.cgs[i] = ret
			cg.DeleteGroup(ret)
			s.sortPokerByNumber(act.cgs[i])
			if c == 3 {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_SanPai)
			} else {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_TongHua)
			}
			continue
		}
		ret = s.GetSZ(c, cg)
		if ret != nil {
			act.cgs[i] = ret
			s.sortPokerByNumber(act.cgs[i])
			if c == 3 {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_SanPai)
			} else {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_ShunZi)
			}

			continue
		}
		ret = s.Get3(cg)
		if ret != nil {
			act.cgs[i] = ret
			if c != 3 {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_SanTiao)
				cards := s.getSPforCards(2, cg)
				s.sortPokerByNumber(act.cgs[i])
				act.cgs[i].AddCardGroup(cards)
			} else {
				act.ct[i] = int32(netproto.SSSDunCardsNomalType_ChongSan)
			}

			continue
		}
		ret = s.Get2d(cg)
		if ret != nil {
			act.cgs[i] = ret
			s.sortPokerByNumber(act.cgs[i])
			act.ct[i] = int32(netproto.SSSDunCardsNomalType_LiangDui)
			act.cgs[i].AddCardGroup(s.getSPforCards(1, cg))
			continue
		}

		ret = s.Get1d(cg)
		if ret != nil {
			act.cgs[i] = ret
			act.ct[i] = int32(netproto.SSSDunCardsNomalType_YiDui)
			if i != 0 {
				cards := s.getSPforCards(3, cg)
				s.sortPokerByNumber(act.cgs[i])
				act.cgs[i].AddCardGroup(cards)
			} else {
				act.cgs[i].AddCardGroup(s.GetSP(1, cg))
			}
			continue
		}

		if i == SSSDunHead {
			act.cgs[i] = s.GetSP(3, cg)
			s.sortPokerByNumber(act.cgs[i])
			act.ct[i] = int32(netproto.SSSDunCardsNomalType_SanPai)
		} else {
			act.cgs[i] = s.GetSP(5, cg)
			s.sortPokerByNumber(act.cgs[i])
			act.ct[i] = int32(netproto.SSSDunCardsNomalType_SanPai)
		}
	}

	for i := 0; i < 3; i++ {
		mlog.Debug("顿类型%v", act.ct[i])
		mlog.Debug("顿牌型%v", act.cgs[i])
	}

	act.Cg2Hc()
	act.recommandCt.Head.Sdt = proto.Int32(int32(netproto.SSSDunType_DunTypeHead))
}

func (s *SSSTable) compareNormal(act1 *AutoCardType, act2 *AutoCardType) bool {
	shui1 := int32(0)
	shui2 := int32(0)
	mlog.Debug("act1.recommandCt.Head.Cards=%v", act1.recommandCt.Head.Cards)
	mlog.Debug("act2.recommandCt.Head.Cards=%v", act2.recommandCt.Head.Cards)

	mlog.Debug("act1.recommandCt.Body.Cards=%v", act1.recommandCt.Body.Cards)
	mlog.Debug("act2.recommandCt.Body.Cards=%v", act2.recommandCt.Body.Cards)

	mlog.Debug("act1.recommandCt.Foot.Cards=%v", act1.recommandCt.Foot.Cards)
	mlog.Debug("act2.recommandCt.Foot.Cards=%v", act2.recommandCt.Foot.Cards)
	for dt := 0; dt < 3; dt++ {
		shui := int32(1)
		if dt == 0 {
			if act1.ct[dt] > act2.ct[dt] {
				if act1.ct[dt] == int32(netproto.SSSDunCardsNomalType_ChongSan) {
					shui = s.GetNsOdds(ChongSan)
				}

				mlog.Debug("shui2------------")
				shui2 += shui
				//shui2 -= shui
			} else if act1.ct[dt] < act2.ct[dt] {
				if act2.ct[dt] == int32(netproto.SSSDunCardsNomalType_ChongSan) {
					shui = s.GetNsOdds(ChongSan)
				}

				mlog.Debug("shui1------------")
				//shui1 -= shui
				shui1 += shui
			} else {
				r := compareDun(act1.recommandCt.GetHead(), act2.recommandCt.GetHead())
				if act1.recommandCt.GetHead().GetCt() == int32(netproto.SSSDunCardsNomalType_ChongSan) {
					shui = s.GetNsOdds(ChongSan)
				}

				if r > 0 {
					mlog.Debug("===shui1------------")
					shui1 += shui
					//shui2 -= shui
				} else if r < 0 {
					//shui1 -= shui
					mlog.Debug("===shui2------------")
					shui2 += shui
				}
			}
		} else if dt == 1 {
			//中墩
			if act1.recommandCt.GetBody().GetCt() > act2.recommandCt.GetBody().GetCt() {
				if act1.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
					shui = s.GetNsOdds(ZhongDunTongHuaShun)
				} else if act1.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
					shui = s.GetNsOdds(ZhongDunZhaDan)
				} else if act1.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
					shui = s.GetNsOdds(ZhongDunHuLu)
				}

				mlog.Debug("shui2------------")
				shui2 += shui
				//shui2 -= shui
			} else if act1.recommandCt.GetBody().GetCt() == act2.recommandCt.GetBody().GetCt() {
				if act1.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
					shui = s.GetNsOdds(ZhongDunTongHuaShun)
				} else if act1.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
					shui = s.GetNsOdds(ZhongDunZhaDan)
				} else if act1.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
					shui = s.GetNsOdds(ZhongDunHuLu)
				}
				r := compareDun(act1.recommandCt.GetBody(), act2.recommandCt.GetBody())
				if r > 0 {
					mlog.Debug("===shui1------------")
					shui1 += shui
					//shui2 -= shui
				} else if r < 0 {
					//shui1 -= shui
					mlog.Debug("=====shui2------------")
					shui2 += shui
				}
			} else {
				if act2.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
					shui = s.GetNsOdds(ZhongDunTongHuaShun)
				} else if act2.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
					shui = s.GetNsOdds(ZhongDunZhaDan)
				} else if act2.recommandCt.GetBody().GetCt() == int32(netproto.SSSDunCardsNomalType_HuLu) {
					shui = s.GetNsOdds(ZhongDunHuLu)
				}
				mlog.Debug("shui1------------")
				shui1 += shui
				//shui1 -= shui
			}
		} else {
			//后墩
			if act1.recommandCt.GetFoot().GetCt() > act2.recommandCt.GetFoot().GetCt() {
				if act1.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
					shui = s.GetNsOdds(DiDunTongHuaShun)
				} else if act1.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
					shui = s.GetNsOdds(DiDunZhaDan)
				}

				mlog.Debug("shui2------------")
				shui2 += shui
				//shui2 -= shui
			} else if act1.recommandCt.GetFoot().GetCt() == act2.recommandCt.GetFoot().GetCt() {
				if act1.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
					shui = s.GetNsOdds(DiDunTongHuaShun)
				} else if act1.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
					shui = s.GetNsOdds(DiDunZhaDan)
				}
				r := compareDun(act1.recommandCt.GetFoot(), act2.recommandCt.GetFoot())
				if r > 0 {
					mlog.Debug("====shui1------------")
					shui1 += shui
					//shui2 -= shui
				} else if r < 0 {
					mlog.Debug("====shui2------------")
					//shui1 -= shui
					shui2 += shui
				}
			} else {
				if act2.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_TongHuaShun) {
					shui = s.GetNsOdds(DiDunTongHuaShun)
				} else if act2.recommandCt.GetFoot().GetCt() == int32(netproto.SSSDunCardsNomalType_ZhaDan) {
					shui = s.GetNsOdds(DiDunZhaDan)
				}
				mlog.Debug("shui1------------")
				shui1 += shui
				//shui1 -= shui
			}
		}
	}

	mlog.Debug("shui1=%d,shui2=%d", shui1, shui2)
	return (shui1 > shui2)
}
