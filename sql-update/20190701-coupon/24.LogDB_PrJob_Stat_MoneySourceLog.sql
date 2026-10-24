USE [LogDB]
GO
/****** Object:  StoredProcedure [dbo].[PrJob_Stat_MoneySourceLog]    Script Date: 2019/6/19 下午 06:55:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jason
-- Create date: 2018/06/04
-- Description:	統計MoneysourceLog #406 金幣統計改為LogDB計算及同步更新CenterDB
--	2018/06/29	jason		調整convert條件 取消createtime並改用lastupdatetime判斷
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
--	2019/06/19	Jason		新增寫入禮碼金額欄位/參數
-- =============================================
ALTER PROCEDURE [dbo].[PrJob_Stat_MoneySourceLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON
	
	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME = DATEADD(DAY,-1,DATEADD(DAY,0,@dtStatTime))
	DECLARE @LogEndTime				DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-30,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END


	/*  取登入有金幣異動紀錄  */
	------------------------------------------------------
	--原作法
	DECLARE @Temp_MoneySourceLog	TABLE ([SourceTypeID] INT,[Amount] BIGINT,[RobotAmount] BIGINT,[RobotTaxAmount] BIGINT,[TaxAmount] BIGINT,[StatTime] DATETIME)

	INSERT INTO @Temp_MoneySourceLog
	SELECT	[SourceTypeID],
			[Amount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN [UpdateAmount] ELSE 0 END),
			[RobotAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [UpdateAmount] ELSE 0 END),
			[RobotTaxAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [TaxAmount] ELSE 0 END),
			[TaxAmount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN [TaxAmount] ELSE 0 END),
			[StatTime] = DATEADD(DAY,0,DATEDIFF(DAY,0,A.[CreateTime]))
	FROM [Money].[MoneyLog]	AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
	WHERE
		A.[CreateTime] >= @LogStartTime
		AND A.[CreateTime] < @LogEndTime
		AND [SourceTypeID] <> 14000
	GROUP BY [SourceTypeID],DATEADD(DAY,0,DATEDIFF(DAY,0,A.[CreateTime]))
	UNION ALL
	SELECT	[SourceTypeID] = 14000,
			[Amount] = SUM(CASE WHEN B.UserID IS NOT NULL AND A.Status = 3 THEN [Amount] ELSE 0 END),
						---SUM(CASE WHEN B.UserID IS NOT NULL AND A.Status = 11 THEN [Amount] ELSE 0 END),
			[RobotAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [Amount] ELSE 0 END),
			[RobotTaxAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [TaxAmount] ELSE 0 END),
			[TaxAmount] = SUM(CASE WHEN B.UserID IS NOT NULL AND A.Status = 3 THEN [TaxAmount] ELSE 0 END),
							--- SUM(CASE WHEN B.UserID IS NOT NULL AND A.Status = 11 THEN [TaxAmount] ELSE 0 END
			[StatTime] = DATEADD(DAY,0,DATEDIFF(DAY,0,A.[LastUpdateTime]))
	FROM [Money].[ConvertLog] AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
	WHERE
		A.[LastUpdateTime] >= @LogStartTime
		AND A.[LastUpdateTime] < @LogEndTime
	GROUP BY DATEADD(DAY,0,DATEDIFF(DAY,0,A.[LastUpdateTime]))

	
	--新對應 20190327 增加PlatformID , AreaID對應
	DECLARE @Temp_MoneySourceLog_New	TABLE ([PlatformID] INT,[AreaID] INT,[SourceTypeID] INT,[Amount] BIGINT,[RobotAmount] BIGINT,[RobotTaxAmount] BIGINT,[TaxAmount] BIGINT,[StatTime] DATETIME,[FlowAmount] BIGINT,[CouponAmount] BIGINT)

	INSERT INTO @Temp_MoneySourceLog_New
	SELECT	A.[PlatformID],
			A.[AreaID],
			[SourceTypeID],
			[Amount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN [UpdateAmount] ELSE 0 END),
			[RobotAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [UpdateAmount] ELSE 0 END),
			[RobotTaxAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [TaxAmount] ELSE 0 END),
			[TaxAmount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN [TaxAmount] ELSE 0 END),
			[StatTime] = DATEADD(DAY,0,DATEDIFF(DAY,0,A.[CreateTime])),
			[FlowAmount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN ISNULL([FlowAmount],0) ELSE 0 END),
			[CouponAmount] = SUM(CASE WHEN B.UserID IS NOT NULL THEN ISNULL([CouponAmount],0) ELSE 0 END)
	FROM [Money].[MoneyLog]	AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
	WHERE
		A.[CreateTime] >= @LogStartTime
		AND A.[CreateTime] < @LogEndTime
		AND [SourceTypeID] <> 14000
	GROUP BY A.[PlatformID],A.[AreaID],[SourceTypeID],DATEADD(DAY,0,DATEDIFF(DAY,0,A.[CreateTime]))
	UNION ALL
	SELECT	A.[PlatformID],
			A.[AreaID],
			[SourceTypeID] = 14000,
			[Amount] = SUM(CASE WHEN B.UserID IS NOT NULL
						AND A.Status = 3
						THEN [Amount] ELSE 0 END),
						---SUM(CASE WHEN B.UserID IS NOT NULL AND A.Status = 11 THEN [Amount] ELSE 0 END),
			[RobotAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [Amount] ELSE 0 END),
			[RobotTaxAmount] = SUM(CASE WHEN B.UserID IS NULL THEN [TaxAmount] ELSE 0 END),
			[TaxAmount] = SUM(CASE WHEN B.UserID IS NOT NULL
							AND A.Status = 3
							THEN [TaxAmount] ELSE 0 END),
							--- SUM(CASE WHEN B.UserID IS NOT NULL AND A.Status = 11 THEN [TaxAmount] ELSE 0 
			[StatTime] = DATEADD(DAY,0,DATEDIFF(DAY,0,A.[LastUpdateTime])),
			[FlowAmount] = 0,
			[CouponAmount] = 0
	FROM [Money].[ConvertLog] AS A WITH (NOLOCK)
		LEFT JOIN [Users].[UserRegLog] AS B WITH (NOLOCK) ON A.UserID = B.UserID
	WHERE
		A.[LastUpdateTime] >= @LogStartTime
		AND A.[LastUpdateTime] < @LogEndTime
	GROUP BY A.[PlatformID],A.[AreaID],DATEADD(DAY,0,DATEDIFF(DAY,0,A.[LastUpdateTime]))
	------------------------------------------------------
	


	/*  若已有統計數據則刪除後再重新寫入  */
	------------------------------------------------------
	--原作法
	MERGE [Stat].[StatisticsMoneySourceLog] AS A
	USING (
			SELECT	[SourceTypeID],	[Amount],	[RobotAmount],	[RobotTaxAmount],	[TaxAmount],	[StatTime]
			FROM @Temp_MoneySourceLog
		) AS B ON A.[SourceTypeID] = B.[SourceTypeID] AND A.[StatTime] = B.[StatTime]
	WHEN MATCHED THEN
		UPDATE SET	A.[Amount] = B.[Amount],
					A.[RobotAmount] = B.[RobotAmount],
					A.[RobotTaxAmount] = B.[RobotTaxAmount],
					A.[TaxAmount] = B.[TaxAmount]
	WHEN NOT MATCHED THEN
		INSERT
			(	[SourceTypeID],		[StatTime],		[Amount],	[RobotAmount],		[RobotTaxAmount],	[TaxAmount]	)
		VALUES (B.[SourceTypeID],	B.[StatTime],	B.[Amount],	B.[RobotAmount],	B.[RobotTaxAmount],	B.[TaxAmount]	);


	--新對應 20190327 增加PlatformID , AreaID對應
	MERGE [Stat].[StatisticsMoneySourceLog_New] AS A
	USING (
			SELECT	[PlatformID],	[AreaID],	[SourceTypeID],	[Amount],	[RobotAmount],	[RobotTaxAmount],	[TaxAmount],	[StatTime],[FlowAmount],[CouponAmount]
			FROM @Temp_MoneySourceLog_New
		) AS B ON A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID] AND A.[SourceTypeID] = B.[SourceTypeID] AND A.[StatTime] = B.[StatTime]
	WHEN MATCHED THEN
		UPDATE SET	A.[Amount] = B.[Amount],
					A.[RobotAmount] = B.[RobotAmount],
					A.[RobotTaxAmount] = B.[RobotTaxAmount],
					A.[TaxAmount] = B.[TaxAmount],
					A.[FlowAmount] = B.[FlowAmount],
					A.[CouponAmount] = B.[CouponAmount]
	WHEN NOT MATCHED THEN
		INSERT
			(	[PlatformID],	[AreaID],	[SourceTypeID],		[StatTime],		[Amount],	[RobotAmount],		[RobotTaxAmount],	[TaxAmount],[FlowAmount],[CouponAmount]	)
		VALUES (B.[PlatformID],	B.[AreaID],	B.[SourceTypeID],	B.[StatTime],	B.[Amount],	B.[RobotAmount],	B.[RobotTaxAmount],	B.[TaxAmount],B.[FlowAmount],B.[CouponAmount]);
	------------------------------------------------------

	RETURN
END