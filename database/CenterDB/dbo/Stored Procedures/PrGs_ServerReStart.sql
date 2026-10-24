-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<游戏服务器重新启动>
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_ServerReStart]
    @chvIPAddress VARCHAR(128),                 --服务器IP
    @intPort INT,                              --端口号 
    @sintServerID SMALLINT,				--服务器ID,
    @chvDomainName VARCHAR(128) ,		--域名
    @chvErrMsg VARCHAR(128) OUTPUT
AS
    SET NOCOUNT ON; 

    DECLARE @chvServerName VARCHAR(128);

	--获取服务器ID
    SELECT  @sintServerID = ServerID
    FROM    ServerInfo.GameServerList
    WHERE   ServerID = @sintServerID;

	--更新服务器信息
    IF @sintServerID IS NOT NULL
        UPDATE  ServerInfo.GameServerList
        SET     @chvServerName = ServerName ,                
                LastUpdateTime = GETDATE()
        WHERE   ServerID = @sintServerID;
    ELSE
        BEGIN
            SET @sintServerID = 0;

            SET @chvErrMsg = '服务器启动失败:不存在服务器配置信息';
            RETURN 0;
        END;

	--更新金币源
    IF NOT EXISTS ( SELECT  1
                    FROM    Stat.MoneySourceType
                    WHERE   SourceID = @sintServerID )
				INSERT  INTO Stat.MoneySourceType
						( SourceID ,
						  SourceName ,
						  ChangeType ,
						  GroupID
						)
						SELECT  @sintServerID ,
								@chvServerName ,
								1 ,
								2;
	
    SET @chvErrMsg = '服务器启动成功';

    RETURN 1;

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ServerReStart] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ServerReStart] TO [xbuser]
    AS [dbo];

