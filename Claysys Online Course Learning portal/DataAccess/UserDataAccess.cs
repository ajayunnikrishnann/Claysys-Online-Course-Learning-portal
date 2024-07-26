using Claysys_Online_Course_Learning_portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;

using System.Data.SqlClient;

namespace Claysys_Online_Course_Learning_portal.DataAccess
{
    public class UserDataAccess
    {
        // Connection string retrieved from the application's configuration file
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString;

        // Method to insert a new user into the database
        public void InsertUser(User user)
        {
            // Create and open a SQL connection
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Create a SQL command to execute the stored procedure for inserting a user
                using (SqlCommand cmd = new SqlCommand("sp_InsertUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameters for the stored procedure
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    cmd.Parameters.AddWithValue("@Gender", user.Gender);
                    cmd.Parameters.AddWithValue("@Phone", user.Phone);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Address", user.Address);
                    cmd.Parameters.AddWithValue("@State", user.State);
                    cmd.Parameters.AddWithValue("@City", user.City);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@ConfirmPassword", user.ConfirmPassword);

                    // Open the connection and execute the command
                    con.Open();
                    cmd.ExecuteNonQuery();

                    Debug.WriteLine($"User inserted: {user.Username}");
                }
            }
        }

        // Method to validate user credentials
        public User ValidateUser(string username, string passwordHash)
        {
            User user = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Create a SQL command to execute the stored procedure for validating a user
                using (SqlCommand cmd = new SqlCommand("sp_ValidateUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameters for the stored procedure
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", passwordHash);

                    // Open the connection and execute the command
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Read the result and populate the user object
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                            
                            };
                        }
                    }
                }
            }
            return user;
        }


        // Method to get a user by username
        public User GetUserByUsername(string username)
        {
            User user = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Create a SQL command to execute the stored procedure for retrieving a user by username
                using (SqlCommand cmd = new SqlCommand("sp_GetUserByUsername", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Add parameter for the stored procedure
                    cmd.Parameters.AddWithValue("@Username", username);

                    // Open the connection and execute the command
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Read the result and populate the user object
                        if (reader.Read())
                        {
                            user = new User
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
                        }
                    }
                }
            }
            return user;
        }

        // Method to get a tutor by username
        public Tutor GetTutorByUsername(string username)
        {
            Tutor tutor = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Query to retrieve a tutor based on username
                string query = "SELECT * FROM Users WHERE Username = @Username AND Role = 'Tutor'";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tutor = new Tutor
                            {
                                TutorID = Convert.ToInt32(reader["UserID"]), 
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
                                Role = reader["Role"].ToString()
                            };
                        }
                    }
                }
            }
            return tutor;
        }


        // Method to check if an email is available (not already used by another user)
        public bool IsEmailAvailable(string email)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Query to count the number of users with the specified email
                string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = (int)command.ExecuteScalar();
                    return count == 0; // Return true if no users found with the given username
                }
            }
        }

        public bool IsUsernameAvailable(string username)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)command.ExecuteScalar();
                    return count == 0;
                }
            }
        }

        // Method to get a list of all users
        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllUsers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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

        // Method to delete a user and related enrollment records
        public void DeleteUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                // Delete related enrollment records
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Enrollments WHERE UserId = @UserID", con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Delete the user
                using (SqlCommand cmd = new SqlCommand("sp_DeleteUserById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }



    }
}
