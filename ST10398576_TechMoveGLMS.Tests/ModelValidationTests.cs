using ST10398576_TechMoveGLMS.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class ModelValidationTests
{
    private IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Fact]
    public void ClientName_IsRequired()
    {
        var client = new Client { ClientContactDetails = "123", ClientRegion = "Cape Town" };
        var results = Validate(client);
        Assert.Contains(results, r => r.MemberNames.Contains("ClientName"));
    }

    [Fact]
    public void ServiceCost_MustBePositive()
    {
        var request = new ServiceRequest { ContractId = 1, ServiceDescription = "Test", ServiceCost = -10, ServiceStatus = "Pending" };
        var results = Validate(request);
        Assert.Contains(results, r => r.MemberNames.Contains("ServiceCost"));
    }
}
