-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<注册人数报警>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Warning_Reg]
AS
SET NOCOUNT ON
DECLARE @intWarningType INT,@tnyStatus TINYINT,@xmlSetting xml
SET @intWarningType = 7

--获取通知配置
SELECT @tnyStatus = Status,@xmlSetting = CAST(Setting AS Xml)
FROM Warning.WarningType
WHERE TypeID = @intWarningType

IF @tnyStatus <> 1
RETURN 0

DECLARE @intTimeSpan INT,@intRegCount INT,@intExRegCount INT
DECLARE cur_regcheck CURSOR FAST_FORWARD READ_ONLY
FOR
SELECT st.s.value('(./@TimeSpan)[1]','int') AS SourceID,
st.s.value('(./@RegCount)[1]','int') AS RegCount
FROM @xmlsetting.nodes('/Setting') as st(s)

OPEN cur_regcheck
FETCH NEXT FROM cur_regcheck INTO @intTimeSpan,@intRegCount
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @intExRegCount = (SELECT COUNT(1) FROM Users.UserInfo WITH(nolock) WHERE dateadd(mi, @intTimeSpan,CreateTime) >=  GETDATE())
	
	IF @intExRegCount >= @intRegCount
	INSERT INTO Warning.WarningLog
	        ( TypeID, Memo, CreateTime )
	SELECT 7,CAST(@intTimeSpan AS VARCHAR)+'分钟内注册人数达到' + CAST(@intExRegCount AS varchar),GETDATE()

	FETCH NEXT FROM cur_regcheck INTO @intTimeSpan,@intRegCount
END

CLOSE cur_regcheck
DEALLOCATE cur_regcheck


UPDATE Warning.WarningStatus
SET WarningStatus = 1
FROM Warning.UserFocus uf,Warning.WarningStatus us
WHERE uf.UserID = us.UserID
AND EXISTS(SELECT 1 FROM Warning.WarningLog WHERE TypeID = uf.TypeID AND DATEDIFF(mi, CreateTime, GETDATE()) <= 5)

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Warning_Reg] TO [xbuser]
    AS [dbo];

