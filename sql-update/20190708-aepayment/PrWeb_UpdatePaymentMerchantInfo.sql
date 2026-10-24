USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_UpdatePaymentMerchantInfo]    Script Date: 2019/7/8 U 11:27:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Mars
-- Create date: 2019/07/08
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePaymentMerchantInfo]
	@chvGWCode varchar(50),
	@chvInfo varchar(2000),
	@chvUuid varchar(100),
	@intMerchantId int = null OUTPUT,
	@chvRtnMsg	varchar(50) = '' OUTPUT	--~T
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @NowTime DATETIME = GETDATE(),
			@intID int = null;
	
	SELECT @intID = ID FROM Payment.PayGateway WHERE GatewayCode = @chvGWCode

	IF (@intID IS NULL)
	BEGIN
		SET @chvRtnMsg = 'dLPay Gateway';
		RETURN -1
	END

	IF (@intMerchantId = 0)
	BEGIN
		INSERT INTO Payment.MerchantInfo(GWID, InfoData, Status, Uuid, CreateTime) VALUES(@intID, @chvInfo, 1, @chvUuid, @NowTime);
		IF (@@ROWCOUNT = 0)
		BEGIN
			SET @chvRtnMsg = 'sW';
			RETURN -1
		END
		SET @intMerchantId = SCOPE_IDENTITY()
		SET @chvRtnMsg = 'sW\';
		RETURN 1
	END
	ELSE
	BEGIN
		UPDATE Payment.MerchantInfo SET InfoData = @chvInfo, Uuid = @chvUuid 
		WHERE ID = @intMerchantId

		IF @@ROWCOUNT > 0
		BEGIN
			SET @chvRtnMsg = 'ާ@\';
			RETURN 1
		END
		ELSE
		BEGIN
			SET @chvRtnMsg = 'ާ@';
			RETURN -1
		END
	END
END
GO


