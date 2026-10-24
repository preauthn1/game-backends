-- =============================================
-- Author:		<xxx>
-- Create date: <xxx>
-- Description:	<获取游戏公告>
-- changelog:
-- 2018/5/29	alan	platform改回傳0,代表全服公告
-- 2018/08/12	Jason	因原本寫法已造成效能問題，調整語法
-- 2018/9/13	alan	新增全平台設定
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_GetNotice]
AS
SET NOCOUNT ON


--玩家赢金币广播公告
SELECT
top 20
W.Id as Id,
0 as PlatformID, --A.PlatformID,
CASE
  floor(rand() * 3)  --0,1,2
WHEN 0 THEN
	'恭喜<color=#ffad33>'+
	CenterDB.dbo.Fn_HideLoginName(case when u.UserType<>3 THEN U.LoginName else '游客'+CAST(w.UserID AS VARCHAR) end)
	+'</color>在游戏<color=#ffad33>'+CenterDB.dbo.Fn_GetRoomName(W.ServerID)+'</color>中赢得<color=#ffff33>'+CenterDB.dbo.Fn_ShowMoney(W.WinAmount)+'</color>金币,让我们恭喜他,相信下一个中大奖的就是你了。' 
WHEN 1 THEN
	'狂贺<color=#ffad33>'+
	CenterDB.dbo.Fn_HideLoginName(case when u.UserType<>3 THEN U.LoginName else '游客'+CAST(w.UserID AS VARCHAR) end)
	+'</color>在<color=#ffad33>'+CenterDB.dbo.Fn_GetRoomName(W.ServerID)+'</color>中赢得<color=#ffff33>'+CenterDB.dbo.Fn_ShowMoney(W.WinAmount)+'</color>金币,这平台根本在大放送啊～～'

WHEN 2 THEN
	'恭喜<color=#ffad33>'+
	CenterDB.dbo.Fn_HideLoginName(case when u.UserType<>3 THEN U.LoginName else '游客'+CAST(w.UserID AS VARCHAR) end)
	+'</color>在<color=#ffad33>'+CenterDB.dbo.Fn_GetRoomName(W.ServerID)+'</color>一把赢得<color=#ffff33>'+CenterDB.dbo.Fn_ShowMoney(W.WinAmount)+'</color>金币。' 
ELSE
	'恭喜<color=#ffad33>'+
	CenterDB.dbo.Fn_HideLoginName(case when u.UserType<>3 THEN U.LoginName else '游客'+CAST(w.UserID AS VARCHAR) end)
	+'</color>在<color=#ffad33>'+CenterDB.dbo.Fn_GetRoomName(W.ServerID)+'</color>一把赢得<color=#ffff33>'+CenterDB.dbo.Fn_ShowMoney(W.WinAmount)+'</color>金币。' 
	
END as MsgContent

INTO #t

FROM CenterDB.Game.WinAnn as W 
LEFT JOIN CenterDB.Users.UserInfo as U with(nolock) ON W.UserID = U.UserID
LEFT JOIN CenterDB.Area.Area as A with(nolock) ON U.AreaID = A.AreaID
where
	--Status = 0 and 
	w.CreateTime >= GETDATE()-3
	order by NEWID()



UPDATE W
SET Status=1
,SendTime=GETDATE()
FROM CenterDB.Game.WinAnn as W
WHERE EXISTS(select 1 from #t where Id = W.Id)



UPDATE #t
set Id = Id + 1000000

INSERT INTO #t
SELECT Id,
CASE WHEN AllPlatform=1 THEN 0 ELSE PlatformID END as PlatformID,
MsgContent
FROM 
CenterDB.Game.GameMessage 
WHERE status = 1
AND GETDATE() BETWEEN BeginTime AND EndTime
AND DATEADD(ss, Interval, ISNULL(LastSendTime,CAST('1990-1-1' AS DATETIME))) <= GETDATE()

IF @@ROWCOUNT > 0
UPDATE gm
SET gm.LastSendTime = GETDATE()
FROM #t,CenterDB.Game.GameMessage gm
WHERE gm.Id = #t.Id


SELECT * FROM #t

DROP TABLE #t


RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_GetNotice] TO [xbuser]
    AS [dbo];

