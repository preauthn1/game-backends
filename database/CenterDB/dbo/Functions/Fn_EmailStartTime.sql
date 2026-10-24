-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取查询邮件的时间范围>
-- =============================================
CREATE FUNCTION [dbo].[Fn_EmailStartTime]
(
	
)
RETURNS DATETIME
AS
BEGIN
	RETURN dateadd(WW,-1,getdate())
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_EmailStartTime] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_EmailStartTime] TO [xbuser]
    AS [dbo];

