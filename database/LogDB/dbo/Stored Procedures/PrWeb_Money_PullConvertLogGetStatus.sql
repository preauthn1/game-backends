-- =============================================
-- Author:		alan
-- Create date: 2018/05/1
-- Description:	取得兌換狀態
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_PullConvertLogGetStatus]
(
	@intID		INT,
	@intStatus	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	SELECT @intStatus = [Status] FROM [Money].[ConvertLog] WITH (NOLOCK)
	WHERE Id = @intID

	

	--SELECT	ID,
	--		UserID as user_id,
	--		'' as gold_pre,
	--		'' as gold_next,
	--		ISNULL(BankAmount,0) as gold_bank,
	--		ABS(Amount) as gold_req,
	--		'' as gold_req_get,
	--		TaxAmount as fee,
	--		CreateIP as ip,
	--		(CASE WHEN ConvertType = 1 THEN Account ELSE '' END) AS alipay_account,
	--		(CASE WHEN ConvertType = 1 THEN Realname ELSE '' END) AS alipay_name,
	--		(CASE WHEN ConvertType = 1 THEN '' ELSE Account END) AS union_pay_account,
	--		(CASE WHEN ConvertType = 1 THEN '' ELSE Realname  END) AS union_pay_name,
	--		'' as total_req,
	--		'' as total_get,
	--		'' as recharge_total,
	--		'' as recharge_delegate,
	--		Convert(varchar,CreateTime,120) as time_start,
	--		'' as time_end,
	--		(CASE [status]
	--		WHEN 0 THEN 0
	--		WHEN 9 THEN 1
	--		WHEN 2 THEN 2
	--		WHEN 8 THEN 3
	--		WHEN 4 THEN 4
	--		WHEN 3 THEN 6
	--		END) as [status],
	--		'' as link_id,
	--		ConvertType as exchange_account_type,
	--		'' as total_win_lose,
	--		'' as union_pay_bank,
	--		''  as exchange_black,
	--		ISNULL(Operator,'') as operator,
	--		0 as export,
	--		'' as [file_name]

	--	FROM [Money].[ConvertLog] WITH (NOLOCK)

	-- WHERE Id = @intID
	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_PullConvertLogGetStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_PullConvertLogGetStatus] TO [xbuser]
    AS [dbo];

