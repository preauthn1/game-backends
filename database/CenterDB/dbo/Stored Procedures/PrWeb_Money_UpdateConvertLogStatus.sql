-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改兑换状态>

--20180315 harry增加:
--移除日期判斷
--CreateTime > dateadd(dd,-1,GETDATE())
--	2018/03/22	Jason		調整為呼叫LINKEDSERVER SP執行
--	2018/04/24	Jason		新增@chvUserName 操作者
--	2018/05/16	Jason		#256 新增兌換功能拒絕兌換歸還用戶金幣
--  2018/05/20	alan		#256 改為退還到銀行
--	2018/05/22	alan		更新用戶基本數據 SumConvert SumConvertTime
--  2019/07/11	CCC			備註增加兌換單號
--  2019/07/15	CCC			增加备注栏位
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_UpdateConvertLogStatus]
	@intId			int,	--ID
	@tnyStatus		int,	--状态
	@chvUserName	VARCHAR(16),
	@chvMemo		VARCHAR(200),
	@intGWID		int = 0
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intRtnCode	INT
	DECLARE @chvRtnMsg	VARCHAR(64)
	declare @platformid int
	declare @areaid int

	

	DECLARE @lngReturnAmount BIGINT,@intUserID INT,@lngConvertAmount BIGINT,@lngConvertTax bigint


	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Money_UpdateConvertLogStatus]
		@intId				= @intId,
		@tnyStatus			= @tnyStatus,
		@chvUserName		= @chvUserName,
		@chvMemo			= @chvMemo,
		@intGWID			= @intGWID,
		@intplatformid		= @platformid output,
		@intareaid			= @areaid output,
		@intUserID			= @intUserID			OUTPUT,
		@lngConvertAmount	= @lngConvertAmount		OUTPUT,
		@lngReturnAmount	= @lngReturnAmount		OUTPUT,
		@intRtnCode			= @intRtnCode			OUTPUT,
		@chvRtnMsg			= @chvRtnMsg			OUTPUT

	IF @tnyStatus = 11 AND @intUserID IS NOT NULL AND @intRtnCode = 1
	BEGIN 
		declare @chvMemo1 varchar(100) =  '兑换退款返还到用户保险箱金币[' + cast(@intId as varchar) + ']'
		--兑换拒絕返还用户金币 
		--exec dbo.PrPsWeb_UpdateUserCash @intUserID,14002,@lngReturnAmount,'0.0.0.0','兑换拒絕返还用户金币',null,0,@areaid,@platformid
		exec @intRtnCode = dbo.PrPsWeb_UpdateUserBank @intUserID,12000,@lngReturnAmount,'0.0.0.0',@chvMemo1,0,@chvRtnMsg out
		IF @intRtnCode = 1
		BEGIN
	
			--记录现金记录	
			Declare @lngCurrentAmount bigint
			SELECT	@lngCurrentAmount = Amount FROM Money.Cash WITH (NOLOCK) WHERE UserID = @intUserID
			EXECUTE LOGDBLINKEDSERVER.LogDB.dbo.PrPsWeb_AddMoneyLog
			   @intUserID			= @intUserID
			  ,@intSourceTypeID		= 14002
			  ,@lngCurrentAmount	= @lngCurrentAmount
			  ,@lngUpdateAmount		= @lngReturnAmount	  
			  ,@chvIP				= '0.0.0.0'
			  ,@chvMemo				= @chvMemo1
			  ,@lngTaxAmount		= 0
			  ,@lngFlowAmount		= 0
			  ,@intAreaID			= @areaid
			  ,@intPlatformID		= @platformid	
	

		
			SET @lngConvertTax = (@lngReturnAmount-@lngConvertAmount)
			--更新用户数据
			UPDATE Users.UserInfo
			SET 
			SumConvert = SumConvert - ABS(@lngConvertAmount)
			,SumConvertTimes = SumConvertTimes - 1
			,SumConvertTax = SumConvertTax - @lngConvertTax
			WHERE UserID = @intUserID AND SumConvertTimes > 0
		
			DECLARE @chvMailContent VARCHAR(1024),@chvMailTitle varchar(128)  
			SET @chvMailTitle = '兑换退还' 
			set @chvMailContent = '您申请的兑换' + dbo.Fn_ShowMoney(@lngConvertAmount) + '金币订单已被退还，系统已返还兑换金币与手续费共'+dbo.Fn_ShowMoney(@lngReturnAmount)+'金币到保险箱 （如有任何疑问请联系客服）'
		
			--发送系统邮件
			EXEC PrPs_SendSysMail
				@intUserID,
				@chvMailTitle,
				@chvMailContent,
				0,
				NULL
		END
	END


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_UpdateConvertLogStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_UpdateConvertLogStatus] TO [xbuser]
    AS [dbo];

