-- =============================================
-- Author:		Jason
-- Create date: 2018/04/11
-- Description:	回傳使用者修改紀錄給Center
--	2018/04/17	Jason		新增回傳分頁
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetUserInfoChangeLog]
(
	@intUserID		INT = NULL,
	@tnyChangeType	TINYINT = NULL,
	@chvIPAddress	VARCHAR(40) = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@intPageIndex	INT = 1,
	@intPageSize	INT = 20,
	@intPageCount	INT			OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID		INT = @intUserID
	DECLARE	@ChangeType	TINYINT = @tnyChangeType
	DECLARE @IPAddress	VARCHAR(40) = @chvIPAddress
	DECLARE @StatTime	DATETIME = @dtStatTime
	DECLARE @EndTime	DATETIME = @dtEndTime


	DECLARE @Temp_UserChangeLog	TABLE
		(
			[rowNum]		INT,
			[LogID]			INT,
			[UserID]		INT,
			[ChangeType]	TINYINT,
			[OldData]		VARCHAR(1024),
			[NewData]		VARCHAR(1024),
			[IPAddress]		VARCHAR(40),
			[Memo]			VARCHAR(128),
			[WriteTime]		DATETIME
		)


	/*  取資料暫存  */
	------------------------------------------------------
	INSERT INTO @Temp_UserChangeLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetUserInfoChangeLog]
		@intUserID		= @UserID,
		@tnyChangeType	= @ChangeType,
		@chvIPAddress	= @IPAddress,
		@dtStatTime		= @StatTime,
		@dtEndTime		= @EndTime,
		@intPageIndex	= @intPageIndex,
		@intPageSize	= @intPageSize,
		@intPageCount	= @intPageCount		OUTPUT
	------------------------------------------------------

	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	A.[LogID] AS [LogID],
			A.[UserID] AS [UserID],
			A.[ChangeType] AS [ChangeType],
			A.[OldData] AS [OldData],
			A.[NewData] AS [NewData],
			A.[IPAddress] AS [IPAddress],
			A.[Memo] AS [Memo],
			A.[WriteTime] AS [WriteTime],
			--U.[UserID] AS [UserID1],
			U.[LoginName] AS [LoginName],
			U.[Password] AS [Password],
			U.[FaceID] AS [FaceID],
			U.[NickName] AS [NickName],
			U.[Sex] AS [Sex],
			U.[HDType] AS [HDType],
			U.[HDCode] AS [HDCode],
			U.[UserType] AS [UserType],
			U.[CreateTime] AS [CreateTime],
			U.[CreateIPAddress] AS [CreateIPAddress],
			U.[LastLogonTime] AS [LastLogonTime],
			U.[LastLogonIPAddress] AS [LastLogonIPAddress],
			U.[RealName] AS [RealName],
			U.[AlipayAccount] AS [AlipayAccount],
			U.[UnionpayAccount] AS [UnionpayAccount],
			U.[UnionpayRealName] AS [UnionpayRealName],
			U.[SumCharge] AS [SumCharge],
			U.[SumTax] AS [SumTax],
			U.[SumLoginTimes] AS [SumLoginTimes],
			U.[SumConvert] AS [SumConvert],
			U.[SumConvertTimes] AS [SumConvertTimes],
			U.[QuestionCount] AS [QuestionCount],
			U.[GradeID] AS [GradeID],
			U.[AreaID] AS [AreaID],
			--AA.[AreaID] AS [AreaID1],
			AA.[AreaName] AS [AreaName],
			--AA.[ClientPlatform] AS [ClientPlatform],
			--AA.[Memo] AS [Memo1],
			--AA.[AdminID] AS [AdminID],
			--AA.[Sort] AS [Sort],
			AA.[PlatformID] AS [PlatformID],
			--PM.[PlatformID] AS [PlatformID1],
			PM.[Name] AS [Name]
			--PM.[PaySort] AS [PaySort],
			--PM.[UIFlag] AS [UIFlag],
			--PM.[UpGradeMsg] AS [UpGradeMsg],
			--PM.[AnnMsg] AS [AnnMsg],
			--PM.[PayTip] AS [PayTip],
			--PM.[FAQContent] AS [FAQContent],
			--PM.[PayAmountConfig] AS [PayAmountConfig],
			--PM.[PayStatus] AS [PayStatus],
			--PM.[PayCloseTip] AS [PayCloseTip],
			--PM.[GiftMoney] AS [GiftMoney]
	FROM @Temp_UserChangeLog AS A
		INNER JOIN [Users].[UserInfo] AS U ON A.[UserID] = U.[UserID]
		INNER JOIN [Area].[Area] AS AA ON U.[AreaID] = AA.[AreaID]
		LEFT OUTER JOIN [Area].[Platform] AS PM ON AA.[PlatformID] = PM.[PlatformID]
	ORDER BY [rowNum] ASC
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetUserInfoChangeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetUserInfoChangeLog] TO [xbuser]
    AS [dbo];

