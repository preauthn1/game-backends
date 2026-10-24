-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<MD5>
-- =============================================
CREATE FUNCTION [dbo].[Fn_MD5]
(
	@chvCode VARCHAR(64)
)
RETURNS VARCHAR(32)
AS
BEGIN
	 DECLARE @md5 varchar(34)
    SET @md5 = sys.fn_VarBinToHexStr(hashbytes('MD5', @chvCode));
    --RETURN SUBSTRING(@md5,11,16)   --16位
    RETURN SUBSTRING(@md5,3,32)    --32位

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_MD5] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_MD5] TO [xbuser]
    AS [dbo];

