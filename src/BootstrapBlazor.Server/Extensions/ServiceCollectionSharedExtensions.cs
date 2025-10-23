using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using BootstrapBlazor.Server.Identity;
using BootstrapBlazor.Server.Services.MindsDB;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionSharedExtensions
{
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    public static IServiceCollection AddBootstrapBlazorServices(this IServiceCollection services, Action<BootstrapBlazorOptions>? configureOptions = null)
    {
        services.AddSingleton<WeatherForecastService>();
        services.AddSingleton<PackageVersionService>();
        services.AddSingleton<CodeSnippetService>();
        services.AddSingleton<DashboardService>();
        services.AddSingleton(typeof(IDataService<>), typeof(TableDemoDataService<>));
        services.AddSingleton<ILookupService, DemoLookupService>();
        services.AddSingleton<MockDataTableDynamicService>();

        services.AddSingleton<MenuService>();
        services.AddScoped<FanControllerDataService>();

        services.AddScoped<IAIChatService, AIChatService>();
        services.AddHttpClient<IMindsDBService, MindsDBService>(client =>
        {
            // Set timeout to 3 minutes for MindsDB API calls
            client.Timeout = TimeSpan.FromMinutes(3);
        });
        services.AddScoped<IRoleManagerService, RoleManagerService>();
        services.AddTransient<IUserManagerService, UserManagerService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddSingleton<IAppConfigService, AppConfigService>();
        services.AddScoped<DownloadFileService>();
        services.AddTransient<JsService>();
        services.AddScoped<IPicDomainAssignmentService, PicDomainAssignmentService>();
        services.AddScoped<IKeywordRankingService, KeywordRankingService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IGroupKeywordService, GroupKeywordService>();
        services.AddScoped<IKeywordService, KeywordService>();
        services.AddScoped<IKeywordMonthlyMetricService, KeywordMonthlyMetricService>();
        services.AddScoped<IMainsiteService, MainsiteService>();
        services.AddScoped<ITelegramContentGroupService, TelegramContentGroupService>();
        services.AddScoped<IDomainSearchService, DomainSearchService>();
        services.AddScoped<ISeoKpiWeightService, SeoKpiWeightService>();
        services.AddScoped<IDomainCheckDailyService, DomainCheckDailyService>();
        services.AddScoped<BootstrapBlazor.Server.Services.DomainCheckDaily.ISeoDailyDataService, BootstrapBlazor.Server.Services.DomainCheckDaily.SeoDailyDataService>();
        services.AddScoped<IFacebookGroupService, FacebookGroupService>();
        services.AddScoped<IFacebookCommentService, FacebookCommentService>();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<IFileManagerService, FileManagerService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<ILinkCheckerService, LinkCheckerService>();
        services.AddScoped<IScrapingbeeCheckerService, ScrapingbeeCheckerService>();
        services.AddScoped<IUserKeywordSelectionService, UserKeywordSelectionService>();
        services.AddScoped<IExcelExportService, ExcelExportService>();
        services.AddBlazoredLocalStorage();
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
        services.AddTransient<ScrapingbeeService>();

        services.AddBootstrapBlazorTotpService();

        services.AddOptionsMonitor<WebsiteOptions>();

        services.AddCascadingAuthenticationState();
        // services.AddScoped<AuthenticationStateProvider, MockAuthenticationStateProvider>();

        services.AddBootstrapBlazorMeiliSearch();

        services.AddBootstrapBlazor(configureOptions);

        //services.AddBootstrapBlazorAzureSpeech();

        services.AddBootstrapBlazorBaiduSpeech();

        services.AddBootstrapBlazorBaiduOcr();

        services.AddBootstrapBlazorAzureOpenAIService();

        services.AddBootstrapBlazorAzureTranslator();

        services.AddBootstrapBlazorHtml2PdfService();

        services.AddBootstrapBlazorHtml2ImageService();

        services.AddBootstrapBlazorWinBoxService();

        services.ConfigureTabItemMenuBindOptions(options =>
        {
            options.Binders.Add("layout-demo/text=Parameter1", new() { Text = "示例网页" });
            options.Binders.Add("layout-demo", new() { Text = "Text 1" });
            options.Binders.Add("layout-demo?text=Parameter", new() { Text = "Text 2" });
            options.Binders.Add("layout-demo/text=Parameter", new() { Text = "Text 3" });
        });

        services.AddBootstrapHolidayService();

        services.AddBootstrapBlazorTableExportService();

        services.AddBootstrapBlazorIP2RegionfService();

        services.AddBootstrapBlazorJuHeIpLocatorService();

        services.AddBootstrapBlazorTcpSocketFactory();

        return services;
    }
}
