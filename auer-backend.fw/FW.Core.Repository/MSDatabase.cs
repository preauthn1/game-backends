using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using System.Data;


namespace FW.Core.Repository
{
    public class MSDatabase : IDatabase
    {
        private List<IDatabase> _databases;
        private bool _transaction;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="databases"></param>
        /// <param name="transaction"></param>
        public MSDatabase(IEnumerable<IDatabase> databases, bool transaction)
        {
            _databases = databases.ToList();
            _transaction = transaction && databases.Count() > 1;
        }

        public int ExecuteCommand(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            if (!checkSqlUpdateData(sql))
            {
                return _databases.FirstOrDefault().ExecuteCommand(sql, parameters);
            }

            int ret = 0;
            if (!_transaction)
            {
                _databases.ForEach(db =>
                {
                    ret = db.ExecuteCommand(sql, parameters);
                });
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {

                    _databases.ForEach(db =>
                    {
                        ret = db.ExecuteCommand(sql, parameters);
                    });

                    scope.Complete();
                }
            }

            return ret;
        }

        public int ExecuteStoreProcedureForResult(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            int ret = 0;

            if (!_transaction)
            {
                _databases.ForEach(db =>
                {
                    ret = db.ExecuteStoreProcedureForResult(sql, parameters);
                });
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    _databases.ForEach(db =>
                    {
                        ret = db.ExecuteStoreProcedureForResult(sql, parameters);
                    });

                    scope.Complete();
                }
            }

            return ret;

        }

        public IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            IEnumerable<TObject> rets = null;

            if (!_transaction)
            {
                _databases.ForEach(db =>
                {
                    rets = db.ExecuteSqlQuery<TObject>(sql, parameters);
                });
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    _databases.ForEach(db =>
                    {
                        rets = db.ExecuteSqlQuery<TObject>(sql, parameters);
                    });

                    scope.Complete();
                }
            }

            return rets;
        }

        public IEnumerable<TObject> ExecuteStoreProcedureForDataList<TObject>(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            IEnumerable<TObject> rets = null;

            if (!_transaction)
            {
                _databases.ForEach(db =>
                {
                    rets = db.ExecuteStoreProcedureForDataList<TObject>(sql, parameters);
                });
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    _databases.ForEach(db =>
                    {
                        rets = db.ExecuteStoreProcedureForDataList<TObject>(sql, parameters);
                    });

                    scope.Complete();
                }
            }

            return rets;
        }


        public System.Data.DataTable ExecuteStoreProcedureDataTable(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            DataTable rets = null;

            if (!_transaction)
            {
                _databases.ForEach(db =>
                {
                    rets = db.ExecuteStoreProcedureDataTable(sql, parameters);
                });
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    _databases.ForEach(db =>
                    {
                        rets = db.ExecuteStoreProcedureDataTable(sql, parameters);
                    });

                    scope.Complete();
                }
            }

            return rets;
        }

        public DataSet ExecuteStoreProcedureDataSet(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            DataSet rets = null;

            if (!_transaction)
            {
                _databases.ForEach(db =>
                {
                    rets = db.ExecuteStoreProcedureDataSet(sql, parameters);
                });
            }
            else
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    _databases.ForEach(db =>
                    {
                        rets = db.ExecuteStoreProcedureDataSet(sql, parameters);
                    });

                    scope.Complete();
                }
            }

            return rets;
        }

        /// <summary>
        /// 检查是否更新数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private bool checkSqlUpdateData(string sql)
        {
            return sql.IndexOf("update", StringComparison.CurrentCultureIgnoreCase) != -1
                || sql.IndexOf("insert", StringComparison.CurrentCultureIgnoreCase) != -1
                || sql.IndexOf("delete", StringComparison.CurrentCultureIgnoreCase) != -1;
        }


        public System.Data.Common.DbConnection Connection
        {
            get
            {
                return _databases.FirstOrDefault().Connection;
            }
        }



    }
}
