


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<新增GameList>
--		2018/5/15 alan	GameList移除areaid欄位, 相關判斷移除
--		2018/7/12 emily	#583 后台新增广告与游戏热门程度管理 -- 新增欄位Popular
--		2018/11/29 alan	增加areaid欄位
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddAreaGame]		
@intPlatformID INT,	
@intAreaID INT,	
@intGameID INT,
@intIconID INT,	
@intSort INT,	
@tintStatus	tinyint,	--状态
@tintPopular	tinyint,
@chvErrMsg	VARCHAR(256) OUTPUT		--返回消息
AS
BEGIN

	--检查是否有重覆

	IF EXISTS(SELECT 1 FROM [CenterDB].[Area].[GameList] WHERE PlatformID = @intPlatformID AND AreaID=@intAreaID AND GameID =@intGameID)
	BEGIN
		SET @chvErrMsg = '已有重覆的设定'
		RETURN 0
	END

	--新增GameList

	INSERT INTO [CenterDB].[Area].[GameList] (PlatformID, AreaID, GameID, Sort, Status, Popular, IconID)
	VALUES (@intPlatformID, @intAreaID, @intGameID, @intSort, @tintStatus, @tintPopular, @intIconID)
	SET @chvErrMsg = '新增成功'
	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddAreaGame] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddAreaGame] TO [xbuser]
    AS [dbo];

