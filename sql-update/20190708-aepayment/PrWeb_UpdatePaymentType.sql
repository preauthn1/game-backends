USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_UpdatePaymentType]    Script Date: 2019/7/8 下午 11:27:25 ******/
IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id('dbo.PrWeb_UpdatePaymentType') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[PrWeb_UpdatePaymentType]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:  Mars
-- Create date: 2019/07/04
-- Description: 增加/更新支付方式
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePaymentType]
(
	@chvGWCode			nvarchar(50),
	@tnyType			tinyint,
	@chvCurrencies 		nvarchar(5),
	@chvBankCode 		nvarchar(50),
	@intMinAmount 		int,
	@intMaxAmount 		int,
	@tnyAmountFixed 	tinyint,
	@tnyFixedAmount 	nvarchar(200) = null,
	@chvRtnMsg			varchar(50) = '' OUTPUT	--錯誤訊息
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime DATETIME = GETDATE(),
			@intGWID int = null;
	
	--檢查是否有GWID
	SELECT @intGWID = ID FROM Payment.PayGateway WHERE GatewayCode = @chvGWCode
	--PRINT @intGWID
	IF (@intGWID IS NULL)
	BEGIN 
		SET @chvRtnMsg = '操作失敗:GWID IS NULL'
		RETURN -1
	END

	IF NOT EXISTS (SELECT * FROM Payment.PayType WHERE GWID = @intGWID AND BankCode = @chvBankCode)
	BEGIN
		--新增支付渠道
		IF (@tnyFixedAmount IS NULL)
		BEGIN
			INSERT INTO Payment.PayType(GWID, Type, Currencies, BankCode, MinAmount, MaxAmount, AmountFixed, Status, CreateTime) 
			VALUES (@intGWID, @tnyType, @chvCurrencies, @chvBankCode, @intMinAmount, @intMaxAmount, @tnyAmountFixed, 1, @NowTime)
		END
		ELSE
		BEGIN
			INSERT INTO Payment.PayType(GWID, Type, Currencies, BankCode, MinAmount, MaxAmount, AmountFixed, FixedAmount, Status, CreateTime) 
			VALUES (@intGWID, @tnyType, @chvCurrencies, @chvBankCode, @intMinAmount, @intMaxAmount, @tnyAmountFixed, @tnyFixedAmount, 1, @NowTime)
		END
			
		--Get ID
		IF @@ROWCOUNT = 0
		BEGIN
			SET @chvRtnMsg = '操作失敗'
			RETURN -1
		END
	END
	ELSE
	BEGIN
		UPDATE [Payment].[PayType] 
		SET [Type] = @tnyType, [Currencies] = @chvCurrencies, [BankCode] = @chvBankCode, [MinAmount] = @intMinAmount, [MaxAmount] = @intMaxAmount, 
			[AmountFixed] = @tnyAmountFixed, [FixedAmount] = @tnyFixedAmount, [LastUpdateTime] = @NowTime 
		WHERE [GWID] = @intGWID AND [BankCode] = @chvBankCode

		IF @@ROWCOUNT = 0
		BEGIN
			SET @chvRtnMsg = '更新失敗'
			RETURN -1
		END
	END

	SET @chvRtnMsg = '操作成功'
	RETURN 1
END 
GO


