# CHANGELOG `v1.2.0.1`

> `20200304-build-2505`

### Bugfix
* [Slot] 修正判斷盤面方向邏輯

# CHANGLOG `v1.2.0`

> `20200227-build-2397`

### New Game
* [WLJR] 百萬金輪五龍爭霸
* [JGYF] 擊鼓迎福

### Feature
* 整合金邊服務設定
  > | File              | Section       | Name           | Description                |
  > |:-----------------:|:-------------:|:--------------:|:---------------------------|
  > | `gameserver.dat`  | `gameserver`  | `lianyunid`    | 聯運ID(舊的，可相容)          |
  > | `serverinfo.dat`  | `server`      | `realaddr`     | 實際對外網址                 |
  > |                   | `gameserver`  | `lianyunid`    | 聯運ID(新的，以此為主)        |
  > |                   |               | `platform`     | 營運平台 (金邊: `PHNUM`)     |
* 五個新遊戲(jdesyd, dczz, dchjk, jlbpk, hdc)的遊戲配置, 水池功能優化

