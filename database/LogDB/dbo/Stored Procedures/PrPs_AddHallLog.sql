-- =============================================
-- Author:		<xxx>
-- Create date: <xxx>
-- Description:	<添加广场登录记录>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddHallLog]
@intUserID INT,
@tnyHDType TINYINT,
@chvHDCode VARCHAR(64),
@chvVersion VARCHAR(16),
@chvIPAddress VARCHAR(15),
@intPlazaServerID INT,
@bKick BIT,
@chvSavePwd VARCHAR(32),
@bSavePwd BIT,
@dtmLoginTime DATETIME,
@dtmLogoutTime DATETIME
AS
SET NOCOUNT ON

--不写退游戏记录
return 1
--INSERT INTO Users.HallLog
--	(UserID, HDType, HDCode, Version,IPAddress, PlazaServerID, Kick,SavePwd,SelectSavePwd, LoginTime, LogoutTime)
--	SELECT
--	@intUserID,@tnyHDType,@chvHDCode,@chvVersion,@chvIPAddress,@intPlazaServerID,@bKick,@chvSavePwd,@bSavePwd,@dtmLoginTime,@dtmLogoutTime



RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddHallLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddHallLog] TO [xbuser]
    AS [dbo];

