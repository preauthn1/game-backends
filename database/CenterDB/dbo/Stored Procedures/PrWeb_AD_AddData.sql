


-- =============================================
-- Author: alan
-- Create date: 2018/07/12
-- Description: 新增廣告內容
-- Change:
--	2018/11/29	alan 增加areaid, setdefault參數
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AD_AddData]
@intPlatformID INT,	
@intAreaID INT,
@chvPicture varchar(512),
@chvLink varchar(512),
@tnyBehavior tinyint,
@tnyStatus tinyint
AS
BEGIN
	
	Insert into [Area].[Advertisement] (PlatformID, AreaID, Picture, Link, Behavior, [Status])
	Values(@intPlatformID, @intAreaID, @chvPicture, @chvLink,@tnyBehavior,@tnyStatus)

END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AD_AddData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AD_AddData] TO [xbuser]
    AS [dbo];

