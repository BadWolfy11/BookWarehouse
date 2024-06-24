using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BookWarehouse
{
    public partial class Registration : Form
    {
        private readonly DbConnection dbUtils;
        public Registration()
        {
            InitializeComponent();
            dbUtils = new DbConnection();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;
            string name = textBox3.Text;
            string phone = textBox4.Text;
            string lastname = textBox5.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(lastname))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Проверка существования логина
                string queryCheckLogin = "SELECT COUNT(*) FROM Auth WHERE username = @username";
                var commandCheckLogin = new NpgsqlCommand(queryCheckLogin);
                commandCheckLogin.Parameters.AddWithValue("@username", username);
                int count = Convert.ToInt32(dbUtils.Query(commandCheckLogin).Rows[0][0]);
                if (count > 0)
                {
                    MessageBox.Show("Логин уже существует. Пожалуйста, выберите другой логин.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Вставка данных в таблицу Person и получение staff_id
                string query = "INSERT INTO Person (first_name, last_name, number) VALUES (@name, @lastname, @phone) RETURNING staff_id";
                var command = new NpgsqlCommand(query);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@lastname", lastname);
                command.Parameters.AddWithValue("@phone", phone);
                DataTable result = dbUtils.Query(command);

                if (result.Rows.Count > 0 && result.Rows[0]["staff_id"] != DBNull.Value)
                {
                    int staffId = Convert.ToInt32(result.Rows[0]["staff_id"]);

                    // Вставка данных в таблицу Auth
                    query = "INSERT INTO Auth (staff_id, username, password) VALUES (@staffId, @username, @password)";
                    command = new NpgsqlCommand(query);
                    command.Parameters.AddWithValue("@staffId", staffId);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    dbUtils.Query(command);

                    
                    AssignRoleToUser(staffId, "Покупатель");

                    MessageBox.Show("Регистрация успешно завершена!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Form RegWindow = new Auth();
                    RegWindow.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Ошибка при регистрации: не удалось получить staff_id.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssignRoleToUser(int staffId, string roleName)
        {
            try
            {
                string query = "UPDATE Person SET position_id = (SELECT position_id FROM Position WHERE title = @roleName) WHERE staff_id = @staffId";
                var command = new NpgsqlCommand(query);
                command.Parameters.AddWithValue("@staffId", staffId);
                command.Parameters.AddWithValue("@roleName", roleName);
                dbUtils.Query(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при назначении роли: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Registration_Load(object sender, EventArgs e)
        {
        }

     

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form AuthWindow = new Auth();
            AuthWindow.Show();
            this.Hide();
           
        }

        private void Registration_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
