USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_UserAgent_GenderQRCode]    Script Date: 2019/6/19 下午 01:02:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	礼码详情 查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Coupon_CouponDataByCode_Get]
(
	@intAdminID	INT,
	@chvCouponCode	CHAR(12),
	@chvMsg	VARCHAR(100)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @MultiUser	INT
	DECLARE @CouoponPlatformID INT
	DECLARE @Status	INT
	DECLARE @ExpiryDate	DATETIME


	DECLARE @Temp_CodeTable TABLE ([CouponCode] CHAR(12),UserID INT,Amount BIGINT,[UseDate] DATETIME)


	SELECT TOP 1 @CouoponPlatformID = PlatformID,
			@MultiUser = [MultiUser],
			@Status = [Status],
			@ExpiryDate = [ExpiryDate]
	FROM [dbo].[CouponUserData] WITH (NOLOCK)
	WHERE
		[CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode


	IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[CouponUserData] WITH (NOLOCK) WHERE [CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode)
	BEGIN
		SELECT	* FROM @Temp_CodeTable

		SET @chvMsg = '操作完成 : 查無此禮碼'
		RETURN -1
	END

	IF @CouoponPlatformID <> 0 AND NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[AgentPlatformRelations] WITH (NOLOCK) WHERE [AdminID] = @intAdminID AND [PlatformID] = @CouoponPlatformID)
	BEGIN
		SELECT	* FROM @Temp_CodeTable

		SET @chvMsg = '操作完成 : 查無此禮碼'
		RETURN -1
	END

	IF(@MultiUser = 0)
	BEGIN
		SELECT	[CouponCode],
				[UserID] = ISNULL([UserID],0),
				[Amount],
				[UseDate]
		FROM [dbo].[CouponUserData] WITH (NOLOCK)
		WHERE
			[CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode
	END
	ELSE
	BEGIN
		SELECT	A.[CouponCode],
				A.UserID,
				B.Amount,
				A.CreateTime
		FROM [dbo].[CouponMultiUserLog] AS A WITH (NOLOCK)
			INNER JOIN [dbo].[CouponUserData] AS B WITH (NOLOCK) ON A.CouponCode = B.CouponCode
		WHERE
			A.[CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCouponCode
	END

	SET @chvMsg = '操作完成 : 完成'
	RETURN 1
END