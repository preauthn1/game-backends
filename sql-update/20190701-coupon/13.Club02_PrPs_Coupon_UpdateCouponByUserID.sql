USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrPs_Coupon_UpdateCouponByUserID]    Script Date: 2019/6/19 下午 03:58:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	玩家兌換禮碼
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Coupon_UpdateCouponByUserID]
(
	@intUserID	INT,
	@chvCouponCode	VARCHAR(12),
	@chvMsg		VARCHAR(100) OUTPUT,
	@intRtnCode	INT OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime DATETIME = GETDATE()
	DECLARE @ExpiryDate	DATETIME
	DECLARE @PlatformID INT
	DECLARE @AreaID	INT
	DECLARE @Status	INT	--0:未使用 1:已使用 2:已停用
	DECLARE @MultiUser	INT
	DECLARE @MultiCounts INT
	DECLARE @Amount	INT
	DECLARE @UserPlatformID INT


	/*  取用戶對應平台  */
	SELECT	@AreaID = AreaID
	FROM CenterDB.Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SELECT	@UserPlatformID = [PlatformID]
	FROM CenterDB.Area.Area WITH (NOLOCK)
	WHERE
		AreaID = @AreaID

	SELECT	@PlatformID = [PlatformID],
			@Status = [Status],
			@MultiUser = [MultiUser],
			@MultiCounts = [MultiCount],
			@Amount = [Amount],
			@ExpiryDate = [ExpiryDate]
	FROM [dbo].[CouponUserData] WITH (NOLOCK)
	WHERE
		[CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode


	IF NOT EXISTS(SELECT TOP 1 1 FROM CenterDB.Users.UserInfo WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		SET @chvMsg = '操作错误 : 用戶不存在'
		SET @intRtnCode = -1
		RETURN
	END

	IF(@PlatformID IS NULL)
	BEGIN
		SET @chvMsg = '输入错误，请重新输入'
		SET @intRtnCode = -1
		RETURN
	END

	IF(@PlatformID <> 0) AND (@UserPlatformID <> @PlatformID)
	BEGIN
		SET @chvMsg = '输入错误，请重新输入'
		SET @intRtnCode = -1
		RETURN
	END

	IF(ISNULL(@ExpiryDate,'2019-01-01') < @NowTime)
	BEGIN
		SET @chvMsg = '输入错误，请重新输入'
		SET @intRtnCode = -1
		RETURN
	END
	
	IF(@MultiUser <> 1) AND (@Status > 0)
	BEGIN
		SET @chvMsg = '输入错误，请重新输入'
		SET @intRtnCode = -1
		RETURN
	END

	IF(@MultiUser <> 0) AND (@Status > 0)
	BEGIN
		SET @chvMsg = '输入错误，请重新输入'
		SET @intRtnCode = -1
		RETURN
	END

	IF(@MultiUser = 1) AND EXISTS (SELECT TOP 1 1 FROM [dbo].[CouponMultiUserLog] WITH (NOLOCK) WHERE [CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode AND UserID = @intUserID)
	BEGIN
		SET @chvMsg = '输入错误，请重新输入'
		SET @intRtnCode = -1
		RETURN
	END


	/*  更新用戶禮碼領取  */
	IF(@MultiUser = 1)
	BEGIN
		INSERT INTO [dbo].[CouponMultiUserLog] ([CouponCode],[UserID],[CreateTime]) VALUES (@chvCouponCode,@intUserID,@NowTime)

		IF(@@ROWCOUNT <> 1)
		BEGIN
			SET @chvMsg = '输入错误，请重新输入'
			SET @intRtnCode = -1
			RETURN
		END

		UPDATE [dbo].[CouponUserData]
		SET [UseCount] = [UseCount] + 1
		WHERE
			[CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode

		IF(@@ROWCOUNT <> 1)
		BEGIN
			SET @chvMsg = '输入错误，请重新输入'
			SET @intRtnCode = -1
			RETURN
		END
	END
	ELSE
	BEGIN
		UPDATE [dbo].[CouponUserData]
		SET [Status] = 1,
			[UserID] = @intUserID,
			[UseDate] = @NowTime
		WHERE
			[CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode

		IF(@@ROWCOUNT <> 1)
		BEGIN
			SET @chvMsg = '输入错误，请重新输入'
			SET @intRtnCode = -1
			RETURN
		END

		UPDATE [CenterDB].[Money].[Cash]
		SET CouponAmount = CouponAmount + @Amount
		WHERE
			UserID = @intUserID
		
		UPDATE [CenterDB].[Users].[UserInfo]
		SET SumCouponAmount = SumCouponAmount + @Amount
		WHERE
			UserID = @intUserID
		
		EXECUTE LOGDBLINKEDSERVER.LogDB.dbo.PrWeb_Stat_AmountLog_Update
		@intPlatformID = @PlatformID,
		@intAreaID = @AreaID,
		@intUserID = @intUserID,
		@intType = 4,	-- 1:charge 2:convert 3:game 4: input coupon
		@lngAmount = 0,
		@lngFlowAmount = NULL,	-- for game flow amount
		@lngTaxAmount = NULL,	-- for game tax and convert tax
		@lngCouponAmount = @Amount	-- for game coupon used or input coupon amount
		
		IF(@@ROWCOUNT <> 1)
		BEGIN
			SET @chvMsg = '输入错误，请重新输入'
			SET @intRtnCode = -1
			RETURN
		END
	END

	SET @chvMsg = '兑换成功'
	SET @intRtnCode = 1
	RETURN

END


