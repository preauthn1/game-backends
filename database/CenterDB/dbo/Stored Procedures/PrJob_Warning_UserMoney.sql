-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<用户金币报警 30min>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Warning_UserMoney]
AS
SET NOCOUNT ON

DECLARE @intWarningType INT,@tnyStatus TINYINT,@xmlSetting xml
SET @intWarningType = 6

--获取通知配置
SELECT @tnyStatus = Status,@xmlSetting = CAST(Setting AS Xml)
FROM Warning.WarningType
WHERE TypeID = @intWarningType

IF @tnyStatus <> 1
RETURN 0

DECLARE @lngMaxAmount BIGINT
SET @lngMaxAmount = @xmlSetting.value('(/Setting/@Amount)[1]','bigint')
IF  @lngMaxAmount IS NULL 
RETURN 0

SELECT Amount,UserID 
INTO #topmoney
FROM (SELECT m.Amount  AS Amount,m.UserID FROM Money.cash m 
WHERE NOT EXISTS(SELECT 1 FROM Warning.UserSumMoneyLog WHERE UserID = m.UserID AND DATEDIFF(day, CreateTime,GETDATE()) <= 3)
) t  
WHERE t.Amount >= @lngMaxAmount

INSERT INTO Warning.WarningLog
        ( TypeID, Memo, CreateTime )
SELECT  6,'金币数量超出设定范围,当前数量' + CAST(Amount AS varchar) + ',用户ID:' + CAST(UserID AS varchar),GETDATE()
from #topmoney

INSERT INTO Warning.UserSumMoneyLog
(UserID, Amount, CreateTime)
SELECT UserID, Amount, GETDATE()
FROM #topmoney

DROP TABLE #topmoney


UPDATE Warning.WarningStatus
SET WarningStatus = 1
FROM Warning.UserFocus
WHERE Warning.UserFocus.UserID = Warning.WarningStatus.UserID
AND EXISTS(SELECT 1 FROM Warning.WarningLog WHERE TypeID = Warning.UserFocus.TypeID AND DATEDIFF(mi, CreateTime, GETDATE()) <= 5)

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Warning_UserMoney] TO [xbuser]
    AS [dbo];

