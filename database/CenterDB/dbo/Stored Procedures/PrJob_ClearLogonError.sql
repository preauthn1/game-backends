-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<清理登录错误记录>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_ClearLogonError]
AS
SET NOCOUNT ON
TRUNCATE TABLE Logon.AccountLogonError
TRUNCATE TABLE Logon.IPLogonError
TRUNCATE TABLE Logon.HDLogonError

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_ClearLogonError] TO [xbuser]
    AS [dbo];

