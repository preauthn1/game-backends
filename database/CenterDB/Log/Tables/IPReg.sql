CREATE TABLE [Log].[IPReg] (
    [IPAddress] VARCHAR (15) NOT NULL,
    [RegCount]  INT          NOT NULL,
    [LogDate]   INT          NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_IPReg_IP]
    ON [Log].[IPReg]([IPAddress] ASC, [LogDate] ASC);

