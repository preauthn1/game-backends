-- =============================================
-- Author:  Jason
-- Create date: 2018/06/25
-- Description: 總代轉帳紀錄
-- Changelog:

-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_GetTotalAgentTransferList]
(
	@intAdminId		INT = NULL,
	@intagentid		INT = NULL,
	@dtstattime		datetime = null,
	@dtendtime		datetime = null,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON


	declare @intRoleId int
	select @intRoleId=RoleId from AdminSys.Admin.UserRole with (nolock) where UserID=@intAdminId

	if @intRoleId = 5 --超级管理员
	begin
		set @intAdminId = null
	end
	

	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminId



	DECLARE @tmp_list TABLE 
	(manage_id int, id int,total_agent_id int,total_agent_name nvarchar(50), 
	[money] int,[old_money] int,[new_money] int,[created_at] datetime,ip varchar(50),
	area nvarchar(50),[device] nvarchar(50), [remark] nvarchar(50), [name] varchar(50) )

	--管理員
	if (@intRoleId = 5)
	begin
		insert into @tmp_list(manage_id, id,total_agent_id,total_agent_name,[money],[old_money],[new_money],[created_at],ip,area,[device],[remark],[name])
		select manage_id, id,total_agent_id,total_agent_name,[money],[old_money],[new_money],[created_at],ip,area, [device] ,[remark], [Name]
		from (

				select 
					a.manage_id, a.[id],a.[total_agent_id],total_agent_name = b.username
					,a.num as [money],a.[old_money],a.[new_money],a.[created_at],a.[ip],a.[area],a.[device],a.[remark]
					, '轰达娱乐' as [Name]
				FROM 
					[agentHD].[dbo].[agent_recharge_total] as a left join 
					[agentHD].[dbo].[agent_total] as b on a.total_agent_id = b.id
				where 
					(@intagentid is null or total_agent_id=@intagentid)
					and isnull(@dtstattime,getdate()-60) <= a.[created_at]
					and isnull(@dtendtime,getdate()) >= a.[created_at]

				union
				select 
					a.manage_id, a.[id],a.[total_agent_id],total_agent_name = b.username
					,a.num as [money],a.[old_money],a.[new_money],a.[created_at],a.[ip],a.[area],a.[device],a.[remark]
					, 'FZ' as [Name]
				FROM 
					[AdminSys].[dbo].[agent_recharge_total] as a left join 
					[AdminSys].[dbo].[agent_total] as b on a.total_agent_id = b.id
				where 
					(@intagentid is null or total_agent_id=@intagentid)
					and isnull(@dtstattime,getdate()-60) <= a.[created_at]
					and isnull(@dtendtime,getdate()) >= a.[created_at]

		) as a

	end
	--如果是聯運轰达娱乐的財務
	else if exists(select 1 from @Temp_PlatformID where PlatformID=119)
	begin
			insert into @tmp_list(manage_id, id,total_agent_id,total_agent_name,[money],[old_money],[new_money],[created_at],ip,area,[device],[remark],[name])
			select 
				a.manage_id,a.[id],a.[total_agent_id],total_agent_name = b.username
				,a.num as [money],a.[old_money],a.[new_money],a.[created_at],a.[ip],a.[area],a.[device],a.[remark]
				, '轰达娱乐' as [Name]
			FROM 
				[agentHD].[dbo].[agent_recharge_total] as a left join 
				[agentHD].[dbo].[agent_total] as b on a.total_agent_id = b.id
			where 
				(@intagentid is null or total_agent_id=@intagentid)
				and isnull(@dtstattime,getdate()-60) <= a.[created_at]
				and isnull(@dtendtime,getdate()) >= a.[created_at]
				and (@intAdminId is null or manage_id=@intAdminId )

	end
	else
	--FZ/77
	begin
			insert into @tmp_list(manage_id, id,total_agent_id,total_agent_name,[money],[old_money],[new_money],[created_at],ip,area,[device],[remark],[name])
			select 
				a.manage_id,a.[id],a.[total_agent_id],total_agent_name = b.username
				,a.num as [money],a.[old_money],a.[new_money],a.[created_at],a.[ip],a.[area],a.[device],a.[remark]
				, 'FZ' as [Name]
			FROM 
				[AdminSys].[dbo].[agent_recharge_total] as a left join 
				[AdminSys].[dbo].[agent_total] as b on a.total_agent_id = b.id
			where 
				(@intagentid is null or total_agent_id=@intagentid)
				and isnull(@dtstattime,getdate()-60) <= a.[created_at]
				and isnull(@dtendtime,getdate()) >= a.[created_at]
				and (@intAdminId is null or manage_id=@intAdminId )
	end


	
	SELECT	@intPageCount = COUNT(1)
	FROM 
	@tmp_list



	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY [created_at] DESC) AS rowNum,
		*
		FROM 
			@tmp_list

	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex



END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_GetTotalAgentTransferList] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_GetTotalAgentTransferList] TO [xbuser]
    AS [dbo];

