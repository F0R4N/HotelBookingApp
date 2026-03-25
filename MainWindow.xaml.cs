using HotelBookingApp.Data;
using HotelBookingApp.Views;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HotelBookingApp
{
    public partial class MainWindow : Window
    {
        Db db = new Db();

        public MainWindow()
        {
            InitializeComponent();
            LoadBookings();
        }

        public void LoadBookings()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT b.Id,
                           u.FullName AS UserName,
                           r.Name AS RoomName,
                           b.StartDate,
                           b.EndDate,
                           b.Comment,
                           CASE b.StatusId
                                WHEN 1 THEN 'Pending'
                                WHEN 2 THEN 'Approved'
                                WHEN 3 THEN 'Rejected'
                           END AS Status
                    FROM Bookings b
                    JOIN Users u ON b.UserId = u.Id
                    JOIN Rooms r ON b.RoomId = r.Id
                    ORDER BY b.StartDate";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                BookingsGrid.ItemsSource = table.DefaultView;
            }
        }

        private void OpenAddBooking(object sender, RoutedEventArgs e)
        {
            AddBookingWindow window = new AddBookingWindow
            {
                Owner = this
            };
            window.ShowDialog();
        }

        private void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                int bookingId = Convert.ToInt32(row["Id"]);
                AddBookingWindow window = new AddBookingWindow(bookingId)
                {
                    Owner = this
                };
                window.ShowDialog();
            }
        }

        private void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                int bookingId = Convert.ToInt32(row["Id"]);

                var result = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (SqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Bookings WHERE Id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", bookingId);
                        cmd.ExecuteNonQuery();
                    }
                    LoadBookings();
                }
            }
        }
    }
}