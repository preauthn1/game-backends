
-- =============================================
-- Author:  alan
-- Create date: 20181005
-- Description: 渠道包下載頁更新
-- Changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePlatformDownload]
@intAdminID			INT=null,
@intPlatformID 		INT,
@chvIOS				varchar(256)=null,
@chvAndroid			varchar(256)=null,
@chvOfficial		varchar(256)=null,
@chvIOSMemo			varchar(256)=null,
@intIOSTerminate	int=null,  --IOS是否下架 1 已經下架 0上架
@intCorrect			int=null,   --是否正確 1正確 0不正確
@chvMemo			varchar(256) = null

AS
BEGIN

	SET NOCOUNT ON
	
	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID


	if @chvIOS not like 'http://%' and @chvIOS not like 'https://%'
	begin
		set @chvIOS = 'http://' + @chvIOS
	end

	if @chvAndroid not like 'http://%' and @chvAndroid not like 'https://%'
	begin
		set @chvAndroid = 'http://' + @chvAndroid
	end

	if @chvOfficial not like 'http://%' and @chvOfficial not like 'https://%'
	begin
		set @chvOfficial = 'http://' + @chvOfficial
	end



	Update [Area].[PlatformDownload] 
	SET 
	IOS=ISNULL(@chvIOS,IOS), Android=ISNULL(@chvAndroid,Android), Official=ISNULL(@chvOfficial,Official),
	IOSMemo=ISNULL(@chvIOSMemo,IOSMemo),
	IOSTerminate=ISNULL(@intIOSTerminate,IOSTerminate),
	Correct=ISNULL(@intCorrect,Correct), 
	UpdateTime=Getdate(),
	Memo=ISNULL(@chvMemo,Memo)
	WHERE PlatformID=@intPlatformID
	AND PlatformID in (select PlatformID from @Temp_PlatformID)

	IF @@ROWCOUNT=0
		INSERT INTO [Area].[PlatformDownload] (PlatformID, IOS, Android, Official, IOSTerminate, IOSMemo, Correct, Memo)
		VALUES(@intPlatformID,@chvIOS,@chvAndroid,@chvOfficial,@intIOSTerminate,@chvIOSMemo,@intCorrect, @chvMemo)

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdatePlatformDownload] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdatePlatformDownload] TO [xbuser]
    AS [dbo];

