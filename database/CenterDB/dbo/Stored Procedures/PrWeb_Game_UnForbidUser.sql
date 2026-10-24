-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<解封用户>
--	2018/04/10	Jason		新增UpdateUserCash寫入PlatformID & AreaID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_UnForbidUser]
(
	@intUserID			INT,					--用户ID
	@chvMemo			VARCHAR(128),			--说明
	@chvIPAddress		VARCHAR(15),			--IP地址
	@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID				INT = @intUserID
	DECLARE @Memo				VARCHAR(128) = @chvMemo
	DECLARE @IPAddress			VARCHAR(40) = @chvIPAddress
	DECLARE @lngCashAmount		BIGINT
	DECLARE @lngBankAmount		BIGINT
	DECLARE @PlatFormID			INT
	DECLARE @AreaID				INT
	DECLARE @lngCurrentAmount	BIGINT

	SET @chvErrMsg = '操作成功:解封用户成功'
		
	IF NOT EXISTS(SELECT 1 FROM Game.ForbidUser WHERE UserID = @UserID)
	BEGIN
		SET @chvErrMsg = '操作失败:用户已经被解锁,不能重复操作'
		RETURN -1
	END
			
	
	--获取用户封锁金币
	SELECT @lngCashAmount = ABS(CashAmount),@lngBankAmount = ABS(BankAmount) FROM Game.ForbidUser WHERE UserID = @intUserID			
	
	--解封用户
	DELETE FROM Game.ForbidUser WHERE UserID = @intUserID			

	--取用戶資料
	SELECT	@AreaID = [AreaID] FROM [Users].[UserInfo] WITH (NOLOCK) WHERE [UserID] = @UserID
	SELECT	@PlatFormID = [PlatformID] FROM [Area].[Area] WITH (NOLOCK) WHERE [PlatformID] = @PlatFormID
	SET @AreaID		= ISNULL(@AreaID,0)
	SET @PlatFormID	= ISNULL(@PlatFormID,0)
		
	--解封现金
	EXEC dbo.PrPsWeb_UpdateUserCash
		@intUserID			= @UserID,
		@sintSourceType		= 13003,
		@lngUpdateAmount	= @lngCashAmount,
		@chvIPAddress		= @IPAddress,
		@chvMemo			= '管理员解封用户扣金币',
		@lngCurrentAmount	= @lngCurrentAmount	OUTPUT,
		@lngTaxAmount		= 0,
		@intAreaID			= @AreaID,
		@intPlatformID		= @PlatFormID
	
	--解封银行金币
	EXEC dbo.PrPsWeb_UpdateUserBank
		@UserID,
		13003,		--封锁扣除金币
		@lngBankAmount,
		@IPAddress,
		'管理员解封用户扣金币',
		NULL,
		NULL
	
	--写封锁记录
	INSERT INTO Game.ForbidUserLog
	SELECT @UserID,1,@lngCashAmount,@lngBankAmount,@Memo,@IPAddress,GETDATE()
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UnForbidUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UnForbidUser] TO [xbuser]
    AS [dbo];

