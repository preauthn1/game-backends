using System.Collections.Generic;
using game.Entity.Game;

namespace game.Service
{
    /// <summary>
    /// 游戏消息业务接口
    /// </summary>
    public interface IGameMessageService
    {
        /// <summary>
        /// 获取游戏消息
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="platformid"></param>
        /// <returns></returns>
        List<GameMessage> GetGameMessage(int adminID, int? platformids);

        /// <summary>
        /// 添加游戏消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="adminID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddGameMessage(GameMessage msg, int adminID, ref string message);

        /// <summary>
        /// 删除游戏消息
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelGameMessage(int msgID, ref string message);

        /// <summary>
        /// 根据ID获取游戏消息
        /// </summary>
        /// <param name="msgID"></param>
        /// <returns></returns>
        GameMessage GetGameMessageByID(int msgID);

        /// <summary>
        /// 修改游戏消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyGameMessage(GameMessage msg, int adminID, ref string message);

    }
}
