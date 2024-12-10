using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCrypt.Net;


namespace RTracker
{
    public partial class RegistrationForm : Form
    {
        // Initialize the registration form.
        public RegistrationForm()
        {
            InitializeComponent();
        }

        // Hashes password using BCrypt for secure storage before saving it to database.
        // Hashing is when we turn the plain password into a secure format to prevent exposing it in case of database breaches.
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);  
        }

        // Registers a new user, add their details to database (SQLite).
        private int RegisterUser(string username, string email, string hashedPassword)
        {
            // Open the connection to database. 
            string databasePath = @"C:\Users\Mark Angelo Alzate\source\repos\RTracker\Rtracker.db";
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();

                // First, check if the email already exists in the database.
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", email);

                    // Check if any records match.
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Indicating message, this is if the user have already an account.
                        MessageBox.Show("You already have an account. Please log in.");
                        return -1; 
                    }
                }

                // Insert the new user into the Users table, this is if the email was not found.
                string query = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.ExecuteNonQuery();
                }

                // Get the UserId of the newly created user.
                string getIdQuery = "SELECT last_insert_rowid()"; 
                using (var cmd = new SQLiteCommand(getIdQuery, connection))
                {
                    // Return the userId for the new user. 
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        //Handles the registration button click event, st.
        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Get username.
            string username = txtUsername.Text;
            // Get email.
            string email = txtEmail.Text;
            // Get password.
            string password = txtPassword.Text;

            // Hash the password.
            string hashedPassword = HashPassword(password);

            // Open the connection for database.
            string databasePath = @"C:\Users\Mark Angelo Alzate\source\repos\RTracker\Rtracker.db";
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();

                // Check again if the email already exists.
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("You already have an account. Please log in.");
                        return;
                    }
                }

                // Insert the user into the Users table, if confirmed that there were no duplicate.
                string query = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Registration successful!");

                // Redirect to login form if the registration was successful. 
                this.Hide();
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
            }
        }

        // Handles the login click event.
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            // Show the login form.
            loginForm.Show();
            // Hide the Registration form when the Login link is clicked.
            this.Hide(); 
        }
    }
}
