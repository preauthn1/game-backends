








-- =============================================
-- Author:  harry
-- Create date: 2018/10/1
-- Description:	代理金币移置游戏帐号
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentWithdraw]
	@AgentUID			uniqueidentifier,
	@intWithdrawAmount	bigint,
	@IPAddress			varchar(50),
	@chvErrMsg		varchar(50) OUT
AS
BEGIN	
	DECLARE @intReturnValue		INT
	DECLARE @chvMemo			VARCHAR(128)
	DECLARE @lngSelfAmount		BIGINT
	DECLARE @remainAmount bigint --钱包金额
	DECLARE @intUserID int
	DECLARE @intAllowWithdraw int --是否允许提现

	SELECT 
	@intUserID = UserID,
	@remainAmount = RemainAmount,
	@intAllowWithdraw = AllowWithdraw
	FROM dbo.AgentUsers with (nolock) WHERE ID = @AgentUID



	IF @intAllowWithdraw=0
	BEGIN
		SET @chvErrMsg = '您的游戏帐号不允许提现，请联系管理员'
		RETURN -1
	END

	IF @intUserID is NULL OR @intUserID=0
	BEGIN
		SET @chvErrMsg = '您的游戏帐号尚未设定，请先设定个人资讯'
		RETURN -1
	END

	IF @intWithdrawAmount > @remainAmount
	BEGIN
		SET @chvErrMsg = '不可提取超过钱包金额'
		RETURN -1
	END

	SELECT TOP 1 UserID FROM [CenterDB].[Users].[UserInfo] with (nolock) WHERE UserID = @intUserID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '游戏帐号不存在，请重新设定'
		RETURN -1
	END

	--转账到用户遊戲錢包
	SET @intReturnValue = 0
	SET @chvMemo = '推广代理转帐金币:'+CAST(@intUserID as varchar)+',数量:'+ CAST(@intWithdrawAmount as varchar)
	EXEC @intReturnValue = [CenterDB].dbo.PrWeb_UpdateUserCash 
		@intUserID,
		14001,		--转账类型
		@intWithdrawAmount,
		@IPAddress,	
		@chvMemo,
		@lngSelfAmount OUTPUT,
		@chvErrMsg OUTPUT

	IF @intReturnValue<>1
	BEGIN
		SET @chvErrMsg = @chvErrMsg
		RETURN -1
	END

	INSERT INTO  dbo.WithdrawLog(YYYYMMDD, ID, CurrentAmount, UpdateAmount, RemainAmount, Type, Memo) 
	Values (convert(varchar, getdate(), 112), @AgentUID, @remainAmount, @intWithdrawAmount * -1, @remainAmount - @intWithdrawAmount, 0, '代理提取金币')

	--更新金额
	UPDATE dbo.AgentUsers SET RemainAmount -= @intWithdrawAmount 
	WHERE ID = @AgentUID
	
	SET @chvErrMsg = '提取成功'
	RETURN 1
END



