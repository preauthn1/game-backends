-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<根据用户ID获取用户详细信息>
--	2018/03/21	Jason		調整為用LINKEDSERVER取稅收及兌換統計紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_GetUserDetailInfo]
    @intUserID		INT ,
    @dtBeginTime	DATETIME = NULL,	--时间开始范围
    @dtEndTime		DATETIME = NULL		--时间结束范围
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @dtBeginTime = ISNULL(@dtBeginTime,GETDATE())
    SET @dtEndTime = ISNULL(@dtEndTime,dateadd(dd,-1,GETDATE()))
        
    DECLARE 
		@chvRegIP			VARCHAR(15),	--用户注册IP
		@chvHDCode			VARCHAR(64),	--用户注册Mac
		@lngSameIPCount		INT,			--同IP账号数
		@lngSameMacCount	INT,			--同MAC账号数
		@lngRangSumCharge	BIGINT,			--查询范围内总充值金额
		@lngRangSumConvert	BIGINT,			--查询范围内总兑换金额
		@lngRangSumTax		BIGINT			--查询范围内贡献总税收金额		
	
				
	SELECT @chvRegIP= CreateIPAddress,
		   @chvHDCode = HDCode
	FROM Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	--统计相同注册IP
	SELECT	@lngSameIPCount = COUNT(1)
	FROM Users.UserInfo WITH (NOLOCK)
	WHERE
		CreateIPAddress = @chvRegIP

	--统计相同注册Mac
	SELECT	@lngSameMacCount = COUNT(1)
	FROM Users.UserInfo WITH (NOLOCK)
	WHERE
		HDCode = @chvHDCode		
	
	--统计充值
	SELECT @lngRangSumCharge = ISNULL(SUM(Amount) ,0)
	FROM Pay.OrderList WITH (NOLOCK)
	WHERE
		CreateTime BETWEEN @dtBeginTime AND @dtEndTime
		AND UserID = @intUserID
		AND Status = 2
	
	
	/*  统计兑换 & 统计游戏税收  */
	--------------------------------------------------------------------------------
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetTaxAmountConvertByUserID]
		@intUserID = @intUserID,
		@dtStatTime = @dtBeginTime,
		@dtEndTime = @dtEndTime,
		@bintTaxAmount = @lngRangSumTax				OUTPUT,
		@bintConvertAmount = @lngRangSumConvert		OUTPUT
	--------------------------------------------------------------------------------
		
	PRINT @lngRangSumTax
	
	SELECT  
		@lngSameIPCount AS SameIPCount,		--同IP账号数
		@lngSameMacCount AS SameMacCount,		--同MAC账号数
		@lngRangSumCharge AS SumCharge,		--查询范围内总充值金额
		ABS(@lngRangSumConvert) AS SumConvert,	--查询范围内总兑换金额
		@lngRangSumTax AS SumGameTax			--查询范围内贡献总税收金额
				
				
    RETURN 1;
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserDetailInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserDetailInfo] TO [xbuser]
    AS [dbo];

