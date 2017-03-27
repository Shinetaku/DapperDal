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
    /// <typeparam name="TPrimaryKey">ʵ��ID������������</typeparam>
    public class DalBase<TEntity, TPrimaryKey> where TEntity : class
    {
        static DalBase()
        {
            DapperExtensions.DapperExtensions.Configure(
                typeof(AutoEntityMapper<>), new List<Assembly>(), new SqlServerDialect());
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
        /// <param name="connectionName">DB�����ַ������ýڵ���</param>
        /// <exception cref="ArgumentNullException">����Ϊ��</exception>
        /// <exception cref="ConfigurationErrorsException">�Ҳ������ýڵ�</exception>
        public DalBase(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                throw new ArgumentNullException("connectionName");
            }

            var conStr = ConfigurationManager.ConnectionStrings[connectionName];
            if (conStr == null)
            {
                throw new ConfigurationErrorsException(
                    string.Format("Failed to find connection string named '{0}' in app/web.config.", connectionName));
            }

            ConnectionString = conStr.ConnectionString;

            _connection = CreateConnection(ConnectionString);
        }

        /// <summary>
        /// ��DB���ӳ�ʼ�� DAL ��ʵ��
        /// </summary>
        /// <param name="connection">DB����</param>
        /// <exception cref="ArgumentNullException">����Ϊ��</exception>
        public DalBase(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            ConnectionString = connection.ConnectionString;

            _connection = connection;
        }

        /// <summary>
        /// DB�����ַ���
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// DB����ʵ��
        /// </summary>
        private IDbConnection _connection;

        /// <summary>
        /// ��ȡDB����ʵ��
        /// </summary>
        public virtual IDbConnection Connection
        {
            get { return OpenConnection(); }
        }

        /// <summary>
        /// ����DB����
        /// </summary>
        /// <param name="connectionString">DB�����ַ���</param>
        /// <returns>DB����</returns>
        private IDbConnection CreateConnection(string connectionString)
        {
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
        /// ��DB����
        /// </summary>
        /// <returns>DB����</returns>
        protected virtual IDbConnection OpenConnection()
        {
            if (_connection == null)
            {
                _connection = CreateConnection(ConnectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                if (string.IsNullOrEmpty(_connection.ConnectionString))
                {
                    _connection.ConnectionString = ConnectionString;
                }
                _connection.Open();
            }

            return _connection;
        }

        /// <summary>
        /// ����ָ��ʵ��
        /// </summary>
        /// <param name="entity">ʵ��</param>
        /// <returns>ʵ������</returns>
        public virtual TPrimaryKey Insert(TEntity entity)
        {
            using (Connection)
            {
                return Connection.Insert(entity);
            }
        }

        /// <summary>
        /// ��������ָ��ʵ�弯��
        /// </summary>
        /// <param name="entities">ʵ�弯��</param>
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            using (Connection)
            {
                Connection.Insert(entities);
            }
        }

        /// <summary>
        /// ɾ��ָ��ʵ��
        /// </summary>
        /// <param name="entity">ʵ��</param>
        /// <returns>ɾ�����</returns>
        public virtual bool Delete(TEntity entity)
        {
            using (Connection)
            {
                return Connection.Delete(entity);
            }
        }

        /// <summary>
        /// ��������ɾ��ʵ��
        /// </summary>
        /// <param name="predicate">ɾ������</param>
        /// <returns>ɾ�����</returns>
        public virtual bool Delete(object predicate)
        {
            using (Connection)
            {
                return Connection.Delete<TEntity>(predicate);
            }
        }

        /// <summary>
        /// ��������ɾ��ʵ��
        /// </summary>
        /// <param name="predicate">ɾ������</param>
        /// <returns>ɾ�����</returns>
        public virtual bool Delete(Expression<Func<TEntity, bool>> predicate)
        {
            using (Connection)
            {
                return Connection.Delete<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>());
            }
        }

        /// <summary>
        /// ����ָ��ʵ��
        /// </summary>
        /// <param name="entity">ʵ��</param>
        /// <returns>���½��</returns>
        public virtual bool Update(TEntity entity)
        {
            using (Connection)
            {
                return Connection.Update(entity);
            }
        }

        /// <summary>
        /// ����ָ��ʵ��ָ������
        /// </summary>
        /// <param name="entity">ʵ��</param>
        /// <param name="props">����������</param>
        /// <returns>���½��</returns>
        public virtual bool Update(TEntity entity, IEnumerable<string> props)
        {
            using (Connection)
            {
                return Connection.Update(entity, props.ToList());
            }
        }

        /// <summary>
        /// ����ָ��ָ������ID����ʵ��ָ������
        /// ������ʹ��ʵ������ID��
        /// </summary>
        /// <param name="entity">����ʵ�壬��������ID���������</param>
        /// <returns>���½��</returns>
        public virtual bool Update(object entity)
        {
            using (Connection)
            {
                return Connection.Update<TEntity>(entity);
            }
        }

        /// <summary>
        /// ����ָ��������������ʵ��ָ������
        /// ������ʹ��ν�ʻ���������
        /// </summary>
        /// <param name="entity">��������</param>
        /// <param name="predicate">����������ʹ��ν�ʻ���������</param>
        /// <returns>���½��</returns>
        public virtual bool Update(object entity, object predicate)
        {
            using (Connection)
            {
                return Connection.Update<TEntity>(entity, predicate);
            }
        }

        /// <summary>
        /// ����ָ��������������ʵ��ָ������
        /// ������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="entity">��������</param>
        /// <param name="predicate">����������ʹ�ñ��ʽ</param>
        /// <returns>���½��</returns>
        public virtual bool Update(object entity, Expression<Func<TEntity, bool>> predicate)
        {
            using (Connection)
            {
                return Connection.Update<TEntity>(entity, predicate.ToPredicateGroup<TEntity, TPrimaryKey>());
            }
        }

        /// <summary>
        /// ����ʵ��ID����������ȡʵ��
        /// </summary>
        /// <param name="id">ʵ��ID</param>
        /// <returns>ʵ��</returns>
        public virtual TEntity Get(TPrimaryKey id)
        {
            using (Connection)
            {
                return Connection.Get<TEntity>(id);
            }
        }

        /// <summary>
        /// ��ȡ����ʵ���б�
        /// </summary>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList()
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ������ȡʵ���б�
        /// ����ѯʹ��ν�ʻ���������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(object predicate)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(predicate);
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���б�
        /// ������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(null,
                    sortingExpression.ToSortable(ascending));
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���б�
        /// ����ѯʹ��ν�ʻ�������������ʹ��Sort����������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="sort">��������</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(object predicate, object sort)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(predicate, sort.ToSortable());
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���б�
        /// ����ѯʹ��ν�ʻ�������������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(object predicate,
            SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(predicate,
                    sortingExpression.ToSortable(ascending));
            }
        }

        /// <summary>
        /// ���ݲ�ѯ������ȡʵ���б�
        /// ����ѯʹ�ñ��ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>());
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���б�
        /// ����ѯʹ�ñ��ʽ������ʹ��Sort����������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="sort">��������</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate, object sort)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    sort.ToSortable());
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���б�
        /// ����ѯʹ�ñ��ʽ������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ���б�</returns>
        public virtual IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate,
            SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetList<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    sortingExpression.ToSortable(ascending));
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���ҳ�б�
        /// ����ѯʹ��ν�ʻ�������������ʹ��Sort����������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="sort">��������</param>
        /// <param name="pageNumber">ҳ�ţ���1��ʼ</param>
        /// <param name="itemsPerPage">ÿҳ����</param>
        /// <returns>ʵ���ҳ�б�</returns>
        public virtual IEnumerable<TEntity> GetListPaged(object predicate, object sort,
            int pageNumber, int itemsPerPage)
        {
            using (Connection)
            {
                return Connection.GetPage<TEntity>(predicate,
                    sort.ToSortable(), pageNumber - 1, itemsPerPage).ToList();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���ҳ�б�
        /// ����ѯʹ��ν�ʻ���������������ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="pageNumber">ҳ�ţ���1��ʼ</param>
        /// <param name="itemsPerPage">ÿҳ����</param>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ���ҳ�б�</returns>
        public virtual IEnumerable<TEntity> GetListPaged(object predicate,
            int pageNumber, int itemsPerPage,
            SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetPage<TEntity>(predicate,
                    sortingExpression.ToSortable(ascending),
                    pageNumber - 1, itemsPerPage).ToList();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���ҳ�б�
        /// ����ѯʹ�ñ��ʽ������ʹ��Sort����������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="sort">��������</param>
        /// <param name="pageNumber">ҳ�ţ���1��ʼ</param>
        /// <param name="itemsPerPage">ÿҳ����</param>
        /// <returns>ʵ���ҳ�б�</returns>
        public virtual IEnumerable<TEntity> GetListPaged(Expression<Func<TEntity, bool>> predicate, object sort,
            int pageNumber, int itemsPerPage)
        {
            using (Connection)
            {
                return Connection.GetPage<TEntity>(
                    predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    sort.ToSortable(),
                    pageNumber - 1,
                    itemsPerPage).ToList();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ���ҳ�б�
        /// ����ѯʹ�ñ��ʽ������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="pageNumber">ҳ�ţ���1��ʼ</param>
        /// <param name="itemsPerPage">ÿҳ����</param>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ���ҳ�б�</returns>
        public virtual IEnumerable<TEntity> GetListPaged(Expression<Func<TEntity, bool>> predicate,
            int pageNumber, int itemsPerPage,
            SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetPage<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    sortingExpression.ToSortable(ascending),
                    pageNumber - 1, itemsPerPage).ToList();
            }
        }


        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ�������б�
        /// ����ѯʹ��ν�ʻ�������������ʹ��Sort����������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="sort">��������</param>
        /// <param name="firstResult">��ʼ����</param>
        /// <param name="maxResults">�������</param>
        /// <returns>ʵ�������б�</returns>
        public virtual IEnumerable<TEntity> GetSet(object predicate, object sort,
            int firstResult, int maxResults)
        {
            using (Connection)
            {
                return Connection.GetSet<TEntity>(predicate, sort.ToSortable(),
                    firstResult, maxResults).ToList();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ�������б�
        /// ����ѯʹ��ν�ʻ���������������ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="firstResult">��ʼ����</param>
        /// <param name="maxResults">�������</param>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ�������б�</returns>
        public virtual IEnumerable<TEntity> GetSet(object predicate,
            int firstResult, int maxResults,
            SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetSet<TEntity>(predicate,
                    sortingExpression.ToSortable(ascending),
                    firstResult, maxResults).ToList();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ�������б�
        /// ����ѯʹ�ñ��ʽ������ʹ��Sort����������
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="sort">��������</param>
        /// <param name="firstResult">��ʼ����</param>
        /// <param name="maxResults">�������</param>
        /// <returns>ʵ�������б�</returns>
        public virtual IEnumerable<TEntity> GetSet(Expression<Func<TEntity, bool>> predicate, object sort,
            int firstResult, int maxResults)
        {
            using (Connection)
            {
                return Connection.GetSet<TEntity>(
                    predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    sort.ToSortable(),
                    firstResult, maxResults).ToList();
            }
        }

        /// <summary>
        /// ���ݲ�ѯ����������������ȡʵ�������б�
        /// ����ѯʹ�ñ��ʽ������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="predicate">��ѯ����</param>
        /// <param name="firstResult">��ʼ����</param>
        /// <param name="maxResults">�������</param>
        /// <param name="ascending">������</param>
        /// <param name="sortingExpression">�����ֶ�</param>
        /// <returns>ʵ�������б�</returns>
        public virtual IEnumerable<TEntity> GetSet(Expression<Func<TEntity, bool>> predicate,
            int firstResult, int maxResults,
            SortDirection ascending,
            params Expression<Func<TEntity, object>>[] sortingExpression)
        {
            using (Connection)
            {
                return Connection.GetSet<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>(),
                    sortingExpression.ToSortable(ascending),
                    firstResult, maxResults).ToList();
            }
        }

        /// <summary>
        /// ����������ȡʵ������
        /// ������ʹ��ν�ʻ���������
        /// </summary>
        /// <param name="predicate">������ʹ��ν�ʻ���������</param>
        /// <returns>ʵ������</returns>
        public virtual int Count(object predicate)
        {
            using (Connection)
            {
                return Connection.Count<TEntity>(predicate);
            }
        }

        /// <summary>
        /// ����������ȡʵ������
        /// ������ʹ�ñ��ʽ��
        /// </summary>
        /// <param name="predicate">������ʹ�ñ��ʽ</param>
        /// <returns>ʵ������</returns>
        public virtual int Count(Expression<Func<TEntity, bool>> predicate)
        {
            using (Connection)
            {
                return Connection.Count<TEntity>(predicate.ToPredicateGroup<TEntity, TPrimaryKey>());
            }
        }

        /// <summary>
        /// ʹ��SQL����ȡʵ�弯��
        /// </summary>
        /// <param name="query">SQL���</param>
        /// <returns>ʵ�弯��</returns>
        public virtual IEnumerable<TEntity> Query(string query)
        {
            using (Connection)
            {
                return Connection.Query<TEntity>(query);
            }
        }

        /// <summary>
        /// ʹ��SQL����ȡʵ�弯��
        /// </summary>
        /// <param name="query">SQL���</param>
        /// <param name="parameters">SQL����</param>
        /// <returns>ʵ�弯��</returns>
        public virtual IEnumerable<TEntity> Query(string query, object parameters)
        {
            using (Connection)
            {
                return Connection.Query<TEntity>(query, parameters);
            }
        }

        /// <summary>
        /// ʹ��SQL����ȡʵ�弯��
        /// </summary>
        /// <param name="query">SQL���</param>
        /// <param name="parameters">SQL����</param>
        /// <param name="commandType">SQL�����������</param>
        /// <returns>ʵ�弯��</returns>
        public virtual IEnumerable<TEntity> Query(string query, object parameters, CommandType commandType)
        {
            using (Connection)
            {
                return Connection.Query<TEntity>(query, parameters, commandType: commandType);
            }
        }

        /// <summary>
        /// ʹ��SQL����ȡָ��ʵ�弯��
        /// </summary>
        /// <param name="query">SQL���</param>
        /// <returns>ʵ�弯��</returns>
        public virtual IEnumerable<TAny> Query<TAny>(string query)
        {
            using (Connection)
            {
                return Connection.Query<TAny>(query);
            }
        }

        /// <summary>
        /// ʹ��SQL����ȡָ��ʵ�弯��
        /// </summary>
        /// <param name="query">SQL���</param>
        /// <param name="parameters">SQL����</param>
        /// <returns>ʵ�弯��</returns>
        public virtual IEnumerable<TAny> Query<TAny>(string query, object parameters)
        {
            using (Connection)
            {
                return Connection.Query<TAny>(query, parameters);
            }
        }

        /// <summary>
        /// ʹ��SQL����ȡָ��ʵ�弯��
        /// </summary>
        /// <param name="query">SQL���</param>
        /// <param name="parameters">SQL����</param>
        /// <param name="commandType">SQL�����������</param>
        /// <returns>ʵ�弯��</returns>
        public virtual IEnumerable<TAny> Query<TAny>(string query, object parameters, CommandType commandType)
        {
            using (Connection)
            {
                return Connection.Query<TAny>(query, parameters, commandType: commandType);
            }
        }
    }
}