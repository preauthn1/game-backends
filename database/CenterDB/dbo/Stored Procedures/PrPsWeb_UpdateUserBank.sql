-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新银行>
--	2018/03/21	Jason		調整寫法及改為呼叫SP執行Log寫入
--	2018/04/10	Jason		新增UpdateUserCash時一併寫入AreaID & PlatformID
--	2018/04/13	Jason		優化更新條件
--	2018/05/08	Jason		調整負值問題
-- =============================================
CREATE PROCEDURE [dbo].[PrPsWeb_UpdateUserBank]
@intUserID			INT,			        --用户ID
@sintSourceType		SMALLINT,				--更新方式
@lngUpdateAmount	BIGINT,					--更新的数量
@chvIPAddress		VARCHAR(15),			--IP地址
@chvMemo			VARCHAR(128),           --说明
@lngCurrentAmount	BIGINT OUTPUT,			--返回当前银行金币数
@chvErrMsg			VARCHAR(128) OUTPUT     --错误消息
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @NowTime		DATETIME = GETDATE()
	DECLARE @LogStatTime	DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,@NowTime),0)
	--DECLARE @LogEndTime		DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,@NowTime),1)
	DECLARE @AreaID			INT
	DECLARE @PlatformID		INT

	DECLARE @Temp_OutCurrentCashAmount	TABLE	(UserID INT,Amount BIGINT)

	--更新数量为0 不执行操作
	IF @lngUpdateAmount = 0
	RETURN 1
	
	--SELECT @lngCurrentAmount = Amount FROM Money.Bank WHERE UserID = @intUserID

	----银行账户不存在
	--IF @lngCurrentAmount IS NULL
	--BEGIN
	--   INSERT INTO Money.Bank
	--		(	UserID,		Amount, Code	)
	--   SELECT	@intUserID, 0,		''
	   
	--   SET @lngCurrentAmount = 0
	--END

	/*  --銀行帳戶不存在則先新增  */
	--------------------------------------------------------------------------------
	IF NOT EXISTS (SELECT TOP 1 1 FROM Money.Bank WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		INSERT INTO Money.Bank
			(	UserID,		Amount, Code	)
		SELECT	@intUserID, 0,		''

		SET @lngCurrentAmount = 0
	END
	ELSE
	BEGIN
		SELECT @lngCurrentAmount = Amount FROM Money.Bank WHERE UserID = @intUserID
	END
	--------------------------------------------------------------------------------


	/*  --检查金额是否足够  */
	--------------------------------------------------------------------------------
	IF @lngCurrentAmount + @lngUpdateAmount < 0
	BEGIN
	   SET @chvErrMsg = '操作失败：账户余额不足'
	   RETURN -1
	END
	--------------------------------------------------------------------------------


	/*  --更新余额 & 获取当前现金数  */
	--------------------------------------------------------------------------------
	UPDATE Money.Bank
	SET Amount = Amount + @lngUpdateAmount,
		Code = ''
	OUTPUT INSERTED.UserID,INSERTED.Amount INTO @Temp_OutCurrentCashAmount
	WHERE
		UserID = @intUserID

	DECLARE @lngCurrentCashAmount BIGINT --更新後數量

	SELECT @lngCurrentCashAmount = Amount
	FROM @Temp_OutCurrentCashAmount
	WHERE
		UserID =  @intUserID

	SET @lngCurrentCashAmount = ISNULL(@lngCurrentCashAmount,0)
	--------------------------------------------------------------------------------


	/*  --取款  */
	--------------------------------------------------------------------------------
	IF @sintSourceType = 12001
	SET @lngCurrentCashAmount = @lngCurrentCashAmount + ABS(@lngUpdateAmount)
	--------------------------------------------------------------------------------


	--写日志
	--insert into Log.BankLog
	--(UserID,SourceTypeID,CashAmount,  CurrentAmount, UpdateAmount, IPAddress, Memo, CreateTime)
	--select 
	--@intUserID,@sintSourceType,ISNULL(@lngCurrentCashAmount,0), @lngCurrentAmount, @lngUpdateAmount, @chvIPAddress, @chvMemo, getdate()

	/*  --写日志  */
	--------------------------------------------------------------------------------
	SELECT	@AreaID = [AreaID] FROM [Users].[UserInfo] WITH (NOLOCK) WHERE [UserID] = @intUserID
	SELECT	@PlatformID = [PlatformID] FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID] = @AreaID


	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_AddBankLog]
		@intUserID = @intUserID,
		@intSourceTypeID = @sintSourceType,
		@lngCashAmount = @lngCurrentCashAmount, --更新後銀行金額
		@lngCurrentAmount = @lngCurrentAmount,  --更新前銀行金額
		@lngUpdateAmount = @lngUpdateAmount,    --此次異動金額
		@chrIPAddress = @chvIPAddress,
		@chrMemo = @chvMemo,
		@dtCreateTime = @NowTime,
		@intAreaID = @AreaID,
		@intPlatformID = @PlatformID

	UPDATE Stat.BankSourceLog
	SET Amount = Amount + @lngUpdateAmount
	WHERE
		SourceID = @sintSourceType
		AND StatTime = @LogStatTime
		--AND StatTime < @LogEndTime

	--------------------------------------------------------------------------------


	SET @chvErrMsg = '操作成功:更新银行成功'

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_UpdateUserBank] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPsWeb_UpdateUserBank] TO [xbuser]
    AS [dbo];

