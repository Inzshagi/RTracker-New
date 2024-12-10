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
        // Connection for SQLite database, so that database can be access. 
        private SQLiteConnection dbConnection;

        public LoginForm()
        {
            // Initialize database connection through its file path.
            InitializeComponent();
            dbConnection = new SQLiteConnection(@"Data Source=C:\Users\Mark Angelo Alzate\source\repos\RTracker\Rtracker.db;Version=3;");
        }

        // Runs when the Login button is click. 
        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Fetch usernmane and password. 
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // Open database connection so we can interact with it. 
            dbConnection.Open();

            // SQL query to fetch user password and UserId for the given username, check if username exist. 
            string query = "SELECT Password, UserId FROM Users WHERE Username = @Username";
            SQLiteCommand cmd = new SQLiteCommand(query, dbConnection);

            // Replace the placeholder with the actual username of the user. 
            cmd.Parameters.AddWithValue("@Username", username);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                // Convert the result to a string (the hashed password stored in the database).
                string storedHashedPassword = result.ToString();

                // Verify if the password match the hashed one. 
                if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
                {
                    // Fetch the UserId, if pw matched. 
                    string getUserIdQuery = "SELECT UserId FROM Users WHERE Username = @Username";
                    SQLiteCommand userIdCmd = new SQLiteCommand(getUserIdQuery, dbConnection);
                    userIdCmd.Parameters.AddWithValue("@Username", username);
                    
                    // Get the userId and convert it to int. 
                    int userId = Convert.ToInt32(userIdCmd.ExecuteScalar());

                    // Pass UserId to the main form, form will be able to know which user is logged in. 
                    Form1 form1 = new Form1(userId);  
                    form1.Show();

                    // Hide LoginForm after successful login.
                    this.Hide();  
                }
                else
                {
                    // If logins did not match, shows an error message. 
                    MessageBox.Show("Invalid username or password.");
                }
            }
            else
            {
                //If  username doesnt exist as well, show an error message.
                MessageBox.Show("Invalid username or password.");
            }

            // Close the connection once done.
            dbConnection.Close();  
        }

        // Runs when user decided to register. 
        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            {
                // Open registration form. 
                RegistrationForm registrationForm = new RegistrationForm();
                registrationForm.Show();

                // Hide the LoginForm after the Register link is clicked.
                this.Hide();  
            }
        }
    }
}
