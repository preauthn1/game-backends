# ElasticsearchHelperGetEachApiErrorCount

## 背景與目標

在 `ElasticsearchHelper` 完成 `GetEachApiErrorCount`，依指定 Elasticsearch index 與時間區間，呼叫 `/{index_name}/_search`，按 `RelativeUrl.keyword` 統計錯誤 API 數量。

本次僅完成 Helper 查詢與完整 Elasticsearch response 的強型別承接，不包含排程通知、告警門檻或其他 Service 邏輯。

## 已確認需求

- 方法沿用現有簽章概念，輸入為 `ElasticLogTableTypeEnum indexType`、`DateTime begin`、`DateTime end`。
- Elasticsearch index 名稱由既有 `TableNameConvertHelper.GetElasticsearchIndexName` 轉換。
- 使用 HTTP POST 呼叫 `/{index_name}/_search`。
- request body：
  - `size` 為 `0`。
  - `query.bool.must` 包含 `CreateTime` range。
  - `gte` 與 `lte` 格式為 `yyyy-MM-ddTHH:mm:ss`。
  - aggregation 名稱為 `group_by_category`。
  - terms field 為 `RelativeUrl.keyword`。
  - terms size 為 `15`。
- 回傳完整 Elasticsearch response 物件，不只回傳 buckets 或原始 JSON 字串。
- 不新增 production dependency。
- 不使用 `async`／`await`。

## 現況分析

- `ElasticsearchHelper.GetEachApiErrorCount` 已存在於尚未提交的工作區異動中，但目前錯誤呼叫 `/_cat/indices/*log*` 且固定回傳 `null`。
- 現有 `ESSearchAPIResponse<T>` 僅承接一般搜尋結果，沒有 `_shards` 與 `aggregations`，且其 `hits` 結構不符合本次完整 response 契約。
- 既有 Elasticsearch 呼叫透過 `InstanceRequestPrameter` 建立驗證與連線設定，再由 `APIRequestHelper.RequestApiResult<T>` 發送並反序列化。
- 工作區另有其他未提交異動；本次不修改或回復那些內容。

## 建議方案

1. 在 Elasticsearch response model 檔案新增本功能專用的強型別 response models，完整承接：
   - `took`
   - `timed_out`
   - `_shards`
   - `hits.total`、`hits.max_score`、`hits.hits`
   - `aggregations.group_by_category.doc_count_error_upper_bound`
   - `aggregations.group_by_category.sum_other_doc_count`
   - `aggregations.group_by_category.buckets[].key`
   - `aggregations.group_by_category.buckets[].doc_count`
2. 將 `GetEachApiErrorCount` 的回傳型別改為上述完整 response model。
3. 使用既有 index 名稱轉換方法與 `InstanceRequestPrameter`，送出指定 aggregation query。
4. 回傳 `apiResult.ResultObject`，讓呼叫端取得完整已反序列化 response。

採用專用 model 可避免以 `object` 或動態型別取值，也不影響既有一般搜尋的 `ESSearchAPIResponse<T>`。

## 預計修改範圍

- `target.Utility/Helper/Elasticsearch/ElasticsearchHelper.cs`
  - 完成 `GetEachApiErrorCount` 查詢與強型別回傳。
- `target.Utility/Helper/Elasticsearch/Model/ElasticsearchResponseModel.cs`
  - 新增本次 aggregation response models。
- 視現有測試專案可測試性，於 `target.Testing` 增加 response 反序列化或相關行為測試；不為測試而公開 production-private 方法。

## API 與資料契約

### 輸入

- `indexType`：轉換成小寫 Elasticsearch index 名稱。
- `begin`：`CreateTime.gte`。
- `end`：`CreateTime.lte`。

### Elasticsearch request

```json
{
  "size": 0,
  "query": {
    "bool": {
      "must": [
        {
          "range": {
            "CreateTime": {
              "gte": "yyyy-MM-ddTHH:mm:ss",
              "lte": "yyyy-MM-ddTHH:mm:ss"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "group_by_category": {
      "terms": {
        "field": "RelativeUrl.keyword",
        "size": 15
      }
    }
  }
}
```

### 回傳

回傳完整、專用的 Elasticsearch aggregation response model。欄位名稱依既有專案風格直接對應 Elasticsearch JSON 欄位名稱。

這是尚未完成方法的契約具體化，目前沒有既有可用行為需要維持；其他 Elasticsearch Helper 方法不變。

## 替代方案

- 回傳 `string`：可完整保留 JSON，但呼叫端每次都要自行反序列化，不符合已確認的 response 物件需求。
- 回傳 `object`／動態型別：編譯期無法檢查欄位，容易在後續告警邏輯取值時出錯。
- 擴充既有 `ESSearchAPIResponse<T>`：會把 aggregation 專用結構混入一般搜尋模型，且既有 `hits` model 與本次 JSON 結構不同，因此不採用。

## 風險與待確認事項

- Elasticsearch 若回傳非成功狀態或內容無法反序列化，行為將沿用 `APIRequestHelper` 現有處理方式。
- `hits.max_score` 在範例為 `null`，新 model 必須使用 nullable 型別。
- `hits.hits` 在本查詢因 `size: 0` 應為空陣列，但仍會納入完整 response model。
- 本次不實際連線 Elasticsearch，以免觸及未明確授權的環境；驗證以建置及離線測試為主。

## 驗收標準

- 傳入 enum 與時間區間後，目標 URL 為 `/{轉換後的 index 名稱}/_search`，HTTP method 為 POST。
- request JSON 與已確認的 range、aggregation field、aggregation size 相符。
- 範例 JSON 可反序列化成完整 response model。
- 呼叫端可由回傳物件讀取 `aggregations.group_by_category.buckets`，同時保留其他頂層 response 資訊。
- 不修改本次範圍外的既有異動。
- 修改檔案均為 CRLF，且 `git diff --check` 通過。

## 測試計畫

- 新增或執行針對範例 Elasticsearch JSON 的反序列化測試，確認 nullable `max_score`、hits total、shards 與 aggregation buckets。
- 執行受影響專案的 `dotnet build`。
- 執行相關測試（若測試可在不連線 Elasticsearch 的情況下隔離執行）。
- 執行 `git diff --check`。
- 檢查本次異動文字檔案的 LF-only 行數為 0。
