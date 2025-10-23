


using System.Globalization;
namespace BootstrapBlazor.Server.Components.Task9;

public sealed partial class LayoutPages
{
    private List<SelectedItem> SideBarItems { get; } =
    [
        new("left-right", "Left - Right"),
        new("top-bottom", "Top - Bottom")
    ];

    [NotNull]
    private SelectedItem? ActiveItem { get; set; }

    private string? StyleString => CssBuilder.Default()
        .AddClass($"height: {Height * 100}px", Height > 0)
        .Build();

    private double Height { get; set; }

    private bool ShowFooter { get; set; }

    private bool IsFixedHeader { get; set; }

    private bool IsFixedTabHeader { get; set; }

    private bool IsFixedFooter { get; set; }

    private bool IsFullSide { get; set; }

    private bool UseTabSet { get; set; }

    private string SelectedCulture { get; set; } = CultureInfo.CurrentUICulture.Name;

    public string Role => RootPage?.Role ?? "";

    [CascadingParameter]
    [NotNull]
    private PageLayout? RootPage { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? Navigator { get; set; }
    [Inject]
    [NotNull]
    private IGroupService GroupService { get; set; }
    [Inject]
    [NotNull]
    private IGroupKeywordService GroupKeywordService { get; set; }
    [Inject]
    [NotNull]
    private IKeywordRankingService KeywordRankingService { get; set; }
    [Inject]
    [NotNull]
    private ToastService? Toast { get; set; }
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    private KeywordRankingImportRequest ModelKeywordRankingImport = new KeywordRankingImportRequest();

    protected override void OnInitialized()
    {
        base.OnInitialized();

        IsFullSide = RootPage.IsFullSide;
        IsFixedHeader = RootPage.IsFixedHeader;
        IsFixedFooter = RootPage.IsFixedFooter;
        ShowFooter = RootPage.ShowFooter;
        UseTabSet = RootPage.UseTabSet;
        IsFixedTabHeader = RootPage.IsFixedTabHeader;
        ActiveItem = IsFullSide ? SideBarItems[0] : SideBarItems[1];
    }

    private Task OnFooterChanged(bool val)
    {
        ShowFooter = val;
        Update();
        return Task.CompletedTask;
    }

    private Task OnTabStateChanged(CheckboxState state, bool val)
    {
        IsFixedTabHeader = val;
        Update();
        return Task.CompletedTask;
    }

    private Task OnHeaderStateChanged(CheckboxState state, bool val)
    {
        IsFixedHeader = val;
        Update();
        return Task.CompletedTask;
    }

    private Task OnFooterStateChanged(CheckboxState state, bool val)
    {
        IsFixedFooter = val;
        Update();
        return Task.CompletedTask;
    }

    private Task OnSideChanged(IEnumerable<SelectedItem> values, SelectedItem item)
    {
        ActiveItem.Active = false;
        item.Active = true;
        ActiveItem = item;
        IsFullSide = item.Value == "left-right";
        Update();
        return Task.CompletedTask;
    }

    private Task OnUseTabSetChanged(bool val)
    {
        UseTabSet = val;
        Update();
        return Task.CompletedTask;
    }
    public void Update()
    {
        RootPage.IsFullSide = IsFullSide;
        RootPage.IsFixedFooter = IsFixedFooter && ShowFooter;
        RootPage.IsFixedHeader = IsFixedHeader;
        RootPage.IsFixedTabHeader = IsFixedTabHeader;
        RootPage.ShowFooter = ShowFooter;
        RootPage.UseTabSet = UseTabSet;
        StateHasChanged();
        //RootPage.Update();
    }

    private async Task OnDropGroupUpload(UploadFile file)
    {
        if (file is null || file.File is null)
        {
            await Toast.Error("Error", "Vui lòng tải file lên");
            return;
        }

        if (file.File is { Size: > 20 * 1024 * 1024 })
        {
            file.Code = 1004;
            await Toast.Information("Error", "File không được phép vượt quá 20Mb");
        }else{
            using var stream = new MemoryStream();
            // Đọc file và lưu vào stream
            await file.File.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(stream);
            ModelKeywordRankingImport.ExcelBytes = stream.ToArray();

            if (ModelKeywordRankingImport.ExcelBytes == null)
            {
                await Toast.Error("Error", "File lỗi");
                return;
            }


            var result = await GroupService.ImportExcelAsync(new ImportRequestBase()
            {
                ExcelBytes= ModelKeywordRankingImport.ExcelBytes,
            });
            if (result.Status)
            {
                ModelKeywordRankingImport = new();
                await Toast.Success("Success", result.Message);
            }
            else
            {
                await Toast.Error("Error", $"Import thất bại. {result.Message} ");
            }
        }

        return;
    }

    private async Task OnDropGroupKeywordUpload(UploadFile file)
    {
        if (file is null || file.File is null)
        {
            await Toast.Error("Error", "Vui lòng tải file lên");
            return;
        }

        if (file.File is { Size: > 20 * 1024 * 1024 })
        {
            file.Code = 1004;
            await Toast.Information("Error", "File không được phép vượt quá 20Mb");
        }else{
            using var stream = new MemoryStream();
            // Đọc file và lưu vào stream
            await file.File.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(stream);
            ModelKeywordRankingImport.ExcelBytes = stream.ToArray();

            if (ModelKeywordRankingImport.ExcelBytes == null)
            {
                await Toast.Error("Error", "File lỗi");
                return;
            }


            var result = await GroupKeywordService.ImportExcelAsync(new ImportRequestBase()
            {
                ExcelBytes= ModelKeywordRankingImport.ExcelBytes,
            });
            if (result.Status)
            {
                ModelKeywordRankingImport = new();
                await Toast.Success("Success", result.Message);
            }
            else
            {
                await Toast.Error("Error", $"Import thất bại. {result.Message} ");
            }
        }

        return;
    }
    private async Task OnDropKeywordUpload(UploadFile file)
    {
        if (file is null || file.File is null)
        {
            await Toast.Error("Error", "Vui lòng tải file lên");
            return;
        }

        if (file.File is { Size: > 20 * 1024 * 1024 })
        {
            file.Code = 1004;
            await Toast.Information("Error", "File không được phép vượt quá 20Mb");
        }else{
            using var stream = new MemoryStream();
            // Đọc file và lưu vào stream
            await file.File.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(stream);
            ModelKeywordRankingImport.ExcelBytes = stream.ToArray();

            if (ModelKeywordRankingImport.ExcelBytes == null)
            {
                await Toast.Error("Error", "File lỗi");
                return;
            }


            var result = await KeywordRankingService.ImportExcelAsync(ModelKeywordRankingImport);
            if (result.Status)
            {
                ModelKeywordRankingImport = new();
                await Toast.Success("Success", result.Message);
            }
            else
            {
                await Toast.Error("Error", $"Import thất bại. {result.Message} ");
            }
        }

        return;
    }
}
