# Candy House 糖果屋

## 封包傳遞

#### Server to Client
|BClassID Name    |Code     |Description                          |
|:---------------:|:-------:|:-----------------------------------:|
|InitID           |10001    |設置相關                              |
|StatusID         |10002    |遊戲狀態                              |
|JackpotPoolID    |10003    |Jackpot 獎池資訊                      |
|GameResult       |10004    |遊戲結果 (轉出內容，中獎項目，中獎金額)    |
|BonusGameID      |10005    |玩家贏得 BonusGame 並取得BonusGame紀錄  |
|LoadHistoryRetID |10006    |回傳玩家查詢記錄                        |

#### Client to Server
|BClassID Name    |Code     |Description                          |
|:---------------:|:-------:|:-----------------------------------:|
|ActionSpinID     |11001    |玩家Spin                              |
|ActionPickID     |11002    |玩家選則PickGame                       |
|LoadHistoryID    |11003    |玩家查詢記錄                           |

##### InitID
Server發送遊戲初始資訊給Client

|Field    |Type     |Description                          |
|:---------------:|:-------:|:-----------------------------------:|
|InitID           |10001    |設置相關                              |
|StatusID         |10002    |遊戲狀態                              |
|JackpotPoolID    |10003    |Jackpot 獎池資訊                      |
|GameResult       |10004    |遊戲結果 (轉出內容，中獎項目，中獎金額)    |
|BonusGameID      |10005    |玩家贏得 BonusGame 並取得BonusGame紀錄  |
|LoadHistoryRetID |10006    |回傳玩家查詢記錄                        |


