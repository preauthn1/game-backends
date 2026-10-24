

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取代付廠商>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetConvertGateway]	
@intAdminId int
AS
BEGIN


	--營運商篩選
	DECLARE @Temp_CompanyID TABLE (Company	INT)

	INSERT INTO @Temp_CompanyID 
	SELECT DISTINCT 
	       P.Company
	  FROM CenterDB.Area.PlatformAdminUser AS  U WITH (NOLOCK)
	  JOIN [CenterDB].[Area].[Platform] AS P WITH (NOLOCK) on U.PlatformID = P.PlatformID
	 WHERE AdminID=@intAdminId

	SELECT [ID],
           [GatewayName],
           [ConvertType],
           [Status],
           G.[Company]
      FROM [CenterDB].[Pay].[ConvertGateway] AS  G WITH (NOLOCK)	 
	  JOIN @Temp_CompanyID AS T on T.Company = G.Company
     WHERE Status = 1

END