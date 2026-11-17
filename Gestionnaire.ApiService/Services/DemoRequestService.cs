using System.Net.Mail;
using Gestionnaire.ApiService.Data;
using Gestionnaire.ApiService.Model;
using Microsoft.EntityFrameworkCore;

namespace Gestionnaire.ApiService.Services;

public sealed class DemoRequestService(Contextbd dbContext)
{
    private static readonly string[] AllowedStatuses = ["New", "InProgress", "Completed"];

    public async Task<(DemoRequest? Request, string? Error)> CreateAsync(
        DemoRequestCreateRequest request,
        CancellationToken cancellationToken)
    {
        // Beaucoup de responsables d'établissement préfèrent être rappelés : un téléphone OU une adresse
        // e-mail suffit, mais il en faut au moins un pour pouvoir répondre.
        var email = request.Email?.Trim() ?? string.Empty;
        var phone = request.Phone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 160 ||
            string.IsNullOrWhiteSpace(request.Establishment) || request.Establishment.Length > 200 ||
            (email.Length == 0 && phone.Length == 0) ||
            email.Length > 254 || (email.Length > 0 && !MailAddress.TryCreate(email, out _)) ||
            !PhoneNumber.IsValidOrEmpty(phone) ||
            string.IsNullOrWhiteSpace(request.StudentCount) || request.StudentCount.Length > 40 ||
            request.Message is null || request.Message.Length > 5000)
        {
            return (null, "Vérifiez votre nom, établissement, téléphone ou adresse e-mail, effectif et message.");
        }

        var demoRequest = new DemoRequest
        {
            FullName = request.FullName.Trim(),
            Establishment = request.Establishment.Trim(),
            Email = email,
            Phone = PhoneNumber.Normalize(phone),
            StudentCount = request.StudentCount,
            Message = request.Message.Trim()
        };
        dbContext.DemoRequests.Add(demoRequest);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (demoRequest, null);
    }

    public Task<List<DemoRequest>> GetAllAsync(CancellationToken cancellationToken) =>
        dbContext.DemoRequests.AsNoTracking().OrderByDescending(request => request.CreatedAt).ToListAsync(cancellationToken);

    public async Task<(DemoRequest? Request, bool InvalidStatus)> SetStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken)
    {
        if (!AllowedStatuses.Contains(status, StringComparer.Ordinal))
        {
            return (null, true);
        }

        var request = await dbContext.DemoRequests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null)
        {
            return (null, false);
        }

        request.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return (request, false);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await dbContext.DemoRequests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        dbContext.DemoRequests.Remove(request);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static DemoRequestResponse ToResponse(DemoRequest request) => new(
        request.Id,
        request.FullName,
        request.Establishment,
        request.Email,
        request.Phone,
        request.StudentCount,
        request.Message,
        request.Status,
        request.CreatedAt);
}
