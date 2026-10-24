-- =============================================
-- Author: Eddie
-- Create date: 2019/07/17
-- Description: oxʲM
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Campaign_GetList]
@intPlatformID INT = null,
@intAreaID INT = null
AS
BEGIN
	Select Id,PlatformID,AreaID,Picture,SeqID,Title,Tag as CampaignTag,Behavior as CampaignBehavior,Link,StartTime,EndTime,[Status] as CampaignStatus,CreateTime from [Area].[Campaign] with (nolock) 
	Where 
	PlatformID = ISNULL(@intPlatformID,PlatformID)
	and AreaID = ISNULL(@intAreaID,AreaID)
	ORDER BY PlatformID
END

GO


