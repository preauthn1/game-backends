CREATE TABLE [Notify].[Notify] (
    [Idx]        INT           IDENTITY (1, 1) NOT NULL,
    [UserID]     INT           NOT NULL,
    [Type]       INT           NOT NULL,
    [Message]    VARCHAR (500) NULL,
    [CreateTime] DATETIME      CONSTRAINT [DF_Notify_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Notify_1] PRIMARY KEY CLUSTERED ([Idx] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Notify_UserID]
    ON [Notify].[Notify]([Idx] ASC, [UserID] ASC)
    INCLUDE([Type], [Message]);

