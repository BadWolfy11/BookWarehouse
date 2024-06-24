using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookWarehouse
{
    internal class DbConnection
    {

        public NpgsqlConnection connection;
        public string connectionString;

        public DbConnection()
        {
            Initialize();
        }
        public DataTable GetTableData(string tableName)
        {
            DataTable dataTable = new DataTable();
            string query = $"SELECT * FROM {tableName}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }
        public void InsertValue(string tableName, string columnName)
        {
            string query = $"INSERT INTO {tableName} ({columnName}) VALUES (NULL);";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetAllTables()
        {
            DataTable dt = new DataTable();
            string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'  AND table_type = 'BASE TABLE';";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }
        public object getNewValue(string typeColumn, string value)
        {
            object newValue = null;

            switch (typeColumn)
            {
                case "DateTime":
                    newValue = Convert.ToDateTime(value);
                    break;
                case "Decimal":
                    newValue = Convert.ToDecimal(value);
                    break;
                case "String":
                    newValue = value;
                    break;
                case "Int32":
                    newValue = Convert.ToInt32(value);
                    break;
                case "Boolean":
                    newValue = Convert.ToBoolean(value);
                    break;
            }

            return newValue;
        }
        public void UpdateData(string query, object value)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value", value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCellValue(string tableName, string columnName, object newValue, int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"UPDATE {tableName} SET {columnName} = @value WHERE id = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@value", newValue);
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
     
        public void Commit(NpgsqlTransaction transaction)
        {
            try
            {
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении транзакции: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Initialize()
        {
            string host = "172.20.7.8";
            string user = "ZxXxR";
            string pass = "159632zZZ";
            string database = "Book_Warehouse";

            connectionString = $"Host={host};Username={user};Password={pass};Database={database}";
            connection = new NpgsqlConnection(connectionString);
        }
        public bool ExecuteNonQuery(NpgsqlCommand command)
        {
            bool result = false;
            if (connect())
            {
                try
                {
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка выполнения запроса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    disconnect();
                }
            }
            return result;
        }


        public void Update(NpgsqlCommand command, DataTable tableData)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                command.Connection = connection;
                using (var adapter = new NpgsqlDataAdapter(command))
                {
                    var builder = new NpgsqlCommandBuilder(adapter);
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.Update(tableData);
                }
            }
        }

      
        private bool connect()
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (NpgsqlException ex)
                {
                    switch (ex.ErrorCode)
                    {
                        case 0:
                            MessageBox.Show("Произошла ошибка при подключении к базе");
                            break;

                        case 1045:
                            MessageBox.Show("Неверное имя пользователя или пароль для подключения к базе данных.");
                            break;
                        default:
                            MessageBox.Show($"Ошибка БД: Code={ex.ErrorCode}, Message={ex.Message}");
                            break;
                    }
                    return false;
                }
            }

            private bool disconnect()
            {
                try
                {
                    connection.Close();
                    return true;
                }
                catch (NpgsqlException ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }

        public DataTable Query(NpgsqlCommand command)
        {
            DataTable results = new DataTable("Results");

            if (connect())
            {
                try
                {
                    command.Connection = connection;
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        results.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка выполнения запроса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    disconnect();
                }
            }

            return results;
        }

        public void DeleteData(string query, int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("id", id);

                    command.ExecuteNonQuery();
                }
            }
        }

        public bool testConnection()
            {
                try
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Текст ошибки: {ex.Message}", "Ошибка соединения с базой данных");
                }

                return false;
            }
          
        }

        
    }

