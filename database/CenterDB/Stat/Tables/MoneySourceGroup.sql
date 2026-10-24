CREATE TABLE [Stat].[MoneySourceGroup] (
    [Id]        INT          NOT NULL,
    [GroupName] VARCHAR (32) NOT NULL,
    [Sort]      INT          NOT NULL,
    CONSTRAINT [PK_MoneySourceGroup] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_MoneySourceGroup_GroupName]
    ON [Stat].[MoneySourceGroup]([GroupName] ASC);

