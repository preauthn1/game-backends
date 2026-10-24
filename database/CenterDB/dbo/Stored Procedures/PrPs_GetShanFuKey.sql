-- =============================================
-- Author:  Jason
-- Create date: 2019/04/13
-- Description:	取得閃付id key
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetShanFuKey]
	@intUserID	int
AS
BEGIN

	SET NOCOUNT ON

	select	[AccountID] = c.[ShanFuAccountID],
			[Sign] = c.[ShanFuSign],
			[SumAmount] = a.SumCharge + a.SumAgentCharge,
			[PayAgentUrl] = c.[ShanFuPayAgentUrl],
			[PayAgentClientUrl] = c.[ShanFuPayAgentClientUrl]
	from [Users].[UserInfo] as a with (nolock)
		inner join area.area as b with (nolock) on a.areaid = b.areaid
		inner join [Area].[Platform] as c with (nolock) on b.PlatformID = c.PlatformID
	where
		userid = @intUserID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetShanFuKey] TO [GameUser]
    AS [dbo];

