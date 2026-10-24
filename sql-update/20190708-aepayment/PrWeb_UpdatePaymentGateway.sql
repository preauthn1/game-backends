USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_UpdatePaymentGateway]    Script Date: 2019/7/8 U 11:26:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:  Mars
-- Create date: 2019/07/04
-- Description: sW/sID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePaymentGateway]
(
	@chvGatewayCode 	varchar(20),
	@chvGatewayName 	varchar(20),
	@intMerchantInfo 	int,
	@tnySupportPayin 	tinyint,
	@tnySupportPayout 	tinyint,
	@tnyReviseAmount 	tinyint,
	@chvBankCodes 		varchar(200),
	@intID 				int = null 			OUTPUT, --Dkey
	@chvRtnMsg			varchar(50) = '' 	OUTPUT	--~T
)
AS
BEGIN

	SET NOCOUNT ON

	SELECT @intID = ID FROM Payment.PayGateway WHERE GatewayCode = @chvGatewayCode

	--ˬdO_thirdPartyCode
	IF (@intID IS NULL)
	BEGIN
		--sWID
		INSERT INTO Payment.PayGateway(GatewayCode, GatewayName, MerchantInfo, SupportPayin, SupportPayout, ReviseAmount, BankCodes, Status, CreateTime) 
		VALUES (@chvGatewayCode, @chvGatewayName, @intMerchantInfo, @tnySupportPayin, @tnySupportPayout, @tnyReviseAmount, @chvBankCodes, 1, getDate())
		--Get ID
		IF @@ROWCOUNT > 0
		BEGIN
			SET @intID = SCOPE_IDENTITY()
			SET @chvRtnMsg = 'ާ@\'
			RETURN 1
		END
		ELSE
		BEGIN
			SET @intID = 0
			SET @chvRtnMsg = 'ާ@'
			RETURN -1
		END	
	END
	ELSE
	BEGIN
		--sID
		UPDATE Payment.PayGateway SET GatewayName = @chvGatewayName, MerchantInfo = @intMerchantInfo, SupportPayin = @tnySupportPayin, SupportPayout = @tnySupportPayout, ReviseAmount = @tnyReviseAmount, BankCodes = @chvBankCodes, 
				Status = 1, LastUpdateTime = getDate() 
		WHERE ID = @intID
		IF @@ROWCOUNT > 0
		BEGIN
			SET @chvRtnMsg = 'ާ@\'
			RETURN 1
		END
		ELSE
		BEGIN
			SET @chvRtnMsg = 'ާ@'
			RETURN -1
		END
	END
END 
GO


