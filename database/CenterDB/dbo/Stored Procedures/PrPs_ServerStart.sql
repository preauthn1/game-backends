-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<广场启动>
--	2018/03/26	Jason		新增@chvHttpAddr參數
--	2018/07/2	alann		新增獲取熱更與支付域名
--	2018/09/11	alann		新增club域名
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_ServerStart]
@sintServerID smallint,
@chvClientAddr VARCHAR(32),
@chvNotifyAddr VARCHAR(32),
@chvHttpAddr VARCHAR(32)
AS
SET NOCOUNT ON
UPDATE ServerInfo.PlazaServerList
SET ClientAddr = @chvClientAddr,
NotifyAddr = @chvNotifyAddr,
HttpAddr = @chvHttpAddr,
LastUpdateTime = GETDATE()
WHERE ServerID = @sintServerID

IF @@ROWCOUNT = 0
INSERT INTO ServerInfo.PlazaServerList
        ( ServerID ,
          ClientAddr,
		  NotifyAddr,
		  HttpAddr,
          LastUpdateTime
        )
SELECT
	@sintServerID,
	@chvClientAddr,
	@chvNotifyAddr,
	@chvHttpAddr,
	GETDATE()

--清除广场在线数据
DELETE Game.HallUserList WHERE ServerID = @sintServerID

--获取客户端平台版本号
SELECT * FROM ServerInfo.HallVersion with (nolock)
--獲取熱更與支付域名
SELECT  DLUpdate as UpdateDomain, Payment as BillDomain, Club as ClubDomain, Customer as CustomerDomain from CenterDB.ServerInfo.DomainConfig with (nolock) WHERE Company = 1
--獲取渠道包相關資訊
SELECT AreaID, ISNULL(AreaConfig,'') From Area.Area with (nolock)

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_ServerStart] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_ServerStart] TO [xbuser]
    AS [dbo];

