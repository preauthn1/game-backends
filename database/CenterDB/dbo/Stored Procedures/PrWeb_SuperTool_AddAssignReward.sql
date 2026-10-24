
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	建立指定局開獎資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_AddAssignReward]
(
	@intGameID int,
	@intFlag int,
	@intTableNo int,
	@chvSetting varchar(Max),
	@chvOperator varchar(50)
)
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @intLogID int

	
	--把之前指定开奖都取消
	Update  [Game].[SuperTool_AssignReward] set Status=0 where GameID=@intGameID and Status=1 and FlagID=@intFlag and TableNo=@intTableNo

	--status 0 取消  1 啟用 2 暫停
	INSERT INTO [Game].[SuperTool_AssignReward](GameID,FlagID,TableNo,Status, Operator,Result)
	VALUES(@intGameID, @intFlag, @intTableNo, 1, @chvOperator, '')
	SET @intLogID = SCOPE_IDENTITY()

	--status 0 未完成, 1 己取用, 2 己完成, 3指定開獎
	INSERT into [Game].[SuperTool_AssignRewardDetail](LogID, Description, Idx, Status, Reward)
	SELECT @intLogID as LogID, Value as Description, id -1 as idx, 0 as Status, '' as Reward from dbo.fn_Split(@chvSetting, ';')

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_AddAssignReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_AddAssignReward] TO [xbuser]
    AS [dbo];

