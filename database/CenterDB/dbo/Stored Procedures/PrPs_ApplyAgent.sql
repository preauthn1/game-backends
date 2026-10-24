-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<申请代理>
-- Change:
--	20180716	alan	新增支付寶資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_ApplyAgent]
	@intUserID	int,				--申请用户ID
	@intAreaID	int,				--渠道ID
	@chvName	varchar(32),
	@chvTel		varchar(20),
	@chvQQ		varchar(20),
	@chvWXNo	varchar(20),
	@chvAlipay	varchar(50),
	@chvMemo	varchar(256),
	@chvErrMsg	varchar(128) OUTPUT
AS
BEGIN
	INSERT INTO Agent.ApplyAgentInfo(UserID,PlatformID,Name,Tel,QQ,WXNo,Alipay,Memo,CreateTime)
	SELECT @intUserID, PlatformID,@chvName,@chvTel,@chvQQ,@chvWXNo,@chvAlipay,@chvMemo,GETDATE()
	from Area.Area where AreaID=@intAreaID
	set @chvErrMsg = '申请成功'
	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_ApplyAgent] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_ApplyAgent] TO [xbuser]
    AS [dbo];

