using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Benefits;

public class BenefitLimitsService : IBenefitLimitsService
{
    private const decimal Unlimited = 9_999_999m;

    // Flat max payouts keyed by (route, tier, claimType).
    // MedicalExpenses age brackets are handled separately in GetMaxPayout.
    private static readonly Dictionary<(TravelRoute, PolicyTier, ClaimType), decimal> Limits = new()
    {
        // ── Domestic ────────────────────────────────────────────────────────────
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 50_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            50_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          5_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        3_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           3_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            3_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                200 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),              500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           1_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        3_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 1_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        200 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          200_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        500_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      500_000 },

        // ── International Silver ─────────────────────────────────────────────────
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 100_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            150_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       1_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        6_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                1_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),              1_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           2_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   1_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 3_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          500 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        500 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          200_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        500_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      500_000 },

        // ── International Gold ───────────────────────────────────────────────────
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 300_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            300_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          10_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       2_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        9_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           10_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            10_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                1_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),              2_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           3_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        5_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   2_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 5_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          1_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        1_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          1_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        1_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      500_000 },

        // ── International Platinum ───────────────────────────────────────────────
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 500_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            500_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       3_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                2_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),              3_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           5_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        10_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   3_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 8_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          2_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        2_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          2_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      500_000 },
    };

    // Age-based medical limits for international only (age bracket 61-70 and 71-80).
    // Domestic MedicalExpenses is a flat limit (no brackets).
    private static readonly Dictionary<(PolicyTier, int MaxAge), decimal> IntlMedicalByAge = new()
    {
        { (PolicyTier.Silver,   70), 100_000 },
        { (PolicyTier.Silver,   80), 50_000 },
        { (PolicyTier.Gold,     70), 200_000 },
        { (PolicyTier.Gold,     80), 100_000 },
        { (PolicyTier.Platinum, 70), 300_000 },
        { (PolicyTier.Platinum, 80), 150_000 },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> FlightDelayRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (100, 2, 200) },
        { (TravelRoute.International, PolicyTier.Silver),   (200, 3, 1_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (200, 3, 1_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (200, 3, 2_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> ConfinementRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (150, 20, 3_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (200, 30, 6_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (300, 30, 9_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (500, 30, 15_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> HijackRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (300, 10, 3_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (500, 10, 5_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (500, 10, 5_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (1_000, 10, 10_000) },
    };

    private static readonly Dictionary<ClaimType, (string DisplayName, string Category)> TypeMeta = new()
    {
        { ClaimType.DeathOrPermanentDisability,    ("Death or Permanent Disability",   nameof(ClaimCategory.PersonalAccident)) },
        { ClaimType.MedicalExpenses,               ("Medical Expenses",                nameof(ClaimCategory.MedicalAndExpenses)) },
        { ClaimType.FollowUpTreatment,             ("Follow-Up Treatment",             nameof(ClaimCategory.MedicalAndExpenses)) },
        { ClaimType.AlternativeTreatment,          ("Alternative Treatment",           nameof(ClaimCategory.MedicalAndExpenses)) },
        { ClaimType.HospitalConfinement,           ("Hospital Confinement",            nameof(ClaimCategory.MedicalAndExpenses)) },
        { ClaimType.TripCancellation,              ("Trip Cancellation",               nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.TripCurtailment,               ("Trip Curtailment",                nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.FlightDelay,                   ("Flight Delay",                    nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.BaggageDelay,                  ("Baggage Delay",                   nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.MissedConnection,              ("Missed Connection",               nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.HijackInconvenience,           ("Hijack Inconvenience",            nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.HomeCare,                      ("Home Care",                       nameof(ClaimCategory.TravelInconveniences)) },
        { ClaimType.BaggageLossAndPersonalEffects, ("Baggage Loss & Personal Effects", nameof(ClaimCategory.PersonalBelongings)) },
        { ClaimType.PersonalMoneyLoss,             ("Personal Money Loss",             nameof(ClaimCategory.PersonalBelongings)) },
        { ClaimType.TravelDocumentsLoss,           ("Travel Documents Loss",           nameof(ClaimCategory.PersonalBelongings)) },
        { ClaimType.PersonalLiability,             ("Personal Liability",              nameof(ClaimCategory.Liability)) },
        { ClaimType.EmergencyEvacuation,           ("Emergency Evacuation",            nameof(ClaimCategory.EmergencyServices)) },
        { ClaimType.RepatriationOfRemains,         ("Repatriation of Remains",        nameof(ClaimCategory.EmergencyServices)) },
    };

    public decimal GetMaxPayout(TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge = 0)
    {
        if (type == ClaimType.MedicalExpenses && route == TravelRoute.International && insuredAge > 60)
        {
            int bracket = insuredAge <= 70 ? 70 : 80;
            if (IntlMedicalByAge.TryGetValue((tier, bracket), out var agePayout))
                return agePayout;
        }

        return Limits.TryGetValue((route, tier, type), out var payout) ? payout : 0;
    }

    public (decimal RatePerBlock, decimal BlockSizeHours, decimal MaxPayout) GetFlightDelayRate(TravelRoute route, PolicyTier tier)
        => FlightDelayRates.TryGetValue((route, tier), out var r) ? r : (0, 1, 0);

    public (decimal DailyRate, int MaxDays, decimal MaxPayout) GetConfinementRate(TravelRoute route, PolicyTier tier)
        => ConfinementRates.TryGetValue((route, tier), out var r) ? r : (0, 0, 0);

    public (decimal DailyRate, int MaxDays, decimal MaxPayout) GetHijackRate(TravelRoute route, PolicyTier tier)
        => HijackRates.TryGetValue((route, tier), out var r) ? r : (0, 0, 0);

    public IReadOnlyList<BenefitItemDto> GetAllBenefits(TravelRoute route, PolicyTier tier, int insuredAge = 0)
    {
        var result = new List<BenefitItemDto>();

        foreach (var claimType in Enum.GetValues<ClaimType>())
        {
            if (!TypeMeta.TryGetValue(claimType, out var meta))
                continue;

            var maxPayout = GetMaxPayout(route, tier, claimType, insuredAge);
            var notes = BuildNotes(route, tier, claimType, insuredAge, maxPayout);

            result.Add(new BenefitItemDto(
                Category: meta.Category,
                ClaimType: claimType.ToString(),
                DisplayName: meta.DisplayName,
                MaxPayout: maxPayout,
                Notes: notes
            ));
        }

        return result;
    }

    private string BuildNotes(TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge, decimal maxPayout)
    {
        if (type == ClaimType.FlightDelay)
        {
            var (rate, block, _) = GetFlightDelayRate(route, tier);
            return $"RM{rate:0}/per {block}-hr block";
        }

        if (type == ClaimType.HospitalConfinement)
        {
            var (daily, maxDays, _) = GetConfinementRate(route, tier);
            return $"RM{daily:0}/day, max {maxDays} days";
        }

        if (type == ClaimType.HijackInconvenience)
        {
            var (daily, maxDays, _) = GetHijackRate(route, tier);
            return $"RM{daily:0}/day, max {maxDays} days";
        }

        if (type == ClaimType.MedicalExpenses && route == TravelRoute.International)
        {
            var (r61, _, max61) = (GetMaxPayout(route, tier, type, 61), 0, GetMaxPayout(route, tier, type, 61));
            var max71 = GetMaxPayout(route, tier, type, 71);
            return $"Age 61-70: RM{r61:N0}; Age 71-80: RM{max71:N0}";
        }

        if (type == ClaimType.EmergencyEvacuation && maxPayout >= Unlimited)
            return "Unlimited";

        return string.Empty;
    }
}
