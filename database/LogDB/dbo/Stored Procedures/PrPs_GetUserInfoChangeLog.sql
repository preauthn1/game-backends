-- =============================================
-- Author:		Jason
-- Create date: 2018/04/11
-- Description:	回傳使用者修改紀錄給Center
--	2018/04/17	Jason		新增回傳分頁
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetUserInfoChangeLog]
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

	
	/*  回傳紀錄  */
	------------------------------------------------------
	SELECT	@intPageCount = COUNT(1)
	FROM [Users].[UserInfoChangeLog] AS A WITH (NOLOCK)
	WHERE
		(@UserID IS NULL OR [UserID] = @UserID)
		AND (@ChangeType IS NULL OR [ChangeType] = @ChangeType)
		AND (@chvIPAddress IS NULL OR [IPAddress] = @chvIPAddress)
		AND (@StatTime IS NULL OR [WriteTime] >= @StatTime)
		AND (@EndTime IS NULL OR [WriteTime] <= @EndTime)

	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY A.[WriteTime] DESC) AS rowNum,
				[LogID],
				[UserID],
				[ChangeType],
				[OldData],
				[NewData],
				[IPAddress],
				[Memo],
				[WriteTime]
		FROM [Users].[UserInfoChangeLog] AS A WITH (NOLOCK)
		WHERE
			(@UserID IS NULL OR [UserID] = @UserID)
			AND (@ChangeType IS NULL OR [ChangeType] = @ChangeType)
			AND (@chvIPAddress IS NULL OR [IPAddress] = @chvIPAddress)
			AND (@StatTime IS NULL OR [WriteTime] >= @StatTime)
			AND (@EndTime IS NULL OR [WriteTime] <= @EndTime)
	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetUserInfoChangeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetUserInfoChangeLog] TO [xbuser]
    AS [dbo];

