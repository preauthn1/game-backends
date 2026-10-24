

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取平台支付方式>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetUnionPlatformPayType]	
@intAdminId int,
@intPlatformID int
AS
BEGIN
	
	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminId

	SELECT DISTINCT 
	'' as PlatformName, 
	T.GWID,
	T.TypeID,
	T.TypeName,
	T.TradeCode,
	T.SourceTypeID,
	T.Scale,
	T.AmountSort,
	T.IsRange,
	T.MinValue,
	T.MaxValue,
	P.Status,	
	G.GatewayName, 
	G.Priority 
	from [CenterDB].[Pay].UnionPlatformPayType AS P
	join @Temp_PlatformID as S on P.PlatformID = S.PlatformID
	LEFT JOIN [CenterDB].[Pay].PayType AS T on P.GWID=T.GWID and P.TypeID=T.TypeID
	LEFT JOIN [CenterDB].[Pay].PayGateway as G on P.GWID= G.ID
	LEFT JOIN [CenterDB].[area].[Platform] as F on P.PlatformID= F.PlatformID
	LEFT JOIN [CenterDB].[area].[Company] as C on F.Company = C.Id	
	WHERE P.PlatformID = @intPlatformID
	AND P.Status = 1
END


