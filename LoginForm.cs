using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCrypt.Net;

namespace RTracker
{
    public partial class LoginForm : Form
    {
        private SQLiteConnection dbConnection;

        public LoginForm()
        {
            InitializeComponent();
            dbConnection = new SQLiteConnection(@"Data Source=C:\Users\Mark Angelo Alzate\source\repos\RTracker\Rtracker.db;Version=3;");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            dbConnection.Open();

            // SQL query to fetch user password and UserId
            string query = "SELECT Password, UserId FROM Users WHERE Username = @Username";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);
            cmd.Parameters.AddWithValue("@Username", username);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                // Check if the entered password matches the stored password
                string storedHashedPassword = result.ToString();
                if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
                {
                    // Fetch the UserId
                    string getUserIdQuery = "SELECT UserId FROM Users WHERE Username = @Username";
                    SQLiteCommand userIdCmd = new SQLiteCommand(getUserIdQuery, dbConnection);
                    userIdCmd.Parameters.AddWithValue("@Username", username);
                    int userId = Convert.ToInt32(userIdCmd.ExecuteScalar());

                    Form1 form1 = new Form1(userId);  // Pass UserId to the main form
                    form1.Show();
                    this.Hide();  // Hide LoginForm after successful login
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }

            dbConnection.Close();  // Close the connection
        }


        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            {
                RegistrationForm registrationForm = new RegistrationForm();
                registrationForm.Show();
                this.Hide();  // Hide the LoginForm when the Register link is clicked
            }
        }
    }
}
