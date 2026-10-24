-- =============================================
-- Author:  alan
-- Create date: 2018/11/30
-- Description:	有效會員留存
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetActiveUserKeepLog]
AS
BEGIN
	SET NOCOUNT ON

	SELECT Convert(datetime,CAST(YYYYMMDD as varchar),112) as StatTime,* from Stat.ActiveUserKeepLog with (nolock)
	order by YYYYMMDD DESC

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetActiveUserKeepLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetActiveUserKeepLog] TO [xbuser]
    AS [dbo];

