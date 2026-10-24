# Server 版本說明

### DB Server `v1.0.4.phnum.a-1` (`20191129-phunm-108`)
### Game Server
* dfdc (多福多財) `v1.0.4.phnum.a-1` (`20191129-phunm-108`)
* jdesyd (經典21點) `v1.0.4.phnum.pre.e-2` (`20191128-phunm-105`)
* jjbx (金吉報喜) `v1.0.4.phnum.a-1` (`20191129-phunm-108`)
* jlbpk (加勒比撲克) `v1.0.4.phnum.pre.e-2` (`20191128-phunm-105`)
* wlzb (五龍爭霸) `v1.0.4.phnum.a-1` (`20191129-phunm-108`)
* xgpd (星光派對) `v1.0.4.phnum.a-1` (`20191129-phunm-108`)


# 檔案內容說明

### Data
老虎機機率設定資料

### GameRoom
每款遊戲房間設定

* dfdc (多福多財)
* jdesyd (經典21點)
* jjbx (金吉報喜)
* jlbpk (加勒比撲克)
* wlzb (五龍爭霸)
* xgpd (星光派對)

### ServerFiles
所有二進制執行檔

### SQL-Update
本次更新所需要執行的SQL語法，請依照編號 **按照順序** 執行。




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

1. 將遊戲執行檔複製到房間內。 `ServerFiles/gameserver/{遊戲}/file/{執行檔}`
2. 調整各遊戲房間內的 `config` 設定
   * `gameserver.dat`: 
     * 調整`gameid`為該遊戲對應之遊戲ID
     * 依照需求在`[gameserver]`這個Group下加入`lianyunid`聯運ID
   * `serverinfo.dat`:
     * 將`GameRoom/serverinfo.dat`(此為空白範例檔)，複製到遊戲房間的 `config` 內，並加以調整
3. 準備 `log` 資料夾
4. 執行二進制檔案