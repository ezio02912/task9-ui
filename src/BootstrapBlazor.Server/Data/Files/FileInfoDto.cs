namespace BootstrapBlazor.Server.Data;
public class FileInfoDto
{
    public string Name { get; set; }
    public ulong Size { get; set; }
    public string Url { get; set; }
    public DateTime LastModified { get; set; }
}


public class ImportRequestBase
{
    public byte[] ExcelBytes { set; get; }
}
public class KeywordRankingImportRequest  : ImportRequestBase
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
 public class PicDomainAssignmentImportRequest  : ImportRequestBase
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
