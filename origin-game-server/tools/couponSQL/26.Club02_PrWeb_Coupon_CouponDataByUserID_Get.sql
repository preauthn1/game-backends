USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_UserAgent_GenderQRCode]    Script Date: 2019/6/19 U 01:02:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	dߪaC
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

		SET @chvMsg = 'ާ@ : dLΤ'
		RETURN -1
	END


	SELECT	@CouponAmount = SumCouponAmount,
			@LastLogon = LastLogonTime
	FROM CenterDB.Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SELECT	@UsedCouponAmount = CouponAmount
	FROM LogDB.[Stat].[StatisticsUserAmount_Day] WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SELECT	@CouponCount = COUNT(DISTINCT A.[CouponCode])
	FROM (
			SELECT	DISTINCT [CouponCode],UserID
			FROM [dbo].[CouponUserData] WITH (NOLOCK)
			WHERE
				[MultiUser] = 0
			UNION
			SELECT	DISTINCT [CouponCode],UserID
			FROM [dbo].[CouponMultiUserLog] WITH (NOLOCK)
			WHERE
				UserID = @intUserID
		) AS A
		INNER JOIN [dbo].[CouponUserData] AS B WITH (NOLOCK) ON A.CouponCode = B.CouponCode


	SET @CouponAmount = ISNULL(@CouponAmount,0)
	SET @UsedCouponAmount = ISNULL(@UsedCouponAmount,0)
	SET @LastLogon = ISNULL(@LastLogon,'1970-01-01')
	SET @CouponCount = ISNULL(@CouponCount,0)

	SELECT	UserID = @intUserID,
			CouponCount = @CouponCount,
			CouponAmount = @CouponAmount - @UsedCouponAmount,
			UsedCouponAmount = @UsedCouponAmount,
			LastLogon = @LastLogon

	SET @chvMsg = 'ާ@ : '
	RETURN 1
END