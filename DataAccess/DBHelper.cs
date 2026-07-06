using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CSA.DataAccess
{
    /// <summary>
    /// Central helper for all ADO.NET database access.
    /// Reads the connection string named "CSAConnection" from Web.config.
    /// </summary>
    public static class DBHelper
    {
        // Reads <connectionStrings> entry "CSAConnection" from Web.config
        private static string ConnStr =>
            ConfigurationManager.ConnectionStrings["CSAConnection"].ConnectionString;

        /// <summary>Opens and returns a new SqlConnection.</summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        /// <summary>
        /// Runs a SELECT query and returns the results as a DataTable.
        /// </summary>
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                return dt;
            }
        }

        /// <summary>
        /// Runs an INSERT / UPDATE / DELETE and returns rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Runs a query that returns a single value (e.g. SCOPE_IDENTITY or COUNT).
        /// </summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}