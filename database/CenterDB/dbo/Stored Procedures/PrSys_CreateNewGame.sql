-- =============================================
-- Description:	建立新遊戲与服务器
-- =============================================
CREATE procedure [dbo].[PrSys_CreateNewGame]
@intGameID int,
@chvGameName varchar(20), --游戏名称
@chvAndroidVersion varchar(20) = '1.0.0',
@chvIOSVersion varchar(20) = '1.0.0',
@chvShortName varchar(20),
@intStatus int=0
As
BEGIN

IF EXISTS(SELECT 1 FROM [ServerInfo].[GameList] WHERE GameID = @intGameID)
BEGIN
	PRINT '游戏號重複'
	RETURN 
END



BEGIN TRAN

	Insert into [ServerInfo].[GameList] (GameID,GameName,AndroidVersion,IOSVersion,ShortName,Status)
	values(@intGameID, @chvGameName, @chvAndroidVersion, @chvIOSVersion, @chvShortName, @intStatus)
	
	DECLARE @chvNum varchar(3)
	--DECLARE @chvServerID varchar(10)
	--DECLARE @intServerID int
	DECLARE @chvServerName varchar(50)
	DECLARE @intLastServerID int

	

	--SET identity_insert [ServerInfo].[GameServerList] on

	--A服
	DECLARE @intNum int = 1
	WHILE @intNum <= 5
	BEGIN
		SET @chvNum = CONVERT(varchar,@intNum) 
		SELECT @intLastServerID=Max(serverID)+1 FROM  [ServerInfo].[GameServerList] with (nolock)
		SET @chvServerName = @chvGameName + 'A_' + @chvNum

		IF EXISTS(SELECT 1 FROM  [ServerInfo].[GameServerList] WHERE ServerID=@intLastServerID) 
		BEGIN  
			PRINT 'ServerID重複'
			ROLLBACK TRAN
			RETURN
		END

		Insert into[ServerInfo].[GameServerList] (ServerID,GameID,ServerName,ShowName,Money,LastUpdateTime) 
		values (@intLastServerID ,@intGameID, @chvServerName, @chvNum, 1000*@intNum, getdate())
		
		SET @intNum = @intNum + 1
	END


	--B服
	SET @intNum  = 1
	WHILE @intNum <= 5
	BEGIN
		SET @chvNum = CONVERT(varchar,@intNum) 
		SELECT @intLastServerID=Max(serverID)+1 FROM  [ServerInfo].[GameServerList] with (nolock)
		SET @chvServerName = @chvGameName + 'B_' + @chvNum

		IF EXISTS(SELECT 1 FROM  [ServerInfo].[GameServerList] WHERE ServerID=@intLastServerID) 
		BEGIN  
			PRINT 'ServerID重複'
			ROLLBACK TRAN
			RETURN
		END

		Insert into[ServerInfo].[GameServerList] (ServerID,GameID,ServerName,ShowName,Money,LastUpdateTime) 
		values (@intLastServerID ,@intGameID, @chvServerName, @chvNum, 1000*@intNum, getdate())
		
		SET @intNum = @intNum + 1
	END



	--SET identity_insert [ServerInfo].[GameServerList] off

	if not exists (select 1 from area.GameAliasName where gameid = @intGameID)
		insert into area.GameAliasName(gameid,IconName,IconTitle,Skin,AliasName) values (@intGameID,'0','0','',@chvGameName)

COMMIT TRAN
RETURN 1

END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_CreateNewGame] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_CreateNewGame] TO [xbuser]
    AS [dbo];

