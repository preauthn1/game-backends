USE [LogDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Stat_AmountLog_Update]    Script Date: 2019/6/19 下午 06:18:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  Jason
-- Create date: 2019/06/05
-- Description:	寫入用戶金幣相關紀錄
--	2019/06/19	Jason	新增寫入禮碼金額欄位/參數
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_Stat_AmountLog_Update]
(
	@intPlatformID		INT,
	@intAreaID			INT,
	@intUserID			INT,
	@intType			INT,	-- 1:charge 2:convert 3:game 4: input coupon
	@lngAmount			BIGINT,
	@lngFlowAmount		BIGINT = NULL,	-- for game flow amount
	@lngTaxAmount		BIGINT = NULL,	-- for game tax and convert tax
	@lngCouponAmount	BIGINT = NULL	-- for game coupon used or input coupon amount
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @LogDate			DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,GETDATE()))

	--SET @lngAmount = CASE WHEN @lngAmount = 0 THEN NULL ELSE @lngAmount END
	--SET @lngFlowAmount = CASE WHEN @lngFlowAmount = 0 THEN NULL ELSE @lngFlowAmount END
	--SET @lngTaxAmount = CASE WHEN @lngTaxAmount = 0 THEN NULL ELSE @lngTaxAmount END	

	SET @lngAmount = ISNULL(@lngAmount,0)
	SET @lngFlowAmount = ISNULL(@lngFlowAmount,0)
	SET @lngTaxAmount = ISNULL(@lngTaxAmount,0)
	SET @lngCouponAmount = ISNULL(@lngCouponAmount,0)


	IF NOT EXISTS(SELECT TOP 1 1 FROM [Users].[UserRegLog] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		RETURN
	END




	IF NOT EXISTS (SELECT TOP 1 1 FROM [Stat].[StatisticsUserAmount_Day] WHERE [LogDate] = @LogDate AND [UserID] = @intUserID)
	BEGIN
		INSERT INTO [Stat].[StatisticsUserAmount_Day]
			([LogDate],[UserID],[PlatformID],[AreaID],[ChargeAmount],[ConvertAmount],[FlowAmount],[GameTaxAmount],[ConvertTaxAmount],
				[ChargeCount],[ConvertCount],[FlowCount],[GameTaxCount],[ConvertTaxCount],[GameAmount],[GameAmountCount],CouponAmount,CouponCount,InputCouponAmount,InputCouponCount)
		SELECT	@LogDate,
				@intUserID,
				@intPlatformID,
				@intAreaID,
				[ChargeAmount] = CASE WHEN @intType = 1 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN @lngAmount ELSE 0 END,
				[ConvertAmount] = CASE WHEN @intType = 2 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN @lngAmount ELSE 0 END,
				[FlowAmount] = CASE WHEN @intType = 3 AND @lngFlowAmount IS NOT NULL AND @lngFlowAmount > 0 THEN @lngFlowAmount ELSE 0 END,
				[GameTaxAmount] = CASE WHEN @intType = 3 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN @lngTaxAmount ELSE 0 END,
				[ConvertTaxAmount] = CASE WHEN @intType = 2 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN @lngTaxAmount ELSE 0 END,
				[ChargeCount] = CASE WHEN @intType = 1 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN 1 ELSE 0 END,
				[ConvertCount] = CASE WHEN @intType = 2 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN 1 ELSE 0 END,
				[FlowCount] = CASE WHEN @intType = 3 AND @lngFlowAmount IS NOT NULL AND @lngFlowAmount > 0 THEN 1 ELSE 0 END,
				[GameTaxCount] = CASE WHEN @intType = 3 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN 1 ELSE 0 END,
				[ConvertTaxCount] = CASE WHEN @intType = 2 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN 1 ELSE 0 END,
				[GameAmount] = CASE WHEN @intType = 3 AND @lngAmount IS NOT NULL AND @lngAmount <> 0 THEN @lngAmount ELSE 0 END,
				[GameAmountCount] = CASE WHEN @intType = 3 AND @lngAmount IS NOT NULL AND @lngAmount <> 0 THEN 1 ELSE 0 END,
				CouponAmount = CASE WHEN @intType = 3 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN @lngCouponAmount ELSE 0 END,
				CouponCount = CASE WHEN @intType = 3 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN 1 ELSE 0 END,
				InputCouponAmount = CASE WHEN @intType = 4 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN @lngCouponAmount ELSE 0 END,
				InputCouponCount = CASE WHEN @intType = 4 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN 1 ELSE 0 END
	END
	ELSE
	BEGIN
		UPDATE [Stat].[StatisticsUserAmount_Day]
		SET [ChargeAmount] = CASE WHEN @intType = 1 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN [ChargeAmount] + @lngAmount ELSE [ChargeAmount] END,
			[ConvertAmount] = CASE WHEN @intType = 2 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN [ConvertAmount] + @lngAmount ELSE [ConvertAmount] END,
			[FlowAmount] = CASE WHEN @intType = 3 AND @lngFlowAmount IS NOT NULL AND @lngFlowAmount > 0 THEN [FlowAmount] + @lngFlowAmount ELSE [FlowAmount] END,
			[GameTaxAmount] = CASE WHEN @intType = 3 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN [GameTaxAmount] + @lngTaxAmount ELSE [GameTaxAmount] END,
			[ConvertTaxAmount] = CASE WHEN @intType = 2 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN [ConvertTaxAmount] + @lngTaxAmount ELSE [ConvertTaxAmount] END,
			[ChargeCount] = CASE WHEN @intType = 1 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN [ChargeCount] + 1 ELSE [ChargeCount] END,
			[ConvertCount] = CASE WHEN @intType = 2 AND @lngAmount IS NOT NULL AND @lngAmount > 0 THEN [ConvertCount] + 1 ELSE [ConvertCount] END,
			[FlowCount] = CASE WHEN @intType = 3 AND @lngFlowAmount IS NOT NULL AND @lngFlowAmount > 0 THEN [FlowCount] + 1 ELSE [FlowCount] END,
			[GameTaxCount] = CASE WHEN @intType = 3 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN [GameTaxCount] + 1 ELSE [GameTaxCount] END,
			[ConvertTaxCount] = CASE WHEN @intType = 2 AND @lngTaxAmount IS NOT NULL AND @lngTaxAmount > 0 THEN [ConvertTaxCount] + 1 ELSE [ConvertTaxCount] END,
			[GameAmount] = CASE WHEN @intType = 3 AND @lngAmount IS NOT NULL AND @lngAmount <> 0 THEN [GameAmount] + @lngAmount ELSE [GameAmount] END,
			[GameAmountCount] = CASE WHEN @intType = 3 AND @lngAmount IS NOT NULL AND @lngAmount <> 0 THEN [GameAmountCount] + 1 ELSE [GameAmountCount] END,
			CouponAmount = CASE WHEN @intType = 3 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN CouponAmount + @lngCouponAmount ELSE CouponAmount END,
			CouponCount = CASE WHEN @intType = 3 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN CouponCount + 1 ELSE CouponCount END,
			InputCouponAmount = CASE WHEN @intType = 4 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN InputCouponAmount + @lngCouponAmount ELSE InputCouponAmount END,
			InputCouponCount = CASE WHEN @intType = 4 AND @lngCouponAmount IS NOT NULL AND @lngCouponAmount <> 0 THEN InputCouponCount + 1 ELSE InputCouponCount END
		WHERE
			[LogDate] = @LogDate
			AND [UserID] = @intUserID
	END

END