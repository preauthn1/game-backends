
-- =============================================
-- Author:  
-- Create date: 20190430
-- Description: 无极限-奖励说明
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetRewardTips]

AS
BEGIN

	SET NOCOUNT ON

	declare @chvSelf varchar(500) = '直销业绩150万<BR>200 X 150<BR>反佣30000'
	declare @chvAgentA varchar(500)  = '推广业绩300万<BR>级差(200-120) X 300<BR>反佣24000'
	declare @chvAgentB varchar(500)  = '推广业绩100万<BR>级差(200-110) X 100<BR>反佣9000'
	declare @chvAgentC varchar(500)  = '推广业绩401万<BR>级差(200-130) X 401<BR>反佣28070'
	declare @chvAgentD varchar(500)  = '推广业绩50万<BR>级差(200-90) X 50<BR>反佣5500'
	declare @chvSummary varchar(500) = '本周反佣总合计：30000+24000+9000+28070+5500=96570元'


	declare @temp_unlimitlevel table (lv int,name varchar(30),minamount bigint,maxamount bigint,BonusRate varchar(30))

	insert into @temp_unlimitlevel
	EXEC [Club02].[dbo].[PrPs_Unlimit_GetRewardTips]

	select 
	@chvSelf as [Self],
	@chvAgentA as AgentA,
	@chvAgentB as AgentB,
	@chvAgentC as AgentC,
	@chvAgentD as AgentD,
	@chvSummary as AgentBonus


	select	lv,
			name,
			minamount,
			maxamount,
			BonusRate
	from @temp_unlimitlevel
		

	return 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Unlimit_GetRewardTips] TO [GameUser]
    AS [dbo];

