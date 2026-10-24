USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Coupon_CouponDataByUserID_Get]    Script Date: 2019/7/3 下午 03:39:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	查詢玩家列表
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Coupon_CouponDataByUserID_Get]
(
	@intAdminID	INT,
	@intUserID	INT,
	@chvMsg	VARCHAR(100)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @CouponAmount BIGINT
	DECLARE @UsedCouponAmount	BIGINT
	DECLARE @LastLogon	DATETIME
	DECLARE @CouponCount INT

	DECLARE @TempUserData TABLE (UserID INT,CouponCount INT,CouponAmount BIGINT,UsedCouponAmount BIGINT,LastLogon DATETIME)


	IF NOT EXISTS (SELECT TOP 1 1 FROM CenterDB.Users.UserInfo WITH (NOLOCK) WHERE [UserID] = @intUserID)
	BEGIN
		SELECT	* FROM @TempUserData

		SET @chvMsg = '操作完成 : 查無此用戶'
		RETURN -1
	END


	SELECT	--@CouponAmount = SumCouponAmount,
			@LastLogon = LastLogonTime
	FROM CenterDB.Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SELECT @CouponAmount = CouponAmount 
	FROM CenterDB.Money.Cash WITH (NOLOCK)
	WHERE UserID = @intUserID

	SELECT	@UsedCouponAmount = SUM(CouponAmount), @CouponCount = SUM(InputCouponCount)
	FROM LogDB.[Stat].[StatisticsUserAmount_Day] WITH (NOLOCK)
	WHERE
		UserID = @intUserID
	GROUP BY UserID


	SET @CouponAmount = ISNULL(@CouponAmount,0)
	SET @UsedCouponAmount = ISNULL(@UsedCouponAmount,0)
	SET @LastLogon = ISNULL(@LastLogon,'1970-01-01')
	SET @CouponCount = ISNULL(@CouponCount,0)

	SELECT	UserID = @intUserID,
			CouponCount = @CouponCount,
			CouponAmount = @CouponAmount,
			UsedCouponAmount = ABS(@UsedCouponAmount),
			LastLogon = @LastLogon

	SET @chvMsg = '操作完成 : 完成'
	RETURN 1
END