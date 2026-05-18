using System.ComponentModel.DataAnnotations;

namespace ST10398576_TechMoveGLMS.Models
{
    public class Contract
    {
        public int ContractId { get; set; }

        [Required]
        public int ClientId { get; set; }   // required FK
        public Client? Client { get; set; } // navigation, optional

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string ContractStatus { get; set; }

        [Required]
        public string ContractServiceLevel { get; set; }
        public string? PdfFilePath { get; set; }

        public ICollection<ServiceRequest>? ServiceRequests { get; set; } // make nullable
    }

}
