using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FW.Core.EventBus.EntityEvents
{
    public static class EventPublisherExtensions
    {
        public const int MESSAGECLASS_INSERTED = 1;
        public const int MESSAGECLASS_DELETED = 2;
        public const int MESSAGECLASS_UPDATED = 3;

        /// <summary>
        /// 发布实体插入事件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="eventPublisher"></param>
        /// <param name="entity"></param>
        public static void EntityInserted<TEntity>(this IEventPublisher eventPublisher, TEntity entity)
        {
            eventPublisher.PublishEvent(new EventMessage(MESSAGECLASS_INSERTED, new EntityInserted<TEntity>(entity)));
        }

        /// <summary>
        /// 发布实体更新事件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="eventPublisher"></param>
        /// <param name="entity"></param>
        /// <param name="props"></param>
        public static void EntityUpdated<TEntity>(this IEventPublisher eventPublisher, TEntity entity, params string[] props)
        {
            eventPublisher.PublishEvent(new EventMessage(MESSAGECLASS_UPDATED, new  EntityUpdated<TEntity>(entity, props)));
        }

        /*
        /// <summary>
        /// 发布实体更新事件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="eventPublisher"></param>
        /// <param name="entity"></param>
        /// <param name="props"></param>
        public static void EntityUpdated<TEntity>(this IEventPublisher eventPublisher, TEntity entity, params Expression<Func<TEntity,object>>[] exps )
        {
            List<string> props = new List<string>();
            if (exps != null)
            {
                Array.ForEach(exps, exp => {
                    string bodyExpressionStr = exp.Body.ToString();
                    var propertyName = bodyExpressionStr.Substring(bodyExpressionStr.IndexOf(".") + 1);
                    props.Add(propertyName);
                });
            }
            
            eventPublisher.PublishEvent(new EventMessage(MESSAGECLASS_UPDATED, new EntityUpdated<TEntity>(entity, props.ToArray())));
        }
        */


        /// <summary>
        /// 发布实体更新事件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="eventPublisher"></param>
        /// <param name="entity"></param>
        public static void EntityDeleted<TEntity>(this IEventPublisher eventPublisher, TEntity entity)
        {
            eventPublisher.PublishEvent(new EventMessage(MESSAGECLASS_UPDATED, new  EntityDeleted<TEntity>(entity)));
        }



    }
}
