-- =============================================
-- Author:		<xxx>
-- Create date: <xxx>
-- Description:	<获取用户幸运值>
--	2018/03/26	Jason		更新原本幸運值0.1為1.00
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_GetUserLuckScore]
@intServerID	INT,				--房间ID
@intUserID		INT,				--用户ID
@lngScore		BIGINT,				--金币变化数
@chvValue		VARCHAR(32)OUTPUT	--用户幸运值
AS
BEGIN	
	SET @chvValue = '1.00'
	--if @intUserID = 1020888
	--begin
	--	SET @chvValue = '100'	
	--end
	return 1	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_GetUserLuckScore] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_GetUserLuckScore] TO [xbuser]
    AS [dbo];

