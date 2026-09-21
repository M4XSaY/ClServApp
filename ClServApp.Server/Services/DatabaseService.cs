using ClServApp.Server.Models;
using Npgsql;

namespace ClServApp.Server.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Строка подключения к базе данных не найдена.");
        }

        public async Task<bool> TestConnectionAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(
                "SELECT 1;",
                connection);

            var result = await command.ExecuteScalarAsync();

            return result is 1;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var products = new List<Product>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT
            т.id,
            т.наименование,
            т.тип_алкоголя,
            т.крепость,
            т.объем,
            т.цена_за_единицу,
            т.id_категории,
            к.название
        FROM "Товар" т
        INNER JOIN "Категория" к
            ON к.id = т.id_категории
        ORDER BY т.id;
        """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    AlcoholType = reader.GetString(2),
                    Strength = reader.GetDecimal(3),
                    Volume = reader.GetDecimal(4),
                    Price = reader.GetDecimal(5),
                    CategoryId = reader.GetInt32(6),
                    CategoryName = reader.GetString(7)
                });
            }

            return products;
        }

        public async Task<List<Client>> GetClientsAsync()
        {
            var clients = new List<Client>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT
            id,
            наименование,
            тип,
            инн,
            телефон,
            email,
            адрес_доставки
        FROM "Клиент"
        ORDER BY id;
        """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clients.Add(new Client
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Inn = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Phone = reader.GetString(4),
                    Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                    DeliveryAddress = reader.GetString(6)
                });
            }

            return clients;
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            var orders = new List<Order>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT
            з.id,
            з.дата_заказа,
            з.статус,
            к.наименование,
            с.фио,
            з.общая_сумма
        FROM "Заказ" з
        INNER JOIN "Клиент" к
            ON к.id = з.id_клиента
        INNER JOIN "Сотрудник" с
            ON с.id = з.id_сотрудника
        ORDER BY з.id;
        """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(new Order
                {
                    Id = reader.GetInt32(0),
                    OrderDate = reader.GetDateTime(1),
                    Status = reader.GetString(2),
                    ClientName = reader.GetString(3),
                    EmployeeName = reader.GetString(4),
                    Total = reader.GetDecimal(5)
                });
            }

            return orders;
        }

        public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            var details = new List<OrderDetail>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT
            товар,
            количество,
            цена,
            сумма
        FROM get_order_details(@order_id);
        """;

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@order_id", orderId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                details.Add(new OrderDetail
                {
                    Product = reader.GetString(0),
                    Quantity = reader.GetInt32(1),
                    Price = reader.GetDecimal(2),
                    Sum = reader.GetDecimal(3)
                });
            }

            return details;
        }

        public async Task<List<StockItem>> GetStockAsync()
        {
            var stock = new List<StockItem>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT
            "Склад",
            "Товар",
            "Тип алкоголя",
            "Количество",
            "Минимальный запас"
        FROM "Представление складских остатков"
        ORDER BY "Склад", "Товар";
        """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                stock.Add(new StockItem
                {
                    Warehouse = reader.GetString(0),
                    Product = reader.GetString(1),
                    AlcoholType = reader.GetString(2),
                    Quantity = reader.GetInt32(3),
                    MinimumQuantity = reader.GetInt32(4)
                });
            }

            return stock;
        }

        public async Task<List<LowStockItem>> GetLowStockAsync()
        {
            var items = new List<LowStockItem>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT
            склад,
            товар,
            текущий_остаток,
            минимальный_запас
        FROM get_low_stock_products();
        """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new LowStockItem
                {
                    Warehouse = reader.GetString(0),
                    Product = reader.GetString(1),
                    CurrentQuantity = reader.GetInt32(2),
                    MinimumQuantity = reader.GetInt32(3)
                });
            }

            return items;
        }

        public async Task AddClientAsync(ClientCreate client)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        INSERT INTO "Клиент"
        (
            наименование,
            тип,
            инн,
            телефон,
            email,
            адрес_доставки
        )
        VALUES
        (
            @name,
            CAST(@type AS client_type),
            @inn,
            @phone,
            @email,
            @address
        );
        """;

            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@name", client.Name);
            command.Parameters.AddWithValue("@type", client.Type);
            command.Parameters.AddWithValue(
                "@inn",
                (object?)client.Inn ?? DBNull.Value);
            command.Parameters.AddWithValue("@phone", client.Phone);
            command.Parameters.AddWithValue(
                "@email",
                (object?)client.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@address", client.Address);

            await command.ExecuteNonQueryAsync();
        }
    }
}