-- =============================================
-- Author:		<xxx>
-- Create date: <15/08/21>
-- Description:	<添加牌局记录>
--	2018/04/16	Jason		新增寫入時間點
--  2018/04/18	alan		add transaction
--	2018/04/23	Jason		新增@intGameID
--	2018/04/26	Jason		新增@chvUserGameRet用戶TradeID識別格式
--	2018/07/26	alan		新增@intWinPrizeID指定开奖编号
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_AddPerGameLog]
@intServerID		INT,				--房间ID
@chvGameData		VARCHAR(max),		--游戏数据详情
@chvUserGameRet		varchar(max),		--用户信息详情 '10001:2000:-10:20180401,10002:1000:-10:20180402,10003:3000:40:20180405,10003:3000:-20:20180409',
										--分别对应UserID,CurrentAmount,UpdateAmount,TradeID
@dtmGameBeginTime	DATETIME,			--游戏开始时间点
@dtmGameEndTime		DATETIME,			--游戏结束时间点
@lngTaxAmount		BIGINT,				--该局游戏税收
@intGameID			INT = NULL,			--遊戲ID
@intWinPrizeID		INT = NULL			--指定开奖编号
AS
SET NOCOUNT ON

declare @intLogID BIGINT
DECLARE @Days	INT = DATEDIFF(MINUTE,0,@dtmGameBeginTime) + CASE WHEN DATEPART(second,@dtmGameBeginTime) + DATEPART(MICROSECOND,@dtmGameBeginTime) > 0 THEN 1 ELSE 0 END

BEGIN TRAN

	--写每局详情记录
	INSERT INTO PerLog.PerGameLog(ServerID,GameData,TaxAmount,BeginTime,EndTime,GameID,[Days])
	SELECT @intServerID,@chvGameData,@lngTaxAmount,@dtmGameBeginTime,@dtmGameEndTime,@intGameID,@Days

	SET @intLogID = @@IDENTITY

	INSERT INTO PerLog.DetailLog(LogID,ServerID,UserID,CurrentAmount,UpdateAmount,GameTime,GameID,[Days],[TradeID])
	SELECT @intLogID,@intServerID,T.UserID,T.CurrentAmount,T.UpdateAmount,@dtmGameEndTime,@intGameID,@Days,T.TradeID
	FROM  dbo.Fn_GetUserDetailInfo(@chvUserGameRet) as T

	--如果有指定开奖, 更新指定开奖记录
	IF @intWinPrizeID IS NOT NULL
	BEGIN
		UPDATE [PerLog].[WinPrizeLog] SET LogID = @intLogID, UpdateTime=GETDATE() WHERE ID = @intWinPrizeID
	END


COMMIT TRAN

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_AddPerGameLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_AddPerGameLog] TO [xbuser]
    AS [dbo];

