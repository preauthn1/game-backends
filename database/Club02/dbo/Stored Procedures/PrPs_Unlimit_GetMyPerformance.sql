
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-我的业绩
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyPerformance]
(
	@intUserID int=null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	----1.	總業績欄(每周重置)
	--declare @intWeekMyFlows bigint=123456 --本周總業績 : 計算自己一周的總流水
	--declare @intWeekTeamFlows bigint=45678 --本周團隊總業績 : 計算自己+樹狀下線流水
	--declare @intWeekMemberFlows bigint=654321	--本周直屬總業績 : 直屬下線的流水總和
	--declare @intReward bigint=100	--現金獎勵 : 自己直推業績*自己級別的提成比例+下線直屬代理所有業績*(自己級別的提成比例-下線直屬代理級別的提成比例)+….直到所有代理的返利都加完 + 直屬下線提現的5%  – 每日估算,以周一收到的獎勵為準

	----2.	每日業績欄位
	--declare @intDayMyFlows bigint=2222 --日總業績 : 每日自己的流水業績
	--declare @intDayTeamFlows bigint=3333 --日團隊業績 : 每日自己+樹狀下線流水的業績
	--declare @intDayMemberFlows bigint=4444	--每日直屬+樹狀下線流水的業績

	declare @intWeekMyFlows bigint
	declare @intWeekTeamFlows bigint
	declare @intWeekMemberFlows bigint
	declare @intReward bigint
	declare @intDayMyFlows bigint
	declare @intDayTeamFlows bigint
	declare @intDayMemberFlows bigint


	declare @userlevelstr varchar(max)
	declare @weekstarttime datetime = case when datepart(weekday,getdate()) = 1 then dateadd(day,0,dateadd(week,datediff(week,0,getdate()-1),0))
											else dateadd(day,0,dateadd(week,datediff(week,0,getdate()),0)) end
	declare @weekendtime datetime = dateadd(day,7,@weekstarttime)
	declare @userlevel bigint


	select	@userlevelstr = [Relations],
			@userlevel = [StageLevel]
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		userid = @intUserID


	/*  取本周總業績/本周團隊總業績/本周直屬總業績  */
	select	@intWeekMemberFlows = sum(case when (@userlevel+1) = a.StageLevel then [TaxAmount] else 0 end),
			@intWeekTeamFlows = sum(case when (@userlevel+1) < a.StageLevel then [TaxAmount] else 0 end) 
	from [dbo].[Unlimit_UserInfo] as a with (nolock)
		inner join [dbo].[Unlimit_UserTaxLog] as b with (nolock) on a.userid = b.userid and b.LogDate >= @weekstarttime and b.LogDate < @weekendtime
	where
		[Relations] like '' + @userlevelstr + '%'
		and a.userid <> @intUserID


	/*  取現金獎勵  */
	select	@intReward = isnull([TeamBonusAmount],0) + isnull([DirectBonusAmount],0)
	from [dbo].[Unlimit_BonusReport] with (nolock)
	where
		userid = @intUserID
		and LogDate >= @weekstarttime and LogDate < @weekendtime


	set @intWeekMemberFlows = isnull(@intWeekMemberFlows,0)
	set @intWeekTeamFlows = isnull(@intWeekTeamFlows,0)
	set @intWeekMyFlows = @intWeekMemberFlows + @intWeekTeamFlows
	set @intReward = isnull(@intReward,0)	


	select	@intWeekMyFlows as WeekMyFlows,
			@intWeekTeamFlows as WeekTeamFlows,
			@intWeekMemberFlows as WeekMemberFlows,
			@intReward as Reward


	select	StatTime = convert(varchar(10),LogDate,23),
			WeekMemberFlows = sum(case when (@userlevel+1) = a.StageLevel then [TaxAmount] else 0 end),
			WeekTeamFlows = sum(case when (@userlevel+1) < a.StageLevel then [TaxAmount] else 0 end),
			WeekMyFlows = sum([TaxAmount])
	from [dbo].[Unlimit_UserInfo] as a with (nolock)
		inner join [dbo].[Unlimit_UserTaxLog] as b with (nolock) on a.userid = b.userid and b.LogDate >= @weekstarttime and b.LogDate < @weekendtime
	where
		[Relations] like '' + @userlevelstr + '%'
		and a.userid <> @intUserID
	group by convert(varchar(10),LogDate,23)

	set @intPageCount = isnull(@intPageCount,0)

	return 1
END
