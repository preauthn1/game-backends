-- =============================================
-- Description:	刪除平台
-- =============================================
create procedure [dbo].[PrSys_DeletePlatform]
@intPlatformID varchar(10) 
As
BEGIN



BEGIN TRAN


	delete from Area.GameList where platformID = @intPlatformID

	delete from [CenterDB].[Area].PlatformAdminUser  where platformID = @intPlatformID

	delete from [CenterDB].[Area].[Advertisement]  where platformID = @intPlatformID

	delete from [CenterDB].[Area].[Platform] where platformID = @intPlatformID

	delete from [CenterDB].[Area].[Area] where platformID = @intPlatformID


	DECLARE @chvPlatformID varchar(20)
	DECLARE @chvAndroidVersion varchar(2000)
	DECLARE @chvIOSVersion varchar(2000)
	DECLARE @chvNewAndroidVersion varchar(2000)
	DECLARE @chvNewIOSVersion varchar(2000)

	SET @chvPlatformID = Convert(varchar,@intPlatformID)

	SELECT @chvAndroidVersion=AndroidVersion,@chvIOSVersion=IOSVersion from  ServerInfo.HallVersion 

	SELECT @chvNewAndroidVersion = COALESCE(@chvNewAndroidVersion + ',', '') + [Value]
	FROM (
		select * from [dbo].[fn_Split](@chvAndroidVersion,',') where Value Not like @chvPlatformID + '_%'
	) as A

	SELECT @chvNewIOSVersion = COALESCE(@chvNewIOSVersion + ',', '') + [Value]
	FROM (
		select * from [dbo].[fn_Split](@chvIOSVersion,',') where Value Not like @chvPlatformID + '_%'
	) as A

	print @chvNewAndroidVersion
	print @chvNewIOSVersion

	UPDATE ServerInfo.HallVersion SET AndroidVersion=@chvNewAndroidVersion, IOSVersion=@chvNewIOSVersion



COMMIT TRAN
RETURN 1

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_DeletePlatform] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_DeletePlatform] TO [xbuser]
    AS [dbo];

