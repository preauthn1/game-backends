-- =============================================
-- Author:		Jason
-- Create date: 2018/03/27
-- Description:	回傳統計留存率 (機制改寫)
--	2018/05/23	Jason		#349 建立統計數據報表提供API回傳 新增3 4 5 6 8 9 10日統計
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_GetStatKeepLog]
(
	@dtCreateTime		DATETIME
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @LogStartTime			DATETIME = DATEADD(DAY,-30,DATEADD(DAY,0,@dtCreateTime))
	DECLARE @LogEndTime				DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtCreateTime))


	/*  取登入紀錄  */
	------------------------------------------------------
	SELECT	[PlatformID],
			[AreaID],
			[RegCount],
			[StatTime],
			[OneDayKeep],
			[TwoDayKeep],
			[Third],
			[Fourth],
			[Fifth],
			[Sixth],
			[WeekKeep],
			[eighth],
			[ninth],
			[tenth],
			[MonthKeep]
	FROM [Users].[KeepLog]
	WHERE
		[StatTime] >= @LogStartTime
		AND [StatTime] < @LogEndTime
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_GetStatKeepLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_GetStatKeepLog] TO [xbuser]
    AS [dbo];

