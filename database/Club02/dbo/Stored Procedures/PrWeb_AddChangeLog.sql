




-- =============================================
-- Author:  alan
-- Create date: 2018/8/27
-- Description:	增加修改紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddChangeLog]
	@intChangeType tinyint,
	@OperateUID	uniqueidentifier, --操作者
	@TargetUID	uniqueidentifier, --對象
	@chvOldData varchar(500),
	@chvNewData varchar(500),
	@chvMemo varchar(50)

AS
	SET NOCOUNT ON;

	INSERT INTO ChangeLog(ChangeType, OperateID, TargetID, OldData, NewData, Memo) 
	VALUES(@intChangeType, @OperateUID, @TargetUID, @chvOldData, @chvNewData, @chvMemo)
		
	RETURN 1


