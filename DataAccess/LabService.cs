using System;
using System.Data;
using System.Data.SqlClient;
using CSA.Services;

namespace CSA.DataAccess
{
    /// <summary>
    /// Data-access layer for the VirtualLabs table (Terminal Sandbox page).
    /// All methods use parameterised ADO.NET commands to prevent SQL injection.
    /// </summary>
    public static class LabService
    {
        /// <summary>
        /// Returns all labs created by a given lecturer, joined to their course,
        /// shaped for the rptLabs Repeater (LabTitle, ShortDesc, CourseName,
        /// Difficulty, ValidationType, IsActive).
        /// </summary>
        public static DataTable GetByLecturer(string lecturerId)
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
                WHERE   vl.CreatedByID = @LecturerID
                ORDER BY vl.CreatedAt DESC;";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@LecturerID", lecturerId));
        }

        /// <summary>
        /// Returns a single lab row by ID (used to populate the editor form on Edit).
        /// Returns null if not found.
        /// </summary>
        public static DataRow GetById(string labId)
        {
            string sql = @"
                SELECT  LabID, CourseID, LabTitle, Scenario, HintText,
                        ExpectedCommand, ValidationType, Difficulty,
                        TimeLimitMinutes, SkillTag, IsPublished
                FROM    VirtualLabs
                WHERE   LabID = @LabID;";

            DataTable dt = DBHelper.ExecuteQuery(sql,
                new SqlParameter("@LabID", labId));

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Skill tags already in use, so the editor can offer them for reuse instead of
        /// letting near-duplicates ("Web Security" / "web sec") pile up.
        /// </summary>
        public static DataTable GetSkillTags()
        {
            return DBHelper.ExecuteQuery(@"
                SELECT DISTINCT SkillTag
                FROM   VirtualLabs
                WHERE  SkillTag IS NOT NULL AND SkillTag <> ''
                ORDER BY SkillTag;");
        }

        /// <summary>
        /// Inserts a new lab or updates an existing one.
        /// Returns the LabID.
        /// </summary>
        public static string Save(
            string labId, string lecturerId, string courseId,
            string title, string instructions, string hint,
            string validationKey, string validationType,
            string difficulty, int? timeLimitMinutes, bool isActive)
            => Save(labId, lecturerId, courseId, title, instructions, hint,
                    validationKey, validationType, difficulty, timeLimitMinutes,
                    isActive, null);

        /// <summary>
        /// Inserts or updates a lab, including the skill it tags. A blank
        /// <paramref name="skillTag"/> stores NULL (untagged). Returns the LabID.
        /// </summary>
        public static string Save(
            string labId, string lecturerId, string courseId,
            string title, string instructions, string hint,
            string validationKey, string validationType,
            string difficulty, int? timeLimitMinutes, bool isActive,
            string skillTag)
        {
            bool isNew = string.IsNullOrEmpty(labId);

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
                new SqlParameter("@SkillTag",         string.IsNullOrWhiteSpace(skillTag) ? (object)DBNull.Value : skillTag.Trim()),
                new SqlParameter("@IsPublished",      isActive),
                new SqlParameter("@CreatedByID",      lecturerId)
            };

            if (isNew)
            {
                labId = IdGenerator.NewId("LAB");
                SqlParameter[] pInsert = new SqlParameter[p.Length + 1];
                p.CopyTo(pInsert, 0);
                pInsert[p.Length] = new SqlParameter("@LabID", labId);

                string insert = @"
                    INSERT INTO VirtualLabs
                        (LabID, CourseID, LabTitle, Scenario, HintText, ExpectedCommand,
                         ValidationType, Difficulty, TimeLimitMinutes, SkillTag, IsPublished, CreatedByID)
                    VALUES
                        (@LabID, @CourseID, @LabTitle, @Scenario, @HintText, @ExpectedCommand,
                         @ValidationType, @Difficulty, @TimeLimitMinutes, @SkillTag, @IsPublished, @CreatedByID);";

                DBHelper.ExecuteNonQuery(insert, pInsert);
                return labId;
            }
            else
            {
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
                        SkillTag         = @SkillTag,
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
        /// Deletes a lab, but only if it belongs to the given lecturer.
        /// Returns rows affected (0 = not found / not owned).
        /// </summary>
        public static int Delete(string labId, string lecturerId)
        {
            string sql = @"
                DELETE FROM VirtualLabs
                WHERE LabID = @LabID AND CreatedByID = @LecturerID;";

            return DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@LabID", labId),
                new SqlParameter("@LecturerID", lecturerId));
        }

        /// <summary>
        /// Returns courses owned by an lecturer, for the ddlCourse dropdown.
        /// Columns: CourseID, CourseName.
        /// </summary>
        public static DataTable GetCoursesForLecturer(string lecturerId)
        {
            string sql = @"
                SELECT CourseID, CourseName
                FROM   Courses
                WHERE  LecturerID = @LecturerID
                ORDER BY CourseName;";

            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@LecturerID", lecturerId));
        }
    }
}