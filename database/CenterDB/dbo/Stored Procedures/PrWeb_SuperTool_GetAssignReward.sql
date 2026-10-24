
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取得 "指定特定局開獎" 結果
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_GetAssignReward]
(
	@intGameID int
)
AS
BEGIN

	SET NOCOUNT ON

	
	--SELECT GameID,FlagID,TableNo,
	--Complete, --完成度
	--Result, --开奖内容	
	--Status, --status 0 取消  1 啟用 2 暫停
	--Operator,CreateTime from Game.SuperTool_AssignReward with (nolock)
	
	--WHERE GameID = @intGameID
	--AND CreateTime >= DateAdd(day, -30, getdate())
	--ORDER BY CreateTime desc
	

	
	SELECT DISTINCT DT2.LogID, 
		SUBSTRING(
			(
				SELECT ' | ' + DT1.Reward   AS [text()]
				FROM [Game].[SuperTool_AssignRewardDetail] DT1
				WHERE DT1.LogID = DT2.LogID 
				AND len(DT1.Reward) > 0
				ORDER BY DT1.LogID,DT1.Idx
				FOR XML PATH ('')
			), 2, 1000) [Detail]

	INTO #Detail
	FROM [Game].[SuperTool_AssignRewardDetail] DT2

	select m.LogID, m.GameID, m.FlagID, m.TableNo, m.Complete, m.Status, m.Operator, m.CreateTime, d.Detail as Result from #Detail as d
	left join  [Game].[SuperTool_AssignReward] as m
	on d.LogID = m.LogID
	WHERE m.CreateTime >= DateAdd(day, -30, getdate())
	AND m.GameID = @intGameID
	ORDER BY m.CreateTime desc
	

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetAssignReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetAssignReward] TO [xbuser]
    AS [dbo];

