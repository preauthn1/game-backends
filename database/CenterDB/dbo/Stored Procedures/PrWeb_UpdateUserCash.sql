-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改用户金币>
--	2018/04/10	Jason		新增UpdateUserCash時寫入PlatformID & AreaID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdateUserCash]
	@intUserID				INT,					--用户ID
	@sintSourceType			SMALLINT,				--金币变化类型
	@lngUpdateAmount		BIGINT,					--更新金币数	
	@chvIPAddress			VARCHAR(15),			--IP地址
	@chvMemo				VARCHAR(128),			--备注
	@lngCurrentAmount		BIGINT OUTPUT,			--修改前的金币数
	@chvErrMsg			VARCHAR(128) OUTPUT		--错误消息
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @UserID			INT = @intUserID
	DECLARE @SourceType		SMALLINT = @sintSourceType
	DECLARE @IPAddress		VARCHAR(40) = @chvIPAddress
	DECLARE @Memo			VARCHAR(128) = @chvMemo
	DECLARE @AreaID			INT
	DECLARE @PlatformID		INT

	IF @lngUpdateAmount=0
	BEGIN
		SET @chvErrMsg = '操作成功'
		RETURN 1
	END
	
	--如果是扣金币操作,需要检查金币锁
	IF @lngUpdateAmount<0
	BEGIN
		IF EXISTS(SELECT 1 FROM Game.GameUserList WITH(NOLOCK) WHERE UserID=@intUserID)
		BEGIN
			SET @lngCurrentAmount = 0
			SET @chvErrMsg = '操作失败:帐号在游戏房间中'
			RETURN -1
		END
	END

	--取用戶資料
	SELECT	@AreaID = [AreaID] FROM [Users].[UserInfo] WITH (NOLOCK) WHERE [UserID] = @UserID
	SELECT	@PlatformID = [PlatformID] FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID] = @AreaID
	
	SET @AreaID		= ISNULL(@AreaID,0)
	SET @PlatformID = ISNULL(@PlatformID,0)
	
	--更新金币
	DECLARE @intRet int
	EXEC @intRet = dbo.PrPsWeb_UpdateUserCash
		@intUserID			= @UserID,
		@sintSourceType		= @SourceType,
		@lngUpdateAmount	= @lngUpdateAmount,
		@chvIPAddress		= @IPAddress,
		@chvMemo			= @Memo,
		@lngCurrentAmount	= @lngCurrentAmount		OUTPUT,
		@lngTaxAmount		= 0,
		@intAreaID			= @AreaID,
		@intPlatformID		= @PlatformID
		
	
	IF @intRet <> 1
	SET @chvErrMsg = '操作失败:金币不足'
	ELSE
	SET @chvErrMsg = '修改成功'
	
	RETURN @intRet
	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateUserCash] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateUserCash] TO [xbuser]
    AS [dbo];

