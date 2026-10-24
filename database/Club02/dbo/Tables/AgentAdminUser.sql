CREATE TABLE [dbo].[AgentAdminUser] (
    [ID]         INT           IDENTITY (1, 1) NOT NULL,
    [Account]    VARCHAR (20)  NOT NULL,
    [Password]   VARCHAR (32)  NOT NULL,
    [Status]     TINYINT       NOT NULL,
    [AdminType]  INT           NULL,
    [CreateTime] DATETIME      NOT NULL,
    [Encrypt]    VARCHAR (10)  NULL,
    [Memo]       VARCHAR (500) NULL,
    [Mode1]      TINYINT       NULL,
    [Mode2]      TINYINT       NULL,
    [Mode3]      TINYINT       NULL,
    [Mode4]      TINYINT       NULL,
    [Mode5]      TINYINT       NULL,
    [Mode6]      TINYINT       NULL,
    [UpdateTime] DATETIME      NULL,
    CONSTRAINT [PK_AgentAdminUser] PRIMARY KEY CLUSTERED ([ID] ASC)
);

