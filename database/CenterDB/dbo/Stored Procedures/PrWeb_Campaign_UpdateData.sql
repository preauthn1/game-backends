-- =============================================
-- Author: Eddie
-- Create date: 2019/07/17
-- Description: sWέק省xʤe
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Campaign_UpdateData]
@intID int,
@chvPicture varchar(512),
@chvTitle	nvarchar(512),
@intSeqID	int,
@tnyTag		tinyint,
@tnyBehavior tinyint,
@chvLink	varchar(512),
@chvStartTime	datetime,
@chvEndTime		datetime,
@tnyStatus tinyint
AS
BEGIN
	
	Update [Area].[Campaign] 
	Set 
	Picture = @chvPicture, 
	Title = @chvTitle,
	SeqID = @intSeqID,
	Tag = @tnyTag,
	Behavior = @tnyBehavior,
	Link = @chvLink,
	StartTime = @chvStartTime,
	EndTime = @chvEndTime,
	[Status] = @tnyStatus	
	WHERE [Id] = @intID

END

GO


