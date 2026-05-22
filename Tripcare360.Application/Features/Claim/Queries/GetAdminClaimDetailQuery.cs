using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Admin;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Entities.Errors;

namespace Tripcare360.Application.Features.Claim.Queries;

public class GetAdminClaimDetailQuery(string claimCode) : IRequest<ClaimDetailResponse>
{
    public string ClaimCode { get; } = claimCode;

    public class Validator : AbstractValidator<GetAdminClaimDetailQuery>
    {
        public Validator() { RuleFor(q => q.ClaimCode).NotEmpty(); }
    }

    public class Handler(IClaimRepository repo, IMinioStorageService minioStorage)
        : IRequestHandler<GetAdminClaimDetailQuery, ClaimDetailResponse>
    {
        public async Task<ClaimDetailResponse> Handle(
            GetAdminClaimDetailQuery query, CancellationToken ct)
        {
            var claim = await repo.GetByClaimCodeAsync(query.ClaimCode);
            if (claim is null) throw new ApiException(ErrorCode.ClaimNotFound);

            var files = new List<FileDetailItem>();
            for (int i = 0; i < claim.FileObjectKeys.Count; i++)
            {
                var key = claim.FileObjectKeys[i];
                var label = i < claim.FileLabels.Count ? claim.FileLabels[i] : "Supporting Document";
                var url = await minioStorage.GetPresignedUrlAsync(key);
                files.Add(new FileDetailItem(key, label, url));
            }

            return claim.ToDetailResponse(files.AsReadOnly());
        }
    }
}
