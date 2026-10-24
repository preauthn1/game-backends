using System;

namespace target.Models.ViewModels
{

    /// <summary>
    /// 若玩家進入lobby有指定特定參數、而當從lobby進入特定遊戲時。該參數必須再指定原值 (e.g. isTestMode)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeepValueWhenLobbyLaunch : Attribute
    {
    }

}
