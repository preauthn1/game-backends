package scorestorage

import (
	"gamelogic"
)

type ConsoleUserScoreStorage struct {
}

func (uss *ConsoleUserScoreStorage) UpdateUserScore(user *gamelogic.GameUser, score int64, tax int64) {
	user.Score += score
	user.UpdateScore += score
}

func (uss *ConsoleUserScoreStorage) OnUserLeave(user *gamelogic.GameUser) {
	//fmt.Printf("user score change:%d\r\n", user.UpdateScore)
}

func (uss *ConsoleUserScoreStorage) OnUserRecover(user *gamelogic.GameUser, dbscore int64) {
	//fmt.Printf("user score recover:%v, %d\r\n", user, dbscore)
}

func (uss *ConsoleUserScoreStorage) OnUserMoneyAdd(user *gamelogic.GameUser, changeScore int64)	{

}
func (uss *ConsoleUserScoreStorage) OnUserReady(user *gamelogic.GameUser) {

}
func (uss *ConsoleUserScoreStorage) UpdateExperienceScore(user *gamelogic.GameUser, score int64, tax int64){

}

func (uss *ConsoleUserScoreStorage) UpdateGameJackpotPool(user *gamelogic.GameUser, amount int64) {

}