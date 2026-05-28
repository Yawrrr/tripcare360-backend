namespace Tripcare360.Domain.Enums;

public enum ClaimType
{
    [ClaimCategory(ClaimCategory.PersonalAccident)]
    DeathOrPermanentDisability,

    [ClaimCategory(ClaimCategory.MedicalAndExpenses)]
    MedicalExpenses,

    [ClaimCategory(ClaimCategory.MedicalAndExpenses)]
    FollowUpTreatment,

    [ClaimCategory(ClaimCategory.MedicalAndExpenses)]
    AlternativeTreatment,

    [ClaimCategory(ClaimCategory.MedicalAndExpenses)]
    HospitalConfinement,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    TripCancellation,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    TripCurtailment,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    FlightDelay,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    BaggageDelay,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    MissedConnection,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    HijackInconvenience,

    [ClaimCategory(ClaimCategory.PersonalBelongings)]
    BaggageLossAndPersonalEffects,

    [ClaimCategory(ClaimCategory.PersonalBelongings)]
    PersonalMoneyLoss,

    [ClaimCategory(ClaimCategory.PersonalBelongings)]
    TravelDocumentsLoss,

    [ClaimCategory(ClaimCategory.TravelInconveniences)]
    HomeCare,

    [ClaimCategory(ClaimCategory.Liability)]
    PersonalLiability,

    [ClaimCategory(ClaimCategory.EmergencyServices)]
    EmergencyEvacuation,

    [ClaimCategory(ClaimCategory.EmergencyServices)]
    RepatriationOfRemains,

    [ClaimCategory(ClaimCategory.OptionalBenefits)]
    AdventurousActivities,

    [ClaimCategory(ClaimCategory.OptionalBenefits)]
    GolfCover,

    [ClaimCategory(ClaimCategory.OptionalBenefits)]
    ExtendedHomeCare
}
