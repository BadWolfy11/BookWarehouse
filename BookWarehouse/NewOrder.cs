using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BookWarehouse.DbConnection;

namespace BookWarehouse
{
    public partial class NewOrder : Form
    {
        private readonly DbConnection dbUtils;
        public int _staffId;

        public NewOrder(int staffId)
        {
             _staffId = staffId;
            InitializeComponent();
            dbUtils = new DbConnection();
            LoadBookTitles();

            textBox1.KeyPress += new KeyPressEventHandler(textBox1_KeyPress);

            textBox1.TextChanged += new EventHandler(textBox1_TextChanged);

            SetupDataGridView();
        }
        public enum OrderStatus
        {
            Ожидание,
            Обработка,
            Отправлено,
            Доставлено,
            Отменено
        }

        private void SetupDataGridView()
        {
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "№";
            dataGridView1.Columns[1].Name = "Название книги";
            dataGridView1.Columns[2].Name = "Количество";
            dataGridView1.Columns[3].Name = "Стоимость";

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ReadOnly = true;
        }

        private void LoadBookTitles()
        {
            try
            {
                string query = "SELECT book_id, title, author, price, stock_quantity FROM Book";
                var command = new NpgsqlCommand(query);
                DataTable books = dbUtils.Query(command);

                comboBox1.DataSource = books;
                comboBox1.DisplayMember = "title";
                comboBox1.ValueMember = "book_id";
                comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки названий книг: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void NewOrder_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
            {
                DataRowView selectedRow = comboBox1.SelectedItem as DataRowView;
                if (selectedRow != null)
                {
                    if (selectedRow.DataView.Table.Columns.Contains("author") &&
                        selectedRow.DataView.Table.Columns.Contains("price") &&
                        selectedRow.DataView.Table.Columns.Contains("stock_quantity"))
                    {
                        label6.Text = $"{selectedRow["author"]}";
                        label3.Text = $"{selectedRow["price"]}";
                        label11.Text = $"{selectedRow["stock_quantity"]}";
                        textBox1.Text = "1";

                    }
                    else
                    {
                        MessageBox.Show("Ошибка: В таблице отсутствуют необходимые столбцы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double quantity;
            double price;

            label1.Text = _staffId.ToString();
            if (double.TryParse(textBox1.Text, out quantity) && double.TryParse(label3.Text, out price))
            {

                double total = quantity * price;
                if (total != 0)
                {
                    label8.Text = total.ToString("F2");
                }
                else
                {
                    label8.Text = "0.00";
                }
            }
            else
            {
                label8.Text = "0.00";
            }


            if (int.TryParse(textBox1.Text, out int num) && int.TryParse(label11.Text, out int stockQuantity))
            {
                if (quantity > stockQuantity)
                {
                    button2.Enabled = false;
                    MessageBox.Show("На складе недостаточно книг для данного количества.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = stockQuantity.ToString();
                }
                else
                {
                    button2.Enabled = true;

                }
            }
            else
            {

                label8.Text = "0.00";
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double quantity;
            if (double.TryParse(textBox1.Text, out quantity))
            {
                quantity += 1;
                textBox1.Text = quantity.ToString();
            }
            else
            {
                textBox1.Text = "1";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double quantity;
            if (double.TryParse(textBox1.Text, out quantity))
            {
                quantity -= 1;
                textBox1.Text = quantity.ToString();
                if (quantity < 0)
                {

                    textBox1.Text = "1";

                }
            }
            else
            {
                textBox1.Text = "0";
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && !string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(label3.Text))
            {
                DataRowView selectedRow = comboBox1.SelectedItem as DataRowView;
                string bookTitle = selectedRow["title"].ToString();
                string quantity = textBox1.Text;
                string totalPrice = label8.Text;
                int listNumber = dataGridView1.Rows.Count;

                string[] row = new string[] { listNumber.ToString(), bookTitle, quantity, totalPrice };
                dataGridView1.Rows.Add(row);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите книгу и введите количество.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                dataGridView1.Rows.Remove(row);
            }
        }
        public async Task CreateOrderAsync()
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, добавьте книги к заказу.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string deliveryAddress = textBox2.Text.Trim();
                if (string.IsNullOrEmpty(deliveryAddress))
                {
                    MessageBox.Show("Пожалуйста, введите адрес доставки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                await dbUtils.connection.OpenAsync();

                // Use the ENUM value directly
                dbUtils.connection.TypeMapper.MapEnum<OrderStatus>();

                await dbUtils.connection.CloseAsync();


                string insertOrderQuery = "INSERT INTO \"order\" (person_id, order_date, delivery_address) VALUES (@personId, @orderDate, @deliveryAddress) RETURNING order_id";
                NpgsqlCommand insertOrderCommand = new NpgsqlCommand(insertOrderQuery);
                insertOrderCommand.Parameters.AddWithValue("personId", _staffId);
                insertOrderCommand.Parameters.AddWithValue("orderDate", DateTime.Now);
                insertOrderCommand.Parameters.AddWithValue("deliveryAddress", deliveryAddress);

                DataTable orderResult = dbUtils.Query(insertOrderCommand);
             
                if (orderResult.Rows.Count == 0)
                {
                    MessageBox.Show("Ошибка при создании заказа.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int orderId = Convert.ToInt32(orderResult.Rows[0]["order_id"]);

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    string bookTitle = row.Cells["Название книги"].Value.ToString();
                    int quantity = Convert.ToInt32(row.Cells["Количество"].Value);
                    decimal price = Convert.ToDecimal(row.Cells["Стоимость"].Value);

                    string query = "SELECT book_id FROM Book WHERE title = @bookTitle";
                    NpgsqlCommand command = new NpgsqlCommand(query);
                    command.Parameters.AddWithValue("bookTitle", bookTitle);

                    DataTable bookInfo = dbUtils.Query(command);
                    if (bookInfo.Rows.Count == 0)
                    {
                        MessageBox.Show($"Ошибка: Книга с названием '{bookTitle}' не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int bookId = Convert.ToInt32(bookInfo.Rows[0]["book_id"]);

                    string insertOrderBookQuery = "INSERT INTO orderbook (order_id, book_id, quantity, price) VALUES (@orderId, @bookId, @quantity, @price)";
                    NpgsqlCommand insertOrderBookCommand = new NpgsqlCommand(insertOrderBookQuery);
                    insertOrderBookCommand.Parameters.AddWithValue("orderId", orderId);
                    insertOrderBookCommand.Parameters.AddWithValue("bookId", bookId);
                    insertOrderBookCommand.Parameters.AddWithValue("quantity", quantity);
                    insertOrderBookCommand.Parameters.AddWithValue("price", price);

                    dbUtils.Query(insertOrderBookCommand);

                    string updateBookQuantityQuery = "UPDATE Book SET stock_quantity = stock_quantity - @stock_quantity WHERE book_id = @bookId";
                    NpgsqlCommand updateBookQuantityCommand = new NpgsqlCommand(updateBookQuantityQuery);
                    updateBookQuantityCommand.Parameters.AddWithValue("stock_quantity", quantity);
                    updateBookQuantityCommand.Parameters.AddWithValue("bookId", bookId);
                    
                    dbUtils.Query(updateBookQuantityCommand);
                }
                
                MessageBox.Show("Заказ успешно создан!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void button5_Click(object sender, EventArgs e)
        {
            Task.Run(async () => await CreateOrderAsync());
        }

    }
    
}
