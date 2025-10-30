using Microsoft.Data.SqlClient;

namespace QuanLyRapChieuPhim_Admin.code
{
    public static class DatabaseHelper
    {
        private static readonly string _connectionString =
            "Data Source=WINDOWS-PC\\SQLEXPRESS;Initial Catalog=QuanLyRapPhim;Integrated Security=True;Trust Server Certificate=True";

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
