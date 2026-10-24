-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计登录记录>
--	2018/04/09	Jason		修正HDType錯誤問題
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatLogonLog]
AS
SET NOCOUNT ON
DECLARE @intTotalLogonCount INT,
@intPCLogonCount INT,
@intIOSLogonCount INT,
@intAndroidLogonCount INT,
@dtmStatTime DATETIME,
@dtmEndTime DATETIME

SET @dtmStatTime = CAST(CONVERT(VARCHAR(10), DATEADD(DAY, -1, GETDATE()), 120) AS DATETIME) 
SET @dtmEndTime = CAST(CONVERT(VARCHAR(10), GETDATE(), 120) AS DATETIME) 

DELETE FROM Stat.LogonUserLog where DATEDIFF(dd,@dtmStatTime,StatTime)=0
INSERT INTO Stat.LogonUserLog(TotalLogonCount,PCLogonCount,IOSLogonCount,AndroidLogonCount,PlatformID,StatTime)
SELECT 0,0,0,0,PlatformID,@dtmStatTime
FROM Area.[Platform]


--统计各平台总数
UPDATE L
SET L.TotalLogonCount = S.TotalLogonCount,
L.PCLogonCount = S.PCLogonCount,
L.IOSLogonCount = S.IOSLogonCount,
L.AndroidLogonCount = S.AndroidLogonCount
FROM Stat.LogonUserLog as L
INNER JOIN (
	SELECT	A.PlatformID,
			COUNT(1) as  TotalLogonCount,
			ISNULL(SUM(CASE WHEN u.HDType = 1 THEN 1 ELSE 0 END),0) as PCLogonCount,
			ISNULL(SUM(CASE WHEN u.HDType = 3 THEN 1 ELSE 0 END),0) as IOSLogonCount,
			ISNULL(SUM(CASE WHEN u.HDType = 2 THEN 1 ELSE 0 END),0) as AndroidLogonCount		
	FROM Users.UserInfo as u with(nolock) 
	inner join Area.Area as A ON u.AreaID=A.AreaID
	WHERE LastLogonTime >= @dtmStatTime and LastLogonTime < @dtmEndTime 
	Group BY A.PlatformID
) as S ON L.PlatformID = S.PlatformID
WHERE DATEDIFF(DD,StatTime,@dtmStatTime)=0

--统计总数
INSERT INTO Stat.LogonUserLog(TotalLogonCount,PCLogonCount,IOSLogonCount,AndroidLogonCount,PlatformID,StatTime)
select SUM(TotalLogonCount),SUM(PCLogonCount),SUM(IOSLogonCount),SUM(AndroidLogonCount),0,@dtmStatTime
FROM Stat.LogonUserLog 
WHERE DATEDIFF(DD,StatTime,@dtmStatTime)=0


RETURN 1
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatLogonLog] TO [xbuser]
    AS [dbo];

