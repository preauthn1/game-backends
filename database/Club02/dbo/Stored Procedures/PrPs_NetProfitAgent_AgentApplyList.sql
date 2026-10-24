-- =============================================
-- Author:  jason
-- Create date: 2019/04/16
-- Description: USER 审核代理申请列表(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_AgentApplyList]
(
	@intUserID		int = null			--用户 ID
)
AS
BEGIN

	set nocount on

	/*  取管理者可用平台  */
	declare @temp_platformid table (platformid int)

	insert into @temp_platformid
	select platformid
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intUserID


	select	[ApplyUser],
			[NickName] = isnull([NickName],''),
			[TopUserID],
			[TopUserName] = isnull([TopUserName],''),
			[CreateTime],
			[Status] = isnull([Status],''),
			[UpdateTime] = isnull([NickName],'')
	from (
	select	[ID] = A.ID,
			[ApplyUser] = A.UserID,
			[NickName] = B.[NickName],
			[TopUserID] = C.UserID,
			[TopUserName] = C.NickName,
			[CreateTime] = A.CreateTime,
			--[StatusNo] = A.[Status],
			[Status] = case	when A.[Status] is null or A.[Status] = 0 then '审核中'
							when A.[Status] = 1 then '已通过'
						else '已拒绝' end,
			[UpdateTime] = A.[UpdateTime],
			[SystemUpdater] = A.Updater,
			[AgentUpdater] = A.[AgentUpdater]
	from [dbo].[NetProfitAgent_Apply] as a with (nolock)
		inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.UserID = b.userid
		inner join @temp_platformid as z on b.PlatformID = z.platformid
		left join [dbo].[NetProfitAgent_UserInfo] as c with (nolock) on b.topuserid = c.userid
	where
		A.TopUserID = @intUserID
	) as a
	order by [Status] asc,[ID] desc

	
	--declare @temp_applyuser table ([ID] int,[ApplyUser] int,[NickName] varchar(32),[TopUserID] int,[TopUserName] varchar(32),
	--								[CreateTime] datetime,[Status] varchar(10),[UpdateTime] datetime,[SystemUpdater] varchar(32),[AgentUpdater] int)

	--insert into @temp_applyuser
	--EXEC [dbo].[PrWeb_NetProfitAgent_AgentApplyList]
	--	@intUserID = @intUserID

	--select	[ApplyUser],
	--		[NickName] = isnull([NickName],''),
	--		[TopUserID],
	--		[TopUserName] = isnull([TopUserName],''),
	--		[CreateTime],
	--		[Status] = isnull([Status],''),
	--		[UpdateTime] = isnull([NickName],'')
	--from @temp_applyuser

	return 1
END





