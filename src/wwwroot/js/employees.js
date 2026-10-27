const departmentForm = document.querySelector("#department-form");
const employeeForm = document.querySelector("#employee-form");
const departmentNameInput = document.querySelector("#department-name");
const departmentSelect = document.querySelector("#employee-department");
const departmentFilter = document.querySelector("#department-filter");
const departmentList = document.querySelector("#department-list");
const employeeList = document.querySelector("#employee-list");
const feedback = document.querySelector("#feedback");
const emptyState = document.querySelector("#empty-state");
const employeeCount = document.querySelector("#employee-count");
const cancelEditButton = document.querySelector("#cancel-edit");
let departments = [];

async function request(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...options.headers
        }
    });

    if (!response.ok) {
        const problem = await response.json().catch(() => null);
        throw new Error(problem?.detail || problem?.title || `Yêu cầu thất bại (${response.status}).`);
    }

    return response.status === 204 ? null : response.json();
}

function showFeedback(message, isError = false) {
    feedback.textContent = message;
    feedback.className = `alert ${isError ? "alert-danger" : "alert-success"}`;
}

function addOption(select, value, label) {
    const option = document.createElement("option");
    option.value = value;
    option.textContent = label;
    select.append(option);
}

function renderDepartments() {
    const selectedDepartment = departmentSelect.value;
    const selectedFilter = departmentFilter.value;
    departmentSelect.replaceChildren();
    addOption(departmentSelect, "", "Chọn phòng ban");
    departmentFilter.replaceChildren();
    addOption(departmentFilter, "", "Tất cả phòng ban");
    departmentList.replaceChildren();

    for (const department of departments) {
        addOption(departmentSelect, department.id, department.name);
        addOption(departmentFilter, department.id, department.name);

        const item = document.createElement("li");
        item.className = "list-group-item d-flex justify-content-between align-items-center px-0";
        const label = document.createElement("span");
        label.textContent = `${department.name} (${department.employeeCount})`;
        const remove = document.createElement("button");
        remove.className = "btn btn-sm btn-outline-danger";
        remove.type = "button";
        remove.dataset.deleteDepartment = department.id;
        remove.textContent = "Xóa";
        item.append(label, remove);
        departmentList.append(item);
    }

    departmentSelect.value = selectedDepartment;
    departmentFilter.value = selectedFilter;
}

function renderEmployees(employees) {
    employeeList.replaceChildren();
    employeeCount.textContent = String(employees.length);
    emptyState.classList.toggle("d-none", employees.length > 0);

    for (const employee of employees) {
        const row = document.createElement("tr");
        for (const value of [employee.name, employee.phone, employee.position, employee.departmentName]) {
            const cell = document.createElement("td");
            cell.textContent = value;
            row.append(cell);
        }

        const actions = document.createElement("td");
        actions.className = "text-end text-nowrap";
        const edit = document.createElement("button");
        edit.className = "btn btn-sm btn-outline-primary me-1";
        edit.type = "button";
        edit.dataset.editEmployee = employee.id;
        edit.textContent = "Sửa";
        const remove = document.createElement("button");
        remove.className = "btn btn-sm btn-outline-danger";
        remove.type = "button";
        remove.dataset.deleteEmployee = employee.id;
        remove.textContent = "Xóa";
        actions.append(edit, remove);
        row.append(actions);
        employeeList.append(row);
    }
}

async function refresh() {
    const filter = departmentFilter.value;
    const employeeUrl = filter
        ? `/api/employees?departmentId=${encodeURIComponent(filter)}`
        : "/api/employees";
    const [departmentData, employeeData] = await Promise.all([
        request("/api/departments"),
        request(employeeUrl)
    ]);
    departments = departmentData;
    renderDepartments();
    renderEmployees(employeeData);
}

function resetEmployeeForm() {
    employeeForm.reset();
    document.querySelector("#employee-id").value = "";
    document.querySelector("#employee-form-title").textContent = "Thêm nhân viên";
    cancelEditButton.classList.add("d-none");
}

departmentForm.addEventListener("submit", async event => {
    event.preventDefault();
    try {
        await request("/api/departments", {
            method: "POST",
            body: JSON.stringify({ name: departmentNameInput.value })
        });
        departmentForm.reset();
        await refresh();
        showFeedback("Đã thêm phòng ban.");
    } catch (error) {
        showFeedback(error.message, true);
    }
});

employeeForm.addEventListener("submit", async event => {
    event.preventDefault();
    const id = document.querySelector("#employee-id").value;
    const employee = {
        name: document.querySelector("#employee-name").value,
        phone: document.querySelector("#employee-phone").value,
        position: document.querySelector("#employee-position").value,
        departmentId: Number(departmentSelect.value)
    };

    try {
        await request(id ? `/api/employees/${id}` : "/api/employees", {
            method: id ? "PUT" : "POST",
            body: JSON.stringify(employee)
        });
        resetEmployeeForm();
        await refresh();
        showFeedback(id ? "Đã cập nhật nhân viên." : "Đã thêm nhân viên.");
    } catch (error) {
        showFeedback(error.message, true);
    }
});

departmentFilter.addEventListener("change", async () => {
    try {
        const employees = await request(departmentFilter.value
            ? `/api/employees?departmentId=${encodeURIComponent(departmentFilter.value)}`
            : "/api/employees");
        renderEmployees(employees);
    } catch (error) {
        showFeedback(error.message, true);
    }
});

cancelEditButton.addEventListener("click", resetEmployeeForm);

document.addEventListener("click", async event => {
    const target = event.target;
    if (!(target instanceof HTMLButtonElement)) {
        return;
    }

    try {
        if (target.dataset.editEmployee) {
            const employee = await request(`/api/employees/${target.dataset.editEmployee}`);
            document.querySelector("#employee-id").value = employee.id;
            document.querySelector("#employee-name").value = employee.name;
            document.querySelector("#employee-phone").value = employee.phone;
            document.querySelector("#employee-position").value = employee.position;
            departmentSelect.value = employee.departmentId;
            document.querySelector("#employee-form-title").textContent = "Cập nhật nhân viên";
            cancelEditButton.classList.remove("d-none");
        } else if (target.dataset.deleteEmployee) {
            await request(`/api/employees/${target.dataset.deleteEmployee}`, { method: "DELETE" });
            await refresh();
            showFeedback("Đã xóa nhân viên.");
        } else if (target.dataset.deleteDepartment) {
            await request(`/api/departments/${target.dataset.deleteDepartment}`, { method: "DELETE" });
            await refresh();
            showFeedback("Đã xóa phòng ban.");
        }
    } catch (error) {
        showFeedback(error.message, true);
    }
});

refresh().catch(error => showFeedback(error.message, true));
