


-- =============================================
-- Author:  alan
-- Create date: 20180903
-- Description:	取得今日人數, 抽水
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_SuperTool_GetInfo]
(
	@intGameID int
)
AS
BEGIN

	if @intGameID=2 set @intGameID=19


	--監控人數
	DECLARE @intVIPUser int = 0
	SELECT @intVIPUser=Count(userid) from [Game].[SuperTool_VIPUser] with (nolock)

	--抽水數量
	DECLARE @intTaxAmount bigint = 0
	SELECT @intTaxAmount=SUM(TaxAmount)
	FROM [CenterDB].[Stat].[MoneySourceLog] with (nolock)
	where SourceID in 
	(select serverid from serverinfo.GameServerList where GameID = @intGameID) 
	and Convert(varchar,StatTime ,112) = Convert(varchar,GETDATE() ,112)
 
	SELECT 
	Count(Distinct UserID) as TodayUser, 
	@intTaxAmount as TodayTax, 
	@intVIPUser as VipUser
	from logdb.game.GameLog with (nolock)
	WHERE  Convert(varchar,LoginTime,112) = Convert(varchar,getdate(),112)
	AND GameID = @intGameID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_SuperTool_GetInfo] TO [xbuser]
    AS [dbo];

