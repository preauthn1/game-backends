-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: 前端API查询邀请码对应用户QR CODE
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetUserQRCode]	
(
	@chvInviteCode	varchar(10)
)
AS
BEGIN

	set nocount on

	select	QRURL = [Domain] + '?a=' + [QRCode]
	from [dbo].[UserAgentQRCode] as a with (nolock)
		inner join [dbo].[DomainSetting] as b with (nolock) on a.DomainID = b.ID
	where
		[QRCode] = @chvInviteCode

	return 1
END
