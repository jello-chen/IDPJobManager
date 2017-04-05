using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Utils
{
    public class DbHelper
    {
        #region Constructors

        public DbHelper(string connectionString, DbProviderFactory providerFactory)
        {
            if (providerFactory == null)
                throw new ArgumentNullException("providerFactory", "You must provide a DbProviderFactory instance.");

            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentException("The connection string cannot be empty.", "connectionString");

            _factory = providerFactory;
            _connectionString = connectionString;
        }

        public DbHelper(string connectionStringName)
        {
            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (css == null)
                throw new ArgumentException("The connection string you specified does not exist in your configuration file.");

            _factory = DbProviderFactories.GetFactory(css.ProviderName);
            _connectionString = css.ConnectionString;
        }

        public DbHelper(string connectionString, string providerName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("The connection string cannot be empty.", "connectionString");
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentException("You must provide a provider name.", "providerName");
            _factory = DbProviderFactories.GetFactory(providerName);
            _connectionString = connectionString;
        }

        #endregion

        #region Properties

        private DbProviderFactory _factory;
        private string _connectionString;

        public DbProviderFactory Factory
        {
            get
            {
                return _factory;
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        #endregion

        #region Private Helpers

        protected static void FillFromReader(DbDataReader reader, int startRecord, int maxRecords, Action<DbDataReader> action)
        {
            if (startRecord < 0)
                throw new ArgumentOutOfRangeException("startRecord", "StartRecord must be zero or higher.");

            while (startRecord > 0)
            {
                if (!reader.Read())
                    return;

                startRecord--;
            }

            if (maxRecords > 0)
            {
                int i = 0;

                while (i < maxRecords && reader.Read())
                {
                    action(reader);
                    i++;
                }
            }
            else
            {
                while (reader.Read())
                    action(reader);
            }
        }

        protected static async Task FillFromReaderAsync(DbDataReader reader, int startRecord, int maxRecords, Action<DbDataReader> action)
        {
            if (startRecord < 0)
                throw new ArgumentOutOfRangeException("startRecord", "StartRecord must be zero or higher.");

            while (startRecord > 0)
            {
                if (!await reader.ReadAsync())
                    return;

                startRecord--;
            }

            if (maxRecords > 0)
            {
                int i = 0;

                while (i < maxRecords && await reader.ReadAsync())
                {
                    action(reader);
                    i++;
                }
            }
            else
            {
                while (await reader.ReadAsync())
                    action(reader);
            }
        }

        private string GetProviderParameterFormatString()
        {
            var builder = Factory.CreateCommandBuilder();
            var type = builder.GetType();
            var method = type.GetMethod("GetParameterPlaceholder", BindingFlags.NonPublic | BindingFlags.Instance);
            var index = 42;
            var parameterName = method.Invoke(builder, new object[] { index }).ToString();
            return parameterName.Replace(index.ToString(CultureInfo.InvariantCulture), "{0}");
        }

        #endregion

        #region Helper Methods and Extension Points

        public DbConnection CreateConnection()
        {
            DbConnection connection = Factory.CreateConnection();
            connection.ConnectionString = ConnectionString;

            return connection;
        }

        public DbCommand CreateCommand(string commandText, params object[] parameters)
        {
            var len = parameters.Length;

            var command = Factory.CreateCommand();
            command.CommandType = CommandType.Text;

            if (len > 0)
            {
                var formatValues = new string[len];

                for (var i = 0; i < len; i++)
                {
                    var parameter = parameters[i];
                    var rawValue = parameter as RawValue;

                    if (rawValue != null)
                    {
                        formatValues[i] = rawValue.Value;
                    }
                    else
                    {
                        var dbParameter = Factory.CreateParameter();
                        var name = CreateParameterName(i);

                        dbParameter.ParameterName = name;
                        dbParameter.Value = parameter ?? DBNull.Value;

                        formatValues[i] = name;
                        command.Parameters.Add(dbParameter);
                    }
                }

                command.CommandText = String.Format(commandText, formatValues);
            }
            else
            {
                command.CommandText = commandText;
            }

            return command;
        }

        private string _parameterFormat;

        protected virtual string CreateParameterName(int index)
        {
            if (_parameterFormat == null)
                _parameterFormat = GetProviderParameterFormatString();

            return String.Format(_parameterFormat, index);
        }

        protected virtual Converter<object, T> GetTypeConverter<T>()
        {
            return (object o) => (T)DBConvert.To<T>(o);
        }

        protected virtual Converter<DbDataReader, T> GetDataReaderConverter<T>()
            where T : new()
        {
            return new DataReaderConverter<T>().Convert;
        }

        protected virtual void OnExecuteCommand(DbCommand command)
        { }

        #endregion

        #region ExecuteNonQuery

        public int ExecuteNonQuery(DbCommand command, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            return command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            return command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(DbCommand command)
        {
            int affectedRows;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                affectedRows = ExecuteNonQuery(command, connection);

                connection.Close();
            }

            return affectedRows;
        }

        #endregion

        #region ExecuteNonQueryAsync

        public Task<int> ExecuteNonQueryAsync(DbCommand command, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            return command.ExecuteNonQueryAsync();
        }

        public Task<int> ExecuteNonQueryAsync(DbCommand command, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            return command.ExecuteNonQueryAsync();
        }

        public async Task<int> ExecuteNonQueryAsync(DbCommand command)
        {
            int affectedRows;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                affectedRows = await ExecuteNonQueryAsync(command, connection);

                connection.Close();
            }

            return affectedRows;
        }

        #endregion

        #region ExecuteScalar<T>

        public T ExecuteScalar<T>(DbCommand command, Converter<object, T> converter, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            var value = command.ExecuteScalar();

            return converter(value);
        }

        public T ExecuteScalar<T>(DbCommand command, Converter<object, T> converter, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            var value = command.ExecuteScalar();

            return converter(value);
        }

        public T ExecuteScalar<T>(DbCommand command, Converter<object, T> converter)
        {
            T o;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                o = ExecuteScalar<T>(command, converter, connection);

                connection.Close();
            }

            return o;
        }

        public T ExecuteScalar<T>(DbCommand command, DbTransaction transaction)
        {
            return ExecuteScalar<T>(command, GetTypeConverter<T>(), transaction);
        }

        public T ExecuteScalar<T>(DbCommand command, DbConnection connection)
        {
            return ExecuteScalar<T>(command, GetTypeConverter<T>(), connection);
        }

        public T ExecuteScalar<T>(DbCommand command)
        {
            return ExecuteScalar<T>(command, GetTypeConverter<T>());
        }

        #endregion

        #region ExecuteScalarAsync<T>

        public async Task<T> ExecuteScalarAsync<T>(DbCommand command, Converter<object, T> converter, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            var value = await command.ExecuteScalarAsync();

            return converter(value);
        }

        public async Task<T> ExecuteScalarAsync<T>(DbCommand command, Converter<object, T> converter, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            var value = await command.ExecuteScalarAsync();

            return converter(value);
        }

        public async Task<T> ExecuteScalarAsync<T>(DbCommand command, Converter<object, T> converter)
        {
            T o;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                o = await ExecuteScalarAsync<T>(command, converter, connection);

                connection.Close();
            }

            return o;
        }

        public Task<T> ExecuteScalarAsync<T>(DbCommand command, DbConnection connection)
        {
            return ExecuteScalarAsync<T>(command, GetTypeConverter<T>(), connection);
        }

        public Task<T> ExecuteScalarAsync<T>(DbCommand command, DbTransaction transaction)
        {
            return ExecuteScalarAsync<T>(command, GetTypeConverter<T>(), transaction);
        }

        public Task<T> ExecuteScalarAsync<T>(DbCommand command)
        {
            return ExecuteScalarAsync<T>(command, GetTypeConverter<T>());
        }

        #endregion

        #region ExecuteReader

        public DbDataReader ExecuteReader(DbCommand command, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            return command.ExecuteReader();
        }

        public DbDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            return command.ExecuteReader();
        }

        public DbDataReader ExecuteReader(DbCommand command)
        {
            OnExecuteCommand(command);

            DbConnection connection = CreateConnection();
            command.Connection = connection;
            connection.Open();

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        #endregion

        #region ExecuteReaderAsync

        public Task<DbDataReader> ExecuteReaderAsync(DbCommand command, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            return command.ExecuteReaderAsync();
        }

        public Task<DbDataReader> ExecuteReaderAsync(DbCommand command, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            return command.ExecuteReaderAsync();
        }

        public async Task<DbDataReader> ExecuteReaderAsync(DbCommand command)
        {
            OnExecuteCommand(command);

            DbConnection connection = CreateConnection();
            command.Connection = connection;
            await connection.OpenAsync();

            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        #endregion

        #region ExecuteDataTable

        public DataTable ExecuteDataTable(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataTable dt = new DataTable();

            if (startRecord >= 0 || maxRecords >= 0)
                adapter.Fill(startRecord, maxRecords, dt);
            else
                adapter.Fill(dt);

            return dt;
        }

        public DataTable ExecuteDataTable(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataTable dt = new DataTable();

            if (startRecord >= 0 || maxRecords >= 0)
                adapter.Fill(startRecord, maxRecords, dt);
            else
                adapter.Fill(dt);

            return dt;
        }

        public DataTable ExecuteDataTable(DbCommand command, int startRecord, int maxRecords)
        {
            DataTable dt;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                dt = ExecuteDataTable(command, startRecord, maxRecords, connection);

                connection.Close();
            }

            return dt;
        }

        public DataTable ExecuteDataTable(DbCommand command, DbConnection connection)
        {
            return ExecuteDataTable(command, 0, 0, connection);
        }

        public DataTable ExecuteDataTable(DbCommand command, DbTransaction transaction)
        {
            return ExecuteDataTable(command, 0, 0, transaction);
        }

        public DataTable ExecuteDataTable(DbCommand command)
        {
            return ExecuteDataTable(command, 0, 0);
        }

        #endregion

        #region ExecuteDataTableAsync

        public Task<DataTable> ExecuteDataTableAsync(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataTable dt = new DataTable();

            if (startRecord >= 0 || maxRecords >= 0)
                adapter.Fill(startRecord, maxRecords, dt);
            else
                adapter.Fill(dt);

            return Task.FromResult(dt);
        }

        public Task<DataTable> ExecuteDataTableAsync(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataTable dt = new DataTable();

            if (startRecord >= 0 || maxRecords >= 0)
                adapter.Fill(startRecord, maxRecords, dt);
            else
                adapter.Fill(dt);

            return Task.FromResult(dt);
        }

        public async Task<DataTable> ExecuteDataTableAsync(DbCommand command, int startRecord, int maxRecords)
        {
            DataTable dt;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                dt = await ExecuteDataTableAsync(command, startRecord, maxRecords, connection);

                connection.Close();
            }

            return dt;
        }

        public Task<DataTable> ExecuteDataTableAsync(DbCommand command, DbConnection connection)
        {
            return ExecuteDataTableAsync(command, 0, 0, connection);
        }

        public Task<DataTable> ExecuteDataTableAsync(DbCommand command, DbTransaction transaction)
        {
            return ExecuteDataTableAsync(command, 0, 0, transaction);
        }

        public Task<DataTable> ExecuteDataTableAsync(DbCommand command)
        {
            return ExecuteDataTableAsync(command, 0, 0);
        }

        #endregion

        #region ExecuteDataSet

        public DataSet ExecuteDataSet(DbCommand command, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return ds;
        }

        public DataSet ExecuteDataSet(DbCommand command, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return ds;
        }

        public DataSet ExecuteDataSet(DbCommand command)
        {
            DataSet ds;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                ds = ExecuteDataSet(command, connection);

                connection.Close();
            }

            return ds;
        }

        #endregion

        #region ExecuteDataSetAsync

        public Task<DataSet> ExecuteDataSetAsync(DbCommand command, DbConnection connection)
        {
            OnExecuteCommand(command);

            command.Connection = connection;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return Task.FromResult(ds);
        }

        public Task<DataSet> ExecuteDataSetAsync(DbCommand command, DbTransaction transaction)
        {
            OnExecuteCommand(command);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;

            DbDataAdapter adapter = Factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DataSet ds = new DataSet();
            adapter.Fill(ds);

            return Task.FromResult(ds);
        }

        public async Task<DataSet> ExecuteDataSetAsync(DbCommand command)
        {
            DataSet ds;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                ds = await ExecuteDataSetAsync(command, connection);

                connection.Close();
            }

            return ds;
        }

        #endregion

        #region ExecuteArray<T>

        public T[] ExecuteArray<T>(DbCommand command, Converter<object, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            List<T> list = new List<T>();

            using (DbDataReader reader = ExecuteReader(command, connection))
            {
                FillFromReader(reader, startRecord, maxRecords, r =>
                {
                    list.Add(
                        converter(r.GetValue(0))
                    );
                });

                reader.Close();
            }

            return list.ToArray();
        }

        public T[] ExecuteArray<T>(DbCommand command, Converter<object, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            List<T> list = new List<T>();

            using (DbDataReader reader = ExecuteReader(command, transaction))
            {
                FillFromReader(reader, startRecord, maxRecords, r =>
                {
                    list.Add(
                        converter(r.GetValue(0))
                    );
                });

                reader.Close();
            }

            return list.ToArray();
        }

        public T[] ExecuteArray<T>(DbCommand command, Converter<object, T> converter, int startRecord, int maxRecords)
        {
            T[] arr;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                arr = ExecuteArray<T>(command, converter, startRecord, maxRecords, connection);

                connection.Close();
            }

            return arr;
        }

        public T[] ExecuteArray<T>(DbCommand command, Converter<object, T> converter, DbConnection connection)
        {
            return ExecuteArray<T>(command, converter, 0, 0, connection);
        }

        public T[] ExecuteArray<T>(DbCommand command, Converter<object, T> converter, DbTransaction transaction)
        {
            return ExecuteArray<T>(command, converter, 0, 0, transaction);
        }

        public T[] ExecuteArray<T>(DbCommand command, Converter<object, T> converter)
        {
            return ExecuteArray<T>(command, converter, 0, 0);
        }

        public T[] ExecuteArray<T>(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
        {
            return ExecuteArray<T>(command, GetTypeConverter<T>(), startRecord, maxRecords, connection);
        }

        public T[] ExecuteArray<T>(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
        {
            return ExecuteArray<T>(command, GetTypeConverter<T>(), startRecord, maxRecords, transaction);
        }

        public T[] ExecuteArray<T>(DbCommand command, int startRecord, int maxRecords)
        {
            return ExecuteArray<T>(command, GetTypeConverter<T>(), startRecord, maxRecords);
        }

        public T[] ExecuteArray<T>(DbCommand command, DbConnection connection)
        {
            return ExecuteArray<T>(command, GetTypeConverter<T>(), connection);
        }

        public T[] ExecuteArray<T>(DbCommand command, DbTransaction transaction)
        {
            return ExecuteArray<T>(command, GetTypeConverter<T>(), transaction);
        }

        public T[] ExecuteArray<T>(DbCommand command)
        {
            return ExecuteArray<T>(command, GetTypeConverter<T>());
        }

        #endregion

        #region ExecuteArrayAsync<T>

        public async Task<T[]> ExecuteArrayAsync<T>(DbCommand command, Converter<object, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            List<T> list = new List<T>();

            using (DbDataReader reader = await ExecuteReaderAsync(command, connection))
            {
                await FillFromReaderAsync(reader, startRecord, maxRecords, r =>
                {
                    list.Add(converter(r.GetValue(0)));
                });

                reader.Close();
            }

            return list.ToArray();
        }

        public async Task<T[]> ExecuteArrayAsync<T>(DbCommand command, Converter<object, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            List<T> list = new List<T>();

            using (DbDataReader reader = await ExecuteReaderAsync(command, transaction))
            {
                await FillFromReaderAsync(reader, startRecord, maxRecords, r =>
                {
                    list.Add(converter(r.GetValue(0)));
                });

                reader.Close();
            }

            return list.ToArray();
        }

        public async Task<T[]> ExecuteArrayAsync<T>(DbCommand command, Converter<object, T> converter, int startRecord, int maxRecords)
        {
            T[] arr;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                arr = await ExecuteArrayAsync<T>(command, converter, startRecord, maxRecords, connection);

                connection.Close();
            }

            return arr;
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, Converter<object, T> converter, DbConnection connection)
        {
            return ExecuteArrayAsync<T>(command, converter, 0, 0, connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, Converter<object, T> converter, DbTransaction transaction)
        {
            return ExecuteArrayAsync<T>(command, converter, 0, 0, transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, Converter<object, T> converter)
        {
            return ExecuteArrayAsync<T>(command, converter, 0, 0);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
        {
            return ExecuteArrayAsync<T>(command, GetTypeConverter<T>(), startRecord, maxRecords, connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
        {
            return ExecuteArrayAsync<T>(command, GetTypeConverter<T>(), startRecord, maxRecords, transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, int startRecord, int maxRecords)
        {
            return ExecuteArrayAsync<T>(command, GetTypeConverter<T>(), startRecord, maxRecords);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, DbConnection connection)
        {
            return ExecuteArrayAsync<T>(command, GetTypeConverter<T>(), connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command, DbTransaction transaction)
        {
            return ExecuteArrayAsync<T>(command, GetTypeConverter<T>(), transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(DbCommand command)
        {
            return ExecuteArrayAsync<T>(command, GetTypeConverter<T>());
        }

        #endregion

        #region ExecuteDictionary<TKey, TValue>

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbConnection connection)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            using (DbDataReader reader = ExecuteReader(command, connection))
            {
                FillFromReader(reader, startRecord, maxRecords, r =>
                {
                    dict.Add(
                    keyConverter(r.GetValue(0)),
                    valueConverter(r.GetValue(1))
                    );
                });

                reader.Close();
            }

            return dict;
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            using (DbDataReader reader = ExecuteReader(command, transaction))
            {
                FillFromReader(reader, startRecord, maxRecords, r =>
                {
                    dict.Add(
                    keyConverter(r.GetValue(0)),
                    valueConverter(r.GetValue(1))
                    );
                });

                reader.Close();
            }

            return dict;
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords)
        {
            Dictionary<TKey, TValue> dict;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                dict = ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords, connection);

                connection.Close();
            }

            return dict;
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbConnection connection)
        {
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, 0, 0, connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbTransaction transaction)
        {
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, 0, 0, transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter)
        {
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, 0, 0);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
        {
            return ExecuteDictionary<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), startRecord, maxRecords, connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
        {
            return ExecuteDictionary<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), startRecord, maxRecords, transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, int startRecord, int maxRecords)
        {
            return ExecuteDictionary<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), startRecord, maxRecords);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, DbConnection connection)
        {
            return ExecuteDictionary<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command, DbTransaction transaction)
        {
            return ExecuteDictionary<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(DbCommand command)
        {
            return ExecuteDictionary<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>());
        }

        #endregion

        #region ExecuteDictionaryAsync<TKey, TValue>

        public async Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbConnection connection)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            using (DbDataReader reader = await ExecuteReaderAsync(command, connection))
            {
                await FillFromReaderAsync(reader, startRecord, maxRecords, r =>
                {
                    dict.Add(
                        keyConverter(r.GetValue(0)),
                        valueConverter(r.GetValue(1))
                    );
                });

                reader.Close();
            }

            return dict;
        }

        public async Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            using (DbDataReader reader = await ExecuteReaderAsync(command, transaction))
            {
                await FillFromReaderAsync(reader, startRecord, maxRecords, r =>
                {
                    dict.Add(
                        keyConverter(r.GetValue(0)),
                        valueConverter(r.GetValue(1))
                    );
                });

                reader.Close();
            }

            return dict;
        }

        public async Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords)
        {
            Dictionary<TKey, TValue> dict;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                dict = await ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords, connection);

                connection.Close();
            }

            return dict;
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbConnection connection)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, 0, 0, connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbTransaction transaction)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, 0, 0, transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, 0, 0);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), startRecord, maxRecords, connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), startRecord, maxRecords, transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, int startRecord, int maxRecords)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), startRecord, maxRecords);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, DbConnection connection)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command, DbTransaction transaction)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>(), transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(DbCommand command)
        {
            return ExecuteDictionaryAsync<TKey, TValue>(command, GetTypeConverter<TKey>(), GetTypeConverter<TValue>());
        }

        #endregion

        #region ExecuteObject<T>

        public T ExecuteObject<T>(DbCommand command, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            T o;

            using (DbDataReader reader = ExecuteReader(command, connection))
            {
                if (reader.Read())
                    o = converter(reader);
                else
                    o = default(T);

                reader.Close();
            }

            return o;
        }

        public T ExecuteObject<T>(DbCommand command, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            T o;

            using (DbDataReader reader = ExecuteReader(command, transaction))
            {
                if (reader.Read())
                    o = converter(reader);
                else
                    o = default(T);

                reader.Close();
            }

            return o;
        }

        public T ExecuteObject<T>(DbCommand command, Converter<DbDataReader, T> converter)
        {
            T o;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                o = ExecuteObject<T>(command, converter, connection);

                connection.Close();
            }

            return o;
        }

        public T ExecuteObject<T>(DbCommand command, DbConnection connection)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteObject<T>(command, converter, connection);
        }

        public T ExecuteObject<T>(DbCommand command, DbTransaction transaction)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteObject<T>(command, converter, transaction);
        }

        public T ExecuteObject<T>(DbCommand command)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteObject<T>(command, converter);
        }

        #endregion

        #region ExecuteObjectAsync<T>

        public async Task<T> ExecuteObjectAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            T o;

            using (DbDataReader reader = await ExecuteReaderAsync(command, connection))
            {
                if (await reader.ReadAsync())
                    o = converter(reader);
                else
                    o = default(T);

                reader.Close();
            }

            return o;
        }

        public async Task<T> ExecuteObjectAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            T o;

            using (DbDataReader reader = await ExecuteReaderAsync(command, transaction))
            {
                if (await reader.ReadAsync())
                    o = converter(reader);
                else
                    o = default(T);

                reader.Close();
            }

            return o;
        }

        public async Task<T> ExecuteObjectAsync<T>(DbCommand command, Converter<DbDataReader, T> converter)
        {
            T o;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                o = await ExecuteObjectAsync<T>(command, converter, connection);

                connection.Close();
            }

            return o;
        }

        public Task<T> ExecuteObjectAsync<T>(DbCommand command, DbConnection connection)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteObjectAsync<T>(command, converter, connection);
        }

        public Task<T> ExecuteObjectAsync<T>(DbCommand command, DbTransaction transaction)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteObjectAsync<T>(command, converter, transaction);
        }

        public Task<T> ExecuteObjectAsync<T>(DbCommand command)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteObjectAsync<T>(command, converter);
        }

        #endregion

        #region ExecuteList<T>

        public List<T> ExecuteList<T>(DbCommand command, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            var list = new List<T>();

            using (DbDataReader reader = ExecuteReader(command, connection))
            {
                FillFromReader(reader, startRecord, maxRecords, r =>
                {
                    list.Add(converter(reader));
                });

                reader.Close();
            }

            return list;
        }

        public List<T> ExecuteList<T>(DbCommand command, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var list = new List<T>();

            using (DbDataReader reader = ExecuteReader(command, transaction))
            {
                FillFromReader(reader, startRecord, maxRecords, r =>
                {
                    list.Add(converter(reader));
                });

                reader.Close();
            }

            return list;
        }

        public List<T> ExecuteList<T>(DbCommand command, Converter<DbDataReader, T> converter, int startRecord, int maxRecords)
        {
            List<T> list;

            using (DbConnection connection = CreateConnection())
            {
                connection.Open();

                list = ExecuteList<T>(command, converter, startRecord, maxRecords, connection);

                connection.Close();
            }

            return list;
        }

        public List<T> ExecuteList<T>(DbCommand command, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            return ExecuteList<T>(command, converter, 0, 0, connection);
        }

        public List<T> ExecuteList<T>(DbCommand command, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            return ExecuteList<T>(command, converter, 0, 0, transaction);
        }

        public List<T> ExecuteList<T>(DbCommand command, Converter<DbDataReader, T> converter)
        {
            return ExecuteList<T>(command, converter, 0, 0);
        }

        public List<T> ExecuteList<T>(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteList<T>(command, converter, startRecord, maxRecords, connection);
        }

        public List<T> ExecuteList<T>(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteList<T>(command, converter, startRecord, maxRecords, transaction);
        }

        public List<T> ExecuteList<T>(DbCommand command, int startRecord, int maxRecords)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteList<T>(command, converter, startRecord, maxRecords);
        }

        public List<T> ExecuteList<T>(DbCommand command, DbConnection connection)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteList<T>(command, converter, connection);
        }

        public List<T> ExecuteList<T>(DbCommand command, DbTransaction transaction)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteList<T>(command, converter, transaction);
        }

        public List<T> ExecuteList<T>(DbCommand command)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteList<T>(command, converter);
        }

        #endregion

        #region ExecuteListAsync<T>

        public async Task<List<T>> ExecuteListAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            var list = new List<T>();

            using (DbDataReader reader = await ExecuteReaderAsync(command, connection))
            {
                await FillFromReaderAsync(reader, startRecord, maxRecords, r =>
                {
                    list.Add(converter(reader));
                });

                reader.Close();
            }

            return list;
        }

        public async Task<List<T>> ExecuteListAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var list = new List<T>();

            using (DbDataReader reader = await ExecuteReaderAsync(command, transaction))
            {
                await FillFromReaderAsync(reader, startRecord, maxRecords, r =>
                {
                    list.Add(converter(reader));
                });

                reader.Close();
            }

            return list;
        }

        public async Task<List<T>> ExecuteListAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, int startRecord, int maxRecords)
        {
            List<T> list;

            using (DbConnection connection = CreateConnection())
            {
                await connection.OpenAsync();

                list = await ExecuteListAsync<T>(command, converter, startRecord, maxRecords, connection);

                connection.Close();
            }

            return list;
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            return ExecuteListAsync<T>(command, converter, 0, 0, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            return ExecuteListAsync<T>(command, converter, 0, 0, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, Converter<DbDataReader, T> converter)
        {
            return ExecuteListAsync<T>(command, converter, 0, 0);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, int startRecord, int maxRecords, DbConnection connection)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteListAsync<T>(command, converter, startRecord, maxRecords, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, int startRecord, int maxRecords, DbTransaction transaction)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteListAsync<T>(command, converter, startRecord, maxRecords, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, int startRecord, int maxRecords)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteListAsync<T>(command, converter, startRecord, maxRecords);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, DbConnection connection)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteListAsync<T>(command, converter, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command, DbTransaction transaction)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteListAsync<T>(command, converter, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(DbCommand command)
            where T : new()
        {
            var converter = GetDataReaderConverter<T>();
            return ExecuteListAsync<T>(command, converter);
        }

        #endregion


        //Begin String Overload 
        #region ExecuteNonQuery

        public int ExecuteNonQuery(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteNonQuery(command, connection);
        }

        public int ExecuteNonQuery(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteNonQuery(command, transaction);
        }

        public int ExecuteNonQuery(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteNonQuery(command);
        }

        #endregion

        #region ExecuteNonQueryAsync

        public Task<int> ExecuteNonQueryAsync(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteNonQueryAsync(command, connection);
        }

        public Task<int> ExecuteNonQueryAsync(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteNonQueryAsync(command, transaction);
        }

        public Task<int> ExecuteNonQueryAsync(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteNonQueryAsync(command);
        }

        #endregion

        #region ExecuteScalar<T>

        public T ExecuteScalar<T>(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalar<T>(command, connection);
        }

        public T ExecuteScalar<T>(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalar<T>(command, transaction);
        }

        public T ExecuteScalar<T>(string commandText, Converter<object, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalar<T>(command, converter, connection);
        }

        public T ExecuteScalar<T>(string commandText, Converter<object, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalar<T>(command, converter, transaction);
        }

        public T ExecuteScalar<T>(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalar<T>(command);
        }

        public T ExecuteScalar<T>(string commandText, Converter<object, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalar<T>(command, converter);
        }

        #endregion

        #region ExecuteScalarAsync<T>

        public Task<T> ExecuteScalarAsync<T>(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalarAsync<T>(command, connection);
        }

        public Task<T> ExecuteScalarAsync<T>(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalarAsync<T>(command, transaction);
        }

        public Task<T> ExecuteScalarAsync<T>(string commandText, Converter<object, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalarAsync<T>(command, converter, connection);
        }

        public Task<T> ExecuteScalarAsync<T>(string commandText, Converter<object, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalarAsync<T>(command, converter, transaction);
        }

        public Task<T> ExecuteScalarAsync<T>(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalarAsync<T>(command);
        }

        public Task<T> ExecuteScalarAsync<T>(string commandText, Converter<object, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteScalarAsync<T>(command, converter);
        }

        #endregion

        #region ExecuteReader

        public DbDataReader ExecuteReader(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteReader(command, connection);
        }

        public DbDataReader ExecuteReader(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteReader(command, transaction);
        }

        public DbDataReader ExecuteReader(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteReader(command);
        }

        #endregion

        #region ExecuteReaderAsync

        public Task<DbDataReader> ExecuteReaderAsync(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteReaderAsync(command, connection);
        }

        public Task<DbDataReader> ExecuteReaderAsync(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteReaderAsync(command, transaction);
        }

        public Task<DbDataReader> ExecuteReaderAsync(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteReaderAsync(command);
        }

        #endregion

        #region ExecuteDataTable

        public DataTable ExecuteDataTable(string commandText, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTable(command, startRecord, maxRecords, connection);
        }

        public DataTable ExecuteDataTable(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTable(command, startRecord, maxRecords, transaction);
        }

        public DataTable ExecuteDataTable(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTable(command, connection);
        }

        public DataTable ExecuteDataTable(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTable(command, transaction);
        }

        public DataTable ExecuteDataTable(string commandText, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTable(command, startRecord, maxRecords);
        }

        public DataTable ExecuteDataTable(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTable(command);
        }

        #endregion

        #region ExecuteDataTableAsync

        public Task<DataTable> ExecuteDataTableAsync(string commandText, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTableAsync(command, startRecord, maxRecords, connection);
        }

        public Task<DataTable> ExecuteDataTableAsync(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTableAsync(command, startRecord, maxRecords, transaction);
        }

        public Task<DataTable> ExecuteDataTableAsync(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTableAsync(command, connection);
        }

        public Task<DataTable> ExecuteDataTableAsync(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTableAsync(command, transaction);
        }

        public Task<DataTable> ExecuteDataTableAsync(string commandText, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTableAsync(command, startRecord, maxRecords);
        }

        public Task<DataTable> ExecuteDataTableAsync(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataTableAsync(command);
        }

        #endregion

        #region ExecuteDataSet

        public DataSet ExecuteDataSet(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataSet(command, connection);
        }

        public DataSet ExecuteDataSet(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataSet(command, transaction);
        }

        public Task<DataSet> ExecuteDataSetAsync(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataSetAsync(command, connection);
        }

        public Task<DataSet> ExecuteDataSetAsync(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataSetAsync(command, transaction);
        }

        public DataSet ExecuteDataSet(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataSet(command);
        }

        public Task<DataSet> ExecuteDataSetAsync(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteDataSetAsync(command);
        }

        #endregion

        #region ExecuteArray<T>

        public T[] ExecuteArray<T>(string commandText, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, startRecord, maxRecords, connection);
        }

        public T[] ExecuteArray<T>(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, startRecord, maxRecords, transaction);
        }

        public T[] ExecuteArray<T>(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, connection);
        }

        public T[] ExecuteArray<T>(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, transaction);
        }

        public T[] ExecuteArray<T>(string commandText, Converter<object, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, converter, startRecord, maxRecords, connection);
        }

        public T[] ExecuteArray<T>(string commandText, Converter<object, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, converter, startRecord, maxRecords, transaction);
        }

        public T[] ExecuteArray<T>(string commandText, Converter<object, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, converter, connection);
        }

        public T[] ExecuteArray<T>(string commandText, Converter<object, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, converter, transaction);
        }

        public T[] ExecuteArray<T>(string commandText, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, startRecord, maxRecords);
        }

        public T[] ExecuteArray<T>(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command);
        }

        public T[] ExecuteArray<T>(string commandText, Converter<object, T> converter, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, converter, startRecord, maxRecords);
        }

        public T[] ExecuteArray<T>(string commandText, Converter<object, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteArray<T>(command, converter);
        }

        #endregion

        #region ExecuteArrayAsync<T>

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, startRecord, maxRecords, connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, startRecord, maxRecords, transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, Converter<object, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, converter, startRecord, maxRecords, connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, Converter<object, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, converter, startRecord, maxRecords, transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, Converter<object, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, converter, connection);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, Converter<object, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, converter, transaction);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, startRecord, maxRecords);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, Converter<object, T> converter, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, converter, startRecord, maxRecords);
        }

        public Task<T[]> ExecuteArrayAsync<T>(string commandText, Converter<object, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteArrayAsync<T>(command, converter);
        }

        #endregion

        #region ExecuteDictionary<TKey, TValue>

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords, connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords, transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, startRecord, maxRecords, connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, startRecord, maxRecords, transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, connection);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, transaction);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, keyConverter, valueConverter);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command, startRecord, maxRecords);
        }

        public Dictionary<TKey, TValue> ExecuteDictionary<TKey, TValue>(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionary<TKey, TValue>(command);
        }

        #endregion

        #region ExecuteDictionaryAsync<TKey, TValue>

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords, connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords, transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, startRecord, maxRecords, connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, startRecord, maxRecords, transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, connection);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, transaction);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter, startRecord, maxRecords);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, Converter<object, TKey> keyConverter, Converter<object, TValue> valueConverter)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, keyConverter, valueConverter);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command, startRecord, maxRecords);
        }

        public Task<Dictionary<TKey, TValue>> ExecuteDictionaryAsync<TKey, TValue>(string commandText)
        {
            var command = CreateCommand(commandText);
            return ExecuteDictionaryAsync<TKey, TValue>(command);
        }

        #endregion

        #region ExecuteObject<T>

        public T ExecuteObject<T>(string commandText, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteObject<T>(command, converter, connection);
        }

        public T ExecuteObject<T>(string commandText, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteObject<T>(command, converter, transaction);
        }

        public T ExecuteObject<T>(string commandText, DbConnection connection)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteObject<T>(command, connection);
        }

        public T ExecuteObject<T>(string commandText, DbTransaction transaction)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteObject<T>(command, transaction);
        }

        public T ExecuteObject<T>(string commandText, Converter<DbDataReader, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteObject<T>(command, converter);
        }

        public T ExecuteObject<T>(string commandText)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteObject<T>(command);
        }

        #endregion

        #region ExecuteObjectAsync<T>

        public Task<T> ExecuteObjectAsync<T>(string commandText, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteObjectAsync<T>(command, converter, connection);
        }

        public Task<T> ExecuteObjectAsync<T>(string commandText, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteObjectAsync<T>(command, converter, transaction);
        }

        public Task<T> ExecuteObjectAsync<T>(string commandText, DbConnection connection)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteObjectAsync<T>(command, connection);
        }

        public Task<T> ExecuteObjectAsync<T>(string commandText, DbTransaction transaction)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteObjectAsync<T>(command, transaction);
        }

        public Task<T> ExecuteObjectAsync<T>(string commandText, Converter<DbDataReader, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteObjectAsync<T>(command, converter);
        }

        public Task<T> ExecuteObjectAsync<T>(string commandText)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteObjectAsync<T>(command);
        }

        #endregion

        #region ExecuteList<T>

        public List<T> ExecuteList<T>(string commandText, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, converter, startRecord, maxRecords, connection);
        }

        public List<T> ExecuteList<T>(string commandText, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, converter, startRecord, maxRecords, transaction);
        }

        public List<T> ExecuteList<T>(string commandText, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, converter, connection);
        }

        public List<T> ExecuteList<T>(string commandText, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, converter, transaction);
        }

        public List<T> ExecuteList<T>(string commandText, int startRecord, int maxRecords, DbConnection connection)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, startRecord, maxRecords, connection);
        }

        public List<T> ExecuteList<T>(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, startRecord, maxRecords, transaction);
        }

        public List<T> ExecuteList<T>(string commandText, DbConnection connection)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, connection);
        }

        public List<T> ExecuteList<T>(string commandText, DbTransaction transaction)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, transaction);
        }

        public List<T> ExecuteList<T>(string commandText, Converter<DbDataReader, T> converter, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, converter, startRecord, maxRecords);
        }

        public List<T> ExecuteList<T>(string commandText, Converter<DbDataReader, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, converter);
        }

        public List<T> ExecuteList<T>(string commandText, int startRecord, int maxRecords)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command, startRecord, maxRecords);
        }

        public List<T> ExecuteList<T>(string commandText)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteList<T>(command);
        }

        #endregion

        #region ExecuteListAsync<T>

        public Task<List<T>> ExecuteListAsync<T>(string commandText, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, converter, startRecord, maxRecords, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, Converter<DbDataReader, T> converter, int startRecord, int maxRecords, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, converter, startRecord, maxRecords, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, Converter<DbDataReader, T> converter, DbConnection connection)
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, converter, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, Converter<DbDataReader, T> converter, DbTransaction transaction)
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, converter, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, int startRecord, int maxRecords, DbConnection connection)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, startRecord, maxRecords, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, int startRecord, int maxRecords, DbTransaction transaction)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, startRecord, maxRecords, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, DbConnection connection)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, connection);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, DbTransaction transaction)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, transaction);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, Converter<DbDataReader, T> converter, int startRecord, int maxRecords)
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, converter, startRecord, maxRecords);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, Converter<DbDataReader, T> converter)
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, converter);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText, int startRecord, int maxRecords)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command, startRecord, maxRecords);
        }

        public Task<List<T>> ExecuteListAsync<T>(string commandText)
            where T : new()
        {
            var command = CreateCommand(commandText);
            return ExecuteListAsync<T>(command);
        }

        #endregion

        //End String Overload

    }

    public class DBConvert
    {
        #region Boolean

        private static bool ToBooleanInternal(object value, IFormatProvider provider)
        {
            string s = value as string;
            bool b;

            if (s != null && Boolean.TryParse(s, out b))
                return b;

            if ((s = value.ToString()) != null)
            {
                int i;

                if (Int32.TryParse(s, out i))
                    return i != 0;
            }

            return Convert.ToBoolean(value, provider);
        }

        public static bool? ToNullableBoolean(object value, IFormatProvider provider)
        {
            return ToNullable<bool>(value, provider, ToBooleanInternal);
        }

        public static bool? ToNullableBoolean(object value)
        {
            return ToNullable<bool>(value, CultureInfo.CurrentCulture, ToBooleanInternal);
        }

        public static bool ToBoolean(object value, IFormatProvider provider)
        {
            return To<bool>(value, provider, ToBooleanInternal);
        }

        public static bool ToBoolean(object value)
        {
            return To<bool>(value, CultureInfo.CurrentCulture, ToBooleanInternal);
        }

        #endregion

        #region SByte

        private static sbyte ToSByteInternal(object value, IFormatProvider provider)
        {
            return Convert.ToSByte(value, provider);
        }

        public static sbyte? ToNullableSByte(object value, IFormatProvider provider)
        {
            return ToNullable<sbyte>(value, provider, ToSByteInternal);
        }

        public static sbyte? ToNullableSByte(object value)
        {
            return ToNullable<sbyte>(value, CultureInfo.CurrentCulture, ToSByteInternal);
        }

        public static sbyte ToSByte(object value, IFormatProvider provider)
        {
            return To<sbyte>(value, provider, ToSByteInternal);
        }

        public static sbyte ToSByte(object value)
        {
            return To<sbyte>(value, CultureInfo.CurrentCulture, ToSByteInternal);
        }

        #endregion

        #region Int16

        private static short ToInt16Internal(object value, IFormatProvider provider)
        {
            return Convert.ToInt16(value, provider);
        }

        public static short? ToNullableInt16(object value, IFormatProvider provider)
        {
            return ToNullable<short>(value, provider, ToInt16Internal);
        }

        public static short? ToNullableInt16(object value)
        {
            return ToNullable<short>(value, CultureInfo.CurrentCulture, ToInt16Internal);
        }

        public static short ToInt16(object value, IFormatProvider provider)
        {
            return To<short>(value, provider, ToInt16Internal);
        }

        public static short ToInt16(object value)
        {
            return To<short>(value, CultureInfo.CurrentCulture, ToInt16Internal);
        }

        #endregion

        #region Int32

        private static int ToInt32Internal(object value, IFormatProvider provider)
        {
            return Convert.ToInt32(value, provider);
        }

        public static int? ToNullableInt32(object value, IFormatProvider provider)
        {
            return ToNullable<int>(value, provider, ToInt32Internal);
        }

        public static int? ToNullableInt32(object value)
        {
            return ToNullable<int>(value, CultureInfo.CurrentCulture, ToInt32Internal);
        }

        public static int ToInt32(object value, IFormatProvider provider)
        {
            return To<int>(value, provider, ToInt32Internal);
        }

        public static int ToInt32(object value)
        {
            return To<int>(value, CultureInfo.CurrentCulture, ToInt32Internal);
        }

        #endregion

        #region Int64

        private static long ToInt64Internal(object value, IFormatProvider provider)
        {
            return Convert.ToInt64(value, provider);
        }

        public static long? ToNullableInt64(object value, IFormatProvider provider)
        {
            return ToNullable<long>(value, provider, ToInt64Internal);
        }

        public static long? ToNullableInt64(object value)
        {
            return ToNullable<long>(value, CultureInfo.CurrentCulture, ToInt64Internal);
        }

        public static long ToInt64(object value, IFormatProvider provider)
        {
            return To<long>(value, provider, ToInt64Internal);
        }

        public static long ToInt64(object value)
        {
            return To<long>(value, CultureInfo.CurrentCulture, ToInt64Internal);
        }

        #endregion

        #region Byte

        private static byte ToByteInternal(object value, IFormatProvider provider)
        {
            return Convert.ToByte(value, provider);
        }

        public static byte? ToNullableByte(object value, IFormatProvider provider)
        {
            return ToNullable<byte>(value, provider, ToByteInternal);
        }

        public static byte? ToNullableByte(object value)
        {
            return ToNullable<byte>(value, CultureInfo.CurrentCulture, ToByteInternal);
        }

        public static byte ToByte(object value, IFormatProvider provider)
        {
            return To<byte>(value, provider, ToByteInternal);
        }

        public static byte ToByte(object value)
        {
            return To<byte>(value, CultureInfo.CurrentCulture, ToByteInternal);
        }

        #endregion

        #region UInt16

        private static ushort ToUInt16Internal(object value, IFormatProvider provider)
        {
            return Convert.ToUInt16(value, provider);
        }

        public static ushort? ToNullableUInt16(object value, IFormatProvider provider)
        {
            return ToNullable<ushort>(value, provider, ToUInt16Internal);
        }

        public static ushort? ToNullableUInt16(object value)
        {
            return ToNullable<ushort>(value, CultureInfo.CurrentCulture, ToUInt16Internal);
        }

        public static ushort ToUInt16(object value, IFormatProvider provider)
        {
            return To<ushort>(value, provider, ToUInt16Internal);
        }

        public static ushort ToUInt16(object value)
        {
            return To<ushort>(value, CultureInfo.CurrentCulture, ToUInt16Internal);
        }

        #endregion

        #region UInt32

        private static uint ToUInt32Internal(object value, IFormatProvider provider)
        {
            return Convert.ToUInt32(value, provider);
        }

        public static uint? ToNullableUInt32(object value, IFormatProvider provider)
        {
            return ToNullable<uint>(value, provider, ToUInt32Internal);
        }

        public static uint? ToNullableUInt32(object value)
        {
            return ToNullable<uint>(value, CultureInfo.CurrentCulture, ToUInt32Internal);
        }

        public static uint ToUInt32(object value, IFormatProvider provider)
        {
            return To<uint>(value, provider, ToUInt32Internal);
        }

        public static uint ToUInt32(object value)
        {
            return To<uint>(value, CultureInfo.CurrentCulture, ToUInt32Internal);
        }

        #endregion

        #region UInt64

        private static ulong ToUInt64Internal(object value, IFormatProvider provider)
        {
            return Convert.ToUInt64(value, provider);
        }

        public static ulong? ToNullableUInt64(object value, IFormatProvider provider)
        {
            return ToNullable<ulong>(value, provider, ToUInt64Internal);
        }

        public static ulong? ToNullableUInt64(object value)
        {
            return ToNullable<ulong>(value, CultureInfo.CurrentCulture, ToUInt64Internal);
        }

        public static ulong ToUInt64(object value, IFormatProvider provider)
        {
            return To<ulong>(value, provider, ToUInt64Internal);
        }

        public static ulong ToUInt64(object value)
        {
            return To<ulong>(value, CultureInfo.CurrentCulture, ToUInt64Internal);
        }

        #endregion

        #region Decimal

        private static decimal ToDecimalInternal(object value, IFormatProvider provider)
        {
            return Convert.ToDecimal(value, provider);
        }

        public static decimal? ToNullableDecimal(object value, IFormatProvider provider)
        {
            return ToNullable<decimal>(value, provider, ToDecimalInternal);
        }

        public static decimal? ToNullableDecimal(object value)
        {
            return ToNullable<decimal>(value, CultureInfo.CurrentCulture, ToDecimalInternal);
        }

        public static decimal ToDecimal(object value, IFormatProvider provider)
        {
            return To<decimal>(value, provider, ToDecimalInternal);
        }

        public static decimal ToDecimal(object value)
        {
            return To<decimal>(value, CultureInfo.CurrentCulture, ToDecimalInternal);
        }

        #endregion

        #region Single

        private static float ToSingleInternal(object value, IFormatProvider provider)
        {
            return Convert.ToSingle(value, provider);
        }

        public static float? ToNullableSingle(object value, IFormatProvider provider)
        {
            return ToNullable<float>(value, provider, ToSingleInternal);
        }

        public static float? ToNullableSingle(object value)
        {
            return ToNullable<float>(value, CultureInfo.CurrentCulture, ToSingleInternal);
        }

        public static float ToSingle(object value, IFormatProvider provider)
        {
            return To<float>(value, provider, ToSingleInternal);
        }

        public static float ToSingle(object value)
        {
            return To<float>(value, CultureInfo.CurrentCulture, ToSingleInternal);
        }

        #endregion

        #region Double

        private static double ToDoubleInternal(object value, IFormatProvider provider)
        {
            return Convert.ToDouble(value, provider);
        }

        public static double? ToNullableDouble(object value, IFormatProvider provider)
        {
            return ToNullable<double>(value, provider, ToDoubleInternal);
        }

        public static double? ToNullableDouble(object value)
        {
            return ToNullable<double>(value, CultureInfo.CurrentCulture, ToDoubleInternal);
        }

        public static double ToDouble(object value, IFormatProvider provider)
        {
            return To<double>(value, provider, ToDoubleInternal);
        }

        public static double ToDouble(object value)
        {
            return To<double>(value, CultureInfo.CurrentCulture, ToDoubleInternal);
        }

        #endregion

        #region Char

        private static char ToCharInternal(object value, IFormatProvider provider)
        {
            var s = value as string;

            if (!String.IsNullOrEmpty(s))
                return s[0];

            return Convert.ToChar(value, provider);
        }

        public static char? ToNullableChar(object value, IFormatProvider provider)
        {
            return ToNullable<char>(value, provider, ToCharInternal);
        }

        public static char? ToNullableChar(object value)
        {
            return ToNullable<char>(value, CultureInfo.CurrentCulture, ToCharInternal);
        }

        public static char ToChar(object value, IFormatProvider provider)
        {
            return To<char>(value, provider, ToCharInternal);
        }

        public static char ToChar(object value)
        {
            return To<char>(value, CultureInfo.CurrentCulture, ToCharInternal);
        }

        #endregion

        #region DateTime

        private static DateTime ToDateTimeInternal(object value, IFormatProvider provider)
        {
            return Convert.ToDateTime(value, provider);
        }

        public static DateTime? ToNullableDateTime(object value, IFormatProvider provider)
        {
            return ToNullable<DateTime>(value, provider, ToDateTimeInternal);
        }

        public static DateTime? ToNullableDateTime(object value)
        {
            return ToNullable<DateTime>(value, CultureInfo.CurrentCulture, ToDateTimeInternal);
        }

        public static DateTime ToDateTime(object value, IFormatProvider provider)
        {
            return To<DateTime>(value, provider, ToDateTimeInternal);
        }

        public static DateTime ToDateTime(object value)
        {
            return To<DateTime>(value, CultureInfo.CurrentCulture, ToDateTimeInternal);
        }

        #endregion

        #region DateTimeOffset

        private static DateTimeOffset ToDateTimeOffsetInternal(object value, IFormatProvider provider, DateTimeStyles styles)
        {
            if (value is DateTime)
                return (DateTimeOffset)value;

            return DateTimeOffset.Parse(value.ToString(), provider, styles);
        }

        private static DateTimeOffset ToDateTimeOffsetInternal(object value, IFormatProvider provider)
        {
            return ToDateTimeOffsetInternal(value, provider, DateTimeStyles.None);
        }

        public static DateTimeOffset? ToNullableDateTimeOffset(object value, IFormatProvider provider)
        {
            return ToNullable<DateTimeOffset>(value, provider, ToDateTimeOffsetInternal);
        }

        public static DateTimeOffset? ToNullableDateTimeOffset(object value)
        {
            return ToNullable<DateTimeOffset>(value, CultureInfo.CurrentCulture, ToDateTimeOffsetInternal);
        }

        public static DateTimeOffset ToDateTimeOffset(object value, IFormatProvider provider)
        {
            return To<DateTimeOffset>(value, provider, ToDateTimeOffsetInternal);
        }

        public static DateTimeOffset ToDateTimeOffset(object value)
        {
            return To<DateTimeOffset>(value, CultureInfo.CurrentCulture, ToDateTimeOffsetInternal);
        }

        #endregion

        #region String

        private static string ToStringInternal(object value, IFormatProvider provider)
        {
            return Convert.ToString(value, provider);
        }

        public static string ToString(object value, IFormatProvider provider)
        {
            return To<string>(value, provider, ToStringInternal);
        }

        public static string ToString(object value)
        {
            return To<string>(value, CultureInfo.CurrentCulture, ToStringInternal);
        }

        #endregion

        #region Guid

        // This method accepts IFormatProvider just to match the signature required by
        // To<T> and ToNullable<T>, but it is not used
        private static Guid ToGuidInternal(object value, IFormatProvider provider)
        {
            byte[] bytes = value as byte[];

            if (bytes != null)
                return new Guid(bytes);

            return new Guid(value.ToString());
        }

        public static Guid? ToNullableGuid(object value)
        {
            return ToNullable<Guid>(value, null, ToGuidInternal);
        }

        public static Guid ToGuid(object value)
        {
            return To<Guid>(value, null, ToGuidInternal);
        }

        #endregion

        #region ByteArray

        public static byte[] ToByteArray(object value)
        {
            if (value is byte[])
                return (byte[])value;

            if (value == null || value == DBNull.Value)
                return null;

            throw new FormatException("Cannot cast value to byte[].");
        }

        #endregion

        #region To

        public static T To<T>(object value)
        {
            return To<T>(value, CultureInfo.CurrentCulture);
        }

        public static T To<T>(object value, IFormatProvider provider)
        {
            if (value is T)
                return (T)value;

            if (value == null || value == DBNull.Value)
                return default(T);

            return (T)To(typeof(T), value, provider);
        }

        public static object To(Type type, object value)
        {
            return To(type, value, CultureInfo.CurrentCulture);
        }

        public static object To(Type type, object value, IFormatProvider provider)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (value != null && value.GetType() == type)
                return value;

            var isNullable = IsNullable(type);

            if (isNullable)
                type = Nullable.GetUnderlyingType(type);

            if (value == null || value == DBNull.Value)
            {
                if (isNullable || !type.IsValueType)
                    return null;
                else
                    return Activator.CreateInstance(type);
            }

            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return ToBooleanInternal(value, provider);

                case TypeCode.SByte:
                    return ToSByteInternal(value, provider);

                case TypeCode.Int16:
                    return ToInt16Internal(value, provider);

                case TypeCode.Int32:
                    return ToInt32Internal(value, provider);

                case TypeCode.Int64:
                    return ToInt64Internal(value, provider);

                case TypeCode.Byte:
                    return ToByteInternal(value, provider);

                case TypeCode.UInt16:
                    return ToUInt16Internal(value, provider);

                case TypeCode.UInt32:
                    return ToUInt32Internal(value, provider);

                case TypeCode.UInt64:
                    return ToUInt64Internal(value, provider);

                case TypeCode.Decimal:
                    return ToDecimalInternal(value, provider);

                case TypeCode.Single:
                    return ToSingleInternal(value, provider);

                case TypeCode.Double:
                    return ToDoubleInternal(value, provider);

                case TypeCode.Char:
                    return ToCharInternal(value, provider);

                case TypeCode.DateTime:
                    return ToDateTimeInternal(value, provider);

                case TypeCode.String:
                    return ToStringInternal(value, provider);

                case TypeCode.Object:
                    if (type == typeof(Guid))
                        return ToGuidInternal(value, null);

                    if (type == typeof(byte[]))
                        return ToByteArray(value);

                    if (type == typeof(DateTimeOffset))
                        return ToDateTimeOffsetInternal(value, null);

                    break;
            }

            // fallback to System.Convert for IConvertible types
            return Convert.ChangeType(value, typeCode, provider);
        }

        #endregion

        #region ToDBValue

        public static object ToDBValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            return value;
        }

        #endregion

        #region Internal Private Helpers

        private static T? ToNullable<T>(object value, IFormatProvider provider, Func<object, IFormatProvider, T> converter)
            where T : struct
        {
            if (value is T)
                return (T)value;

            if (value == null || value == DBNull.Value)
                return null;

            return converter(value, provider);
        }

        private static T To<T>(object value, IFormatProvider provider, Func<object, IFormatProvider, T> converter)
        {
            if (value is T)
                return (T)value;

            if (value == null || value == DBNull.Value)
                return default(T);

            return converter(value, provider);
        }

        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        #endregion
    }

    public class DataReaderConverter<T> where T : new()
    {
        private class Mapping
        {
            public int Index;
            public PropertyInfo Property;
        }

        private Mapping[] _mappings;
        private DbDataReader _lastReader;

        public T Convert(DbDataReader reader)
        {
            if (_mappings == null || reader != _lastReader)
                _mappings = MapProperties(reader);

            var o = new T();

            foreach (var mapping in _mappings)
            {
                var prop = mapping.Property;
                var rawValue = reader.GetValue(mapping.Index);
                var value = DBConvert.To(prop.PropertyType, rawValue);
                prop.SetValue(o, value, null);
            }

            _lastReader = reader;

            return o;
        }

        private Mapping[] MapProperties(DbDataReader reader)
        {
            var fieldCount = reader.FieldCount;

            var fields = new Dictionary<string, int>(fieldCount);

            for (var i = 0; i < fieldCount; i++)
                fields.Add(reader.GetName(i).ToLowerInvariant(), i);

            var type = typeof(T);

            var mapping = new List<Mapping>(fieldCount);

            foreach (var prop in type.GetProperties())
            {
                var name = prop.Name.ToLowerInvariant();

                int index;

                if (fields.TryGetValue(name, out index))
                    mapping.Add(new Mapping() { Index = index, Property = prop });
            }

            return mapping.ToArray();
        }
    }

    public static class DataReaderExtensions
    {
        public static T Get<T>(this DbDataReader reader, int ordinal, IFormatProvider provider)
        {
            return DBConvert.To<T>(reader[ordinal], provider);
        }

        public static T Get<T>(this DbDataReader reader, int ordinal)
        {
            return DBConvert.To<T>(reader[ordinal], CultureInfo.CurrentCulture);
        }

        public static T Get<T>(this DbDataReader reader, string name, IFormatProvider provider)
        {
            return DBConvert.To<T>(reader[name], provider);
        }

        public static T Get<T>(this DbDataReader reader, string name)
        {
            return DBConvert.To<T>(reader[name], CultureInfo.CurrentCulture);
        }
    }

    public class RawValue
    {
        public RawValue(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }
    }

}
