
-- =============================================
-- Author:  <xxxx>
-- Create date: <xxxx>
-- Description:	取得渠道管理者資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetAreaAdminUser]
@intAdminID INT
AS
BEGIN
	select U.*,A.AreaName from Area.AreaAdminUser U with(nolock)
	left join area.area A with(nolock)
	on  U.AreaID=A.AreaID
	where U.AdminID=@intAdminID
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetAreaAdminUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetAreaAdminUser] TO [xbuser]
    AS [dbo];

