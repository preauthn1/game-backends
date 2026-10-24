-- =============================================
-- Author:  jason
-- Create date: 2019/04/16
-- Description: USER 审核代理申请列表(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_AgentApplyList]
(
@intAdminID int = null,				--后台管理者ID
@intPlatformID int = null,			--平台ID
@intAreaID		int = null,			--渠道ID
@dtStartTime	datetime = null,	--申请起始时间
@dtEndTime		datetime = null,	--申请结束时间
@intUserID		int = null,			--用户 ID
@intTargetID	int = null
)
AS
BEGIN

	set nocount on

	declare @StartDate	datetime = dateadd(day,0,datediff(day,0,isnull(@dtStartTime,'1970-01-01')))
	declare @EndDate	datetime = dateadd(day,1,datediff(day,0,isnull(@dtEndTime,'2100-01-01')))


	if(@intAdminID is not null)
	begin
		select	[ID] = A.ID,
				[ApplyUser] = A.UserID,
				[NickName] = B.[NickName],
				[TopUserID] = C.UserID,
				[TopUserName] = C.NickName,
				[CreateTime] = A.CreateTime,
				[StatusNo] = cast(A.[Status] as int),
				[Status] = case	when A.[Status] is null or A.[Status] = 0 then '审核中'
								when A.[Status] = 1 then '已通过'
							else '已拒绝' end,
				[UpdateTime] = A.[UpdateTime],
				[SystemUpdater] = A.Updater,
				[AgentUpdater] = A.[AgentUpdater]
		from [dbo].[NetProfitAgent_Apply] as a with (nolock)
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.UserID = b.userid
			left join [dbo].[NetProfitAgent_UserInfo] as c with (nolock) on b.topuserid = c.userid
		where
			A.CreateTime >= @StartDate
			AND A.CreateTime <= @EndDate
			AND (@intTargetID is null or a.UserID = @intTargetID)
		order by [Status] asc,[ID] desc
	end

	if(@intUserID is not null)
	begin
		select	[ID] = A.ID,
				[ApplyUser] = A.UserID,
				[NickName] = B.[NickName],
				[TopUserID] = C.UserID,
				[TopUserName] = C.NickName,
				[CreateTime] = A.CreateTime,
				[StatusNo] = cast(A.[Status] as int),
				[Status] = case	when A.[Status] is null or A.[Status] = 0 then '审核中'
								when A.[Status] = 1 then '已通过'
							else '已拒绝' end,
				[UpdateTime] = A.[UpdateTime],
				[SystemUpdater] = A.Updater,
				[AgentUpdater] = A.[AgentUpdater]
		from [dbo].[NetProfitAgent_Apply] as a with (nolock)
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.UserID = b.userid
			left join [dbo].[NetProfitAgent_UserInfo] as c with (nolock) on b.topuserid = c.userid
		where
			A.TopUserID = @intUserID
			AND A.CreateTime >= @StartDate
			AND A.CreateTime <= @EndDate
			AND (@intTargetID is null or a.UserID = @intTargetID)
		order by [Status] asc,[ID] desc
	end

	return 1
END





