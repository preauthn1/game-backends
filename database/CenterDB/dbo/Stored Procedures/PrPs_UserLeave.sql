-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<玩家离开广场>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserLeave]
@intUserID INT,
@sintServerID SMALLINT,
@chvCer VARCHAR(32)
AS
SET NOCOUNT ON


--清除证书
/*
DELETE Game.Certification WHERE UserID = @intUserID AND CerCode = @chvCer
if @@ROWCOUNT = 0
return 1*/

if EXISTS (select 1 from Game.Certification WHERE UserID = @intUserID AND CerCode = @chvCer)
DELETE Game.HallUserList
WHERE UserID = @intUserID
AND ServerID = @sintServerID




RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLeave] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLeave] TO [xbuser]
    AS [dbo];

