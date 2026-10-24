-- =============================================
-- Author:		<xxx>
-- Create date: <15/08/12>
-- Description:	<删除属性通知>
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_Attr_Del]
@intID INT 
AS
SET NOCOUNT ON
DELETE Notify.Attr
WHERE Id = @intID

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_Attr_Del] TO [xbuser]
    AS [dbo];

