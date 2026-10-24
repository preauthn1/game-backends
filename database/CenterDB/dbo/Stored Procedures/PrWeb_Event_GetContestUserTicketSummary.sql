



-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 後台取得比賽活動所有遊戲免費券資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_GetContestUserTicketSummary]
(
	@intUserID int
)
AS
BEGIN



DECLARE @XML xml;
--Table to XML
SET @XML=( 
SELECT * 
FROM 
(
	select C.GameID,G.GameName,T.UserID,Count(T.ID) Tickets from Event.Ticket  T
	left join Event.Contest C on T.ContestID=C.ID
	left join ServerInfo.GameList G on G.GameID=C.GameID
	where 
	--T.UpdateTime is null
	T.userid = @intUserID
	and C.GameID is not null
	group by C.GameID,G.GameName,T.UserID
) as A
FOR XML path,root
)


--XML to Json
DECLARE @Json varchar(1000)
SELECT @Json=Stuff(  
  (SELECT * from  
    (SELECT ',{'+  
      Stuff((SELECT ',"'+coalesce(b.c.value('local-name(.)', 'NVARCHAR(MAX)'),'')+'":"'+
                    b.c.value('text()[1]','NVARCHAR(MAX)') +'"'
               
             from x.a.nodes('*') b(c)  
             for xml path(''),TYPE).value('(./text())[1]','NVARCHAR(MAX)')
        ,1,1,'')+'}' 
   from @XML.nodes('/root/*') x(a)  
   ) JSON(theLine)  
  for xml path(''),TYPE).value('.','NVARCHAR(MAX)' )
,1,1,'')  


SELECT '[' + @Json + ']' as Result


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestUserTicketSummary] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_GetContestUserTicketSummary] TO [xbuser]
    AS [dbo];

