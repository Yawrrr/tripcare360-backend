using System.Text.Json;
using MediatR;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Events;

public class ClaimFinalizedHandler(
    IClaimRepository claimRepository,
    IBenefitLimitsService benefitLimits,
    ISseEventBroadcaster sseBroadcaster)
    : INotificationHandler<ClaimFinalizedNotification>
{
    private static readonly Dictionary<string, string[]> StpRequiredFields = new()
    {
        { "MedicalExpenses",               ["TreatmentDate", "FacilityName", "BillAmount"] },
        { "FollowUpTreatment",             ["TreatmentDate", "FacilityName", "BillAmount"] },
        { "AlternativeTreatment",          ["TreatmentDate", "FacilityName", "BillAmount"] },
        { "HospitalConfinement",           ["AdmissionDate", "DischargeDate", "FacilityName"] },
        { "TripCancellation",              ["CancellationDate", "Reason"] },
        { "TripCurtailment",               ["ReturnDate", "Reason"] },
        { "FlightDelay",                   ["AirlineName", "FlightNumber", "DepartureDate"] },
        { "BaggageDelay",                  ["AirlineName"] },
        { "MissedConnection",              ["AirlineName", "FlightNumber"] },
        { "HijackInconvenience",           ["AirlineName", "FlightNumber", "StartDate"] },
        { "HomeCare",                      ["ServiceProvider", "ServiceDate"] },
        { "BaggageLossAndPersonalEffects", ["IncidentDate", "LostItems"] },
        { "PersonalMoneyLoss",             ["IncidentDate", "AmountLost"] },
        { "TravelDocumentsLoss",           ["IncidentDate", "DocumentType"] },
        { "PersonalLiability",             ["IncidentDate", "Description"] },
        { "EmergencyEvacuation",           ["IncidentDate", "EvacuationService"] },
        { "RepatriationOfRemains",         ["DeceasedName", "DateOfDeath"] },
        { "DeathOrPermanentDisability",    ["IncidentDate", "CertificateType"] },
        { "AdventurousActivities",         ["ActivityName", "IncidentDate", "InjuryDescription"] },
        { "GolfCover",                     ["SubType", "IncidentDate", "AmountClaimed"] },
        { "ExtendedHomeCare",              ["DamageType", "IncidentDate", "AmountClaimed"] },
    };

    public async Task Handle(ClaimFinalizedNotification notification, CancellationToken ct)
    {
        var claim = await claimRepository.GetByClaimCodeAsync(notification.ClaimCode);
        if (claim is null) return;

        // Outage bypass: external validation failed → always manual review
        if (claim.IsPreValidationFailedDueToOutage)
        {
            await Transition(claim, ClaimStatus.ManualReview, ct);
            return;
        }

        // Payout calculated as zero → genuinely unclaimable
        if (claim.CalculatedPayout == 0)
        {
            await Transition(claim, ClaimStatus.Rejected, ct);
            return;
        }

        var status = DetermineStpStatus(claim);
        await Transition(claim, status, ct);
    }

    private ClaimStatus DetermineStpStatus(ClaimEntity claim)
    {
        if (string.IsNullOrWhiteSpace(claim.AiExtractionResultJson))
            return ClaimStatus.ManualReview;

        try
        {
            var aiData = JsonDocument.Parse(claim.AiExtractionResultJson).RootElement;

            if (StpRequiredFields.TryGetValue(claim.Type.ToString(), out var required))
            {
                foreach (var field in required)
                {
                    if (!aiData.TryGetProperty(field, out var val) || val.ValueKind == JsonValueKind.Null)
                        return ClaimStatus.ManualReview;
                }
            }

            var limit = benefitLimits.GetMaxPayout(claim.Country, claim.Route, claim.Tier, claim.Type, claim.InsuredAge);
            if (limit > 0 && claim.SubmittedAmount > limit)
                return ClaimStatus.ManualReview;

            return ClaimStatus.StpApproved;
        }
        catch
        {
            return ClaimStatus.ManualReview;
        }
    }

    private async Task Transition(ClaimEntity claim, ClaimStatus status, CancellationToken ct)
    {
        claim.Status = status;
        await claimRepository.UpdateAsync(claim);

        await sseBroadcaster.BroadcastStateAsync(claim.ClaimCode, "status_update", new
        {
            claimCode = claim.ClaimCode,
            status = status.ToString()
        });
    }
}
