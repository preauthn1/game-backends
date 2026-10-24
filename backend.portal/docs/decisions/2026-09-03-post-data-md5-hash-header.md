# POST Data MD5 Hash Header

## 背景與目標

部分 POST API 需要依 request data 產生 MD5 驗證值，並透過 `X-target-POST-HASH` HTTP header 傳送。

本次只擴充共用 `request` 方法的選用式 hash 行為，不改變 request body、既有驗證 header、API response 或其他 HTTP method 的行為。

## 已確認需求

- 僅當 `config.method` 為 `post`，且 `config.isHash` 不為 `null` 並且為 `true` 時產生 hash。
- 將 `config.data` 的第一層 key 依字母順序排序。
- 依排序結果組成 `key1=value&key2=value` 格式的字串。
- 一般 value 使用 `String(value)`；array 或 object value 使用 `JSON.stringify(value)`，避免產生 `[object Object]`。
- 將完整字串轉成全小寫，再計算 MD5 hash。
- 將 hash 寫入 HTTP header `X-target-POST-HASH`。
- 不新增 production dependency，使用專案既有的 `ts-md5`。

## 現況分析

- 共用 request 邏輯位於 `src/utilities/api-handler.lib.ts` 的 `HttpModel.request`。
- `request` 目前會依 `tempToken`、`isFromLoginAPI` 與一般請求三種情況重新建立 `cfg.headers`。
- 專案目前以小寫字串 `post` 設定 Axios method。
- 專案已有 `ts-md5` dependency，且其他元件使用 `Md5.hashStr`。
- 目前 hash 邏輯以 `String(cfg.data[key])` 轉換所有 value；array 內的 object 因此會變成 `[object Object]`，無法反映實際內容。
- 目標檔案目前另有未提交變更，包括 local 環境 logging、hash salt，以及現行 `isHash` 判斷；本次只修正 value 序列化並保留這些變更。

## 建議方案

在 `api-handler.lib.ts` 匯入 `Md5`，並在三種既有 header 建立完成後、送出 Axios request 前執行選用式 hash 邏輯：

1. 保留目前 `cfg.isHash != null && cfg.isHash === true` 的啟用條件。
2. 使用 `Object.keys(cfg.data).sort()` 排序第一層 key。
3. value 不為 `null` 且型別為 object（包含 array）時使用 `JSON.stringify(value)`；其他 value 維持 `String(value)`。
4. 對每個 key 以 `${key}=${hashValue}` 產生片段，再以 `&` 串接。
5. 將完整字串呼叫 `toLowerCase()`。
6. 保留目前 local logging、hash salt 與 `X-target-POST-HASH` header 寫入方式。

此方案不修改 `cfg.data`，因此不會改變實際送出的 JSON 欄位順序或內容。

## 預計修改範圍

- 修改 `src/utilities/api-handler.lib.ts`：新增 MD5 import 與條件式 header 邏輯。
- 不修改各 API wrapper、Vue component、package dependency 或後端。
- 本決策文件是唯一新增文件。

## API 與資料契約

輸入的 Axios config 新增選用欄位：

- `isHash: true`：符合 POST 條件時加入 hash header。
- `isHash` 缺少、為 `null`、`false` 或其他值：維持原行為。

輸出的 HTTP request 在符合條件時新增：

- Header：`X-target-POST-HASH: <32-character MD5 hash>`。
- Body：不變。

字串組合採第一層資料欄位，不進行 URL encoding。一般 value 使用 JavaScript `String(value)`；巢狀 object 或 array 使用 `JSON.stringify(value)`。巢狀 key 維持原始 insertion order，不另外排序。

## 替代方案

- 使用 `URLSearchParams`：會進行 URL encoding，與指定的原始 `key=value&...` 契約不同，因此不採用。
- 先建立排序後的新 request body：可能改變送出資料且非需求所需，因此不採用。
- 所有 value 一律使用 `JSON.stringify`：一般字串會多出雙引號，破壞既有 primitive value 的 hash 契約，因此不採用。
- 遞迴排序巢狀 key：會形成不同 hash 契約，且不在本次確認範圍，因此不採用。
- 新增 MD5 套件：專案已有 `ts-md5`，無需新增依賴。

## 風險與待確認事項

- Hash 契約對值的字串化方式敏感；本方案對 primitive 採用 `String(value)`、對 array/object 採用 `JSON.stringify(value)`，不 URL encode，且只排序第一層 key。
- `JSON.stringify` 會保留 array 順序及巢狀 object 的 insertion order；若後端採不同的巢狀 key 排序規則，hash 仍可能不同。
- 若 `isHash === true` 但 `config.data` 為 `null` 或 `undefined`，`Object.keys` 會失敗。實作會以空 object 處理，產生空字串的 MD5，避免 request 在前端中斷。
- `config.method` 依目前專案慣例採精確比對小寫 `post`；大寫 `POST` 不啟用 hash。
- MD5 不適合作為有密鑰的防偽機制；本次依既定 API 契約實作，不擴張為安全協定調整。

## 驗收標準

- `method: 'post'`、`isHash: true`、`data: { b: 'Two', a: 'One' }` 時，以 `a=one&b=two` 計算 MD5，並放入 `X-target-POST-HASH`。
- `data: { items: [{ value: '176082', settingKey: 3735 }] }` 時，hash 原文為 `items=[{"value":"176082","settingkey":3735}]`，不再包含 `[object Object]`。
- `config.data` 本身不被排序、轉小寫或修改。
- `isHash` 缺少、為 `null` 或 `false` 時不加入該 header。
- 本次不變更目前的 method 判斷、local logging 或 hash salt 行為。
- 原有 token、locale、device size 與 content-type headers 均保留。
- 本次異動文字檔案皆為 CRLF，且 Git diff 無 whitespace error。

## 測試計畫

- 執行 TypeScript/Vue lint，確認新增 import 與程式碼可通過既有靜態檢查。
- 以可重現的範例驗算排序後小寫字串與 `Md5.hashStr` 結果。
- 檢查 `git diff --check`。
- 逐一檢查本次異動檔案的 `LFOnly = 0`。
