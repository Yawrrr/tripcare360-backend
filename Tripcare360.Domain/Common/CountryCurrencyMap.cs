using Tripcare360.Domain.Enums;

namespace Tripcare360.Domain.Common;

public static class CountryCurrencyMap
{
    private static readonly Dictionary<Country, Currency> Map = new()
    {
        { Country.Malaysia,    Currency.MYR },
        { Country.Philippines, Currency.PHP },
        { Country.Indonesia,   Currency.IDR },
        { Country.Cambodia,    Currency.KHR },
    };

    private static readonly Dictionary<Currency, string> Symbols = new()
    {
        { Currency.MYR, "RM" },
        { Currency.PHP, "₱" },
        { Currency.IDR, "Rp" },
        { Currency.KHR, "₭" },
    };

    public static Currency GetCurrency(Country country)
        => Map.TryGetValue(country, out var c) ? c : Currency.MYR;

    public static string GetSymbol(Currency currency)
        => Symbols.TryGetValue(currency, out var s) ? s : "";
}
