-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<用户注册-公共逻辑>
--	2018/04/10	Jason		新增UpdateUserCash時一併寫入AreaID & PlatformID
--	2018/04/12	Jason		新增用戶建立時一併新增Bank & Cash Record及調整流程
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_CreateAccount_Common]
(
	@tnyHDType		TINYINT,		--硬件类型
	@chvHDCode		VARCHAR(64),	--硬件码
	@chvIPAddress	VARCHAR(15),	--IP地址
	@intUserID		INT,			--用户ID
	@tnyRegType		TINYINT         --1 账号注册
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @NowTime				DATETIME = GETDATE()
	DECLARE @UserID					INT = @intUserID
	DECLARE @lngCurrentAmount		BIGINT
	DECLARE @AreaID					INT
	DECLARE @PlatFormID				INT	
	DECLARE @NewAccountSourceTypeID	INT = 10001
	DECLARE @MemoRegMoney			VARCHAR(128) = '注册送金币'
	DECLARE @intRegMoney			INT  -- 注册送金币金額
	DECLARE @intMsgID				INT
	DECLARE @MessageTitle			VARCHAR(128) = '欢迎来到棋牌游戏世界'
	DECLARE @MessageSort			INT = 0
	DECLARE @chvNickName			VARCHAR(32) = CAST(@UserID AS VARCHAR)
	DECLARE @chvMsgContent			VARCHAR(1024)

	
	--取用戶對應平台資料
	SELECT	@AreaID = [AreaID] FROM [Users].[UserInfo] WITH (NOLOCK) WHERE [UserID] = @UserID
	SELECT	@PlatFormID = [PlatformID], @intRegMoney=DefaultMoney FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID] = @AreaID


	SET @AreaID		= ISNULL(@AreaID,0)
	SET @PlatFormID	= ISNULL(@PlatFormID,0)
	SET @intRegMoney = ISNULL(@intRegMoney,0)

	SET @chvMsgContent = '亲爱的玩家：' + '
		恭喜你注册成功，欢迎来到棋牌游戏世界.你想要的应有尽有，赶快开始体验吧，祝你游戏愉快！
                                                                                                                                  
	（如有任何疑问请联系客服）'

	/*  創建用戶銀行及現金紀錄  */
	-------------------------------------------------------------------------------------------
	INSERT INTO [Money].[Bank] ([UserID],[Amount],[Code]) VALUES (@UserID,0,'')
	INSERT INTO [Money].[Cash] ([UserID],[Amount],[Code]) VALUES (@UserID,0,'')

	IF @intRegMoney > 0
	BEGIN

		--执行送金币操作 注册送金币
		EXEC [dbo].PrPsWeb_UpdateUserCash
			@intUserID			= @UserID,
			@sintSourceType		= @NewAccountSourceTypeID,
			@lngUpdateAmount	= @intRegMoney,
			@chvIPAddress		= @chvIPAddress,
			@chvMemo			= @MemoRegMoney,
			@lngCurrentAmount	= @lngCurrentAmount		OUTPUT,
			@lngTaxAmount		= 0,
			@intAreaID			= @AreaID,
			@intPlatformID		= @PlatFormID

	END
	-------------------------------------------------------------------------------------------


	/*  --发送系统邮件  */
	-------------------------------------------------------------------------------------------
	EXEC PrPs_SendSysMail
		@intUserID		= @UserID,
		@chvTitle		= @MessageTitle,
		@chvContent		= @chvMsgContent,
		@intSort		= @MessageSort,
		@intMsgID		= @intMsgID	OUTPUT
	-------------------------------------------------------------------------------------------


	/*  寫入新增用戶紀錄  */
	-------------------------------------------------------------------------------------------
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_AddUserLog]
		@tnyLogType		= @tnyRegType,
		@intUserID		= @UserID,
		@dtStatTime		= @NowTime,
		@tnyHDType		= @tnyHDType,
		@chvHDCode		= @chvHDCode,
		@chvIPAddress	= @chvIPAddress,
		@intAreaID		= @AreaID,
		@intPlatformID	= @PlatformID
	-------------------------------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_CreateAccount_Common] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_CreateAccount_Common] TO [xbuser]
    AS [dbo];

