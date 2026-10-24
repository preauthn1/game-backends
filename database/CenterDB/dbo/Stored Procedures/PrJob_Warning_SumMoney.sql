-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<总金币报警10min>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Warning_SumMoney]
as
set nocount ON
DECLARE @intWarningType INT,@tnyStatus TINYINT,@xmlSetting xml
SET @intWarningType = 1

--获取通知配置
SELECT @tnyStatus = Status,@xmlSetting = CAST(Setting AS Xml)
FROM Warning.WarningType
WHERE TypeID = @intWarningType

IF @tnyStatus <> 1
RETURN 0

--获取配置数据
DECLARE 
@lngMaxAmount BIGINT,
@lngRangeAmount BIGINT

SET @lngMaxAmount = @xmlSetting.value('(/Setting/@MaxAmount)[1]','bigint')
SET @lngRangeAmount = @xmlSetting.value('(/Setting/@ChangeAmount)[1]','bigint')

IF  @lngMaxAmount IS NULL 
OR @lngRangeAmount IS NULL
RETURN 0

--获取当前总金币
declare @lngCashAmount bigint,
@lngBankAmount bigint,
@lngCircleAmount BIGINT,
@lngSumAmount BIGINT,
@lngExMoneyAmount BIGINT

--现金
SELECT @lngCashAmount = SUM(Amount)
FROM Money.Cash WITH(nolock)


--计算总金币
SET @lngSumAmount = ISNULL(@lngCashAmount, 0)


--检查金币范围
IF @lngSumAmount >= @lngMaxAmount
INSERT INTO Warning.WarningLog
        ( TypeID, Memo, CreateTime )
SELECT @intWarningType, '总金币超出设定范围，当前总金币' + CAST(@lngSumAmount AS varchar), GETDATE()

--获取前一天总金币
SELECT @lngExMoneyAmount = ISNULL(CashAmount + BankAmount,0)
from
(SELECT TOP 1 *
FROM Stat.SumMoneyLog
WHERE StatTime >= CAST(CONVERT(varchar(10),GETDATE(),120) AS DATETIME)
ORDER BY StatTime asc
) t


IF @lngExMoneyAmount > 0 AND @lngExMoneyAmount + @lngRangeAmount <= @lngSumAmount
INSERT INTO Warning.WarningLog
        ( TypeID, Memo, CreateTime )
SELECT @intWarningType, '总金币超出变化范围，当前总金币' + CAST(@lngSumAmount AS varchar) + ',变动数量' + CAST(@lngSumAmount - @lngExMoneyAmount AS varchar),GETDATE() 

UPDATE Warning.WarningStatus
SET WarningStatus = 1
FROM Warning.UserFocus
WHERE Warning.UserFocus.UserID = Warning.WarningStatus.UserID
AND EXISTS(SELECT 1 FROM Warning.WarningLog WHERE TypeID = Warning.UserFocus.TypeID AND DATEDIFF(mi, CreateTime, GETDATE()) <= 5)


RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Warning_SumMoney] TO [xbuser]
    AS [dbo];

