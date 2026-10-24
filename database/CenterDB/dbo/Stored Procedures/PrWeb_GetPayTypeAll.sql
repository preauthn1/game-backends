

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取PayType信息>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPayTypeAll]	
@intAdminId int
AS
BEGIN


	--平台篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminId

	Declare @intOpUserType int
	select @intOpUserType=UserType from AdminSys.Admin.Users with (nolock) where UserID=@intAdminID

	if @intOpUserType=10
	begin
		--檢查聯運可用的支付渠道
		SELECT DISTINCT C.Name as platformName, 
		T.GWID,T.TypeID,T.TypeName,T.TradeCode,T.SourceTypeID,T.Scale,
		T.AmountSort,T.IsRange,T.MinValue,T.MaxValue,
		T.Status,	
		G.GatewayName, G.Priority 
		from [CenterDB].[Pay].UnionPlatformPayType AS P
		join @Temp_PlatformID as S on P.PlatformID = S.PlatformID
		LEFT JOIN [CenterDB].[Pay].PayType AS T on P.GWID=T.GWID and P.TypeID=T.TypeID
		LEFT JOIN [CenterDB].[Pay].PayGateway as G on P.GWID= G.ID
		LEFT JOIN [CenterDB].[area].[Platform] as F on P.PlatformID= F.PlatformID
		LEFT JOIN [CenterDB].[area].[Company] as C on F.Company = C.Id
		WHERE T.GWID != 7
	end 
	else
	begin	
		SELECT '主平台' as platformName, T.GWID,T.TypeID,T.TypeName,T.TradeCode,T.SourceTypeID,T.Scale,
		T.AmountSort,T.IsRange,T.MinValue,T.MaxValue,
		T.Status,
		G.GatewayName,G.Priority
		from [CenterDB].[Pay].PayType AS T  with (nolock) 
		LEFT JOIN [CenterDB].[Pay].PayGateway as G  with (nolock) 
		on T.GWID= G.ID

		order by T.GWID DESC, T.TypeID
		
	end
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPayTypeAll] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPayTypeAll] TO [xbuser]
    AS [dbo];

