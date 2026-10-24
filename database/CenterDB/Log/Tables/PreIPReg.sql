CREATE TABLE [Log].[PreIPReg] (
    [LogDate]    INT          NOT NULL,
    [IPAddress]  VARCHAR (40) NOT NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_PreIPReg_CreateTime] DEFAULT (getdate()) NOT NULL,
    [Counts]     INT          NULL,
    CONSTRAINT [PK_PreIPReg] PRIMARY KEY CLUSTERED ([IPAddress] ASC, [LogDate] ASC)
);

