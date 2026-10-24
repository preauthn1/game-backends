

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取營運商支付方式>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetCompanyPayType]	
@intAdminId int
AS
BEGIN
	
	--營運商篩選
	DECLARE @Temp_Company TABLE (Company INT)

	INSERT INTO @Temp_Company 
	Select DISTINCT 
	       P.Company
	From CenterDB.Area.PlatformAdminUser as A
	JOIN CenterDB.Area.[Platform] as P
	  ON A.PlatformID = P.PlatformID
	where AdminID=@intAdminId

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
	T.Status,	
	G.GatewayName, 
	G.Priority 
	from [CenterDB].[Pay].PayGateway as G 
	JOIN @Temp_Company as S on S.Company = G.Company
	LEFT JOIN [CenterDB].[Pay].PayType AS T on T.GWID = G.ID 
	WHERE NOT (T.GWID = 7 AND TypeID != 0)	--閃付只顯示總開關
	ORDER BY T.GWID DESC, T.TypeID
END


