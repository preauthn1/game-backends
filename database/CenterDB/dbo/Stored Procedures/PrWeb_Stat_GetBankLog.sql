-- =============================================
-- Author:  Jason
-- Create date: 2018/04/10
-- Description:	用戶管理-用戶金幣-銀行紀錄查詢 (原ORM轉為SP)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetBankLog]
(
	@intAdminID	int,
	@intUserID			INT = NULL,
	@intSourceType		INT = NULL,
	@chvIPAddress		VARCHAR(40) = NULL,
	@beginTime			DATETIME = NULL,
	@endTime			DATETIME = NULL,
	@intAreaID			INT = NULL,
	@intPlatformID		INT = NULL,
	@intPageIndex		INT = 1,
	@intPageSize		INT = 20,
	@intPageCount		INT				OUTPUT	--记录总数		
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @UserID				INT = @intUserID
	DECLARE @SourceType			INT = @intSourceType
	DECLARE @IPAddress			VARCHAR(40) = @chvIPAddress
	DECLARE @LogStatTime		DATETIME = @beginTime
	DECLARE @LogEndTime			DATETIME = @endTime
	DECLARE @AreaID				INT = @intAreaID


	DECLARE @Temp_GetBankLog	TABLE
		(
			[rowNum]		INT,
			[LogID]			INT,
			[UserID]		INT,
			[SourceTypeID]	INT,
			[CashAmount]	BIGINT,
			[CurrentAmount] BIGINT,
			[UpdateAmount]	BIGINT,
			[IPAddress]		VARCHAR(15),
			[Memo]			VARCHAR(128),
			[CreateTime]	DATETIME,
			[AreaID]		INT,
			[PlatformID]	INT
		)



	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From CenterDB.Area.AreaAdminUser where AdminID=@intAdminID


	/*  取資料暫存  */
	--------------------------------------------------------------------------------
	INSERT INTO @Temp_GetBankLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetBankLog]
		@intUserID			= @intUserID,
		@intSourceType		= @intSourceType,
		@chvIPAddress		= @chvIPAddress,
		@beginTime			= @beginTime,
		@endTime			= @endTime,
		@intAreaID			= @intAreaID,
		@intPlatformID		= @intPlatformID,
		@intPageIndex		= @intPageIndex,
		@intPageSize		= @intPageSize,
		@intPageCount		= @intPageCount		OUTPUT	--记录总数		
	--------------------------------------------------------------------------------


	/*  回傳資料  */
	--------------------------------------------------------------------------------
	SELECT	A.*,
			[User.ID] = UI.[UserID],
			[User.LoginName] = UI.[LoginName],
			[User.Grade] = UI.[GradeID],
			[SourceType.ChangeType] = BST.ChangeType,
			[SourceType.SourceID] = BST.SourceID,
			[SourceType.SourceName] = ISNULL(BST.SourceName,'異常動作'),
			[User.Area.Platform.Name] = ISNULL(PM.Name,'')
	FROM @Temp_GetBankLog AS A
		LEFT JOIN [Users].[UserInfo] AS UI WITH (NOLOCK) ON A.UserID = UI.UserID
		LEFT JOIN [Stat].[BankSourceType] AS BST WITH (NOLOCK) ON A.SourceTypeID = BST.SourceID
		LEFT JOIN [Area].[Area] AS AA WITH (NOLOCK) ON A.AreaID = AA.AreaID
		LEFT JOIN [Area].[Platform] AS PM WITH (NOLOCK) ON A.PlatformID = PM.PlatformID
	where A.AreaID in (select areaid from @Temp_AreaID)
	--------------------------------------------------------------------------------
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBankLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBankLog] TO [xbuser]
    AS [dbo];

