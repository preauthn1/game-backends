-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取用户地区>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetUserNickName]
(
	@chvIP	varchar(15)
)
RETURNS varchar(32)
AS
BEGIN

	IF (dbo.Fn_CheckValidIP(@chvIP) = 0)
		return '来自火星'

	declare @lngIP bigint,@chvNickName varchar(32)
	
	set @lngIP = dbo.f_IP2Int(@chvIP)
	SELECT @chvNickName = AreaName FROM Area.IP with (nolock) WHERE @lngIP between IPmin and IPmax
	return ISNULL(@chvNickName,'来自火星')

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetUserNickName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetUserNickName] TO [xbuser]
    AS [dbo];

