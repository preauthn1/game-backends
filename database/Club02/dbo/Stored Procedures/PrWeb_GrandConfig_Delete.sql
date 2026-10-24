







-- =============================================
-- Author:  harry
-- Create date: 2018/08/13
-- Description:	刪除查询代理阶级设定
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GrandConfig_Delete]
	@OperateUID		uniqueidentifier,  --操作者
	@GrandUID		uniqueidentifier,
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN	
	DECLARE @intLevel INT
	DECLARE @chvGradeName varchar(50)
	DECLARE @intRate INT
	DECLARE @AgentUID uniqueidentifier
	
	--檢查检查下级是否有使用到此阶级
	SELECT @AgentUID = AgentID, @intLevel = Level, @chvGradeName = GradeName, @intRate=DistributeRate From dbo.GrandConfig  with (nolock) Where ID = @GrandUID

	IF EXISTS(Select 1 from dbo.AgentUsers with (nolock) WHERE ID = @AgentUID AND Level = @intLevel)
	BEGIN
		SET @chvErrMsg = '有玩家使用此阶级，无法删除'
		RETURN -1
	END

	DELETE FROM dbo.GrandConfig WHERE ID = @GrandUID

	DECLARE @chvOldData varchar(200) = 
		'阶级=' + Convert(varchar(5),@intLevel) + 
		' 阶级名称=' + @chvGradeName +  
		' 收益比率=' + Convert(varchar(5),@intRate)
	
	
	--紀錄變更	
	EXEC [dbo].[PrWeb_AddChangeLog] 
	@intChangeType = 5,
	@OperateUID = @OperateUID,
	@TargetUID = @AgentUID,
	@chvOldData = @chvOldData,
	@chvNewData = '',
	@chvMemo = ''



	SET @chvErrMsg = '删除阶级成功'
	RETURN 1
END






