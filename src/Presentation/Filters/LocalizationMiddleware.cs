using System.Globalization;

namespace Presentation.Filters;

public class LocalizationMiddleware : IMiddleware
{
    private static CultureInfo[] AcceptableCultures { get; } =
    [
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US")
    ];

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cultureKey = context.Request.Headers["Accept-Language"];

        CultureInfo culture;

        if (!string.IsNullOrEmpty(cultureKey) && DoesCultureAcceptable(cultureKey!))
        {
            culture = new CultureInfo(cultureKey!);
        }
        else
        {
            culture = new CultureInfo("en-US");
        }

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        await next(context);
    }

    private static bool DoesCultureAcceptable(string cultureName)
    {
        return Array.Exists(AcceptableCultures, culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
    }
}
