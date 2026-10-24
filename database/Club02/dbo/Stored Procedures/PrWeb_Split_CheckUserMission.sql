-- =============================================
-- Author:  jason
-- Create date: 2019/05/06
-- Description: USER 确认玩家任务(裂变)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_CheckUserMission]
(
	@intUserID		int,				--用户 ID
	@intRtnCode		int OUTPUT,
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @platformid int
	declare @missionflag1 int
	declare @missionflag2 int
	declare @missionflag3 int
	declare @missionflag4 int
	declare @missionflag5 int
	declare @minamount bigint
	declare @mobilename varchar(32)
	declare @alipayaccount varchar(64)
	declare @unionpayaccount varchar(64)
	declare @chargeamount bigint
	declare @convartamount bigint
	declare @updateflag1 int = 0
	declare @updateflag2 int = 0
	declare @updateflag3 int = 0
	declare @updateflag4 int = 0
	declare @updateflag5 int = 0
	declare @missioncondition1 int
	declare @missioncondition2 int
	declare @missioncondition3 int


	/*  取用户平台  */
	select	@platformid = [PlatformID]
	from [dbo].[Split_UserInfo] with (nolock)
	where
		userid = @intUserID


	/*  取用户现有任务  */
	select	@missionflag1 = [MobileBind],
			@missionflag2 = [AlipayBind],
			@missionflag3 = [UnionPayBind],
			@missionflag4 = [ChargeBind],
			@missionflag5 = [ConvertBind]
	from [dbo].[Split_UserMission] with (nolock)
	where
		userid = @intUserID


	/*  取最低充值兑换额度  */
	select	@minamount = [TaxAmount]
	from [dbo].[Split_SystemLevelRate] with (nolock)
	where
		platformid = @platformid


	/*  取用户数据  */
	select	@mobilename = [LoginName],
			@alipayaccount = [AlipayAccount],
			@unionpayaccount = [UnionpayAccount],
			@chargeamount = [SumCharge],
			@convartamount = [SumConvert]
	from centerdb.users.userinfo with (nolock)
	where
		userid = @intUserID


	if(@missionflag1 = 1) and (@missionflag2 = 1 or @missionflag3 = 1) and (@missionflag4 = 1 or @missionflag5 = 1)
	begin
		set @intRtnCode = 0
		set @chvMsg = '操作成功 : 用户已绑定成功过'
		return 1
	end


	if(@missionflag1 = 0) and (@mobilename not like 'Fast%')
	begin
		set @updateflag1 = 1
	end

	if(@missionflag2 = 0) and (@alipayaccount is not null)
	begin
		set @updateflag2 = 1
	end
	

	if(@missionflag3 = 0) and (@unionpayaccount is not null)
	begin
		set @updateflag3 = 1
	end


	if(@missionflag4= 0) and (@chargeamount >=  @minamount)
	begin
		set @updateflag4 = 1
	end


	if(@missionflag5 = 0) and (@convartamount > 0)
	begin
		set @updateflag5 = 1
	end


	if((@updateflag1+@updateflag2+@updateflag3+@updateflag4+@updateflag5) > 0)
	begin
		update [dbo].[Split_UserMission]
		set [MobileBind] = case when @updateflag1 = 1 then 1 else [MobileBind] end,
			[AlipayBind] = case when @updateflag2 = 1 then 1 else [AlipayBind] end,
			[UnionPayBind] = case when @updateflag3 = 1 then 1 else [UnionPayBind] end,
			[ChargeBind] = case when @updateflag4 = 1 then 1 else [ChargeBind] end,
			[ConvertBind] = case when @updateflag5 = 1 then 1 else [ChargeBind] end,
			[MobileBindDate] = case when @updateflag1 = 1 then getdate() else [MobileBindDate] end,
			[AlipayBindDate] = case when @updateflag2 = 1 then getdate() else [AlipayBindDate] end,
			[UnionPayBindDate] = case when @updateflag3 = 1 then getdate() else [UnionPayBindDate] end,
			[ChargeBindDate] = case when @updateflag4 = 1 then getdate() else [ChargeBindDate] end,
			[ConvertBindDate] = case when @updateflag5 = 1 then getdate() else [ConvertBindDate] end
		where
			userid = @intUserID

		if @@rowcount <> 1
		begin
			set @intRtnCode = 0
			set @chvMsg = '操作失败 : 更新用户任务状态失败'
			return 1
		end
	end


	/*  取用户现有任务  */
	select	@missionflag1 = [MobileBind],
			@missionflag2 = [AlipayBind],
			@missionflag3 = [UnionPayBind],
			@missionflag4 = [ChargeBind],
			@missionflag5 = [ConvertBind]
	from [dbo].[Split_UserMission] with (nolock)
	where
		userid = @intUserID

	
	/*  确认条件1是否达成  */
	if(@missionflag1 = 1) and (@missionflag2 = 1 or @missionflag3 = 1) and (@missionflag4 = 1 or @missionflag5 = 1)
	begin
		set @intRtnCode = 1
		set @chvMsg = '操作成功 : 用户任务完成'
		return 1
	end
	else
	begin
		set @intRtnCode = 0
		set @chvMsg = '操作失败 : 用户任务尚未达成'
		return 1
	end

END









