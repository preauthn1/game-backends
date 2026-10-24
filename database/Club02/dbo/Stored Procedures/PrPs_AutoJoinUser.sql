






-- =============================================
-- Author:  alan
-- Create date: 2018/9/4
-- Description:	自動綁定下線
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AutoJoinUser]
	@intUserID			INT, --被綁定的用戶ID
	@chvIPAddress		VARCHAR(20),
	@chvErrMsg			Varchar(50) OUT
AS
BEGIN

	Declare @chvInviteCode varchar(10)

	--取15分鐘內有效的邀請碼
	Select @chvInviteCode=InviteCode from AutoAgentApply with (nolock) where IPAddress=@chvIPAddress and CreateTime >= DATEADD(MINUTE,-15,GETDATE())
	IF @@ROWCOUNT > 0
	BEGIN

		DECLARE	@return_value int
		EXEC	@return_value = [dbo].[PrPs_JoinUser]
				@intUserID = @intUserID,
				@chvInviteCode = @chvInviteCode,
				@chvErrMsg = @chvErrMsg OUTPUT

		Return @return_value

	END

	SET @chvErrMsg = '找不到有效資料'
	RETURN -1

END


