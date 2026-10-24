CREATE TABLE [dbo].[agent_tip] (
    [id]      INT            IDENTITY (1, 1) NOT NULL,
    [info]    NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [content] NVARCHAR (255) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理举报表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_tip';

