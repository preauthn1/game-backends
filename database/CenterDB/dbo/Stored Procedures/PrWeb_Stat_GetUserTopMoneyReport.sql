-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<按金币总量排行获取用户金币报表>
-- Changelog:
--		20180410	alan 修正bug, U.[UserID] as 'User.UserID' -> U.[UserID] as 'User.ID' 
--					alan 增加 U.[GrandID]
--					alan 增加 B.Amount
--		20190515	Jason		 濾掉機器人帳號
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetUserTopMoneyReport]
	@longAmount     BIGINT=null,		--最大金币数量  如果<=0则查询金币总量<@longAmount 的前N名
	@intCount		INT					--前N名
AS
BEGIN
	
	if @longAmount is null or @longAmount <=0
	begin	
		SELECT T.* 
				,U.[UserID] as 'User.ID' 
				,U.[LoginName] as 'User.LoginName'
				,U.[NickName]  as 'User.NickName'
				,'***' as 'User.Password' 
				,U.[Sex] as 'User.Sex'
				,U.[HDType]  as 'User.HDType'
				,U.[HDCode] as 'User.HDCode'
				,U.[UserType] as 'User.UserType'
				,U.[GradeID] as 'User.GradeID'
				,U.[CreateTime] as 'User.CreateTime'
				,'***' as 'User.CreateIPAddress'
				,U.[LastLogonTime] as 'User.LastLogonTime'
				,'***' as 'User.LastLogonIPAddress'
				,e.Name as 'User.Area.Platform.Name'
				,B.Amount as 'bankAmount'
		FROM (
			SELECT TOP (@intCount)
				C.UserID
				,C.Amount AS CashAmount 
			FROM Money.Cash AS C WITH(NOLOCK) 			
			ORDER BY (C.Amount) DESC
		) AS T
		INNER JOIN Users.UserInfo AS U ON T.UserID = U.UserID and U.UserType <> 2
		INNER JOIN Area.Area d on U.AreaID = d.AreaID
		INNER JOIN Area.Platform e on d.PlatformID = e.PlatformID
		LEFT JOIN Money.Bank B ON B.UserID = U.UserID
		ORDER BY CashAmount  DESC
	end
	else
	begin
		SELECT T.* 
				,U.[UserID] as 'User.ID' 
				,U.[LoginName] as 'User.LoginName'
				,'***' as 'User.Password' 
				,U.[Sex] as 'User.Sex'
				,U.[HDType] -1 as 'User.HDType'
				,U.[HDCode] as 'User.HDCode'
				,U.[UserType] as 'User.UserType'
				,U.[GradeID] as 'User.GradeID'
				,U.[CreateTime] as 'User.CreateTime'
				,'***' as 'User.CreateIPAddress'
				,U.[LastLogonTime] as 'User.LastLogonTime'
				,'***' as 'User.LastLogonIPAddress'
				,e.Name as 'User.Area.Platform.Name'
				,B.Amount as 'bankAmount'
		FROM (
			SELECT TOP (@intCount)
				C.UserID
				,C.Amount AS CashAmount
			FROM Money.Cash AS C WITH(NOLOCK)	
			WHERE (C.Amount) <= @longAmount			
			ORDER BY (C.Amount) DESC
		) AS T
		INNER JOIN Users.UserInfo AS U ON T.UserID = U.UserID and U.UserType <> 2
		INNER JOIN Area.Area d on U.AreaID = d.AreaID
		INNER JOIN Area.Platform e on d.PlatformID = e.PlatformID
		LEFT JOIN Money.Bank B ON B.UserID = U.UserID
		ORDER BY CashAmount  DESC
	end
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetUserTopMoneyReport] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetUserTopMoneyReport] TO [xbuser]
    AS [dbo];

