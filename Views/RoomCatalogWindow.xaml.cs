using HotelBookingApp.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace HotelBookingApp.Views
{
    public partial class RoomCatalogWindow : Window
    {
        Db db = new Db();

        public RoomCatalogWindow()
        {
            InitializeComponent();
            LoadRooms();
        }

        private void LoadRooms()
        {
            using (SqlConnection conn = db.GetConnection())
            {
                conn.Open();

                // Запрос для каталога номеров с русскими статусами и подсчётом свободных мест
                string query = @"
                    SELECT 
                        r.Id,
                        r.Name AS Class,             -- класс номера (берём Name)
                        r.Price,
                        r.Capacity - COUNT(b.Id) AS FreePlaces  -- свободные места
                    FROM Rooms r
                    LEFT JOIN Bookings b
                        ON r.Id = b.RoomId AND b.StatusId = 2  -- учитываем только одобренные брони
                    GROUP BY r.Id, r.Name, r.Price, r.Capacity
                    ORDER BY r.Id";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                RoomsGrid.ItemsSource = table.DefaultView;
            }
        }
    }
}