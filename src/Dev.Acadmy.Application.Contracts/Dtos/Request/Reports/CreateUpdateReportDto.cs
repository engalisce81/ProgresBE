using System;
using System.ComponentModel.DataAnnotations;
namespace Dev.Acadmy.Dtos.Request.Reports
{
    public class CreateUpdateReportDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(128, ErrorMessage = "Title cannot exceed 128 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Report Type is required")]
        public int Type { get; set; }

        // ID of the entity being reported (e.g., PostId or CommentId)
        public Guid? TargetEntityId { get; set; }
    }
}
