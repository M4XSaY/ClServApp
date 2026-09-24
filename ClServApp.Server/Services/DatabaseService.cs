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

        public async Task<List<(int Id, string Name)>> GetCategoriesAsync()
        {
            var categories = new List<(int Id, string Name)>();

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = """
        SELECT id, название
        FROM "Категория"
        ORDER BY название;
        """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add((
                    reader.GetInt32(0),
                    reader.GetString(1)
                ));
            }

            return categories;
        }

        // ==========================================
        // Товары: добавление
        // ==========================================

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Название товара не может быть пустым.");

            if (string.IsNullOrWhiteSpace(product.AlcoholType))
                throw new ArgumentException("Тип алкоголя не может быть пустым.");

            if (product.Strength < 0)
                throw new ArgumentException("Крепость не может быть отрицательной.");

            if (product.Volume <= 0)
                throw new ArgumentException("Объём должен быть больше нуля.");

            if (product.Price < 0)
                throw new ArgumentException("Цена не может быть отрицательной.");

            if (product.CategoryId <= 0)
                throw new ArgumentException("Необходимо выбрать категорию.");

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string insertSql = """
        INSERT INTO "Товар"
        (
            наименование,
            тип_алкоголя,
            крепость,
            объем,
            цена_за_единицу,
            id_категории
        )
        VALUES
        (
            @name,
            CAST(@alcoholType AS alcohol_type),
            @strength,
            @volume,
            @price,
            @categoryId
        );
        """;

            await using var command = new NpgsqlCommand(insertSql, connection);

            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@alcoholType", product.AlcoholType);
            command.Parameters.AddWithValue("@strength", product.Strength);
            command.Parameters.AddWithValue("@volume", product.Volume);
            command.Parameters.AddWithValue("@price", product.Price);
            command.Parameters.AddWithValue("@categoryId", product.CategoryId);

            await command.ExecuteNonQueryAsync();
        }


        // ==========================================
        // Товары: изменение
        // ==========================================

        public async Task UpdateProductAsync(Product product)
        {
            if (product.Id <= 0)
                throw new ArgumentException("Некорректный ID товара.");

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Название товара не может быть пустым.");

            if (string.IsNullOrWhiteSpace(product.AlcoholType))
                throw new ArgumentException("Тип алкоголя не может быть пустым.");

            if (product.Strength < 0)
                throw new ArgumentException("Крепость не может быть отрицательной.");

            if (product.Volume <= 0)
                throw new ArgumentException("Объём должен быть больше нуля.");

            if (product.Price < 0)
                throw new ArgumentException("Цена не может быть отрицательной.");

            if (product.CategoryId <= 0)
                throw new ArgumentException("Необходимо выбрать категорию.");

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string updateSql = """
        UPDATE "Товар"
        SET
            наименование = @name,
            тип_алкоголя = CAST(@alcoholType AS alcohol_type),
            крепость = @strength,
            объем = @volume,
            цена_за_единицу = @price,
            id_категории = @categoryId
        WHERE id = @id;
        """;

            await using var command = new NpgsqlCommand(updateSql, connection);

            command.Parameters.AddWithValue("@id", product.Id);
            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@alcoholType", product.AlcoholType);
            command.Parameters.AddWithValue("@strength", product.Strength);
            command.Parameters.AddWithValue("@volume", product.Volume);
            command.Parameters.AddWithValue("@price", product.Price);
            command.Parameters.AddWithValue("@categoryId", product.CategoryId);

            var affectedRows = await command.ExecuteNonQueryAsync();

            if (affectedRows == 0)
                throw new InvalidOperationException("Товар с указанным ID не найден.");
        }


        // ==========================================
        // Товары: удаление
        // ==========================================

        public async Task DeleteProductAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Некорректный ID товара.");

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                const string checkProductSql = """
        SELECT COUNT(*)
        FROM "Товар"
        WHERE id = @id;
        """;

                await using (var checkProductCommand =
                    new NpgsqlCommand(
                        checkProductSql,
                        connection,
                        transaction))
                {
                    checkProductCommand.Parameters.AddWithValue(
                        "id",
                        id);

                    var productExists =
                        Convert.ToInt32(
                            await checkProductCommand.ExecuteScalarAsync());

                    if (productExists == 0)
                        throw new Exception("Товар с указанным ID не найден.");
                }

                const string checkOrdersSql = """
        SELECT COUNT(*)
        FROM "Детали заказа"
        WHERE id_товара = @id;
        """;

                await using (var checkOrdersCommand =
                    new NpgsqlCommand(
                        checkOrdersSql,
                        connection,
                        transaction))
                {
                    checkOrdersCommand.Parameters.AddWithValue(
                        "id",
                        id);

                    var orderCount =
                        Convert.ToInt32(
                            await checkOrdersCommand.ExecuteScalarAsync());

                    if (orderCount > 0)
                    {
                        throw new Exception(
                            "Нельзя удалить товар, поскольку он используется в существующих заказах.");
                    }
                }

                const string deleteSql = """
        DELETE FROM "Товар"
        WHERE id = @id;
        """;

                await using (var deleteCommand =
                    new NpgsqlCommand(
                        deleteSql,
                        connection,
                        transaction))
                {
                    deleteCommand.Parameters.AddWithValue(
                        "id",
                        id);

                    await deleteCommand.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Client>> GetClientsAsync()
        {
            var clients = new List<Client>();

            await using var connection =
                new NpgsqlConnection(_connectionString);

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

            await using var command =
                new NpgsqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clients.Add(new Client
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Inn = reader.IsDBNull(3)
                        ? null
                        : reader.GetString(3),
                    Phone = reader.GetString(4),
                    Email = reader.IsDBNull(5)
                        ? null
                        : reader.GetString(5),
                    DeliveryAddress = reader.GetString(6)
                });
            }

            return clients;


        }

        public async Task<Client?> GetClientAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException(
                "Некорректный ID клиента.");

            await using var connection =
                new NpgsqlConnection(_connectionString);

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
    WHERE id = @id;
    """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "id",
                id);

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Client
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Type = reader.GetString(2),
                Inn = reader.IsDBNull(3)
                    ? null
                    : reader.GetString(3),
                Phone = reader.GetString(4),
                Email = reader.IsDBNull(5)
                    ? null
                    : reader.GetString(5),
                DeliveryAddress = reader.GetString(6)
            };


        }

        public async Task<int> AddClientAsync(ClientCreate client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(client.Name))
                throw new ArgumentException(
                    "Наименование клиента не может быть пустым.");

            if (string.IsNullOrWhiteSpace(client.Type))
                throw new ArgumentException(
                    "Необходимо указать тип клиента.");

            if (string.IsNullOrWhiteSpace(client.Phone))
                throw new ArgumentException(
                    "Телефон клиента не может быть пустым.");

            if (string.IsNullOrWhiteSpace(client.Address))
                throw new ArgumentException(
                    "Адрес доставки не может быть пустым.");

            await using var connection =
                new NpgsqlConnection(_connectionString);

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
    )
    RETURNING id;
    """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "name",
                client.Name.Trim());

            command.Parameters.AddWithValue(
                "type",
                client.Type);

            command.Parameters.AddWithValue(
                "inn",
                string.IsNullOrWhiteSpace(client.Inn)
                    ? DBNull.Value
                    : client.Inn.Trim());

            command.Parameters.AddWithValue(
                "phone",
                client.Phone.Trim());

            command.Parameters.AddWithValue(
                "email",
                string.IsNullOrWhiteSpace(client.Email)
                    ? DBNull.Value
                    : client.Email.Trim());

            command.Parameters.AddWithValue(
                "address",
                client.Address.Trim());

            var result =
                await command.ExecuteScalarAsync();

            if (result == null)
                throw new InvalidOperationException(
                    "Не удалось получить ID нового клиента.");

            return Convert.ToInt32(result);


        }

        public async Task DeleteClientAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException(
                "Некорректный ID клиента.");

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                // Проверяем существование клиента
                const string checkClientSql = """
        SELECT COUNT(*)
        FROM "Клиент"
        WHERE id = @id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        checkClientSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "id",
                        id);

                    var exists =
                        Convert.ToInt32(
                            await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        throw new KeyNotFoundException(
                            "Клиент с указанным ID не найден.");
                    }
                }


                // Проверяем наличие заказов клиента
                const string checkOrdersSql = """
        SELECT COUNT(*)
        FROM "Заказ"
        WHERE id_клиента = @id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        checkOrdersSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "id",
                        id);

                    var orderCount =
                        Convert.ToInt32(
                            await command.ExecuteScalarAsync());

                    if (orderCount > 0)
                    {
                        throw new InvalidOperationException(
                            "Нельзя удалить клиента, поскольку он используется в существующих заказах.");
                    }
                }


                // Удаляем клиента
                const string deleteSql = """
        DELETE FROM "Клиент"
        WHERE id = @id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        deleteSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "id",
                        id);

                    var affectedRows =
                        await command.ExecuteNonQueryAsync();

                    if (affectedRows == 0)
                    {
                        throw new KeyNotFoundException(
                            "Клиент с указанным ID не найден.");
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


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


        public async Task AddOrderAsync(OrderCreate order)
        {
            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                // ==========================================
                // 1. Проверяем данные заказа
                // ==========================================

                if (order.ClientId <= 0)
                    throw new Exception("Не выбран клиент.");

                if (order.EmployeeId <= 0)
                    throw new Exception("Не выбран сотрудник.");

                if (order.Items == null || order.Items.Count == 0)
                    throw new Exception("В заказе должна быть хотя бы одна товарная позиция.");

                foreach (var item in order.Items)
                {
                    if (item.ProductId <= 0)
                        throw new Exception("В заказе указан некорректный товар.");

                    if (item.Quantity <= 0)
                        throw new Exception(
                            "Количество товара должно быть больше нуля.");
                }


                // ==========================================
                // 2. Проверяем существование клиента
                // ==========================================

                const string clientCheckSql = """
            SELECT COUNT(*)
            FROM "Клиент"
            WHERE id = @client_id;
            """;

                await using (var cmd = new NpgsqlCommand(
                    clientCheckSql,
                    connection,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "client_id",
                        order.ClientId);

                    var count =
                        Convert.ToInt32(
                            await cmd.ExecuteScalarAsync());

                    if (count == 0)
                    {
                        throw new Exception(
                            $"Клиент с ID {order.ClientId} не найден.");
                    }
                }


                // ==========================================
                // 3. Проверяем существование сотрудника
                // ==========================================

                const string employeeCheckSql = """
            SELECT COUNT(*)
            FROM "Сотрудник"
            WHERE id = @employee_id;
            """;

                await using (var cmd = new NpgsqlCommand(
                    employeeCheckSql,
                    connection,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "employee_id",
                        order.EmployeeId);

                    var count =
                        Convert.ToInt32(
                            await cmd.ExecuteScalarAsync());

                    if (count == 0)
                    {
                        throw new Exception(
                            $"Сотрудник с ID {order.EmployeeId} не найден.");
                    }
                }


                // ==========================================
                // 4. Создаём заказ
                // ==========================================

                const string orderSql = """
            INSERT INTO "Заказ"
            (
                id,
                id_клиента,
                id_сотрудника,
                дата_заказа,
                статус,
                общая_сумма
            )
            VALUES
            (
                (
                    SELECT COALESCE(
                        (
                            SELECT MIN(t.id + 1)
                            FROM "Заказ" t
                            WHERE NOT EXISTS
                            (
                                SELECT 1
                                FROM "Заказ" t2
                                WHERE t2.id = t.id + 1
                            )
                        ),
                        1
                    )
                ),
                @client_id,
                @employee_id,
                CURRENT_TIMESTAMP,
                'Новый',
                0
            )
            RETURNING id;
            """;

                int orderId;

                await using (var cmd = new NpgsqlCommand(
                    orderSql,
                    connection,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "client_id",
                        order.ClientId);

                    cmd.Parameters.AddWithValue(
                        "employee_id",
                        order.EmployeeId);

                    orderId =
                        Convert.ToInt32(
                            await cmd.ExecuteScalarAsync());
                }


                // ==========================================
                // 5. Добавляем все товары заказа
                // ==========================================

                const string priceSql = """
            SELECT цена_за_единицу
            FROM "Товар"
            WHERE id = @product_id;
            """;

                const string detailSql = """
            INSERT INTO "Детали заказа"
            (
                id_заказа,
                id_товара,
                количество,
                цена
            )
            VALUES
            (
                @order_id,
                @product_id,
                @quantity,
                @price
            );
            """;

                foreach (var item in order.Items)
                {
                    decimal price;

                    // Получаем цену товара
                    await using (var priceCommand = new NpgsqlCommand(
                        priceSql,
                        connection,
                        transaction))
                    {
                        priceCommand.Parameters.AddWithValue(
                            "product_id",
                            item.ProductId);

                        var result =
                            await priceCommand.ExecuteScalarAsync();

                        if (result == null)
                        {
                            throw new Exception(
                                $"Товар с ID {item.ProductId} не найден.");
                        }

                        price = Convert.ToDecimal(result);
                    }


                    // Добавляем позицию
                    await using (var detailCommand = new NpgsqlCommand(
                        detailSql,
                        connection,
                        transaction))
                    {
                        detailCommand.Parameters.AddWithValue(
                            "order_id",
                            orderId);

                        detailCommand.Parameters.AddWithValue(
                            "product_id",
                            item.ProductId);

                        detailCommand.Parameters.AddWithValue(
                            "quantity",
                            item.Quantity);

                        detailCommand.Parameters.AddWithValue(
                            "price",
                            price);

                        await detailCommand.ExecuteNonQueryAsync();
                    }
                }


                // ==========================================
                // 6. Рассчитываем общую сумму заказа
                // ==========================================

                const string totalSql = """
            UPDATE "Заказ"
            SET общая_сумма =
            (
                SELECT COALESCE(
                    SUM(количество * цена),
                    0
                )
                FROM "Детали заказа"
                WHERE id_заказа = @order_id
            )
            WHERE id = @order_id;
            """;

                await using (var cmd = new NpgsqlCommand(
                    totalSql,
                    connection,
                    transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "order_id",
                        orderId);

                    await cmd.ExecuteNonQueryAsync();
                }


                // ==========================================
                // 7. Подтверждаем транзакцию
                // ==========================================

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            var employees = new List<Employee>();

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = """
        SELECT
            с.id,
            с.фио,
            с.id_должности,
            д.название_должности,
            с.дата_найма,
            с.зарплата,
            с.id_отдела,
            о.название_отдела
        FROM "Сотрудник" с
        INNER JOIN "Должность" д
            ON д.id = с.id_должности
        INNER JOIN "Отдел" о
            ON о.id = с.id_отдела
        ORDER BY с.id;
        """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    PositionId = reader.GetInt32(2),
                    PositionName = reader.GetString(3),
                    HireDate = reader.GetDateTime(4),
                    Salary = reader.GetDecimal(5),
                    DepartmentId = reader.GetInt32(6),
                    DepartmentName = reader.GetString(7)
                });
            }

            return employees;
        }

        public async Task<List<(int Id, string Name)>> GetPositionsAsync()
        {
            var positions = new List<(int Id, string Name)>();

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = """
        SELECT id, название_должности
        FROM "Должность"
        ORDER BY название_должности;
        """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                positions.Add((
                    reader.GetInt32(0),
                    reader.GetString(1)
                ));
            }

            return positions;
        }

        public async Task<List<(int Id, string Name)>> GetDepartmentsAsync()
        {
            var departments = new List<(int Id, string Name)>();

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = """
        SELECT id, название_отдела
        FROM "Отдел"
        ORDER BY название_отдела;
        """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                departments.Add((
                    reader.GetInt32(0),
                    reader.GetString(1)
                ));
            }

            return departments;
        }

        public async Task AddEmployeeAsync(EmployeeCreate employee)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            const string sql = @"
        INSERT INTO ""Сотрудник""
        (
            id,
            фио,
            id_должности,
            дата_найма,
            зарплата,
            id_отдела
        )
        VALUES
        (
            (
                SELECT COALESCE(
                    (
                        SELECT MIN(t.id + 1)
                        FROM ""Сотрудник"" t
                        WHERE NOT EXISTS
                        (
                            SELECT 1
                            FROM ""Сотрудник"" t2
                            WHERE t2.id = t.id + 1
                        )
                    ),
                    1
                )
            ),
            @name,
            @positionId,
            @hireDate,
            @salary,
            @departmentId
        );
    ";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@name", employee.Name);
            command.Parameters.AddWithValue("@positionId", employee.PositionId);
            command.Parameters.AddWithValue("@hireDate", employee.HireDate);
            command.Parameters.AddWithValue("@salary", employee.Salary);
            command.Parameters.AddWithValue("@departmentId", employee.DepartmentId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateEmployeeAsync(
    int id,
    EmployeeCreate employee)
        {
            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = """
        UPDATE "Сотрудник"
        SET
            фио = @name,
            id_должности = @position_id,
            дата_найма = @hire_date,
            зарплата = @salary,
            id_отдела = @department_id
        WHERE id = @id;
        """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "id",
                id);

            command.Parameters.AddWithValue(
                "name",
                employee.Name);

            command.Parameters.AddWithValue(
                "position_id",
                employee.PositionId);

            command.Parameters.AddWithValue(
                "hire_date",
                employee.HireDate.Date);

            command.Parameters.AddWithValue(
                "salary",
                employee.Salary);

            command.Parameters.AddWithValue(
                "department_id",
                employee.DepartmentId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = """
        DELETE FROM "Сотрудник"
        WHERE id = @id;
        """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "id",
                id);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateOrderAsync(
    int orderId,
    OrderEdit order)
        {
            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var transaction =
                await connection.BeginTransactionAsync();

            try
            {
                // Проверяем существование заказа
                const string checkOrderSql = """
        SELECT COUNT(*)
        FROM "Заказ"
        WHERE id = @order_id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        checkOrderSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "order_id",
                        orderId);

                    var exists =
                        Convert.ToInt32(
                            await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        throw new Exception(
                            "Заказ с указанным ID не найден.");
                    }
                }

                // Проверяем клиента
                const string checkClientSql = """
        SELECT COUNT(*)
        FROM "Клиент"
        WHERE id = @client_id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        checkClientSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "client_id",
                        order.ClientId);

                    var exists =
                        Convert.ToInt32(
                            await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        throw new Exception(
                            "Выбранный клиент не найден.");
                    }
                }

                // Проверяем сотрудника
                const string checkEmployeeSql = """
        SELECT COUNT(*)
        FROM "Сотрудник"
        WHERE id = @employee_id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        checkEmployeeSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "employee_id",
                        order.EmployeeId);

                    var exists =
                        Convert.ToInt32(
                            await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        throw new Exception(
                            "Выбранный сотрудник не найден.");
                    }
                }

                if (order.Items == null ||
                    order.Items.Count == 0)
                {
                    throw new Exception(
                        "Заказ должен содержать хотя бы один товар.");
                }

                foreach (var item in order.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        throw new Exception(
                            "Количество товара должно быть больше нуля.");
                    }

                    const string checkProductSql = """
            SELECT COUNT(*)
            FROM "Товар"
            WHERE id = @product_id;
            """;

                    await using var command =
                        new NpgsqlCommand(
                            checkProductSql,
                            connection,
                            transaction);

                    command.Parameters.AddWithValue(
                        "product_id",
                        item.ProductId);

                    var exists =
                        Convert.ToInt32(
                            await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        throw new Exception(
                            $"Товар с ID {item.ProductId} не найден.");
                    }
                }

                // Обновляем основные данные заказа
                const string updateOrderSql = """
        UPDATE "Заказ"
        SET
            id_клиента = @client_id,
            id_сотрудника = @employee_id
        WHERE id = @order_id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        updateOrderSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "order_id",
                        orderId);

                    command.Parameters.AddWithValue(
                        "client_id",
                        order.ClientId);

                    command.Parameters.AddWithValue(
                        "employee_id",
                        order.EmployeeId);

                    await command.ExecuteNonQueryAsync();
                }

                // Удаляем старые позиции
                const string deleteDetailsSql = """
        DELETE FROM "Детали заказа"
        WHERE id_заказа = @order_id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        deleteDetailsSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "order_id",
                        orderId);

                    await command.ExecuteNonQueryAsync();
                }

                // Добавляем новые позиции
                const string priceSql = """
        SELECT цена_за_единицу
        FROM "Товар"
        WHERE id = @product_id;
        """;

                const string insertDetailSql = """
        INSERT INTO "Детали заказа"
        (
            id_заказа,
            id_товара,
            количество,
            цена
        )
        VALUES
        (
            @order_id,
            @product_id,
            @quantity,
            @price
        );
        """;

                foreach (var item in order.Items)
                {
                    decimal price;

                    await using (var command =
                        new NpgsqlCommand(
                            priceSql,
                            connection,
                            transaction))
                    {
                        command.Parameters.AddWithValue(
                            "product_id",
                            item.ProductId);

                        var result =
                            await command.ExecuteScalarAsync();

                        if (result == null)
                        {
                            throw new Exception(
                                "Не удалось получить цену товара.");
                        }

                        price =
                            Convert.ToDecimal(result);
                    }

                    await using (var command =
                        new NpgsqlCommand(
                            insertDetailSql,
                            connection,
                            transaction))
                    {
                        command.Parameters.AddWithValue(
                            "order_id",
                            orderId);

                        command.Parameters.AddWithValue(
                            "product_id",
                            item.ProductId);

                        command.Parameters.AddWithValue(
                            "quantity",
                            item.Quantity);

                        command.Parameters.AddWithValue(
                            "price",
                            price);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Пересчитываем общую сумму
                const string updateTotalSql = """
        UPDATE "Заказ"
        SET общая_сумма =
        (
            SELECT COALESCE(
                SUM(количество * цена),
                0
            )
            FROM "Детали заказа"
            WHERE id_заказа = @order_id
        )
        WHERE id = @order_id;
        """;

                await using (var command =
                    new NpgsqlCommand(
                        updateTotalSql,
                        connection,
                        transaction))
                {
                    command.Parameters.AddWithValue(
                        "order_id",
                        orderId);

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderEdit> GetOrderForEditAsync(int orderId)
        {
            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            const string orderSql = """
        SELECT
            id_клиента,
            id_сотрудника
        FROM "Заказ"
        WHERE id = @order_id;
        """;

            int clientId;
            int employeeId;

            await using (var command = new NpgsqlCommand(orderSql, connection))
            {
                command.Parameters.AddWithValue("order_id", orderId);

                await using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new Exception("Заказ с указанным ID не найден.");
                }

                clientId = reader.GetInt32(0);
                employeeId = reader.GetInt32(1);
            }

            const string itemsSql = """
        SELECT
            id_товара,
            количество
        FROM "Детали заказа"
        WHERE id_заказа = @order_id
        ORDER BY id_товара;
        """;

            var items = new List<OrderItemCreate>();

            await using (var command = new NpgsqlCommand(itemsSql, connection))
            {
                command.Parameters.AddWithValue("order_id", orderId);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    items.Add(new OrderItemCreate
                    {
                        ProductId = reader.GetInt32(0),
                        Quantity = reader.GetInt32(1)
                    });
                }
            }

            return new OrderEdit
            {
                ClientId = clientId,
                EmployeeId = employeeId,
                Items = items
            };
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Проверяем, существует ли заказ
                const string checkSql = """
            SELECT COUNT(*)
            FROM "Заказ"
            WHERE id = @order_id;
            """;

                await using (var command = new NpgsqlCommand(checkSql, connection, transaction))
                {
                    command.Parameters.AddWithValue("order_id", orderId);

                    var exists = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        throw new Exception("Заказ с указанным ID не найден.");
                    }
                }

                // Сначала удаляем детали заказа
                const string deleteDetailsSql = """
            DELETE FROM "Детали заказа"
            WHERE id_заказа = @order_id;
            """;

                await using (var command = new NpgsqlCommand(
                    deleteDetailsSql,
                    connection,
                    transaction))
                {
                    command.Parameters.AddWithValue("order_id", orderId);

                    await command.ExecuteNonQueryAsync();
                }

                // Затем удаляем сам заказ
                const string deleteOrderSql = """
            DELETE FROM "Заказ"
            WHERE id = @order_id;
            """;

                await using (var command = new NpgsqlCommand(
                    deleteOrderSql,
                    connection,
                    transaction))
                {
                    command.Parameters.AddWithValue("order_id", orderId);

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}