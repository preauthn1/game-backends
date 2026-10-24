
-- =============================================
-- Author:  20190325
-- Create date: jason
-- Description:	<讀取QZNN搶庄牛牛獎池>
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_GetQZNN_SystemPool]
(
	@intFlagID			int = 0			-- 暫時不分區預設0
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @LogDate DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))
	DECLARE @Amount BIGINT

	SELECT	@Amount = [Amount]
	FROM [Game].[QZNN_SystemPool] WITH (NOLOCK)
	WHERE
		[FlagID] = @intFlagID

	IF(@Amount IS NULL)
	BEGIN
		--伺服器無獎池記錄時 先墊入50000人民幣
		SET @Amount = 5000000
		INSERT INTO [Game].[QZNN_SystemPool]
		VALUES(@intFlagID,@Amount)

		INSERT INTO [Stat].[QZNN_PoolAmountDaily]
		VALUES (@LogDate,0-@Amount)
	END

	SET @Amount = ISNULL(@Amount,0)

	SELECT	@Amount AS Amount
	
	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_GetQZNN_SystemPool] TO [GameUser]
    AS [dbo];

