-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	获取支付金额接口
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPayAmountList]	
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
		   T.TradeCode, 
		   T.SourceTypeID, 
		   T.Scale, 
		   T.[Status],
		   T.AmountSort, 
		   T.IsRange,
		   T. MinValue, 
		   T.MaxValue,
		   G.GatewayName,
		   Popular = CAST(0 as tinyint),
		   Sort = 0,
		   Priority
	FROM @UnionnPlatform as P
	JOIN [CenterDB].[Pay].[PayType] AS T on T.GWID = P.GWID and T.TypeID = P.TypeID
	LEFT JOIN [CenterDB].[Pay].[PayGateway] as G on T.GWID= G.ID		
	WHERE T.Status = 1	 	
END