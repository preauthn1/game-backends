



-- =============================================
-- Author:  harry
-- Create date: 2018/10/9
-- Description:	查询注册总人数,今日人数
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentUseRregisteredCount_Get]
	@AgentUID		uniqueidentifier
AS
BEGIN	
	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1,2,3,4級代理 99:一般玩家  

	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有渠道代理
	BEGIN
		SELECT  
			SUM(CASE WHEN U.Class != 99 THEN 1 ELSE 0 END) AS AgentTotal,
			SUM(CASE WHEN U.Class != 99 AND DateDiff(dd,U.CreateTime,getdate())=0 THEN 1 ELSE 0 END) AS AgentToday,
			SUM(CASE WHEN U.Class = 99 THEN 1 ELSE 0 END) AS CustomerTotal,
			SUM(CASE WHEN U.Class = 99 AND DateDiff(dd,U.CreateTime,getdate())=0 THEN 1 ELSE 0 END) AS CustomerToday
		FROM AgentUsers U
	END
	ELSE
	BEGIN
		SELECT  
			SUM(CASE WHEN U.Class != 99 THEN 1 ELSE 0 END) AS AgentTotal,
			SUM(CASE WHEN U.Class != 99 AND DateDiff(dd,U.CreateTime,getdate())=0 THEN 1 ELSE 0 END) AS AgentToday,
			SUM(CASE WHEN U.Class = 99 THEN 1 ELSE 0 END) AS CustomerTotal,
			SUM(CASE WHEN U.Class = 99 AND DateDiff(dd,U.CreateTime,getdate())=0 THEN 1 ELSE 0 END) AS CustomerToday
		FROM AgentUsers U
		WHERE  U.ID in 
		(
			SELECT ID from [fn_RecursiveDown](@AgentUID)
			WHERE ID != @AgentUID
		) 
	END
END
