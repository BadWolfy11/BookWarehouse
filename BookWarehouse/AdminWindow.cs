using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BookWarehouse
{
    public partial class AdminWindow : Form
    {
        private DbConnection dbcon = new DbConnection();
        public AdminWindow()
        {
            InitializeComponent();
            setAdminCombobox();
        }

        private void setAdminCombobox()
        {
            try
            {
                DataTable tables = dbcon.GetAllTables();
                comboBox1.Items.Clear();

                foreach (DataRow row in tables.Rows)
                {
                    comboBox1.Items.Add(row["table_name"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке таблиц: {ex.Message}");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillDataGridView();
        }
        private void fillDataGridView()
        {
            try
            {
                string selectedTable = comboBox1.SelectedItem.ToString();
                DataTable tableData = null;

                tableData = dbcon.GetTableData(selectedTable);

                dataGridView1.DataSource = tableData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при заполнении таблицы: {ex.Message}");
            }
        }

        private void AdminWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string typeColumn = dataGridView1.Columns[e.ColumnIndex].ValueType.Name;

                object value = dbcon.getNewValue(typeColumn, dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());

                string tableName = null;
                string columnName = null;
                string firstColumnName = null;
                int id = -1;

                tableName = comboBox1.SelectedItem.ToString();
                columnName = dataGridView1.Columns[e.ColumnIndex].Name;
                firstColumnName = dataGridView1.Columns[0].HeaderText;
                label2.Text = columnName;

                if (columnName != "id")
                {
                    id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                    label2.Text = id.ToString();
                    string query = null;

                    if (columnName == "password")
                        query = "UPDATE " + tableName + " SET " + columnName + " = @value WHERE "+ firstColumnName +" = " + id + ";";
                    else
                        query = "UPDATE " + tableName + " SET " + columnName + " = @value WHERE "+ firstColumnName +" = " + id + ";";

                    dbcon.UpdateData(query, value);

                    fillDataGridView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу");
                }
                else
                {
                    string tableName = comboBox1.SelectedItem.ToString();
                    string firstColumnName = null;
                    int id = 0;

                    if (dataGridView1.SelectedCells.Count > 0)
                    {
                        int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;
                        id = Convert.ToInt32(dataGridView1.Rows[selectedRowIndex].Cells[0].Value);
                        firstColumnName = dataGridView1.Columns[0].HeaderText;
                    }

                    if (id != -1)
                    {
                        string query = $"DELETE FROM {tableName} WHERE {firstColumnName} = @id;";
                        dbcon.DeleteData(query, id);

                        fillDataGridView();
                    }
                    else
                    {
                        MessageBox.Show("Выберите строку для удаления.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении данных: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите таблицу");
                }
                else
                {
                    string tableName = comboBox1.SelectedItem.ToString();

                    string columnName = dataGridView1.Columns[1].HeaderText;

                    dbcon.InsertValue(tableName, columnName);

                    fillDataGridView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении значения NULL: {ex.Message}");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = textBox1.Text.ToLower();

                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = "";

                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = $"Convert({dataGridView1.Columns[1].Name}, 'System.String') LIKE '%{searchText}%'";

                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                    wordApp.Visible = true;

                    Microsoft.Office.Interop.Word.Document doc = wordApp.Documents.Add();

                    int rows = dataGridView1.Rows.Count;
                    int cols = dataGridView1.Columns.Count;
                    Microsoft.Office.Interop.Word.Table table = doc.Tables.Add(doc.Range(), rows + 1, cols);

                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        table.Cell(1, i + 1).Range.Text = dataGridView1.Columns[i].HeaderText;
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            table.Cell(i + 2, j + 1).Range.Text = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        }
                    }

                    table.Borders.Enable = 1;

                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "Word files (*.docx)|*.docx|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = saveFileDialog1.FileName;

                        if (Path.GetExtension(fileName).ToLower() != ".docx")
                        {
                            MessageBox.Show("Пожалуйста, выберите файл с расширением .docx.");
                            return;
                        }

                        object fileNameObj = fileName;
                        doc.SaveAs2(ref fileNameObj);

                        MessageBox.Show("Таблица сохранена в Word.");
                    }
                }
                else
                {
                    MessageBox.Show("Нет данных для сохранения.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении таблицы: {ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                    excelApp.Visible = true;

                    Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Add();

                    Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.Worksheets[1];

                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
                    }

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        }
                    }

                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = saveFileDialog1.FileName;

                        if (Path.GetExtension(fileName).ToLower() != ".xlsx")
                        {
                            MessageBox.Show("Пожалуйста, выберите файл с расширением .xlsx.");
                            return;
                        }

                        workbook.SaveAs(fileName);

                        MessageBox.Show("Таблица сохранена в Excel.");
                    }
                }
                else
                {
                    MessageBox.Show("Нет данных для сохранения.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении таблицы: {ex.Message}");
            }
        }
    }
}

