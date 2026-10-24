# Server 版本說明

### DB Server `v1.1.4-phnum` (`20200110-phnum-1638`)
### Game Server
* dfdc (多福多財) `v1.1.4-phnum` (`20200110-phnum-1638`)
* jjbx (金吉報喜) `v1.1.4-phnum` (`20200110-phnum-1638`)
* wlzb (五龍爭霸) `v1.1.4-phnum` (`20200110-phnum-1638`)
* xgpd (星光派對) `v1.1.4-phnum` (`20200110-phnum-1638`)
* xmbx (熊貓報喜) `v1.1.4-phnum` (`20200110-phnum-1638`)
* dhzw (東海之王) `v1.1.4-phnum` (`20200110-phnum-1638`)

### 更新內容
* SLOT遊戲 遊戲歷程簡化

> `SQL-Update` 內請將各個資料夾內的SQL檔案按照順序執行


### 設定檔新增控制參數

#### `gameserver.dat` 
```
[gameserver]
scoremode=3               // 0 即时写分 1退房间写分 3條件寫分
deallog=4                 // 牌局记录器类型0 不记录 ,1 文本, 2 db, 4 條件寫db
settlescore=1000          // 大於SettleScore分時結算, deallog=4 或 scoremode=3 吃這個設定
settleround=10            // 大於SettleRound局時結算, deallog=4 或 scoremode=3 吃這個設定
savehistoryinterval=5000  // 刷新歷史紀錄秒數(ms), 5000=5秒
```


# 檔案內容說明

### Data
老虎機機率設定資料

### ServerFiles
所有二進制執行檔

# 啟動遊戲

### DB Server
1. 將執行檔複製到資料夾內。 `ServerFiles/dbserver/dbserver`
2. 調整 `config/db.dat` 設定檔內容
    ```
    [tcp]
    addr=                       // 侦听地址

    [conn]
    connstring=""               // 中心库数据库连接字符串 CenterDB
    deallogconnstring=""        // 牌局记录库数据库连接字符串 PerGameLog
    clubconnstring=""           // Club数据库连接字符串 Club
    semaphore=5                 // 数据库并发控制
    pool=200                    // 数据库连接池大小
    ```
3. 啟動 `dbserver`

### 老虎機
> * dfdc (多福多財)
> * jjbx (金吉報喜)
> * wlzb (五龍爭霸)
> * xgpd (星光派對)
> * xmbx (熊貓報喜)
> * dhzw (東海之王)

1. 將遊戲執行檔複製到房間內。 `ServerFiles/gameserver/{遊戲}/file/{執行檔}`
2. 將 `Data` 內的 `protodata` 複製到房間內
3. 調整各遊戲房間內的 `config` 設定
   * `gameserver.dat`: 
     * 調整`gameid`為該遊戲對應之遊戲ID
     * 依照需求在`[gameserver]`這個Group下加入`lianyunid`聯運ID
   * `serverinfo.dat`:
     * 將`GameRoom/serverinfo.dat`(此為空白範例檔)，複製到遊戲房間的 `config` 內，並加以調整
   * `slotconfig.dat`:
     * 此檔案請勿異動
4. 準備 `log` 資料夾
5. 執行二進制檔案

### 棋牌
> * jdesyd (經典21點)
> * jlbpk (加勒比撲克)
> * dczz (賭場戰爭)
> * hdc (海盜船)

1. 將遊戲執行檔複製到房間內。 `ServerFiles/gameserver/{遊戲}/file/{執行檔}`
2. 調整各遊戲房間內的 `config` 設定
   * `gameserver.dat`: 
     * 調整`gameid`為該遊戲對應之遊戲ID
     * 依照需求在`[gameserver]`這個Group下加入`lianyunid`聯運ID
   * `serverinfo.dat`:
     * 將`GameRoom/serverinfo.dat`(此為空白範例檔)，複製到遊戲房間的 `config` 內，並加以調整
3. 準備 `log` 資料夾
4. 執行二進制檔案
