using System;

public class EnrollmentRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRejected { get; set; }
    public DateTime RequestDate { get; set; }
    public string AdminComments { get; set; }

    public string CourseTitle { get; set; }

}
