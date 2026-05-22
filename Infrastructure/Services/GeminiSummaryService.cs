using Application.Interfaces.Services;
using Domain.Entities.Logging;
using Infrastructure.Persistence.Configurations.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace Infrastructure.Services;

public class GeminiSummaryService : IAiSummaryService
{
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiSummaryService> _logger;

    public GeminiSummaryService(
        IOptions<GeminiSettings> options,
        ILogger<GeminiSummaryService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateSummaryAsync(
        ChangeLog changeLog)
    {
        _logger.LogInformation(
            "Calling Gemini for log {LogId} - {ActionType}",
            changeLog.Id,
            changeLog.ActionType);

        var googleAI = new GoogleAI(
            apiKey: _settings.ApiKey);

        var model = googleAI.GenerativeModel(
            model: _settings.Model);

        var prompt =
            "Generate an operational AI summary " +
            "for the following backend audit event.\n\n" +
            $"Log Source:\n{changeLog.LogSource}\n\n" +
            $"Entity:\n{changeLog.EntityName}\n\n" +
            $"Action Type:\n{changeLog.ActionType}\n\n" +
            $"Changed Fields:\n{changeLog.ChangedFields}\n\n" +
            $"Old Values:\n{changeLog.OldValues}\n\n" +
            $"New Values:\n{changeLog.NewValues}\n\n" +
            $"Fallback Description:\n{changeLog.Description}\n\n" +
            "Rules:\n" +
            "- Do NOT repeat the fallback description verbatim\n" +
            "- Infer operational or business significance\n" +
            "- Focus on meaningful changes\n" +
            "- Be concise and professional\n" +
            "- Keep summary under 25 words\n" +
            "- Avoid raw field-name listing unless necessary\n" +
            "- Do not invent information";

        try
        {
            var response =
                await model.GenerateContent(prompt);

            var summary = response.Text
                ?? changeLog.Description;

            _logger.LogInformation(
                "Gemini succeeded for log {LogId}: {Summary}",
                changeLog.Id,
                summary);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Gemini failed for log {LogId}: {Message}",
                changeLog.Id,
                ex.Message);

            throw;
        }
    }
}