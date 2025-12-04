using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("import-excel")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            var result = await _adminService.ImportExcel(file);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var bytes = await _adminService.ExportExcel();

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DanhSachMon_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }

        [HttpPost("import-excel-table")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcelTable(IFormFile file)
        {
            var result = await _adminService.ImportExcelTable(file);
            return StatusCode(result.StatusCode, result);
        }


        [HttpPost("import-excel-topping/{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcelTopping(
            Guid id,
             IFormFile file
        )
        {
            var result = await _adminService.ImportExcelTopping(id, file);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("export-excel-topping/{id:guid}")]
        public async Task<IActionResult> ExportExcelTopping(Guid id)
        {
            var bytes = await _adminService.ExportExcelTopping(id);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"DanhSachTopping_{id}_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }
    }
}
