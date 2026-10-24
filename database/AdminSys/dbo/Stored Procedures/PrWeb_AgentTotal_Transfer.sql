
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<总代理转账>

-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentTotal_Transfer]
(
	@intOperatorID			INT,					--操作者ID
	@chvOperatorName		VARCHAR(128),			--操作者名称
	@intAgentID				INT,					--代理ID
	@lngAmount				BIGINT,					--修改数量
	@chvIP					VARCHAR(128),			--IP
	@chvErrMsg				VARCHAR(128) OUTPUT		--返回消息
)
AS
BEGIN
	SET NOCOUNT ON
	set xact_abort on
	DECLARE @intReturnValue		INT
	DECLARE @intAgentTotalID		INT
	DECLARE @intoOldMoney		INT
	

	SELECT	@intAgentTotalID = A.id,
			@intoOldMoney = A.money
	FROM agent_total AS A WITH (NOLOCK)
	WHERE id = @intAgentID

	IF @intAgentTotalID IS NULL
	BEGIN
		SET @chvErrMsg = '代理不存在'
		RETURN -1
	END

	--更新现金
	UPDATE agent_total
	SET money = money + @lngAmount
	WHERE id = @intAgentID

	SET @intReturnValue = @@ROWCOUNT

	IF @intReturnValue<>1
	BEGIN
		SET @chvErrMsg = '操作失败:添加金币失败'
		ROLLBACK TRAN
		RETURN -1
	END

	INSERT INTO agent_money_total (total_agent_id, type, user_agent_id,money,old_money,new_money,created_at,remark)
	VALUES (@intAgentID, 2, @intAgentID, @lngAmount, @intoOldMoney, @lngAmount + @intoOldMoney, GETDATE(),'运营(' + @chvOperatorName + ')-转账-充值');

	INSERT INTO agent_recharge_total (manage_id, total_agent_id, num, created_at,old_money,new_money,ip, area, device, remark)
	VALUES (@intOperatorID, @intAgentID, @lngAmount, GETDATE(), @intoOldMoney, @lngAmount + @intoOldMoney, @chvIP, '保留地址',1,'运营(' + @chvOperatorName + ')-转账-充值');

	SET @chvErrMsg = '操作成功'
	RETURN @intReturnValue
END
