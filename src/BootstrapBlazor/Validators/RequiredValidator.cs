// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License
// See the LICENSE file in the project root for more information.
// Maintainer: Argo Zhang(argo@live.ca) Website: https://www.blazor.zone

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections;
using System.Globalization;

namespace BootstrapBlazor.Components;

/// <summary>
/// Required 验证实现类
/// </summary>
public class RequiredValidator : ValidatorBase
{

    public string? ErrorMessage { get; set; }

    public bool AllowEmptyString { get; set; }

    public IStringLocalizerFactory? LocalizerFactory { get; set; }

    public JsonLocalizationOptions? Options { get; set; }


    public override void Validate(object? propertyValue, ValidationContext context, List<ValidationResult> results)
    {
        if (string.IsNullOrEmpty(ErrorMessage))
        {
            var localizer = context.GetRequiredService<IStringLocalizer<ValidateBase<string>>>();
            var l = localizer["DefaultRequiredErrorMessage"];
            if (!l.ResourceNotFound)
            {
                ErrorMessage = l.Value;
            }
        }
        if (propertyValue == null)
        {
            results.Add(GetValidationResult(context));
        }
        else if (propertyValue is string val)
        {
            if (!AllowEmptyString && val == string.Empty)
            {
                results.Add(GetValidationResult(context));
            }
        }
        else if (propertyValue is IEnumerable v)
        {
            var enumerator = v.GetEnumerator();
            var valid = enumerator.MoveNext();
            if (!valid)
            {
                results.Add(GetValidationResult(context));
            }
        }
        else if (propertyValue is DateTimeRangeValue dv && dv is { NullStart: null, NullEnd: null })
        {
            results.Add(GetValidationResult(context));
        }
    }

    private ValidationResult GetValidationResult(ValidationContext context)
    {
        var errorMessage = GetLocalizerErrorMessage(context, LocalizerFactory, Options);
        return context.GetValidationResult(errorMessage);
    }

    protected virtual string GetRuleKey() => GetType().Name.Split(".").Last().Replace("Validator", "");

    protected virtual string? GetLocalizerErrorMessage(ValidationContext context, IStringLocalizerFactory? localizerFactory = null, JsonLocalizationOptions? options = null)
    {
        var errorMessage = ErrorMessage;
        if (!string.IsNullOrEmpty(context.MemberName) && !string.IsNullOrEmpty(errorMessage))
        {
            var memberName = context.MemberName;

            if (localizerFactory != null)
            {
                var isResx = false;
                if (options is { ResourceManagerStringLocalizerType: not null })
                {
                    var localizer = localizerFactory.Create(options.ResourceManagerStringLocalizerType);
                    if (localizer.TryGetLocalizerString(errorMessage, out var resx))
                    {
                        errorMessage = resx;
                        isResx = true;
                    }
                }

                if (!isResx && localizerFactory.Create(context.ObjectType).TryGetLocalizerString($"{memberName}.{GetRuleKey()}", out var msg))
                {
                    errorMessage = msg;
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                var displayName = new FieldIdentifier(context.ObjectInstance, context.MemberName).GetDisplayName();
                errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, displayName);
            }
        }
        return errorMessage;
    }
}
