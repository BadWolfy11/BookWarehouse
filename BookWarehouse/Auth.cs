using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace BookWarehouse
{
    public partial class Auth : Form
    {
        private readonly DbConnection dbUtils = new DbConnection();

        public Auth()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
               
                var commandText = new NpgsqlCommand("SELECT A.staff_id, P.position_id, Po.title " +
                                                    "FROM Auth A " +
                                                    "JOIN Person P ON A.staff_id = P.staff_id " +
                                                    "JOIN Position Po ON P.position_id = Po.position_id " +
                                                    "WHERE A.username = @username AND A.password = @password");
                commandText.Parameters.AddWithValue("@username", username.Trim());
                commandText.Parameters.AddWithValue("@password", password.Trim());

                DataTable userResult = dbUtils.Query(commandText);

                if (userResult.Rows.Count == 0)
                {
                    MessageBox.Show("Данного аккаунта не существует. Попробуйте еще раз.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int staffId = Convert.ToInt32(userResult.Rows[0]["staff_id"]);
                string role = userResult.Rows[0]["title"].ToString();

              
                Form mainForm;
                if (role == "Покупатель")
                {
                    mainForm = new MainWindow(staffId);
                }
                else
                {
                    mainForm = new AdminWindow();
                }

                mainForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении запроса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
          
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form RegWindow = new Registration();
            RegWindow.Show();
            this.Hide();
            
        }

        private void Auth_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {

        }
    }
}
