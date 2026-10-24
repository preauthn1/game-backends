-- =============================================
-- Author:  alan
-- Create date: 2018/09/29
-- Description:	充值留存
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetChargeKeepLog]
@intHdType int=null
AS
BEGIN
	SET NOCOUNT ON

	SELECT Convert(datetime,CAST(YYYYMMDD as varchar),112) as StatTime,* from Stat.ChargeKeepLog with (nolock)
	WHERE (@intHdType is null or HDType=@intHdType)
	order by YYYYMMDD DESC,HDType ASC

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetChargeKeepLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetChargeKeepLog] TO [xbuser]
    AS [dbo];

