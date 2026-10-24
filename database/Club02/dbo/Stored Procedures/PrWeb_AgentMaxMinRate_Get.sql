









-- =============================================
-- Author:  harry
-- Create date: 2018/10/06
-- Description:	查询分成上下限
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentMaxMinRate_Get]
	@OperateUID		uniqueidentifier,
	@AgentUID		uniqueidentifier
AS
BEGIN
	DECLARE @intMaxRate int --
	DECLARE @intMinRate int --
	DECLARE @tnyUserType tinyint --玩家身分 100: 系統管理者 0:渠道代理 1:一級代理  2:二級代理 99:一般玩家  
	DECLARE @tnyOperateClass tinyint --玩家身分 100: 系統管理者 0:渠道代理 1:一級代理  2:二級代理 99:一般玩家  
	DECLARE @intOperateRate int --

	--查询阶级
	SELECT @tnyUserType = Class From dbo.AgentUsers With (nolock) Where ID = @AgentUID

	--查询操作者阶级
	SELECT @tnyOperateClass = Class, @intOperateRate = G.DistributeRate From dbo.AgentUsers U With (nolock) 
	LEFT JOIN GrandConfig G ON G.AgentID = U.ID
	Where U.ID = @OperateUID

	IF @tnyOperateClass = 0
		BEGIN
			SELECT TOP 1 @intMaxRate = [DistributeRate]
			FROM [Club].[dbo].[AgentRateRange] WHERE ID = @OperateUID
		END
	ELSE
		BEGIN
			SET @intMaxRate = @intOperateRate
		END

	SELECT MinRate = MAX (G.DistributeRate), MaxRate = @intMaxRate FROM GrandConfig G
	JOIN (
		SELECT * FROM fn_RecursiveDown(@AgentUID)
		WHERE Class = @tnyUserType + 1
	)  A ON  A.Id = G.AgentID

END




