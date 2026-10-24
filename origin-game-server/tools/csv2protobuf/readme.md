### csv 轉檔工具
1. 將 csv 檔轉換成 protobuf
2. 將 protobuf 轉換成 golang struct
3. 將 csv 資料載入, 並匯出為 bytes 檔

### csv 檔定義
1. excel 的前五行用于结构定义, 其余则为数据，按第一行区分, 分别解释：
第一行 all/client/server 生產分類, all 表示為client server都要產出
第二行 required/optional/repeated 欄位屬性
第三行 int32/bytes/float/string 欄位
第四行 欄位名
第五行 注釋
第六行以下為數據