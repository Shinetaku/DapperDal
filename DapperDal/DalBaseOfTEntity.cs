using Dapper;
using DapperDal.Mapper;
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
            DapperConfiguration.Default.DefaultMapper = typeof(AutoEntityMapper<>);
            DapperConfiguration.Default.Nolock = true;
            DapperConfiguration.Default.Buffered = true;
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
            // ��ʼ��������
            InitOptions();

            ConnectionString = ResolveConnectionString(connNameOrConnStr);
        }

        /// <summary>
        /// ������
        /// </summary>
        public DapperConfiguration Configuration
        {
            get { return DapperConfiguration.Default; }
        }

        /// <summary>
        /// ������
        /// </summary>
        public DalOptions Options { get; private set; }

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
            return OpenConnection(ConnectionString);
        }

        /// <summary>
        /// ��DB����
        /// </summary>
        /// <param name="connNameOrConnStr">DB �����ַ������ýڵ���</param>
        /// <returns>DB����</returns>
        protected virtual IDbConnection OpenConnection(string connNameOrConnStr)
        {
            var connectionString = ResolveConnectionString(connNameOrConnStr);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            var connection = new SqlConnection(connectionString);
            if (connection == null)
                throw new ConfigurationErrorsException(
                    string.Format("Failed to create a connection using the connection string '{0}'.", connectionString));

            connection.Open();

            return connection;
        }

        /// <summary>
        /// ��ʼ��������
        /// </summary>
        private void InitOptions()
        {
            if (Options == null)
            {
                Options = new DalOptions();

                Options.SoftDeletePropsFactory = () => new { IsActive = 0 };

                Options.SoftActivePropsFactory = () => new { IsActive = 1 };
            }
        }

        /// <summary>
        /// ��ȡ DB ���Ӵ�
        /// </summary>
        /// <param name="connNameOrConnStr">DB �����ַ������ýڵ���</param>
        /// <returns>DB ���Ӵ�</returns>
        private string ResolveConnectionString(string connNameOrConnStr)
        {
            if (string.IsNullOrEmpty(connNameOrConnStr))
            {
                throw new ArgumentNullException("connNameOrConnStr");
            }

            if (connNameOrConnStr.Contains("=") || connNameOrConnStr.Contains(";"))
            {
                return connNameOrConnStr;
            }
            else
            {
                var conStr = ConfigurationManager.ConnectionStrings[connNameOrConnStr];
                if (conStr == null)
                {
                    throw new ConfigurationErrorsException(
                        string.Format("Failed to find connection string named '{0}' in app/web.config.", connNameOrConnStr));
                }

                return conStr.ConnectionString;
            }
        }

    }
}