-- =============================================
-- Author:  jason
-- Create date: 2019/04/20
-- Description: 全民代理后台 提领审核(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_WithdrawFinish]
(
	@intAdminID int,				--后台管理者ID
	@intOrderID	int,				--提领单据ID
	@tnyStatus	int,				--1:已提领(同意) 2:已退还(拒绝)
	@chvMsg		varchar(100)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @userplatformid int
	declare @orderstatus	tinyint
	declare @userid			int
	declare @orderamount	bigint	
	declare @Temp_platformid	table (platformid int)
	

	/*  取管理者可使用平台  */
	insert into @Temp_platformid
	select platformid from [dbo].[AgentPlatformRelations] with (nolock) where AdminID = @intAdminID


	/*  取提领单据用户归属平台  */
	select	@userplatformid = platformid,
			@userid	= a.userid,
			@orderstatus = a.status,
			@orderamount = [WithDrawAmount]
	from [dbo].[NetProfitAgent_WithDrawLog] as a
		inner join [dbo].[NetProfitAgent_UserInfo] as b on a.userid = b.UserID
	where
		a.id = @intOrderID

	/*  取出用户关连  */
	declare @topuserrelation varchar(max)
	select	@topuserrelation = [Relations]
	from (
			select	topuserid
			from [dbo].[NetProfitAgent_UserInfo] with (nolock)
			where
				userid = @userid
		) as a
		inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.topuserid = b.UserID

	declare @temp_upuser table (userid int)
	insert into @temp_upuser
	select	cast(value as int)
	from [dbo].[fn_Split](@topuserrelation,',')



	if (@tnyStatus not in (1,2))
	begin
		set @chvMsg = '操作错误 : 审核失败'
		return -1
	end

	if not exists(select top 1 1 from @Temp_platformid where platformid = @userplatformid)
	begin
		set @chvMsg = '操作错误 : 管理者无权限审核'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[NetProfitAgent_WithDrawLog] with (nolock) where id = @intOrderID)
	begin
		set @chvMsg = '操作错误 : 提领单据不存在'
		return -1
	end

	if(@orderstatus <> 0)
	begin
		set @chvMsg = '操作错误 : 提领单据已处理过无法再次审核'
		return -1
	end

	if(@orderamount <= 0)
	begin
		set @chvMsg = '操作错误 : 提领单据单据金额错误'
		return -1
	end


	/*  单据更新  */
	update [dbo].[NetProfitAgent_WithDrawLog]
	set [Status] = @tnyStatus,
	    UpdateUser = @intAdminID
	where
		id = @intOrderID

	if (@@rowcount <> 1)
	begin
		set @chvMsg = '操作错误 : 审核用户提领失败'
		return -1
	end

	if(@tnyStatus = 2)
	begin
		update [dbo].[NetProfitAgent_Pool]
		set [WithDrawAmount] = [WithDrawAmount] - @orderamount
		where
			userid = @userid

		if (@@rowcount <> 1)
		begin
			set @chvMsg = '操作错误 : 更新用户数据失败'
			return -1
		end

		update [dbo].[NetProfitAgent_UserInfo]
		set [SumSubWithdrawAmount] = [SumSubWithdrawAmount] - @orderamount
		where
			userid in (select userid from @temp_upuser)
	end

	set @chvMsg = '操作错误 : 审核完成'
	return 1
END







