using System;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;


namespace FW.Core.EventBus
{
    [DataContract]
    public class EventMessage
    {
        /// <summary>
        /// ctor
        /// </summary>
        public EventMessage()
        {
            MessageID = Guid.NewGuid().ToString().Replace("-","");
            EventTime = DateTime.Now;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="classID"></param>
        /// <param name="messageContent"></param>
        public EventMessage(int classID, object messageContent)
            :this()
        {
            ClassID = classID;
            MessageContent = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(messageContent);
        }

        /// <summary>
        /// 消息ID
        /// </summary>
        [DataMember]
        public string MessageID
        {
            get;
            private set;
        }

        /// <summary>
        /// 消息分类
        /// </summary>
        [DataMember]
        public int ClassID
        {
            get;
            set;
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        [DataMember]
        public string MessageContent
        {
            get;
            set;
        }

        /// <summary>
        /// 时间
        /// </summary>
        [DataMember]
        public DateTime EventTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 设置消息内容
        /// </summary>
        /// <param name="data"></param>
        public void SetMessageContent(object data)
        {
            MessageContent = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(data);
        }

        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(this).Replace("{", "[").Replace("}", "]");
        }

        public TObject GetMessageObject<TObject>()
        {
            if (String.IsNullOrWhiteSpace(MessageContent))
            {
                return default(TObject);
            }

            return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<TObject>(MessageContent);
        }

    }
}
