using Integration.business.DTOs.ModuleDTOs;
using Integration.data.Models;

namespace Integration.business.Services.Interfaces
{
    public interface IModuleService
    {
        public Task<ApiResponse<int>> Sync(int ModuleId,SyncType syncType);

        public Task<List<ModuleForReturnDTO>> GetModules();

        public Task<ApiResponse<bool>> CreateModule(ModuleForCreateDTO moduleForCreateDTO);
    }

}

