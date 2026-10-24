


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<总代理转账>

-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentTotal_Transfer]
(
	@intOperatorID			INT,					--操作者ID
	@chvOperatorName		VARCHAR(128),			--操作者名称
	@intAgentID				INT,					--代理ID
	@lngAmount				BIGINT,					--修改数量
	@chvIP					VARCHAR(128),			--IP
	@chvErrMsg				VARCHAR(128) OUTPUT		--返回消息
)
AS
BEGIN
	SET NOCOUNT ON

	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	Declare @intPlatformID int
	INSERT INTO @Temp_PlatformID Select PlatformId From CenterDB.Area.PlatformAdminUser where AdminID=@intOperatorID

	
	--如果是聯運轰达娱乐的財務
	--if exists(select 1 from @Temp_PlatformID where PlatformID=119)
	if exists(select 1 from AgentHD.dbo.agent_total WITH (NOLOCK) WHERE id = @intAgentID)
	begin

		EXEC [AgentHD].[dbo].[PrWeb_AgentTotal_Transfer]
			@intOperatorID = @intOperatorID,
			@chvOperatorName = @chvOperatorName,
			@intAgentID = @intAgentID,
			@lngAmount = @lngAmount,
			@chvIP = @chvIP,
			@chvErrMsg = @chvErrMsg OUTPUT

	end
	else if exists(select 1 from AgentGD.dbo.agent_total WITH (NOLOCK) WHERE id = @intAgentID)
	begin
		EXEC [AgentGD].[dbo].[PrWeb_AgentTotal_Transfer]
			@intOperatorID = @intOperatorID,
			@chvOperatorName = @chvOperatorName,
			@intAgentID = @intAgentID,
			@lngAmount = @lngAmount,
			@chvIP = @chvIP,
			@chvErrMsg = @chvErrMsg OUTPUT
	end
	else
	--如果是聯運77或其他平台的財務
	begin

		EXEC [AdminSys].[dbo].[PrWeb_AgentTotal_Transfer]
			@intOperatorID = @intOperatorID,
			@chvOperatorName = @chvOperatorName,
			@intAgentID = @intAgentID,
			@lngAmount = @lngAmount,
			@chvIP = @chvIP,
			@chvErrMsg = @chvErrMsg OUTPUT

	end


END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AgentTotal_Transfer] TO [GameUser]
    AS [dbo];

