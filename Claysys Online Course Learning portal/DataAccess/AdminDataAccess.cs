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
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString;

        public void InsertAdmin(Admin admin)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertAdmin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", admin.Name);
                    cmd.Parameters.AddWithValue("@PhoneNumber", admin.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", admin.Email);
                    cmd.Parameters.AddWithValue("@Password", admin.Password);
                    cmd.Parameters.AddWithValue("@ConfirmPassword", admin.ConfirmPassword);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


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

                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }

        public void DeleteUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteUserById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Admin GetAdminByEmail(string email)
        {
            Admin admin = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAdminByEmail", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
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

        public bool IsEmailAvailable(string email)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Admins WHERE Email = @Email";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = (int)command.ExecuteScalar();
                    return count == 0;
                }
            }
        }
    }
}
