using Application.Interfaces.Services;
using Domain.Entities.Logging;
using Infrastructure.Persistence.Configurations.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace Infrastructure.Services;

public class GeminiSummaryService : IAiSummaryService
{
    private readonly GeminiSettings _settings;

    public GeminiSummaryService(
        IOptions<GeminiSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> GenerateSummaryAsync(
        ChangeLog changeLog)
    {
        try
        {
            var googleAI = new GoogleAI(
                apiKey: _settings.ApiKey);

            var model = googleAI.GenerativeModel(
                model: _settings.Model);

            var prompt =
                "Generate a concise operational summary " +
                "for the following backend event.\n\n" +
                $"Log Source:\n{changeLog.LogSource}\n\n" +
                $"Entity:\n{changeLog.EntityName}\n\n" +
                $"Action Type:\n{changeLog.ActionType}\n\n" +
                $"Description:\n{changeLog.Description}\n\n" +
                $"Raw Data:\n{changeLog.RawData}\n\n" +
                $"Changed Fields:\n{changeLog.ChangedFields}\n\n" +
                $"Old Values:\n{changeLog.OldValues}\n\n" +
                $"New Values:\n{changeLog.NewValues}\n\n" +
                "Rules:\n" +
                "- Keep summary under 25 words\n" +
                "- Be concise and professional\n" +
                "- Focus only on meaningful operational insights\n" +
                "- Do not invent information";

            var response =
                await model.GenerateContent(prompt);

            return response.Text
                ?? changeLog.Description;
        }
        catch
        {
            return changeLog.Description;
        }
    }
}