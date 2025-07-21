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

        /// <summary>
        /// Unique identifier for the Azure OpenAI service.
        /// </summary>
        public static readonly Guid AzureOpenAI = new("9f29b328-8e63-4b87-a78d-51e96a660135");
    }
}
