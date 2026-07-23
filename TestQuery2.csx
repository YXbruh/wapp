using System.Data;
using System.Data.SqlClient;
using System.Configuration;

string connStr = ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;
connStr = connStr.Replace("|DataDirectory|", @"C:\Users\ivanc\source\repos\wapp\App_Data");

using (var con = new SqlConnection(connStr))
using (var cmd = new SqlCommand(@"
    DECLARE @StudentID NVARCHAR(10) = 'USRFEI001';
    SELECT
        q.QuizID,
        q.Title,
        q.PassMark,
        q.DurationMinutes,
        ISNULL(q.MaxAttempts, 3) AS MaxAttempts,
        c.CourseName,
        COUNT(DISTINCT qq.QuestionID) AS QuestionCount,
        COUNT(DISTINCT qa.AttemptID) AS AttemptCount,
        CAST(
            CASE
                WHEN MAX(CAST(qa.IsPassed AS INT)) = 1
                THEN 1
                ELSE 0
            END
            AS BIT
        ) AS HasPassed
    FROM Quizzes q
    INNER JOIN Courses c ON c.CourseID = q.CourseID
    INNER JOIN Enrollments e ON e.CourseID = q.CourseID AND e.StudentID = @StudentID
    LEFT JOIN QuizQuestions qq ON qq.QuizID = q.QuizID
    LEFT JOIN QuizAttempts qa ON qa.QuizID = q.QuizID AND qa.StudentID = @StudentID
    WHERE q.IsPublished = 1
    GROUP BY q.QuizID, q.Title, q.PassMark, q.DurationMinutes, q.MaxAttempts, c.CourseName
    ORDER BY q.Title;", con))
using (var adapter = new SqlDataAdapter(cmd))
{
    var dt = new DataTable();
    adapter.Fill(dt);
    Console.WriteLine("Columns: " + string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
    Console.WriteLine("Rows: " + dt.Rows.Count);
    foreach (DataColumn col in dt.Columns)
    {
        Console.WriteLine($"  {col.ColumnName} ({col.DataType})");
    }
}