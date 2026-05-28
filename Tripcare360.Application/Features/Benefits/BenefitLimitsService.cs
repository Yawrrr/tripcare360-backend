using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Common;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Benefits;

public class BenefitLimitsService : IBenefitLimitsService
{
    private const decimal Unlimited = 9_999_999m;

    // ── Country-specific claim type availability ─────────────────────────────
    private static readonly Dictionary<Country, HashSet<ClaimType>> UnavailableTypes = new()
    {
        { Country.Malaysia,    new HashSet<ClaimType>() },
        { Country.Philippines, new HashSet<ClaimType>() },
        { Country.Indonesia,   new HashSet<ClaimType>() },
        { Country.Cambodia,    new HashSet<ClaimType> { ClaimType.HomeCare, ClaimType.ExtendedHomeCare } },
    };

    // ── Flat max payouts keyed by (Country, TravelRoute, PolicyTier, ClaimType) ─
    private static readonly Dictionary<(Country, TravelRoute, PolicyTier, ClaimType), decimal> Limits = new()
    {
        // ════════════════════════════════════════════════════════
        // MALAYSIA (MYR)
        // ════════════════════════════════════════════════════════
        // Domestic
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 50_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            50_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          5_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       500 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        3_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           3_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            3_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                200 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),               500 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           1_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        3_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   500 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 1_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          500 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        200 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          200_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        500_000 },
        { (Country.Malaysia, TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      500_000 },
        // International Silver
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 100_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            150_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        6_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),               1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 3_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          500 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        500 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          200_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        500_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      500_000 },
        // International Gold
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 300_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            300_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          10_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        9_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           10_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            10_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),               2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           3_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        1_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          1_000_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        1_000_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      500_000 },
        // International Platinum
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 500_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            500_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          15_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       3_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        15_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           15_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            15_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),               3_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           5_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        10_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   3_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 8_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        2_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          2_000_000 },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (Country.Malaysia, TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      500_000 },

        // ════════════════════════════════════════════════════════
        // PHILIPPINES (PHP — approx ×12 from MYR)
        // ════════════════════════════════════════════════════════
        // Domestic
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 600_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            600_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          60_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       6_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        36_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           36_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            36_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                2_500 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),               6_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           12_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        36_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   6_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 12_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          6_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        2_500 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          2_400_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        6_000_000 },
        { (Country.Philippines, TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      6_000_000 },
        // International Silver
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 1_200_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            1_800_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        72_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),               12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 36_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          6_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        6_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          2_400_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        6_000_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      6_000_000 },
        // International Gold
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 3_600_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            3_600_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          120_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        108_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           120_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            120_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),               24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           36_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          12_000_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        12_000_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      6_000_000 },
        // International Platinum
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 6_000_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            6_000_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          180_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       36_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        180_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           180_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            180_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),               36_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           60_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        120_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   36_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 96_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          24_000_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      6_000_000 },

        // ════════════════════════════════════════════════════════
        // INDONESIA (IDR — approx ×3,500 from MYR)
        // ════════════════════════════════════════════════════════
        // Domestic
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 175_000_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            175_000_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          17_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       1_750_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        10_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           10_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            10_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                700_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),               1_750_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           3_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        10_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   1_750_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 3_500_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          1_750_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        700_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          700_000_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        1_750_000_000 },
        { (Country.Indonesia, TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      1_750_000_000 },
        // International Silver
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 350_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            525_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        21_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),               3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 10_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          1_750_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        1_750_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          700_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        1_750_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      1_750_000_000 },
        // International Gold
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 1_050_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            1_050_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          35_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        31_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           35_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            35_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),               7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           10_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        3_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          3_500_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        3_500_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      1_750_000_000 },
        // International Platinum
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 1_750_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            1_750_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          52_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       10_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        52_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           52_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            52_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),               10_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           17_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        35_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   10_500_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 28_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        7_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          7_000_000_000 },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (Country.Indonesia, TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      1_750_000_000 },

        // ════════════════════════════════════════════════════════
        // CAMBODIA (KHR — approx ×1,000 from MYR)
        // ════════════════════════════════════════════════════════
        // Domestic
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.DeathOrPermanentDisability), 50_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.MedicalExpenses),            50_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.FollowUpTreatment),          5_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.AlternativeTreatment),       500_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HospitalConfinement),        3_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCancellation),           3_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TripCurtailment),            3_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.FlightDelay),                200_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageDelay),               500_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.MissedConnection),           1_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HijackInconvenience),        3_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.HomeCare),                   500_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.BaggageLossAndPersonalEffects), 1_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalMoneyLoss),          500_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.TravelDocumentsLoss),        200_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.PersonalLiability),          200_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.EmergencyEvacuation),        500_000_000 },
        { (Country.Cambodia, TravelRoute.Domestic, PolicyTier.None, ClaimType.RepatriationOfRemains),      500_000_000 },
        // International Silver
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.DeathOrPermanentDisability), 100_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.MedicalExpenses),            150_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.FollowUpTreatment),          5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.AlternativeTreatment),       1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.HospitalConfinement),        6_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCancellation),           5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.TripCurtailment),            5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.FlightDelay),                1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageDelay),               1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.MissedConnection),           2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.HijackInconvenience),        5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.HomeCare),                   1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.BaggageLossAndPersonalEffects), 3_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalMoneyLoss),          500_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.TravelDocumentsLoss),        500_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.PersonalLiability),          200_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.EmergencyEvacuation),        500_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Silver, ClaimType.RepatriationOfRemains),      500_000_000 },
        // International Gold
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.DeathOrPermanentDisability), 300_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.MedicalExpenses),            300_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.FollowUpTreatment),          10_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.AlternativeTreatment),       2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.HospitalConfinement),        9_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCancellation),           10_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.TripCurtailment),            10_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.FlightDelay),                1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageDelay),               2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.MissedConnection),           3_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.HijackInconvenience),        5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.HomeCare),                   2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.BaggageLossAndPersonalEffects), 5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalMoneyLoss),          1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.TravelDocumentsLoss),        1_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.PersonalLiability),          1_000_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.EmergencyEvacuation),        1_000_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Gold, ClaimType.RepatriationOfRemains),      500_000_000 },
        // International Platinum
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.DeathOrPermanentDisability), 500_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.MedicalExpenses),            500_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.FollowUpTreatment),          15_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.AlternativeTreatment),       3_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HospitalConfinement),        15_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCancellation),           15_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TripCurtailment),            15_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.FlightDelay),                2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageDelay),               3_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.MissedConnection),           5_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HijackInconvenience),        10_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.HomeCare),                   3_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.BaggageLossAndPersonalEffects), 8_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalMoneyLoss),          2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.TravelDocumentsLoss),        2_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.PersonalLiability),          2_000_000_000 },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.EmergencyEvacuation),        Unlimited },
        { (Country.Cambodia, TravelRoute.International, PolicyTier.Platinum, ClaimType.RepatriationOfRemains),      500_000_000 },

        // ════════════════════════════════════════════════════════
        // OPTIONAL BENEFITS — AdventurousActivities (= MedicalExpenses limits)
        // ════════════════════════════════════════════════════════
        { (Country.Malaysia,    TravelRoute.Domestic,      PolicyTier.None,     ClaimType.AdventurousActivities), 50_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Silver,   ClaimType.AdventurousActivities), 150_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Gold,     ClaimType.AdventurousActivities), 300_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Platinum, ClaimType.AdventurousActivities), 500_000 },
        { (Country.Philippines, TravelRoute.Domestic,      PolicyTier.None,     ClaimType.AdventurousActivities), 600_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver,   ClaimType.AdventurousActivities), 1_800_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold,     ClaimType.AdventurousActivities), 3_600_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.AdventurousActivities), 6_000_000 },
        { (Country.Indonesia,   TravelRoute.Domestic,      PolicyTier.None,     ClaimType.AdventurousActivities), 175_000_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Silver,   ClaimType.AdventurousActivities), 525_000_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Gold,     ClaimType.AdventurousActivities), 1_050_000_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Platinum, ClaimType.AdventurousActivities), 1_750_000_000 },
        { (Country.Cambodia,    TravelRoute.Domestic,      PolicyTier.None,     ClaimType.AdventurousActivities), 50_000_000 },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Silver,   ClaimType.AdventurousActivities), 150_000_000 },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Gold,     ClaimType.AdventurousActivities), 300_000_000 },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Platinum, ClaimType.AdventurousActivities), 500_000_000 },

        // OPTIONAL BENEFITS — GolfCover
        { (Country.Malaysia,    TravelRoute.Domestic,      PolicyTier.None,     ClaimType.GolfCover), 1_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Silver,   ClaimType.GolfCover), 1_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Gold,     ClaimType.GolfCover), 2_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Platinum, ClaimType.GolfCover), 2_000 },
        { (Country.Philippines, TravelRoute.Domestic,      PolicyTier.None,     ClaimType.GolfCover), 12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver,   ClaimType.GolfCover), 12_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold,     ClaimType.GolfCover), 24_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.GolfCover), 24_000 },
        { (Country.Indonesia,   TravelRoute.Domestic,      PolicyTier.None,     ClaimType.GolfCover), 3_500_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Silver,   ClaimType.GolfCover), 3_500_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Gold,     ClaimType.GolfCover), 7_000_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Platinum, ClaimType.GolfCover), 7_000_000 },
        { (Country.Cambodia,    TravelRoute.Domestic,      PolicyTier.None,     ClaimType.GolfCover), 1_000_000 },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Silver,   ClaimType.GolfCover), 1_000_000 },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Gold,     ClaimType.GolfCover), 2_000_000 },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Platinum, ClaimType.GolfCover), 2_000_000 },

        // OPTIONAL BENEFITS — ExtendedHomeCare (international only; Cambodia excluded via UnavailableTypes)
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Silver,   ClaimType.ExtendedHomeCare), 20_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Gold,     ClaimType.ExtendedHomeCare), 20_000 },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Platinum, ClaimType.ExtendedHomeCare), 20_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver,   ClaimType.ExtendedHomeCare), 240_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold,     ClaimType.ExtendedHomeCare), 240_000 },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum, ClaimType.ExtendedHomeCare), 240_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Silver,   ClaimType.ExtendedHomeCare), 70_000_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Gold,     ClaimType.ExtendedHomeCare), 70_000_000 },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Platinum, ClaimType.ExtendedHomeCare), 70_000_000 },
    };

    // Age-based medical limits for international only.
    // Key: (Country, PolicyTier, MaxAge) where MaxAge is 70 or 80.
    private static readonly Dictionary<(Country, PolicyTier, int), decimal> IntlMedicalByAge = new()
    {
        { (Country.Malaysia,    PolicyTier.Silver,   70), 100_000 },       { (Country.Malaysia,    PolicyTier.Silver,   80), 50_000 },
        { (Country.Malaysia,    PolicyTier.Gold,     70), 200_000 },       { (Country.Malaysia,    PolicyTier.Gold,     80), 100_000 },
        { (Country.Malaysia,    PolicyTier.Platinum, 70), 300_000 },       { (Country.Malaysia,    PolicyTier.Platinum, 80), 150_000 },
        { (Country.Philippines, PolicyTier.Silver,   70), 1_000_000 },     { (Country.Philippines, PolicyTier.Silver,   80), 500_000 },
        { (Country.Philippines, PolicyTier.Gold,     70), 2_000_000 },     { (Country.Philippines, PolicyTier.Gold,     80), 1_000_000 },
        { (Country.Philippines, PolicyTier.Platinum, 70), 3_000_000 },     { (Country.Philippines, PolicyTier.Platinum, 80), 1_500_000 },
        { (Country.Indonesia,   PolicyTier.Silver,   70), 350_000_000 },   { (Country.Indonesia,   PolicyTier.Silver,   80), 175_000_000 },
        { (Country.Indonesia,   PolicyTier.Gold,     70), 700_000_000 },   { (Country.Indonesia,   PolicyTier.Gold,     80), 350_000_000 },
        { (Country.Indonesia,   PolicyTier.Platinum, 70), 1_050_000_000 }, { (Country.Indonesia,   PolicyTier.Platinum, 80), 525_000_000 },
        { (Country.Cambodia,    PolicyTier.Silver,   70), 100_000_000 },   { (Country.Cambodia,    PolicyTier.Silver,   80), 50_000_000 },
        { (Country.Cambodia,    PolicyTier.Gold,     70), 200_000_000 },   { (Country.Cambodia,    PolicyTier.Gold,     80), 100_000_000 },
        { (Country.Cambodia,    PolicyTier.Platinum, 70), 300_000_000 },   { (Country.Cambodia,    PolicyTier.Platinum, 80), 150_000_000 },
    };

    private static readonly Dictionary<(Country, TravelRoute, PolicyTier), (decimal Rate, decimal Block, decimal Max)> FlightDelayRates = new()
    {
        { (Country.Malaysia,    TravelRoute.Domestic,      PolicyTier.None),     (100, 2, 200) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Silver),   (200, 3, 1_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Gold),     (200, 3, 1_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Platinum), (200, 3, 2_000) },
        { (Country.Philippines, TravelRoute.Domestic,      PolicyTier.None),     (1_200, 2, 2_500) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver),   (2_400, 3, 12_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold),     (2_400, 3, 12_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum), (2_400, 3, 24_000) },
        { (Country.Indonesia,   TravelRoute.Domestic,      PolicyTier.None),     (350_000, 2, 700_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Silver),   (700_000, 3, 3_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Gold),     (700_000, 3, 3_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Platinum), (700_000, 3, 7_000_000) },
        { (Country.Cambodia,    TravelRoute.Domestic,      PolicyTier.None),     (100_000, 2, 200_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Silver),   (200_000, 3, 1_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Gold),     (200_000, 3, 1_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Platinum), (200_000, 3, 2_000_000) },
    };

    private static readonly Dictionary<(Country, TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> ConfinementRates = new()
    {
        { (Country.Malaysia,    TravelRoute.Domestic,      PolicyTier.None),     (150, 20, 3_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Silver),   (200, 30, 6_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Gold),     (300, 30, 9_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Platinum), (500, 30, 15_000) },
        { (Country.Philippines, TravelRoute.Domestic,      PolicyTier.None),     (1_800, 20, 36_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver),   (2_400, 30, 72_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold),     (3_600, 30, 108_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum), (6_000, 30, 180_000) },
        { (Country.Indonesia,   TravelRoute.Domestic,      PolicyTier.None),     (525_000, 20, 10_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Silver),   (700_000, 30, 21_000_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Gold),     (1_050_000, 30, 31_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Platinum), (1_750_000, 30, 52_500_000) },
        { (Country.Cambodia,    TravelRoute.Domestic,      PolicyTier.None),     (150_000, 20, 3_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Silver),   (200_000, 30, 6_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Gold),     (300_000, 30, 9_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Platinum), (500_000, 30, 15_000_000) },
    };

    private static readonly Dictionary<(Country, TravelRoute, PolicyTier), (decimal Daily, int MaxDays, decimal Max)> HijackRates = new()
    {
        { (Country.Malaysia,    TravelRoute.Domestic,      PolicyTier.None),     (300, 10, 3_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Silver),   (500, 10, 5_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Gold),     (500, 10, 5_000) },
        { (Country.Malaysia,    TravelRoute.International, PolicyTier.Platinum), (1_000, 10, 10_000) },
        { (Country.Philippines, TravelRoute.Domestic,      PolicyTier.None),     (3_600, 10, 36_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Silver),   (6_000, 10, 60_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Gold),     (6_000, 10, 60_000) },
        { (Country.Philippines, TravelRoute.International, PolicyTier.Platinum), (12_000, 10, 120_000) },
        { (Country.Indonesia,   TravelRoute.Domestic,      PolicyTier.None),     (1_050_000, 10, 10_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Silver),   (1_750_000, 10, 17_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Gold),     (1_750_000, 10, 17_500_000) },
        { (Country.Indonesia,   TravelRoute.International, PolicyTier.Platinum), (3_500_000, 10, 35_000_000) },
        { (Country.Cambodia,    TravelRoute.Domestic,      PolicyTier.None),     (300_000, 10, 3_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Silver),   (500_000, 10, 5_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Gold),     (500_000, 10, 5_000_000) },
        { (Country.Cambodia,    TravelRoute.International, PolicyTier.Platinum), (1_000_000, 10, 10_000_000) },
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
        { ClaimType.RepatriationOfRemains,         ("Repatriation of Remains",         nameof(ClaimCategory.EmergencyServices)) },
        { ClaimType.AdventurousActivities,         ("Adventurous Activities",          nameof(ClaimCategory.OptionalBenefits)) },
        { ClaimType.GolfCover,                     ("Golf Cover",                      nameof(ClaimCategory.OptionalBenefits)) },
        { ClaimType.ExtendedHomeCare,              ("Extended Home Care",              nameof(ClaimCategory.OptionalBenefits)) },
    };

    public decimal GetMaxPayout(Country country, TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge = 0)
    {
        if (type == ClaimType.MedicalExpenses && route == TravelRoute.International && insuredAge > 60)
        {
            int bracket = insuredAge <= 70 ? 70 : 80;
            if (IntlMedicalByAge.TryGetValue((country, tier, bracket), out var agePayout))
                return agePayout;
        }
        return Limits.TryGetValue((country, route, tier, type), out var payout) ? payout : 0;
    }

    public (decimal RatePerBlock, decimal BlockSizeHours, decimal MaxPayout) GetFlightDelayRate(Country country, TravelRoute route, PolicyTier tier)
        => FlightDelayRates.TryGetValue((country, route, tier), out var r) ? r : (0, 1, 0);

    public (decimal DailyRate, int MaxDays, decimal MaxPayout) GetConfinementRate(Country country, TravelRoute route, PolicyTier tier)
        => ConfinementRates.TryGetValue((country, route, tier), out var r) ? r : (0, 0, 0);

    public (decimal DailyRate, int MaxDays, decimal MaxPayout) GetHijackRate(Country country, TravelRoute route, PolicyTier tier)
        => HijackRates.TryGetValue((country, route, tier), out var r) ? r : (0, 0, 0);

    public IReadOnlyList<BenefitItemDto> GetAllBenefits(Country country, TravelRoute route, PolicyTier tier, int insuredAge = 0)
    {
        var currency = CountryCurrencyMap.GetCurrency(country);
        var unavailable = UnavailableTypes.TryGetValue(country, out var set) ? set : new HashSet<ClaimType>();
        var result = new List<BenefitItemDto>();

        foreach (var claimType in Enum.GetValues<ClaimType>())
        {
            if (unavailable.Contains(claimType)) continue;
            if (!TypeMeta.TryGetValue(claimType, out var meta)) continue;

            var maxPayout = GetMaxPayout(country, route, tier, claimType, insuredAge);
            var notes = BuildNotes(country, currency, route, tier, claimType, insuredAge, maxPayout);

            result.Add(new BenefitItemDto(
                Category: meta.Category,
                ClaimType: claimType.ToString(),
                DisplayName: meta.DisplayName,
                MaxPayout: maxPayout,
                Notes: notes,
                Currency: currency.ToString()
            ));
        }

        return result;
    }

    private string BuildNotes(Country country, Currency currency, TravelRoute route, PolicyTier tier, ClaimType type, int insuredAge, decimal maxPayout)
    {
        var sym = CountryCurrencyMap.GetSymbol(currency);

        if (type == ClaimType.FlightDelay)
        {
            var (rate, block, _) = GetFlightDelayRate(country, route, tier);
            return $"{sym}{rate:0}/per {block}-hr block";
        }

        if (type == ClaimType.HospitalConfinement)
        {
            var (daily, maxDays, _) = GetConfinementRate(country, route, tier);
            return $"{sym}{daily:0}/day, max {maxDays} days";
        }

        if (type == ClaimType.HijackInconvenience)
        {
            var (daily, maxDays, _) = GetHijackRate(country, route, tier);
            return $"{sym}{daily:0}/day, max {maxDays} days";
        }

        if (type == ClaimType.MedicalExpenses && route == TravelRoute.International)
        {
            var max61 = GetMaxPayout(country, route, tier, type, 61);
            var max71 = GetMaxPayout(country, route, tier, type, 71);
            return $"Age 61-70: {sym}{max61:N0}; Age 71-80: {sym}{max71:N0}";
        }

        if (type == ClaimType.EmergencyEvacuation && maxPayout >= Unlimited)
            return "Unlimited";

        return string.Empty;
    }
}
