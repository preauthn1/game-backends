-- =============================================
-- Author:		Jason
-- Create date: 2018/04/30
-- Description:	TradeID查詢遊戲紀錄ID
--	2018/05/25	Jason		#371 用戶基本資料查詢 透過現金紀錄的Trade ID 查詢該筆現金的每局遊戲記錄 與原本TradeID取LogID共用同隻SP
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_PerGameLogByTradeID]
(
	@lngTradeID		BIGINT,	--房间ID
	@intUserID		INT = NULL
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE	@TradeID	BIGINT = @lngTradeID
	DECLARE @UserID		INT = @intUserID
	DECLARE @LogID		BIGINT

	IF (@UserID IS NULL)
	BEGIN
		SELECT	[LogID]
		FROM [PerGameLog].[PerLog].[DetailLog] WITH (NOLOCK)
		WHERE
			[TradeID] = @TradeID
	END
	ELSE
	BEGIN
	
		select a.*,b.ServerName as 'ServerName',c.GameName as 'GameName',
			   d.LoginName as 'LoginName',d.NickName as 'NickName',d.GradeID as 'GradeID'
		from PerLog.DetailLog as a 
			 inner join CENTERDBLINKEDSERVER.[Centerdb].ServerInfo.GameServerList as b on a.ServerID = b.ServerID
			 inner join CENTERDBLINKEDSERVER.[Centerdb].ServerInfo.GameList as c on b.GameID = c.GameID
			 inner join CENTERDBLINKEDSERVER.[Centerdb].Users.UserInfo as d on a.UserID = d.UserID	 
		where a.[TradeID] = @TradeID AND a.[UserID] = @UserID
	END

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_PerGameLogByTradeID] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_PerGameLogByTradeID] TO [xbuser]
    AS [dbo];

