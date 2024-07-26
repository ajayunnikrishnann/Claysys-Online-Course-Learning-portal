using Claysys_Online_Course_Learning_portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Claysys_Online_Course_Learning_portal.DataAccess
{
    public class AdminDataAccess
    {
        // Connection string retrieved from the application's configuration file
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString;

        // Method to insert a new admin into the database
        public void InsertAdmin(Admin admin)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Create a SQL command to execute the stored procedure for inserting an admin
                using (SqlCommand cmd = new SqlCommand("sp_InsertAdmin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameters for the stored procedure
                    cmd.Parameters.AddWithValue("@Name", admin.Name);
                    cmd.Parameters.AddWithValue("@PhoneNumber", admin.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", admin.Email);
                    cmd.Parameters.AddWithValue("@Password", admin.Password);
                    cmd.Parameters.AddWithValue("@ConfirmPassword", admin.ConfirmPassword);

                    // Open the connection and execute the command
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        // Method to retrieve all users from the database
        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Create a SQL command to execute the stored procedure for retrieving all users
                using (SqlCommand cmd = new SqlCommand("sp_GetAllUsers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Read each record and populate the user objects
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                                Gender = reader["Gender"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Email = reader["Email"].ToString(),
                                Address = reader["Address"].ToString(),
                                State = reader["State"].ToString(),
                                City = reader["City"].ToString(),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                ConfirmPassword = reader["ConfirmPassword"].ToString()
                            };

                            users.Add(user); // Add the user to the list
                        }
                    }
                }
            }

            return users;
        }

        // Method to delete a user from the database
        public void DeleteUser(int userId)
        {
            // Create a SQL command to execute the stored procedure for deleting a user
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteUserById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameter for the stored procedure
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Method to retrieve an admin by their email
        public Admin GetAdminByEmail(string email)
        {
            Admin admin = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Create a SQL command to execute the stored procedure for retrieving an admin by email
                using (SqlCommand cmd = new SqlCommand("sp_GetAdminByEmail", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameter for the stored procedure
                    cmd.Parameters.AddWithValue("@Email", email);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Read the result and populate the admin object
                        if (reader.Read())
                        {
                            admin = new Admin
                            {
                                AdminID = Convert.ToInt32(reader["AdminID"]),
                                Name = reader["Name"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString(),
                                ConfirmPassword = reader["ConfirmPassword"].ToString()
                            };
                        }
                    }
                }
            }
            return admin;
        }

        // Method to check if an email is available (i.e., not used by any other admin)
        public bool IsEmailAvailable(string email)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Query to count the number of admins with the specified email
                string query = "SELECT COUNT(*) FROM Admins WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = (int)command.ExecuteScalar();
                    return count == 0; // Return true if no admins found with the given email
                }
            }
        }
    }
}
