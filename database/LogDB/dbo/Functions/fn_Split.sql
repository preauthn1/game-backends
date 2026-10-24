CREATE FUNCTION [dbo].[fn_Split](    
 @String nvarchar (max),    
 @Delimiter nvarchar (10)    
)    
RETURNS @ValueTable TABLE ([Value] NVARCHAR(max),[id] int)    
BEGIN    
 DECLARE @NextString nvarchar(max),    
  @Pos int,    
  @NextPos int,    
  @CommaCheck nvarchar(1),  
 @id int  
  
set @id=1    
     
 SET @NextString = ''    
 SET @CommaCheck = right(@String,1)     
     
 SET  @String = @String + @Delimiter    
     
  SET @Pos = CHARINDEX(@Delimiter,@String)    
  SET @NextPos = 1    
     
 WHILE (@pos <>  0)      
 BEGIN    
  SET @NextString = SUBSTRING(@String,1,@Pos - 1)    
      
  INSERT INTO @ValueTable ( [Value],[id]) VALUES (@NextString,@id)    
      
  SET @String = SUBSTRING(@String,@pos +1,LEN(@String))    
       
  SET @NextPos = @Pos    
  SET @pos  = CHARINDEX(@Delimiter,@String)    
  
 set @id = @id +1  
 END    
     
 RETURN    
END

GO
GRANT SELECT
    ON OBJECT::[dbo].[fn_Split] TO [GameUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[fn_Split] TO [xbuser]
    AS [dbo];

