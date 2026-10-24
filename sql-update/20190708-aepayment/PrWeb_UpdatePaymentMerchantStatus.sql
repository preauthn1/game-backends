USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_UpdatePaymentMerchantStatus]    Script Date: 2019/7/8 U 11:27:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePaymentMerchantStatus]
	@intMerchantId	int,
	@intStatus int,
	@chvRtnMsg varchar(50) = '' OUTPUT	--~T
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Payment.merchantInfo SET Status = @intStatus WHERE ID = @intMerchantId

	IF @@ROWCOUNT > 0
	BEGIN
	SET @chvRtnMsg = 'sA\';
	RETURN 1
	END

	SET @chvRtnMsg = 'sA';
	RETURN -1
END
GO


