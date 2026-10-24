-- =============================================
-- Author: Eddie
-- Create date: 2019/07/17
-- Description: sWxʤe
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Campaign_AddData]
@intPlatformID INT,	
@intAreaID INT,
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
	
	Insert into [Area].[Campaign] (PlatformID, AreaID, Picture, Title, SeqID, Tag, Behavior, Link, StartTime, EndTime, [Status])
	Values(@intPlatformID, @intAreaID, @chvPicture, @chvTitle, @intSeqID, @tnyTag, @tnyBehavior, @chvLink, @chvStartTime, @chvEndTime, @tnyStatus)

END

GO


