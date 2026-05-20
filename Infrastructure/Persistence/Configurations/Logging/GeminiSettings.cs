using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistence.Configurations.Logging
{
    public class GeminiSettings
    {
        public const string SectionName =
            "Gemini";

        public string ApiKey { get; set; } = null!;

        public string Model { get; set; } = null!;
    }
}
