-- =============================================
-- Author:  alan
-- Create date: 2019/04/19
-- Description: 客服补单查询匯出
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_ExportOrderLogByCS]
(
	@intAdminID		INT,
	@chvCSName		varchar(50) = NULL, --篩選客服登入名稱
	@intUserID		INT = NULL,			--篩選玩家ID
	@intOrderID		INT = NULL,			--篩選訂單號
	@dtStartTime		DATETIME = NULL, --篩選開始時間
	@dtEndTime		DATETIME = NULL, --篩選結束時間

	@intPayTypeID	INT = NULL, --篩選支付類型
	@intGWID        INT = NULL --篩選支付平台商
)
AS
BEGIN

	SET NOCOUNT ON


	IF @dtStartTime is null 
	BEGIN
		SET @dtStartTime = convert(date, getdate())
		SET @dtEndTime = null		
	END

	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From Area.AreaAdminUser where AdminID=@intAdminID

	select 
	ROW_NUMBER() OVER (ORDER BY L.ID desc) AS rowNum,		
	L.*,O.PayTypeID,O.GWID
	,P.TypeName
	,G.GatewayName
	from pay.CSOrderLog L with (nolock)
	left join pay.OrderList O with (nolock) on L.OrderID=O.ID
	left join pay.PayType P with (nolock) on O.PayTypeID=P.TypeID and O.GWID=P.GWID 
	left join pay.PayGateway G with (nolock) on G.ID=O.GWID
	left join Users.UserInfo U with (nolock) on U.UserID=O.UserID
	where
	(@chvCSName IS NULL OR L.CSName=@chvCSName)
	AND (@intUserID IS NULL OR L.UserID = @intUserID)
	AND (@intOrderID IS NULL OR L.OrderID = @intOrderID)
	AND (@dtStartTime IS NULL OR L.CreateTime >= @dtStartTime)
	AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
	AND (@intPayTypeID IS NULL OR O.PayTypeID = @intPayTypeID)
	AND (@intGWID IS NULL OR O.GWID = @intGWID)
	AND U.AreaID in (select AreaID from @Temp_AreaID)

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_ExportOrderLogByCS] TO [GameUser]
    AS [dbo];

