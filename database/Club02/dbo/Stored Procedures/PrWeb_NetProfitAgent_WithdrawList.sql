-- =============================================
-- Author:  jason
-- Create date: 2019/04/20
-- Description: 全民代理后台 提领列表(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_WithdrawList]
(
@intAdminID int,				--后台管理者ID
@intUserID		int = null,			--用户 ID 查询特定用户
@tnyStatus		int = null,			--单据状态 0:审核中 1:已提领 2:已退还
@dtStartTime	datetime = null,	--申请起始时间
@dtEndTime	datetime = null,		--申请结束时间
@chvMsg		varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartDate	datetime = dateadd(day,0,datediff(day,0,isnull(@dtStartTime,'1970-01-01')))
	declare @EndDate	datetime = dateadd(day,0,datediff(day,0,isnull(@dtEndTime,'2100-01-01')))


	if not exists (select top 1 1 from [dbo].[AgentAdminUser] with (nolock) where [ID] = @intAdminID)
	begin
		set @chvMsg = '操作错误 : 管理者不存在'
		return -1
	end

	
	declare @Temp_platform	table (PlatformID int)
	insert into @Temp_platform
	select	platformid from [dbo].[AgentPlatformRelations] with (nolock) where [AdminID] = @intAdminID

	
	select	a.[ID],
			a.[UserID],
			[WithDrawAmount],	--提领金额
			[WithDrawRate] = cast(WithDrawRate as int),		--提领申请时分成比例
			[StatusNo] = cast(A.[Status] as int),
			[Status] = case	when A.[Status] is null or A.[Status] = 0 then '审核中'
								when A.[Status] = 1 then '已提领'
							else '已退还' end,
			a.[CreateTime],		--提领申请时间
			a.[Updatetime],		--最后处理时间
			[Account] = isnull([Account],''), --最后处理账号		
			LoginName, 
			UnionpayRealName, 
			UnionpayBank, 
			p.BankName,
			UnionpayAccount
	from [dbo].[NetProfitAgent_WithDrawLog] as a with (nolock)
		inner join [dbo].[NetProfitAgent_UserInfo] as c with (nolock) on a.userid = c.userid
		inner join @Temp_platform as z on c.PlatformID = z.PlatformID
		left join [dbo].[AgentAdminUser] as b with (nolock) on a.UpdateUser = b.ID
		left join CenterDB.Users.UserInfo as u with (nolock) on a.UserID = u.UserID
		left join CenterDB.Money.UnionpayBankCode as p with (nolock) on u.UnionpayBank = p.Code
	where
		(a.[UserID] = @intUserID or @intUserID is null)
		and (a.[Status] = @tnyStatus or @tnyStatus is null)
		and a.[Updatetime] >= @StartDate
		and a.[Updatetime] < @EndDate

	set @chvMsg = '操作完成 : 查询完成'
	return 1
END





