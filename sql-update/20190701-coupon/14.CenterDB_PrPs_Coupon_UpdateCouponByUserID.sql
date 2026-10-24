USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_UserAgent_GenderQRCode]    Script Date: 2019/6/19 下午 01:02:57 ******/
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
	@chvMsg		VARCHAR(100) OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @intRtnCode INT

	EXEC [Club02].[dbo].[PrPs_Coupon_UpdateCouponByUserID]
		@intUserID = @intUserID,
		@chvCouponCode = @chvCouponCode,
		@chvMsg = @chvMsg OUTPUT,
		@intRtnCode = @intRtnCode OUTPUT

	RETURN @intRtnCode
END


