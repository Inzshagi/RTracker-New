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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RTracker
{

    public partial class Form1 : Form
    {

        //SQLite database connection 
        private SQLiteConnection connection;

        // flag to determine if we are happen editing an existing task 
        private bool isEditing = false;

        // Current user ID
        private int currentUserId;

        // Flag to track if the user is logged in
        private bool isLoggedIn = true;

        // List to keep track of tasks that have already triggered a notification
        private List<string> notifiedTasks = new List<string>();


        public Form1(int userId)
        {
            InitializeComponent();

            // Store the current user's ID
            currentUserId = userId; 

            // Path to SQLite Database file that we have use
            string databasePath = @"C:\Users\Mark Angelo Alzate\source\repos\RTracker\Rtracker.db";

            // Initialize connection with our SQL
            connection = new SQLiteConnection($"Data Source={databasePath};Version=3;");

            // Can't customize fonts or size of the time picker in the form so had to customize their font size here
            dateTimePicker1.Font = new Font("Microsoft Sans Serif", 12); 
            dueDatePicker.Font = new Font("Microsoft Sans Serif", 12); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connection.Open();

            // Start the timer to check for overdue tasks every minute
            timer1.Start();

            //Add status items to the combo box
            statusComboBox.Items.AddRange(new[] { "Pending", "In Progress", "Completed" });

            // Can't customize fonts here as well in forms
            toDoListView.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 12);
            toDoListView.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 12);

            // Load al tasks
            LoadData("");
        }

        private void searchButton_Click(object sender, EventArgs e)

        {
            //Search text and filter data
            LoadData(searchTextBox.Text.Trim());
        }
        private void LoadData(string filter)
        {
            // Trim any leading or trailing whitespace from the search filter
            filter = filter.Trim();

            // Fetch tasks from the database filtered by UserId
            string query = "SELECT Title, Description, DueDate, Time, Status FROM TodoList WHERE UserId = @UserId";

            // If a filter is provided (non-empty), add it to the query with a LIKE operator
            if (!string.IsNullOrEmpty(filter))
            {
                query += " AND Title LIKE @Filter";  // This filters by Title
            }

            // Prepare the SQLite data adapter and set parameters
            SQLiteDataAdapter da = new SQLiteDataAdapter(query, connection);
            da.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId);

            // If there's a filter, add the parameter for the Title search
            if (!string.IsNullOrEmpty(filter))
            {
                da.SelectCommand.Parameters.AddWithValue("@Filter", "%" + filter + "%");  // The percent sign (%) is the wildcard for partial matching
            }

            DataTable dt = new DataTable();
            da.Fill(dt);

            // If no tasks are found
            if (dt.Rows.Count == 0)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    MessageBox.Show("Congrats! You don't have any pending tasks!", "Task List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No tasks found matching your search.", "Task List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Time"] != DBNull.Value && !string.IsNullOrEmpty(row["Time"].ToString()))
                    {
                        DateTime time = DateTime.ParseExact(row["Time"].ToString(), "HH:mm", null);
                        row["Time"] = time.ToString("hh:mm tt");
                    }
                }
            }

            // Update the DataGridView or ListView with the filtered data
            toDoListView.DataSource = dt;
        }

        // For saving tasks, and or saving an update made
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(titleTextBox.Text) || string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                MessageBox.Show("Title and Description cannot be empty.");
                return;
            }

            if (statusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a status.");
                return;
            }

            // Check if the due date is in the past
            DateTime selectedDueDate = dueDatePicker.Value.Date + dateTimePicker1.Value.TimeOfDay;
            if (selectedDueDate < DateTime.Now)
            {
                MessageBox.Show("The due date cannot be in the past. Please choose a valid date and time.", "Invalid Due Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Prepare the query to either update or insert the task
            string query = isEditing
                ? "UPDATE TodoList SET Title = @Title, Description = @Description, DueDate = @DueDate, Time = @Time, Status = @Status WHERE Title = @OriginalTitle AND UserId = @UserId"
                : "INSERT INTO TodoList (Title, Description, DueDate, Time, Status, UserId) VALUES (@Title, @Description, @DueDate, @Time, @Status, @UserId)";

            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Title", titleTextBox.Text);
                cmd.Parameters.AddWithValue("@Description", descriptionTextBox.Text);
                cmd.Parameters.AddWithValue("@DueDate", dueDatePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@Time", dateTimePicker1.Value.ToString("HH:mm"));
                cmd.Parameters.AddWithValue("@Status", statusComboBox.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@UserId", currentUserId); // Ensure this is not null

                if (isEditing)
                {
                    cmd.Parameters.AddWithValue("@OriginalTitle", titleTextBox.Text);
                }

                cmd.ExecuteNonQuery();
            }

            LoadData("");
            titleTextBox.Clear();
            descriptionTextBox.Clear();
            statusComboBox.SelectedIndex = -1;
            isEditing = false;
        }

        // For editing existing tasks
        private void editButton_Click(object sender, EventArgs e)
        {
            //Make sure that a row is selected before editing
            if (toDoListView.CurrentRow == null)
            {
                MessageBox.Show("Select a row to edit.");
                return;
            }

            //Allowing editing an existing task and loads it
            isEditing = true;
            titleTextBox.Text = toDoListView.CurrentRow.Cells[0].Value.ToString();
            descriptionTextBox.Text = toDoListView.CurrentRow.Cells[1].Value.ToString();


            dueDatePicker.Value = DateTime.Parse(toDoListView.CurrentRow.Cells[2].Value.ToString());

            //Converts time value stored in database which is in string and display it in the form accordingly
            string timeString = toDoListView.CurrentRow.Cells[3].Value.ToString();

            if (!string.IsNullOrEmpty(timeString))
            {
                dateTimePicker1.Value = DateTime.Parse(timeString);
            }
            else
            {
                dateTimePicker1.Value = DateTime.Now;
            }
        }
        //For deleting an existing task
        private void deleteButton_Click(object sender, EventArgs e)
        {
            //Make sure that a row is selected before deleting
            if (toDoListView.CurrentRow == null)
            {
                MessageBox.Show("Select a row to delete.");
                return;
            }

            // Delete query to remove specific tasks
            string query = "DELETE FROM TodoList WHERE Title = @Title";
            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Title", toDoListView.CurrentRow.Cells[0].Value.ToString());
                cmd.ExecuteNonQuery();
            }

            // Reload tasks after deletion
            LoadData(""); 
        }

        //For adding a new tasks, new button click
        private void newButton_Click(object sender, EventArgs e)
        {
            //Clear fields (title and text boxes) and reset the editing flag
            titleTextBox.Clear();
            descriptionTextBox.Clear();
            isEditing = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            // Close connection when form is closed
            connection.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isLoggedIn)
            {
                CheckForDueTasks();
            }
        }

        private void CheckForDueTasks()
        {
            // Query to select tasks for the currently logged-in user (filtered by UserId)
            string query = "SELECT Title, DueDate, Time FROM TodoList WHERE UserId = @UserId"; // Only select tasks for the current user

            SQLiteDataAdapter da = new SQLiteDataAdapter(query, connection);
            da.SelectCommand.Parameters.AddWithValue("@UserId", currentUserId); // Pass the current user's ID to filter tasks

            DataTable dt = new DataTable();
            da.Fill(dt);

            foreach (DataRow row in dt.Rows)
            {
                DateTime dueDate = Convert.ToDateTime(row["DueDate"]);
                string timeString = row["Time"].ToString();
                DateTime taskTime = DateTime.ParseExact(timeString, "HH:mm", null);

                // Combine date and time to get the full task deadline datetime
                DateTime taskDueDateTime = dueDate.Date + taskTime.TimeOfDay;

                // If the task's due time is in the past, show a reminder
                if (taskDueDateTime <= DateTime.Now && !notifiedTasks.Contains(row["Title"].ToString()))
                {
                    // Only show notification for overdue tasks belonging to the logged-in user
                    ShowTaskReminder(row["Title"].ToString());

                    // Add the task to the list of notified tasks to prevent future notifications
                    notifiedTasks.Add(row["Title"].ToString());
                }
            }
        }

        // Show Task Reminder: Displays a notification for overdue tasks
        private void ShowTaskReminder(string taskTitle)
        {
            MessageBox.Show($"Task '{taskTitle}' is overdue!", "Task Reminder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Stop the timer to prevent notifications while logged out
            isLoggedIn = false;
            // Stop checking for overdue tasks
            timer1.Stop();
            // Clear the notifiedTasks list when logging out
            notifiedTasks.Clear();


            this.Hide();
            RegistrationForm registrationForm = new RegistrationForm();
            registrationForm.Show();
        }
    }
}