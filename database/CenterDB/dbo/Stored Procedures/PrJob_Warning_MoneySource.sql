-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<金币变化渠道报警10min>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Warning_MoneySource]
AS
SET NOCOUNT ON
IF NOT EXISTS(SELECT 1 FROM Warning.WarningType WHERE TypeID = 2 AND Status = 1)
RETURN 0

--获取配置信息
DECLARE @xmlsetting XML
SELECT @xmlsetting = CAST(setting AS XML)
FROM Warning.WarningType
WHERE TypeID = 2

INSERT INTO Warning.WarningLog
        ( TypeID, Memo, CreateTime )
SELECT 2, mt.SourceName +  '金币变动超出设定范围,当前数量' + CAST(Amount AS varchar), GETDATE()
FROM Stat.MoneySourceLog l, 
(
SELECT st.s.value('(./@SourceID)[1]','int') AS SourceID,
--st.s.value('(./@MinAmount)[1]','int') AS MinAmount,
st.s.value('(./@MaxAmount)[1]','int') AS MaxAmount,
st.s.value('(./@ChangeAmount)[1]','int') AS ChangeAmount
FROM @xmlsetting.nodes('/Setting') as st(s)
) ms,Stat.MoneySourceType mt
WHERE l.StatTime >= CAST(CONVERT(varchar(10),GETDATE(),120) AS DATETIME)
AND l.StatTime <= GETDATE()
AND l.SourceID = ms.SourceID
AND ABS(l.Amount) >= ms.MaxAmount
AND l.SourceID = mt.SourceID
AND NOT EXISTS(SELECT 1 FROM Warning.WarningLog WHERE TypeID = 2 AND CreateTime >= DATEADD(mi,- 120, GETDATE()) AND Memo LIKE  mt.SourceName +  '%')

INSERT INTO Warning.WarningLog
        ( TypeID, Memo, CreateTime )
SELECT 2, mt.SourceName +  '金币变动异常,当前数量' + CAST(Amount AS varchar), GETDATE()
FROM Stat.MoneySourceLog l, 
(
SELECT st.s.value('(./@SourceID)[1]','int') AS SourceID,
--st.s.value('(./@MinAmount)[1]','int') AS MinAmount,
st.s.value('(./@MaxAmount)[1]','int') AS MaxAmount,
st.s.value('(./@ChangeAmount)[1]','int') AS ChangeAmount
FROM @xmlsetting.nodes('/Setting') as st(s)
) ms
,Stat.MoneySourceType mt
WHERE l.StatTime >= CAST(CONVERT(varchar(10),GETDATE(),120) AS DATETIME)
AND l.StatTime <= GETDATE()
AND l.SourceID = ms.SourceID
AND abs(l.Amount) >= abs(ISNULL((SELECT TOP 1 Amount FROM Stat.MoneySourceLog ll 
WHERE ll.SourceID = l.SourceID 
AND ll.StatTime <= CAST(CONVERT(varchar(10),GETDATE(),120) AS DATETIME)  ORDER BY id desc),0)) + ms.ChangeAmount
AND l.SourceID = mt.SourceID
AND NOT EXISTS(SELECT 1 FROM Warning.WarningLog WHERE TypeID = 2 AND CreateTime >= DATEADD(mi,- 120, GETDATE()) AND Memo LIKE  mt.SourceName +  '%')

UPDATE Warning.WarningStatus
SET WarningStatus = 1
FROM Warning.UserFocus
WHERE Warning.UserFocus.UserID = Warning.WarningStatus.UserID
AND EXISTS(SELECT 1 FROM Warning.WarningLog WHERE TypeID = Warning.UserFocus.TypeID AND DATEDIFF(mi, CreateTime, GETDATE()) <= 5)

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Warning_MoneySource] TO [xbuser]
    AS [dbo];

