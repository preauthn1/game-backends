-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER QRCode加入
-- 2019-04-15	jason	全民代理模式/无限级默认加入 其余须审核
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AddUserQRCode]	
@intUserID		int,				--用户 ID
@intplatformid  int,
@intareaid		int,
--@chvQrCode		varchar(250),		--用户 qrcode
@chvMsg			varchar(250)	OUTPUT
AS
BEGIN

	set nocount on

	declare @status tinyint = 1
	declare @userlevel int = 1
	declare @defaultvalues int = 0
	declare @chvQRUrl varchar(250)
	declare @password varchar(64)
	declare @modeopenstatus tinyint = 1
	declare @modeclosestatus tinyint = 0

	
	/*  取得玩家昵称  */
	declare @temp_getnickname varchar(30)
	select	@temp_getnickname = centerdb.[dbo].[Fn_GetUserNickName]([CreateIPAddress]),
			@password = [REDACTED]
	from centerdb.[Users].[UserInfo] with (nolock)
	where
		userid = @intUserID

	declare @chvCode varchar(8)
	EXEC [dbo].[PrWeb_UserAgent_GenderQRCode]
		@chvCode = @chvCode OUTPUT
	WHILE EXISTS(select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where [QRCode] = @chvCode)
	BEGIN
		EXEC [dbo].[PrWeb_UserAgent_GenderQRCode]
			@chvCode = @chvCode OUTPUT
	END

	/*  取得渠道对应QR URL  */
	DECLARE @chvDomain varchar(200)
	declare @domainid int
	select	@domainid = [ID],
			@chvDomain=[Domain]
	from [dbo].[DomainSetting] with (nolock)
	where
		platformid = @intplatformid
		and areaid = @intareaid


	/*  取前台是否有开放  */
	declare @string varchar(max)
	select	top 1 @string = areaconfig from CenterDB.area.area where platformid = @intplatformid and AreaID = @intareaid

	declare @uiclub2agentmode1 varchar(10)
	declare @uiclub2agentmode2 varchar(10)
	declare @uiclub2agentmode3 varchar(10)
	declare @uiclub2agentmode4 varchar(10)
	declare @uiclub2agentmode5 varchar(10)
	declare @uiclub2agentmode6 varchar(10)
	
	declare @temp_json table (element_id int,sequenceno int,parent_id int,objectid int,name varchar(1024),value varchar(100),valuetype varchar(30))
	insert into @temp_json
	select *
	from [dbo].[fn_parseJSON](@string)

	select	@uiclub2agentmode1 = sum(case when name = 'UIClub2AgentMode1' and value = 'true' then 1 else 0 end),
			@uiclub2agentmode2 = sum(case when name = 'UIClub2AgentMode2' and value = 'true' then 1 else 0 end),
			@uiclub2agentmode3 = sum(case when name = 'UIClub2AgentMode3' and value = 'true' then 1 else 0 end),
			@uiclub2agentmode4 = sum(case when name = 'UIClub2AgentMode4' and value = 'true' then 1 else 0 end),
			@uiclub2agentmode5 = sum(case when name = 'UIClub2AgentMode5' and value = 'true' then 1 else 0 end),
			@uiclub2agentmode6 = sum(case when name = 'UIClub2AgentMode6' and value = 'true' then 1 else 0 end)
	from @temp_json
	where
		name in ('UIClub2AgentMode1','UIClub2AgentMode2','UIClub2AgentMode3','UIClub2AgentMode4','UIClub2AgentMode5','UIClub2AgentMode6')

	SET @chvQRUrl = @chvDomain + '?a=' + @chvCode


	--if exists (select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where userid = @intUserID)
	--begin
	--	set @chvMsg	= '操作错误 : 使用者已有QR CODE'
	--	return -1
	--end

	if exists (select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where [QRCode] = @chvQRUrl)
	begin
		set @chvMsg	= '操作错误 : QR CODE已被使用'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[UserAgentQRCode]
			(	userid,qrcode,[QRURL],status,DomainID,Mode1,Mode2,Mode3,Mode4,Mode5,Mode6,nickname,[password],createtime	)
		values (@intUserID,@chvCode,@chvQRUrl,@status,@domainid,@uiclub2agentmode1,@uiclub2agentmode2,@uiclub2agentmode3,@uiclub2agentmode4,@uiclub2agentmode5,@uiclub2agentmode6,@temp_getnickname,@password,getdate())

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 新增QR code错误'
			return -1
		end
	end

	if(@uiclub2agentmode1 = 1)
	--if(1 = 1)
	begin
		if not exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[UserAgentInfo]
				([UserID],[UserLevel],[SumTaxTotal],[SumTaxBonus],[SumTaxConvert],[SumTaxPerformed],[Createtime],platformid,areaid,status,unwithdrawtotal,nickname,[password])
			values
				(@intUserID,@userlevel,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,getdate(),@intplatformid,@intareaid,@status,@defaultvalues,@temp_getnickname,@password)

			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 新增全民代理用户错误'
				return -1
			end
		end

		if not exists (select top 1 1 from [dbo].[UserAgentUserRate] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[UserAgentUserRate] ([UserID],[UserLevel],[Stage1],[Stage2],[Stage3])
			select	@intUserID,[Level],[Stage1],[Stage2],[Stage3]
			from [dbo].[UserAgentSystemLevelRate] with (nolock)

			if(@@rowcount = 0)
			begin
				set @chvMsg	= '操作错误 : 预设比例失败'
				return -1
			end
		end
	end

	
	if(@uiclub2agentmode3 = 1)
		and exists (select top 1 1 from [dbo].[DomainSetting] with (nolock) where platformid = @intplatformid)
		and not exists (select top 1 1 from [dbo].[Split_UserInfo] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[Split_UserInfo]
			([UserID],StageLevel,[CreateTime],[Relations],[Status],[PlatformID],[AreaID],[NickName],[Password],Permission)
		values
			(@intUserID,1,getdate(),cast(@intUserID as varchar),1,@intplatformid,@intareaid,@temp_getnickname,@password,1)
	
		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 新增列变用户错误'
			return -1
		end

		insert into [dbo].[Split_Pool]
			([UserID],[PoolAmount],[WithDrawAmount],[SuccessSplitAmount],[CreateTime])
		values
			(@intUserID,0,0,0,getdate())

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 新增列变用户错误'
			return -1
		end

		insert into [dbo].[Split_RecruitSuccess]
			([UserID],[RecruitCountB],[RecruitCountC],[RecruitCountD],[SuccessCountB],[SuccessCountC],[SuccessCountD])
		values
			(@intUserID,0,0,0,0,0,0)

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 新增列变用户错误'
			return -1
		end

		insert into [dbo].[Split_UserMission]
			([UserID],[MobileBind],[AlipayBind],[UnionPayBind],[ChargeBind],[ConvertBindDate])
		values
			(@intUserID,0,0,0,0,0)

		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 新增列变用户错误'
			return -1
		end
	end


	if(@uiclub2agentmode4 = 1)
		and exists (select top 1 1 from [dbo].[DomainSetting] with (nolock) where platformid = @intplatformid)
		and not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		insert into [dbo].[NetProfitAgent_UserInfo]
			([UserID],StageLevel,UserLevel,[SumSubUser],[SumSubAgent],[WithDrawDayLimit],[SumChargeAmount],[SumWithDrawAmount],[SumSubWithDrawAmount],[Status],[CreateTime],
			[PlatformID],[AreaID],[NickName],[Password],Permission,Relations)
		values
			(@intUserID,1,1,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,1,getdate(),
			@intplatformid,@intareaid,@temp_getnickname,@password,1,cast(@intUserID as varchar))
	
		if(@@rowcount <> 1)
		begin
			set @chvMsg	= '操作错误 : 新增净利润用户错误'
			return -1
		end
	end

	if(@uiclub2agentmode5 = 1)
		and exists (select top 1 1 from [dbo].[DomainSetting] with (nolock) where platformid = @intplatformid)
		and not exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID)
	begin
		if not exists (select top 1 1 from [dbo].[Unlimit_UserInfo] where userid = @intUserID)
		begin
			insert into [dbo].[Unlimit_UserInfo]
				([UserID],StageLevel,UserLevel,[SumSubUser],[SumSubAgent],[WithDrawDayLimit],[SumChargeAmount],[SumWithDrawAmount],[SumSubWithDrawAmount],[Status],[CreateTime],
				[PlatformID],[AreaID],[NickName],[Password],Permission,Relations)
			values
				(	@intUserID,1,1,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,@defaultvalues,1,getdate(),
				@intplatformid,@intareaid,@temp_getnickname,@password,1,cast(@intUserID as varchar))
	
			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 新增无限级用户错误'
				return -1
			end
		end

		if not exists (select * from [dbo].[Unlimit_Pool] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[Unlimit_Pool] ([UserID],[PoolAmount],[WithDrawAmount],[Rate],[CreateTime])
			values (@intUserID,0,0,0,getdate())

			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 新增无限级用户错误'
				return -1
			end
		end

		if not exists (select * from [dbo].[Unlimit_Promotion] with (nolock) where userid = @intUserID)
		begin
			insert into [dbo].[Unlimit_Promotion] ([UserID],[SubUser],[SubDirectUser],[NewUser],[DAU])
			values (@intUserID,0,0,0,0)

			if(@@rowcount <> 1)
			begin
				set @chvMsg	= '操作错误 : 新增无限级用户错误'
				return -1
			end
		end
	end

	set @chvMsg	= '操作完成 : 新增用户完成'
	return 1
END












