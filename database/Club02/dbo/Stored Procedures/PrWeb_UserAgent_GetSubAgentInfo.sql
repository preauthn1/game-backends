-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 取得一级下线资料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetSubAgentInfo]	
	@intUserID		int,		--用户 ID
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
AS
BEGIN

	set nocount on

	select	@intPageCount = count(A.UserID)
	from [dbo].[UserAgentInfo] as a with (nolock)
		inner join [dbo].[UserAgentUserRate] as b with (nolock) on A.UserID = B.[UserID]
	where
		[TopUserID] = @intUserID


	;WITH CTE
	AS (
			select	ROW_NUMBER() OVER (ORDER BY [LastUpdatetime] DESC) AS rowNum,
					A.UserID,
					a.[UserLevel],
					[SumTaxTotal],
					[SumTaxConvert],
					[SumTaxPerformed],
					[UnWithdrawTotal],
					[RateStage1] = B.[Stage1],
					[RateStage2] = B.[Stage2],
					[RateStage3] = B.[Stage3],
					[SysRate1] = C.Stage1,
					[SysRate2] = C.Stage2,
					[SysRate3] = C.Stage3,
					A.[Status],
					A.[CreateTime],
					[TopUserID],
					[LastUpdatetime]
			from [dbo].[UserAgentInfo] as a with (nolock)
				inner join [dbo].[UserAgentUserRate] as b with (nolock) on A.UserID = B.[UserID] and a.UserLevel = B.UserLevel
				inner join [dbo].[UserAgentSystemLevelRate] as c with (nolock) on a.UserLevel = c.Level
			where
					[TopUserID] = @intUserID
			)
		SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
		ORDER BY rowNum ASC

END






