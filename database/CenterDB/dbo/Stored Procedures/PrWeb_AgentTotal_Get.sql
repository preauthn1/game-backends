
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<取得代理支付总代理資料>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentTotal_Get]
(
	@intAdminId			INT,
	@intAgentID			INT					--代理ID
)
AS
BEGIN

	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	Declare @intPlatformID int	
	INSERT INTO @Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminId

	
	--如果是聯運轰达娱乐的財務
	
	if exists(select 1 from AgentHD.dbo.agent_total AS A WITH (NOLOCK) WHERE id = @intAgentID)
	--if exists(select 1 from @Temp_PlatformID where PlatformID=119)
	begin

		SELECT	*, '轰达娱乐' as name
		FROM AgentHD.dbo.agent_total AS A WITH (NOLOCK)
		WHERE id = @intAgentID

	end
	else if exists(select 1 from AgentGD.dbo.agent_total AS A WITH (NOLOCK) WHERE id = @intAgentID)
	begin
		SELECT	*, '金牌' as name
		FROM AgentGD.dbo.agent_total AS A WITH (NOLOCK)
		WHERE id = @intAgentID
	end
	else
	--如果是聯運77或其他平台的財務
	begin
	
	

		SELECT	*,'方舟' as name
		FROM AdminSys.dbo.agent_total AS A WITH (NOLOCK)
		WHERE id = @intAgentID

	end
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AgentTotal_Get] TO [GameUser]
    AS [dbo];

