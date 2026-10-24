-- =============================================
-- Author:  Jason
-- Create date: 2018/06/13
-- Description:	#447 後台系統設置->用戶平台 2. 新加入的平台, 沒有被分配給使用者 機制評估調整 排程定期給予客服權限
-- Changelog:
--		2018/7/2 alan 排除聯運帳號
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_AutoGrantNewPlatformAdminUser]
	
AS
BEGIN 

	
	SET NOCOUNT ON
	--平台
	INSERT INTO [CenterDB].[Area].[PlatformAdminUser]
		([AdminID],[PlatformID])
	SELECT	[AdminID],[PlatformID]
	FROM (
			SELECT	B.PlatformID,
					[AdminID] = A.userid
			FROM [AdminSys].[Admin].[Users] AS A WITH (NOLOCK)
				INNER JOIN [CenterDB].[Area].[Platform] AS B WITH (NOLOCK) ON 1=1
			WHERE
				A.CreateTime >= DATEADD(HOUR,-2,GETDATE())
				AND A.CreateTime < GETDATE()
				AND A.UserType <> 10 -- 排除聯運帳號
				--AND B.PlatformID not in (select PlatformID from Area.UnionPlatform) -- 排除聯運平台
			UNION
			SELECT	A.PlatformID,
					[AdminID] = B.userid
			  FROM [CenterDB].[Area].[Platform] AS A WITH (NOLOCK)
				INNER JOIN [AdminSys].[Admin].[Users] AS B WITH (NOLOCK) ON 1=1
			WHERE
				A.PlatformCreateTime >= DATEADD(HOUR,-2,GETDATE())
				AND A.PlatformCreateTime < GETDATE()
				AND B.UserType <> 10 -- 排除聯運帳號
				--AND A.PlatformID not in (select PlatformID from Area.UnionPlatform) -- 排除聯運平台
		) AS A
	EXCEPT
	SELECT	[AdminID],[PlatformID]
	FROM [CenterDB].[Area].[PlatformAdminUser] WITH (NOLOCK)


	--渠道
	INSERT INTO [CenterDB].[Area].[AreaAdminUser]
		([AdminID],[PlatformID],[AreaID])
	SELECT	[AdminID],[PlatformID],[AreaID]
	FROM (
			SELECT	B.PlatformID,
					[AdminID] = A.userid,
					[AreaID] = B.AreaID
			FROM [AdminSys].[Admin].[Users] AS A WITH (NOLOCK)
				INNER JOIN [CenterDB].[Area].[Area] AS B WITH (NOLOCK) ON 1=1
			WHERE
				A.CreateTime >= DATEADD(HOUR,-2,GETDATE())
				AND A.CreateTime < GETDATE()
				AND A.UserType <> 10 -- 排除聯運帳號
				--AND B.PlatformID not in (select PlatformID from Area.UnionPlatform) -- 排除聯運平台
			UNION
			SELECT	A.PlatformID,
					[AdminID] = B.userid,
					[AreaID] = A.AreaID
			  FROM [CenterDB].[Area].[Area] AS A WITH (NOLOCK)
				INNER JOIN [AdminSys].[Admin].[Users] AS B WITH (NOLOCK) ON 1=1
			WHERE
				A.CreateTime >= DATEADD(HOUR,-2,GETDATE())
				AND A.CreateTime < GETDATE()
				AND B.UserType <> 10 -- 排除聯運帳號
				--AND A.PlatformID not in (select PlatformID from Area.UnionPlatform) -- 排除聯運平台
		) AS A
	EXCEPT
	SELECT	[AdminID],[PlatformID],[AreaID]
	FROM [CenterDB].[Area].[AreaAdminUser] WITH (NOLOCK)




END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_AutoGrantNewPlatformAdminUser] TO [xbuser]
    AS [dbo];

