-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	获取支付金额接口
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetAmountList]	
@intPlatformID		INT=NULL
AS
BEGIN
	
	DECLARE @UnionnPlatform TABLE (PlatformID int,GWID int,TypeID int,[Status] tinyint)

	INSERT INTO @UnionnPlatform 
	SELECT PlatformID,GWID,TypeID,[Status]
	FROM [CenterDB].[Pay].[UnionPlatformPayType] with (nolock) 
	WHERE PlatformID = @intPlatformID
	AND Status = 1	

	SELECT T.GWID, 
		   T.TypeID, 
		   T.TypeName, 
		   TradeCode = S.TradeCode, 
		   T.SourceTypeID, 
		   T.Scale, 
		   T.[Status],
		   T.AmountSort, 
		   T.IsRange,
		   T. MinValue, 
		   T.MaxValue,
		   G.GatewayName,
		   Popular = CAST(0 as tinyint),
		   Sort = 0
	FROM @UnionnPlatform as P
	JOIN [CenterDB].[Pay].[PayType] AS T on T.GWID = P.GWID and T.TypeID = P.TypeID
	JOIN (SELECT * FROM [CenterDB].[Pay].[PayType] where GWID = 6) as S on S.TypeID = T.TypeID		--暫時使用統一代號
	LEFT JOIN [CenterDB].[Pay].[PayGateway] as G on T.GWID= G.ID		
	WHERE T.Status = 1	 	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetAmountList] TO [GameUser]
    AS [dbo];

