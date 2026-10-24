






-- =============================================
-- Author:  harry
-- Create date: 2018/08/21
-- Description:	查询申请代理总数
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentApplyCount_Get]
	@AgentUID		uniqueidentifier
AS
BEGIN
	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1,2,3,4,級代理 99:一般玩家  
	DECLARE @intCount int --
	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有申请清单
	BEGIN
		SELECT @intCount = Count(ApplyID) FROM [Club].[dbo].[AgentApply]
		WHERE Status = 0
		RETURN @intCount
	END
	ELSE
	BEGIN
		SELECT @intCount = Count(ApplyID) FROM [Club].[dbo].[AgentApply]
		WHERE ParentID = @AgentUID AND Status = 0
		RETURN @intCount
	END
END

