





-- =============================================
-- Author:  alan
-- Create date: 2018/08/10
-- Description:	每天計算前一日代理的月收益
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_AgentProfit_Monthly]
AS
Begin

  DECLARE @dtYesterday datetime = DateAdd(day,-1,getdate())
  DECLARE @YYYYMM varchar(6) = 
	  Convert(varchar,YEAR(@dtYesterday)) + 
	  CASE 
	  WHEN Month(@dtYesterday) >= 10 THEN Convert(varchar,Month(@dtYesterday))
	  ELSE '0' + Convert(varchar,Month(@dtYesterday)) END

  MERGE INTO [dbo].[Stat_AgentProfitMonthly] as T
  USING 
  (SELECT @YYYYMM as YYYYMM, ID, SUM(Flows) as SumFlows, SUM(Income) as SumIncome
    FROM [Club].[dbo].[Stat_AgentProfitDaily]
	WHERE YYYYMMDD like @YYYYMM + '%'
	GROUP BY ID) as S
  ON T.YYYYMM = S.YYYYMM AND T.ID = S.ID

  WHEN MATCHED THEN
	UPDATE SET T.Income = S.SumIncome, T.Flows = S.SumFlows
  WHEN NOT MATCHED THEN
	INSERT (YYYYMM,ID,Flows,Income) VALUES(S.YYYYMM, S.ID, S.SumFlows, S.SumIncome);


 

END


