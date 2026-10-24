-- =============================================
-- Author:  alan
-- Create date: 2018-8-14
-- Description:	更新节点登入记录
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UpdateProtectNodeStatus]
@chvToken		varchar(10),
@intLoginUserID	int = null,
@intStatus		tinyint --0无 1正常登入 2登入失败
AS
BEGIN

	Update [Game].[ProtectNodeLog] SET [status] = @intStatus, LoginUserID = @intLoginUserID where Token = [REDACTED]

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UpdateProtectNodeStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UpdateProtectNodeStatus] TO [xbuser]
    AS [dbo];

