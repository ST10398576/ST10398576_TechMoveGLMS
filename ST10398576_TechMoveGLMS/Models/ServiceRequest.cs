namespace ST10398576_TechMoveGLMS.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public Contract Contract { get; set; }

        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
    }

}
