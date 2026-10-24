# server-team-tf-server

> 本目录原始 README 见文末，本节为快照补充信息。

原始路径：`server-team-tf-server`　|　文件 **19**　|　主语言：Go 18, XML 1

## 目录构成

```
internal/tfserver/  — 14
internal/tfconfig/  — 2
cmd/tfconfig/  — 2
cmd/tfserver/  — 1
```

---

## 原始说明

# Servers

* [TF Server](#tf-server)
* [TF Config](#tf-config)

# TF Server

# TF Config

### RestfulAPIs

| Method | URI                    | Description            |
|:------:|:----------------------:|:-----------------------|
| GET    | [config](#config)      | 加載配置               |
| GET    | [submit](#submit)      | 交轉發服務器狀態       |
| GET    | [tfstatus](#tf-status) | 讀取轉發服務器運行狀況 |

---
#### Config
加載配置
##### Method `GET`
##### URI `${hostname}/config`
---
#### Submit
交轉發服務器狀態
##### Method `GET`
##### URI `${hostname}/submit`
##### Arguments
| Name | Type   | Required | Descirption |
| :--: | :----: | :------: | :---------- |
| cn   | Int    | Required | 當前連接數  |
| code | string | Required | 校驗碼      |
> `code`: 要帶入`tfconfig.dat`裡的`submitcheckcode`
---
#### TF Status
讀取轉發服務器運行狀況
##### Method `GET`
##### URI `${hostname}/tfstatus`
---

