using System.ComponentModel.DataAnnotations;

namespace InteractHub.Api.Dtos.Reports
{
    public class CreateReportRequest
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "Ly do phai co it nhat 5 ky tu")]
        public string Reason { get; set; } = string.Empty;
    }
}
