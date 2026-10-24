







-- =============================================
-- Author:  harry
-- Create date: 2018/08/16
-- Description:	更新操作者密码
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentUser_Update_Password]
	@UID				uniqueidentifier,
	@chvPassword		varchar(100),
	@chvEncrypt			varchar(50),
	@chvErrMsg			varchar(50) OUT
AS
BEGIN	

	DECLARE @chvOldData varchar(100) 
	SELECT @chvOldData = dbo.Fn_HideData(Password) from dbo.AgentUsers with (nolock) WHERE ID = @UID


	UPDATE dbo.AgentUsers 
	SET Password = [REDACTED], Encrypt = @chvEncrypt
	WHERE ID = @UID
	SET @chvErrMsg = '更新成功'


	DECLARE @chvNewData varchar(100) = dbo.Fn_HideData(@chvPassword)
	--紀錄變更	
	EXEC [dbo].[PrWeb_AddChangeLog] 
	@intChangeType = 1,
	@OperateUID = @UID,
	@TargetUID = null,
	@chvOldData = @chvOldData,
	@chvNewData = @chvNewData,
	@chvMemo = ''


	RETURN 1
END



