
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-我的奖励
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyReward]
(
	@intUserID int=null
)
AS
BEGIN

	SET NOCOUNT ON



	--declare @intWeekMyFlows bigint=123456		--本周總業績 : 計算自己一周的總流水
	--declare @intWeekTeamFlows bigint=45678		--本周團隊總業績 : 計算自己+樹狀下線流水
	--declare @intWeekMemberFlows bigint=654321	--本周直屬總業績 : 直屬下線的流水總和
	--declare @intReward bigint=100				--獎勵 : 當周發放的金額
	--declare @intStatus	tinyint=1					--狀態 : 1=可提的會顯示可提現, 0=其他是顯示為預估值



	SELECT StatTime,
		   WeekMyFlows = sum(WeekMyFlows),
		   WeekTeamFlows = sum(WeekTeamFlows),
		   WeekMemberFlows = sum(WeekMemberFlows),
		   Reward = sum(Reward),
		   Status = case when StatTime < GETDATE() then 1 else 0 end
	  FROM (
			select StatTime = case when datepart(weekday, LogDate) = 1 
								   then convert(varchar(10),dateadd(day,0,dateadd(week,datediff(week,0,LogDate-1)+1,-1)),23)
								   else convert(varchar(10),dateadd(day,0,dateadd(week,datediff(week,0,LogDate)+1,-1)) ,23)
        					  end,
				   WeekMyFlows = [TotalPerformance],
        		   WeekTeamFlows = [TeamPerformance],
        		   WeekMemberFlows = [DirectPerformance],
        		   Reward = [TeamBonusAmount] + [DirectBonusAmount]
			  from [dbo].[Unlimit_BonusReport] with (nolock)
			 where userid = @intUserID
			 ) A
	 GROUP BY StatTime

	return 1
END
