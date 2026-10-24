-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: 后台 新增管理者账户
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AddAgentUserPermission]
(
	@chvAdminAccount	varchar(32), --管理者账号
	@chvPassword		varchar(32),
	@chvEncrypt			varchar(10),
	@intAdminType		int = 2,
	@chvPlatformID		varchar(max) = null,
	@tnymode1			tinyint = null,
	@tnymode2			tinyint = null,
	@tnymode3			tinyint = null,
	@tnymode4			tinyint = null,
	@tnymode5			tinyint = null,
	@tnymode6			tinyint = null,
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	declare @temp_adminid table (id int)
	declare @adminid int

	insert into [dbo].[AgentAdminUser]
		([Account],[Password],[Encrypt],[admintype],[CreateTime],[Status],[Mode1],[Mode2],[Mode3],[Mode4],[Mode5],[Mode6])
	output inserted.id into @temp_adminid
	values
		(@chvAdminAccount,@chvPassword,@chvEncrypt,@intAdminType,getdate(),1,@tnymode1,@tnymode2,@tnymode3,@tnymode4,@tnymode5,@tnymode6)

	select @adminid = id from @temp_adminid

	declare @temp_platform table (platformID int)
	insert into @temp_platform
	select	value
	from [dbo].[fn_Split](@chvPlatformID,',')

	delete from [dbo].[AgentPlatformRelations] where adminid = @adminid

	insert into [dbo].[AgentPlatformRelations]
	select	@adminid,
			platformID
	from @temp_platform
	

	set @chvMsg	= '操作完成 : 新增代理管理者完成'
	return 1
END

