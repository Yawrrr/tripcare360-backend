using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Benefits;

public class BenefitLimitsService : IBenefitLimitsService
{
    private const decimal Unlimited = 9_999_999m;

    // ─── Currency symbols per country ────────────────────────────────────────
    private static readonly Dictionary<string, string> CurrencySymbols = new()
    {
        { "my",  "RM"  },
        { "ph",  "₱"   },
        { "idn", "Rp"  },
        { "kh",  "KHR" },
    };

    private static string SymbolFor(string country) =>
        CurrencySymbols.TryGetValue(country.ToLowerInvariant(), out var s) ? s : "RM";

    // ════════════════════════════════════════════════════════════════════════
    // ─── MALAYSIA (RM) ──────────────────────────────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

    private static readonly Dictionary<(TravelRoute, PolicyTier, ClaimType), decimal> MyLimits = new()
    {
        // Domestic
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
        // International Silver
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
        // International Gold
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
        // International Platinum
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

    private static readonly Dictionary<(PolicyTier, int MaxAge), decimal> MyIntlMedicalByAge = new()
    {
        { (PolicyTier.Silver,   70), 100_000 }, { (PolicyTier.Silver,   80), 50_000 },
        { (PolicyTier.Gold,     70), 200_000 }, { (PolicyTier.Gold,     80), 100_000 },
        { (PolicyTier.Platinum, 70), 300_000 }, { (PolicyTier.Platinum, 80), 150_000 },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> MyFlightDelayRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (100, 2, 200) },
        { (TravelRoute.International, PolicyTier.Silver),   (200, 3, 1_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (200, 3, 1_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (200, 3, 2_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> MyConfinementRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (150, 20, 3_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (200, 30, 6_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (300, 30, 9_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (500, 30, 15_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> MyHijackRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (300, 10, 3_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (500, 10, 5_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (500, 10, 5_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (1_000, 10, 10_000) },
    };

    // ════════════════════════════════════════════════════════════════════════
    // ─── PHILIPPINES (₱) ────────────────────────────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

    private static readonly Dictionary<(TravelRoute, PolicyTier, ClaimType), decimal> PhLimits = new()
    {
        // Domestic
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 250_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            250_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          25_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       2_500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        15_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           15_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            15_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                1_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),              2_500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           5_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        15_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   2_500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 5_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          2_500 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        1_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          1_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        2_500_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      2_500_000 },
        // International Silver
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 500_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            750_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          25_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        30_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           25_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            25_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),              5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           10_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        25_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   5_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 15_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          2_500 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        2_500 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          1_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        2_500_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      2_500_000 },
        // International Gold
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 1_500_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            1_500_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          50_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       10_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        45_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           50_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            50_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                5_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),              10_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           15_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        25_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   10_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 25_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          5_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        5_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          5_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        5_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      2_500_000 },
        // International Platinum
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 2_500_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            2_500_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          75_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        75_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           75_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            75_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                10_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),              15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           25_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        50_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   15_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 40_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          10_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        10_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          10_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      2_500_000 },
    };

    private static readonly Dictionary<(PolicyTier, int MaxAge), decimal> PhIntlMedicalByAge = new()
    {
        { (PolicyTier.Silver,   70), 500_000 },   { (PolicyTier.Silver,   80), 250_000 },
        { (PolicyTier.Gold,     70), 1_000_000 }, { (PolicyTier.Gold,     80), 500_000 },
        { (PolicyTier.Platinum, 70), 1_500_000 }, { (PolicyTier.Platinum, 80), 750_000 },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> PhFlightDelayRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (500, 2, 1_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (1_000, 3, 5_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (1_000, 3, 5_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (1_000, 3, 10_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> PhConfinementRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (750, 20, 15_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (1_000, 30, 30_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (1_500, 30, 45_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (2_500, 30, 75_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> PhHijackRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (1_500, 10, 15_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (2_500, 10, 25_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (2_500, 10, 25_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (5_000, 10, 50_000) },
    };

    // ════════════════════════════════════════════════════════════════════════
    // ─── INDONESIA (Rp) ─────────────────────────────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

    private static readonly Dictionary<(TravelRoute, PolicyTier, ClaimType), decimal> IdnLimits = new()
    {
        // Domestic
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 150_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            150_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          15_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       2_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        10_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           10_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            10_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                600_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),              2_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           4_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        10_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   2_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 4_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          2_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        750_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          500_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        1_500_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      1_500_000_000 },
        // International Silver
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 300_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            450_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          15_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       4_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        21_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           20_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            20_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                3_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),              4_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           8_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        20_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   4_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 12_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          2_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        2_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          500_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        1_500_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      1_500_000_000 },
        // International Gold
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 900_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            900_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          30_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       8_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        30_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           40_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            40_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                3_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),              8_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           12_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        20_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   8_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 20_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          4_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        4_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          2_500_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        3_000_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      1_500_000_000 },
        // International Platinum
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 1_500_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            1_500_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          45_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       12_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        45_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           60_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            60_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                8_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),              12_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           20_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        40_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   12_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 32_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          8_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        8_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          5_000_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      1_500_000_000 },
    };

    private static readonly Dictionary<(PolicyTier, int MaxAge), decimal> IdnIntlMedicalByAge = new()
    {
        { (PolicyTier.Silver,   70), 300_000_000 }, { (PolicyTier.Silver,   80), 150_000_000 },
        { (PolicyTier.Gold,     70), 600_000_000 }, { (PolicyTier.Gold,     80), 300_000_000 },
        { (PolicyTier.Platinum, 70), 900_000_000 }, { (PolicyTier.Platinum, 80), 450_000_000 },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> IdnFlightDelayRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (300_000, 2, 600_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (600_000, 3, 3_000_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (600_000, 3, 3_000_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (600_000, 3, 8_000_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> IdnConfinementRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (500_000, 20, 10_000_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (700_000, 30, 21_000_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (1_000_000, 30, 30_000_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (1_500_000, 30, 45_000_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> IdnHijackRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (1_000_000, 10, 10_000_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (2_000_000, 10, 20_000_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (2_000_000, 10, 20_000_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (4_000_000, 10, 40_000_000) },
    };

    // ════════════════════════════════════════════════════════════════════════
    // ─── CAMBODIA (KHR) ─────────────────────────────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

    private static readonly Dictionary<(TravelRoute, PolicyTier, ClaimType), decimal> KhLimits = new()
    {
        // Domestic
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 80_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            80_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          8_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       800_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        5_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           8_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            8_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                800_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),              1_600_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           3_200_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        8_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   1_600_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 3_200_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          1_600_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        640_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          400_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        1_200_000_000 },
        { (TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      1_200_000_000 },
        // International Silver
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 160_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            240_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          8_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       3_200_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        9_600_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           16_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            16_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                3_200_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),              3_200_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           6_400_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        16_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   3_200_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 9_600_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          1_600_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        1_600_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          400_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        1_200_000_000 },
        { (TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      1_200_000_000 },
        // International Gold
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 480_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            480_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          16_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       6_400_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        14_400_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           32_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            32_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                3_200_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),              6_400_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           9_600_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        16_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   6_400_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 16_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          3_200_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        3_200_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          2_000_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        2_400_000_000 },
        { (TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      1_200_000_000 },
        // International Platinum
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 800_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            800_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          24_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       9_600_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        19_200_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           48_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            48_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                6_400_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),              9_600_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           16_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        32_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   9_600_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 25_600_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          6_400_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        6_400_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          4_000_000_000 },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      1_200_000_000 },
    };

    private static readonly Dictionary<(PolicyTier, int MaxAge), decimal> KhIntlMedicalByAge = new()
    {
        { (PolicyTier.Silver,   70), 160_000_000 }, { (PolicyTier.Silver,   80), 80_000_000 },
        { (PolicyTier.Gold,     70), 320_000_000 }, { (PolicyTier.Gold,     80), 160_000_000 },
        { (PolicyTier.Platinum, 70), 480_000_000 }, { (PolicyTier.Platinum, 80), 240_000_000 },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> KhFlightDelayRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (400_000, 2, 800_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (640_000, 3, 3_200_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (640_000, 3, 3_200_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (640_000, 3, 6_400_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> KhConfinementRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (250_000, 20, 5_000_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (320_000, 30, 9_600_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (480_000, 30, 14_400_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (640_000, 30, 19_200_000) },
    };

    private static readonly Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> KhHijackRates = new()
    {
        { (TravelRoute.Domestic,      PolicyTier.None),     (800_000, 10, 8_000_000) },
        { (TravelRoute.International, PolicyTier.Silver),   (1_600_000, 10, 16_000_000) },
        { (TravelRoute.International, PolicyTier.Gold),     (1_600_000, 10, 16_000_000) },
        { (TravelRoute.International, PolicyTier.Platinum), (3_200_000, 10, 32_000_000) },
    };

    // ════════════════════════════════════════════════════════════════════════
    // ─── Claim type display metadata (shared) ───────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

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

    // ════════════════════════════════════════════════════════════════════════
    // ─── Country dispatch helpers ────────────────────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

    private static Dictionary<(TravelRoute, PolicyTier, ClaimType), decimal> LimitsFor(string country) =>
        country.ToLowerInvariant() switch
        {
            "ph"  => PhLimits,
            "idn" => IdnLimits,
            "kh"  => KhLimits,
            _     => MyLimits,
        };

    private static Dictionary<(PolicyTier, int MaxAge), decimal> IntlMedicalByAgeFor(string country) =>
        country.ToLowerInvariant() switch
        {
            "ph"  => PhIntlMedicalByAge,
            "idn" => IdnIntlMedicalByAge,
            "kh"  => KhIntlMedicalByAge,
            _     => MyIntlMedicalByAge,
        };

    private static Dictionary<(TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> FlightDelayRatesFor(string country) =>
        country.ToLowerInvariant() switch
        {
            "ph"  => PhFlightDelayRates,
            "idn" => IdnFlightDelayRates,
            "kh"  => KhFlightDelayRates,
            _     => MyFlightDelayRates,
        };

    private static Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> ConfinementRatesFor(string country) =>
        country.ToLowerInvariant() switch
        {
            "ph"  => PhConfinementRates,
            "idn" => IdnConfinementRates,
            "kh"  => KhConfinementRates,
            _     => MyConfinementRates,
        };

    private static Dictionary<(TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> HijackRatesFor(string country) =>
        country.ToLowerInvariant() switch
        {
            "ph"  => PhHijackRates,
            "idn" => IdnHijackRates,
            "kh"  => KhHijackRates,
            _     => MyHijackRates,
        };

    // ════════════════════════════════════════════════════════════════════════
    // ─── IBenefitLimitsService implementation ───────────────────────────────
    // ════════════════════════════════════════════════════════════════════════

    public decimal GetMaxPayout(TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge = 0, string country = "")
    {
        if (type == ClaimType.MedicalExpenses && route == TravelRoute.International && insuredAge > 60)
        {
            int bracket = insuredAge <= 70 ? 70 : 80;
            if (IntlMedicalByAgeFor(country).TryGetValue((tier, bracket), out var agePayout))
                return agePayout;
        }
        return LimitsFor(country).TryGetValue((route, tier, type), out var payout) ? payout : 0;
    }

    public (decimal RatePerBlock, decimal BlockSizeHours, decimal MaxPayout) GetFlightDelayRate(TravelRoute route, PolicyTier tier, string country = "")
        => FlightDelayRatesFor(country).TryGetValue((route, tier), out var r) ? r : (0, 1, 0);

    public (decimal DailyRate, int MaxDays, decimal MaxPayout) GetConfinementRate(TravelRoute route, PolicyTier tier, string country = "")
        => ConfinementRatesFor(country).TryGetValue((route, tier), out var r) ? r : (0, 0, 0);

    public (decimal DailyRate, int MaxDays, decimal MaxPayout) GetHijackRate(TravelRoute route, PolicyTier tier, string country = "")
        => HijackRatesFor(country).TryGetValue((route, tier), out var r) ? r : (0, 0, 0);

    public IReadOnlyList<BenefitItemDto> GetAllBenefits(TravelRoute route, PolicyTier tier, int insuredAge = 0, string country = "")
    {
        var sym = SymbolFor(country);
        var result = new List<BenefitItemDto>();

        foreach (var claimType in Enum.GetValues<ClaimType>())
        {
            if (!TypeMeta.TryGetValue(claimType, out var meta))
                continue;

            var maxPayout = GetMaxPayout(route, tier, claimType, insuredAge, country);
            var notes = BuildNotes(route, tier, claimType, insuredAge, maxPayout, sym, country);

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

    private string BuildNotes(TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge, decimal maxPayout, string sym, string country)
    {
        if (type == ClaimType.FlightDelay)
        {
            var (rate, block, _) = GetFlightDelayRate(route, tier, country);
            return $"{sym} {rate:N0}/per {block}-hr block";
        }

        if (type == ClaimType.HospitalConfinement)
        {
            var (daily, maxDays, _) = GetConfinementRate(route, tier, country);
            return $"{sym} {daily:N0}/day, max {maxDays} days";
        }

        if (type == ClaimType.HijackInconvenience)
        {
            var (daily, maxDays, _) = GetHijackRate(route, tier, country);
            return $"{sym} {daily:N0}/day, max {maxDays} days";
        }

        if (type == ClaimType.MedicalExpenses && route == TravelRoute.International)
        {
            var r61 = GetMaxPayout(route, tier, type, 61, country);
            var max71 = GetMaxPayout(route, tier, type, 71, country);
            return $"Age 61-70: {sym} {r61:N0}; Age 71-80: {sym} {max71:N0}";
        }

        if (type == ClaimType.EmergencyEvacuation && maxPayout >= Unlimited)
            return "Unlimited";

        return string.Empty;
    }
}
