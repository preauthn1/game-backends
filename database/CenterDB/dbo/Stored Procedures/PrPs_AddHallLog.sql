-- =============================================
-- Author:  <xxx>
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

	--写记录
	EXEC LOGDBLINKEDSERVER.LogDB.dbo.PrPs_AddHallLog 
	   @intUserID = @intUserID
	  ,@tnyHDType = @tnyHDType
	  ,@chvHDCode = @chvHDCode
	  ,@chvVersion = @chvVersion
	  ,@chvIPAddress = @chvIPAddress
	  ,@intPlazaServerID = @intPlazaServerID
	  ,@bKick = @bKick
	  ,@chvSavePwd = [REDACTED]
	  ,@bSavePwd = [REDACTED]
	  ,@dtmLoginTime = @dtmLoginTime
	  ,@dtmLogoutTime = @dtmLogoutTime


RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddHallLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddHallLog] TO [xbuser]
    AS [dbo];

