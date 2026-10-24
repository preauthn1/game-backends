
-- =============================================
-- Author:  
-- Create date: 20190604
-- Description: 无极限-奖励说明
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetRewardTips]
--(
--	@intPlatformID	INT
--)
AS
BEGIN

	SET NOCOUNT ON

	SELECT	[Level],
			[LevelName],
			[MinAmount] = [MinAmount] / 100,
			[MaxAmount] = CASE WHEN [MaxAmount] = 9999999999999 THEN 0 ELSE [MaxAmount] / 100 END,
			[BonusAmount]
	FROM [dbo].[Unlimit_SystemRate] WITH (NOLOCK)
	--WHERE
	--	PlatformID = @intPlatformID

	return 1
END


