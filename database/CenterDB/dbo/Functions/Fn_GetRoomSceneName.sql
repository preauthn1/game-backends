-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取游戏房间场名称>
-- 2019/04/02	Jason		依照lisa需求調整顯示名稱
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetRoomSceneName]
(
	@intShowName		varchar(64),	--房间ID
	@intGameID			int
)
RETURNS varchar(32)
AS
BEGIN
	
	DECLARE @chvRoomSceneName varchar(32)
	SET @chvRoomSceneName = case 

							  when @intShowName = '1' and @intGameID in (1) then '高级场'
							  /* 20190402 依照需求調整  */
							 -- when @intShowName = '1' and @intGameID in (2,3,15) then '试炼场'
							 when @intShowName = '1' and @intGameID in (2,3,4,5,7,8,10,12,14,15,16,17,22,24) then '初级场'
							  when @intShowName = '1' then '万人场'

							  when @intShowName = '2' then '菁英场'
 
							  when @intShowName = '3' and @intGameID in (1) then '富豪场'
							  when @intShowName = '3' then '豪华场'

							  when @intShowName = '4' then '富豪场'
							  when @intShowName = '5' then '皇家场'
							  when @intShowName = '6' then '体验场'
							  else '未知场' end	
	return @chvRoomSceneName			
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetRoomSceneName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetRoomSceneName] TO [xbuser]
    AS [dbo];

