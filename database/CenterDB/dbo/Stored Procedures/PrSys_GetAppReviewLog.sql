-- =============================================
-- Author:  alan
-- Create date: 
-- Description:	取得app審合資訊
-- Changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrSys_GetAppReviewLog]
AS
BEGIN

	SET NOCOUNT ON

	SELECT top 1000 a.*,
	case when b.userflag is null then 'N/A' else convert(varchar,b.UserFlag) end as block_userlfage,
	case when c.IP is null then 'N/A' else c.IP end as block_IP,
	case when c.AreaID is null then 'N/A' else convert(varchar,c.AreaID) end as block_Area, 
	case when d.AreaID is null then 'N' ELSE 'Y' END as app_review

	FROM [CenterDB].[Apple].[ReviewHistory] a
	left join  apple.ReviewUser  b on a.HDCode=b.HDCode
	left join apple.IP c on a.IPAddress=c.IP and a.AreaID=c.AreaID
	left join apple.ReviewArea d on a.AreaID = d.AreaID

	order by logid desc

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_GetAppReviewLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_GetAppReviewLog] TO [xbuser]
    AS [dbo];

