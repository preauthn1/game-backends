-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<代理转账功能>
--	2018/03/23	Jason		調整寫法改為LINKEDSERVER寫資料
--							@intSourceID 目前無使用
--	2018/04/03	Jason		新增PlatformID AreaID
--	2018/04/18	alan		add transaction
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_Transfer]
(
	@intAgentID				INT,					--代理ID
	@intUserID				INT,					--接收转账的用户ID
	@lngAmount				BIGINT,					--修改数量
	@chvIPAddress			VARCHAR(15),			--IP地址
	@chvErrMsg				VARCHAR(128) OUTPUT		--返回消息
)
AS
BEGIN
	SET NOCOUNT ON

	set xact_abort on

	DECLARE @intReturnValue		INT
	DECLARE @intAgentUserID		INT
	DECLARE @chvMemo			VARCHAR(128)
	DECLARE @lngSelfAmount		BIGINT
	DECLARE @lngRealtionAmount	BIGINT
	DECLARE @NowTime			DATETIME = GETDATE()
	DECLARE @intRelationAgentID INT
	DECLARE @intPlatformID		INT
	DECLARE @intAreaID			INT
	--DECLARE @intSourceID INT

	SELECT	@intAgentUserID = A.UserID,
			@intRelationAgentID = AgentID,
			@intPlatformID = C.PlatformID,
			@intAreaID = B.AreaID
	FROM Agent.Users AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserInfo] AS B WITH (NOLOCK) ON A.UserID = B.UserID
		LEFT JOIN [Area].[Area] AS C WITH (NOLOCK) ON B.AreaID = C.AreaID
	WHERE AgentID = @intAgentID

	IF @intAgentUserID IS NULL
	BEGIN
		SET @chvErrMsg = '代理不存在'
		RETURN -1
	END

	IF NOT EXISTS(SELECT 1 FROM Users.UserInfo WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		SET @chvErrMsg = '用户ID不存在'
		RETURN -1
	END

	IF NOT EXISTS(SELECT 1 FROM Money.Cash WITH (NOLOCK) WHERE UserID=@intAgentUserID AND Amount-ABS(@lngAmount)>0)
	BEGIN
		SET @chvErrMsg = '操作失败:金币不足'
		RETURN -1
	END

	
	BEGIN TRAN


	--扣除转账用户金币
	SET @intReturnValue = 0
	SET @lngAmount = ABS(@lngAmount) * -1
	SET @chvMemo = '转账给用户:'+CAST(@intUserID as varchar)+',数量:'+CAST(ABS(@lngAmount) as varchar)
	EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash 
		@intAgentUserID,
		14001,		--转账类型
		@lngAmount,
		@chvIPAddress,	
		@chvMemo,
		@lngSelfAmount OUTPUT,
		@chvErrMsg OUTPUT

	IF @intReturnValue<>1
	BEGIN
		SET @chvErrMsg = '操作失败:扣除金币失败'
		ROLLBACK TRAN
		RETURN -1
	END

	--添加接收用户的金币
	SET @intReturnValue = 0
	SET @lngAmount = ABS(@lngAmount)
	SET @chvMemo = '收到用户:'+CAST(@intAgentUserID as varchar) + '转账,数量:' + CAST(@lngAmount as varchar)
	EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash 
		@intUserID,
		14001,		--转账类型
		@lngAmount,
		@chvIPAddress,	
		@chvMemo,
		@lngRealtionAmount OUTPUT,
		@chvErrMsg OUTPUT

	IF @intReturnValue<>1
	BEGIN
		SET @chvErrMsg = '操作失败:添加金币失败'
		ROLLBACK TRAN
		RETURN -1
	END

	/*  --写交易记录  */
	------------------------------------------------------------------------------------------------

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_AddAgentTransferLog]
		@intAgentID = @intAgentID,
		@intRelationUserID = @intUserID,
		@intRelationAgentID = @intRelationAgentID,
		@lngSelfAmount = @lngSelfAmount,
		@lngRelationAmount = @lngRealtionAmount,
		@lngAmount = @lngAmount,
		@chvIPAddress = @chvIPAddress,
		@dtStatTime = @NowTime,
		@intPlatformID = @intPlatformID,
		@intAreaID = @intAreaID

	--更新代理交易日报表
	UPDATE Stat.AgentTransferSourceLog
	SET Amount = ABS(@lngAmount)*-1
	WHERE
		AgentID = @intAgentID
		AND DATEDIFF(DAY,StatTime,GETDATE()) = 0

	IF (@intRelationAgentID IS NOT NULL)
	BEGIN
		--更新代理交易日报表
		UPDATE Stat.AgentTransferSourceLog
		SET Amount = ABS(@lngAmount)
		WHERE
			AgentID = @intRelationAgentID
			AND DATEDIFF(DAY,StatTime,GETDATE()) = 0
	END
	------------------------------------------------------------------------------------------------

	COMMIT TRAN

	SET @chvErrMsg = '操作成功'
	RETURN @intReturnValue
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_Transfer] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_Transfer] TO [xbuser]
    AS [dbo];

