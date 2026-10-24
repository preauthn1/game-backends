USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_GetPaymentAmountList]    Script Date: 2019/7/10 上午 12:00:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	获取支付金额接口
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_GetPaymentAmountList]	
@intPlatformID		INT=NULL
AS
BEGIN
	SELECT	a.GWID,
			b.TypeID,
			TypeName = a.BankCode,
			TradeCode = a.ID,
			b.SourceTypeID,
			Scale = 100,
			[Status] = 1,
			AmountSort = CASE WHEN a.amountFixed = 1 THEN a.fixedAmount ELSE '10000,20000,30000,50000,60000,80000,90000,100000' END,
			IsRange = CASE WHEN a.amountFixed = 1 THEN 0 ELSE 1 END,
			MinValue = dbo.Fn_PaymentMinAmount(a.minAmount, a.amountFixed, a.fixedAmount),
			MaxValue = CASE WHEN a.maxAmount <> 0 THEN a.maxAmount ELSE 100000000 END,
			c.GatewayName,
			Popular = 0,
			Sort = 0
	FROM [Payment].[PayType] AS a 
	LEFT JOIN [Payment].[BankCodeType] AS b ON a.BankCode = b.BankCode 
	LEFT JOIN [Payment].[PayGateway] AS c ON a.GWID = c.ID
	WHERE a.GWID IN (
		SELECT DISTINCT GWID FROM [Payment].[MerchantInfo] WHERE status = 1
	) AND TypeID IS NOT NULL
	
	
	--DECLARE @UnionnPlatform TABLE (PlatformID int,GWID int,TypeID int,[Status] tinyint)

	--INSERT INTO @UnionnPlatform 
	--SELECT PlatformID,GWID,TypeID,[Status]
	--FROM [CenterDB].[Pay].[UnionPlatformPayType] with (nolock) 
	--WHERE PlatformID = @intPlatformID

	--SELECT T.GWID, 
	--	   T.TypeID, 
	--	   T.TypeName, 
	--	   TradeCode = S.TradeCode, 
	--	   T.SourceTypeID, 
	--	   T.Scale, 
	--	   T.[Status],
	--	   T.AmountSort, 
	--	   T.IsRange,
	--	   T. MinValue, 
	--	   T.MaxValue,
	--	   G.GatewayName,
	--	   Popular = CAST(0 as tinyint),
	--	   Sort = 0
	--FROM @UnionnPlatform as P
	--JOIN [CenterDB].[Pay].[PayType] AS T on T.GWID = P.GWID and T.TypeID = P.TypeID
	--JOIN (SELECT * FROM [CenterDB].[Pay].[PayType] where GWID = 6) as S on S.TypeID = T.TypeID		--暫時使用統一代號
	--LEFT JOIN [CenterDB].[Pay].[PayGateway] as G on T.GWID= G.ID		
	--WHERE T.Status = 1	 	
END
