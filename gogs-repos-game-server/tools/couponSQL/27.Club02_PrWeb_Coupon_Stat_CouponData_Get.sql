USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Coupon_Stat_CouponData_Get]    Script Date: 2019/6/21 下午 06:08:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	日报查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Coupon_Stat_CouponData_Get]
(
	@intAdminID	INT,
	@dtStartTime	DATETIME = NULL,
	@dtEndTime	DATETIME = NULL,
	@chvMsg	VARCHAR(100)	OUTPUT,
	@intTotalCouponCount INT OUTPUT,
	@lngTotalCouponAmount	BIGINT OUTPUT,
	@lngTotalUsedCouponAmount	BIGINT OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStartTime = ISNULL(@dtStartTime,DATEADD(DAY,-30,DATEDIFF(DAY,0,GETDATE())))
	SET @dtEndTime = ISNULL(@dtEndTime,DATEADD(DAY,1,DATEDIFF(DAY,0,GETDATE())))


	DECLARE @Temp_PlatformID TABLE (PlatfomrID INT)

	INSERT INTO @Temp_PlatformID
	SELECT PlatformID
	FROM [dbo].[AgentPlatformRelations] WITH (NOLOCK)
	WHERE
		AdminID = @intAdminID

	/*  總輸入組數/總發送金額/總消耗金額  */
	SELECT	@intTotalCouponCount = COUNT(DISTINCT [CouponCode]),
			@lngTotalCouponAmount = SUM([Amount])
	FROM (
			SELECT	[CouponCode],
					[Amount] = [Amount]
			FROM [dbo].[CouponUserData] WITH (NOLOCK)
			WHERE
				[MultiUser] = 0
				AND Status = 1
				AND PlatformID IN (SELECT PlatformID FROM @Temp_PlatformID)
			UNION
			SELECT	A.[CouponCode],
					[Amount] = SUM(B.[Amount])
			FROM [dbo].[CouponMultiUserLog] AS A WITH (NOLOCK)
				INNER JOIN [dbo].[CouponUserData] AS B WITH (NOLOCK) ON A.CouponCode = B.CouponCode AND B.MultiUser = 1
			WHERE
				B.PlatformID IN (SELECT PlatformID FROM @Temp_PlatformID)
			GROUP BY A.CouponCode
		) AS A


	SELECT	@lngTotalUsedCouponAmount = SUM(ISNULL(CouponAmount,0))
	FROM LogDB.[Stat].[StatisticsUserAmount_Day] WITH (NOLOCK)

	SET @intTotalCouponCount = ISNULL(@intTotalCouponCount,0)
	SET @lngTotalCouponAmount = ISNULL(@lngTotalCouponAmount,0)
	SET @lngTotalUsedCouponAmount = ISNULL(@lngTotalUsedCouponAmount,0)


	SELECT	A.LogDate,
			[CouponCount] = ISNULL(B.CouponCount,0),
			[Amount] = ISNULL(B.Amount,0),
			[DailyUsedCouponAmount] = ISNULL( C.CouponAmount, 0)
	FROM (SELECT DISTINCT LogDate FROM LogDB.[Stat].[StatisticsUserAmount_Day] WITH (NOLOCK) WHERE LogDate >= @dtStartTime AND LogDate <= @dtEndTime) AS A
		LEFT JOIN (
					SELECT	[UseDate],
							[CouponCount] = COUNT(DISTINCT [CouponCode]),
							[Amount] = SUM([Amount])
					FROM [dbo].[CouponUserData] AS A WITH (NOLOCK)
					WHERE
						[UseDate] >= @dtStartTime AND [UseDate] <= @dtEndTime
						AND PlatformID IN (SELECT PlatformID FROM @Temp_PlatformID)
					GROUP BY [UseDate]
				) AS B ON A.LogDate = B.[UseDate]
		LEFT JOIN (
					SELECT	LogDate,
							CouponAmount = SUM(ISNULL(CouponAmount,0))
					FROM LogDB.[Stat].[StatisticsUserAmount_Day] WITH (NOLOCK)
					WHERE
						LogDate >= @dtStartTime AND LogDate <= @dtEndTime
						AND PlatformID IN (SELECT PlatformID FROM @Temp_PlatformID)
					GROUP BY LogDate
				) AS C ON A.LogDate = C.LogDate
	RETURN
END