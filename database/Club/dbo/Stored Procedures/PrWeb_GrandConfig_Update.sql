





-- =============================================
-- Author:  harry
-- Create date: 2018/08/13
-- Description:	更新查询代理阶级设定
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GrandConfig_Update]
	@OperateUID		uniqueidentifier,  --操作者
	@GrandUID		uniqueidentifier = NULL, --流水編號
	@ParentUID		uniqueidentifier, --父代理
	@UserUID		uniqueidentifier, --子代理
	@intDistributeRate		INT,
	@chvErrMsg			Varchar(50) OUT
AS
BEGIN	
	DECLARE @oldRate INT --原比例
	DECLARE @chvOldData varchar(100) --旧的资料
	DECLARE @chvNewData varchar(100) --旧的资料

	DECLARE @tnyClass tinyint
	SELECT @tnyClass=Class FROM dbo.AgentUsers with (nolock) WHERE ID = @UserUID

	--檢查比例是否合規, 不能大於上級比例, 不能小於所有下級的最大值
	IF @intDistributeRate IS NOT NULL
	BEGIN
		--從渠道代理檢查
		IF  @tnyClass=1 and @intDistributeRate >= (select DistributeRate from AgentRateRange with (nolock) where ID = @ParentUID)
		BEGIN
			SET @chvErrMsg = '错误：分成比例不能大于等于上级代理'
			RETURN -1
		END
		--從一般代理檢查
		IF  @tnyClass in (2,3,4) and @intDistributeRate >= (select DistributeRate from GrandConfig with (nolock) where AgentID = @ParentUID)
		BEGIN
			SET @chvErrMsg = '错误：分成比例不能大于等于上级代理'
			RETURN -1
		END
		--從一般代理檢查
		IF  @tnyClass in (0,1,2,3) and @intDistributeRate <= (select top 1 Max(DistributeRate) from GrandConfig with (nolock) where ParentID = @UserUID)
		BEGIN
			SET @chvErrMsg = '错误：分成比例不能小于等于下级代理'
			RETURN -1
		END

	END

	--檢查旧比例
	SELECT @oldRate = DistributeRate From dbo.GrandConfig with (nolock) Where AgentID = @UserUID AND ParentID = @ParentUID AND ID = @GrandUID

	IF @intDistributeRate <> @oldRate
	BEGIN
		--獲取變更前資料
		SELECT @chvOldData ='收益比例=' + Convert(varchar,DistributeRate) FROM GrandConfig where ID = @GrandUID

		UPDATE dbo.GrandConfig 
		SET DistributeRate = @intDistributeRate
		WHERE ID = @GrandUID
		SET @chvErrMsg = '更新阶级成功'


		--紀錄變更
		SET @chvNewData = '收益比例=' + Convert(varchar,@intDistributeRate)
		EXEC [dbo].[PrWeb_AddChangeLog] 
		@intChangeType = 4,
		@OperateUID = @OperateUID,
		@TargetUID = @UserUID,
		@chvOldData = @chvOldData,
		@chvNewData = @chvNewData,
		@chvMemo = ''
	END

	RETURN 1
END



