using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Infrastructure.Services;

public class EmailNotificationService(
    IConfiguration config,
    ILogger<EmailNotificationService> logger)
    : IEmailNotificationService
{
    public async Task SendClaimOutcomeEmailAsync(ClaimEntity claim, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(claim.InsuredEmail))
        {
            logger.LogWarning("[Email] Skipped — no InsuredEmail on claim {ClaimCode}", claim.ClaimCode);
            return;
        }

        var host     = config["Email:SmtpHost"]    ?? "localhost";
        var port     = int.Parse(config["Email:SmtpPort"] ?? "1025");
        var fromAddr = config["Email:FromAddress"] ?? "no-reply@tripcare360.com";
        var fromName = config["Email:FromName"]    ?? "TripCare360 Claims";

        var isApproved = claim.Status is ClaimStatus.StpApproved or ClaimStatus.AdminApproved;
        var claimTypeLabel = FormatClaimType(claim.Type.ToString());

        var subject = isApproved
            ? $"Your TripCare360 claim {claim.ClaimCode} has been approved"
            : $"Your TripCare360 claim {claim.ClaimCode} has been rejected";

        var body = isApproved
            ? BuildApprovedHtml(claim, claimTypeLabel)
            : BuildRejectedHtml(claim, claimTypeLabel);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddr));
        message.To.Add(new MailboxAddress(claim.InsuredName, claim.InsuredEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, SecureSocketOptions.None, ct);
            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
            logger.LogInformation("[Email] Sent {Status} notification to {Email} for claim {ClaimCode}",
                claim.Status, claim.InsuredEmail, claim.ClaimCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Email] Failed to send notification for claim {ClaimCode} to {Email} — continuing",
                claim.ClaimCode, claim.InsuredEmail);
        }
    }

    private static string FormatClaimType(string raw) =>
        string.Concat(raw.Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString())).Trim();

    private static string BuildApprovedHtml(ClaimEntity claim, string claimTypeLabel) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;background:#f8f9fa;padding:32px;margin:0">
          <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08)">
            <div style="background:#FBBC00;padding:24px 32px">
              <h1 style="margin:0;font-size:22px;color:#261a00">TripCare360</h1>
            </div>
            <div style="padding:32px">
              <h2 style="color:#191c1d;margin-top:0">Your claim has been <span style="color:#16a34a">approved</span> ✓</h2>
              <p style="color:#504532">Dear <strong>{claim.InsuredName}</strong>,</p>
              <p style="color:#504532">Great news! Your travel insurance claim has been reviewed and approved.</p>
              <table style="width:100%;border-collapse:collapse;margin:24px 0">
                <tr style="background:#f8f9fa">
                  <td style="padding:10px 14px;color:#795900;font-size:11px;font-weight:bold;text-transform:uppercase;letter-spacing:.08em">Claim ID</td>
                  <td style="padding:10px 14px;color:#191c1d;font-family:monospace;font-weight:bold">{claim.ClaimCode}</td>
                </tr>
                <tr>
                  <td style="padding:10px 14px;color:#795900;font-size:11px;font-weight:bold;text-transform:uppercase;letter-spacing:.08em">Claim Type</td>
                  <td style="padding:10px 14px;color:#191c1d">{claimTypeLabel}</td>
                </tr>
                <tr style="background:#f8f9fa">
                  <td style="padding:10px 14px;color:#795900;font-size:11px;font-weight:bold;text-transform:uppercase;letter-spacing:.08em">Approved Payout</td>
                  <td style="padding:10px 14px;color:#191c1d;font-weight:bold">{claim.CalculatedPayout:F2}</td>
                </tr>
              </table>
              <p style="color:#504532;font-size:14px">The approved amount will be transferred to your registered bank account within 3–5 business days.</p>
              <p style="color:#504532;font-size:14px;margin-bottom:0">Thank you for choosing TripCare360.</p>
            </div>
            <div style="background:#f8f9fa;padding:16px 32px;text-align:center">
              <p style="margin:0;color:#504532;font-size:12px">TripCare360 — Travel Insurance Claims</p>
            </div>
          </div>
        </body>
        </html>
        """;

    private static string BuildRejectedHtml(ClaimEntity claim, string claimTypeLabel)
    {
        var commentsSection = !string.IsNullOrWhiteSpace(claim.AdminComments)
            ? $"""
               <div style="background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:16px;margin:16px 0">
                 <p style="margin:0;font-size:12px;font-weight:bold;text-transform:uppercase;letter-spacing:.08em;color:#dc2626">Insurer Note</p>
                 <p style="margin:4px 0 0;color:#504532;font-size:14px">{claim.AdminComments}</p>
               </div>
               """
            : string.Empty;

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;background:#f8f9fa;padding:32px;margin:0">
              <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08)">
                <div style="background:#FBBC00;padding:24px 32px">
                  <h1 style="margin:0;font-size:22px;color:#261a00">TripCare360</h1>
                </div>
                <div style="padding:32px">
                  <h2 style="color:#191c1d;margin-top:0">Your claim has been <span style="color:#dc2626">rejected</span></h2>
                  <p style="color:#504532">Dear <strong>{claim.InsuredName}</strong>,</p>
                  <p style="color:#504532">After careful review, we were unable to approve your travel insurance claim.</p>
                  <table style="width:100%;border-collapse:collapse;margin:24px 0">
                    <tr style="background:#f8f9fa">
                      <td style="padding:10px 14px;color:#795900;font-size:11px;font-weight:bold;text-transform:uppercase;letter-spacing:.08em">Claim ID</td>
                      <td style="padding:10px 14px;color:#191c1d;font-family:monospace;font-weight:bold">{claim.ClaimCode}</td>
                    </tr>
                    <tr>
                      <td style="padding:10px 14px;color:#795900;font-size:11px;font-weight:bold;text-transform:uppercase;letter-spacing:.08em">Claim Type</td>
                      <td style="padding:10px 14px;color:#191c1d">{claimTypeLabel}</td>
                    </tr>
                  </table>
                  {commentsSection}
                  <p style="color:#504532;font-size:14px">If you believe this decision is incorrect, please contact our concierge team for assistance.</p>
                  <p style="color:#504532;font-size:14px;margin-bottom:0">Thank you for choosing TripCare360.</p>
                </div>
                <div style="background:#f8f9fa;padding:16px 32px;text-align:center">
                  <p style="margin:0;color:#504532;font-size:12px">TripCare360 — Travel Insurance Claims</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }
}
