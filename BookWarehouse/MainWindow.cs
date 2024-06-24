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
    public partial class MainWindow : Form
    {
        private int selectedOrderId = 0;
        private int _staffId;
        private DataRow _userInfo;
        int selectedGroup = 0;
        private readonly DbConnection dbUtils = new DbConnection();

        public MainWindow(int staffId)
        {
            InitializeComponent();
             _staffId = staffId;
            _userInfo = GetUserInfo(_staffId);
            DisplayUserInfo();
            LoadUserOrders(_staffId);
        }


        private DataRow GetUserInfo(int staffId)
        {
            DataRow userInfo = null;
            try
            {
                string query = "SELECT * FROM Person WHERE staff_id = @staffId";
                var command = new NpgsqlCommand(query);
                command.Parameters.AddWithValue("@staffId", staffId);

                DataTable result = new DbConnection().Query(command);

                if (result.Rows.Count > 0)
                {
                    userInfo = result.Rows[0];
                }
                else
                {
                    MessageBox.Show("Информация не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении информации о пользователе: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return userInfo;
        }


        private void DisplayUserInfo()
        {
            if (_userInfo != null)
            {

                label2.Text = _userInfo["first_name"].ToString() + " "+_userInfo["last_name"].ToString();
           

                label3.Text = Convert.ToDateTime(_userInfo["date_of_birth"]).ToShortDateString();
                label4.Text = _userInfo["number"].ToString();
              
            }

        }

        private void LoadUserOrders(int _staffId)
        {
            try
            {
                string query = "SELECT order_id, quantity, order_date, status FROM \"order\" WHERE person_id = @staffId"; 
                var command = new NpgsqlCommand(query);
                command.Parameters.AddWithValue("@staffId", _staffId);

                DataTable orders = new DbConnection().Query(command);

                if (orders.Rows.Count > 0)
                {
                    dataGridView1.DataSource = orders;
                }
                else
                {
                    MessageBox.Show("Нет заказов для этого пользователя.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
           
        
         private void LoadBooksForOrder(int orderId)
        {
            try
            {
                string query = "SELECT b.title, ob.quantity, ob.price FROM OrderBook ob JOIN Book b ON ob.book_id = b.book_id WHERE ob.order_id = @orderId";
                var command = new NpgsqlCommand(query);
                command.Parameters.AddWithValue("@orderId", orderId);

                DataTable books = dbUtils.Query(command);
                dataGridView2.DataSource = books;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке книг для заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Отмена заказа возможна только при статусе заказа ()", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (selectedGroup > 0)
            {
                DialogResult result = MessageBox.Show("Вы уверены, что хотите Отменить заказ?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    NpgsqlCommand Command = new NpgsqlCommand($"DELETE FROM \"order\" WHERE order_id = {selectedGroup};");
                    DataTable userResult = dbUtils.Query(Command);

                    LoadUserOrders(_staffId);
                    MessageBox.Show("Заказ отменен", "Уведомление");

                    selectedGroup = 0;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dataGridView1.Rows[selectedRowIndex];
                    if (selectedRow != null && selectedRow.Cells[0].Value != null)
                    {
                        selectedGroup = Convert.ToInt32(selectedRow.Cells[0].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            Form newOrderForm = new NewOrder(_staffId);
            newOrderForm.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form newOrderForm = new EditInfo(_staffId);
            newOrderForm.ShowDialog();
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dataGridView1.Rows[selectedRowIndex];
                    if (selectedRow != null && selectedRow.Cells["order_id"].Value != null)
                    {
                        selectedOrderId = Convert.ToInt32(selectedRow.Cells["order_id"].Value);
                        LoadBooksForOrder(selectedOrderId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Refresh();
            LoadUserOrders(_staffId);
        }
    }

}

