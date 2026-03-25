using HotelBookingApp.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace HotelBookingApp.Views
{
    public partial class AddBookingWindow : Window
    {
        Db db = new Db();
        private int? editingBookingId = null; // для редактирования

        // Конструктор для создания новой заявки
        public AddBookingWindow()
        {
            InitializeComponent();
            LoadUsers();
            LoadRooms();
            LoadStatuses();
        }

        // Конструктор для редактирования существующей заявки
        public AddBookingWindow(int bookingId) : this()
        {
            editingBookingId = bookingId;
            LoadBooking(bookingId);
        }

        private void LoadUsers()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Id, FullName FROM Users", conn);
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
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT Id, Name FROM Rooms", conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                RoomsBox.ItemsSource = table.DefaultView;
            }
        }

        private void LoadStatuses()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));

                // Маппинг английских статусов на русские
                table.Rows.Add(1, "Ожидает");
                table.Rows.Add(2, "Подтверждено");
                table.Rows.Add(3, "Отклонено");

                StatusBox.ItemsSource = table.DefaultView;
                StatusBox.SelectedValue = 1; 
            }
        }

        private void LoadBooking(int bookingId)
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Bookings WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", bookingId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    UsersBox.SelectedValue = reader.GetInt32(reader.GetOrdinal("UserId"));
                    RoomsBox.SelectedValue = reader.GetInt32(reader.GetOrdinal("RoomId"));
                    StatusBox.SelectedValue = reader.GetInt32(reader.GetOrdinal("StatusId"));
                    StartDatePicker.SelectedDate = reader.GetDateTime(reader.GetOrdinal("StartDate"));
                    EndDatePicker.SelectedDate = reader.GetDateTime(reader.GetOrdinal("EndDate"));
                    CommentBox.Text = reader["Comment"].ToString();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (UsersBox.SelectedValue == null || RoomsBox.SelectedValue == null || StatusBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите пользователя, номер и статус");
                return;
            }

            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();
                string query;

                if (editingBookingId.HasValue)
                {
                    // Обновление существующей заявки
                    query = @"UPDATE Bookings 
                              SET UserId=@user, RoomId=@room, StatusId=@status, StartDate=@start, EndDate=@end, Comment=@comment
                              WHERE Id=@id";
                }
                else
                {
                    // Создание новой заявки
                    query = @"INSERT INTO Bookings 
                              (UserId, RoomId, StatusId, StartDate, EndDate, Comment)
                              VALUES (@user, @room, @status, @start, @end, @comment)";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@user", Convert.ToInt32(UsersBox.SelectedValue));
                cmd.Parameters.AddWithValue("@room", Convert.ToInt32(RoomsBox.SelectedValue));
                cmd.Parameters.AddWithValue("@status", Convert.ToInt32(StatusBox.SelectedValue));
                cmd.Parameters.AddWithValue("@start", StartDatePicker.SelectedDate);
                cmd.Parameters.AddWithValue("@end", EndDatePicker.SelectedDate);
                cmd.Parameters.AddWithValue("@comment", CommentBox.Text);

                if (editingBookingId.HasValue)
                    cmd.Parameters.AddWithValue("@id", editingBookingId.Value);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show(editingBookingId.HasValue ? "Заявка обновлена" : "Заявка создана");
            this.Close();
        }
    }
}