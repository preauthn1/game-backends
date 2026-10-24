CREATE TABLE [dbo].[AutoAgentApply] (
    [IPAddress]  VARCHAR (32) NOT NULL,
    [InviteCode] VARCHAR (20) NOT NULL,
    [CreateTime] DATETIME     DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_AutoAgentApply] PRIMARY KEY CLUSTERED ([IPAddress] ASC)
);

