-- =============================================
-- Author:  jason
-- Create date: 2019/05/06
-- Description: USER 绑定确认(裂变)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_BindingCheck]
(
	@intUserID		int,				--用户 ID
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @intRtnCode int

	exec [dbo].[PrWeb_Split_CheckUserMission]
		@intUserID = @intUserID,	--用户 ID
		@intRtnCode = @intRtnCode OUTPUT,
		@chvMsg = @chvMsg OUTPUT


	if(@intRtnCode = 1)
	begin
		exec [dbo].[PrWeb_Split_GiveBonusLog]
			@intUserID = @intUserID,			--用户 ID
			@chvMsg = @chvMsg	OUTPUT
	end

	return 1
END









