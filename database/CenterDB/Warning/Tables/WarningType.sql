CREATE TABLE [Warning].[WarningType] (
    [TypeID]   INT            IDENTITY (1, 1) NOT NULL,
    [TypeName] VARCHAR (32)   NOT NULL,
    [Status]   TINYINT        NOT NULL,
    [Setting]  VARCHAR (8000) NULL,
    CONSTRAINT [PK_WarningType] PRIMARY KEY CLUSTERED ([TypeID] ASC) WITH (FILLFACTOR = 80)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_WarningType_TypeName]
    ON [Warning].[WarningType]([TypeName] ASC);

