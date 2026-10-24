CREATE TABLE [Customer].[ReplyType] (
    [TypeID]   INT          IDENTITY (1, 1) NOT NULL,
    [TypeName] VARCHAR (64) NOT NULL,
    [AdminID]  INT          NOT NULL,
    [Sort]     INT          NOT NULL,
    CONSTRAINT [PK_QusetionReplyType] PRIMARY KEY CLUSTERED ([TypeID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'类型ID', @level0type = N'SCHEMA', @level0name = N'Customer', @level1type = N'TABLE', @level1name = N'ReplyType', @level2type = N'COLUMN', @level2name = N'TypeID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'类型名称', @level0type = N'SCHEMA', @level0name = N'Customer', @level1type = N'TABLE', @level1name = N'ReplyType', @level2type = N'COLUMN', @level2name = N'TypeName';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'所属后台账号ID', @level0type = N'SCHEMA', @level0name = N'Customer', @level1type = N'TABLE', @level1name = N'ReplyType', @level2type = N'COLUMN', @level2name = N'AdminID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'排序号', @level0type = N'SCHEMA', @level0name = N'Customer', @level1type = N'TABLE', @level1name = N'ReplyType', @level2type = N'COLUMN', @level2name = N'Sort';

