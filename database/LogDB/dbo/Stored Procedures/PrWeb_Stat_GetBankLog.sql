-- =============================================
-- Author:  Jason
-- Create date: 2018/04/10
-- Description:	查询用户銀行记录 (機制改寫)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetBankLog]
(
	@intUserID			INT = NULL,
	@intSourceType		INT = NULL,
	@chvIPAddress		VARCHAR(15) = NULL,
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

	SELECT @intPageCount = COUNT(1)
	FROM [Money].[BankLog] AS A WITH (NOLOCK)
	WHERE
		A.[SourceTypeID] = ISNULL(@SourceType,A.[SourceTypeID])
		AND A.UserID = ISNULL(@UserID,A.UserID)
		AND A.IPAddress = ISNULL(@IPAddress,A.IPAddress)
		AND A.CreateTime >= ISNULL(@LogStatTime,A.CreateTime)
		AND A.CreateTime <= ISNULL(@LogEndTime,A.CreateTime)
		AND A.AreaID = ISNULL(@AreaID,A.AreaID)
		AND A.PlatformID = ISNULL(@intPlatformID,A.PlatformID)

	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY A.CreateTime DESC) AS rowNum,
				a.*
		FROM [Money].[BankLog] AS A WITH (NOLOCK)
		WHERE
			A.[SourceTypeID] = ISNULL(@SourceType,A.[SourceTypeID])
			AND A.UserID = ISNULL(@UserID,A.UserID)
			AND A.IPAddress = ISNULL(@IPAddress,A.IPAddress)
			AND A.CreateTime >= ISNULL(@LogStatTime,A.CreateTime)
			AND A.CreateTime <= ISNULL(@LogEndTime,A.CreateTime)
			AND A.AreaID = ISNULL(@AreaID,A.AreaID)
			AND A.PlatformID = ISNULL(@intPlatformID,A.PlatformID)
	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBankLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetBankLog] TO [xbuser]
    AS [dbo];

