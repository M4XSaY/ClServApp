// ==========================================
// Работа с товарами
// ==========================================

async function loadProducts() {

    const table = document.getElementById("productsTable");

    table.innerHTML = `
        <tr>
            <td colspan="7">Загрузка данных...</td>
        </tr>
    `;

    try {

        const response = await fetch("/api/products");

        if (!response.ok) {
            throw new Error("Ошибка получения товаров");
        }

        const products = await response.json();

        table.innerHTML = "";

        products.forEach(product => {

            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${product.id}</td>
                <td>${product.name}</td>
                <td>${product.alcoholType}</td>
                <td>${product.strength}%</td>
                <td>${product.volume} л</td>
                <td>${product.price.toFixed(2)} ₽</td>
                <td>${product.categoryName}</td>
            `;

            table.appendChild(row);
        });

    }
    catch (error) {

        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="7">
                    Не удалось загрузить товары.
                </td>
            </tr>
        `;
    }
}


// ==========================================
// Переключение страниц
// ==========================================

const menuItems = document.querySelectorAll(".menu-item");

menuItems.forEach(item => {

    item.addEventListener("click", () => {

        const page = item.dataset.page;

        // Убираем active
        menuItems.forEach(menuItem => {
            menuItem.classList.remove("active");
        });

        item.classList.add("active");

        // Скрываем все страницы
        document.querySelectorAll(".page").forEach(pageElement => {
            pageElement.classList.add("hidden");
        });

        // Показываем выбранную
        document
            .getElementById(page + "Page")
            .classList.remove("hidden");

        updateHeader(page);

        // Загружаем данные нужного раздела
        if (page === "products") {
            loadProducts();
        }

        if (page === "clients") {
            loadClients();
        }

        if (page === "orders") {
            loadOrders();
        }

        if (page === "stock") {
            loadStock();
        }

        document
            .getElementById("refreshStock")
            .addEventListener("click", loadStock);

        document
            .getElementById("refreshOrders")
            .addEventListener("click", loadOrders);
    });
});


// ==========================================
// Заголовок страницы
// ==========================================

function updateHeader(page) {

    const title = document.getElementById("pageTitle");
    const description = document.getElementById("pageDescription");

    if (page === "products") {

        title.textContent = "Товары";
        description.textContent =
            "Список алкогольной продукции";

    }
    else if (page === "clients") {

        title.textContent = "Клиенты";
        description.textContent =
            "Список клиентов компании";

    }
    else if (page === "orders") {

        title.textContent = "Заказы";
        description.textContent =
            "Управление заказами клиентов";

    }
    else if (page === "stock") {

        title.textContent = "Склад";
        description.textContent =
            "Состояние складских остатков";
    }
}


// ==========================================
// Кнопка обновления товаров
// ==========================================

document
    .getElementById("refreshProducts")
    .addEventListener("click", loadProducts);


// ==========================================
// Клиенты
// ==========================================

async function loadClients() {

    const table = document.getElementById("clientsTable");

    table.innerHTML = `
        <tr>
            <td colspan="7">Загрузка данных...</td>
        </tr>
    `;

    try {

        const response = await fetch("/api/clients");

        if (!response.ok) {
            throw new Error("Ошибка получения клиентов");
        }

        const clients = await response.json();

        table.innerHTML = "";

        clients.forEach(client => {

            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${client.id}</td>
                <td>${client.name}</td>
                <td>${client.type}</td>
                <td>${client.inn ?? "-"}</td>
                <td>${client.phone}</td>
                <td>${client.email ?? "-"}</td>
                <td>${client.deliveryAddress}</td>
            `;

            table.appendChild(row);
        });

    }
    catch (error) {

        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="7">
                    Не удалось загрузить клиентов.
                </td>
            </tr>
        `;
    }
}


// ==========================================
// Кнопка обновления клиентов
// ==========================================

document
    .getElementById("refreshClients")
    .addEventListener("click", loadClients);


// ==========================================
// Начальная загрузка
// ==========================================

loadProducts();

// ==========================================
// Заказы
// ==========================================

async function loadOrders() {

    const table = document.getElementById("ordersTable");

    table.innerHTML = `
        <tr>
            <td colspan="7">Загрузка данных...</td>
        </tr>
    `;

    try {

        const response = await fetch("/api/orders");

        if (!response.ok) {
            throw new Error("Ошибка получения заказов");
        }

        const orders = await response.json();

        table.innerHTML = "";

        orders.forEach(order => {

            const row = document.createElement("tr");

            const date = new Date(order.orderDate)
                .toLocaleDateString("ru-RU");

            row.innerHTML = `
                <td>${order.id}</td>
                <td>${date}</td>
                <td>${order.clientName}</td>
                <td>${order.employeeName}</td>
                <td>${order.status}</td>
                <td>${order.total.toFixed(2)} ₽</td>
                <td>
                    <button
                        class="details-button"
                        onclick="showOrderDetails(${order.id})">
                        Подробнее
                    </button>
                </td>
            `;

            table.appendChild(row);
        });

    }
    catch (error) {

        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="7">
                    Не удалось загрузить заказы.
                </td>
            </tr>
        `;
    }
}

// ==========================================
// Детали заказа
// ==========================================

async function showOrderDetails(orderId) {

    const modal = document.getElementById("orderModal");
    const content = document.getElementById("orderDetailsContent");

    modal.classList.remove("hidden");

    content.innerHTML = `
        <p>Загрузка данных...</p>
    `;

    try {

        const response = await fetch(`/api/orders/${orderId}/details`);

        if (!response.ok) {
            throw new Error("Ошибка получения деталей заказа");
        }

        const details = await response.json();

        if (details.length === 0) {

            content.innerHTML = `
                <p>В заказе нет товаров.</p>
            `;

            return;
        }

        let total = 0;

        let rows = "";

        details.forEach(detail => {

            total += detail.sum;

            rows += `
                <tr>
                    <td>${detail.product}</td>
                    <td>${detail.quantity}</td>
                    <td>${detail.price.toFixed(2)} ₽</td>
                    <td>${detail.sum.toFixed(2)} ₽</td>
                </tr>
            `;
        });

        content.innerHTML = `
            <table class="details-table">

                <thead>
                    <tr>
                        <th>Товар</th>
                        <th>Количество</th>
                        <th>Цена</th>
                        <th>Сумма</th>
                    </tr>
                </thead>

                <tbody>
                    ${rows}
                </tbody>

            </table>

            <div class="details-total">
                Итого: ${total.toFixed(2)} ₽
            </div>
        `;

    }
    catch (error) {

        console.error(error);

        content.innerHTML = `
            <p>
                Не удалось загрузить детали заказа.
            </p>
        `;
    }
}

document
    .getElementById("closeOrderModal")
    .addEventListener("click", () => {

        document
            .getElementById("orderModal")
            .classList.add("hidden");
    });

document
    .getElementById("orderModal")
    .addEventListener("click", (event) => {

        if (event.target.id === "orderModal") {

            event.currentTarget.classList.add("hidden");
        }
    });

// ==========================================
// Склад
// ==========================================

async function loadStock() {

    const table = document.getElementById("stockTable");
    const warningBlock = document.getElementById("lowStockBlock");
    const warningContent = document.getElementById("lowStockContent");

    table.innerHTML = `
        <tr>
            <td colspan="5">Загрузка данных...</td>
        </tr>
    `;

    try {

        const response = await fetch("/api/stock");

        if (!response.ok) {
            throw new Error("Ошибка получения складских остатков");
        }

        const stock = await response.json();

        table.innerHTML = "";

        stock.forEach(item => {

            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${item.warehouse}</td>
                <td>${item.product}</td>
                <td>${item.alcoholType}</td>
                <td>${item.quantity}</td>
                <td>${item.minimumQuantity}</td>
            `;

            table.appendChild(row);
        });

        await loadLowStock();

    }
    catch (error) {

        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="5">
                    Не удалось загрузить складские остатки.
                </td>
            </tr>
        `;
    }
}


async function loadLowStock() {

    const warningBlock =
        document.getElementById("lowStockBlock");

    const warningContent =
        document.getElementById("lowStockContent");

    try {

        const response = await fetch("/api/stock/low");

        if (!response.ok) {
            throw new Error("Ошибка получения товаров для пополнения");
        }

        const items = await response.json();

        if (items.length === 0) {

            warningBlock.classList.add("hidden");

            return;
        }

        warningBlock.classList.remove("hidden");

        warningContent.innerHTML = "";

        items.forEach(item => {

            const element = document.createElement("div");

            element.className = "warning-item";

            element.textContent =
                `${item.warehouse}: ${item.product} — ` +
                `остаток ${item.currentQuantity}, ` +
                `минимум ${item.minimumQuantity}`;

            warningContent.appendChild(element);
        });

    }
    catch (error) {

        console.error(error);

        warningBlock.classList.add("hidden");
    }
}

// ==========================================
// Добавление клиента
// ==========================================

const clientModal =
    document.getElementById("clientModal");

const addClientButton =
    document.getElementById("addClientButton");

const closeClientModal =
    document.getElementById("closeClientModal");

const cancelClient =
    document.getElementById("cancelClient");

const clientForm =
    document.getElementById("clientForm");


addClientButton.addEventListener("click", () => {

    clientModal.classList.remove("hidden");

});


closeClientModal.addEventListener("click", () => {

    clientModal.classList.add("hidden");

});


cancelClient.addEventListener("click", () => {

    clientModal.classList.add("hidden");

});


clientForm.addEventListener("submit", async (event) => {

    event.preventDefault();

    const client = {

        name:
            document.getElementById("clientName").value,

        type:
            document.getElementById("clientType").value,

        inn:
            document.getElementById("clientInn").value,

        phone:
            document.getElementById("clientPhone").value,

        email:
            document.getElementById("clientEmail").value,

        address:
            document.getElementById("clientAddress").value
    };


    try {

        const response = await fetch(
            "/api/clients",
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json"
                },

                body: JSON.stringify(client)
            }
        );


        if (!response.ok) {

            throw new Error(
                "Не удалось добавить клиента"
            );
        }


        alert("Клиент успешно добавлен.");

        clientForm.reset();

        clientModal.classList.add("hidden");

        await loadClients();

    }
    catch (error) {

        console.error(error);

        alert(
            "Ошибка при добавлении клиента."
        );
    }

});

// ==========================================
// Создание нового заказа (Модальное окно и отправка)
// ==========================================

// Глобальная функция для открытия модалки заказа (вызывается из HTML по кнопке)
// Открытие модального окна для создания заказа
window.openOrderModal = async function() {
    const modal = document.getElementById("orderModal");
    const title = document.getElementById("orderModalTitle");
    const detailsContent = document.getElementById("orderDetailsContent");
    const orderForm = document.getElementById("orderForm");
    
    if (title) title.textContent = "Новый заказ";
    if (detailsContent) detailsContent.style.display = "none";
    if (orderForm) orderForm.style.display = "block";
    if (modal) modal.classList.remove("hidden");

    const clientSelect = document.getElementById("orderClient");
    const productSelect = document.getElementById("orderProduct");

    try {
        const response = await fetch("/api/orders/form-data");
        if (!response.ok) throw new Error("Не удалось загрузить данные формы");
        const data = await response.json();

        if (clientSelect) {
            clientSelect.innerHTML = '<option value="">Выберите клиента</option>';
            data.clients.forEach(c => {
                clientSelect.innerHTML += `<option value="${c.id}">${c.name}</option>`;
            });
        }

        if (productSelect) {
            productSelect.innerHTML = '<option value="">Выберите товар</option>';
            data.products.forEach(p => {
                productSelect.innerHTML += `<option value="${p.id}">${p.name} (${p.price} ₽)</option>`;
            });
        }
    } catch (err) {
        console.error(err);
        alert("Ошибка при загрузке списков для заказа");
    }
}

// Измени также функцию показа деталей, чтобы она скрывала форму создания:
async function showOrderDetails(orderId) {
    const modal = document.getElementById("orderModal");
    const title = document.getElementById("orderModalTitle");
    const content = document.getElementById("orderDetailsContent");
    const orderForm = document.getElementById("orderForm");

    if (title) title.textContent = "Детали заказа #" + orderId;
    if (orderForm) orderForm.style.display = "none";
    if (content) content.style.display = "block";
    if (modal) modal.classList.remove("hidden");

    content.innerHTML = `<p>Загрузка данных...</p>`;

    try {
        const response = await fetch(`/api/orders/${orderId}/details`);
        if (!response.ok) throw new Error("Ошибка получения деталей заказа");
        const details = await response.json();

        if (details.length === 0) {
            content.innerHTML = `<p>В заказе нет товаров.</p>`;
            return;
        }

        let total = 0;
        let rows = "";

        details.forEach(detail => {
            total += detail.sum;
            rows += `
                <tr>
                    <td>${detail.product}</td>
                    <td>${detail.quantity}</td>
                    <td>${detail.price.toFixed(2)} ₽</td>
                    <td>${detail.sum.toFixed(2)} ₽</td>
                </tr>
            `;
        });

        content.innerHTML = `
            <table class="details-table">
                <thead>
                    <tr>
                        <th>Товар</th>
                        <th>Количество</th>
                        <th>Цена</th>
                        <th>Сумма</th>
                    </tr>
                </thead>
                <tbody>${rows}</tbody>
            </table>
            <div class="details-total" style="margin-top: 10px; font-weight: bold;">
                Итого: ${total.toFixed(2)} ₽
            </div>
        `;
    } catch (error) {
        console.error(error);
        content.innerHTML = `<p>Не удалось загрузить детали заказа.</p>`;
    }
}

window.closeOrderModal = function() {
    const modal = document.getElementById("orderModal");
    if (modal) modal.classList.add("hidden");
}
const orderForm = document.getElementById("orderForm");
if (orderForm) {
    orderForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const orderData = {
            clientId: parseInt(document.getElementById("orderClient").value),
            employeeId: 1, // Фиксированный ID сотрудника для примера
            productId: parseInt(document.getElementById("orderProduct").value),
            quantity: parseInt(document.getElementById("orderQuantity").value)
        };

        if (orderData.quantity <= 0) {
            alert("Количество товара должно быть больше нуля!");
            return;
        }

        try {
            const response = await fetch("/api/orders", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(orderData)
            });

            const result = await response.json();
            if (!response.ok) throw new Error(result.message || "Ошибка создания заказа");

            alert("Заказ успешно создан!");
            closeOrderModal();
            loadOrders();
        } catch (err) {
            alert("Ошибка: " + err.message);
        }
    });
}