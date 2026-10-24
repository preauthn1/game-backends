CREATE TABLE [dbo].[agent_red] (
    [red_agent_id]  INT NOT NULL,
    [red_player_id] INT NOT NULL
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理红点', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_red';

