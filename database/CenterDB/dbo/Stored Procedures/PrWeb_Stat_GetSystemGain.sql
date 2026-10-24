-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询系统收益情况>
--	2018/03/19	Jason		調整寫法並新增變數接參數
--	2018/03/26	Jason		調整ORDER BY從ASC改為DESC
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetSystemGain]	
	@dtbeginTime		datetime=null,
	@dtendTime			datetime=null
AS
BEGIN

	DECLARE @BeginTime	DATETIME = ISNULL(@dtbeginTime,'1900-01-01 00:00:00.000')
	DECLARE @EndTime	DATETIME = ISNULL(@dtendTime,'2100-01-01 00:00:00.000')

	SELECT	C.Amount AS ConvertAmount
			,C.TaxAmount AS ConvertTax
			,C.RealTaxAmount AS ConvertRealTax
			,C.StatTime
			,ChargeAmount = ISNULL(S.ChargeAmount,0)
			,TaxAmount = ISNULL(S.TaxAmount,0)
			,RobotTaxAmount = ISNULL(S.RobotTaxAmount,0)
			,GiveAmount = ISNULL(S.GiveAmount,0)
	FROM Stat.ConvertLog AS C WITH (NOLOCK)
		LEFT JOIN (
					SELECT	CONVERT(VARCHAR(10),StatTime,120) AS StatTime
							,SUM(CASE WHEN ST.GroupID = 4 THEN Amount ELSE 0 END) AS ChargeAmount				--充值统计
							,SUM(CASE WHEN L.SourceID < 10000 THEN Amount ELSE 0 END) AS TaxAmount			--用户税收		
							,SUM(CASE WHEN L.SourceID < 10000 THEN RobotAmount ELSE 0 END) AS RobotTaxAmount	--机器人税收
							,SUM(CASE WHEN ST.GroupID = 7 THEN Amount ELSE 0 END) AS GiveAmount				--赠送金币		
					FROM Stat.MoneySourceLog AS L WITH (NOLOCK)
						LEFT JOIN Stat.MoneySourceType AS ST WITH (NOLOCK) ON L.SourceID = ST.SourceID
					WHERE L.StatTime >= @BeginTime
						AND L.StatTime <= @EndTime
					GROUP BY CONVERT(VARCHAR(10),StatTime,120)
				) AS S ON DATEDIFF(DAY,C.StatTime,S.StatTime) = 0
	WHERE C.StatTime >= @BeginTime
		AND C.StatTime <= @EndTime

	ORDER BY C.StatTime	DESC
		
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetSystemGain] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetSystemGain] TO [xbuser]
    AS [dbo];

