


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新GameList>
-- 2018/07/12  Emily #583 后台新增广告与游戏热门程度管理 -- 新增欄位Popular
-- 2018/11/29 alan	增加AliasName,SetDefault欄位
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdateAreaGame]
@intID INT,
@intSort INT,	
@tintStatus	tinyint,	--状态
@tintPopular	tinyint,            --熱門程度
@chvErrMsg	VARCHAR(256) OUTPUT		--返回消息

AS
BEGIN
	--更新[GameList]
	UPDATE [CenterDB].[Area].[GameList]
	SET Sort=@intSort,
		Status=@tintStatus,
		Popular=@tintPopular
	WHERE [ID] = @intID

	SET @chvErrMsg = '新增成功'
	RETURN 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateAreaGame] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateAreaGame] TO [xbuser]
    AS [dbo];

