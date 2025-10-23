using Microsoft.Extensions.Localization;

namespace BootstrapBlazor.Components;

/// <summary>
///
/// </summary>
public partial class LogoutLink
{
    [Inject]
    [NotNull]
    private IStringLocalizer<LogoutLink>? Localizer { get; set; }

    /// <summary>
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// </summary>
    [Parameter]
    [NotNull]
    public string? Text { get; set; }

    /// <summary>
    /// </summary>
    [Parameter]
    [NotNull]
    public string? Url { get; set; }

    [Inject]
    [NotNull]
    private IIconTheme? IconTheme { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Text ??= Localizer[nameof(Text)];
        Icon ??= IconTheme.GetIconByKey(ComponentIcons.LogoutLinkIcon);
        Url ??= "/login";
    }
}
