
CREATE FUNCTION [dbo].[fn_RecursiveUp](    
@UID uniqueidentifier
)    
RETURNS @ValueTable TABLE ([Id] uniqueidentifier, UserID int,LoginName varchar(32), ParentID uniqueidentifier, Class tinyint, TreeLevel int, Rate int)    
BEGIN    
     
;WITH Tree AS (

	SELECT ID, UserID,LoginName, ParentID, Class, 100 AS TreeLevel
	FROM [AgentUsers] WHERE ID = @UID --AND ParentID is NULL
	UNION ALL
	--跟 CTE 自身做遞迴 JOIN (A 的節點ID是 Tree 表的父節點ParentID)
	SELECT A.ID, A.UserID, A.LoginName, A.ParentID, A.Class, (B.TreeLevel - 1) AS TreeLevel 
	FROM [AgentUsers] A
	INNER JOIN Tree B ON A.ID = B.ParentID
)
Insert into @ValueTable(Id, userID, LoginName,ParentID,Class,TreeLevel,Rate) 
Select tree.Id, tree.userID, tree.LoginName, tree.ParentID, tree.Class, tree.TreeLevel, CASE WHEN tree.Class=0 THEN d.DistributeRate ELSE c.DistributeRate END as Rate FROM Tree
left join dbo.GrandConfig c with (nolock) on tree.id=c.agentid 
left join dbo.AgentRateRange d with (nolock) on tree.id=d.ID
order by TreeLevel

 RETURN    
END
