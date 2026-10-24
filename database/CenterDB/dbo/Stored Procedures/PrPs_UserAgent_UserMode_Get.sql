-- =============================================
-- Author:  jason
-- Create date: 2019/04/19
-- Description: USER 取得已開的模式
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_UserMode_Get]
(
	@intUserID		int,				--用户 ID
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	EXEC [Club02].[dbo].[PrPs_UserAgent_UserMode_Get]
		@intUserID = @intUserID,
		@chvMsg = @chvMsg OUTPUT

	return 1
END









GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserAgent_UserMode_Get] TO [GameUser]
    AS [dbo];

