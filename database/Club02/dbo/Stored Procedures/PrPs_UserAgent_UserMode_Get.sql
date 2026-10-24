-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 取得已开的模式
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_UserMode_Get]
(
	@intUserID		int,				--用户 ID
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	/*  取用户平台ID  */
	declare @intplatformid int
	select	@intplatformid = platformid
	from CenterDB.Users.UserInfo as a with (nolock)
		inner join CenterDB.Area.area as b with (nolock) on a.areaid = b.areaid
	where
		a.userid = @intUserID

	/*  取前台是否有开放  */
	declare @string varchar(max)
	select	top 1 @string = areaconfig from CenterDB.area.area where platformid = @intplatformid

	declare @uiclub2agentmode1 varchar(10)
	declare @uiclub2agentmode2 varchar(10)
	declare @uiclub2agentmode3 varchar(10)
	declare @uiclub2agentmode4 varchar(10)
	declare @uiclub2agentmode5 varchar(10)
	declare @uiclub2agentmode6 varchar(10)


	if(@intplatformid is null)
	begin
		/*  回传资料  */
		declare @temp_user table (userid int,Mode1 int,Mode2 int,Mode3 int,Mode4 int,Mode5 int,Mode6 int)
		select	* from @temp_user
		set @chvMsg	= '操作完成 : 完成'
		return 1
	end
	
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

	set @uiclub2agentmode1 = isnull(@uiclub2agentmode1,0)
	set @uiclub2agentmode2 = isnull(@uiclub2agentmode2,0)
	set @uiclub2agentmode3 = isnull(@uiclub2agentmode3,0)
	set @uiclub2agentmode4 = isnull(@uiclub2agentmode4,0)
	set @uiclub2agentmode5 = isnull(@uiclub2agentmode5,0)
	set @uiclub2agentmode6 = isnull(@uiclub2agentmode6,0)
	
	declare @permission1 int
	declare @permission4 int

	if exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where userid = @intUserID and permission = 0)
	begin
		set @permission1 = 0
	end
	
	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID and permission = 0)
	begin
		set @permission4 = 0
	end
	
	/*  回传资料  */
	select	userid,
			[Mode1] = case	when @permission1 = 0 then 0
							when [Mode1] = 1 then 1 else @uiclub2agentmode1 end,
			[Mode2] = case when [Mode2] = 1 then 1 else @uiclub2agentmode2 end,
			[Mode3] = case when [Mode3] = 1 then 1 else @uiclub2agentmode3 end,
			[Mode4] = case when @permission4 = 0 then 0
							when [Mode4] = 1 then 1 else @uiclub2agentmode4 end,
			[Mode5] = case when [Mode5] = 1 then 1 else @uiclub2agentmode5 end,
			[Mode6] = case when [Mode6] = 1 then 1 else @uiclub2agentmode6 end
	from [dbo].[UserAgentQRCode] with (nolock)
	where
		userid = @intUserID


	set @chvMsg	= '操作完成 : 完成'
	return 1
END








