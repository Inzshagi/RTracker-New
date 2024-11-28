using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace RTracker
{
    public partial class ToDoList : Form
    {
        private SQLiteConnection connection;
        private bool isEditing = false;

        public ToDoList()
        {
            InitializeComponent();
            string databasePath = @"C:\Users\Mark Angelo Alzate\source\repos\RTracker\Rtracker.db";
            connection = new SQLiteConnection($"Data Source={databasePath};Version=3;");
        }

        private void ToDoList_Load(object sender, EventArgs e)
        {
            connection.Open();

            //Add status items to the combo box
            statusComboBox.Items.Add("Pending");
            statusComboBox.Items.Add("In Progress");
            statusComboBox.Items.Add("Completed");

            LoadData();
        }

        private void LoadData()
        {
            string query = "SELECT Title, Description, DueDate, Status FROM TodoList";
            SQLiteDataAdapter da = new SQLiteDataAdapter(query, connection);
            DataTable dt = new DataTable();
            da.Fill(dt);
            toDoListView.DataSource = dt;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(titleTextBox.Text) || string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                MessageBox.Show("Title and Description cannot be empty.");
                return;
            }

            string query = isEditing
                ? "UPDATE TodoList SET Title = @Title, Description = @Description, DueDate = @DueDate, Status = @Status WHERE Title = @OriginalTitle"
                : "INSERT INTO TodoList (Title, Description, DueDate, Status) VALUES (@Title, @Description, @DueDate, @Status)"; // Include Status in the insert query

            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Title", titleTextBox.Text);
                cmd.Parameters.AddWithValue("@Description", descriptionTextBox.Text);
                cmd.Parameters.AddWithValue("@DueDate", dueDatePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Status", statusComboBox.SelectedItem.ToString()); 

                if (dueDatePicker.Value.Date < DateTime.Now.Date)
                {
                    MessageBox.Show("Due Date cannot be in the past.");
                    return;
                }

                if (isEditing)
                {
                    cmd.Parameters.AddWithValue("@OriginalTitle", titleTextBox.Text); // For updating existing task
                }

                cmd.ExecuteNonQuery();
            }

            LoadData(); // Reload data to refresh DataGridView
            titleTextBox.Clear();
            descriptionTextBox.Clear();
            statusComboBox.SelectedIndex = -1;
            isEditing = false;
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            if (toDoListView.CurrentRow == null)
            {
                MessageBox.Show("Select a row to edit.");
                return;
            }

            isEditing = true;
            titleTextBox.Text = toDoListView.CurrentRow.Cells[0].Value.ToString();
            descriptionTextBox.Text = toDoListView.CurrentRow.Cells[1].Value.ToString();
            dueDatePicker.Value = DateTime.Parse(toDoListView.CurrentRow.Cells[2].Value.ToString());
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (toDoListView.CurrentRow == null)
            {
                MessageBox.Show("Select a row to delete.");
                return;
            }

            string query = "DELETE FROM TodoList WHERE Title = @Title";
            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Title", toDoListView.CurrentRow.Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }

            LoadData(); // Reload data to refresh DataGridView
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            titleTextBox.Clear();
            descriptionTextBox.Clear();
            isEditing = false;
        }

        private void ToDoList_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
    }
}