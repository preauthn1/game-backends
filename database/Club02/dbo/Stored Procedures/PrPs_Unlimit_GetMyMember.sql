
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-我的玩家
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyMember]
(
	@intUserID int=null,
	@intTargetID int = null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	declare @intWeekTeamFlows bigint --本周團隊貢獻 : 這個玩家+下線所貢獻之流水
	declare @intWeekMemberFlows bigint --本周玩家貢獻 : 這個玩家個人貢獻的流水
	declare @intTeamMember bigint	--團隊人數 : 這個玩家樹狀下線所有的人數
	declare @intAgent int		--直屬下線人數
	declare @intMember int	--直屬玩家 : 這個玩家的直屬玩家人數
	declare @intMemberType int	--類型 : 該玩家的身分級別(最下線則是玩家)


	
	declare @userlevelstr varchar(max)
	declare @weekstarttime datetime = dateadd(day,-7,datediff(day,0,getdate()))
	declare @weekendtime datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @userlevel bigint
	DECLARE	@StartRow			INT = @intPageSize * (@intPageIndex -1) + 1
	DECLARE	@EndRow				INT = @intPageSize * @intPageIndex


	select	@userlevelstr = [Relations],
			@userlevel = [StageLevel]
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		userid = @intUserID


	/*  取B層級玩家  */
	declare @temp_buser table (userid int,status int,nickname varchar(32))

	insert into @temp_buser
	select	userid,Status,NickName
	from (
			select	a.userid,a.Status,a.NickName
			from [dbo].[Unlimit_UserInfo] as a with (nolock)
			where
				[Relations] like '' + @userlevelstr + '%'
				and a.StageLevel = (@userlevel + 1)
		) as a
	where
		(@intTargetID is null or userid = @intTargetID)


	declare @temp_userdata table (rowno int,userid int,nickname varchar(32),WeekTeamFlows bigint,WeekMemberFlows bigint,TeamMember int,Member int,MemberType int)

	insert into @temp_userdata
	select	
			row_number() over (order by a.userid asc) as rowno,
			a.userid,
			a.nickname,
			WeekTeamFlows = case when a.status = 2 then sum(isnull(b.[TaxAmount],0) + isnull(e.[TaxAmount],0)) else 0 end,
			WeekMemberFlows = case when a.status = 1 then sum(isnull(b.[TaxAmount],0)) else 0 end,
			TeamMember = case when a.status = 2 then 1 + sum(isnull(c.[SubUser],0) + isnull(c.[SubDirectUser],0)) else 0 end,
			Member = case when a.status = 1 then 1 else 0  end,
			MemberType = a.status
	from @temp_buser as a
		left join (
					select	userid,[TaxAmount] = sum([TaxAmount])
					from [dbo].[Unlimit_UserTaxLog] with (nolock)
					where
						[LogDate] >= @weekstarttime
						and [LogDate] < @weekendtime
					group by userid
				) as b on a.userid = b.userid
		left join [dbo].[Unlimit_Promotion] as c with (nolock) on a.userid = c.userid
		left join (
					select	userid,[TaxAmount] = sum([TotalPerformance])
					from [dbo].[Unlimit_BonusReport] with (nolock)
					where
						[LogDate] >= @weekstarttime
						and [LogDate] < @weekendtime
					group by userid
				) as e on a.userid = e.userid
	group by a.userid,a.nickname,c.[SubDirectUser],a.status

	
	/*  回傳資料  */
	select @intPageCount = max(rowno) from @temp_userdata
	set @intPageCount = isnull(@intPageCount,0)
	
	SELECT	userid,
			nickname,
			WeekTeamFlows,
			WeekMemberFlows,
			TeamMember,
			Member,
			MemberType
	FROM (
			SELECT	rowno,
					userid,
					nickname,
					WeekTeamFlows = isnull(WeekTeamFlows,0),
					WeekMemberFlows = isnull(WeekMemberFlows,0),
					TeamMember = isnull(TeamMember,0),
					Member = isnull(Member,0),
					MemberType = isnull(MemberType,0)
			FROM @temp_userdata
		) AS A
	WHERE
		rowno BETWEEN @StartRow AND @EndRow


	return 1
END
