-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<用户统计信息查询> 統計分析-用戶統計-用戶統計信息查詢
--	2018/04/09	Jason		調整為查詢LOGDB
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_UserStatInfo]
	@tnySearchType		tinyint,		--查询类型 1同IP注册数 2 同IP登录数 3同Mac注册数 4同Mac登录数
	@intCount			int				--数量
AS
BEGIN
	
	EXEC [LOGDBLINKEDSERVER].[LogDB].dbo.PrWeb_User_UserStatInfo
		@tnySearchType	= @tnySearchType,
		@intCount		= @intCount


END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_UserStatInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_UserStatInfo] TO [xbuser]
    AS [dbo];

