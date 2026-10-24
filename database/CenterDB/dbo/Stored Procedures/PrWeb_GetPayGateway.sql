-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	用户选择支付方式, 获取可用支付通道
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPayGateway]	
@intAreaID			int=null,	--根据平台渠道筛选可用的支付
@intPayTypeID		int			--PayTypeID
AS
BEGIN

	declare @intPlatformID int=null
	declare @chvPlatformName varchar(100)

	select @intPlatformID=PlatformID from area.Area with (nolock) where AreaID=@intAreaID
	select @chvPlatformName=Name from area.Platform with (nolock) where PlatformID=@intPlatformID

	DECLARE @UnionnPlatform TABLE (PlatformID int,GWID int,TypeID int,[Status] tinyint)

	INSERT INTO @UnionnPlatform 
	SELECT PlatformID,GWID,TypeID,[Status] 
	FROM [CenterDB].[Pay].[UnionPlatformPayType] with (nolock) 
	WHERE PlatformID = @intPlatformID
	AND [Status] = 1

	SELECT @chvPlatformName as PlatformName, 
	       T.GWID, 
		   T.TypeID, 
		   T.TypeName, 
		   T.TradeCode,
		   T.SourceTypeID,
		   T.Scale, 
		   T.[Status], 
		   T.AmountSort, 
		   T.IsRange, 
		   T.MinValue, 
		   T.MaxValue,
		   G.GatewayName, 
	       G.[Priority]
	FROM @UnionnPlatform AS P
    LEFT JOIN [CenterDB].[Pay].PayType AS T on P.GWID=T.GWID and P.TypeID=T.TypeID
    LEFT JOIN [CenterDB].[Pay].PayGateway as G on P.GWID= G.ID
    WHERE T.TypeID = @intPayTypeID 
	and T.Status = 1

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPayGateway] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPayGateway] TO [xbuser]
    AS [dbo];

