namespace RR.AI_Chat.Dto.Enums
{
    /// <summary>
    /// Contains unique identifiers for different AI service providers.
    /// </summary>
    public static class AIServiceType
    {
        /// <summary>
        /// Unique identifier for the Ollama AI service.
        /// </summary>
        public static readonly Guid Ollama = new("89440e45-346f-453b-8e31-a249e4c6c0c5");

        /// <summary>
        /// Unique identifier for the OpenAI service.
        /// </summary>
        public static readonly Guid OpenAI = new("3ad5a77e-515a-4b72-920b-7e4f1d183dfe");

        public static readonly Guid AzureAIFoundry = new("3f2a91b5-9e5a-4a0a-a57a-ec70b540bbf0");

        public static readonly Guid Anthropic = new("1d094036-4235-4308-81b8-185b1bc9d3b1");
    }
}
