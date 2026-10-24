-- =============================================
-- Author:		<xxx>
-- Create date: <15/08/12>
-- Description:	<属性改变通知>
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_Attr_GetList]
AS
SET NOCOUNT ON
SELECT TOP 200 * INTO #attrlist 
FROM Notify.Attr WITH(INDEX=IX_Attr_CreateTime_Status)
WHERE Status = 0
AND DATEDIFF(mi, CreateTime, GETDATE()) <= 5
ORDER BY LastPostTime

UPDATE Notify.Attr
SET Status = 1,
LastPostTime = GETDATE()
FROM #attrlist
WHERE Notify.Attr.Id = #attrlist.Id

SELECT UserID, TypeID, ISNULL(Param, 0) AS Param, ISNULL(SParam, '') AS SParam FROM #attrlist

DROP TABLE #attrlist

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_Attr_GetList] TO [xbuser]
    AS [dbo];

