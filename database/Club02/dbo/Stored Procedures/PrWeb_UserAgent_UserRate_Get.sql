-- =============================================
-- Author:  jason
-- Create date: 2019/04/13
-- Description: 全民代理后台 查询一级下线比例
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_UserRate_Get]
(
@intUserID		int,				--用户 ID
@intSubUserID	int,				--调整用户 ID
@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	--if not exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where userid = @intSubUserID and TopUserID = @intUserID)
	--begin
	--	set @chvMsg	= '操作错误 : 无法越级调整比例'
	--	return -1
	--end

	select	[UserID] = @intSubUserID,
			[UserLevel],
			[Stage1],
			[Stage2],
			[Stage3]
	from [dbo].[UserAgentUserRate] with (nolock)
	where
		userid = @intSubUserID

	set @chvMsg	= '操作完成'
	return 1
END







