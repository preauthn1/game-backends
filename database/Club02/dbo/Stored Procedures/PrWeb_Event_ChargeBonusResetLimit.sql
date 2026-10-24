-- =============================================
-- Author:  jason
-- Create date: 2019/05/28
-- Description: 活動 充值返利活動 兌換流水歸0
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_ChargeBonusResetLimit]	
(
	@intUserID			INT,
	@chvMsg		VARCHAR(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @NowTime			DATETIME = GETDATE()
	DECLARE @PlatformID			INT
	DECLARE @AreaID				INT
	DECLARE @FlowAmountRate		BIGINT
	DECLARE @FlowAmountLimit	BIGINT
	DECLARE @SumFlowAmount		BIGINT
	DECLARE @ChargeAmount		BIGINT
	DECLARE @EventID			INT = 1		-- 活動編號 依照後台已新增後才建立判斷SP
	DECLARE @Status				INT = 1		-- 0:活動關閉 1:活動開啟


	/*  取用戶現有資訊  */
	SELECT	@SumFlowAmount = SumFlowAmount,
			@AreaID = AreaID
	FROM CenterDB.Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SELECT	@PlatformID = PlatformID
	FROM CenterDB.Area.Area WITH (NOLOCK)
	WHERE
		AreaID = @AreaID


	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[EventPlatformInfo] WITH (NOLOCK) WHERE PlatformID = @PlatformID AND AreaID = @AreaID AND [Status] = @Status)
	BEGIN
		SET @chvMsg = '無充值返利活動'
		RETURN 1
	END


	/*  寫入活動資料  */
	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[Event_ChargeBonusRecord] WITH (NOLOCK))
	BEGIN
		INSERT INTO [dbo].[Event_ChargeBonusRecord]
			([UserID],[ChargeDate],[ChargeAmount],[SumFlowAmount],[FlowAmountLimit])
		VALUES
			(@intUserID,@NowTime,@ChargeAmount,@SumFlowAmount,@FlowAmountLimit)
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Event_ChargeBonusRecord]
		SET [ChargeDate] = @NowTime,
			[SumFlowAmount] = @SumFlowAmount,
			[FlowAmountLimit] = @SumFlowAmount
		WHERE
			UserID = @intUserID 
	END

	IF @@ROWCOUNT <> 1
	BEGIN
		SET @chvMsg = '操作失敗 : 返利活動資格有誤,請聯繫客服'
		RETURN -1
	END


	SET @chvMsg = '操作完成'
	RETURN 1
END
