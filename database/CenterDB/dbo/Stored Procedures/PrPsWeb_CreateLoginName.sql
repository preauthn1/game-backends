-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<生成用户名>
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_CreateLoginName]
@chvPrefix VARCHAR(32) = NULL,
@intLen INT,
@chvLoginName VARCHAR(32) OUTPUT
AS
SET NOCOUNT ON

SET @chvLoginName = LEFT(ISNULL(@chvPrefix,'') + REPLACE(NEWID(), '-', ''), @intLen)

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_CreateLoginName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_CreateLoginName] TO [xbuser]
    AS [dbo];

