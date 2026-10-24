

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比賽活動用戶資料
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_GetContestUserData]
(
	@intGameID int,
	@intUserID int
)
AS
BEGIN

	select top 1
	*
	from Event.ContestUserData With (nolock) where GameID=@intGameID and UserID=@intUserID
	order by CreateTime desc

	RETURN 1	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestUserData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestUserData] TO [xbuser]
    AS [dbo];

