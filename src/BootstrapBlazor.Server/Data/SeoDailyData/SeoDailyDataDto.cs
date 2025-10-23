using System.ComponentModel.DataAnnotations;

namespace BootstrapBlazor.Server.Data
{
    public class UploadSeoDailyDataRequest
    {
        public DateTime DataDate { get; set; }
        public byte[] ExcelBytes { get; set; } = Array.Empty<byte>();
        public string? FileName { get; set; }
    }

    public class GetSeoDailyDataRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Team { get; set; }
        public string? Domain { get; set; }
        public string? Keyword { get; set; }
        public string? UserName { get; set; } // Tên user để filter domain thuộc quyền quản lý
        public int? UserId { get; set; } // ID user để lấy fullname
        public bool OnlyManagedDomains { get; set; } = false; // Chỉ trả về các dòng có domain được phụ trách
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 20;
    }

    public class SeoDailyDataWithManagedDomainsDto
    {
        public int Id { get; set; }
        public DateTime DataDate { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public int? SearchVolumePreviousMonth { get; set; }
        public int? SearchVolumeCurrentMonth { get; set; }
        public string Team { get; set; } = string.Empty;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public int? BrandOdx { get; set; }
        public string? Top1 { get; set; }
        public string? Top2 { get; set; }
        public string? Top3 { get; set; }
        public string? Top4 { get; set; }
        public string? Top5 { get; set; }
        public string? Top6 { get; set; }
        public string? Top7 { get; set; }
        public string? Top8 { get; set; }
        public string? Top9 { get; set; }
        public string? Top10 { get; set; }
        public List<string> ManagedDomains { get; set; } = new List<string>();
        public Dictionary<string, bool> IsManagedDomain { get; set; } = new Dictionary<string, bool>();
    }

    public class UploadSeoDailyDataResponse
    {
        public int TotalRecords { get; set; }
        public int SuccessRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<SeoDailyDataWithManagedDomainsDto> Data { get; set; } = new List<SeoDailyDataWithManagedDomainsDto>();
    }


}
