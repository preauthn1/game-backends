-- =============================================
-- Author:  Jason
-- Create date: 2018/05/29
-- Description:	#351 日報表查詢 充值用戶明細/登入遊戲用戶明細
--	2018/05/31	Jason		#397 統計報表顯示調整 調整SumCharge to SumChargeByQuery 新增SumCharge from B.SumCharge
--	2018/07/04	alan		#548 SumCharge加上代理充值, 並改由moneylog撈取
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetStatReportDetail]
(
	@intPlatformID		INT,
	@intAreaID			INT,
	@dtStatTime			DATETIME,
	@StatChargeAmount	BIGINT = NULL,
	@StatLogonCount		INT = NULL,
	@intPageIndex		INT = 1,
	@intPageSize		INT = 20,
	@intPageCount		INT = 0		OUTPUT		--记录总数	
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @PlatformID	INT = @intPlatformID
	DECLARE @AreaID		INT = @intAreaID
	DECLARE @StatTime	DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	DECLARE @EndTime	DATETIME = DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))

	DECLARE @Temp_StatUserID	TABLE (UserID INT)


	/*  依功能暫存用戶ID  */
	--------------------------------------------------------------------------------
	IF (@StatChargeAmount IS NULL) AND (@StatLogonCount IS NULL)
	BEGIN
		SELECT	rowNum = 0,
				--PlatformName = E.Name,
				AreaName = 0,
				UserID = 0,
				LoginName = '',
				GradeID = 0,
				SumCharge = 0,
				Amount = 0,
				Sex = 0,
				HDType = 0,
				HDCode = '',
				UserType = 0,
				CreateTime = 0,
				CreateIPAddress = 0,
				LastLogonTime = 0,
				LastLogonIPAddress = 0

		RETURN
	END

	IF (@StatChargeAmount IS NOT NULL)
	BEGIN
		INSERT INTO @Temp_StatUserID
		SELECT DISTINCT A.UserID
		FROM [LogDB].[Money].[MoneyLog] AS A WITH (NOLOCK)
			INNER JOIN [Users].[UserInfo] AS B ON A.UserID = B.UserID AND B.UserType <> 2
			INNER JOIN [Area].[Area] AS D WITH (NOLOCK) ON B.AreaID = D.AreaID
			INNER JOIN [Area].[Platform] AS E WITH (NOLOCK) ON D.PlatformID = E.PlatformID
		WHERE
			E.PlatformID = @PlatformID
			AND B.AreaID = @AreaID
			AND A.[CreateTime] >= @StatTime
			AND A.[CreateTime] < @EndTime
			AND A.SourceTypeID >= 11001 and A.SourceTypeID <= 11020 --此段範圍都屬充值類型 
	END

	IF (@StatLogonCount IS NOT NULL)
	BEGIN
		INSERT INTO @Temp_StatUserID
		EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Stat_GetGameLogUser]
			@intPlatformID	= @PlatformID,
			@intAreaID		= @AreaID,
			@dtStatTime		= @dtStatTime
	END


	SELECT	@intPageCount = COUNT(1)
	FROM @Temp_StatUserID AS A
		INNER JOIN [Users].[UserInfo] AS B ON A.UserID = B.UserID AND B.UserType <> 2
	--------------------------------------------------------------------------------


	/*  依暫存用戶ID回傳用戶資料  */
	--------------------------------------------------------------------------------
	;WITH CTE AS
	(
	
		SELECT	ROW_NUMBER() OVER (ORDER BY B.CreateTime DESC) AS rowNum,
		UserID = C.UserID,
		AreaName = D.AreaName,
		LoginName = B.LoginName,
		GradeID = B.GradeID,
		SumCharge = B.SumCharge + B.SumAgentCharge,
		SumConvert = B.SumConvert,
		SumChargeByQuery = C.SumChargeByQuery, 
		Amount = F.Amount,
		Sex = B.Sex,
		HDType = B.HDType,
		HDCode = B.HDCode,
		UserType = B.UserType,
		CreateTime = B.CreateTime,
		CreateIPAddress = B.CreateIPAddress,
		LastLogonTime = B.LastLogonTime,
		LastLogonIPAddress = B.LastLogonIPAddress
		FROM (
			SELECT UserID, AreaID, SUM(UpdateAmount/100) as SumChargeByQuery
			FROM [LogDB].[Money].[MoneyLog] WITH (NOLOCK)
			WHERE
			PlatformID = @PlatformID
			AND AreaID = @AreaID
			AND SourceTypeID >= 11001 and SourceTypeID <= 11020 --此段範圍都屬充值類型 
			AND [CreateTime] >= @StatTime AND CreateTime < @EndTime 
			AND UserID in (SELECT UserID from @Temp_StatUserID)
			Group by UserID,AreaID
		) as C

		LEFT JOIN [Users].[UserInfo] AS B ON C.UserID = B.UserID AND B.UserType <> 2
		LEFT JOIN [Area].[Area] AS D WITH (NOLOCK) ON C.AreaID = D.AreaID
		LEFT JOIN [Money].[Cash] AS F WITH (NOLOCK) ON C.UserID = F.UserID

	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	--------------------------------------------------------------------------------


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetStatReportDetail] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetStatReportDetail] TO [xbuser]
    AS [dbo];

