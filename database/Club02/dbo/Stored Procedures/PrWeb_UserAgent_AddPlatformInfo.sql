-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 后台 admin新增Platform 
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AddPlatformInfo]
(
	@intAdminID		int,
	@intPlatformID	int,
	@chvDomain		varchar(250)
)
AS
BEGIN

	set nocount on

	if(@intAdminID <> 1)
	begin
		return 1
	end
	
	declare @platformname	varchar(50)

	select @platformname = [Name]
	from CenterDB.[Area].[Platform] with (nolock)
	where
		platformid = @intPlatformID

	declare @temp_areaid	table (rowno int,PlatformID int,PlatformName varchar(50),Areaid int,Domain varchar(250))

	insert into @temp_areaid
	select	row_number() over (order by areaid asc) as rowno,
			@intPlatformID,
			B.Name,
			areaid,
			@chvDomain
	from CenterDB.area.area as a with (nolock)
		inner join centerdb.area.[platform] as b on a.PlatformID = b.PlatformID
	where
		a.platformid = @intPlatformID

	merge [dbo].[DomainSetting] AS a
	using
	(
		select PlatformID,PlatformName,Areaid,Domain from @temp_areaid
	) as b
	on (a.platformid = b.platformid and a.areaid = b.areaid)
	when matched then
		update set a.[Domain] = B.Domain
	when not matched then
		insert ([PlatformID],[AreaID],[Domain],[Status],[CreateTime],[PlatformName])
		values (b.[PlatformID],b.Areaid,b.Domain,1,getdate(),b.PlatformName);

	if not exists (select top 1 1 from [dbo].[AgentPlatformRelations] with (nolock) where adminid = @intAdminID and platformid = @intPlatformID)
	begin
		insert into [dbo].[AgentPlatformRelations] (adminid,platformid) values (@intAdminID,@intPlatformID)
	end

	return 1
END







