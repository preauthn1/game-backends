-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取充值代理列表>
-- changelog:
--		2018/6/7	alan	修改從代理系統取得代理資訊
--		2018/7/15	alan	新增支付寶代理資訊
--		2018/7/25	alan	增加使用者ID, 篩選代理
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetPayAgent]	
@intAreaID		int,		--渠道ID
@intUserID		int = null
AS
BEGIN

	Declare @intTotalCharge bigint
	Declare @intAdPlan bigint
	Declare @dtAdPlanTime datetime
	Declare @intCompany int
	SELECT @dtAdPlanTime=ISNULL(AdPlanTime,'2000-1-1'), @intAdPlan=AdPlan, @intTotalCharge=(SumCharge+SumAgentCharge) From [centerdb].[users].[UserInfo] WITH (NOLOCK) WHERE USERID = @intUserID

	declare @intPlatformID int
	select @intPlatformID=PlatformID from area.area with (nolock) where areaid = @intAreaID
	select @intCompany = Company from [area].[Platform] where PlatformID = @intPlatformID
	
	--轟達
	if @intCompany = 2
	begin
		
		--針對特定渠道

		--如果有设置方案 并且未超过24HR
		IF @intAdPlan > 0  and DateAdd(HOUR,24, @dtAdPlanTime) > getdate() 
		BEGIN
			SELECT TOP 8 Convert(varchar,qq) as QQ, weixin as WXNo, [name] as Name, Alipay, Jiuliao from [AgentHD].[dbo].[agent_user] as a WITH (NOLOCK)
			WHERE 
			[status] = 1 
			and clock=1 --有打卡上班者
			and [level] in (	
				--筛选方案
				select distinct agents from  [AgentHD].[dbo].[agent_plan] where id = @intAdPlan 
			)

			ORDER BY NEWID()

		END
		ELSE 
		BEGIN
			--未设置方案用充值金额筛选代理名单
			SELECT TOP 8 Convert(varchar,qq) as QQ, weixin as WXNo, [name] as Name, Alipay, Jiuliao from [AgentHD].[dbo].[agent_user] as a WITH (NOLOCK)
			WHERE 
			[status] = 1 
			and clock = 1 --有打卡上班者
			and [level] in (
				--筛选方案
				select  distinct agents from  [AgentHD].[dbo].[agent_plan] where @intTotalCharge/100 >= total_recharge
			)

			ORDER BY NEWID()
		END
		
	end	
	else if @intCompany = 3	--金牌
	begin
		--針對特定渠道

		--如果有设置方案 并且未超过24HR
		IF @intAdPlan > 0  and DateAdd(HOUR,24, @dtAdPlanTime) > getdate() 
		BEGIN
			SELECT TOP 8 Convert(varchar,qq) as QQ, weixin as WXNo, [name] as Name, Alipay, Jiuliao from [AgentGD].[dbo].[agent_user] as a WITH (NOLOCK)
			WHERE 
			[status] = 1 
			and clock=1 --有打卡上班者
			and [level] in (	
				--筛选方案
				select distinct agents from  [AgentGD].[dbo].[agent_plan] where id = @intAdPlan 
			)

			ORDER BY NEWID()

		END
		ELSE 
		BEGIN
			--未设置方案用充值金额筛选代理名单
			SELECT TOP 8 Convert(varchar,qq) as QQ, weixin as WXNo, [name] as Name, Alipay, Jiuliao from [AgentGD].[dbo].[agent_user] as a WITH (NOLOCK)
			WHERE 
			[status] = 1 
			and clock = 1 --有打卡上班者
			and [level] in (
				--筛选方案
				select  distinct agents from  [AgentGD].[dbo].[agent_plan] where @intTotalCharge/100 >= total_recharge
			)

			ORDER BY NEWID()
		END
	end
	--77跟方舟共用
	begin


		--如果有设置方案 并且未超过24HR
		IF @intAdPlan > 0  and DateAdd(HOUR,24, @dtAdPlanTime) > getdate() 
		BEGIN
			SELECT TOP 8 Convert(varchar,qq) as QQ, weixin as WXNo, [name] as Name, Alipay, Jiuliao from [adminsys].[dbo].[agent_user] as a WITH (NOLOCK)
			WHERE 
			[status] = 1 
			and clock=1 --有打卡上班者
			and [level] in (	
				--筛选方案
				select distinct agents from  [adminsys].[dbo].[agent_plan] where id = @intAdPlan 
			)
			
			ORDER BY NEWID()

		END
		ELSE 
		BEGIN
			--未设置方案用充值金额筛选代理名单
			SELECT TOP 8 Convert(varchar,qq) as QQ, weixin as WXNo, [name] as Name, Alipay, Jiuliao from [adminsys].[dbo].[agent_user] as a WITH (NOLOCK)
			WHERE 
			[status] = 1 
			and clock = 1 --有打卡上班者
			and [level] in (
				--筛选方案
				select  distinct agents from  [adminsys].[dbo].[agent_plan] where @intTotalCharge/100 >= total_recharge
			)
			
			ORDER BY NEWID()
		END


	end

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetPayAgent] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetPayAgent] TO [xbuser]
    AS [dbo];

