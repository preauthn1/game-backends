-- =============================================
-- Author:  alan
-- Create date: 2018/8/23
-- Description:	查询平台实时在线且充值超過10000的數量, 提供後台通知
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetOnlineVIPUser_Count]
@intAdminId int=null,
@intCount	int output			--數量
AS
BEGIN 
	

	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From Area.AreaAdminUser where AdminID=@intAdminID

	SELECT  
            @intCount = Count(B.[UserID]) 
    FROM    Game.GameUserList AS A WITH(NOLOCK)
            INNER JOIN Users.UserInfo AS B WITH(NOLOCK) ON A.UserID = B.UserID

    WHERE (B.SumCharge+B.SumAgentCharge) >= 1000000
	AND B.AreaID in (select AreaId from @Temp_AreaID)
	AND UserType <> 2
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetOnlineVIPUser_Count] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetOnlineVIPUser_Count] TO [xbuser]
    AS [dbo];

