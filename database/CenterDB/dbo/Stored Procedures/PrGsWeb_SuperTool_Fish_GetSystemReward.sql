-- =============================================
-- Author:  alan
-- Create date: 20180908
-- Description:	取得 捕魚系統放分 清單
-- =============================================
CREATE PROCEDURE [dbo].[PrGsWeb_SuperTool_Fish_GetSystemReward]
(
	@intFlagID int = null,
	@intStatus tinyint=null --是否進行中 0無 1正在放分
)
AS
BEGIN

	SET NOCOUNT ON
	
	SELECT S.*, F.Name as FishName from [Game].[SuperTool_Fish_SystemReward] S with (nolock)
	left join [Game].[SuperTool_FishItem] F with (nolock)
	ON S.FishID = F.FishId
	Where
	(@intFlagID IS NULL OR flagID=@intFlagID)
	AND (@intStatus IS NULL OR Status=@intStatus)
	Order by FlagID asc,FishID asc, Status desc

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGsWeb_SuperTool_Fish_GetSystemReward] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGsWeb_SuperTool_Fish_GetSystemReward] TO [xbuser]
    AS [dbo];

