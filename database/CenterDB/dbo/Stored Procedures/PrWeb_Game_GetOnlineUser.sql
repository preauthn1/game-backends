-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询平台实时在线人数>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetOnlineUser]
    @intUserID INT = NULL ,
    @intGameID INT = NULL ,
    @intServerID INT = NULL ,
    @intPageIndex INT = 1 ,
    @intPageSize INT = 50 ,
    @intPageCount INT OUTPUT			--记录总数		
AS
    BEGIN 
	
        SELECT  @intPageCount = COUNT(1)
        FROM    Game.GameUserList AS A WITH(NOLOCK)
                INNER JOIN Users.UserInfo AS B ON A.UserID = B.UserID
                LEFT JOIN Money.Bank AS C ON C.UserID = B.UserID
                LEFT JOIN Money.Cash AS D ON D.UserID = B.UserID
                LEFT JOIN ServerInfo.GameServerList AS E ON E.ServerID = A.ServerID
                LEFT JOIN ServerInfo.GameList AS F ON F.GameID = E.GameID
        WHERE   ( @intUserID IS NULL OR @intUserID = A.UserID )
                AND ( @intGameID IS NULL OR @intGameID = E.GameID )
                AND ( @intServerID IS NULL OR @intServerID = A.ServerID );
            WITH    tab
                      AS ( SELECT   ROW_NUMBER() OVER ( ORDER BY A.UserID  ) AS rowNum ,
                                    B.[UserID] ,
                                    B.[LoginName]  ,
                                    E.ServerName ,
                                    F.GameName,
                                    C.Amount [BankAmount] ,
                                    D.Amount [CashAmount] ,
                                    B.[Sex] ,
                                    B.[CreateTime],
                                    B.[HDType] ,
                                    B.[CreateIPAddress] ,
                                    B.[LastLogonTime]  ,
                                    B.LastLogonHDType ,
                                    B.[LastLogonIPAddress] 
                           FROM     Game.GameUserList AS A WITH(NOLOCK)
                                    INNER JOIN Users.UserInfo
                                    AS B ON A.UserID = B.UserID
                                    LEFT JOIN Money.Bank AS C ON C.UserID = B.UserID
                                    LEFT JOIN Money.Cash AS D ON D.UserID = B.UserID
                                    LEFT JOIN ServerInfo.GameServerList AS E ON E.ServerID = A.ServerID
                                    LEFT JOIN ServerInfo.GameList AS F ON F.GameID = E.GameID
                          WHERE   ( @intUserID IS NULL OR @intUserID = A.UserID )
								AND ( @intGameID IS NULL OR @intGameID = E.GameID )
								AND ( @intServerID IS NULL OR @intServerID = A.ServerID )
                         )
            SELECT  *
            FROM    tab
            WHERE   rowNum BETWEEN @intPageSize * ( @intPageIndex - 1 ) + 1
                           AND     @intPageSize * @intPageIndex;
	
    END;

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetOnlineUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetOnlineUser] TO [xbuser]
    AS [dbo];

