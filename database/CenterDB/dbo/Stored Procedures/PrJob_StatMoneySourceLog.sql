-- =============================================
-- Author:		Jason
-- Create date: 2018/06/04
-- Description:	回傳MoneysourceLog #406 金幣統計改為LogDB計算及同步更新CenterDB
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatMoneySourceLog]
(
	@dtStatTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Type	TINYINT

	/*  原作法不更動 僅新增呼叫LogDB SP時增加@type參數 */
	----------------------------------------------------------------------------------------------------------------------	
	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())
	
	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME
	DECLARE @TempDates		DATETIME

	DECLARE @Temp_MoneySourceType	TABLE ([SourceTypeID] INT)
	DECLARE @Temp_StatDate	TABLE ([StatTime] DATETIME)
	DECLARE @Temp_MoneySourceLog	TABLE ([SourceTypeID] INT,[StatTime] DATETIME,[Amount] BIGINT,[RobotAmount] BIGINT,[RobotTaxAmount] BIGINT,[TaxAmount] BIGINT)

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

	SET @TempDates	= @LogStartTime

	/*  暫存紀錄  */
	------------------------------------------------------
	INSERT INTO @Temp_MoneySourceType
	SELECT DISTINCT [SourceID]
	FROM [Stat].[MoneySourceType] WITH (NOLOCK)
	WHERE
		[SourceID] > 10000	--小於10000為Game ServerID


	WHILE (@TempDates <= @LogEndTime)
	BEGIN
		INSERT INTO @Temp_StatDate
		VALUES (@TempDates)

		SET @TempDates	=	DATEADD(DAY,1,@TempDates)
	END


	INSERT INTO @Temp_MoneySourceLog
	EXEC [LogDB].[dbo].[PrWeb_Stat_GetMoneySourceLog]
		@dtStatTime		= @LogStartTime,		
		@dtEndTime		= @LogEndTime
	------------------------------------------------------


	/*  更新紀錄  */
	------------------------------------------------------
	MERGE [Stat].[MoneySourceLog] AS A
	USING (
				SELECT	DISTINCT	SourceID = B.SourceTypeID,
									Amount = ISNULL(C.Amount,0),
									RobotAmount = ISNULL(C.RobotAmount,0),
									RobotTaxAmount = ISNULL(C.RobotTaxAmount,0),
									TaxAmount = ISNULL(C.TaxAmount,0),
									StatTime = A.StatTime
				FROM @Temp_StatDate AS A
					INNER JOIN @Temp_MoneySourceType AS B ON 1=1
					LEFT JOIN @Temp_MoneySourceLog AS C ON A.StatTime = C.StatTime AND B.SourceTypeID = C.SourceTypeID
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.SourceID = B.SourceID)
	WHEN MATCHED THEN
		UPDATE SET	A.Amount = B.Amount,
					A.RobotAmount = B.RobotAmount,
					A.RobotTaxAmount = B.RobotTaxAmount,
					A.TaxAmount = B.TaxAmount
	WHEN NOT MATCHED THEN
		INSERT (SourceID,	Amount,				RobotAmount,			RobotTaxAmount,			TaxAmount,			StatTime)
		VALUES
			(
				B.SourceID,	ISNULL(Amount,0),	ISNULL(RobotAmount,0),ISNULL(RobotTaxAmount,0),	ISNULL(TaxAmount,0),StatTime
			);
	------------------------------------------------------

	----------------------------------------------------------------------------------------------------------------------


	/* 新作法 */
	----------------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_MoneySourceLog_New	TABLE ([PlatformID] INT,[AreaID] INT,[SourceTypeID] INT,[StatTime] DATETIME,[Amount] BIGINT,[RobotAmount] BIGINT,[RobotTaxAmount] BIGINT,[TaxAmount] BIGINT,[FlowAmount] BIGINT)

	--新做法使用
	SET @Type	=	2

	INSERT INTO @Temp_MoneySourceLog_New
	EXEC [LogDB].[dbo].[PrWeb_Stat_GetMoneySourceLog]
		@Type			= @Type,
		@dtStatTime		= @LogStartTime,		
		@dtEndTime		= @LogEndTime

	MERGE [Stat].[MoneySourceLog_New] AS A
	USING (
				SELECT	DISTINCT	PlatformID = PlatformID,
									AreaID = AreaID,
									SourceID = SourceTypeID,
									Amount = ISNULL(Amount,0),
									RobotAmount = ISNULL(RobotAmount,0),
									RobotTaxAmount = ISNULL(RobotTaxAmount,0),
									TaxAmount = ISNULL(TaxAmount,0),
									StatTime = StatTime,
									[FlowAmount] = [FlowAmount]
				FROM @Temp_MoneySourceLog_New AS A
		) AS B ON (A.[StatTime] = B.[StatTime] AND A.SourceID = B.SourceID AND A.PlatformID = B.PlatformID AND A.AreaID = B.AreaID)
	WHEN MATCHED THEN
		UPDATE SET	A.Amount = B.Amount,
					A.RobotAmount = B.RobotAmount,
					A.RobotTaxAmount = B.RobotTaxAmount,
					A.TaxAmount = B.TaxAmount,
					A.[FlowAmount] = B.[FlowAmount]
	WHEN NOT MATCHED THEN
		INSERT (PlatformID,	AreaID,	SourceID,	Amount,				RobotAmount,			RobotTaxAmount,			TaxAmount,			StatTime,	[FlowAmount])
		VALUES
			(
				PlatformID,	AreaID,	B.SourceID,	ISNULL(Amount,0),	ISNULL(RobotAmount,0),ISNULL(RobotTaxAmount,0),	ISNULL(TaxAmount,0),StatTime,	ISNULL([FlowAmount],0)
			);
	----------------------------------------------------------------------------------------------------------------------


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatMoneySourceLog] TO [xbuser]
    AS [dbo];

