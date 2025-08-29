using System.Text.Json.Serialization;

namespace IceSync.Api.Domain.DTOs
{
    public class WorkflowDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        
        [JsonPropertyName("multiExecBehavior")]
        public string MultiExecBehavior { get; set; } = string.Empty;
    }
}
