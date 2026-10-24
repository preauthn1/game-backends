








-- =============================================
-- Author:  harry
-- Create date: 2018/08/28
-- Description:	查询操作纪录
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_ChangeLog_Get]
	@AgentUID		uniqueidentifier,
	@intUserID		int = NULL,
	@chvloginName	VARCHAR(50) = NULL,
	@dtStartDate	datetime = NULL,
	@dtEndDate		datetime = NULL,
	@intPageIndex        int=1,
    @intPageSize         int=5,
    @sys_Order nvarchar(100) = 'CreateTime',
    @sys_SortDirection nvarchar(10) = 'D',
	@intPageCount int OUTPUT
AS
BEGIN
	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1:一級代理  2:二級代理 99:一般玩家  
	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有申请清单
	BEGIN

		SELECT @intPageCount = COUNT(1) 
		FROM ChangeLog C      
		LEFT JOIN AgentUsers U1 ON U1.ID = C.OperateID
		LEFT JOIN AgentUsers U2 ON U2.ID = C.TargetID
		WHERE (@intUserID IS NULL OR @intUserID = U2.UserID)
		AND (@chvloginName IS NULL OR @chvloginName = U2.LoginName)
		AND (@dtStartDate IS NULL OR @dtStartDate <= C.CreateTime)
		AND (@dtEndDate IS NULL OR @dtEndDate >= C.CreateTime)  

		
		SELECT  C.LogID, OperateName, TargetName, C.ChangeType, C.OldData, C.NewData, C.Memo, C.CreateTime FROM
		(
			SELECT TOP (@intPageIndex * @intPageSize) LogID, U1.LoginName AS OperateName, U2.UserID, U2.LoginName AS TargetName, ROW_NUMBER() OVER (
				ORDER BY 
				CASE WHEN @sys_Order = 'OperateName' AND @sys_SortDirection = 'A' THEN U1.LoginName END ASC,
				CASE WHEN @sys_Order = 'OperateName' AND @sys_SortDirection = 'D' THEN U1.LoginName END DESC,
				CASE WHEN @sys_Order = 'TargetName' AND @sys_SortDirection = 'A' THEN U2.LoginName END ASC,
				CASE WHEN @sys_Order = 'TargetName' AND @sys_SortDirection = 'D' THEN U2.LoginName END DESC,
				CASE WHEN @sys_Order = 'ChangeType' AND @sys_SortDirection = 'A' THEN ChangeType END ASC,
				CASE WHEN @sys_Order = 'ChangeType' AND @sys_SortDirection = 'D' THEN ChangeType END DESC,
				CASE WHEN @sys_Order = 'OldData' AND @sys_SortDirection = 'A' THEN OldData END ASC,
				CASE WHEN @sys_Order = 'OldData' AND @sys_SortDirection = 'D' THEN OldData END DESC,
				CASE WHEN @sys_Order = 'NewData' AND @sys_SortDirection = 'A' THEN NewData END ASC,
				CASE WHEN @sys_Order = 'NewData' AND @sys_SortDirection = 'D' THEN NewData END DESC,
				CASE WHEN @sys_Order = 'Memo' AND @sys_SortDirection = 'A' THEN Memo END ASC,
				CASE WHEN @sys_Order = 'Memo' AND @sys_SortDirection = 'D' THEN Memo END DESC,
				CASE WHEN @sys_Order = 'CreateTime' AND @sys_SortDirection = 'A' THEN C.CreateTime END ASC,
				CASE WHEN @sys_Order = 'CreateTime' AND @sys_SortDirection = 'D' THEN C.CreateTime END DESC
			) AS RowNum 
			FROM ChangeLog C       
			   LEFT JOIN AgentUsers U1 ON U1.ID = C.OperateID
			   LEFT JOIN AgentUsers U2 ON U2.ID = C.TargetID
			   WHERE (@intUserID IS NULL OR @intUserID = U2.UserID)
			   AND (@chvloginName IS NULL OR @chvloginName = U2.LoginName)
			   AND (@dtStartDate IS NULL OR @dtStartDate <= C.CreateTime)
			   AND (@dtEndDate IS NULL OR @dtEndDate >= C.CreateTime)   
		) AS tempTable
		INNER JOIN ChangeLog C on C.LogID=tempTable.LogID
		WHERE tempTable.RowNum > (@intPageIndex-1) * @intPageSize
		
		RETURN 1
	END
	ELSE
	BEGIN

		SELECT @intPageCount = COUNT(1) 
		FROM ChangeLog C      
		LEFT JOIN AgentUsers U1 ON U1.ID = C.OperateID
		LEFT JOIN AgentUsers U2 ON U2.ID = C.TargetID
		WHERE (C.TargetID = @AgentUID OR C.OperateID =  @AgentUID) 
		AND (@intUserID IS NULL OR @intUserID = U2.UserID)
		AND (@chvloginName IS NULL OR @chvloginName = U2.LoginName)
		AND (@dtStartDate IS NULL OR @dtStartDate <= C.CreateTime)
		AND (@dtEndDate IS NULL OR @dtEndDate >= C.CreateTime)  

		SELECT  C.LogID, OperateName, TargetName, C.ChangeType, C.OldData, C.NewData, C.Memo, C.CreateTime FROM
		(
			SELECT TOP (@intPageIndex * @intPageSize) LogID, U1.LoginName AS OperateName, U2.UserID, U2.LoginName AS TargetName, ROW_NUMBER() OVER (
				ORDER BY 
				CASE WHEN @sys_Order = 'OperateName' AND @sys_SortDirection = 'A' THEN U1.LoginName END ASC,
				CASE WHEN @sys_Order = 'OperateName' AND @sys_SortDirection = 'D' THEN U1.LoginName END DESC,
				CASE WHEN @sys_Order = 'TargetName' AND @sys_SortDirection = 'A' THEN U2.LoginName END ASC,
				CASE WHEN @sys_Order = 'TargetName' AND @sys_SortDirection = 'D' THEN U2.LoginName END DESC,
				CASE WHEN @sys_Order = 'ChangeType' AND @sys_SortDirection = 'A' THEN ChangeType END ASC,
				CASE WHEN @sys_Order = 'ChangeType' AND @sys_SortDirection = 'D' THEN ChangeType END DESC,
				CASE WHEN @sys_Order = 'OldData' AND @sys_SortDirection = 'A' THEN OldData END ASC,
				CASE WHEN @sys_Order = 'OldData' AND @sys_SortDirection = 'D' THEN OldData END DESC,
				CASE WHEN @sys_Order = 'NewData' AND @sys_SortDirection = 'A' THEN NewData END ASC,
				CASE WHEN @sys_Order = 'NewData' AND @sys_SortDirection = 'D' THEN NewData END DESC,
				CASE WHEN @sys_Order = 'Memo' AND @sys_SortDirection = 'A' THEN Memo END ASC,
				CASE WHEN @sys_Order = 'Memo' AND @sys_SortDirection = 'D' THEN Memo END DESC,
				CASE WHEN @sys_Order = 'CreateTime' AND @sys_SortDirection = 'A' THEN C.CreateTime END ASC,
				CASE WHEN @sys_Order = 'CreateTime' AND @sys_SortDirection = 'D' THEN C.CreateTime END DESC
			) AS RowNum 
			FROM ChangeLog C       
			   LEFT JOIN AgentUsers U1 ON U1.ID = C.OperateID
			   LEFT JOIN AgentUsers U2 ON U2.ID = C.TargetID
			   WHERE (C.TargetID = @AgentUID OR C.OperateID =  @AgentUID) 
			   AND (@intUserID IS NULL OR @intUserID = U2.UserID)
			   AND (@chvloginName IS NULL OR @chvloginName = U2.LoginName)
			   AND (@dtStartDate IS NULL OR @dtStartDate <= C.CreateTime)
			   AND (@dtEndDate IS NULL OR @dtEndDate >= C.CreateTime)   
		) AS tempTable
		INNER JOIN ChangeLog C on C.LogID=tempTable.LogID
		WHERE tempTable.RowNum > (@intPageIndex-1) * @intPageSize
	END
END




