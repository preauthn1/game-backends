-- =============================================
-- Author:		Jason
-- Create date: 2018/03/22
-- Description:	批量更新兌換紀錄並回傳給Center
--	2018/04/24	Jason		新增@chvUserName參數及寫入[Operator] = @chvUserName
--	2018/05/01	alan		變更回傳欄位方式, 取消字串組, 改為各欄位回傳
--							(CASE	WHEN ConvertType = 1 THEN '支付寶-' ELSE '银聯卡-' END) + Account AS PayAccount
--	2018/05/11	alan		依營運需求, 更改回傳規格
--	2018/05/23	alan		只支援支付寶, 回傳amount不使用 '分'
--  2019/07/17  CCC			增加 @intGWID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_PullConvertLog]
(
	@tnyFlag		tinyint,			--兌換狀態 
	@chvNumber		varchar(100),		--篩選方式 (AL：所有记录，JS：所有奇数记录，OS：所有偶数记录，"1,2":表示获取尾数为1或者2的所有记录)
	@chvUserName	VARCHAR(16) = null,	--操作者
	@intGWID		INT = null			--代付第三方
)
AS
BEGIN
	SET NOCOUNT ON

	--DECLARE @Temp_ConvertLogID	TABLE (ID INT)

	/*  更新紀錄  */
	------------------------------------------------------
	--UPDATE TOP (@intCount) [Money].[ConvertLog]
	--SET Status = 99,
	--	LastUpdateTime = GETDATE(),
	--	[Operator] = @chvUserName
	--OUTPUT INSERTED.Id INTO @Temp_ConvertLogID
	--WHERE
	--	Status = 2
	------------------------------------------------------
	
	
	/*  篩選條件轉換  */
	------------------------------------------------------
	--Flag是第三方自動兌換程式狀態, 對應內部Stats狀態
	--提出兌換
	--IF @tnyFlag = 0 Set @tnyFlag = 0
	
	----疑似洗錢
	--IF @tnyFlag = 1 Set @tnyFlag = 9
	
	----審核通過
	--IF @tnyFlag = 2 Set @tnyFlag = 2

	----已拒絕
	--IF @tnyFlag = 3 Set @tnyFlag = 8

	----打款失敗
	--IF @tnyFlag = 4 Set @tnyFlag = 4

	----兌換成功
	--IF @tnyFlag = 6 Set @tnyFlag = 3

	----兌換失敗
	--IF @tnyFlag = 7 Set @tnyFlag = 4


	--取全部記錄
	IF @chvNumber = 'AL' 
	BEGIN
		SET @chvNumber = '%[1,2,3,4,5,6,7,8,9,0]'
	END	
	--JS：所有奇数记录
	IF @chvNumber = 'JS' 
	BEGIN
		SET @chvNumber = '%[1,3,5,7,9]'
	END
	--OS：所有偶数记录
	IF @chvNumber = 'OS' 
	BEGIN
		SET @chvNumber = '%[2,4,6,8,0]'
	END


	/*  回傳紀錄
	------------------------------------------------------
	id varchar(20), --自增id，也是订单id
	user_id varchar(20),--玩家id
	gold_pre varchar(20), --提现前金额
	gold_next  varchar(20),--提现后金额
	gold_bank  varchar(20), --银行金额
	gold_req int, -- 当前订单金额
	gold_req_get varchar(20), --已到账金额
	fee bigint, -- 手续费
	ip  varchar(20), --玩家ip
	alipay_account varchar(20), --支付宝账户
	alipay_name varchar(20), --支付宝名字
	union_pay_account varchar(20), --银行卡账户
	union_pay_name varchar(20), --银行卡名字
	total_req varchar(20), --积累申请兑换金额
	total_get varchar(20), --积累兑换金额
	recharge_total varchar(20), --总充值金额
	recharge_delegate varchar(20), --代理充值金额
	time_start varchar(20), --提交时间
	time_end varchar(20), --到账时间
	[status] varchar(5), --订单状态 0.兑换完成 1.审核中 2.已审核 3.已打款 4.订单异常 5. 异常帐号无法提现 6.黑名单账户无法提现 7.提现账户或名字错误 8.拒绝兑换 9.审核中，可能是刷分账户
	link_id varchar(5), --连接erlang表的id
	exchange_account_type tinyint, -- 1-支付宝，2-银行卡
	total_win_lose varchar(20), --总输赢
	union_pay_bank varchar(20), --银行
	exchange_black varchar(1), --是否兑换黑名单 0.不是 1.是
	operator varchar(20), --操作人员
	export tinyint, --是否导出1是0否
	[file_name] varchar(20) --导出的文件名
	*/
	SELECT	ID,
			Convert(varchar,UserID) as user_id,
			'' as gold_pre,
			'' as gold_next,
			ISNULL(BankAmount,0) as gold_bank,
			ABS(Amount/100) as gold_req,
			'' as gold_req_get,
			TaxAmount as fee,
			CreateIP as ip,
			(CASE WHEN ConvertType = 1 THEN Account ELSE '' END) AS alipay_account,
			(CASE WHEN ConvertType = 1 THEN Realname ELSE '' END) AS alipay_name,
			(CASE WHEN ConvertType = 1 THEN '' ELSE Account END) AS union_pay_account,
			(CASE WHEN ConvertType = 1 THEN '' ELSE Realname  END) AS union_pay_name,
			'' as total_req,
			'' as total_get,
			'' as recharge_total,
			'' as recharge_delegate,
			Convert(varchar,CreateTime,120) as time_start,
			'' as time_end,
			[status],
			'' as link_id,
			ConvertType as exchange_account_type,
			'' as total_win_lose,
			'' as union_pay_bank,
			''  as exchange_black,
			ISNULL(Operator,'') as operator,
			0 as export,
			'' as [file_name]
	   FROM [Money].[ConvertLog] as C WITH (NOLOCK)
	  WHERE Id LIKE @chvNumber
	    AND [Status] = @tnyFlag
	    AND ConvertType = 1		--支付宝
	    AND C.GWID = @intGWID
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_PullConvertLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_PullConvertLog] TO [xbuser]
    AS [dbo];

