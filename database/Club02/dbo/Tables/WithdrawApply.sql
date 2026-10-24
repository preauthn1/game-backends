CREATE TABLE [dbo].[WithdrawApply] (
    [ApplyID]        UNIQUEIDENTIFIER CONSTRAINT [DF_WithdrawApply_ApplyID] DEFAULT (newid()) NOT NULL,
    [ID]             UNIQUEIDENTIFIER NOT NULL,
    [WithdrawAmount] BIGINT           NOT NULL,
    [Status]         TINYINT          CONSTRAINT [DF_WithdrawApply_Status] DEFAULT ((0)) NOT NULL,
    [Memo]           VARCHAR (50)     NULL,
    [CreateTime]     DATETIME         CONSTRAINT [DF_WithdrawApply_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_WithdrawApply] PRIMARY KEY CLUSTERED ([ApplyID] ASC)
);

