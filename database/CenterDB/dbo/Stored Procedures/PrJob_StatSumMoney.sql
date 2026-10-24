
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<总金币统计>
--	2018/03/19	Jason		調整寫法,降低邏輯讀取數及增加統計BankAmount
--	2018/06/05	Jason		#406 金幣統計改為LogDB計算及同步更新CenterDB 調整為可帶日期參數及調整紀錄時間為當日整點
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatSumMoney]
(
	@dtStatTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @CashAmount		BIGINT
	DECLARE @BankAmount		BIGINT
	DECLARE @SumIn			BIGINT
	DECLARE @SumOut			BIGINT
	DECLARE @LogStartTime	DATETIME
	DECLARE @LogEndTime		DATETIME

	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END

	SELECT @CashAmount = SUM(amount) FROM [money].cash
	
	SELECT @BankAmount = SUM(amount) FROM [Money].[Bank]

	
	/*  20190327 因應Stat.MoneySourceLog 需要新增PlatformID AreaID 故讀取新表 */
	-------------------------------------------------------------------------------------------------------------
	--SELECT @SumIn = SUM(CASE WHEN t.ChangeType = 0 THEN l.Amount ELSE 0 END),
	--		@SumOut = SUM(CASE WHEN t.ChangeType = 1 THEN l.Amount ELSE 0 END)
	--FROM Stat.MoneySourceLog l , Stat.MoneySourceType t
	--WHERE
	--	l.SourceID = t.SourceID
	--	AND t.SourceID NOT IN(15001,15002)
	--	AND StatTime >= @LogStartTime AND StatTime < @LogEndTime

	SELECT @SumIn = SUM(CASE WHEN t.ChangeType = 0 THEN l.Amount ELSE 0 END),
			@SumOut = SUM(CASE WHEN t.ChangeType = 1 THEN l.Amount ELSE 0 END)
	FROM Stat.MoneySourceLog_New l , Stat.MoneySourceType t
	WHERE
		l.SourceID = t.SourceID
		AND t.SourceID NOT IN(15001,15002)
		AND StatTime >= @LogStartTime AND StatTime < @LogEndTime
	-------------------------------------------------------------------------------------------------------------

	if exists (select top 1 1 from Stat.SumMoneyLog with (nolock) where stattime = @LogStartTime)
	begin
		update Stat.SumMoneyLog
		set [CashAmount] = ISNULL(@CashAmount,0),
			[BankAmount] = ISNULL(@BankAmount,0),
			[SumIn] = ISNULL(@SumIn,0),
			[SumOut] = ISNULL(@SumOut,0)
		where
			stattime = @LogStartTime
	end
	else
	begin
		INSERT INTO Stat.SumMoneyLog
	        (	CashAmount,
				BankAmount,
				SumIn,
				SumOut,
				StatTime
			)
		SELECT	ISNULL(@CashAmount,0),
				ISNULL(@BankAmount,0),
				ISNULL(@SumIn,0),
				ISNULL(@SumOut,0),
				@LogStartTime
	end

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatSumMoney] TO [xbuser]
    AS [dbo];

