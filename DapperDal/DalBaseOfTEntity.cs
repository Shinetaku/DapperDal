using Dapper;
using DapperDal.Mapper;
using DapperExtensions;
using DapperExtensions.Expressions;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperDal
{
    /// <summary>
    /// ʵ�����ݷ��ʲ����
    /// </summary>
    /// <typeparam name="TEntity">ʵ������</typeparam>
    public class DalBase<TEntity> : DalBase<TEntity, int> where TEntity : class
    {
        /// <summary>
        /// Ĭ�ϳ�ʼ�� DAL ��ʵ��
        /// </summary>
        public DalBase() : this("Default")
        {
        }

        /// <summary>
        /// �����ýڵ�����ʼ�� DAL ��ʵ��
        /// </summary>
        /// <param name="connectionName">DB�����ַ������ýڵ���</param>
        /// <exception cref="ArgumentNullException">����Ϊ��</exception>
        /// <exception cref="ConfigurationErrorsException">�Ҳ������ýڵ�</exception>
        public DalBase(string connectionName) : base(connectionName)
        {
        }
    }

    /// <summary>
    /// ʵ�����ݷ��ʲ����
    /// </summary>
    /// <typeparam name="TEntity">ʵ������</typeparam>
    /// <typeparam name="TPrimaryKey">ʵ��ID������������</typeparam>
    public partial class DalBase<TEntity, TPrimaryKey> where TEntity : class
    {
        static DalBase()
        {
            DapperExtensions.DapperExtensions.Configure(
                configuration =>
                {
                    configuration.DefaultMapper = typeof(AutoEntityMapper<>);
                    configuration.Nolock = true;
                    configuration.SoftDeletePropsFactory = () => new { IsActive = 0 };
                    configuration.Buffered = true;
                });
        }

        /// <summary>
        /// Ĭ�ϳ�ʼ�� DAL ��ʵ��
        /// </summary>
        public DalBase() : this("Default")
        {
        }

        /// <summary>
        /// �����ýڵ�����ʼ�� DAL ��ʵ��
        /// </summary>
        /// <param name="connNameOrConnStr">DB�����ַ������ýڵ���</param>
        /// <exception cref="ArgumentNullException">����Ϊ��</exception>
        /// <exception cref="ConfigurationErrorsException">�Ҳ������ýڵ�</exception>
        public DalBase(string connNameOrConnStr)
        {
            if (string.IsNullOrEmpty(connNameOrConnStr))
            {
                throw new ArgumentNullException("connNameOrConnStr");
            }

            if (connNameOrConnStr.Contains("=") || connNameOrConnStr.Contains(";"))
            {
                ConnectionString = connNameOrConnStr;
            }
            else
            {
                var conStr = ConfigurationManager.ConnectionStrings[connNameOrConnStr];
                if (conStr == null)
                {
                    throw new ConfigurationErrorsException(
                        string.Format("Failed to find connection string named '{0}' in app/web.config.", connNameOrConnStr));
                }

                ConnectionString = conStr.ConnectionString;
            }
        }

        /// <summary>
        /// DB�����ַ���
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// ��DB����
        /// </summary>
        /// <returns>DB����</returns>
        protected virtual IDbConnection OpenConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            var connection = new SqlConnection(ConnectionString);
            if (connection == null)
                throw new ConfigurationErrorsException(
                    string.Format("Failed to create a connection using the connection string '{0}'.", ConnectionString));

            connection.Open();

            return connection;
        }
    }
}