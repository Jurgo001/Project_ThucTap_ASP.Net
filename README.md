# Product CRUD - Week 2

Đây là project thực tập tuần 2, phát triển tiếp từ bài CRUD sản phẩm ở tuần 1.

## Nội dung đã làm

* Phân trang, tìm kiếm, lọc và sắp xếp dữ liệu bằng LINQ + IQueryable.
* Phân quyền người dùng theo 3 role: Admin, Editor, Viewer.
* Dùng Route Guard để chặn các trang không đúng quyền.
* Ẩn/hiện các nút Thêm, Sửa, Xóa theo role.
* Ghi lại lịch sử thêm, sửa, xóa sản phẩm vào Audit Log.
* Chỉ Admin được xem màn hình lịch sử hoạt động.
* Upload hình ảnh và file PDF, có giới hạn dung lượng và hiển thị tiến trình upload.
* Xử lý lỗi tập trung ở Backend bằng Middleware.
* Frontend dùng HttpInterceptor và Toast để hiển thị lỗi.
* Nếu token hết hạn thì tự chuyển về trang đăng nhập.

## Công nghệ sử dụng

* ASP.NET Core 8 Web API
* Entity Framework Core
* LINQ / IQueryable
* SQL Server
* JWT
* Angular 18
* TypeScript

## Cấu trúc project

```text
ProductCRUD/
├── backend/
├── frontend/
└── sql/
```

## Chạy project

### Backend

```bash
cd backend/ProductCrud.Api
dotnet run
```

Swagger:

```text
http://localhost:5081/swagger
```

### Frontend

```bash
cd frontend/product-crud-ui
npm install
npm start
```

Frontend:

```text
http://localhost:4200
```

## Tài khoản test

| Username | Password   | Role   |
| -------- | ---------- | ------ |
| admin    | Admin@123  | Admin  |
| editor   | Editor@123 | Editor |
| viewer   | Viewer@123 | Viewer |

## Phân quyền

* Viewer: chỉ xem sản phẩm
* Editor: xem, thêm, sửa
* Admin: toàn quyền và xem Audit Log
