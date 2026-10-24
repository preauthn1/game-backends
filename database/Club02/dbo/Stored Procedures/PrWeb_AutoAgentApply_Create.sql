








-- =============================================
-- Author:  harry
-- Create date: 2018/09/4
-- Description:	新增自动邀请
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AutoAgentApply_Create]
	@chvInviteCode		varchar(20),
	@chvIPAddress		varchar(30) = NULL,
	@chvRedirectURL			Varchar(500) OUT,
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN
	SELECT ID From dbo.AgentUsers  (nolock) Where InviteCode = @chvInviteCode
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '代理不存在'
		RETURN -1
	END

	Update dbo.AutoAgentApply SET InviteCode=@chvInviteCode, CreateTime=getdate() Where IPAddress = @chvIPAddress

	--新增自动邀请
	IF @@ROWCOUNT = 0 
		INSERT INTO  dbo.AutoAgentApply(IPAddress, InviteCode) Values (@chvIPAddress, @chvInviteCode)

	SET @chvErrMsg = '送出成功!'
	RETURN 1
END




