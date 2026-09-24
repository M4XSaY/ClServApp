function escapeHtml(value) {
    if (value === null || value === undefined) {
        return "";
    }

/* ==========================================
   Удаление клиента
   ========================================== */

const deleteClientModal = document.getElementById("deleteClientModal");
const closeDeleteClientModal = document.getElementById("closeDeleteClientModal");
const cancelDeleteClient = document.getElementById("cancelDeleteClient");
const confirmDeleteClient = document.getElementById("confirmDeleteClient");
const deleteClientName = document.getElementById("deleteClientName");

let clientToDeleteId = null;

function openDeleteClientModal(clientId, clientName) {

    clientToDeleteId = Number(clientId);

    if (deleteClientName) deleteClientName.textContent = clientName;

    if (deleteClientModal) deleteClientModal.classList.remove("hidden");

}

function closeDeleteClientConfirmation() {

    clientToDeleteId = null;

    if (deleteClientModal) deleteClientModal.classList.add("hidden");

}

if (closeDeleteClientModal) {
    closeDeleteClientModal.addEventListener("click", closeDeleteClientConfirmation);
}

if (cancelDeleteClient) {
    cancelDeleteClient.addEventListener("click", closeDeleteClientConfirmation);
}

if (deleteClientModal) {
    deleteClientModal.addEventListener("click", event => {
        if (event.target === deleteClientModal) closeDeleteClientConfirmation();
    });
}

if (confirmDeleteClient) { confirmDeleteClient.addEventListener("click", async () => { 
    try { const response = await fetch(`/api/clients/${clientToDeleteId}`, { method: "DELETE" }); 
    if (!response.ok) throw new Error(await response.text()); 
    
    closeDeleteClientConfirmation(); 
    showToast(`Клиент успешно удалён.`, "success"); 
    await loadClients(); 
    }
    catch (error) { 
        console.error(error); 
        showToast(`Ошибка при удалении клиента:\n${error.message || error}`, "error"); 
        } finally {
            if (confirmDeleteClient) {
                confirmDeleteClient.disabled = false;
                confirmDeleteClient.textContent = "Удалить";
            }
        }

    });
}

/* ==========================================
   Редактирование клиента
   ========================================== */

async function editClient(clientId) {

    try {
        const response = await fetch(`/api/clients/${clientId}`);
        if (!response.ok) throw new Error("Не удалось загрузить данные клиента.");

        const client = await response.json();

        editingClientId = Number(clientId);

        document.getElementById("clientName").value = client.name ?? "";
        document.getElementById("clientType").value = client.type ?? "Юридическое лицо";
        document.getElementById("clientInn").value = client.inn ?? "";
        document.getElementById("clientPhone").value = client.phone ?? "";
        document.getElementById("clientEmail").value = client.email ?? "";
        document.getElementById("clientAddress").value = client.deliveryAddress ?? "";

        const submitButton = clientForm ? clientForm.querySelector('button[type="submit"]') : null;
        if (submitButton) {
            submitButton.textContent = "Сохранить изменения";
            submitButton.disabled = false;
        }

        const titleEl = document.querySelector('#clientModal .modal-header h2');
        if (titleEl) titleEl.textContent = `Редактирование клиента №${clientId}`;

        clientModal.classList.remove("hidden");

    } catch (error) {
        console.error(error);
        alert("Ошибка при открытии формы редактирования клиента:\n" + (error.message || error));
    }

}

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// ==========================================
// Работа с товарами
// ==========================================
let allProducts = [];

async function loadProducts() {

    const table = document.getElementById("productsTable");

    if (!table) {
        return;
    }

    table.innerHTML = `
        <tr>
            <td colspan="8">Загрузка данных...</td>
        </tr>
    `;

    try {

        const response = await fetch("/api/products");

        if (!response.ok) {
            throw new Error("Не удалось загрузить товары.");
        }

        allProducts = await response.json();

        await loadProductCategoryFilter();

        renderProducts();

    } catch (error) {

        console.error("Ошибка загрузки товаров:", error);

        table.innerHTML = `
            <tr>
                <td colspan="8">
                    Не удалось загрузить товары
                </td>
            </tr>
        `;
    }
}

function renderProducts() {

    const table = document.getElementById("productsTable");

    if (!table) {
        return;
    }

    const searchInput =
        document.getElementById("productSearch");

    const categoryFilter =
        document.getElementById("productCategoryFilter");


    const searchText =
        searchInput
            ? searchInput.value.trim().toLowerCase()
            : "";

    const selectedCategory =
        categoryFilter
            ? categoryFilter.value
            : "";


    const filteredProducts =
        allProducts.filter(product => {

            const matchesSearch =
                !searchText ||
                String(product.name || "")
                    .toLowerCase()
                    .includes(searchText);


            const matchesCategory =
                !selectedCategory ||
                Number(product.categoryId) ===
                Number(selectedCategory);


            return matchesSearch && matchesCategory;

        });


    if (!filteredProducts.length) {

        table.innerHTML = `
            <tr>
                <td colspan="8">
                    По заданным условиям товары не найдены.
                </td>
            </tr>
        `;

        return;
    }


    table.innerHTML = filteredProducts.map(product => `

        <tr>

            <td>${product.id}</td>

            <td>${escapeHtml(product.name)}</td>

            <td>${escapeHtml(product.categoryName)}</td>

            <td>${escapeHtml(product.alcoholType)}</td>

            <td>
                ${Number(product.strength).toFixed(1)}%
            </td>

            <td>
                ${Number(product.volume).toFixed(2)} л
            </td>

            <td>
                ${Number(product.price).toLocaleString(
                    "ru-RU",
                    {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    }
                )} ₽
            </td>

            <td class="table-actions">

                <button
                    type="button"
                    class="action-button edit-button"
                    onclick="editProduct(${product.id})"
                    title="Редактировать">
                    ✏️
                </button>

                <button
                    type="button"
                    class="action-button delete-button"
                    onclick="openDeleteProductModal(
                        ${product.id},
                        '${escapeHtml(product.name)}'
                    )"
                    title="Удалить">
                    🗑️
                </button>

            </td>

        </tr>

    `).join("");
}

async function loadProductCategoryFilter() {

    const select =
        document.getElementById("productCategoryFilter");

    if (!select) {
        return;
    }

    try {

        const response =
            await fetch("/api/products/categories");

        if (!response.ok) {
            throw new Error(
                "Не удалось загрузить категории."
            );
        }

        const categories =
            await response.json();

        const currentValue =
            select.value;

        select.innerHTML = `
            <option value="">
                Все категории
            </option>

            ${categories.map(category => `
                <option value="${category.id}">
                    ${escapeHtml(category.name)}
                </option>
            `).join("")}
        `;

        if (
            currentValue &&
            categories.some(
                category =>
                    String(category.id) === currentValue
            )
        ) {
            select.value = currentValue;
        }

    } catch (error) {

        console.error(
            "Ошибка загрузки фильтра категорий:",
            error
        );

    }
}

const productSearch =
    document.getElementById("productSearch");

const productCategoryFilter =
    document.getElementById("productCategoryFilter");


if (productSearch) {

    productSearch.addEventListener(
        "input",
        renderProducts
    );

}


if (productCategoryFilter) {

    productCategoryFilter.addEventListener(
        "change",
        renderProducts
    );

}

/* ==========================================
   CRUD товаров
   ========================================== */

const productModal =
    document.getElementById("productModal");

const addProductButton =
    document.getElementById("addProductButton");

const closeProductModal =
    document.getElementById("closeProductModal");

const cancelProduct =
    document.getElementById("cancelProduct");

const productForm =
    document.getElementById("productForm");

const productModalTitle =
    document.getElementById("productModalTitle");

const saveProductButton =
    document.getElementById("saveProductButton");

let editingProductId = null;


/* ==========================================
   Загрузка категорий
   ========================================== */

async function loadProductCategories() {

    const select =
        document.getElementById("productCategory");

    if (!select) {
        return;
    }

    try {

        const response =
            await fetch("/api/products/categories");

        if (!response.ok) {
            throw new Error(
                "Не удалось загрузить категории."
            );
        }

        const categories =
            await response.json();

        select.innerHTML = `
            <option value="">
                Выберите категорию
            </option>

            ${categories.map(category => `
                <option value="${category.id}">
                    ${escapeHtml(category.name)}
                </option>
            `).join("")}
        `;

    } catch (error) {

        console.error(
            "Ошибка загрузки категорий:",
            error
        );

        select.innerHTML = `
            <option value="">
                Не удалось загрузить категории
            </option>
        `;

        throw error;
    }
}


/* ==========================================
   Открытие добавления товара
   ========================================== */

if (addProductButton) {

    addProductButton.addEventListener("click", async () => {

        try {

            editingProductId = null;

            productForm.reset();

            await loadProductCategories();

            productModalTitle.textContent =
                "Добавление товара";

            saveProductButton.textContent =
                "Сохранить";

            productModal.classList.remove("hidden");

        } catch (error) {

            showToast(
                "Не удалось открыть форму товара.",
                "error"
            );

        }

    });

}


/* ==========================================
   Редактирование товара
   ========================================== */

async function editProduct(productId) {

    try {

        const product =
            allProducts.find(
                item =>
                    Number(item.id) ===
                    Number(productId)
            );


        if (!product) {

            throw new Error(
                "Товар не найден."
            );

        }


        editingProductId =
            Number(productId);


        await loadProductCategories();


        document.getElementById(
            "productName"
        ).value =
            product.name ?? "";


        document.getElementById(
            "productAlcoholType"
        ).value =
            product.alcoholType ?? "";


        document.getElementById(
            "productStrength"
        ).value =
            product.strength ?? "";


        document.getElementById(
            "productVolume"
        ).value =
            product.volume ?? "";


        document.getElementById(
            "productPrice"
        ).value =
            product.price ?? "";


        document.getElementById(
            "productCategory"
        ).value =
            product.categoryId ?? "";


        productModalTitle.textContent =
            `Редактирование товара №${productId}`;


        saveProductButton.textContent =
            "Сохранить изменения";


        productModal.classList.remove(
            "hidden"
        );

    } catch (error) {

        console.error(
            "Ошибка открытия товара:",
            error
        );


        editingProductId = null;


        showToast(
            error.message ||
            "Не удалось открыть товар.",
            "error"
        );

    }
}


/* ==========================================
   Закрытие формы товара
   ========================================== */

function closeProductForm() {

    editingProductId = null;

    productForm.reset();

    productModal.classList.add("hidden");
}


if (closeProductModal) {

    closeProductModal.addEventListener(
        "click",
        closeProductForm
    );

}


if (cancelProduct) {

    cancelProduct.addEventListener(
        "click",
        closeProductForm
    );

}


if (productModal) {

    productModal.addEventListener("click", event => {

        if (event.target === productModal) {
            closeProductForm();
        }

    });

}


/* ==========================================
   Сохранение товара
   ========================================== */

if (productForm) {

    productForm.addEventListener(
        "submit",
        async event => {

            event.preventDefault();


            const name =
                document
                    .getElementById("productName")
                    .value
                    .trim();


            const alcoholType =
                document
                    .getElementById("productAlcoholType")
                    .value
                    .trim();


            const strengthValue =
                document
                    .getElementById("productStrength")
                    .value;


            const volumeValue =
                document
                    .getElementById("productVolume")
                    .value;


            const priceValue =
                document
                    .getElementById("productPrice")
                    .value;


            const categoryValue =
                document
                    .getElementById("productCategory")
                    .value;


            if (!name) {

                showToast(
                    "Введите название товара.",
                    "error"
                );

                return;
            }


            if (!alcoholType) {

                showToast(
                    "Введите тип алкоголя.",
                    "error"
                );

                return;
            }


            if (
                strengthValue === "" ||
                !Number.isFinite(Number(strengthValue)) ||
                Number(strengthValue) < 0
            ) {

                showToast(
                    "Укажите корректную крепость.",
                    "error"
                );

                return;
            }


            if (
                volumeValue === "" ||
                !Number.isFinite(Number(volumeValue)) ||
                Number(volumeValue) <= 0
            ) {

                showToast(
                    "Укажите корректный объём.",
                    "error"
                );

                return;
            }


            if (
                priceValue === "" ||
                !Number.isFinite(Number(priceValue)) ||
                Number(priceValue) < 0
            ) {

                showToast(
                    "Укажите корректную цену.",
                    "error"
                );

                return;
            }


            if (!categoryValue) {

                showToast(
                    "Выберите категорию.",
                    "error"
                );

                return;
            }


            const product = {

                name: name,

                alcoholType: alcoholType,

                strength: Number(strengthValue),

                volume: Number(volumeValue),

                price: Number(priceValue),

                categoryId: Number(categoryValue)

            };


            const isEditing =
                editingProductId !== null;


            const url = isEditing
                ? `/api/products/${editingProductId}`
                : "/api/products";


            const method = isEditing
                ? "PUT"
                : "POST";


            saveProductButton.disabled = true;

            saveProductButton.textContent =
                isEditing
                    ? "Сохранение..."
                    : "Добавление...";


            try {

                console.log(
                    "Отправка товара:",
                    product
                );


                const response =
                    await fetch(
                        url,
                        {
                            method: method,

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body:
                                JSON.stringify(product)
                        }
                    );


                const result =
                    await response
                        .json()
                        .catch(() => null);


                if (!response.ok) {

                    console.error(
                        "Ошибка API:",
                        result
                    );


                    throw new Error(
                        result?.error ||
                        result?.message ||
                        `Ошибка сервера: ${response.status}`
                    );

                }


                closeProductForm();


                showToast(
                    isEditing
                        ? "Товар успешно изменён."
                        : "Товар успешно добавлен.",
                    "success"
                );


                await loadProducts();

            } catch (error) {

                console.error(
                    "Ошибка сохранения товара:",
                    error
                );


                showToast(
                    error.message ||
                    "Не удалось сохранить товар.",
                    "error"
                );

            } finally {

                saveProductButton.disabled = false;

                saveProductButton.textContent =
                    isEditing
                        ? "Сохранить изменения"
                        : "Сохранить";

            }

        }
    );

}

/* ==========================================
   Удаление товара
   ========================================== */

const deleteProductModal =
    document.getElementById("deleteProductModal");

const closeDeleteProductModal =
    document.getElementById("closeDeleteProductModal");

const cancelDeleteProduct =
    document.getElementById("cancelDeleteProduct");

const confirmDeleteProduct =
    document.getElementById("confirmDeleteProduct");

const deleteProductName =
    document.getElementById("deleteProductName");

let productToDeleteId = null;


function openDeleteProductModal(productId, productName) {

    productToDeleteId =
        Number(productId);

    if (deleteProductName) {

        deleteProductName.textContent =
            productName;
    }

    if (deleteProductModal) {

        deleteProductModal.classList.remove(
            "hidden"
        );

    }
}


function closeDeleteProductConfirmation() {

    productToDeleteId = null;

    if (deleteProductModal) {

        deleteProductModal.classList.add(
            "hidden"
        );

    }
}


if (closeDeleteProductModal) {

    closeDeleteProductModal.addEventListener(
        "click",
        closeDeleteProductConfirmation
    );

}


if (cancelDeleteProduct) {

    cancelDeleteProduct.addEventListener(
        "click",
        closeDeleteProductConfirmation
    );

}


if (deleteProductModal) {

    deleteProductModal.addEventListener(
        "click",
        event => {

            if (event.target === deleteProductModal) {

                closeDeleteProductConfirmation();

            }

        }
    );

}


if (confirmDeleteProduct) {

    confirmDeleteProduct.addEventListener(
        "click",
        async () => {

            if (!productToDeleteId) {
                return;
            }

            const productId =
                productToDeleteId;


            confirmDeleteProduct.disabled =
                true;

            confirmDeleteProduct.textContent =
                "Удаление...";


            try {

                const response =
                    await fetch(
                        `/api/products/${productId}`,
                        {
                            method: "DELETE"
                        }
                    );


                const result =
                    await response
                        .json()
                        .catch(() => null);


                if (!response.ok) {

                    throw new Error(
                        result?.error ||
                        result?.message ||
                        "Не удалось удалить товар."
                    );

                }


                closeDeleteProductConfirmation();

                showToast(
                    "Товар успешно удалён.",
                    "success"
                );

                await loadProducts();

            } catch (error) {

                console.error(
                    "Ошибка удаления товара:",
                    error
                );

                closeDeleteProductConfirmation();

                showToast(
                    error.message ||
                    "Не удалось удалить товар.",
                    "error"
                );

            } finally {

                confirmDeleteProduct.disabled =
                    false;

                confirmDeleteProduct.textContent =
                    "Удалить";

            }

        }
    );

}


// ==========================================
// Переключение страниц
// ==========================================

const menuItems = document.querySelectorAll(".menu-item");

menuItems.forEach(item => {

    item.addEventListener("click", () => {

        const page = item.dataset.page;

        menuItems.forEach(menuItem => {
            menuItem.classList.remove("active");
        });

        item.classList.add("active");

        document.querySelectorAll(".page").forEach(pageElement => {
            pageElement.classList.add("hidden");
        });

        const selectedPage =
            document.getElementById(page + "Page");

        if (selectedPage) {
            selectedPage.classList.remove("hidden");
        }

        updateHeader(page);

        if (page === "products") {
            loadProducts();
        }

        if (page === "clients") {
            loadClients();
        }

        if (page === "orders") {
            loadOrders();
        }

        if (page === "employees") {
            loadEmployees();
        }

        if (page === "stock") {
            loadStock();
        }
    });
});


// ==========================================
// Заголовок страницы
// ==========================================

function updateHeader(page) {

    const title =
        document.getElementById("pageTitle");

    const description =
        document.getElementById("pageDescription");

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
    else if (page === "employees") {

        title.textContent = "Сотрудники";

        description.textContent =
            "Управление сотрудниками компании";

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

const refreshProducts =
    document.getElementById("refreshProducts");

if (refreshProducts) {

    refreshProducts.addEventListener(
        "click",
        loadProducts
    );
}


// ==========================================
// Клиенты
// ==========================================

async function loadClients() {

    const table =
        document.getElementById("clientsTable");

    if (!table) {
        return;
    }

    table.innerHTML = `
        <tr>
            <td colspan="8">Загрузка данных...</td>
        </tr>
    `;

    try {

        const response =
            await fetch("/api/clients");

        if (!response.ok) {

            throw new Error(
                "Ошибка получения клиентов"
            );
        }

        const clients =
            await response.json();
        console.debug("loadClients: received clients", clients && clients.length);

        table.innerHTML = "";

        clients.forEach(client => {

            const row =
                document.createElement("tr");

            row.innerHTML = `
                <td>${client.id}</td>
                <td>${escapeHtml(client.name)}</td>
                <td>${escapeHtml(client.type)}</td>
                <td>${escapeHtml(client.inn ?? "-")}</td>
                <td>${escapeHtml(client.phone)}</td>
                <td>${escapeHtml(client.email ?? "-")}</td>
                <td>${escapeHtml(client.deliveryAddress)}</td>
                <td class="table-actions">

                    <button
                        type="button"
                        class="action-button edit-button"
                        onclick="editClient(${client.id})"
                        title="Редактировать">
                        ✏️
                    </button>

                    <button
                        type="button"
                        class="action-button delete-button"
                        onclick="openDeleteClientModal(${client.id}, '${escapeHtml(client.name)}')"
                        title="Удалить">
                        🗑️
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
                <td colspan="8">
                    Не удалось загрузить клиентов.
                </td>
            </tr>
        `;
    }
}


// ==========================================
// Кнопка обновления клиентов
// ==========================================

const refreshClients =
    document.getElementById("refreshClients");

if (refreshClients) {

    refreshClients.addEventListener(
        "click",
        loadClients
    );
}


// ==========================================
// Сотрудники
// ==========================================

async function loadEmployees() {

    console.log("Загрузка сотрудников...");

    const table =
        document.getElementById("employeesTable");

    if (!table) {

        console.error(
            "Элемент #employeesTable не найден!"
        );

        return;
    }

    table.innerHTML = `
        <tr>
            <td colspan="7">
                Загрузка данных...
            </td>
        </tr>
    `;

    try {

        const response =
            await fetch("/api/employees");

        console.log(
            "Ответ /api/employees:",
            response.status
        );

        if (!response.ok) {

            throw new Error(
                `Ошибка HTTP: ${response.status}`
            );
        }

        const employees =
            await response.json();

        console.log(
            "Получены сотрудники:",
            employees
        );

        table.innerHTML = "";

        if (
            !Array.isArray(employees) ||
            employees.length === 0
        ) {

            table.innerHTML = `
                <tr>
                    <td colspan="7">
                        Сотрудников пока нет.
                    </td>
                </tr>
            `;

            return;
        }

        employees.forEach(employee => {

            const row =
                document.createElement("tr");

            const hireDate =
                employee.hireDate
                    ? new Date(employee.hireDate)
                        .toLocaleDateString("ru-RU")
                    : "—";

            const salary =
                Number(employee.salary || 0)
                    .toLocaleString(
                        "ru-RU",
                        {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        }
                    );

            row.innerHTML = `
                <td>${employee.id}</td>

                <td>${employee.name}</td>

                <td>${employee.positionName}</td>

                <td>${hireDate}</td>

                <td>${salary} ₽</td>

                <td>${employee.departmentName}</td>

                <td>
                    <button
                        type="button"
                        onclick="editEmployee(${employee.id})">
                        ✏️
                    </button>

                    <button
                        type="button"
                        onclick="deleteEmployee(${employee.id})">
                        🗑️
                    </button>
                </td>
            `;

            table.appendChild(row);
        });

        console.log(
            `Отображено сотрудников: ${employees.length}`
        );

    }
    catch (error) {

        console.error(
            "Ошибка загрузки сотрудников:",
            error
        );

        table.innerHTML = `
            <tr>
                <td colspan="7">
                    Не удалось загрузить сотрудников.
                </td>
            </tr>
        `;
    }
}


// ==========================================
// Изменение сотрудника
// ==========================================

async function editEmployee(id) {

    try {

        const response =
            await fetch("/api/employees");

        if (!response.ok) {

            throw new Error(
                "Не удалось загрузить сотрудников."
            );
        }

        const employees =
            await response.json();

        const employee =
            employees.find(
                item => item.id === id
            );

        if (!employee) {

            throw new Error(
                "Сотрудник не найден."
            );
        }

        await loadEmployeeFormData();

        document.getElementById(
            "employeeId"
        ).value = employee.id;

        document.getElementById(
            "employeeName"
        ).value = employee.name;

        document.getElementById(
            "employeePosition"
        ).value = employee.positionId;

        document.getElementById(
            "employeeHireDate"
        ).value =
            employee.hireDate.substring(0, 10);

        document.getElementById(
            "employeeSalary"
        ).value = employee.salary;

        document.getElementById(
            "employeeDepartment"
        ).value = employee.departmentId;

        employeeModalTitle.textContent =
            "Редактирование сотрудника";

        employeeModal.classList.remove(
            "hidden"
        );

    }
    catch (error) {

        console.error(
            "Ошибка редактирования сотрудника:",
            error
        );

        alert(
            "Не удалось открыть сотрудника для редактирования.\n\n" +
            error.message
        );
    }
}


// ==========================================
// Удаление сотрудника
// ==========================================

async function deleteEmployee(id) {

    const confirmed =
        confirm(
            "Вы действительно хотите удалить этого сотрудника?"
        );

    if (!confirmed) {
        return;
    }

    try {

        const response =
            await fetch(
                `/api/employees/${id}`,
                {
                    method: "DELETE"
                }
            );

        const result =
            await response.json();

        if (!response.ok) {

            throw new Error(
                result.error ||
                result.message ||
                "Не удалось удалить сотрудника."
            );
        }

        alert(
            "Сотрудник успешно удалён."
        );

        await loadEmployees();

    }
    catch (error) {

        console.error(
            "Ошибка удаления сотрудника:",
            error
        );

        alert(
            "Не удалось удалить сотрудника.\n\n" +
            error.message
        );
    }
}


// ==========================================
// Элементы формы сотрудника
// ==========================================

const employeeModal =
    document.getElementById("employeeModal");

const employeeForm =
    document.getElementById("employeeForm");

const addEmployeeButton =
    document.getElementById("addEmployeeButton");

const closeEmployeeModal =
    document.getElementById("closeEmployeeModal");

const cancelEmployee =
    document.getElementById("cancelEmployee");

const refreshEmployees =
    document.getElementById("refreshEmployees");

const employeeModalTitle =
    document.getElementById("employeeModalTitle");


// ==========================================
// Обновление сотрудников
// ==========================================

if (refreshEmployees) {

    refreshEmployees.addEventListener(
        "click",
        loadEmployees
    );
}


// ==========================================
// Загрузка должностей и отделов
// ==========================================

async function loadEmployeeFormData() {

    const positionSelect =
        document.getElementById(
            "employeePosition"
        );

    const departmentSelect =
        document.getElementById(
            "employeeDepartment"
        );


    // ---------- Должности ----------

    const positionsResponse =
        await fetch(
            "/api/employees/positions"
        );

    if (!positionsResponse.ok) {

        throw new Error(
            "Не удалось загрузить должности."
        );
    }

    const positions =
        await positionsResponse.json();

    positionSelect.innerHTML =
        positions
            .map(position => `
                <option value="${position.id}">
                    ${position.name}
                </option>
            `)
            .join("");


    // ---------- Отделы ----------

    const departmentsResponse =
        await fetch(
            "/api/employees/departments"
        );

    if (!departmentsResponse.ok) {

        throw new Error(
            "Не удалось загрузить отделы."
        );
    }

    const departments =
        await departmentsResponse.json();

    departmentSelect.innerHTML =
        departments
            .map(department => `
                <option value="${department.id}">
                    ${department.name}
                </option>
            `)
            .join("");
}


// ==========================================
// Открытие формы нового сотрудника
// ==========================================

if (addEmployeeButton) {

    addEmployeeButton.addEventListener(
        "click",
        async () => {

            try {

                await loadEmployeeFormData();

                employeeForm.reset();

                document.getElementById(
                    "employeeId"
                ).value = "";

                employeeModalTitle.textContent =
                    "Новый сотрудник";

                employeeModal.classList.remove(
                    "hidden"
                );

            }
            catch (error) {

                console.error(
                    "Ошибка открытия формы сотрудника:",
                    error
                );

                alert(
                    "Не удалось загрузить данные для формы сотрудника.\n\n" +
                    error.message
                );
            }
        }
    );
}


// ==========================================
// Закрытие формы сотрудника
// ==========================================

function closeEmployeeModalWindow() {

    employeeModal.classList.add(
        "hidden"
    );
}

if (closeEmployeeModal) {

    closeEmployeeModal.addEventListener(
        "click",
        closeEmployeeModalWindow
    );
}

if (cancelEmployee) {

    cancelEmployee.addEventListener(
        "click",
        closeEmployeeModalWindow
    );
}


// ==========================================
// Сохранение сотрудника
// ==========================================

if (employeeForm) {

    employeeForm.addEventListener(
        "submit",
        async (event) => {

            event.preventDefault();

            event.stopPropagation();

            const id =
                document.getElementById(
                    "employeeId"
                ).value;

            const name =
                document.getElementById(
                    "employeeName"
                ).value.trim();

            const positionId =
                parseInt(
                    document.getElementById(
                        "employeePosition"
                    ).value,
                    10
                );

            const hireDate =
                document.getElementById(
                    "employeeHireDate"
                ).value;

            const salary =
                parseFloat(
                    document.getElementById(
                        "employeeSalary"
                    ).value
                );

            const departmentId =
                parseInt(
                    document.getElementById(
                        "employeeDepartment"
                    ).value,
                    10
                );


            if (!name) {

                alert(
                    "Введите ФИО сотрудника."
                );

                return;
            }

            if (!positionId) {

                alert(
                    "Выберите должность."
                );

                return;
            }

            if (!hireDate) {

                alert(
                    "Укажите дату найма."
                );

                return;
            }

            if (
                Number.isNaN(salary) ||
                salary < 0
            ) {

                alert(
                    "Введите корректную зарплату."
                );

                return;
            }

            if (!departmentId) {

                alert(
                    "Выберите отдел."
                );

                return;
            }


            const employee = {

                name: name,

                positionId: positionId,

                hireDate: hireDate,

                salary: salary,

                departmentId: departmentId
            };


            try {

                const url =
                    id
                        ? `/api/employees/${id}`
                        : "/api/employees";

                const method =
                    id
                        ? "PUT"
                        : "POST";


                const response =
                    await fetch(
                        url,
                        {
                            method: method,

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body:
                                JSON.stringify(
                                    employee
                                )
                        }
                    );


                const result =
                    await response.json();


                if (!response.ok) {

                    throw new Error(
                        result.error ||
                        result.message ||
                        "Ошибка сохранения сотрудника."
                    );
                }


                alert(
                    id
                        ? "Сотрудник успешно изменён!"
                        : "Сотрудник успешно добавлен!"
                );


                employeeModal.classList.add(
                    "hidden"
                );

                employeeForm.reset();

                document.getElementById(
                    "employeeId"
                ).value = "";

                await loadEmployees();

            }
            catch (error) {

                console.error(
                    "Ошибка сохранения сотрудника:",
                    error
                );

                alert(
                    "Не удалось сохранить сотрудника.\n\n" +
                    error.message
                );
            }
        }
    );
}


// ==========================================
// Начальная загрузка
// ==========================================

loadProducts();


// ==========================================
// Заказы
// ==========================================

async function loadOrders() {

    const table =
        document.getElementById(
            "ordersTable"
        );

    if (!table) {
        return;
    }

    table.innerHTML = `
        <tr>
            <td colspan="7">
                Загрузка данных...
            </td>
        </tr>
    `;

    try {

        const response =
            await fetch("/api/orders");

        if (!response.ok) {

            throw new Error(
                "Ошибка получения заказов"
            );
        }

        const orders =
            await response.json();

        table.innerHTML = "";

        if (
            !Array.isArray(orders) ||
            orders.length === 0
        ) {

            table.innerHTML = `
                <tr>
                    <td colspan="7">
                        Заказов пока нет.
                    </td>
                </tr>
            `;

            return;
        }

        orders.forEach(order => {

            const row =
                document.createElement("tr");

            const date =
                new Date(order.orderDate)
                    .toLocaleDateString(
                        "ru-RU"
                    );

            row.innerHTML = `
                <td>${order.id}</td>

                <td>${date}</td>

                <td>${order.clientName}</td>

                <td>${order.employeeName}</td>

                <td>${order.status}</td>

                <td>
                    ${Number(order.total).toFixed(2)} ₽
                </td>

                <td>

                    <button
                        class="details-button"
                        type="button"
                        onclick="showOrderDetails(${order.id})">
                        Подробнее
                    </button>

                    <button
                        class="details-button"
                        type="button"
                        onclick="editOrder(${order.id})">
                        ✏️
                    </button>

                    <button
                        type="button"
                        class="action-button delete-button"
                        onclick="openDeleteOrderModal(${order.id})">
                        🗑️
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

//Удаление заказа
let orderToDeleteId = null;


/* ==========================================
   Удаление заказа
   ========================================== */

const deleteOrderModal = document.getElementById("deleteOrderModal");
const closeDeleteOrderModal =
    document.getElementById("closeDeleteOrderModal");
const cancelDeleteOrder =
    document.getElementById("cancelDeleteOrder");
const confirmDeleteOrder =
    document.getElementById("confirmDeleteOrder");
const deleteOrderNumber =
    document.getElementById("deleteOrderNumber");


function openDeleteOrderModal(orderId) {

    orderToDeleteId = Number(orderId);

    if (deleteOrderNumber) {
        deleteOrderNumber.textContent = `№${orderId}`;
    }

    if (deleteOrderModal) {
        deleteOrderModal.classList.remove("hidden");
    }
}


function closeDeleteOrderConfirmation() {

    orderToDeleteId = null;

    if (deleteOrderModal) {
        deleteOrderModal.classList.add("hidden");
    }
}


if (closeDeleteOrderModal) {
    closeDeleteOrderModal.addEventListener("click", () => {
        closeDeleteOrderConfirmation();
    });
}


if (cancelDeleteOrder) {
    cancelDeleteOrder.addEventListener("click", () => {
        closeDeleteOrderConfirmation();
    });
}


if (deleteOrderModal) {
    deleteOrderModal.addEventListener("click", event => {

        if (event.target === deleteOrderModal) {
            closeDeleteOrderConfirmation();
        }

    });
}


if (confirmDeleteOrder) {
    confirmDeleteOrder.addEventListener("click", async () => {

        if (!orderToDeleteId) {
            return;
        }

        const orderId = orderToDeleteId;

        confirmDeleteOrder.disabled = true;
        confirmDeleteOrder.textContent = "Удаление...";

        try {

            const response = await fetch(`/api/orders/${orderId}`, {
                method: "DELETE"
            });

            const result = await response.json().catch(() => null);

            if (!response.ok) {

                throw new Error(
                    result?.error ||
                    result?.message ||
                    "Не удалось удалить заказ."
                );

            }

            closeDeleteOrderConfirmation();

            showToast(
                `Заказ №${orderId} успешно удалён.`,
                "success"
            );

            await loadOrders();

        } catch (error) {

            console.error("Ошибка удаления заказа:", error);

            closeDeleteOrderConfirmation();

            showToast(
                error.message || "Не удалось удалить заказ.",
                "error"
            );

        } finally {

            confirmDeleteOrder.disabled = false;
            confirmDeleteOrder.textContent = "Удалить";

        }

    });
}

/* ==========================================
   Уведомления
   ========================================== */

let toastTimeout = null;


function showToast(message, type = "success") {

    const toast = document.getElementById("toast");
    const toastIcon = document.getElementById("toastIcon");
    const toastMessage = document.getElementById("toastMessage");

    if (!toast || !toastIcon || !toastMessage) {
        return;
    }

    if (toastTimeout) {
        clearTimeout(toastTimeout);
    }

    toast.classList.remove("hidden");
    toast.classList.remove("success", "error");

    toast.classList.add(type);

    if (type === "success") {
        toastIcon.textContent = "✓";
    } else {
        toastIcon.textContent = "✕";
    }

    toastMessage.textContent = message;

    toastTimeout = setTimeout(() => {
        toast.classList.add("hidden");
    }, 3500);
}


// ==========================================
// Обновление заказов
// ==========================================

const refreshOrders =
    document.getElementById("refreshOrders");

if (refreshOrders) {

    refreshOrders.addEventListener(
        "click",
        loadOrders
    );
}


// ==========================================
// Детали заказа
// ==========================================

async function showOrderDetails(orderId) {

    const modal =
        document.getElementById(
            "orderModal"
        );

    const content =
        document.getElementById(
            "orderDetailsContent"
        );

    modal.classList.remove(
        "hidden"
    );

    content.innerHTML = `
        <p>Загрузка данных...</p>
    `;

    try {

        const response =
            await fetch(
                `/api/orders/${orderId}/details`
            );

        if (!response.ok) {

            throw new Error(
                "Ошибка получения деталей заказа"
            );
        }

        const details =
            await response.json();

        if (details.length === 0) {

            content.innerHTML = `
                <p>
                    В заказе нет товаров.
                </p>
            `;

            return;
        }

        let total = 0;

        let rows = "";

        details.forEach(detail => {

            total += Number(detail.sum);

            rows += `
                <tr>
                    <td>${detail.product}</td>
                    <td>${detail.quantity}</td>
                    <td>${Number(detail.price).toFixed(2)} ₽</td>
                    <td>${Number(detail.sum).toFixed(2)} ₽</td>
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


const closeOrderModal =
    document.getElementById(
        "closeOrderModal"
    );

if (closeOrderModal) {

    closeOrderModal.addEventListener(
        "click",
        () => {

            document
                .getElementById("orderModal")
                .classList.add(
                    "hidden"
                );
        }
    );
}


const orderModal =
    document.getElementById(
        "orderModal"
    );

if (orderModal) {

    orderModal.addEventListener(
        "click",
        (event) => {

            if (
                event.target.id ===
                "orderModal"
            ) {

                event.currentTarget
                    .classList.add(
                        "hidden"
                    );
            }
        }
    );
}


// ==========================================
// Склад
// ==========================================

async function loadStock() {

    const table =
        document.getElementById(
            "stockTable"
        );

    const warningBlock =
        document.getElementById(
            "lowStockBlock"
        );

    if (!table) {
        return;
    }

    table.innerHTML = `
        <tr>
            <td colspan="5">
                Загрузка данных...
            </td>
        </tr>
    `;

    try {

        const response =
            await fetch("/api/stock");

        if (!response.ok) {

            throw new Error(
                "Ошибка получения складских остатков"
            );
        }

        const stock =
            await response.json();

        table.innerHTML = "";

        if (
            !Array.isArray(stock) ||
            stock.length === 0
        ) {

            table.innerHTML = `
                <tr>
                    <td colspan="5">
                        Складских остатков пока нет.
                    </td>
                </tr>
            `;

            await loadLowStock();

            return;
        }

        stock.forEach(item => {

            const row =
                document.createElement("tr");

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
        document.getElementById(
            "lowStockBlock"
        );

    const warningContent =
        document.getElementById(
            "lowStockContent"
        );

    if (!warningBlock || !warningContent) {
        return;
    }

    try {

        const response =
            await fetch(
                "/api/stock/low"
            );

        if (!response.ok) {

            throw new Error(
                "Ошибка получения товаров для пополнения"
            );
        }

        const items =
            await response.json();

        if (items.length === 0) {

            warningBlock.classList.add(
                "hidden"
            );

            return;
        }

        warningBlock.classList.remove(
            "hidden"
        );

        warningContent.innerHTML = "";

        items.forEach(item => {

            const element =
                document.createElement(
                    "div"
                );

            element.className =
                "warning-item";

            element.textContent =
                `${item.warehouse}: ${item.product} — ` +
                `остаток ${item.currentQuantity}, ` +
                `минимум ${item.minimumQuantity}`;

            warningContent.appendChild(
                element
            );
        });

    }
    catch (error) {

        console.error(error);

        warningBlock.classList.add(
            "hidden"
        );
    }
}


// ==========================================
// Добавление клиента
// ==========================================

const clientModal =
    document.getElementById(
        "clientModal"
    );

const addClientButton =
    document.getElementById(
        "addClientButton"
    );

const closeClientModal =
    document.getElementById(
        "closeClientModal"
    );

const cancelClient =
    document.getElementById(
        "cancelClient"
    );

const clientForm =
    document.getElementById(
        "clientForm"
    );

let editingClientId = null;


if (addClientButton) {

    addClientButton.addEventListener(
        "click",
        () => {

            // prepare form for creating new client
            editingClientId = null;
            clientForm.reset();

            const submitButton = clientForm ? clientForm.querySelector('button[type="submit"]') : null;
            if (submitButton) {
                submitButton.textContent = "Сохранить";
                submitButton.disabled = false;
            }

            // set modal title if present
            const titleEl = document.querySelector('#clientModal .modal-header h2');
            if (titleEl) titleEl.textContent = 'Добавление клиента';

            clientModal.classList.remove("hidden");
        }
    );
}


if (closeClientModal) {

    closeClientModal.addEventListener(
        "click",
        () => {

            clientModal.classList.add(
                "hidden"
            );
        }
    );
}


if (cancelClient) {

    cancelClient.addEventListener(
        "click",
        () => {

            clientModal.classList.add(
                "hidden"
            );
        }
    );
}


if (clientForm) {

    clientForm.addEventListener(
        "submit",
        async (event) => {

            event.preventDefault();

            const client = {

                name:
                    document.getElementById(
                        "clientName"
                    ).value,

                type:
                    document.getElementById(
                        "clientType"
                    ).value,

                inn:
                    document.getElementById(
                        "clientInn"
                    ).value,

                phone:
                    document.getElementById(
                        "clientPhone"
                    ).value,

                email:
                    document.getElementById(
                        "clientEmail"
                    ).value,

                address:
                    document.getElementById(
                        "clientAddress"
                    ).value
            };


            try {

                const submitButton = clientForm.querySelector('button[type="submit"]');

                const method = editingClientId ? "PUT" : "POST";
                const url = editingClientId ? `/api/clients/${editingClientId}` : "/api/clients";

                if (submitButton) {
                    submitButton.disabled = true;
                    submitButton.textContent = editingClientId ? "Сохранение..." : "Сохранение...";
                }

                const response = await fetch(url, {
                    method,
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(client)
                });

                const result = await response.json().catch(() => null);

                if (!response.ok) {
                    throw new Error(result?.error || result?.message || (editingClientId ? "Не удалось обновить клиента" : "Не удалось добавить клиента"));
                }

                alert(editingClientId ? "Клиент успешно обновлён." : "Клиент успешно добавлен.");

                clientForm.reset();

                clientModal.classList.add("hidden");

                editingClientId = null;

                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.textContent = "Сохранить";
                }

                await loadClients();

            } catch (error) {

                console.error(error);

                alert("Ошибка при сохранении клиента:\n" + (error.message || error));

                const submitButton = clientForm.querySelector('button[type="submit"]');
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.textContent = "Сохранить";
                }
            }
        }
    );
}


// ==========================================
// Создание / редактирование заказа
// ==========================================

const orderModalCreate =
    document.getElementById(
        "orderModalCreate"
    );

const addOrderButton =
    document.getElementById(
        "addOrderButton"
    );

const closeOrderModalCreate =
    document.getElementById(
        "closeOrderModalCreate"
    );

const cancelOrder =
    document.getElementById(
        "cancelOrder"
    );

const orderForm =
    document.getElementById(
        "orderForm"
    );


// ==========================================
// Режим заказа
// ==========================================

// null = создание нового заказа
// число = редактирование существующего заказа
let editingOrderId = null;


// ==========================================
// Массив позиций текущего заказа
// ==========================================

let orderItems = [];


// ==========================================
// Элемент добавления позиции
// ==========================================

const addOrderItemButton =
    document.getElementById(
        "addOrderItemButton"
    );


// ==========================================
// Отображение позиций заказа
// ==========================================

function renderOrderItems() {

    const list =
        document.getElementById(
            "orderItemsList"
        );

    const totalElement =
        document.getElementById(
            "orderTotalPreview"
        );

    if (!list || !totalElement) {
        return;
    }


    if (orderItems.length === 0) {

        list.innerHTML = `
            <div class="empty-order-items">
                Товары пока не добавлены
            </div>
        `;

        totalElement.textContent =
            "0 ₽";

        return;
    }


    let total = 0;


    list.innerHTML =
        orderItems
            .map((item, index) => {

                const sum =
                    Number(item.price) *
                    Number(item.quantity);

                total += sum;


                return `
                    <div class="order-item-row">

                        <div class="order-item-name">
                            ${item.name}
                        </div>

                        <div class="order-item-quantity">
                            ${item.quantity} шт.
                        </div>

                        <div class="order-item-price">
                            ${sum.toLocaleString(
                                "ru-RU",
                                {
                                    minimumFractionDigits: 2,
                                    maximumFractionDigits: 2
                                }
                            )} ₽
                        </div>

                        <button
                            type="button"
                            class="remove-order-item"
                            onclick="removeOrderItem(${index})"
                            title="Удалить товар">
                            ×
                        </button>

                    </div>
                `;
            })
            .join("");


    totalElement.textContent =
        `${total.toLocaleString(
            "ru-RU",
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        )} ₽`;
}


// ==========================================
// Удаление позиции
// ==========================================

function removeOrderItem(index) {

    if (
        index < 0 ||
        index >= orderItems.length
    ) {
        return;
    }

    orderItems.splice(
        index,
        1
    );

    renderOrderItems();
}


// ==========================================
// Добавление позиции
// ==========================================

if (addOrderItemButton) {

    addOrderItemButton.addEventListener(
        "click",
        () => {

            const productSelect =
                document.getElementById(
                    "orderProductSelect"
                );

            const quantityInput =
                document.getElementById(
                    "orderQuantity"
                );


            const productId =
                parseInt(
                    productSelect.value,
                    10
                );

            const quantity =
                parseInt(
                    quantityInput.value,
                    10
                );


            if (!productId) {

                alert(
                    "Выберите товар."
                );

                return;
            }


            if (
                Number.isNaN(quantity) ||
                quantity <= 0
            ) {

                alert(
                    "Введите корректное количество."
                );

                return;
            }


            const selectedOption =
                productSelect.options[
                    productSelect.selectedIndex
                ];


            const productName =
                selectedOption.dataset.name ||
                selectedOption.textContent.trim();


            const productPrice =
                parseFloat(
                    selectedOption.dataset.price
                );


            if (Number.isNaN(productPrice)) {

                alert(
                    "Не удалось определить цену товара."
                );

                return;
            }


            const existingItem =
                orderItems.find(
                    item =>
                        item.productId === productId
                );


            if (existingItem) {

                existingItem.quantity += quantity;

            }
            else {

                orderItems.push({

                    productId:
                        productId,

                    name:
                        productName,

                    price:
                        productPrice,

                    quantity:
                        quantity
                });
            }


            renderOrderItems();

            quantityInput.value = 1;

            productSelect.value = "";
        }
    );
}


// ==========================================
// Открытие окна создания заказа
// ==========================================

if (addOrderButton) {

    addOrderButton.addEventListener(
        "click",
        async () => {

            console.log(
                "Нажата кнопка Создать заказ"
            );


            // Режим создания
            editingOrderId = null;

            orderItems = [];

            renderOrderItems();


            // Заголовок
            const title =
                orderModalCreate.querySelector(
                    ".modal-header h2"
                );

            if (title) {
                title.textContent =
                    "Новый заказ";
            }


            // Кнопка сохранения
            const submitButton =
                orderForm.querySelector(
                    'button[type="submit"]'
                );

            if (submitButton) {
                submitButton.textContent =
                    "Сохранить";
            }


            try {

                await loadOrderFormData();

                orderModalCreate.classList.remove(
                    "hidden"
                );

            }
            catch (error) {

                console.error(
                    "Не удалось открыть форму заказа:",
                    error
                );

                alert(
                    "Не удалось загрузить данные для создания заказа.\n\n" +
                    error.message
                );
            }
        }
    );
}


// ==========================================
// Редактирование существующего заказа
// ==========================================

async function editOrder(orderId) {

    try {

        console.log(
            "Открытие заказа для редактирования:",
            orderId
        );


        editingOrderId =
            Number(orderId);


        // Загружаем списки клиентов,
        // товаров и сотрудников
        await loadOrderFormData();


        // Загружаем все заказы
        const ordersResponse =
            await fetch(
                "/api/orders"
            );


        if (!ordersResponse.ok) {

            throw new Error(
                "Не удалось загрузить список заказов."
            );
        }


        const orders =
            await ordersResponse.json();


        const order =
            orders.find(
                item =>
                    Number(item.id) ===
                    Number(orderId)
            );


        if (!order) {

            throw new Error(
                "Заказ не найден."
            );
        }


        // ==========================================
        // Клиент
        // ==========================================

        const clientSelect =
            document.getElementById(
                "orderClientSelect"
            );


        const clientsResponse =
            await fetch(
                "/api/clients"
            );


        const clients =
            await clientsResponse.json();


        const client =
            clients.find(
                item =>
                    item.name ===
                    order.clientName
            );


        if (client) {

            clientSelect.value =
                client.id;

        }
        else {

            console.warn(
                "Не удалось определить клиента:",
                order.clientName
            );
        }


        // ==========================================
        // Сотрудник
        // ==========================================

        const employeeSelect =
            document.getElementById(
                "orderEmployeeSelect"
            );


        const employeesResponse =
            await fetch(
                "/api/employees"
            );


        const employees =
            await employeesResponse.json();


        const employee =
            employees.find(
                item =>
                    item.name ===
                    order.employeeName
            );


        if (employee) {

            employeeSelect.value =
                employee.id;

        }
        else {

            console.warn(
                "Не удалось определить сотрудника:",
                order.employeeName
            );
        }


        // ==========================================
        // Детали заказа
        // ==========================================

        const detailsResponse =
            await fetch(
                `/api/orders/${orderId}/details`
            );


        if (!detailsResponse.ok) {

            throw new Error(
                "Не удалось загрузить товары заказа."
            );
        }


        const details =
            await detailsResponse.json();


        orderItems = [];


        details.forEach(detail => {

            const productSelect =
                document.getElementById(
                    "orderProductSelect"
                );


            // Ищем товар по названию
            const option =
                Array.from(
                    productSelect.options
                ).find(
                    item =>
                        item.dataset.name ===
                        detail.product
                );


            if (!option) {

                console.warn(
                    "Товар не найден:",
                    detail.product
                );

                return;
            }


            orderItems.push({

                productId:
                    Number(option.value),

                name:
                    option.dataset.name,

                price:
                    Number(option.dataset.price),

                quantity:
                    Number(detail.quantity)

            });

        });


        renderOrderItems();


        // ==========================================
        // Меняем заголовок
        // ==========================================

        const title =
            orderModalCreate.querySelector(
                ".modal-header h2"
            );


        if (title) {

            title.textContent =
                `Редактирование заказа №${orderId}`;
        }


        // ==========================================
        // Меняем кнопку
        // ==========================================

        const submitButton =
            orderForm.querySelector(
                'button[type="submit"]'
            );


        if (submitButton) {

            submitButton.textContent =
                "Сохранить изменения";
        }


        // ==========================================
        // Открываем окно
        // ==========================================

        orderModalCreate.classList.remove(
            "hidden"
        );

    }
    catch (error) {

        console.error(
            "Ошибка открытия заказа:",
            error
        );

        editingOrderId = null;

        alert(
            "Не удалось открыть заказ для редактирования.\n\n" +
            error.message
        );
    }
}


// ==========================================
// Закрытие окна создания / редактирования
// ==========================================

function closeOrderCreateModal() {

    editingOrderId = null;

    orderItems = [];

    renderOrderItems();


    if (orderForm) {
        orderForm.reset();
    }


    const title =
        orderModalCreate.querySelector(
            ".modal-header h2"
        );


    if (title) {

        title.textContent =
            "Новый заказ";
    }


    const submitButton =
        orderForm.querySelector(
            'button[type="submit"]'
        );


    if (submitButton) {

        submitButton.textContent =
            "Сохранить";
    }


    orderModalCreate.classList.add(
        "hidden"
    );
}


if (closeOrderModalCreate) {

    closeOrderModalCreate.addEventListener(
        "click",
        closeOrderCreateModal
    );
}


// ==========================================
// Отмена
// ==========================================

if (cancelOrder) {

    cancelOrder.addEventListener(
        "click",
        closeOrderCreateModal
    );
}


// ==========================================
// Загрузка данных формы заказа
// ==========================================

async function loadOrderFormData() {

    const clientSelect =
        document.getElementById(
            "orderClientSelect"
        );

    const productSelect =
        document.getElementById(
            "orderProductSelect"
        );

    const employeeSelect =
        document.getElementById(
            "orderEmployeeSelect"
        );


    // ==========================================
    // Клиенты
    // ==========================================

    const clientsRes =
        await fetch(
            "/api/clients"
        );

    if (!clientsRes.ok) {

        throw new Error(
            "Не удалось загрузить клиентов"
        );
    }

    const clients =
        await clientsRes.json();

    if (clients.length === 0) {

        throw new Error(
            "В базе данных нет клиентов"
        );
    }


    clientSelect.innerHTML =
        clients
            .map(client => `
                <option value="${client.id}">
                    ${client.name}
                </option>
            `)
            .join("");


    // ==========================================
    // Товары
    // ==========================================

    const productsRes =
        await fetch(
            "/api/products"
        );

    if (!productsRes.ok) {

        throw new Error(
            "Не удалось загрузить товары"
        );
    }

    const products =
        await productsRes.json();

    if (products.length === 0) {

        throw new Error(
            "В базе данных нет товаров"
        );
    }


    productSelect.innerHTML = `
        <option value="">
            Выберите товар
        </option>

        ${products
            .map(product => `
                <option
                    value="${product.id}"
                    data-name="${product.name}"
                    data-price="${product.price}">
                    ${product.name} (${product.price} ₽)
                </option>
            `)
            .join("")}
    `;


    // ==========================================
    // Сотрудники
    // ==========================================

    const employeesRes =
        await fetch(
            "/api/employees"
        );

    if (!employeesRes.ok) {

        throw new Error(
            "Не удалось загрузить сотрудников"
        );
    }

    const employees =
        await employeesRes.json();

    if (employees.length === 0) {

        throw new Error(
            "В базе данных нет сотрудников"
        );
    }


    employeeSelect.innerHTML =
        employees
            .map(employee => `
                <option value="${employee.id}">
                    ${employee.name}
                </option>
            `)
            .join("");
}


// ==========================================
// Отправка заказа
// ==========================================

if (orderForm) {

    orderForm.addEventListener(
        "submit",
        async (event) => {

            event.preventDefault();

            event.stopPropagation();


            const clientId =
                parseInt(
                    document.getElementById(
                        "orderClientSelect"
                    ).value,
                    10
                );


            const employeeId =
                parseInt(
                    document.getElementById(
                        "orderEmployeeSelect"
                    ).value,
                    10
                );


            // ==========================================
            // Проверка клиента
            // ==========================================

            if (!clientId) {

                alert(
                    "Выберите клиента."
                );

                return;
            }


            // ==========================================
            // Проверка сотрудника
            // ==========================================

            if (!employeeId) {

                alert(
                    "Выберите сотрудника."
                );

                return;
            }


            // ==========================================
            // Проверка позиций
            // ==========================================

            if (orderItems.length === 0) {

                alert(
                    "Добавьте хотя бы один товар в заказ."
                );

                return;
            }


            // ==========================================
            // Объединяем одинаковые товары
            // ==========================================

            const mergedItems = [];


            orderItems.forEach(item => {

                const existing =
                    mergedItems.find(
                        existingItem =>
                            existingItem.productId ===
                            item.productId
                    );


                if (existing) {

                    existing.quantity +=
                        Number(item.quantity);

                }
                else {

                    mergedItems.push({

                        productId:
                            Number(item.productId),

                        quantity:
                            Number(item.quantity)
                    });
                }

            });


            // ==========================================
            // Формируем заказ
            // ==========================================

            const orderData = {

                clientId:
                    clientId,

                employeeId:
                    employeeId,

                items:
                    mergedItems
            };


            console.log(
                editingOrderId
                    ? "Изменяем заказ:"
                    : "Создаём заказ:",
                orderData
            );


            try {

                let response;


                // ==========================================
                // РЕДАКТИРОВАНИЕ
                // ==========================================

                if (editingOrderId) {

                    response =
                        await fetch(
                            `/api/orders/${editingOrderId}`,
                            {
                                method: "PUT",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        orderData
                                    )
                            }
                        );

                }

                // ==========================================
                // СОЗДАНИЕ
                // ==========================================

                else {

                    response =
                        await fetch(
                            "/api/orders",
                            {
                                method: "POST",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        orderData
                                    )
                            }
                        );
                }


                let result = null;

                try {
                    result =
                        await response.json();
                }
                catch {
                    result = null;
                }


                console.log(
                    "Ответ сервера:",
                    response.status,
                    result
                );


                if (!response.ok) {

                    throw new Error(
                        result?.error ||
                        result?.message ||
                        "Не удалось сохранить заказ"
                    );
                }


                // ==========================================
                // Сообщение
                // ==========================================

                if (editingOrderId) {

                    alert(
                        "Заказ успешно изменён!"
                    );

                }
                else {

                    alert(
                        "Заказ успешно создан!"
                    );
                }


                // ==========================================
                // Закрываем окно
                // ==========================================

                closeOrderCreateModal();


                // ==========================================
                // Обновляем таблицу
                // ==========================================

                await loadOrders();

            }
            catch (error) {

                console.error(
                    "Ошибка сохранения заказа:",
                    error
                );

                alert(
                    "Ошибка при сохранении заказа:\n\n" +
                    error.message
                );
            }
        }
    );
}