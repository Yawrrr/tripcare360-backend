using MediatR;

namespace Tripcare360.Application.Features.Claim.Events;

public record ClaimFinalizedNotification(string ClaimCode) : INotification;
