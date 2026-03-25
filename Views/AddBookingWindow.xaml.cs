using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using HotelBookingApp.Data;

namespace HotelBookingApp.Views
{
    public partial class AddBookingWindow : Window
    {
        Db db = new Db();
        private int? _bookingId = null;

        public AddBookingWindow()
        {
            InitializeComponent();
            LoadUsers();
            LoadRooms();
            StatusBox.SelectedIndex = 0; // по умолчанию Pending
        }

        public AddBookingWindow(int bookingId) : this()
        {
            _bookingId = bookingId;
            LoadBookingData();
        }

        private void LoadUsers()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Id, FullName FROM Users ORDER BY FullName", conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                UsersBox.ItemsSource = table.DefaultView;
            }
        }

        private void LoadRooms()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Id, Name FROM Rooms ORDER BY Name", conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                RoomsBox.ItemsSource = table.DefaultView;
            }
        }

        private void LoadBookingData()
        {
            if (_bookingId == null) return;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"SELECT UserId, RoomId, StartDate, EndDate, Comment, StatusId
                                                 FROM Bookings WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", _bookingId.Value);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    UsersBox.SelectedValue = reader["UserId"];
                    RoomsBox.SelectedValue = reader["RoomId"];
                    StartDatePicker.SelectedDate = Convert.ToDateTime(reader["StartDate"]);
                    EndDatePicker.SelectedDate = Convert.ToDateTime(reader["EndDate"]);
                    CommentBox.Text = reader["Comment"].ToString();

                    int statusId = Convert.ToInt32(reader["StatusId"]);
                    foreach (ComboBoxItem item in StatusBox.Items)
                    {
                        if (Convert.ToInt32(item.Tag) == statusId)
                        {
                            StatusBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (UsersBox.SelectedValue == null || RoomsBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите пользователя и номер", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int statusId = StatusBox.SelectedValue != null ? (int)StatusBox.SelectedValue : 1;

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlCommand cmd;

                if (_bookingId == null)
                {
                    cmd = new SqlCommand(@"INSERT INTO Bookings 
                        (UserId, RoomId, StatusId, StartDate, EndDate, Comment)
                        VALUES (@user, @room, @status, @start, @end, @comment)", conn);
                }
                else
                {
                    cmd = new SqlCommand(@"UPDATE Bookings
                        SET UserId=@user, RoomId=@room, StatusId=@status, StartDate=@start, EndDate=@end, Comment=@comment
                        WHERE Id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _bookingId.Value);
                }

                cmd.Parameters.AddWithValue("@user", UsersBox.SelectedValue);
                cmd.Parameters.AddWithValue("@room", RoomsBox.SelectedValue);
                cmd.Parameters.AddWithValue("@status", statusId);
                cmd.Parameters.AddWithValue("@start", StartDatePicker.SelectedDate);
                cmd.Parameters.AddWithValue("@end", EndDatePicker.SelectedDate);
                cmd.Parameters.AddWithValue("@comment", CommentBox.Text);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            if (Owner is MainWindow main)
            {
                main.LoadBookings();
            }

            this.Close();
        }
    }
}