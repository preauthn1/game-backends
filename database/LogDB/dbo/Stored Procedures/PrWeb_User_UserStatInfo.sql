-- =============================================
-- Author:  Jason
-- Create date: 2018/04/09
-- Description:	統計分析-用戶統計-用戶統計信息查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_UserStatInfo]
	@tnySearchType		TINYINT,		--查询类型 1同IP注册数 2 同IP登录数 3同Mac注册数 4同Mac登录数
	@intCount			INT				--数量
AS
BEGIN
	
	IF @tnySearchType = 1
	BEGIN
		SELECT [IPAddress] as Info,COUNT(1) as Amount
		FROM [Users].[UserRegLog] WITH (NOLOCK)
		GROUP BY [IPAddress]
		HAVING COUNT(1)>@intCount
		ORDER By Amount DESC
	END
	ELSE IF @tnySearchType = 2
	BEGIN
		SELECT [IPAddress] AS Info,COUNT(1) as Amount
		FROM [Users].[UserLogonLog] WITH (NOLOCK)
		GROUP BY [IPAddress]
		HAVING COUNT(1) > @intCount
		ORDER By Amount DESC
	END
	ELSE IF @tnySearchType = 3
	BEGIN
		SELECT HDCode as Info,COUNT(1) as Amount
		FROM [Users].[UserRegLog] WITH (NOLOCK)
		GROUP BY HDCode
		HAVING COUNT(1)>@intCount
		ORDER By Amount DESC
	END
	ELSE IF @tnySearchType = 4
	BEGIN
		SELECT [HDCode] AS Info,COUNT(1) as Amount
		FROM [Users].[UserLogonLog] WITH (NOLOCK)
		GROUP BY [HDCode]
		HAVING COUNT(1) > @intCount
		ORDER By Amount DESC
	END
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_UserStatInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_UserStatInfo] TO [xbuser]
    AS [dbo];

