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

namespace BookWarehouse
{
    public partial class EditInfo : Form
    {

        private readonly DbConnection dbUtils;
        private int _personId;
        public EditInfo(int personId)
        {
            _personId = personId;
            InitializeComponent();
            dbUtils = new DbConnection();
            
        }
        

        private void EditInfo_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = textBox1.Text.Trim();
            string lastName = textBox2.Text.Trim();
            string patronomic = textBox3.Text.Trim();
            DateTime? dateOfBirth = dateTimePicker1.Checked ? (DateTime?)dateTimePicker1.Value : null;
            string number = textBox4.Text.Trim();

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(patronomic) && !dateOfBirth.HasValue && string.IsNullOrEmpty(number))
            {
                MessageBox.Show("Пожалуйста, введите данные для обновления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> updates = new List<string>();
            NpgsqlCommand command = new NpgsqlCommand();

            if (!string.IsNullOrEmpty(firstName))
            {
                updates.Add("first_name = @firstName");
                command.Parameters.AddWithValue("firstName", firstName);
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                updates.Add("last_name = @lastName");
                command.Parameters.AddWithValue("lastName", lastName);
            }

            if (!string.IsNullOrEmpty(patronomic))
            {
                updates.Add("patronomic = @patronomic");
                command.Parameters.AddWithValue("patronomic", patronomic);
            }

            if (dateOfBirth.HasValue)
            {
                updates.Add("date_of_birth = @dateOfBirth");
                command.Parameters.AddWithValue("dateOfBirth", dateOfBirth.Value);
            }

            if (!string.IsNullOrEmpty(number))
            {
                updates.Add("number = @number");
                command.Parameters.AddWithValue("number", number);
            }

            if (updates.Count == 0)
            {
                MessageBox.Show("Нет данных для обновления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string updateQuery = $"UPDATE Person SET {string.Join(", ", updates)} WHERE staff_id = @personId";
            command.CommandText = updateQuery;
            command.Parameters.AddWithValue("personId", _personId);

            try
            {
                dbUtils.Query(command);
                MessageBox.Show("Данные пользователя успешно обновлены.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

           
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
