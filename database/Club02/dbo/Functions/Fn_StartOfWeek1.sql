
-- =============================================
-- Author:  CCC
-- Create date: 2019/7/14
-- Description: 取得星期一的日期
-- =============================================
CREATE FUNCTION [dbo].[Fn_StartOfWeek1] 
(
    @dt date
)
RETURNS DATETIME
AS
BEGIN
	declare @i int = case when datepart(weekday,@dt) = 1 then 1 else 0 end
	
    RETURN dateadd(week,datediff(week,0,@dt)-@i,0)
END