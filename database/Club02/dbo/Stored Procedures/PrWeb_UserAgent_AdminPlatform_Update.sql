-- =============================================
-- Author:  jason
-- Create date: 2019/04/23
-- Description: 后台 提供admin更新平台权限
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AdminPlatform_Update]
(
	@intAdminID	int,	--管理者
	@intUserID	int,	--总代身份
	@chvPlatformID	varchar(max)
)
AS
BEGIN

	set nocount on

	
	declare @temp_platform table (value int)
	declare @temp_deleteplatform table (id int)

	insert into @temp_platform
	select	cast(value as int)
	from [dbo].[fn_Split](@chvPlatformID,',')

	delete from [dbo].[AgentPlatformRelations]
	output deleted.PlatformID into @temp_deleteplatform
	where
		adminid = @intUserID

	declare @deleted_platform varchar(max)
	select	@deleted_platform = Name
	from (
			SELECT distinct STUFF(  (
					SELECT ',' + cast(platformid as varchar) FROM [dbo].[AgentPlatformRelations] as a WHERE a.adminid = b.adminid
					FOR XML PATH('')
					  ), 1, 1, '') AS NAME
			FROM [dbo].[AgentPlatformRelations] as B
			where
				b.adminID = @intUserID
		) AS A


	insert into [dbo].[AgentPlatformRelations]
	select	@intUserID,value
	from @temp_platform

	insert into [dbo].[Log_SystemAdminChangeUserStatusLog]
		([AdminID],[UserID],[CreateTime],[platformStr],[BF_platformStr])
	values
		(@intAdminID,@intUserID,getdate(),@chvPlatformID,@deleted_platform)



	return 1
END







