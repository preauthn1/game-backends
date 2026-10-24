
-- =============================================
-- Author:  20190325
-- Create date: jason
-- Description:	<更新QZNN搶庄牛牛獎池>
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_UpdateQZNN_SystemPool]
(
	@intFlagID			int = 0,			-- 暫時不分區預設0
	@lngUpdateAmount	bigint			--变化数量
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @LogDate DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))

	DECLARE @DefaultAmount BIGINT = 5000000	--只有第一次新增時才使用預設使用50000人民幣

	UPDATE [Game].[QZNN_SystemPool]
	SET [Amount] = [Amount] + @lngUpdateAmount
	WHERE
		[FlagID] = @intFlagID

	IF(@@ROWCOUNT <> 1)
	BEGIN
		INSERT INTO [Game].[QZNN_SystemPool]
		VALUES (@intFlagID,@DefaultAmount)
	END

	UPDATE [Stat].[QZNN_PoolAmountDaily]
	SET [Amount] = [Amount] + @lngUpdateAmount
	WHERE
		[LogDate] = @LogDate

	IF(@@ROWCOUNT <> 1)
	BEGIN
		INSERT INTO [Stat].[QZNN_PoolAmountDaily]
		VALUES (@LogDate,@lngUpdateAmount)
	END

	SELECT	[Amount]
	FROM [Game].[QZNN_SystemPool] WITH (NOLOCK)
	WHERE
		[FlagID] = @intFlagID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_UpdateQZNN_SystemPool] TO [GameUser]
    AS [dbo];

