﻿using Assets.Common.Attributes;
using Assets.Common.Entity;
using Assets.Common.Tools;
using Assets.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;


// \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
// \\                                       \\
// \\                teenyda                \\
// \\                                       \\
// \\     __                       __       \\
// \\    / /____ ___ ___  __ _____/ /__ _   \\
// \\   / __/ -_) -_) _ \/ // / _  / _ `/   \\
// \\   \__/\__/\__/_//_/\_, /\_,_/\_,_/    \\
// \\                   /___/               \\
// \\                                       \\
// \\                                       \\
// \\                                       \\
// \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

/// 2020/6/5


namespace Assets.Common.Dao
{

    public abstract class CommonDao<T> where T : TableEntity, new()
    {

        public CommonDao()
        {
            setTableName();
            getPrimaryKey();
            setSqlCmd();
        }

        /// <summary>
        /// 表格名
        /// </summary>
        private string tableName;

        /// <summary>
        /// 主键 
        /// </summary>
        private string primaryKey;

        /// <summary>
        /// 获取全部数据
        /// </summary>
        private string cmd_select_all = "select * from {0}";

        /// <summary>
        /// 根据主键获取数据
        /// </summary>
        private string cmd_select_by_id = "select * from {0} where {1} = ";

        /// <summary>
        /// 根据某个字段获取数据
        /// </summary>
        private string cmd_select_by_field = "select * from {0} where {1} = {2}";

        /// <summary>
        /// 获取id
        /// 如果是0需要子类实现的方法initId()获取id
        /// </summary>
        private string cmd_select_last_id = "select isnull(max({0}),0) from {1}";

        private string cmd_delete_by_id = "delete from {0} where {1} = {2}";

        /// <summary>
        /// 插入语句
        /// </summary>
        private string cmd_insert_all = "";


        /// <summary>
        /// 设置表格名称
        /// </summary>
        private void setTableName()
        {
            tableName = getEntityName();
        }

        /// <summary>
        /// 设置主键
        /// </summary>
        /// <param name="key"></param>
        private void setPrimaryKey(string key)
        {
            primaryKey = key;
        }

        /// <summary>
        /// 初始化sql命令
        /// </summary>
        private void setSqlCmd()
        {
            cmd_select_all = String.Format(cmd_select_all, tableName);
            cmd_select_by_id = String.Format(cmd_select_by_id, tableName, primaryKey);
            cmd_select_last_id = String.Format(cmd_select_last_id, primaryKey, tableName);
            buildInsertSql();

        }


        private SqlConnection getConnection()
        {
            //SqlConnection conn = SQL.getConnectionByLocal(SQL.DATABASE_NAME);

            SqlConnection conn = SQL.getConnByConfig();
            conn.Open();
            return conn;
        }

        private void closeConnection(SqlConnection conn)
        {
            conn.Close();
        }


        /// <summary>
        /// 获取T的类名
        /// </summary>
        /// <returns></returns>
        private string getEntityName()
        {
            Type type = typeof(T);
            string name = type.Name;//获取当前成员的名称
            string fullName = type.FullName;//获取类的全部名称不包括程序集
            string nameSpace = type.Namespace;//获取该类的命名空间
            var assembly = type.Assembly;//获取该类的程序集名
            var module = type.Module;//获取该类型的模块名            
            var memberInfos = type.GetMembers();//得到所有公共成员
            TableAliasAttribute tableAlias = (TableAliasAttribute)type.GetCustomAttribute(typeof(TableAliasAttribute), true);
            if (tableAlias != null)
                return tableAlias.Alias;
            return name.ToLower();
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        private void getPrimaryKey()
        {
            PropertyInfo[] peroperties = typeof(T).GetProperties();//BindingFlags.Public | BindingFlags.Instance

            foreach (PropertyInfo property in peroperties)
            {
                object[] objs = property.GetCustomAttributes(typeof(TableFieldAttribute), true);
                if (objs.Length > 0)
                {
                    Console.WriteLine("{0}: {1}", property.Name, ((TableFieldAttribute)objs[0]).FieldName);
                    //属性名 BrandId
                    string propertyName = property.Name;
                    //表字段名 brand_id
                    string fieldName = ((TableFieldAttribute)objs[0]).FieldName;
                    //表类型 int varchar...
                    string fieldType = ((TableFieldAttribute)objs[0]).FieldType;
                    //主键 true false
                    bool primaryKey = ((TableFieldAttribute)objs[0]).PrimaryKey;

                    if (primaryKey)
                    {
                        setPrimaryKey(fieldName);
                        return;
                    }


                }
            }

        }

        /// <summary>
        /// 获取特性<propertyName,fieldName,fieldType,primaryKey>
        /// Dictionary存放属性名和特性字段
        /// </summary>
        private Dictionary<String, Object> getAttributeFieldByPropertyName(string pName)
        {
            Dictionary<String, Object> d = new Dictionary<String, Object>();

            PropertyInfo[] peroperties = typeof(T).GetProperties();//BindingFlags.Public | BindingFlags.Instance

            foreach (PropertyInfo property in peroperties)
            {
                object[] objs = property.GetCustomAttributes(typeof(TableFieldAttribute), true);
                if (objs.Length > 0)
                {
                    Console.WriteLine("{0}: {1}", property.Name, ((TableFieldAttribute)objs[0]).FieldName);
                    //属性名 BrandId
                    string propertyName = property.Name;

                    if (!propertyName.Equals(pName))
                        continue;

                    d.Add("propertyName", propertyName);

                    //表字段名 brand_id
                    string fieldName = ((TableFieldAttribute)objs[0]).FieldName;
                    d.Add("fieldName", fieldName);

                    //表类型 int varchar...
                    string fieldType = ((TableFieldAttribute)objs[0]).FieldType;
                    d.Add("fieldType", fieldType);

                    //主键 true false
                    bool primaryKey = ((TableFieldAttribute)objs[0]).PrimaryKey;
                    d.Add("primaryKey", primaryKey);
                }
            }

            return d;
        }

        /// <summary>
        /// 获取特性<propertyName,fieldName,fieldType,primaryKey>
        /// Dictionary存放属性名和特性字段
        /// </summary>
        private Dictionary<String, Object> getAttributeFieldByFiledName(string name)
        {
            string propertyName = Tool.analysisFieldName(name);
            return getAttributeFieldByPropertyName(propertyName);
        }

        /// <summary>
        /// 从类获取所有特性的表字段
        /// </summary>
        private List<string> getTableFields()
        {
            List<string> list = new List<string>();

            PropertyInfo[] peroperties = typeof(T).GetProperties();//BindingFlags.Public | BindingFlags.Instance

            foreach (PropertyInfo property in peroperties)
            {
                object[] objs = property.GetCustomAttributes(typeof(TableFieldAttribute), true);
                if (objs.Length > 0)
                {

                    //表字段名 brand_id
                    string fieldName = ((TableFieldAttribute)objs[0]).FieldName;
                    list.Add(fieldName);
                }
            }

            return list;
        }



        /// <summary>
        /// SqlDataReader
        /// </summary>
        /// <returns></returns>
        protected List<T> findAll()
        {
            //string cmd = String.Format(cmd_select_all, tableName);

            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand(cmd_select_all, conn);
            SqlDataReader dr = com.ExecuteReader();
            List<T> list = new List<T>();
            while (dr.Read())
            {
                T t = foreachField(dr);
                list.Add(t);
            }

            closeConnection(conn);

            return list;
        }


        /// <summary>
        /// 存放数据到 DataTable
        /// </summary>
        private List<T> findAllByDataTable()
        {
            SqlConnection conn = getConnection();

            List<T> list = new List<T>();

            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd_select_all, conn);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds, tableName);

            DataTable dt = ds.Tables[tableName];
            // 行
            foreach (DataRow dataRow in dt.Rows)
            {
                List<string> fields = getTableFields();
                T t = new T();

                int index = 0;
                //属性
                foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
                {
                    string fieldName = Tool.analysisFieldName(fields[index]);
                    if (p.Name.Equals(fieldName))
                    {
                        try
                        {

                            string value = dataRow[fields[index++]].ToString();
                            object[] objs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                            //如果有枚举关联
                            if (objs.Length > 0)
                            {
                                object enumValue = Enum.Parse(((EnumFieldAttribute)objs[0]).EnumClazz, value);
                                p.SetValue(t, enumValue.ToString());
                            }
                            else
                            {
                                typeConversion(p, value, t);
                            }


                        }
                        catch (ArgumentException e)
                        {

                        }

                    }
                }

                list.Add(t);

            }

            return list;
        }

        /// <summary>
        /// 使用特性的TableFieldAttribute字段与表字段对比
        /// </summary>
        /// <returns></returns>
        private List<T> findAllByDataTable1()
        {
            SqlConnection conn = getConnection();

            List<T> list = new List<T>();

            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd_select_all, conn);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds, tableName);

            DataTable dt = ds.Tables[tableName];
            // 行
            foreach (DataRow dataRow in dt.Rows)
            {
                // 获取所有表字段
                List<string> fields = getTableFields();
                T t = new T();

                int index = 0;
                //属性
                foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
                {
                    //string fieldName = Tool.analysisFieldName(fields[index]);
                    object[] objs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);
                    if (objs.Length > 0)
                    {

                        //表字段名 brand_id
                        string attributeFieldName = ((TableFieldAttribute)objs[0]).FieldName;
                        if (attributeFieldName.Equals(fields[index]))
                        {
                            try
                            {

                                string value = dataRow[fields[index++]].ToString();
                                object[] objs1 = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                                //如果有枚举关联
                                if (objs1.Length > 0)
                                {
                                    object enumValue = Enum.Parse(((EnumFieldAttribute)objs1[0]).EnumClazz, value);
                                    p.SetValue(t, enumValue.ToString());
                                }
                                else
                                {
                                    typeConversion(p, value, t);
                                }


                            }
                            catch (ArgumentException e)
                            {

                            }

                        }
                    }

                }

                list.Add(t);

            }

            return list;
        }


        protected T findById(int id)
        {
            cmd_select_by_id += id;

            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand(cmd_select_by_id, conn);
            SqlDataReader dr = com.ExecuteReader();
            T t = null;
            while (dr.Read())
            {
                t = foreachField(dr);
            }

            closeConnection(conn);

            return t;
        }

        protected T findByField()
        {
            return null;
        }


        /// <summary>
        /// 遍历字段，通过反射赋值给对象
        /// by teenyda
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private T foreachField(SqlDataReader dr)
        {
            T t = new T();
            int count = dr.VisibleFieldCount;
            // 遍历实体字段，利用反射赋值给对象
            for (int i = 0; i < count; i++)
            {
                string name = dr.GetName(i);
                string fieldName = Tool.analysisFieldName(name);
                object value = dr.GetValue(i);

                foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
                {
                    if (p.Name.Equals(fieldName))
                    {
                        object[] objs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                        //如果有枚举关联
                        if (objs.Length > 0)
                        {
                            object enumValue = Enum.Parse(((EnumFieldAttribute)objs[0]).EnumClazz, Convert.ToString(value));
                            p.SetValue(t, enumValue.ToString());
                        }
                        else
                        {
                            typeConversion(p, value, t);
                        }

                        break;

                    }
                }
            }

            return t;

        }





        private void buildInsertSql()
        {
            List<string> fields = getTableFields();
            string cmd = String.Format("insert into {0}", tableName) + "(";
            foreach (string field in fields)
            {
                cmd += field + ",";
            }
            cmd = cmd.Substring(0, cmd.Length - 1) + ")";

            cmd += " values(";

            foreach (string field in fields)
            {
                cmd += "@" + field + ",";
            }

            cmd = cmd.Substring(0, cmd.Length - 1) + ")";
            cmd_insert_all = cmd;
        }

        protected void executeSql(string sql)
        {
            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand(sql, conn);

            com.ExecuteNonQuery();

            closeConnection(conn);
        }

        protected void executeSql(string sql, List<SqlParameter> parameters)
        {
            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand(sql, conn);

            foreach(SqlParameter sp in parameters)
            {
                com.Parameters.Add(sp);
            }

            com.ExecuteNonQuery();

            closeConnection(conn);
        }

        protected DataSet executeReaderSql(string sql, string tableName)
        {
            SqlConnection conn = getConnection();

            //SqlCommand com = new SqlCommand(sql, conn);

            SqlDataAdapter sda = new SqlDataAdapter(sql, conn);

            DataSet ds = new DataSet();

            sda.Fill(ds, tableName);

            return ds;
        }

        /// <summary>
        /// 添加实体类至数据库
        /// </summary>
        /// <param name="t"></param>
        protected void insert(T t)
        {
            if (t == null)
                return;
            List<string> fields = getTableFields();
            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand(cmd_insert_all, conn);

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                //遍历表的字段
                foreach (string field in fields)
                {
                    if (p.Name.Equals(Tool.analysisFieldName(field)))
                    {

                        object[] fieldObjs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);
                        //如果是主键 设置id值
                        if (fieldObjs.Length > 0)
                        {
                            //主键 true false
                            bool primaryKey = ((TableFieldAttribute)fieldObjs[0]).PrimaryKey;
                            if (primaryKey)
                            {
                                if (p.GetValue(t) == null)
                                {
                                    int id = getLastId() + 1;
                                    p.SetValue(t, id);
                                }

                            }
                        }

                        object[] objs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                        //如果有枚举关联 转换为数值 已启用->1
                        if (objs.Length > 0)
                        {
                            object value = p.GetValue(t);
                            object defaultValue = ((TableFieldAttribute)fieldObjs[0]).DefaultValue;
                            if (value == null && defaultValue == null)
                            {
                                throw new System.Exception("对象的值为空，请检查或注释特性!");
                            }
                            Type type = ((EnumFieldAttribute)objs[0]).EnumClazz;

                            Enum e = null;

                            if (value != null)
                            {
                                e = (Enum)Enum.Parse(type, Convert.ToString(value));

                            }

                            if (defaultValue != null)
                            {
                                e = (Enum)Enum.Parse(type, Convert.ToString(defaultValue));
                            }

                            com.Parameters.Add(new SqlParameter("@" + field, Convert.ToInt32(e)));

                        }
                        else
                        {
                            object value = p.GetValue(t);
                            object defaultValue = ((TableFieldAttribute)fieldObjs[0]).DefaultValue;
                            if (value == null && defaultValue == null)
                            {
                                throw new System.Exception("对象的值为空，请检查或注释特性!");
                            }
                            if(value != null)
                            {
                                com.Parameters.Add(new SqlParameter("@" + field, p.GetValue(t)));
                            }
                            if(defaultValue != null)
                            {
                                com.Parameters.Add(new SqlParameter("@" + field, defaultValue));
                            }
                        }

                    }

                }
            }
            //执行sql
            int res = com.ExecuteNonQuery();

            closeConnection(conn);

        }

        /// <summary>
        /// 更新实体类(by id)
        /// </summary>
        /// <param name="t"></param>
        protected void update(T t)
        {
            if (t == null)
                return;

            string updateCmd = "update {0} set ";
            updateCmd = String.Format(updateCmd, tableName);

            string primaryKeyName = string.Empty;
            object primaryKeyValue = string.Empty;

            List<string> fields = getTableFields();
            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand();

            /////////////////////
            /// 构建set = ...语句
            /////////////////////

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                //遍历表的字段
                foreach (string field in fields)
                {
                    string filedName = Tool.analysisFieldName(field);
                    if (p.Name.Equals(filedName))
                    {

                        object o = p.GetValue(t);

                        if (o == null)
                            continue;

                        object[] fieldObjs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);
                        //如果是主键
                        if (fieldObjs.Length > 0)
                        {
                            //主键 true false
                            bool primaryKey = ((TableFieldAttribute)fieldObjs[0]).PrimaryKey;
                            if (primaryKey)
                            {
                                //主键字段名 brand_id
                                //primaryKeyName = ((TableFieldAttribute)fieldObjs[0]).FieldName;
                                primaryKeyName = field;
                                primaryKeyValue = p.GetValue(t);
                                continue;
                            }

                        }

                        updateCmd += field + "=@" + field + ",";

                    }

                }
            }
            updateCmd = updateCmd.Substring(0, updateCmd.Length - 1);

            updateCmd += " where {0} = {1}";
            updateCmd = String.Format(updateCmd, primaryKeyName, primaryKeyValue);


            /////////////////////
            /// Parameters赋值
            /////////////////////

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                //遍历表的字段
                foreach (string field in fields)
                {
                    if (p.Name.Equals(Tool.analysisFieldName(field)))
                    {
                        object o = p.GetValue(t);

                        if (o == null)
                            continue;


                        object[] objs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                        //如果有枚举关联 转换为数值 已启用->1
                        if (objs.Length > 0)
                        {
                            object value = p.GetValue(t);
                            Type type = ((EnumFieldAttribute)objs[0]).EnumClazz;

                            Enum e = (Enum)Enum.Parse(type, Convert.ToString(value));

                            com.Parameters.Add(new SqlParameter("@" + field, Convert.ToInt32(e)));
                        }
                        else
                        {
                            com.Parameters.Add(new SqlParameter("@" + field, p.GetValue(t)));
                        }

                    }

                }
            }

            com.CommandText = updateCmd;
            com.Connection = conn;

            //执行sql
            int res = com.ExecuteNonQuery();

            closeConnection(conn);
        }

        /// <summary>
        /// 更新实体类(by id)
        /// </summary>
        /// <param name="t"></param>
        protected void updateByPrimarykey(T t)
        {
            if (t == null)
                return;

            string updateCmd = "update {0} set ";
            updateCmd = String.Format(updateCmd, tableName);

            string primaryKeyName = string.Empty;
            object primaryKeyValue = string.Empty;

            List<string> fields = getTableFields();
            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand();

            /////////////////////
            /// 构建set = ...语句
            /////////////////////

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                //遍历表的字段
                foreach (string field in fields)
                {
                    string filedName = Tool.analysisFieldName(field);
                    if (p.Name.Equals(filedName))
                    {

                        object o = p.GetValue(t);

                        if (o == null)
                            continue;

                        object[] fieldObjs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);
                        //如果是主键
                        if (fieldObjs.Length > 0)
                        {
                            //主键 true false
                            bool primaryKey = ((TableFieldAttribute)fieldObjs[0]).PrimaryKey;
                            if (primaryKey)
                            {
                                //主键字段名 brand_id
                                //primaryKeyName = ((TableFieldAttribute)fieldObjs[0]).FieldName;
                                primaryKeyName = field;
                                primaryKeyValue = p.GetValue(t);
                                continue;
                            }

                        }

                        updateCmd += field + "=@" + field + ",";

                    }

                }
            }
            updateCmd = updateCmd.Substring(0, updateCmd.Length - 1);

            updateCmd += " where {0} = @{0}";
            updateCmd = String.Format(updateCmd, primaryKeyName);


            /////////////////////
            /// Parameters赋值
            /////////////////////

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                //遍历表的字段
                foreach (string field in fields)
                {
                    if (p.Name.Equals(Tool.analysisFieldName(field)))
                    {
                        object o = p.GetValue(t);

                        if (o == null)
                            continue;


                        object[] objs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                        //如果有枚举关联 转换为数值 已启用->1
                        if (objs.Length > 0)
                        {
                            object value = p.GetValue(t);
                            Type type = ((EnumFieldAttribute)objs[0]).EnumClazz;

                            Enum e = (Enum)Enum.Parse(type, Convert.ToString(value));

                            com.Parameters.Add(new SqlParameter("@" + field, Convert.ToInt32(e)));
                        }
                        else
                        {
                            //包括主键
                            com.Parameters.Add(new SqlParameter("@" + field, p.GetValue(t)));
                        }

                    }

                }
            }

            com.CommandText = updateCmd;
            com.Connection = conn;

            //执行sql
            int res = com.ExecuteNonQuery();

            closeConnection(conn);
        }

        /// <summary>
        /// 根据主键id删除
        /// </summary>
        /// <param name="id"></param>
        protected void delete(int id)
        {
            SqlConnection conn = getConnection();
            string tag = cmd_delete_by_id;
            tag = String.Format(cmd_delete_by_id, tableName, primaryKey, id);
            SqlCommand com = new SqlCommand(tag, conn);

            com.ExecuteNonQuery();
            closeConnection(conn);
        }

        /// <summary>
        /// 获取最后一个ID
        /// </summary>
        public int getLastId()
        {
            SqlConnection conn = getConnection();

            SqlCommand com = new SqlCommand(cmd_select_last_id, conn);
            SqlDataReader reader = com.ExecuteReader();
            if (reader.Read())
            {
                object o = reader.GetValue(0);
                int id = Convert.ToInt32(o);
                return id == 0 ? initId() : id;
            }

            //返回个子类实现的方法获取id
            return initId();

        }

        /// <summary>
        /// 初始化id值
        /// </summary>
        /// <returns></returns>
        protected abstract int initId();


        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="p"></param>
        /// <param name="value"></param>
        /// <param name="t"></param>
        private void typeConversion(PropertyInfo p, object value, T t)
        {
            if (p.PropertyType == typeof(string))
            {
                if (string.IsNullOrEmpty(Convert.ToString(value)))
                {
                    p.SetValue(t, string.Empty);
                }
                else
                {
                    p.SetValue(t, value);

                }
            }
            if (p.PropertyType == typeof(int) || p.PropertyType == typeof(uint))
            {
                p.SetValue(t, Convert.ToInt32(value));
            }
            if (p.PropertyType == typeof(DateTime))
            {
                //
            }
            if (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(double) || p.PropertyType == typeof(float))
            {
                //
            }

            if (p.PropertyType == typeof(bool))
            {
                p.SetValue(t, Convert.ToBoolean(value));
            }
            if (p.PropertyType == typeof(sbyte))
            {
                //
            }
            if (p.PropertyType == typeof(byte) || p.PropertyType == typeof(short) || p.PropertyType == typeof(ushort))
            {
                //
            }
            if (p.PropertyType == typeof(long) || p.PropertyType == typeof(ulong))
            {
                //
            }

            //values +=string.Format( "{0}={1},", p.Name, p.GetValue(t));
            //Console.WriteLine("Name:{0} Value:{1}", p.Name, p.GetValue(t));
        }

        protected List<T> selectByField(T t)
        {
            if (t == null)
                return null;

            //select * from tableName
            string sql = cmd_select_all;

            sql += " where ";

            List<string> fields = getTableFields();
            SqlConnection conn = getConnection();

            object primaryKeyValue = null;

            SqlCommand com = new SqlCommand(cmd_insert_all, conn);

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                //遍历表的字段
                foreach (string field in fields)
                {
                    if (p.Name.Equals(Tool.analysisFieldName(field)))
                    {

                        if (p.GetValue(t) == null)
                            break;

                        var type = p.PropertyType.Name;

                        if (type.Equals("Int32"))
                        {
                            int val = Convert.ToInt32(p.GetValue(t));
                            if (val == 0)
                                break;
                        }

                        object[] fieldObjs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);

                        if (fieldObjs.Length > 0)
                        {
                            //主键 true false
                            bool primaryKey = ((TableFieldAttribute)fieldObjs[0]).PrimaryKey;
                            if (primaryKey)
                            {
                                primaryKeyValue = p.GetValue(t);
                                break;

                            }
                            else
                            {
                                string fieldName = ((TableFieldAttribute)fieldObjs[0]).FieldName;
                                sql += fieldName + "=@" + fieldName + " and ";
                                break;
                            }

                        }

                    }

                }
            }

            if (primaryKeyValue == null)
            {
                sql = sql.Substring(0, sql.Length - 5);
            }
            else
            {
                sql += this.primaryKey + "=@" + this.primaryKey;

            }


            /////////////////////
            /// Parameters赋值
            /////////////////////

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                object o = p.GetValue(t);

                if (o == null)
                    continue;

                var type1 = p.PropertyType.Name;

                if (type1.Equals("Int32"))
                {
                    int val = Convert.ToInt32(p.GetValue(t));
                    if (val == 0)
                        continue;
                }

                string field = string.Empty;

                object[] fieldObjs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);

                if (fieldObjs.Length > 0)
                {
                    //获取表字段
                    field = ((TableFieldAttribute)fieldObjs[0]).FieldName;

                }


                object[] objs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);

                //如果有枚举关联 转换为数值 已启用->1
                if (objs.Length > 0)
                {
                    object value = p.GetValue(t);
                    Type type = ((EnumFieldAttribute)objs[0]).EnumClazz;

                    Enum e = (Enum)Enum.Parse(type, Convert.ToString(value));

                    com.Parameters.Add(new SqlParameter("@" + field, Convert.ToInt32(e)));
                }
                else
                {
                    com.Parameters.Add(new SqlParameter("@" + field, p.GetValue(t)));
                }
            }

            com.CommandText = sql;

            //执行sql
            SqlDataReader sdr = com.ExecuteReader();

            List<T> list = new List<T>();
            while (sdr.Read())
            {
                T t1 = foreachField(sdr);
                list.Add(t1);
            }

            closeConnection(conn);

            return list;

        }


        //-------------------------------------------
        //-------------------------------------------
        //小更新

        // \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        // \\                                       \\
        // \\                teenyda                \\
        // \\                                       \\
        // \\     __                       __       \\
        // \\    / /____ ___ ___  __ _____/ /__ _   \\
        // \\   / __/ -_) -_) _ \/ // / _  / _ `/   \\
        // \\   \__/\__/\__/_//_/\_, /\_,_/\_,_/    \\
        // \\                   /___/               \\
        // \\                                       \\
        // \\                                       \\
        // \\                                       \\
        // \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        /// 2020/6/25

        /// <summary>
        /// by teenyda
        /// 遍历对象的属性，获取特性值
        /// TableFieldAttribute、EnumFieldAttribute
        /// </summary>
        /// <param name="t"></param>
        /// <param name="iTake"></param>
        private void foreachAttribute(T t, ITakeAttribute iTake)
        {


            if (iTake == null)
                throw new System.Exception("ITakeAttribute对象为空，请检查");

            //遍历p的属性
            foreach (System.Reflection.PropertyInfo p in t.GetType().GetProperties())
            {
                iTake.onTakeAttribute(p);

                object[] tableObjs = p.GetCustomAttributes(typeof(TableFieldAttribute), true);
                //表字段特性
                if (tableObjs.Length > 0)
                {
                    TableFieldAttribute tableField = (TableFieldAttribute)tableObjs[0];
                    iTake.onTakeTableFieldAttribute(tableField);

                    if (tableField.PrimaryKey && p.GetValue(t) != null)
                    {
                        iTake.onTakePrimaryKey(tableField.FieldName, p.GetValue(t));
                    }
                }

                object[] enumObjs = p.GetCustomAttributes(typeof(EnumFieldAttribute), true);
                //枚举特性
                if (enumObjs.Length > 0)
                {
                    iTake.onTakeEnumFieldAttribute((EnumFieldAttribute)enumObjs[0]);
                }
            }

        }


        interface ITakeAttribute
        {
            //当获取属性时
            void onTakeAttribute(System.Reflection.PropertyInfo p);

            //当获取到主键时
            void onTakePrimaryKey(string primaryKeyName, object primaryKeyValue);

            //当获取到TableFieldAttribute
            void onTakeTableFieldAttribute(TableFieldAttribute tableField);

            //当获取到枚举特性EnumFieldAttribute
            void onTakeEnumFieldAttribute(EnumFieldAttribute enumField);

        }



    }


}
