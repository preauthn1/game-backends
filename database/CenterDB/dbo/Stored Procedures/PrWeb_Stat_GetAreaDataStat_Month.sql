-- =============================================
-- Author:  Jason
-- Create date: 2018/03/31
-- Description: 渠道管理-渠道數據統計-月報表查詢
-- 2018/05/23   Emily		#347 日報/月報表增加註冊用戶人數/註冊用戶充值人數/ARPU計算調整
--	2018/05/30	Jason		#378 排序調整--	
-- 2018/06/04 Emily 	    #378 統計日報月報可用時間排序	
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetAreaDataStat_Month]
(
	@intPlatformID	INT = NULL,
	@intAreaID		INT = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@tnyColumns		TINYINT = 0,			--0:充值 1:兌換
	@tnySort		TINYINT = 0,			--0:降序 1:升序
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @PlatformID		INT
	DECLARE @AreaID			INT
	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME

	SET @PlatFormID		= ISNULL(@intPlatformID,0)
	SET @AreaID			= ISNULL(@intAreaID,0)
	SET @LogStartTime	= CASE	WHEN @dtStatTime IS NULL THEN DATEADD(DAY,-7,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime)) END
	SET @LogEndTime		= CASE	WHEN @dtEndTime IS NULL THEN DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE())) ELSE DATEADD(DAY,0,DATEDIFF(DAY,0,@dtEndTime)) END

	
	DECLARE @Temp_RegUser	TABLE (StatTime DATETIME,PlatformID INT,AreaID INT,TotalRegCount INT)

	INSERT INTO @Temp_RegUser
	SELECT	DATEADD(MONTH,DATEDIFF(MONTH,0,StatTime),0),PlatformID,AreaID,SUM(TotalRegCount) AS TotalRegCount
	FROM [Stat].[RegUserLog] AS A WITH (NOLOCK)
	WHERE
		ISNULL(@PlatFormID,A.PlatformID) = A.PlatFormID
		AND ISNULL(@AreaID,A.AreaID) = A.AreaID
		AND ISNULL(@LogStartTime,A.StatTime) <= A.StatTime
		AND ISNULL(@LogEndTime,A.StatTime) >= A.StatTime
	GROUP BY DATEDIFF(MONTH,0,StatTime),PlatformID,AreaID


	/*  --回傳查詢資料  */
	----------------------------------------------------------------------------------------------------------------------
	SELECT	@intPageCount = COUNT(1)
	FROM [Stat].[AreaDataMonth] AS A WITH (NOLOCK)
	WHERE
		ISNULL(@PlatFormID,A.PlatformID) = A.PlatFormID
		AND ISNULL(@AreaID,A.AreaID) = A.AreaID
		AND ISNULL(@LogStartTime,A.StatTime) <= A.StatTime
		AND ISNULL(@LogEndTime,A.StatTime) >= A.StatTime

	;WITH CTE
	AS (
		SELECT	ROW_NUMBER() OVER (ORDER BY CASE	WHEN @tnyColumns = 0 AND @tnySort = 0 THEN (oldcharge + newcharge)
													WHEN @tnyColumns = 0 AND @tnySort = 1 THEN 0 - (oldcharge + newcharge)
													WHEN @tnyColumns = 1 AND @tnySort = 0 THEN (oldConvert + newConvert)
													WHEN @tnyColumns = 1 AND @tnySort = 1 THEN (0 - (oldConvert + newConvert))
													WHEN @tnyColumns = 2 AND @tnySort = 0 THEN DATEDIFF(DAY,0, A.StatTime)
													WHEN @tnyColumns = 2 AND @tnySort = 1 THEN 0 - DATEDIFF(DAY,0, A.StatTime)
													END DESC,(oldConvert + newConvert)) AS rowNum,
				--ROW_NUMBER() OVER (ORDER BY A.StatTime DESC) AS rowNum,
				A.PlatformID,
				A.AreaID,
				StatTime = CONVERT(NVARCHAR(10),A.stattime,23),
				ChargeAmount = (oldcharge + newcharge),
				oldcharge,
				newcharge,
				ConvertAmount = (oldConvert + newConvert),
				oldConvert,
				newConvert,
				LoginCount = (oldLogin + NewLogin),
				OldLogin,
				NewLogin,
				LoginMac,
				[OldLoginMac],
				[NewLoginMac],
				LoginIP,
				[OldLoginIP],
				[NewLoginIP],
				[GameTax] = ABS(ISNULL([GameTax],0)),
				[GameTaxDetail],
				ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
							WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
							ELSE 0 END,
				OldUser_ARPU =CASE	WHEN Total_OrderUser IS NULL THEN 0
							  WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
							  ELSE 0 END,
				NewUser_ARPU = CASE	WHEN Total_OrderUser IS NULL THEN 0
								WHEN (Total_OrderUser) > 0 THEN ABS([GameTax])/Total_OrderUser
							    ELSE 0 END,
				Total_OrderUser = Total_OrderUser,
				OldOrderUser = (Total_OrderUser - NewOrderUser),
				NewOrderUser,
				[RobotTaxAmount],
				TotalRegCount = ISNULL(B.TotalRegCount,0),
				ISNULL(C.AreaName,'全部') as AreaName,
				ISNULL(P.Name,'全部') as PlatformName	
		FROM [Stat].[AreaDataMonth] AS A WITH (NOLOCK)
			LEFT JOIN @Temp_RegUser AS B ON A.AreaID = B.AreaID AND A.PlatformID = B.PlatformID AND A.StatTime = B.StatTime
			LEFT JOIN Area.Area C with (nolock) on B.AreaID=C.AreaID
			LEFT JOIN Area.Platform P with (nolock) on B.PlatformID=P.PlatformID			
		WHERE
			ISNULL(@PlatFormID,A.PlatformID) = A.PlatFormID
			AND ISNULL(@AreaID,A.AreaID) = A.AreaID
			AND ISNULL(@LogStartTime,A.StatTime) <= A.StatTime
			AND ISNULL(@LogEndTime,A.StatTime) >= A.StatTime
		)
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	----------------------------------------------------------------------------------------------------------------------
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetAreaDataStat_Month] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetAreaDataStat_Month] TO [xbuser]
    AS [dbo];

