-- =============================================
-- Author:	 alan
-- Create date: 2018/03/22
-- Description:	聯運專用: 批量更新兌換紀錄並回傳給Center
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_UnionPullConvertLog]
(
	@tnyFlag tinyint,		 -- 兌換狀態 
	@chvNumber varchar(100), -- 篩選方式 (AL：所有记录，JS：所有奇数记录，OS：所有偶数记录，"1,2":表示获取尾数为1或者2的所有记录)
	@chvUserName	VARCHAR(16) --操作者
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Temp_ConvertLogID	TABLE (ID INT)
	DECLARE @tnyStatus	tinyint = 0

	--取全部記錄
	IF @chvNumber = 'AL' 
	BEGIN
		SET @chvNumber = '1,2,3,4,5,6,7,8,9,0'
	END	
	--JS：所有奇数记录
	IF @chvNumber = 'JS' 
	BEGIN
		SET @chvNumber = '1,3,5,7,9'
	END
	--OS：所有偶数记录
	IF @chvNumber = 'OS' 
	BEGIN
		SET @chvNumber = '2,4,6,8,0'
	END

	SELECT	ID,
			UserID as user_id,
			'' as gold_pre,
			'' as gold_next,
			ISNULL(BankAmount,0) as gold_bank,
			ABS(Amount/100) as gold_req,
			'' as gold_req_get,
			TaxAmount as fee,
			CreateIP as ip,
			(CASE WHEN ConvertType = 1 THEN Account ELSE '' END) AS alipay_account,
			(CASE WHEN ConvertType = 1 THEN Realname ELSE '' END) AS alipay_name,
			(CASE WHEN ConvertType = 1 THEN '' ELSE Account END) AS union_pay_account,
			(CASE WHEN ConvertType = 1 THEN '' ELSE Realname  END) AS union_pay_name,
			'' as total_req,
			'' as total_get,
			'' as recharge_total,
			'' as recharge_delegate,
			Convert(varchar,CreateTime,120) as time_start,
			'' as time_end,
			(CASE [status]
			WHEN 0 THEN 0 --提出兌換
			WHEN 9 THEN 1 --疑似洗錢
			WHEN 2 THEN 2 --審核通過
			WHEN 8 THEN 3 --已拒絕
			WHEN 4 THEN 4 --打款失敗
			WHEN 3 THEN 6 --兌換成功
			END) as [status],
			'' as link_id,
			ConvertType as exchange_account_type,
			'' as total_win_lose,
			'' as union_pay_bank,
			''  as exchange_black,
			ISNULL(Operator,'') as operator,
			0 as export,
			'' as [file_name]

		FROM [Money].[ConvertLog] WITH (NOLOCK)

	WHERE  RIGHT(convert(varchar,ID),1) IN (select value from dbo.fn_Split(@chvNumber,','))
	AND [Status] = @tnyStatus
	AND ConvertType = 1
	AND PlatformID in (select PlatformID from CenterDB.area.UnionPlatform with (nolock) where PlatformID <> 110) --只取得聯運平台的兌換清單, 但110 (77棋牌)例外

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_UnionPullConvertLog] TO [GameUser]
    AS [dbo];

