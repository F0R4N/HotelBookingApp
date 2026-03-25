using HotelBookingApp.Data;
using HotelBookingApp.Views;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        // Метод загрузки заявок в таблицу
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
                        WHEN 1 THEN 'Ожидает'
                        WHEN 2 THEN 'Одобрено'
                        WHEN 3 THEN 'Отклонено'
                   END AS Status
            FROM Bookings b
            JOIN Users u ON b.UserId = u.Id
            JOIN Rooms r ON b.RoomId = r.Id
            ORDER BY b.StartDate";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                // Отключаем автогенерацию колонок
                BookingsGrid.AutoGenerateColumns = false;
                BookingsGrid.Columns.Clear();

                // Основные колонки
                BookingsGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("Id") });
                BookingsGrid.Columns.Add(new DataGridTextColumn { Header = "Пользователь", Binding = new Binding("UserName") });
                BookingsGrid.Columns.Add(new DataGridTextColumn { Header = "Номер", Binding = new Binding("RoomName") });
                BookingsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Дата начала",
                    Binding = new Binding("StartDate") { StringFormat = "yyyy.MM.dd HH:mm" }
                });
                BookingsGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "Дата окончания",
                    Binding = new Binding("EndDate") { StringFormat = "yyyy.MM.dd HH:mm" }
                });
                BookingsGrid.Columns.Add(new DataGridTextColumn { Header = "Комментарий", Binding = new Binding("Comment") });
                BookingsGrid.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("Status") });

                // Кнопка "Редактировать"
                var editColumn = new DataGridTemplateColumn { Header = "Редактировать" };
                var editFactory = new FrameworkElementFactory(typeof(Button));
                editFactory.SetValue(Button.ContentProperty, "✎"); // Можно "Редактировать"
                editFactory.SetValue(Button.MarginProperty, new Thickness(2));
                editFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditBooking_Click));
                editColumn.CellTemplate = new DataTemplate { VisualTree = editFactory };
                BookingsGrid.Columns.Add(editColumn);

                // Кнопка "Удалить"
                var deleteColumn = new DataGridTemplateColumn { Header = "Удалить" };
                var deleteFactory = new FrameworkElementFactory(typeof(Button));
                deleteFactory.SetValue(Button.ContentProperty, "✖"); // Можно "Удалить"
                deleteFactory.SetValue(Button.MarginProperty, new Thickness(2));
                deleteFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteBooking_Click));
                deleteColumn.CellTemplate = new DataTemplate { VisualTree = deleteFactory };
                BookingsGrid.Columns.Add(deleteColumn);

                // Привязка данных
                BookingsGrid.ItemsSource = table.DefaultView;
            }
        }

        // Открытие окна создания новой заявки
        private void OpenAddBooking(object sender, RoutedEventArgs e)
        {
            AddBookingWindow window = new AddBookingWindow
            {
                Owner = this
            };

            // Подписка на закрытие окна для авто-обновления
            window.Closed += (s, args) => LoadBookings();

            window.ShowDialog();
        }

        // Открытие окна редактирования заявки
        private void EditBooking_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DataRowView row)
            {
                int bookingId = Convert.ToInt32(row["Id"]);
                AddBookingWindow window = new AddBookingWindow(bookingId)
                {
                    Owner = this
                };

                // Подписка на закрытие окна для авто-обновления
                window.Closed += (s, args) => LoadBookings();

                window.ShowDialog();
            }
        }

        // Удаление заявки
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

        // Открытие каталога номеров
        private void OpenRoomCatalog(object sender, RoutedEventArgs e)
        {
            RoomCatalogWindow window = new RoomCatalogWindow();
            window.ShowDialog();
        }
    }
}