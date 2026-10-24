
-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取得 "指定特定局開獎" 內容
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_SuperTool_GetAssignRewardDetail]
(
	@intGameID int,
	@intFlagID	int,
	@intTableNo int
)
AS
BEGIN

	SET NOCOUNT ON
	Declare @intLogID int = 0
	Declare @intIdx int = 0

	--Status 0 取消 1 啟用 2 暫停
	Select Top 1 @intLogID = LogID from [Game].[SuperTool_AssignReward] with (nolock) 
	where GameID=@intGameID and FlagID=@intFlagID and TableNo=@intTableNo and Status=1
	Order by CreateTime desc

	--取得還未完成的第一筆
	--status: 0 未完成, 1 己取用, 2 己完成, 3指定開獎
	Select Top 1 @intIdx=Idx from [Game].[SuperTool_AssignRewardDetail] with (nolock) where status = 0 and LogID = @intLogID order by idx asc
	IF @@ROWCOUNT > 0
	BEGIN

		Update [Game].[SuperTool_AssignRewardDetail] set Status = 1 where Idx=@intIdx and LogID = @intLogID

		--取得內容
		Select 
		A.LogID, A.Idx, A.Description, ISNULL(A.Reward,'') as Reward, A.Status,
		B.GameID, B.FlagID, B.TableNo, B.Complete, ISNULL(B.Result,'') as Result, B.Operator
		from [Game].[SuperTool_AssignRewardDetail] A with (nolock)
		left join [Game].[SuperTool_AssignReward] B with (nolock)
		on A.LogID = B.LogID 
		Where A.Idx=@intIdx and A.LogID = @intLogID
		RETURN 1
	END

	
	RETURN 0
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_GetAssignRewardDetail] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_SuperTool_GetAssignRewardDetail] TO [xbuser]
    AS [dbo];

