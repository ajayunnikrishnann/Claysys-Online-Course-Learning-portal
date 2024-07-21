using Claysys_Online_Course_Learning_portal.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System;




namespace Claysys_Online_Course_Learning_portal.DataAccess
{
    public class CourseDataAccess
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString;

        public void InsertCourse(Course course)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyAppDbContext"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertCourse", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Title", course.Title);
                    cmd.Parameters.AddWithValue("@Description", course.Description);  // Add this line
                    cmd.Parameters.AddWithValue("@SmallVideoPath", course.SmallVideoPath);
                    cmd.Parameters.AddWithValue("@ImageBase64", course.ImageBase64);

                    // Set the command timeout to 5 minutes (300 seconds)
                    cmd.CommandTimeout = 300;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public List<Course> GetAllCourses()
        {
            var courses = new List<Course>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Courses WHERE IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 300;
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var course = new Course
                            {
                                CourseId = Convert.ToInt32(reader["CourseId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                SmallVideoPath = reader["SmallVideoPath"].ToString(),
                                ImageBase64 = reader["ImageBase64"].ToString(),
                                ReferenceLinks = reader["ReferenceLinks"].ToString(),
                                UserPurchasedCount = reader["UserPurchasedCount"] != DBNull.Value ? Convert.ToInt32(reader["UserPurchasedCount"]) : 0,
                                PurchaseLimit = reader["PurchaseLimit"] != DBNull.Value ? Convert.ToInt32(reader["PurchaseLimit"]) : 0
                            };

                            courses.Add(course);
                        }
                    }
                }
            }

            return courses;
        }


        public Course GetCourseById(int courseId)
        {
            Course course = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Courses WHERE CourseId = @CourseId AND IsDeleted = 0";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            course = new Course
                            {
                                CourseId = Convert.ToInt32(reader["CourseId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                SmallVideoPath = reader["SmallVideoPath"].ToString(),
                                ImageBase64 = reader["ImageBase64"].ToString(),
                                ReferenceLinks = reader["ReferenceLinks"].ToString(),
                                UserPurchasedCount = Convert.ToInt32(reader["UserPurchasedCount"]),
                                PurchaseLimit = reader["PurchaseLimit"] != DBNull.Value ? (int)reader["PurchaseLimit"] : 0,
                            };
                        }
                    }
                }
            }
            return course;
        }


        public void UpdateCourse(Course course)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Courses SET Title = @Title, Description = @Description, ReferenceLinks = @ReferenceLinks, UserPurchasedCount = @UserPurchasedCount, PurchaseLimit = @PurchaseLimit";

                if (!string.IsNullOrEmpty(course.SmallVideoPath))
                {
                    query += ", SmallVideoPath = @SmallVideoPath";
                }

                if (!string.IsNullOrEmpty(course.ImageBase64))
                {
                    query += ", ImageBase64 = @ImageBase64";
                }

                query += " WHERE CourseId = @CourseId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Title", course.Title);
                    cmd.Parameters.AddWithValue("@Description", course.Description);
                    cmd.Parameters.AddWithValue("@ReferenceLinks", course.ReferenceLinks);
                    cmd.Parameters.AddWithValue("@UserPurchasedCount", course.UserPurchasedCount);
                    cmd.Parameters.AddWithValue("@CourseId", course.CourseId);
                    cmd.Parameters.AddWithValue("@PurchaseLimit", course.PurchaseLimit);
                    if (!string.IsNullOrEmpty(course.SmallVideoPath))
                    {
                        cmd.Parameters.AddWithValue("@SmallVideoPath", course.SmallVideoPath);
                    }

                    if (!string.IsNullOrEmpty(course.ImageBase64))
                    {
                        cmd.Parameters.AddWithValue("@ImageBase64", course.ImageBase64);
                    }

                    cmd.CommandTimeout = 300;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void DeleteCourse(int courseId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SoftDeleteCourse", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public void AddReview(Review review)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddReview", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CourseId", review.CourseId);
                    cmd.Parameters.AddWithValue("@UserId", review.UserId);
                    cmd.Parameters.AddWithValue("@ReviewScore", review.ReviewScore);
                    cmd.Parameters.AddWithValue("@Comment", review.Comment);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    UpdateCourseAverageReviewScore(review.CourseId);
                }
            }
        }

        public void DeleteReview(int reviewId, string userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteReview", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReviewId", reviewId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Review> GetReviewsByCourseId(int courseId)
        {
            var reviews = new List<Review>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetReviewsByCourseId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CourseId", courseId);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var review = new Review
                            {
                                ReviewId = Convert.ToInt32(reader["ReviewId"]),
                                CourseId = Convert.ToInt32(reader["CourseId"]),
                                UserId = reader["UserId"].ToString(),
                                ReviewScore = Convert.ToDouble(reader["ReviewScore"]),
                                Comment = reader["Comment"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            };

                            reviews.Add(review);
                        }
                    }
                }
            }

            return reviews;
        }

        public Course GetCourseWithReviewsById(int courseId)
        {
            Course course = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT c.CourseId, c.Title, c.Description, c.SmallVideoPath, c.ImageBase64, c.ReferenceLinks, c.UserPurchasedCount
            FROM Courses c
            WHERE c.CourseId = @CourseId;

            SELECT r.ReviewId, r.CourseId, r.UserId, r.ReviewScore, r.Comment, r.CreatedAt
            FROM Reviews r
            WHERE r.CourseId = @CourseId;
        ";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            course = new Course
                            {
                                CourseId = Convert.ToInt32(reader["CourseId"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                SmallVideoPath = reader["SmallVideoPath"].ToString(),
                                ImageBase64 = reader["ImageBase64"].ToString(),
                                ReferenceLinks = reader["ReferenceLinks"].ToString(),
                                UserPurchasedCount = Convert.ToInt32(reader["UserPurchasedCount"]),
                                Reviews = new List<Review>()

                            };
                        }

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var review = new Review
                                {
                                    ReviewId = Convert.ToInt32(reader["ReviewId"]),
                                    CourseId = Convert.ToInt32(reader["CourseId"]),
                                    UserId = reader["UserId"].ToString(),
                                    ReviewScore = Convert.ToDouble(reader["ReviewScore"]),
                                    Comment = reader["Comment"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                };

                                course.Reviews.Add(review);
                            }
                        }
                    }
                }
            }

            return course;
        }


        public void UpdateReview(Review review)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateReview", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReviewId", review.ReviewId);
                    cmd.Parameters.AddWithValue("@UserId", review.UserId);
                    cmd.Parameters.AddWithValue("@ReviewScore", review.ReviewScore);
                    cmd.Parameters.AddWithValue("@Comment", review.Comment);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsReviewOwner(int reviewId, string userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(1) FROM Reviews WHERE ReviewId = @ReviewId AND UserId = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ReviewId", reviewId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    return Convert.ToBoolean(result);
                }
            }
        }

        public decimal GetAverageReviewScore(int courseId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT AVG(ReviewScore) 
                    FROM Reviews 
                    WHERE CourseId = @CourseId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public void UpdateCourseAverageReviewScore(int courseId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
                    UPDATE Courses 
                    SET AverageReviewScore = (
                        SELECT AVG(ReviewScore) 
                        FROM Reviews 
                        WHERE CourseId = @CourseId
                    )
                    WHERE CourseId = @CourseId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public bool IsUserEnrolledInCourse(int userId, int courseId)
        {
            string query = "SELECT COUNT(*) FROM Enrollments WHERE UserId = @UserId AND CourseId = @CourseId";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        // Add a new enrollment record
        public void AddEnrollment(int userId, int courseId)
        {
            string query = "INSERT INTO Enrollments (UserId, CourseId) VALUES (@UserId, @CourseId)";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CourseId", courseId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        

    }
}