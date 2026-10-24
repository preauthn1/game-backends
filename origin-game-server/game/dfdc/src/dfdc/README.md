# 多福多財 DFDC

### 資料夾結構

```
--- dfdc
  | --- const.go                // 基本的常數定義在這
  | --- game.go                 // 遊戲邏輯
  | --- game-result.go          // 遊戲結果，拉霸轉出內容
  | --- jackpot.go              // 獎池邏輯
  | --- jackpot-game.go         // BonusGame 翻牌遊戲
  | --- logic.go                // 主要流程邏輯
  | --- message-client.go       // 跟Client溝通的訊息在這
  | --- message-rpc.go          // 跟DB Server溝通的訊息在這
  | --- payline-matcher.go      // 定義賠付線比對器介面
  | --- paylineall-matcher.go   // 全線賠付比對器
  | --- paylinefixed-mathcer.go // 指定線賠付比對器
  | --- payresult.go            // 比對中獎結果
  | --- protoconfig.go          // 負責抓取protoconfig設定
```

# 流程

### 遊戲邏輯建置 (NewLogic)

1. 讀取dat檔設定資料
2. 讀取proto config設定資料
   > 整理後只取dat檔內設定的SlotPattern，找出對應的
   > * MainGameTable            // 主遊戲表
   > * FreeSpinTable            // 免費遊戲表
   > * SlotSymboDistribution    // 開獎符號表
   > * SymbolAttribute          // 特殊符號表
   > * SlotPayLine              // 指定賠付線表
   > * SlotPayTable             // 賠付資料表
   > * JackpotTable             // 獎池設定
3. 定義此Logic要用哪種賠付比對(全線 / 指定線)
4. 初始化Jackpot

### 主遊戲

> 箭頭說明:
>
> -> 10004 表示 Server 傳給 Client
> 
> <- 10002 表示 Client 傳給 Server

> 每個階段都會把資訊存入DB，所以玩家離開再進入只需在讀取DB資訊進而判斷是否傳給Client過去訊息

1. UserEnter
    * Load Jackpot, BonusGame, FreeSpinData
    * Send Setting Message to client
    * Send Jackpot, BonusGame, FreeSpinData to client
    > -> 10011  黃金符號倍率
    > 
    > -> 10001 遊戲場景設置內容
    > 
    > -> 10004 遊戲狀態(等待下注)
    >
    > -> 10009 Jackpot資訊
2. Spin
    > <- 10002 玩家拉霸
    > 
    > -> 10010 下注金額狀態
    > 
    > -> 10004 遊戲狀態(拉霸，等待開獎)
    > 
    > -> 10004 遊戲狀態(開獎)
    > * 取得目前設定的SlotPattern
    > * 依照下注倍率找出要開獎的符號群組ID
    > * 累積Jackpot金額
    >   > 向DB更新Jackpot金額
    >   >
    >   > -> 10009 向Client更新Jackpot資訊
    > * 產生遊戲結果(哪一格開什麼符號)
    > * 計算遊戲結果(判斷中獎邏輯)
    > * 計算遊戲獎勵
    >   * 有中 FreeSpin，依照中獎的FreeSpinGroup與當前下注倍率找出對應的FreeSpinTable，並向DB更新，Logic內紀錄為有中FreeSpin
    >   * 有中 BonusGame，建立BonusGame邏輯，產生中獎結果，向DB更新，Logic內紀錄為有中BonusGame
    >   * 計算連線金額
    >
    > -> 10005 FreeSpin 剩餘次數
    > > 若有中 BonusGame 
    > > 
    > > -> 10012 發送 BonusGame 開獎歷程與中獎金額
    > 
    > -> 10003 發送開獎結果，中獎金額，賠付資訊
    > 
    > -> 10004 遊戲狀態(等待下局遊戲)
    > 
    > -> 8 GameEndID
    >
    > -> 10004 遊戲狀態(等待下注)
