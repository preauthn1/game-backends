






-- =============================================
-- Author:  harry
-- Create date: 2018/08/21
-- Description:	查询代理下级总数
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentRecursiveDownCount_Get]
	@AgentUID		uniqueidentifier
AS
BEGIN
	DECLARE @intCount int --

	SELECT @intCount = Count(Id) from [fn_RecursiveDown](@AgentUID)
	WHERE ID != @AgentUID AND Class != 99

	RETURN @intCount
END

