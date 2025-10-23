# 📊 CPD UI Components - Content Performance Dashboard

## 🎯 Tổng Quan

Bộ UI components cho hệ thống CPD (Content Performance Dashboard) được xây dựng dựa trên BootstrapBlazor framework, cung cấp giao diện người dùng trực quan và dễ sử dụng cho việc quản lý và theo dõi hiệu suất nội dung.

## 🏗️ Cấu Trúc Components

### 1. CpdManagement.razor
**Component chính** - Quản lý tất cả các tính năng CPD thông qua TabSet
- **Route**: `/cpd`
- **Chức năng**: Container chứa tất cả các tab con
- **Tabs**:
  - Tải lên dữ liệu
  - Xem dữ liệu  
  - Theo dõi thay đổi
  - Báo cáo thống kê

### 2. CpdUpload.razor
**Component upload Excel** - Tải lên file Excel CPD
- **Route**: `/cpd-upload`
- **Chức năng**: 
  - Upload file Excel (.xlsx)
  - Chọn ngày dữ liệu
  - Hiển thị kết quả import
  - Hiển thị thống kê thay đổi

**Tính năng chính**:
- ✅ DropUpload với validation file size (max 20MB)
- ✅ Validation file extension (.xlsx only)
- ✅ Hiển thị progress khi upload
- ✅ Kết quả import chi tiết (inserted, updated, total)
- ✅ Thống kê thay đổi so với ngày trước
- ✅ Error handling và thông báo

### 3. CpdDataView.razor
**Component xem dữ liệu** - Hiển thị danh sách dữ liệu CPD
- **Route**: `/cpd-data`
- **Chức năng**:
  - Hiển thị bảng dữ liệu CPD với pagination
  - Tìm kiếm và filter theo nhiều tiêu chí
  - Hiển thị chi tiết từng record

**Tính năng chính**:
- ✅ Table với pagination, sorting, searching
- ✅ Filter theo: Ngày, PIC, Brand, Position
- ✅ Hiển thị links (Publink, Original Link, Short Link)
- ✅ Status badges với màu sắc phân biệt
- ✅ Tooltip cho ghi chú
- ✅ Card view cho mobile

### 4. CpdChanges.razor
**Component theo dõi thay đổi** - Quản lý và xem thay đổi dữ liệu
- **Route**: `/cpd-changes`
- **Chức năng**:
  - Hiển thị danh sách thay đổi
  - Filter theo loại thay đổi, mức độ quan trọng
  - Xem chi tiết thay đổi trong modal

**Tính năng chính**:
- ✅ Table hiển thị thay đổi với pagination
- ✅ Filter theo: Ngày, Loại thay đổi, Mức độ, Business Key Hash
- ✅ Badge phân loại thay đổi (NEW/CHANGED/REMOVED)
- ✅ Badge mức độ quan trọng (1-3)
- ✅ Modal chi tiết thay đổi
- ✅ Hiển thị các trường thay đổi

### 5. CpdReports.razor
**Component báo cáo** - Thống kê và báo cáo dữ liệu
- **Route**: `/cpd-reports`
- **Chức năng**:
  - Báo cáo tổng quan theo khoảng thời gian
  - Biểu đồ thay đổi theo loại và PIC
  - Thống kê chi tiết

**Tính năng chính**:
- ✅ Chọn khoảng thời gian báo cáo
- ✅ Summary cards (Tổng items, Thay đổi, Mới, Bị xóa)
- ✅ Biểu đồ Doughnut cho thay đổi theo loại
- ✅ Biểu đồ Bar cho thay đổi theo PIC
- ✅ Bảng chi tiết với progress bars
- ✅ Responsive charts

## 🎨 UI/UX Features

### Design Pattern
- **Consistent**: Sử dụng BootstrapBlazor components
- **Responsive**: Tối ưu cho desktop, tablet, mobile
- **Accessible**: Hỗ trợ keyboard navigation và screen readers
- **Modern**: UI hiện đại với animations và transitions

### Color Scheme
- **Primary**: #007bff (Blue)
- **Success**: #28a745 (Green) - NEW items
- **Warning**: #ffc107 (Yellow) - CHANGED items  
- **Danger**: #dc3545 (Red) - REMOVED items
- **Info**: #17a2b8 (Cyan) - Info items
- **Secondary**: #6c757d (Gray) - Default

### Icons
- **Upload**: fa-solid fa-upload
- **Table**: fa-solid fa-table
- **Changes**: fa-solid fa-chart-line
- **Reports**: fa-solid fa-chart-bar
- **Search**: fa-solid fa-search
- **Refresh**: fa-solid fa-refresh
- **Eye**: fa-solid fa-eye
- **Info**: fa-solid fa-info-circle

## 🔧 Technical Implementation

### Dependencies
```csharp
// Required services
ICpdService - Main CPD service interface
ToastService - Notification service
DialogService - Modal dialogs
IJSRuntime - JavaScript interop for charts
```

### Data Models
```csharp
// Core DTOs
ImportCpdRequest - Upload request
ImportCpdResult - Upload result
CpdItem - Data item
CpdChangeItem - Change item
CpdSummaryData - Report data

// Filter DTOs
CpdFilter - Data filter
CpdSearchFilterDto - Search filter
CpdChangesSearchFilterDto - Changes filter
```

### Service Interface
```csharp
public interface ICpdService
{
    Task<CpdImportResponse> ImportAsync(ImportCpdRequest request);
    Task<CpdSummaryResponse> GetSummaryAsync(DateOnly from, DateOnly to);
    Task<CpdChangesResponse> GetChangesAsync(DateOnly dateKey);
    Task<CpdItemsResponse> GetItemsAsync(CpdFilter filter);
}
```

## 📱 Responsive Design

### Breakpoints
- **Desktop**: >= 1200px - Full layout
- **Tablet**: 768px - 1199px - Condensed layout
- **Mobile**: < 768px - Stacked layout

### Mobile Optimizations
- Tab navigation với icons
- Collapsible search forms
- Touch-friendly buttons
- Optimized table layouts
- Card view cho data tables

## 🚀 Usage Examples

### 1. Upload Excel File
```razor
<CpdUpload />
```
- Chọn ngày dữ liệu
- Drag & drop file Excel
- Xem kết quả import

### 2. View Data
```razor
<CpdDataView />
```
- Filter theo ngày, PIC, Brand
- Xem chi tiết từng record
- Export data (nếu cần)

### 3. Track Changes
```razor
<CpdChanges />
```
- Xem thay đổi theo ngày
- Filter theo loại và mức độ
- Xem chi tiết trong modal

### 4. Generate Reports
```razor
<CpdReports />
```
- Chọn khoảng thời gian
- Xem biểu đồ và thống kê
- Export báo cáo

## 🔍 Error Handling

### Validation
- File size validation (max 20MB)
- File extension validation (.xlsx only)
- Date range validation
- Required field validation

### Error Messages
- Toast notifications cho success/error
- Inline validation messages
- Loading states với spinners
- Empty states với helpful messages

## 📊 Charts Integration

### Chart.js Integration
- Doughnut chart cho thay đổi theo loại
- Bar chart cho thay đổi theo PIC
- Responsive charts
- Custom colors và styling

### JavaScript Functions
```javascript
window.renderCpdCharts = function(changesByType, changesByPic) {
    // Render charts using Chart.js
}
```

## 🎯 Best Practices

### Performance
- Lazy loading cho large datasets
- Pagination cho tables
- Debounced search
- Optimized re-renders

### Accessibility
- ARIA labels
- Keyboard navigation
- Screen reader support
- High contrast support

### Maintainability
- Modular component structure
- Reusable DTOs
- Consistent naming conventions
- Comprehensive CSS organization

## 🔄 Future Enhancements

### Planned Features
- [ ] Export to Excel/PDF
- [ ] Advanced filtering options
- [ ] Real-time updates
- [ ] Bulk operations
- [ ] Data visualization improvements
- [ ] User preferences
- [ ] Audit logging

### Technical Improvements
- [ ] Caching strategies
- [ ] Performance optimizations
- [ ] Unit testing
- [ ] Integration testing
- [ ] Documentation improvements

---

**© 2024 CPD UI Components. All rights reserved.**
