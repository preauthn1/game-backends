-- =============================================
-- Author:  alan
-- Create date: 2018-3-14
-- Description:	取得兌換紀錄
--	2018/03/20	Jason		調整寫法把原本直接從LOGDB資料表取資料改為呼叫LOGDBLINKEDSERVER中的SP回傳資料
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetConvertLog]
	@intUserID int,
	@logInBeginTime datetime=null, 
	@logInEndTime datetime=null,
	@tnyStatus		tinyint=null
AS
BEGIN
	
	SET NOCOUNT ON

	/*  取資料  */
	---------------------------------------------------------------------------------------------
	CREATE TABLE #ConvertLogByUser	(Amount BIGINT,TaxAmount BIGINT,Status TINYINT,Payment VARCHAR(255),CreateTime DATETIME)

	INSERT INTO #ConvertLogByUser
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetConvertLogByUserID]
		@intUserID = @intUserID,
		@logInBeginTime = @logInBeginTime,
		@logInEndTime = @logInEndTime,
		@tnyStatus = @tnyStatus
	---------------------------------------------------------------------------------------------

	
	/*  回傳資料  */
	---------------------------------------------------------------------------------------------
	SELECT	Amount,
			TaxAmount,
			Status,
			Payment,
			CreateTime
	FROM #ConvertLogByUser
	ORDER BY CreateTime
	---------------------------------------------------------------------------------------------

	DROP TABLE #ConvertLogByUser
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLog] TO [xbuser]
    AS [dbo];

