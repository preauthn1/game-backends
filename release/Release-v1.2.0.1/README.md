# Server `v1.2.0.1` 版本說明

### DB Server `v1.2.0.1` (`20200304-build-2505`)
### Game Server
* Slot Game:
  * dfdc (多福多財) `v1.2.0.1` (`20200304-build-2505`)
  * wlzb (五龍爭霸) `v1.2.0.1` (`20200304-build-2505`)
  * xgpd (星光派對) `v1.2.0.1` (`20200304-build-2505`)
  * jjbx (金吉報喜) `v1.2.0.1` (`20200304-build-2505`)
  * xmbx (熊貓報喜) `v1.2.0.1` (`20200304-build-2505`)
  * dhzw (東海之王) `v1.2.0.1` (`20200304-build-2505`)
  * wlzbjp (五龍爭霸_彩金版) `v1.2.0.1` (`20200304-build-2505`)
  * csdjp (財神到_彩金版) `v1.2.0.1` (`20200304-build-2505`)
  * wlhc (五龍皇朝) `v1.2.0.1` (`20200304-build-2505`)
  * wljr (百萬金輪五龍爭霸) `v1.2.0.1` (`20200304-build-2505`)
  * jgyf (擊鼓迎福) `v1.2.0.1` (`20200304-build-2505`)
* Pocker Game:
  * jlbpk (加勒比撲克) `v1.2.0.1` (`20200304-build-2505`)
  * jdesyd (經典21點) `v1.2.0.1` (`20200304-build-2505`)
  * dczz (賭場戰爭) `v1.2.0.1` (`20200304-build-2505`)
  * hdc (海盜船) `v1.2.0.1` (`20200304-build-2505`)
  * dchjk (賭場黑傑克) `v1.2.0.1` (`20200304-build-2505`)

### 更新內容

> 異動內容請參照 [CHANGELOG](CHANGELOG.md)

* 請將 `dbserver` 換成 `dbserver-phnum` 啟動
* SLOT 遊戲請重新將 機率表 (`Data/protodata`) 更新到各個遊戲中
* 所有遊戲的設定檔需調整設定
  > | File              | Section       | Name           | Description                |
  > |:-----------------:|:-------------:|:--------------:|:---------------------------|
  > | `gameserver.dat`  | `gameserver`  | `lianyunid`    | 聯運ID(舊的，可相容)          |
  > | `serverinfo.dat`  | `server`      | `realaddr`     | 實際對外網址                 |
  > |                   | `gameserver`  | `lianyunid`    | 聯運ID(新的，以此為主)        |
  > |                   |               | `platform`     | 營運平台 (金邊: `PHNUM`)     |
