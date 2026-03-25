using System.Data.SqlClient;

namespace HotelBookingApp.Data
{
    public class Db
    {
        private string connectionString =
            "Server=localhost;Database=HotelBooking;Trusted_Connection=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}