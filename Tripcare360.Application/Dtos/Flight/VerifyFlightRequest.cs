using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Dtos.Flight;

public record VerifyFlightRequest(string FlightNumber, TravelRoute Route);
