using MCDT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Web;
namespace MCDT
{
    public class DataBase : IDisposable
    {

        public void Dispose()
        {
            Conne.Close();
            Conne.Dispose();
        }

        public SqlConnection Conne;

        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <param name="connStr"></param>
        public DataBase(string str)
        {
            Conne = new SqlConnection(str);
        }
        public DataBase(string Server, string DBName, string LoginUser, string Pwd)
        {
            string conStr = string.Format("server={0};database={1};user id={2};password={3}", Server, DBName, LoginUser, Pwd);

            Conne = new SqlConnection(conStr);
        }


        public void Inserts(string table, DataTable dt)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Conne.ConnectionString, SqlBulkCopyOptions.KeepIdentity))
            {
                bulkCopy.DestinationTableName = table;//数据库中的表名
                bulkCopy.WriteToServer(dt);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="model"></param>
        /// <param name="identityFiled">标识列名称</param>
        /// <returns></returns>
        public int? Insert(string tableName, dynamic model, string identityFiled = "ID")
        {
            List<SqlParameter> parms = GetParameterListByModel(model);
            Type type = model.GetType();
            var pros = type.GetProperties();
            List<string> keys = new List<string>();
            List<string> colums = new List<string>();


            if (model is Dictionary<string, object>)
            {
                foreach (var item in model as Dictionary<string, object>)
                {
                    keys.Add(item.Key);
                    colums.Add("@" + item.Key);
                }
            }
            else
            {

                foreach (var item in pros)
                {
                    if (item.GetValue(model) != null)
                    {
                        keys.Add(item.Name);
                        colums.Add("@" + item.Name);
                    }
                }
            }
            string sql = "insert into  " + tableName + "(" + string.Join(",", keys.ToArray()) + ") output inserted." + identityFiled + " values(" + string.Join(",", colums.ToArray()) + ")";
            if (string.IsNullOrEmpty(identityFiled))
                sql = "insert into  " + tableName + "(" + string.Join(",", keys.ToArray()) + ")  values(" + string.Join(",", colums.ToArray()) + ")";
            var index = getSqlCommand(sql, (com) =>
            {
                com.Parameters.AddRange(parms.ToArray());
                return com.ExecuteScalar();

            });
            return Convert.ToInt32(index);
        }



        public int? Update(string tableName, string condition, dynamic model)
        {
            List<SqlParameter> parms = GetParameterListByModel(model);
            Type type = model.GetType();
            var pros = type.GetProperties();
            List<string> keys = new List<string>();


            if (model is Dictionary<string, object>)
            {
                foreach (var item in model as Dictionary<string, object>)
                {
                    keys.Add(item.Key + "=@" + item.Key);
                }
            }
            else
            {

                foreach (var item in pros)
                {
                    if (item.GetValue(model) != null)
                    {
                        keys.Add(item.Name + "=@" + item.Name);
                    }
                }
            }

            string sql = "update " + tableName + " set " + string.Join(",", keys.ToArray()) + " where " + condition;
            return getSqlCommand(sql, (com) =>
            {
                com.Parameters.AddRange(parms.ToArray());
                return com.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// 启动一个事务
        /// </summary>
        /// <param name="action">回调</param>
        public dynamic Transaction(Func<dynamic> action)
        {
            using (TransactionScope transaction = new TransactionScope())//使用事务
            {
                if (Conne.State != ConnectionState.Open)
                    Conne.Open();
                var result = action();
                transaction.Complete();
                return result;
            }

        }

        /// <summary>
        /// 获取一个SqlCommand回调
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public dynamic getSqlCommand(string sql, Func<SqlCommand, dynamic> action)
        {
            if (Conne.State != ConnectionState.Open)
                Conne.Open();
            using (SqlCommand com = new SqlCommand(sql, Conne))
            {
                return action(com);
            }
        }

        /// <summary>
        /// 离线查询，返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public DataTable ExecuteTable(string sql, dynamic model = null)
        {
            var par = GetParametersByModel(model);
            using (SqlDataAdapter sda = new SqlDataAdapter(sql, Conne))
            {
                if (par != null && par.Length > 0)
                {
                    sda.SelectCommand.Parameters.AddRange(par);
                }
                DataTable dt = new DataTable();
                sda.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// 动态Model转为SqlParameter[]
        /// </summary>
        /// <param name="model"></param>
        /// <returns>SqlParameter[]</returns>
        private SqlParameter[] GetParametersByModel(dynamic model)
        {
            return GetParameterListByModel(model).ToArray();
        }
        /// <summary>
        /// 动态Model转为SqlParameter[]
        /// </summary>
        /// <param name="model"></param>
        /// <returns>SqlParameter[]</returns>
        private List<SqlParameter> GetParameterListByModel(dynamic model)
        {
            if (model == null) return new List<SqlParameter>();
            List<SqlParameter> parms = new List<SqlParameter>();
            try
            {
                Type parmType = model.GetType();
                if (model is Dictionary<string, object>)
                {
                    foreach (var item in model)
                    {
                        parms.Add(new SqlParameter
                        {
                            ParameterName = item.Key,
                            Value = item.Value
                        });
                    }
                }
                else
                {
                    var proarr = parmType.GetProperties();
                    foreach (var item in proarr)
                    {
                        if (item.GetValue(model) != null)
                        {
                            parms.Add(new SqlParameter
                            {
                                ParameterName = item.Name,
                                Value = item.GetValue(model)
                            });
                        }
                    }
                }

                return parms;
            }
            catch (Exception e)
            {
                throw new Exception("SQL参数出错：" + e.Message);
            }
        }

        public T Query<T>(string sql, dynamic parms = null, int index = 0) where T : class, new()
        {
            var modelList = Query<T>(sql, parms);
            return modelList.Count > index ? modelList[index] : null;
        }
        public List<T> Query<T>(string sql) where T : class, new()
        {
            return Query<T>(sql, null);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <returns>List<T></returns>
        public List<T> Query<T>(string sql, dynamic parms = null) where T : class, new()
        {
            return getSqlCommand(sql, (command) =>
            {
                List<T> ret = new List<T>();
                var par = GetParametersByModel(parms);
                if (par != null && par.Length > 0)
                {
                    command.Parameters.AddRange(par);
                }
                using (SqlDataReader sdr = command.ExecuteReader())
                {
                    object data;
                    while (sdr.Read())
                    {
                        T entity = new T();
                        var modelType = entity.GetType();
                        Type t = typeof(T);
                        for (int i = 0; i < sdr.FieldCount; i++)
                        {
                            data = sdr.GetValue(i);
                            var modelPropertyInfo = modelType.GetProperty(sdr.GetName(i));
                            if (modelPropertyInfo != null)
                                modelPropertyInfo.SetValue(entity, data == DBNull.Value ? null : data, modelPropertyInfo.GetIndexParameters());
                        }
                        ret.Add(entity);
                    }
                }
                return ret;
            });
        }
        public List<JSON> Query(string sql, dynamic parms = null)
        {
            return getSqlCommand(sql, (command) =>
            {
                List<JSON> ret = new List<JSON>();
                var par = GetParametersByModel(parms);
                if (par != null && par.Length > 0)
                {
                    command.Parameters.AddRange(par);
                }
                using (SqlDataReader sdr = command.ExecuteReader())
                {
                    object data;
                    while (sdr.Read())
                    {
                        JSON json = new JSON();
                        for (int i = 0; i < sdr.FieldCount; i++)
                        {
                            data = sdr.GetValue(i);
                            if (data == DBNull.Value)
                                data = null;
                            json.Add(sdr.GetName(i), data == null ? "" : data.GetType().Name == "DateTime" ? ((DateTime)data).ToString("yyyy-MM-dd HH:mm:ss") : data);
                        }
                        ret.Add(json);
                    }
                }
                return ret;
            });
        }


        public JSON Query(string sql, int index, dynamic parms = null)
        {
            var result = Query(sql, parms);
            if (result.Count > index)
                return result[index];
            return null;
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">载体类型</typeparam>
        /// <param name="sql">要查询的表</param>
        /// <param name="page">页数</param>
        /// <param name="limit">宽度</param>
        /// <param name="total">总数</param>
        /// <param name="shortBy">排序字段</param>
        /// <param name="orderBy">排序方式</param>
        /// <param name="parms">参数化</param>
        /// <returns></returns>
        public List<T> Query<T>(string sql, int page, int limit, out int total, string shortBy = "id", string orderBy = "desc", dynamic parms = null) where T : class, new()
        {
            string pageSql = @"select top (" + limit + @") * from(
                               select row_number() over(order by " + shortBy + " " + orderBy + @") as rownumber, *from
                               (" + sql + @")__tempTable
                               )temp_table where rownumber > (" + page + " - 1) * " + limit;
            string countSql = "select count(0) from (" + sql + ")pager";
            total = ExecuteScalar(countSql, parms);
            return Query<T>(pageSql, parms);
        }




        public List<JSON> Query(string sql, int page, int limit, out int total, string shortBy = "id", string orderBy = "desc", dynamic parms = null)
        {
            string pageSql = @"WITH CTE AS(
    SELECT    ROW_NUMBER() OVER (order by " + shortBy + " " + orderBy + @") AS number,
	*
    FROM  (" + sql + @")__tempTable
) 
SELECT * FROM CTE
WHERE   number BETWEEN (" + page + "-1)*" + limit + "+1 AND " + page + "*" + limit + ";";
            string countSql = "select count(0) from (" + sql + ")pager";
            total = ExecuteScalar(countSql, parms);
            return Query(pageSql, parms);
        }

        /// <summary>
        /// 查询首行首列，返回object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, dynamic model)
        {
            var par = GetParametersByModel(model);
            return getSqlCommand(sql, (com) =>
            {
                if (par != null && par.Length > 0)
                {
                    com.Parameters.AddRange(par);
                }

                return com.ExecuteScalar();
            });
        }

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="sql">sql语句 参数如@Name在model中传过来</param>
        /// <param name="model">参数new{Name="value"}</param>
        /// <param name="defaultValue">默认返回值 0,"" 等</param>
        /// <returns></returns>
        public T? GetValue<T>(string sql, dynamic model = null, dynamic defaultValue = null) where T : struct
        {
            var par = GetParametersByModel(model);

            return getSqlCommand(sql, (com) =>
            {
                if (par != null && par.Length > 0)
                    com.Parameters.AddRange(par);
                if (com.ExecuteScalar() == null) return defaultValue;
                return (T)com.ExecuteScalar();
            });
        }



        /// <summary>
        /// 在线查询，返回SqlDataReader，存储过程
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        private SqlDataReader ExecuteReader1(string procname, dynamic model)
        {
            var par = GetParametersByModel(model);
            return getSqlCommand(procname, (com) =>
            {
                com.CommandType = CommandType.StoredProcedure;
                if (par != null && par.Length > 0)
                {
                    com.Parameters.AddRange(par);
                }
                return com.ExecuteReader(CommandBehavior.CloseConnection);
            });
        }

        /// <summary>
        /// 在线查询，返回SqlDataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string procname, dynamic model)
        {
            var par = GetParametersByModel(model);
            return getSqlCommand(procname, (com) =>
            {
                if (par != null && par.Length > 0)
                {
                    com.Parameters.AddRange(par);
                }

                return com.ExecuteReader(CommandBehavior.CloseConnection);
            });
        }

        /// <summary>
        /// 增删改方法
        /// </summary>
        /// <param name="sql"></param>
        public int ExecuteNonQuery(string sql, dynamic model)
        {
            var par = GetParametersByModel(model);

            return getSqlCommand(sql, com =>
            {
                if (par != null && par.Length > 0)
                {
                    com.Parameters.AddRange(par);
                }
                return com.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// 增删改方法，存储过程
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="par"></param>
        /// <returns></returns>
        public int ExecuteNonQueryProc(string sql, dynamic model)
        {
            var par = GetParametersByModel(model);
            return getSqlCommand(sql, com =>
            {
                com.CommandTimeout = 60;
                com.CommandType = CommandType.StoredProcedure;
                if (par != null && par.Length > 0)
                {
                    com.Parameters.AddRange(par);
                }
                return com.ExecuteNonQuery();
            });
        }
    }
}