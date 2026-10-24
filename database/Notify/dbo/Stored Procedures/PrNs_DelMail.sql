-- =============================================
-- Author:		<xxx>
-- Create date: <15/08/11>
-- Description:	<删除邮件>
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_DelMail]
@intQID INT
AS
SET NOCOUNT ON
DELETE Notify.EmailQueue
WHERE Id = @intQID

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_DelMail] TO [xbuser]
    AS [dbo];

