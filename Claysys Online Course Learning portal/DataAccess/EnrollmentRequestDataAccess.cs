using Claysys_Online_Course_Learning_portal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Claysys_Online_Course_Learning_portal.DataAccess
{
    public class EnrollmentRequestDataAccess
    {
        private readonly string _connectionString;

        public EnrollmentRequestDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertEnrollmentRequest(EnrollmentRequest request)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("InsertEnrollmentRequest", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", request.UserId);
                cmd.Parameters.AddWithValue("@CourseId", request.CourseId);
                cmd.Parameters.AddWithValue("@Username", request.Username);
                cmd.Parameters.AddWithValue("@Email", request.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", request.PhoneNumber);
                cmd.Parameters.AddWithValue("@RequestDate", request.RequestDate);
                cmd.Parameters.AddWithValue("@IsApproved", request.IsApproved);
                cmd.Parameters.AddWithValue("@IsRejected", request.IsRejected);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public List<EnrollmentRequest> GetAllEnrollmentRequests()
        {
            var requests = new List<EnrollmentRequest>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetAllEnrollmentRequests", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var request = new EnrollmentRequest
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        CourseId = Convert.ToInt32(reader["CourseId"]),
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                        IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                        IsRejected = Convert.ToBoolean(reader["IsRejected"]),
                        CourseTitle = reader["CourseTitle"].ToString() // Ensure this line is included
                    };
                    requests.Add(request);
                }
            }
            return requests;
        }


        public List<EnrollmentRequest> GetEnrollmentRequestsByUserId(int userId)
        {
            var requests = new List<EnrollmentRequest>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("GetEnrollmentRequestsByUserId", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var request = new EnrollmentRequest
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        CourseId = Convert.ToInt32(reader["CourseId"]),
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                        IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                        IsRejected = Convert.ToBoolean(reader["IsRejected"])
                    };
                    requests.Add(request);
                }
            }
            return requests;
        }


        public void UpdateEnrollmentRequestStatus(int id, bool isApproved, bool isRejected)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("UpdateEnrollmentRequestStatus", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@IsApproved", isApproved);
                cmd.Parameters.AddWithValue("@IsRejected", isRejected);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<EnrollmentRequest> GetApprovedEnrollmentRequestsByUserId(int userId)
        {
            var requests = new List<EnrollmentRequest>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetApprovedEnrollmentRequestsByUserId", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var request = new EnrollmentRequest
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                            IsApproved = reader.GetBoolean(reader.GetOrdinal("IsApproved")),
                            IsRejected = reader.GetBoolean(reader.GetOrdinal("IsRejected")),
                            RequestDate = reader.GetDateTime(reader.GetOrdinal("RequestDate")),
                            CourseTitle = reader.GetString(reader.GetOrdinal("CourseTitle"))  // Include this line
                        };
                        requests.Add(request);
                    }
                }
            }

            return requests;
        }

        public IEnumerable<Course> GetApprovedCoursesByUserId(int userId)
        {
            var courses = new List<Course>();

            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("GetApprovedCoursesByUserId", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var course = new Course
                        {
                            CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            ImageBase64 = reader.GetString(reader.GetOrdinal("ImageBase64"))
                            
                        };
                        courses.Add(course);
                    }
                }
            }

            return courses;
        }

    }
}
