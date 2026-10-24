


-- =============================================
-- Author: alan
-- Create date: 2018/07/12
-- Description: 新增或修改廣告內容
--		2018/11/29	alan 增加setdefault 參數
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AD_UpdateData]
@intID int,
@chvPicture varchar(512),
@chvLink varchar(512),
@tnyBehavior tinyint,
@tnyStatus tinyint
AS
BEGIN
	
	Update [Area].[Advertisement] 
	Set 
	Picture = @chvPicture, 
	Link = @chvLink,
	Behavior = @tnyBehavior,
	[Status] = @tnyStatus
	WHERE [Id] = @intID

END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AD_UpdateData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AD_UpdateData] TO [xbuser]
    AS [dbo];

