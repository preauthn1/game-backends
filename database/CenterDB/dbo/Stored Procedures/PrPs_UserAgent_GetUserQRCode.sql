-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 取得用戶qr code
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetUserQRCode]	
@intUserID		int				--用戶 ID
AS
BEGIN

	set nocount on

	declare @temp_userinfo	table ([UserID] int,[QRCode] varchar(250),[InviteCode] varchar(8),UpperUserID int)

	insert into @temp_userinfo
	EXEC [Club02].[dbo].[PrPs_UserAgent_GetUserQRCode]	
		@intUserID = @intUserID

	select	[UserID],
			[QRCode],
			[InviteCode],
			UpperUserID
	from @temp_userinfo

	return 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_GetUserQRCode] TO [GameUser]
    AS [dbo];

