using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.business.Services.Interfaces
{
    public interface ILocalService
    {
        Task<ApiResponse<int>> SyncLocalToPublic(int moduleId);
    }   

}

