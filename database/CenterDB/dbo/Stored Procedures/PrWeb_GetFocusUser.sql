-- =============================================
-- Author:		<xxx>
-- Create date: <xxx>
-- Description:	<查询关注用户信息>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetFocusUser]
AS
BEGIN	

SELECT 
U.*,
A.AreaName,
P.Name as PlatformName,
ISNULL(M.Amount,0) as CashAmount,
ISNULL(B.Amount,0) as BankAmount,
CASE WHEN GU.UserID IS NULL THEN 0 ELSE 1 END as IsGame,
CASE WHEN GU.UserID IS NULL THEN '' ELSE GS.ServerName END as ServerName
FROM Users.FocusUser as FU 
LEFT JOIN Money.Cash as M with(nolock) ON FU.UserID = M.UserID
LEFT JOIN Money.Bank as B with(nolock) ON FU.UserID = B.UserID
LEFT JOIN Users.UserInfo as U with(nolock) ON U.UserID = FU.UserID
LEFT JOIN Area.Area as A ON U.AreaID = A.AreaID
LEFT JOIN Area.[Platform] as P ON A.PlatformID = P.PlatformID
LEFT JOIN Game.GameUserList as GU ON GU.UserID = FU.UserID
LEFT JOIN ServerInfo.GameServerList as GS ON GU.ServerID = GS.ServerID
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetFocusUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetFocusUser] TO [xbuser]
    AS [dbo];

