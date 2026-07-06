using System;
using System.Data;
using System.Data.SqlClient;

namespace CSA.DataAccess
{
    /// <summary>
    /// Data-access layer for the VirtualLabs table (Terminal Sandbox page).
    /// All methods use parameterised ADO.NET commands to prevent SQL injection.
    /// </summary>
    public static class LabService
    {
        /// <summary>
        /// Returns all labs created by a given instructor, joined to their course,
        /// shaped for the rptLabs Repeater (LabTitle, ShortDesc, CourseName,
        /// Difficulty, ValidationType, IsActive).
        /// </summary>
        public static DataTable GetByInstructor(int instructorId)
        {
            string sql = @"
                SELECT  vl.LabID,
                        vl.LabTitle,
                        LEFT(ISNULL(vl.Scenario, ''), 60) AS ShortDesc,
                        c.CourseName,
                        vl.Difficulty,
                        vl.ValidationType,
                        vl.IsPublished AS IsActive
                FROM    VirtualLabs vl
                INNER JOIN Courses c ON vl.CourseID = c.CourseID
                WHERE   vl.CreatedByID = @InstructorID
                ORDER BY vl.CreatedAt DESC;";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@InstructorID", instructorId));
        }

        /// <summary>
        /// Returns a single lab row by ID (used to populate the editor form on Edit).
        /// Returns null if not found.
        /// </summary>
        public static DataRow GetById(int labId)
        {
            string sql = @"
                SELECT  LabID, CourseID, LabTitle, Scenario, HintText,
                        ExpectedCommand, ValidationType, Difficulty,
                        TimeLimitMinutes, IsPublished
                FROM    VirtualLabs
                WHERE   LabID = @LabID;";

            DataTable dt = DBHelper.ExecuteQuery(sql,
                new SqlParameter("@LabID", labId));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Inserts a new lab (labId = 0) or updates an existing one.
        /// Returns the LabID.
        /// </summary>
        public static int Save(
            int labId, int instructorId, int courseId,
            string title, string instructions, string hint,
            string validationKey, string validationType,
            string difficulty, int? timeLimitMinutes, bool isActive)
        {
            SqlParameter[] p = new[]
            {
                new SqlParameter("@CourseID",         courseId),
                new SqlParameter("@LabTitle",         title),
                new SqlParameter("@Scenario",         (object)instructions ?? DBNull.Value),
                new SqlParameter("@HintText",         string.IsNullOrWhiteSpace(hint) ? (object)DBNull.Value : hint),
                new SqlParameter("@ExpectedCommand",  validationKey),
                new SqlParameter("@ValidationType",   validationType),
                new SqlParameter("@Difficulty",       difficulty),
                new SqlParameter("@TimeLimitMinutes", (object)timeLimitMinutes ?? DBNull.Value),
                new SqlParameter("@IsPublished",      isActive),
                new SqlParameter("@CreatedByID",      instructorId)
            };

            if (labId == 0)
            {
                // INSERT — return the new identity value
                string insert = @"
                    INSERT INTO VirtualLabs
                        (CourseID, LabTitle, Scenario, HintText, ExpectedCommand,
                         ValidationType, Difficulty, TimeLimitMinutes, IsPublished, CreatedByID)
                    VALUES
                        (@CourseID, @LabTitle, @Scenario, @HintText, @ExpectedCommand,
                         @ValidationType, @Difficulty, @TimeLimitMinutes, @IsPublished, @CreatedByID);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                object newId = DBHelper.ExecuteScalar(insert, p);
                return Convert.ToInt32(newId);
            }
            else
            {
                // UPDATE — add the LabID parameter (ownership enforced via CreatedByID)
                string update = @"
                    UPDATE VirtualLabs SET
                        CourseID         = @CourseID,
                        LabTitle         = @LabTitle,
                        Scenario         = @Scenario,
                        HintText         = @HintText,
                        ExpectedCommand  = @ExpectedCommand,
                        ValidationType   = @ValidationType,
                        Difficulty       = @Difficulty,
                        TimeLimitMinutes = @TimeLimitMinutes,
                        IsPublished      = @IsPublished,
                        UpdatedAt        = GETDATE()
                    WHERE LabID = @LabID AND CreatedByID = @CreatedByID;";

                SqlParameter[] pUpdate = new SqlParameter[p.Length + 1];
                p.CopyTo(pUpdate, 0);
                pUpdate[p.Length] = new SqlParameter("@LabID", labId);

                DBHelper.ExecuteNonQuery(update, pUpdate);
                return labId;
            }
        }

        /// <summary>
        /// Deletes a lab, but only if it belongs to the given instructor.
        /// Returns rows affected (0 = not found / not owned).
        /// </summary>
        public static int Delete(int labId, int instructorId)
        {
            string sql = @"
                DELETE FROM VirtualLabs
                WHERE LabID = @LabID AND CreatedByID = @InstructorID;";

            return DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@LabID", labId),
                new SqlParameter("@InstructorID", instructorId));
        }

        /// <summary>
        /// Returns courses owned by an instructor, for the ddlCourse dropdown.
        /// Columns: CourseID, CourseName.
        /// </summary>
        public static DataTable GetCoursesForInstructor(int instructorId)
        {
            string sql = @"
                SELECT CourseID, CourseName
                FROM   Courses
                WHERE  InstructorID = @InstructorID
                ORDER BY CourseName;";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@InstructorID", instructorId));
        }
    }
}