using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Data;

public class KeywordSearchFilterDto : ITableSearchModel
{
    public string? KeywordText { get; set; }
    public string? BrandName { get; set; }
    public string? UserNames { get; set; }
    public int? UserId { get; set; }
    public bool OnlyWithUsers { get; set; } = true;
    
    public SelectedItem Pic { get; set; } = new();
    
    public List<BrandDto> Brands { get; set; } = new();
    
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        return ret;
    }

    public void Reset()
    {
        KeywordText = null;
        BrandName = null;
        UserNames = null;
        UserId = null;
        OnlyWithUsers = true;
    }
}
