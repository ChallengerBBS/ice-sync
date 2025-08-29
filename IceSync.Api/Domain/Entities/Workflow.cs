using System.ComponentModel.DataAnnotations;

namespace IceSync.Api.Domain.Entities
{
    public class Workflow
    {
        [Key]
        public int WorkflowId { get; set; }
        
        [Required]
        public string WorkflowName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public string MultiExecBehavior { get; set; } = string.Empty;
    }
}
