using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10398576_TechMoveGLMS.Interfaces;
using ST10398576_TechMoveGLMS.Models;
using System.Diagnostics.Contracts;

namespace ST10398576_TechMoveGLMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        public ContractsController(IContractService contractService) => _contractService = contractService;

        [HttpGet]
        public async Task<IActionResult> GetContracts(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(status) && string.IsNullOrEmpty(serviceLevel) && !startDate.HasValue && !endDate.HasValue)
            {
                var allContracts = await _contractService.GetAllAsync();
                return Ok(allContracts);
            }

            var contracts = await _contractService.SearchAsync(status, serviceLevel, startDate, endDate);
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();
            return Ok(contract);
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] ST10398576_TechMoveGLMS.Models.Contract contract)
        {
            var created = await _contractService.CreateAsync(contract, null);
            return CreatedAtAction(nameof(GetContract), new { id = created.ContractId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] ST10398576_TechMoveGLMS.Models.Contract contract)
        {
            if (id != contract.ContractId) return BadRequest();
            var updated = await _contractService.UpdateAsync(contract, null);
            return Ok(updated);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var contract = await _contractService.GetByIdAsync(id);
            if (contract == null) return NotFound();

            contract.ContractStatus = status;
            await _contractService.UpdateAsync(contract, null);
            return Ok(contract);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            await _contractService.DeleteAsync(id);
            return NoContent();
        }
    }
}
