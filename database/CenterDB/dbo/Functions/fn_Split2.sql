
CREATE FUNCTION [dbo].[fn_Split2](    
 @String nvarchar (max),    
 @Delimiter nvarchar (10)    
)    
RETURNS @ValueTable TABLE ([Value] NVARCHAR(max),[id] int)    
BEGIN    
 DECLARE @DelimiterLen int
 DECLARE @NextString nvarchar(max),    
  @Pos int,    
  @NextPos int,    
  @CommaCheck nvarchar(1),  
  @id int  
  

SET @DelimiterLen = len(@Delimiter)
set @id=1    
     
 SET @NextString = ''    
 SET @CommaCheck = right(@String,1)     
     
 SET  @String = @String + @Delimiter    
     
  SET @Pos = CHARINDEX(@Delimiter,@String)    
  SET @NextPos = 1    
     
 WHILE (@pos <>  0)      
 BEGIN    
  SET @NextString = SUBSTRING(@String,1,@Pos - 1)    
      
  INSERT INTO @ValueTable ([Value],[id]) VALUES (@NextString,@id)    
      
  SET @String = SUBSTRING(@String,@pos + @DelimiterLen,LEN(@String))    
       
  SET @NextPos = @Pos    
  SET @pos  = CHARINDEX(@Delimiter,@String)    
  
 set @id = @id +1  
 END    
     
 RETURN    
END

GO
GRANT SELECT
    ON OBJECT::[dbo].[fn_Split2] TO [GameUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[fn_Split2] TO [xbuser]
    AS [dbo];

