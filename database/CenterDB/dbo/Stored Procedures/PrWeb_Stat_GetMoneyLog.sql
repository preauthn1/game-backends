-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	用戶管理-用戶金幣-現金紀錄查詢<查询用户现金记录>
--	2018/04/10	Jason		調整取資料方式
--  2019/07/31	CCC			增加用戶等級 User.GradeID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetMoneyLog]
(
	@intUserID			INT = NULL,
	@intSourceTypeGroup	INT = NULL,
	@intSourceType		INT = NULL,
	@chvIPAddress		VARCHAR(40) = NULL,
	@chvMemo			VARCHAR(128) = NULL,
	@beginTime			DATETIME= NULL,
	@endTime			DATETIME= NULL,
	@intAreaID			INT = NULL,
	@intPlatformID		INT = NULL,
	@intPageIndex		INT = 1,
	@intPageSize		INT = 20,
	@intPageCount		INT = 0		OUTPUT		--记录总数	
)	
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @Temp_GetMoneyLog	TABLE
		(
			[rowNum]		INT,
			[LogID]			INT,
			[UserID]		INT,
			[SourceTypeID]	INT,
			[CurrentAmount] BIGINT,
			[UpdateAmount]	BIGINT,
			[TaxAmount]		BIGINT,
			[IPAddress]		VARCHAR(40),
			[Memo]			VARCHAR(128),
			[CreateTime]	DATETIME,
			[AreaID]		INT,
			[PlatformID]	INT,
			[TradeID]		BIGINT,
			[FlowAmount] 	BIGINT
		)

	/*  取資料暫存  */
	--------------------------------------------------------------------------------------------
	INSERT INTO @Temp_GetMoneyLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetMoneyLog]
		@intUserID			= @intUserID,
		@intSourceTypeGroup	= @intSourceTypeGroup,
		@intSourceType		= @intSourceType,
		@chvIPAddress		= @chvIPAddress,
		@chvMemo			= @chvMemo,
		@beginTime			= @beginTime,
		@endTime			= @endTime,
		@intAreaID			= @intAreaID,
		@intPlatformID		= @intPlatformID,
		@intPageIndex		= @intPageIndex,
		@intPageSize		= @intPageSize,
		@intPageCount		= @intPageCount		OUTPUT		
	--------------------------------------------------------------------------------------------


	/*  回傳資料  */
	--------------------------------------------------------------------------------------------		
	SELECT	A.*,
			[User.ID] = UI.[UserID],
			[User.LoginName] = UI.[LoginName],
			[User.GradeID] = UI.[GradeID],
			[User.Password] = UI.[Password],
			[User.Sex] = UI.[Sex],
			[User.HDType] = UI.[HDType],
			[User.HDCode] = UI.[HDCode],
			[User.UserType] = UI.[UserType],
			[User.CreateTime] = UI.[CreateTime],
			[User.CreateIPAddress] = UI.[CreateIPAddress],
			[User.LastLogonTime] = UI.[LastLogonTime],
			[User.LastLogonIPAddress] = UI.[LastLogonIPAddress],
			[SourceType.ChangeType] = MST.ChangeType,
			[SourceType.GroupID] = MST.GroupID,
			[SourceType.SourceID] = MST.SourceID,
			[SourceType.SourceName] = ISNULL(MST.SourceName,'異常動作'),
			[User.Area.Platform.Name] = ISNULL(PM.Name,'')
	FROM @Temp_GetMoneyLog AS A
		LEFT JOIN [Users].[UserInfo] AS UI WITH (NOLOCK) ON A.UserID = UI.UserID
		LEFT JOIN [Stat].[MoneySourceType] AS MST WITH (NOLOCK) ON A.SourceTypeID = MST.SourceID
		LEFT JOIN [Stat].[MoneySourceGroup] AS MSG WITH (NOLOCK) ON MST.GroupID = MSG.Id
		LEFT JOIN [Area].[Area] AS AA WITH (NOLOCK) ON A.AreaID = AA.AreaID
		LEFT JOIN [Area].[Platform] AS PM WITH (NOLOCK) ON A.PlatformID = PM.PlatformID
	--------------------------------------------------------------------------------------------
	
END





GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetMoneyLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetMoneyLog] TO [xbuser]
    AS [dbo];

