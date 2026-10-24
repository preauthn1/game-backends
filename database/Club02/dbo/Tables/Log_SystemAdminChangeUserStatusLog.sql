CREATE TABLE [dbo].[Log_SystemAdminChangeUserStatusLog] (
    [ID]             BIGINT        IDENTITY (1, 1) NOT NULL,
    [AdminID]        INT           NOT NULL,
    [UserID]         INT           NOT NULL,
    [Mode]           INT           NULL,
    [Status]         INT           NULL,
    [BF_Status]      INT           NULL,
    [CreateTime]     DATETIME      NOT NULL,
    [platformStr]    VARCHAR (MAX) NULL,
    [BF_platformStr] VARCHAR (MAX) NULL,
    [systemrate]     VARCHAR (100) NULL,
    [BF_systemrate]  VARCHAR (100) NULL,
    CONSTRAINT [PK_Log_SystemAdminChangeUserStatusLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

