
-- =============================================
-- Author:  
-- Create date: 20190604
-- Description: 无极限-奖励说明列表
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetRewardTipsList]
(
	@intPlatformID	INT
)
AS
BEGIN

	SET NOCOUNT ON

	SELECT	PlatformID,
			[Level],
			[LevelName],
			[MinAmount],
			[MaxAmount],
			[BonusAmount]
	FROM [dbo].[Unlimit_SystemRate] WITH (NOLOCK)
	WHERE
		PlatformID = @intPlatformID

	return 1
END


