# Server 版本說明

### DB Server `v1.1.3-phnum` (`20200103-phnum-1550`)
### Game Server
* wlzb (五龍爭霸彩金版) `v1.1.5.1-phnum` (`20200117-phnum-1768`)
* dchjk (賭場黑傑克) `v1.1.5.1-phnum` (`20200117-phnum-1768`)


### 更新內容
* 新增遊戲 `賭場黑傑克`
* 新增遊戲 `五龍爭霸彩金版`

# 檔案內容說明

### Data
老虎機機率設定資料

### ServerFiles
所有二進制執行檔

# 啟動遊戲

### 老虎機
> * wlzb (五龍爭霸彩金版)

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
> * dchjk (賭場黑傑克)

1. 將遊戲執行檔複製到房間內。 `ServerFiles/gameserver/{遊戲}/file/{執行檔}`
2. 調整各遊戲房間內的 `config` 設定
   * `gameserver.dat`: 
     * 調整`gameid`為該遊戲對應之遊戲ID
     * 依照需求在`[gameserver]`這個Group下加入`lianyunid`聯運ID
   * `serverinfo.dat`:
     * 將`GameRoom/serverinfo.dat`(此為空白範例檔)，複製到遊戲房間的 `config` 內，
   * `poker.dat`:
     * 此檔案無需異動
3. 準備 `log` 資料夾
4. 執行二進制檔案
