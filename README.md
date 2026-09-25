# Product CRUD - Internship Project

Dự án Product CRUD được phát triển trong quá trình thực tập, bắt đầu từ chức năng CRUD sản phẩm cơ bản và được mở rộng dần với phân quyền, Audit Log, upload file, caching, Docker, CI/CD, centralized logging và API Gateway.

Ở phiên bản hiện tại, hệ thống sử dụng **ASP.NET Core .NET 9**, **Angular 18**, **SQL Server**, **Redis**, **Serilog + Seq**, **YARP API Gateway** và **Docker Compose**.

---

## 1. Chức năng chính

### Quản lý sản phẩm

- Xem danh sách sản phẩm.
- Thêm, sửa và xóa sản phẩm.
- Xóa sản phẩm theo cơ chế Soft Delete.
- Tìm kiếm sản phẩm theo mã hoặc tên.
- Phân trang dữ liệu.
- Lọc dữ liệu theo trạng thái.
- Sắp xếp dữ liệu theo nhiều trường.
- Truy vấn bằng LINQ + `IQueryable`.
- Sử dụng `AsNoTracking()` cho các truy vấn chỉ đọc.

### Quản lý danh mục

- Xem danh sách danh mục.
- Thêm và sửa danh mục với quyền Admin hoặc Editor.
- Xóa danh mục với quyền Admin.
- Cache danh sách Category bằng Redis.
- Tự động xóa cache khi Category được thêm, sửa hoặc xóa.

Luồng Cache-Aside:

```text
GET Categories
      |
      v
    Redis
   /     \
 HIT     MISS
  |        |
  |        v
  |    SQL Server
  |        |
  |        v
  |    Redis SET
  |        |
  +--------+
      |
      v
   Response
```

Cache Category có thời gian sống 10 phút.

Khi dữ liệu Category thay đổi:

```text
Create / Update / Delete
          |
          v
      SQL Server
          |
          v
   Redis Invalidate
```

SQL Server vẫn là nguồn dữ liệu chính, Redis chỉ lưu dữ liệu cache tạm thời.

---

## 2. Authentication và Authorization

Hệ thống sử dụng JWT Authentication.

Có 3 role:

| Role | Quyền |
| --- | --- |
| Viewer | Xem sản phẩm và danh mục |
| Editor | Xem, thêm, sửa sản phẩm và danh mục |
| Admin | Toàn quyền, bao gồm xóa dữ liệu và xem Audit Log |

Backend kiểm tra quyền bằng:

```csharp
[Authorize]
[Authorize(Roles = "Admin,Editor")]
[Authorize(Roles = "Admin")]
```

Frontend sử dụng:

- `AuthGuard` để kiểm tra đăng nhập.
- `RoleGuard` để giới hạn truy cập theo role.
- Ẩn hoặc hiện các nút Thêm, Sửa, Xóa theo quyền người dùng.

Nếu JWT hết hạn, `HttpInterceptor` sẽ đăng xuất người dùng và chuyển về màn hình đăng nhập.

---

## 3. Audit Logging bất đồng bộ

Các thao tác thay đổi Product được ghi lại vào Audit Log.

Thay vì ghi Audit Log trực tiếp trong request chính, hệ thống sử dụng:

```text
Product Request
      |
      v
 Entity Framework
      |
      v
AuditLogInterceptor
      |
      v
 C# Channel
      |
      v
AuditLogBackgroundService
      |
      v
 AuditLogs Table
```

Các thành phần chính:

```text
AuditLogInterceptor
AuditLogQueue
IAuditLogQueue
AuditLogBackgroundService
```

`AuditLogInterceptor` đóng vai trò Producer.

`Channel` đóng vai trò queue trong bộ nhớ.

`AuditLogBackgroundService` đóng vai trò Consumer và ghi Audit Log xuống SQL Server ở background.

Điều này giúp việc ghi Audit Log không làm request chính phải chờ thêm thao tác ghi log.

---

## 4. Upload file

Hệ thống hỗ trợ upload:

- JPG
- JPEG
- PNG
- PDF

Giới hạn dung lượng:

```text
5 MB
```

Backend kiểm tra:

- Extension của file.
- Dung lượng file.
- File signature để hạn chế trường hợp đổi đuôi file giả.

File được lưu tại:

```text
wwwroot/uploads/
```

và được truy cập thông qua:

```text
/uploads/{fileName}
```

---

## 5. Xử lý lỗi tập trung

Backend sử dụng:

```text
GlobalExceptionMiddleware
```

để xử lý exception tập trung và trả về response thống nhất.

Một response lỗi có thể bao gồm:

```json
{
  "success": false,
  "message": "Thông báo lỗi",
  "statusCode": 400,
  "traceId": "..."
}
```

Frontend sử dụng `HttpInterceptor` để:

- Gắn JWT token vào request.
- Xử lý lỗi HTTP.
- Hiển thị Toast.
- Tự đăng xuất khi token hết hạn.
- Retry tối đa 3 lần cho một số lỗi tạm thời khi thực hiện GET request.

---

## 6. Redis Cache

Redis được sử dụng để cache các dữ liệu ít thay đổi, hiện tại áp dụng cho danh sách Category.

Công nghệ:

```text
Microsoft.Extensions.Caching.StackExchangeRedis
```

Cache key hiện tại:

```text
ProductCrud:categories:all
```

Luồng đọc:

```text
Request
   |
   v
Redis
   |
   +---- HIT ----> Response
   |
   +---- MISS ---> SQL Server
                     |
                     v
                  Redis SET
                     |
                     v
                  Response
```

Luồng ghi:

```text
Create / Update / Delete Category
             |
             v
         SQL Server
             |
             v
       Remove Redis Cache
```

Có thể kiểm tra Redis bằng:

```bash
docker compose exec redis redis-cli KEYS "*"
```

---

## 7. Centralized Logging với Serilog và Seq

Backend sử dụng Serilog để ghi log có cấu trúc.

Các package chính:

```text
Serilog.AspNetCore
Serilog.Sinks.Console
Serilog.Sinks.Seq
```

Log được gửi tới:

```text
Console
   +
Seq
```

Luồng logging:

```text
HTTP Request
     |
     v
ProductCrud.Api
     |
     v
   Serilog
   /     \
Console   Seq
```

API sử dụng:

```csharp
app.UseSerilogRequestLogging();
```

để ghi lại các HTTP request.

Khi chạy bằng Docker, API gửi log tới:

```text
http://seq:5341
```

Seq UI được truy cập từ máy host tại:

```text
http://localhost:5342
```

---

## 8. API Gateway với YARP

Hệ thống có một API Gateway riêng sử dụng:

```text
YARP - Yet Another Reverse Proxy
```

Project:

```text
backend/ProductCrud.Gateway
```

Gateway nhận các request:

```text
/api/**
/uploads/**
```

và chuyển tiếp tới Product CRUD API.

Luồng request khi chạy bằng Docker:

```text
Browser
   |
   v
Angular
   |
   v
Nginx
   |
   v
YARP Gateway
   |
   v
ProductCrud.Api
   |
   +------> Redis
   |
   +------> SQL Server
   |
   +------> Seq
```

Trong Docker network:

```text
frontend
   |
   v
gateway:8080
   |
   v
api:8080
```

Gateway được expose ra host tại:

```text
http://localhost:5082
```

API chính vẫn có thể truy cập trực tiếp tại:

```text
http://localhost:5081
```

---

## 9. Docker

Toàn bộ hệ thống được chạy bằng Docker Compose.

Các service hiện tại:

```text
sqlserver
redis
seq
api
gateway
frontend
```

Kiến trúc:

```text
                        +----------------+
                        |     Browser    |
                        +-------+--------+
                                |
                                v
                    http://localhost:4200
                                |
                                v
                       +----------------+
                       | Angular/Nginx  |
                       +-------+--------+
                               |
                               v
                       +----------------+
                       |  YARP Gateway  |
                       +-------+--------+
                               |
                               v
                       +----------------+
                       | ProductCrud.Api|
                       +--+----------+--+
                          |          |
                 +--------+          +--------+
                 |                            |
                 v                            v
           +-----------+                +-----------+
           |   Redis   |                |SQL Server |
           +-----------+                +-----------+

                       ProductCrud.Api
                              |
                              v
                           Serilog
                              |
                              v
                             Seq
```

### Docker ports

| Service | Host | Container |
| --- | ---: | ---: |
| Frontend | 4200 | 80 |
| Gateway | 5082 | 8080 |
| API | 5081 | 8080 |
| Seq UI | 5342 | 80 |
| SQL Server | 1433 | 1433 |
| Redis | 6379 | 6379 |

SQL Server sử dụng Docker Volume:

```text
sqlserver_data
```

để dữ liệu không bị mất khi container được tạo lại.

Seq sử dụng:

```text
seq_data
```

để lưu dữ liệu log.

---

## 10. CI/CD với GitHub Actions

Project sử dụng GitHub Actions tại:

```text
.github/workflows/ci.yml
```

Workflow chạy khi:

```text
Push
Pull Request
Manual workflow_dispatch
```

Pipeline hiện tại:

```text
                   Git Push / Pull Request
                            |
             +--------------+--------------+
             |              |              |
             v              v              v
      Angular Build    .NET API Build  Gateway Build
             |              |              |
             v              v              v
      Frontend Docker   API Docker    Gateway Docker
          Build            Build           Build
```

Các job:

```text
Angular Build
.NET API Build
.NET Gateway Build
Frontend Docker Build
API Docker Build
Gateway Docker Build
```

Angular được build production bằng:

```bash
npm ci
npm run build -- --configuration production
```

API và Gateway được build bằng .NET 9.

Sau khi build source thành công, GitHub Actions tiếp tục build và kiểm tra Docker Image.

> Pipeline hiện tại tập trung vào Continuous Integration và tự động kiểm tra khả năng đóng gói Docker Image. Docker Image chưa được push lên container registry hoặc tự động triển khai lên production server.

---

## 11. Automated Deployment

Project có script:

```text
deploy.ps1
```

để tự động build và khởi động toàn bộ Docker stack.

Script thực hiện:

```text
Check Docker
    |
    v
Check configuration
    |
    v
Build Docker Images
    |
    v
docker compose up
    |
    v
Wait for services
    |
    v
Verify endpoints
```

Các service được kiểm tra:

```text
SQL Server
Redis
Seq
API
Gateway
Frontend
```

Script cũng kiểm tra:

- Frontend.
- Swagger API.
- Gateway.
- Seq UI.

---

## 12. Công nghệ sử dụng

### Backend

- ASP.NET Core .NET 9
- Entity Framework Core 9
- LINQ / IQueryable
- JWT Authentication
- ASP.NET Core Authorization
- C# Channel
- BackgroundService
- Serilog
- YARP Reverse Proxy

### Frontend

- Angular 18
- TypeScript 5.5
- RxJS
- Angular Router
- HttpInterceptor
- Route Guard

### Database và Cache

- SQL Server 2022
- Redis 7

### DevOps

- Docker
- Docker Compose
- Nginx
- PowerShell
- GitHub Actions
- Seq

---

## 13. Cấu trúc project

```text
ProductCRUD/
│
├── .github/
│   └── workflows/
│       └── ci.yml
│
├── backend/
│   │
│   ├── ProductCrud.Api/
│   │   ├── BackgroundServices/
│   │   ├── Controllers/
│   │   ├── Infrastructure/
│   │   ├── Middleware/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Dockerfile
│   │   └── Program.cs
│   │
│   ├── ProductCrud.DataServices/
│   │   ├── Audit/
│   │   ├── Data/
│   │   ├── Entities/
│   │   ├── Infrastructure/
│   │   ├── Migrations/
│   │   ├── Models/
│   │   ├── Repositories/
│   │   └── Services/
│   │
│   └── ProductCrud.Gateway/
│       ├── Dockerfile
│       ├── Program.cs
│       └── appsettings.json
│
├── frontend/
│   └── product-crud-ui/
│       ├── src/
│       ├── Dockerfile
│       ├── nginx.conf
│       └── package.json
│
├── docker-compose.yml
├── deploy.ps1
├── .env.example
└── README.md
```

---

## 14. Cách chạy project

### Yêu cầu

Máy cần cài:

```text
Docker Desktop
PowerShell
Git
```

Nếu chỉ chạy bằng Docker thì không cần chạy Angular hoặc API thủ công.

### Bước 1: Clone project

```bash
git clone <repository-url>
cd ProductCRUD
```

### Bước 2: Tạo file `.env`

Tạo:

```text
.env
```

dựa trên:

```text
.env.example
```

Cấu hình:

```env
SA_PASSWORD=your_sql_server_password
JWT_KEY=your_jwt_secret_key
```

Không commit `.env` chứa secret lên Git.

### Bước 3: Chạy toàn bộ hệ thống

Trên PowerShell:

```powershell
.\deploy.ps1
```

Hoặc:

```bash
docker compose up -d --build
```

### Bước 4: Kiểm tra container

```bash
docker compose ps
```

Hệ thống cần có 6 service:

```text
sqlserver
redis
seq
api
gateway
frontend
```

---

## 15. Địa chỉ truy cập

| Thành phần | URL |
| --- | --- |
| Frontend | http://localhost:4200 |
| API Gateway | http://localhost:5082 |
| Swagger API | http://localhost:5081/swagger |
| Seq | http://localhost:5342 |
| SQL Server | localhost:1433 |
| Redis | localhost:6379 |

Khi sử dụng giao diện Angular, request API sẽ đi theo luồng:

```text
Browser
   ↓
Angular / Nginx
   ↓
YARP Gateway
   ↓
ProductCrud.Api
```

---

## 16. Tài khoản test

Khi database chưa có user, hệ thống tự tạo ba tài khoản:

| Username | Password | Role |
| --- | --- | --- |
| admin | Admin@123 | Admin |
| editor | Editor@123 | Editor |
| viewer | Viewer@123 | Viewer |

Password được lưu dưới dạng hash bằng ASP.NET Core `PasswordHasher`.

---

## 17. Một số lệnh kiểm tra

### Xem trạng thái Docker

```bash
docker compose ps
```

### Xem log API

```bash
docker compose logs -f api
```

### Xem log Gateway

```bash
docker compose logs -f gateway
```

### Kiểm tra Redis

```bash
docker compose exec redis redis-cli KEYS "*"
```

### Xóa Category cache để test MISS

```bash
docker compose exec redis redis-cli DEL "ProductCrud:categories:all"
```

Sau đó gọi API Categories lần đầu:

```text
REDIS MISS - Query Categories from database
```

Gọi lại lần thứ hai:

```text
REDIS HIT - Categories
```

### Dừng hệ thống

```bash
docker compose down
```

Lệnh trên không xóa SQL Server volume.

Không sử dụng:

```bash
docker compose down -v
```

nếu muốn giữ dữ liệu database.

---

## 18. Luồng hệ thống tổng thể

```text
                           GitHub
                              |
                              v
                       GitHub Actions
                              |
             +----------------+----------------+
             |                |                |
             v                v                v
         Angular           .NET API        .NET Gateway
          Build             Build             Build
             |                |                |
             v                v                v
         Docker            Docker            Docker
          Build             Build             Build


                           Browser
                              |
                              v
                      Angular + Nginx
                              |
                              v
                        YARP Gateway
                              |
                              v
                       ProductCrud.Api
                      /       |       \
                     /        |        \
                    v         v         v
               SQL Server   Redis    Serilog
                                      |
                                      v
                                     Seq

                       Product CRUD
                              |
                              v
                    AuditLogInterceptor
                              |
                              v
                         C# Channel
                              |
                              v
                  AuditLogBackgroundService
                              |
                              v
                         AuditLogs
```

---

## 19. Tiến độ hiện tại

Các nội dung chính đã triển khai:

- CRUD Product.
- LINQ + IQueryable.
- Pagination, Search, Filter và Sort.
- JWT Authentication.
- Authorization theo Admin / Editor / Viewer.
- Route Guard và phân quyền giao diện.
- Upload JPG, PNG và PDF.
- Global Exception Middleware.
- Audit Log.
- Async Audit Logging bằng Channel + BackgroundService.
- Docker hóa Frontend và Backend.
- SQL Server chạy bằng Docker.
- Redis Cache và Cache-Aside.
- Cache invalidation.
- PowerShell automated deployment.
- CI/CD automation bằng GitHub Actions.
- Build Docker Image trong pipeline.
- Serilog structured logging.
- Centralized Logging với Seq.
- API Gateway bằng YARP.
- Frontend định tuyến API thông qua Gateway.
