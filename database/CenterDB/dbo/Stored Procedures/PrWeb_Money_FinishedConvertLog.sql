
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<完成兑换订单结果>
-- Changelog:
--		20180312 alan增加:
--		增加銀聯卡支援

--		20180314 harry增加:
--		@tnyConvertType 判斷 1.支付寶 其它為銀行卡
--		修改 Log.ConvertLog Status 判斷 (原為4,7)

--		20180316 harry增加:
--		修改 Log.ConvertLog Status 判斷 2,99

--		20180322 Jason增加:
--		修改Update Log.ConvertLog 為呼叫SP執行更新
--	2018/04/24	Jason		新增@chvUserName參數
--	2018/05/01	alan		更改呼叫sp名稱 PrPs_UPDConvertLog -> PrWeb_Money_FinishedConvertLog
--	2019/05/11	Jason		增加裂變檢查
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_FinishedConvertLog]
	@intId				INT,				--订单ID
	@tnyStatus			TINYINT,			--订单状态
	@lngRealTaxAmount	BIGINT=null,		--提现实际手续费
	@chvTransID			VARCHAR(64) = NULL,	--提现订单号(失败时没有值)
	@chvTransData		VARCHAR(64) = NULL,	--提现结果备注说明
	@chvUserName		VARCHAR(16),		--操作者
	@chvErrMsg			VARCHAR(64) OUTPUT	--返回消息	
AS
BEGIN

	SET NOCOUNT ON

	--DECLARE @dtStatTime			DATETIME = '2018-03-01 00:00:00.000'
	DECLARE @dtUpdateTime			DATETIME = GETDATE()
	DECLARE @intUserID			INT = NULL
	DECLARE @lngTaxAmount		BIGINT = NULL
	DECLARE @lngConvertAmount	BIGINT = NULL
	DECLARE @lngReturnAmount	BIGINT = NULL
	DECLARE @tnyConvertType		BIGINT = NULL
	DECLARE @chvAccount			VARCHAR(64) = ''
	DECLARE @intRtnCode			INT	= 0


	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Money_FinishedConvertLog]
		@intId				= @intId,
		@lngRealTaxAmount	= @lngRealTaxAmount,
		@tnyStatus			= @tnyStatus,
		@chvTransID			= @chvTransID,
		@chvTransData		= @chvTransData,
		@dtUpdateTime		= @dtUpdateTime,
		@chvUserName		= @chvUserName,
		@intUserID			= @intUserID			OUTPUT,
		@lngTaxAmount		= @lngTaxAmount			OUTPUT,
		@lngConvertAmount	= @lngConvertAmount		OUTPUT,
		@lngReturnAmount	= @lngReturnAmount		OUTPUT,
		@tnyConvertType		= @tnyConvertType		OUTPUT,
		@chvAccount			= @chvAccount			OUTPUT,
		@intRtnCode			= @intRtnCode			OUTPUT,
		@chvRtnMsg			= @chvErrMsg 			OUTPUT
	-------------------------------------------------------------------------------------------------------

	IF (@intRtnCode = 1)
	BEGIN	
		DECLARE @chvMailContent VARCHAR(1024),@chvMailTitle varchar(128), @strPayName varchar(10)
			
		UPDATE Stat.ConvertLog 
		SET Amount = Amount + @lngConvertAmount
		,TaxAmount = @lngTaxAmount
		,RealTaxAmount = @lngRealTaxAmount			
		WHERE DATEDIFF(DD,StatTime,GETDATE())=0

		IF @tnyConvertType = 1
		BEGIN
			SET @strPayName = '支付宝'
		END
		ELSE
		BEGIN
			SET @strPayName = '银行卡'
		END
			
		SET @chvMailContent = '亲爱的玩家：
			您申请的兑换金币' + dbo.Fn_ShowMoney(@lngConvertAmount) + ' ' + @strPayName + '订单成功，请注意查收
			                                                                                                                                  
		（如有任何疑问请联系客服）'

		SET @chvMailTitle = '兑换成功'	
		SET @chvErrMsg = '订单结果处理成功'			

		--发送系统邮件
		EXEC PrPs_SendSysMail
					@intUserID, 
					@chvMailTitle,
					@chvMailContent,
					0,
					NULL


		IF(@tnyStatus = 3)
		BEGIN
			EXEC Club02.[dbo].[PrWeb_Event_ChargeBonusResetLimit]	
				@intUserID = @intUserID,
				@chvMsg = ''
		END

		EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
			@intUserID = @intUserID,				--用户 ID
			@chvMsg	= ''

	
		RETURN 1	
		
	END
	ELSE
	BEGIN
		return @intRtnCode
	END
					
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_FinishedConvertLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_FinishedConvertLog] TO [xbuser]
    AS [dbo];

