-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 更新一级下线分成比例数据
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_ChangeSubAgentLog]
	@intUserID		int,
	@intSubUserID	int,
	@tnyStage1		tinyint,
	@tnyStage2		tinyint,
	@tnyStage3		tinyint,
	@tnyBFStage1	tinyint,
	@tnyBFStage2	tinyint,
	@tnyBFStage3	tinyint,
	@tnyStatus		tinyint,
	@tnyBFStatus	tinyint
AS
BEGIN

	set nocount on

	declare @NowTime datetime = getdate()

	insert into [dbo].[Log_UpdateUserAgentLog]
		([LogDate],[UserID],[SubUserID],[Stage1],[Stage2],[Stage3],[BF_Stage1],[BF_Stage2],[BF_Stage3],[Status],[BF_Status])
	values
		(@NowTime,@intUserID,@intSubUserID,@tnyStage1,@tnyStage2,@tnyStage3,@tnyBFStage1,@tnyBFStage2,@tnyBFStage3,@tnyStatus,@tnyBFStatus)

END






