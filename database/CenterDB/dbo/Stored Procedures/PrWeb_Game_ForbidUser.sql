-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<封锁用户>
--	2018/04/10	Jason	新增UpdateUserCash寫入PlatformID & AreaID
--  2018/08/03	alan	新增封锁帐号时一并封锁电话与支付宝帐号
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_ForbidUser]
(
	@intUserID			INT,					--用户ID
	@chvMemo			VARCHAR(128),			--说明
	@chvIPAddress		VARCHAR(15),			--IP地址
	@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
)
AS
BEGIN
	
	SET NOCOUNT ON
	DECLARE @tnyUserType		TINYINT
	DECLARE @chvLoginName		VARCHAR(32)
	DECLARE @chvAlipayAccount	VARCHAR(64)
	DECLARE @AreaID				INT
	DECLARE @PlatformID			INT
	DECLARE @lngCashAmount		BIGINT
	DECLARE	@lngBankAmount		BIGINT
	DECLARE @lngCurrentAmount	BIGINT

	SET @chvErrMsg = '操作成功:封锁用户成功'

	IF EXISTS(SELECT 1 FROM Game.GameUserList WHERE UserID=@intUserID)
	BEGIN
		SET @chvErrMsg = '操作失败:用户在游戏房间中,请先踢出后再封锁'
		RETURN -1
	END


	/*  取用戶當下資料  */
	-----------------------------------------------------------------------------------------
	SELECT	@AreaID = [AreaID], @tnyUserType=UserType,@chvLoginName=LoginName,@chvAlipayAccount=ISNULL(AlipayAccount,'') FROM [Users].[UserInfo] WITH (NOLOCK) WHERE [UserID] = @intUserID
	SELECT	@PlatformID = [PlatformID] FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID] = @AreaID
	
	SET @AreaID		= ISNULL(@AreaID,0)
	SET @PlatformID = ISNULL(@PlatformID,0)

	SELECT @lngCashAmount = Amount FROM [Money].[Cash] WHERE UserID = @intUserID
	SELECT @lngBankAmount = Amount FROM [Money].[Bank] WHERE UserID = @intUserID

	SET @lngCashAmount = ABS(ISNULL(@lngCashAmount,0))*-1
	SET @lngBankAmount = ABS(ISNULL(@lngBankAmount,0))*-1
	-----------------------------------------------------------------------------------------


	--封锁用户
	-----------------------------------------------------------------------------------------
	UPDATE Game.ForbidUser
	SET CashAmount = @lngCashAmount,
		BankAmount = @lngBankAmount,
		Memo = @chvMemo
	WHERE
		UserID = @intUserID
	
	IF @@ROWCOUNT = 0
	BEGIN
		INSERT INTO Game.ForbidUser
		SELECT @intUserID,@lngCashAmount,@lngBankAmount,GETDATE(),@chvMemo
	END
	

	--如果帐号是正式帐号, 封锁loginname(电话）
	------------------------------------------------------------------------------------------
	IF @tnyUserType = 1 AND NOT EXISTS(Select 1 FROM Game.ForbidAccount WITH(NOLOCK) WHERE AccountID=@chvLoginName AND IdType=1)
	BEGIN
		INSERT INTO  Game.ForbidAccount(AccountID, IdType, Memo) Values(@chvLoginName, 1, '管理员封锁用户ID,同时封锁正式帐号')

	END	


	--如果有支付宝帐号, 封锁支付宝帐号
	------------------------------------------------------------------------------------------
	IF @chvAlipayAccount <> '' AND NOT EXISTS(Select 1 FROM Game.ForbidAccount WITH(NOLOCK) WHERE AccountID=@chvAlipayAccount AND IdType=2)
	BEGIN
		INSERT INTO  Game.ForbidAccount(AccountID, IdType, Memo) Values(@chvAlipayAccount, 2, '管理员封锁用户ID,同时封锁支付宝帐号')

	END	



	--封锁现金
	EXEC dbo.PrPsWeb_UpdateUserCash
		@intUserID			= @intUserID,
		@sintSourceType		= 13002,
		@lngUpdateAmount	= @lngCashAmount,
		@chvIPAddress		= @chvIPAddress,
		@chvMemo			= '管理员封锁用户扣金币',
		@lngCurrentAmount	= @lngCurrentAmount OUTPUT,
		@lngTaxAmount		= 0,
		@intAreaID			= @AreaID,
		@intPlatformID		= @PlatformID
	
	--封锁银行金币
	EXEC dbo.PrPsWeb_UpdateUserBank
		@intUserID,
		13002,		--封锁扣除金币
		@lngBankAmount,
		@chvIPAddress,
		'管理员封锁用户扣金币',
		NULL,
		NULL
	
	--写封锁记录
	INSERT INTO Game.ForbidUserLog
	SELECT @intUserID,0,@lngCashAmount,@lngBankAmount,@chvMemo,@chvIPAddress,GETDATE()
	-----------------------------------------------------------------------------------------

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_ForbidUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_ForbidUser] TO [xbuser]
    AS [dbo];

