USE [LogDB]
GO
DECLARE @ConstraintName SYSNAME
DECLARE @SchemaName SYSNAME
DECLARE @TableName SYSNAME
DECLARE @ColumnName SYSNAME
SET @SchemaName = 'Game'
SET @TableName = 'SlotGameHistoryLog'
--刪除CurrentCoupon
SET @ColumnName = 'CurrentCoupon'
SET @ConstraintName = ISNULL((SELECT DC.name FROM sys.default_constraints AS DC JOIN sys.all_columns C ON C.object_id = DC.parent_object_id AND DC.parent_column_id = C.column_id
WHERE OBJECT_NAME(DC.parent_object_id) = @TableName AND SCHEMA_NAME(DC.schema_id) = @SchemaName AND C.name = @ColumnName),'')
IF (@ConstraintName <> '')
BEGIN
EXEC ('ALTER TABLE ' + @SchemaName + '.' + @TableName  + ' DROP CONSTRAINT ' + @ConstraintName)
END
ALTER TABLE [LogDB].[Game].[SlotGameHistoryLog] DROP COLUMN [CurrentCoupon]
--刪除BetCoupon
SET @ColumnName = 'BetCoupon'
SET @ConstraintName = ISNULL((SELECT DC.name FROM sys.default_constraints AS DC JOIN sys.all_columns C ON C.object_id = DC.parent_object_id AND DC.parent_column_id = C.column_id
WHERE OBJECT_NAME(DC.parent_object_id) = @TableName AND SCHEMA_NAME(DC.schema_id) = @SchemaName AND C.name = @ColumnName),'')
IF (@ConstraintName <> '')
BEGIN
EXEC ('ALTER TABLE ' + @SchemaName + '.' + @TableName  + ' DROP CONSTRAINT ' + @ConstraintName)
END
ALTER TABLE [LogDB].[Game].[SlotGameHistoryLog] DROP COLUMN [BetCoupon]
--刪除FinalCoupon
SET @ColumnName = 'FinalCoupon'
SET @ConstraintName = ISNULL((SELECT DC.name FROM sys.default_constraints AS DC JOIN sys.all_columns C ON C.object_id = DC.parent_object_id AND DC.parent_column_id = C.column_id
WHERE OBJECT_NAME(DC.parent_object_id) = @TableName AND SCHEMA_NAME(DC.schema_id) = @SchemaName AND C.name = @ColumnName),'')
IF (@ConstraintName <> '')
BEGIN
EXEC ('ALTER TABLE ' + @SchemaName + '.' + @TableName  + ' DROP CONSTRAINT ' + @ConstraintName)
END
ALTER TABLE [LogDB].[Game].[SlotGameHistoryLog] DROP COLUMN [FinalCoupon]