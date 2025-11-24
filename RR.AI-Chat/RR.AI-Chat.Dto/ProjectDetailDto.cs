namespace RR.AI_Chat.Dto
{
    public class ProjectDetailDto : ProjectDto
    {
        public List<SessionDto> Sessions { get; set; } = [];
    }
}
