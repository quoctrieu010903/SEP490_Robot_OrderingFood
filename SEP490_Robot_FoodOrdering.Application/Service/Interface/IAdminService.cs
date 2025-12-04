
using Microsoft.AspNetCore.Http;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IAdminService
    {
        Task<BaseResponseModel<bool>> ImportExcel(IFormFile file);

        Task<BaseResponseModel<bool>> ImportExcelTable(IFormFile file);

        Task<BaseResponseModel<bool>> ImportExcelTopping(Guid id, IFormFile file);

        Task<byte[]> ExportExcel();

        Task<byte[]> ExportExcelTable();
        Task<byte[]> ExportExcelTopping(Guid id);
    }
}
