-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 確認上級代理有幾種模式 依序新增
-- Changelog:
--	2019/6/18	CCC		取消判斷後台設定是否有開啟
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_CheckAgentMode]
(
	@intUserID		int,				--用户 ID
	@chvInviteCode	VARCHAR(20) = null,		--邀請碼
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @platformid int
	declare @areaid int
	declare @string varchar(max)
	
	select	@platformid = b.platformid,
			@areaid = a.areaid,
			@string = areaconfig
	from CenterDB.[Users].[UserInfo] as a with (nolock)
		inner join CenterDB.[Area].[Area] as b with (nolock) on a.areaid = b.areaid
	where
		userid = @intUserID


	/*  取前台是否有顯示狀態  */
	declare @uiclub2agentmode1 tinyint
	declare @uiclub2agentmode2 tinyint
	declare @uiclub2agentmode3 tinyint
	declare @uiclub2agentmode4 tinyint
	declare @uiclub2agentmode5 tinyint
	declare @uiclub2agentmode6 tinyint
	
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

	/*  取後台設定是否有開啟狀態  */
	declare @agentmode1 tinyint
	declare @agentmode2 tinyint
	declare @agentmode3 tinyint
	declare @agentmode4 tinyint
	declare @agentmode5 tinyint
	declare @agentmode6 tinyint
	select	@agentmode1 = [Mode1],
			@agentmode2 = [Mode2],
			@agentmode3 = [Mode3],
			@agentmode4 = [Mode4],
			@agentmode5 = [Mode5],
			@agentmode6 = [Mode6]
	from [dbo].[AgentModeConfig] with (nolock)
	where
		platformid = @platformid
		and areaid = @areaid

	set @agentmode1 = isnull(@agentmode1,0)
	set @agentmode2 = isnull(@agentmode2,0)
	set @agentmode3 = isnull(@agentmode3,0)
	set @agentmode4 = isnull(@agentmode4,0)
	set @agentmode5 = isnull(@agentmode5,0)
	set @agentmode6 = isnull(@agentmode6,0)

	/*  取用戶是否已具備啟用狀態  */
	declare @usermode1 tinyint
	declare @usermode2 tinyint
	declare @usermode3 tinyint
	declare @usermode4 tinyint
	declare @usermode5 tinyint
	declare @usermode6 tinyint
	select	@usermode1 = [Mode1],
			@usermode2 = [Mode2],
			@usermode3 = [Mode3],
			@usermode4 = [Mode4],
			@usermode5 = [Mode5],
			@usermode6 = [Mode6]
	from [dbo].[UserAgentQRCode] with (nolock)
	where
		userid = @intUserID

	set @usermode1 = isnull(@usermode1,0)
	set @usermode2 = isnull(@usermode2,0)
	set @usermode3 = isnull(@usermode3,0)
	set @usermode4 = isnull(@usermode4,0)
	set @usermode5 = isnull(@usermode5,0)
	set @usermode6 = isnull(@usermode6,0)


	/*  先取邀請碼對應用戶狀態  */
	declare @userlevel int = 1
	declare @defaultvalues int = 0
	declare @status tinyint = 1
	declare @upperUserStatus tinyint
	declare @invitecode varchar(20)
	declare @intUpperUserID int

	--A 抽水 全民代理
	if((@uiclub2agentmode1 + @usermode1) = 2)
	begin
		EXEC [dbo].[PrWeb_UserAgent_AddUserRelation]
			@intUserID = @intUserID,			--用户 ID
			@chvInviteCode= @chvInviteCode,		--邀請碼
			@chvMsg = ''
	end

	----B 流水 推廣代理
	--if((@uiclub2agentmode2 + @usermode2) = 2)
	--begin
	--	select 1
	--end

	--C 裂變
	if((@uiclub2agentmode3 + @usermode3) = 2)
	begin
		EXEC [dbo].[PrWeb_Split_AddUserRelation]
			@intUserID = @intUserID,			--用户 ID
			@chvInviteCode= @chvInviteCode,		--邀請碼
			@chvMsg = ''
	end

	--D 淨利潤
	if((@uiclub2agentmode4 + @usermode4) = 2)
	begin
		EXEC [dbo].[PrWeb_NetProfitAgent_AddUserRelation]
			@intUserID = @intUserID,			--用户 ID
			@chvInviteCode= @chvInviteCode,		--邀請碼
			@chvMsg = ''
	end

	--E 無限級
	if((@uiclub2agentmode5 + @usermode5) = 2)
	begin
		EXEC [dbo].[PrWeb_Unlimit_AddUserRelation]
			@intUserID = @intUserID,			--用户 ID
			@chvInviteCode= @chvInviteCode,		--邀請碼
			@chvMsg = ''
	end

	----F 禮碼
	--if((@uiclub2agentmode6 + @usermode6) = 2)
	--begin
	--	select 1
	--end
END





