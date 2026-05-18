using System.ComponentModel.DataAnnotations;

namespace ST10398576_TechMoveGLMS.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Required]
        public int ContractId { get; set; }   // required FK

        public Contract? Contract { get; set; } // navigation, optional

        [Required]
        public string ServiceDescription { get; set; }

        [Range(1, double.MaxValue)]
        public decimal ServiceCost { get; set; }

        [Required]
        public string ServiceStatus { get; set; }
    }
}