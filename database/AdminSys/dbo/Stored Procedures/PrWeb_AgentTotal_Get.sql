


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<总代理>

-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentTotal_Get]
(
	@intAgentID				INT					--代理ID
)
AS
BEGIN
	SELECT	*
	FROM agent_total AS A WITH (NOLOCK)
	WHERE id = @intAgentID
END


