# 🔧 CPD Service - Content Performance Dashboard

## 🎯 Tổng Quan

CPD Service cung cấp các API calls để tương tác với backend CPD system, bao gồm import dữ liệu, truy vấn thay đổi, và tạo báo cáo.

## 🏗️ Cấu Trúc Service

### 1. ICpdService.cs
**Interface chính** - Định nghĩa các method cho CPD operations
- `ImportAsync()` - Import dữ liệu từ Excel
- `GetSummaryAsync()` - Lấy báo cáo tổng quan
- `GetChangesAsync()` - Lấy danh sách thay đổi
- `GetItemsAsync()` - Lấy dữ liệu với filter
- `GetChangeDetailAsync()` - Lấy chi tiết thay đổi

### 2. CpdService.cs
**Implementation** - Triển khai các API calls
- HttpClient integration
- JSON serialization/deserialization
- Error handling và logging
- Timeout configuration

### 3. CpdModels.cs
**Data Models** - Tất cả DTOs và Response models
- Request models (ImportCpdRequest, CpdFilter)
- Response models (CpdImportResponse, CpdSummaryResponse, etc.)
- Data models (CpdItem, CpdChangeItem, etc.)
- Search filter DTOs

### 4. CpdServiceExtensions.cs
**DI Configuration** - Cấu hình Dependency Injection
- HttpClient configuration
- Service registration
- Configuration options

## 🚀 Cách Sử Dụng

### 1. Đăng Ký Service

Trong `Program.cs` hoặc `Startup.cs`:

```csharp
// Cách 1: Sử dụng default configuration
builder.Services.AddCpdServices();

// Cách 2: Sử dụng custom configuration
builder.Services.AddCpdServices(options =>
{
    options.BaseUrl = "https://your-api-url.com";
    options.ApiKey = "your-api-key";
    options.Timeout = TimeSpan.FromMinutes(15);
});
```

### 2. Inject Service

Trong component hoặc service:

```csharp
[Inject]
private ICpdService CpdService { get; set; }
```

### 3. Sử Dụng API

```csharp
// Import Excel file
var importRequest = new ImportCpdRequest
{
    DateKey = DateOnly.FromDateTime(DateTime.Today),
    ExcelBytes = excelFileBytes,
    FileName = "data.xlsx"
};

var result = await CpdService.ImportAsync(importRequest);

// Get summary data
var summary = await CpdService.GetSummaryAsync(
    DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
    DateOnly.FromDateTime(DateTime.Today)
);

// Get changes for specific date
var changes = await CpdService.GetChangesAsync(DateOnly.FromDateTime(DateTime.Today));

// Get items with filter
var filter = new CpdFilter
{
    DateKey = DateOnly.FromDateTime(DateTime.Today),
    Pic = "John Doe",
    Skip = 0,
    Take = 20
};

var items = await CpdService.GetItemsAsync(filter);
```

## 🔧 Configuration

### HttpClient Configuration

```csharp
services.AddHttpClient<ICpdService, CpdService>(client =>
{
    client.BaseAddress = new Uri("https://your-api-url.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(10);
});
```

### Environment Configuration

```json
{
  "CpdApi": {
    "BaseUrl": "https://localhost:5001",
    "ApiKey": "your-api-key",
    "Timeout": "00:10:00"
  }
}
```

## 📡 API Endpoints

### Import
- `POST /api/cpd/import` - Import Excel file

### Data
- `GET /api/cpd/items` - Get CPD items with filter
- `GET /api/cpd/summary` - Get summary data
- `GET /api/cpd/dates` - Get available dates

### Changes
- `GET /api/cpd/changes` - Get changes for date
- `GET /api/cpd/changes/detail/{id}` - Get change details

## 🛠️ Error Handling

### Response Pattern
```csharp
public class CpdResponse<T>
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
```

### Error Types
- **Validation Errors**: File format, size, required fields
- **Network Errors**: Connection timeout, server unavailable
- **Business Logic Errors**: Invalid data, processing failures
- **Authentication Errors**: Invalid API key, unauthorized access

## 📊 Logging

Service sử dụng ILogger để log các operations:

```csharp
_logger.LogInformation("Starting CPD import for date: {DateKey}", request.DateKey);
_logger.LogError(ex, "Error during CPD import");
```

### Log Levels
- **Information**: Successful operations
- **Warning**: Non-critical issues
- **Error**: Failed operations
- **Debug**: Detailed debugging info

## 🔄 Retry Policy

Service hỗ trợ retry policy cho network calls:

```csharp
services.AddHttpClient<ICpdService, CpdService>()
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
            });
}
```

## 🧪 Testing

### Unit Testing
```csharp
[Test]
public async Task ImportAsync_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var service = new CpdService(mockHttpClient.Object, mockLogger.Object);
    
    // Act
    var result = await service.ImportAsync(validRequest);
    
    // Assert
    Assert.True(result.Status);
}
```

### Integration Testing
```csharp
[Test]
public async Task ImportAsync_WithRealApi_ReturnsSuccess()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddCpdServices("https://test-api-url.com");
    var provider = services.BuildServiceProvider();
    var service = provider.GetRequiredService<ICpdService>();
    
    // Act
    var result = await service.ImportAsync(testRequest);
    
    // Assert
    Assert.True(result.Status);
}
```

## 🔒 Security

### API Key Authentication
```csharp
client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
```

### HTTPS Only
```csharp
client.BaseAddress = new Uri("https://secure-api-url.com");
```

### Input Validation
- File size limits (20MB max)
- File type validation (.xlsx only)
- Date range validation
- Required field validation

## 📈 Performance

### Caching
```csharp
services.AddMemoryCache();
services.AddScoped<ICpdService, CachedCpdService>();
```

### Compression
```csharp
services.AddHttpClient<ICpdService, CpdService>()
    .AddHttpMessageHandler<CompressionHandler>();
```

### Timeout Configuration
```csharp
client.Timeout = TimeSpan.FromMinutes(10); // For large file uploads
```

## 🔄 Future Enhancements

### Planned Features
- [ ] Caching layer
- [ ] Batch operations
- [ ] Real-time updates
- [ ] Offline support
- [ ] Advanced retry policies
- [ ] Circuit breaker pattern

### Technical Improvements
- [ ] Response compression
- [ ] Request batching
- [ ] Connection pooling
- [ ] Metrics collection
- [ ] Health checks

---

**© 2024 CPD Service. All rights reserved.**
