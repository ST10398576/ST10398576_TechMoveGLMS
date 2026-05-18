namespace ST10398576_TechMoveGLMS.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }   // required FK
        public Contract? Contract { get; set; } // navigation, optional

        public string ServiceDescription { get; set; }
        public decimal ServiceCost { get; set; }
        public string ServiceStatus { get; set; }
    }


}