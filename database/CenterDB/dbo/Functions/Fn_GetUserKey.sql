-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取用户加密字符串>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetUserKey]
(
	@intUserID	int,
	@intLevel	bigint
)
RETURNS varchar(32)
AS
BEGIN
	
	RETURN dbo.Fn_MD5(CAST(@intUserID AS VARCHAR)+CAST(@intLevel AS VARCHAR)+'!@#FAsD')

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetUserKey] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetUserKey] TO [xbuser]
    AS [dbo];

