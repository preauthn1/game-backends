-- =============================================
-- Author:		Jason
-- Create date: 2018/06/04
-- Description:	回傳MoneysourceLog #406 金幣統計改為LogDB計算及同步更新CenterDB
--	2019/03/27	Jason		新增PlatformID AreaID 對應 寫到新的[Stat].[StatisticsMoneySourceLog_New]
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetMoneySourceLog]
(
	@Type			TINYINT = 1,		-- 1:原作法 2:新作法(會有PlatformID AreaID對應關係)
	@dtStatTime		DATETIME = NULL,		
	@dtEndTime		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @StatTime		DATETIME = @dtStatTime		
	DECLARE @EndTime		DATETIME = @dtEndTime


	/*  回傳統計紀錄  */
	------------------------------------------------------
	IF(@Type = 1)
	BEGIN
		SELECT	[SourceTypeID],		[StatTime],		[Amount],	[RobotAmount],		[RobotTaxAmount],	[TaxAmount]
		FROM [Stat].[StatisticsMoneySourceLog] WITH (NOLOCK)
		WHERE
			StatTime >= @StatTime
			AND StatTime  < @dtEndTime
	END
	ELSE
	BEGIN
		SELECT	[PlatformID],	[AreaID],	[SourceTypeID],		[StatTime],		[Amount],	[RobotAmount],		[RobotTaxAmount],	[TaxAmount],	[FlowAmount]
		FROM [Stat].[StatisticsMoneySourceLog_New] WITH (NOLOCK)
		WHERE
			StatTime >= @StatTime
			AND StatTime  < @dtEndTime
	END
	------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetMoneySourceLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetMoneySourceLog] TO [xbuser]
    AS [dbo];

