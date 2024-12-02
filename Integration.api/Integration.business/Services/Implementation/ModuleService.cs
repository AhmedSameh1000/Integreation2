using AutoRepairPro.Data.Repositories.Interfaces;
using Integration.business.DTOs.ModuleDTOs;
using Integration.business.Services.Interfaces;
using Integration.data.Data;
using Integration.data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.business.Services.Implementation
{
    public class ModuleService : IModuleService
    {
        private readonly IGenericRepository<Module> _moduleRepository;
        private readonly ILocalService _localService;
        private readonly IGenericRepository<TableReference> _tableReference;
        private readonly AppDbContext _appDbContext;

        public ModuleService(
            IGenericRepository<Module> ModuleRepository,
            ILocalService localService,
            IGenericRepository<TableReference> TableReference,
            AppDbContext appDbContext
        )
        {
            _moduleRepository = ModuleRepository;
            _localService = localService;
            _tableReference = TableReference;
            _appDbContext = appDbContext;
        }

        public async Task<ApiResponse<bool>> CreateModule(ModuleForCreateDTO moduleForCreateDTO)
        {
            try
            {
                // Map the DTO to the entity model
                var module = new Module()
                {
                    Name = moduleForCreateDTO.ModuleName,
                    TableFromName = moduleForCreateDTO.TableFromName,
                    TableToName = moduleForCreateDTO.TableToName,
                    ToPrimaryKeyName = moduleForCreateDTO.ToPrimaryKeyName,
                    fromPrimaryKeyName = moduleForCreateDTO.FromPrimaryKeyName,
                    LocalIdName = moduleForCreateDTO.LocalIdName,
                    CloudIdName = moduleForCreateDTO.CloudIdName,
                    ToDbId = moduleForCreateDTO.ToDbId,
                    FromDbId = moduleForCreateDTO.FromDbId,
                    ToInsertFlagName = moduleForCreateDTO.ToInsertFlagName,
                    ToUpdateFlagName = moduleForCreateDTO.ToUpdateFlagName,
                    FromInsertFlagName = moduleForCreateDTO.FromInsertFlagName,
                    FromUpdateFlagName = moduleForCreateDTO.FromUpdateFlagName,
                    condition=moduleForCreateDTO.condition,
                    columnFroms = moduleForCreateDTO.Columns.Select(c => new ColumnFrom()
                    {
                        ColumnFromName = c.ColumnFrom,
                        ColumnToName = c.ColumnTo,
                        isReference = c.IsChecked,
                        TableReferenceName = c.Referance 
                    }).ToList()
                };

                // Add the module and save changes
                bool isModuleCreated = await AddModuleAsync(module);
                if (!isModuleCreated)
                    return new ApiResponse<bool>(false, "Error on creating module.", false);

                // Create and add table references
                if (moduleForCreateDTO.References.Count > 0)
                {
                    var references = moduleForCreateDTO.References.Select(c => new TableReference()
                    {
                        LocalName = c.LocalName,
                        PrimaryName = c.PrimaryName,
                        TableFromName = c.TableFromName,
                        ModuleId = module.Id
                    }).ToList();

                    bool areReferencesAdded = await AddReferencesAsync(references);
                    if (!areReferencesAdded)
                        return new ApiResponse<bool>(false, "Error on adding references.", false);

                }
                return new ApiResponse<bool>(true, "Module created successfully.", true);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, "Error: " + ex.Message, false);
            }
        }

        private async Task<bool> AddModuleAsync(Module module)
        {
            try
            {
                await _moduleRepository.Add(module);
                return await _moduleRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log exception (if needed)
                throw new Exception("Failed to add module: " + ex.Message);
            }
        }

        private async Task<bool> AddReferencesAsync(List<TableReference> references)
        {
            try
            {
                await _tableReference.AddRange(references);
                return await _tableReference.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log exception (if needed)
                throw new Exception("Failed to add references: " + ex.Message);
            }
        }

        public async Task<List<ModuleForReturnDTO>> GetModules()
        {
            var Modules=await _moduleRepository.GetAllAsNoTracking();

            return Modules.Select(c=>new ModuleForReturnDTO()
            {
                Id = c.Id,
                Name = c.Name,
                SyncType=c.SyncType.ToString(),
                TableFromName=c.TableFromName,
                TableToName=c.TableToName,
            }).ToList();
        }

        public async Task<ApiResponse<int>> Sync(int ModuleId, SyncType syncType)
        {
            try
            {
                var response = await _localService.SyncLocalToPublic(ModuleId);
                return response;
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ModuleFullDataForReturnDTO>> GetModuleById(int Id)
        {
            var Module=await _moduleRepository.GetFirstOrDefault(c => c.Id == Id, new[] { "columnFroms" } );

            if (Module is null)
                return new ApiResponse<ModuleFullDataForReturnDTO>(false, "Module Not Found");


            var Referances=await _tableReference.GetAllAsNoTracking(c=>c.ModuleId==Id);

            var ModuleForReturn = new ModuleFullDataForReturnDTO()
            {
                Id = Id,
                Name = Module.Name,
                CloudIdName = Module.CloudIdName,
                LocalIdName = Module.LocalIdName,
                FromDbId = Module.FromDbId,
                ToDbId = Module.ToDbId,
                FromInsertFlagName = Module.FromInsertFlagName,
                FromUpdateFlagName = Module.FromUpdateFlagName,
                ToInsertFlagName = Module.ToInsertFlagName,
                ToUpdateFlagName = Module.ToUpdateFlagName,
                SyncType = Module.SyncType.ToString(),
                fromPrimaryKeyName = Module.fromPrimaryKeyName,
                TableFromName = Module.TableFromName,
                TableToName = Module.TableToName,
                ToPrimaryKeyName = Module.ToPrimaryKeyName,
                condition=Module.condition,
                columnsFromDTOs = Module.columnFroms.Select(c => new ColumnFromDTO()
                {
                    Id = c.Id,
                    ColumnFromName = c.ColumnFromName,
                    ColumnToName = c.ColumnToName,
                    isReference = c.isReference,
                    ModuleId = c.ModuleId,
                    TableReferenceName = c.TableReferenceName
                }).ToList(),
                referancesForReturnDTOs = Referances.Select(c => new ReferancesForReturnDTO()
                {
                    Id = c.Id,
                    ModuleId = c.ModuleId,
                    LocalName = c.LocalName,
                    PrimaryName = c.PrimaryName,
                    TableFromName = c.TableFromName
                }).ToList()
            };
            return new ApiResponse<ModuleFullDataForReturnDTO>(true,"Module Found",ModuleForReturn);
         
        }

        public async Task<ApiResponse<bool>> DeleteModule(int id)
        {
           var Module=await _moduleRepository.GetFirstOrDefault(c=>c.Id==id);

            if (Module is null)
                return new ApiResponse<bool>(false, "Module not found");
            var Referance=await _tableReference.GetAllAsTracking(c=>c.ModuleId==id);

            if (Referance.Any())
            {
                _tableReference.RemoveRange(Referance);
                var deleted = await _tableReference.SaveChanges();

                if (!deleted)
                    return new ApiResponse<bool>(false, "En error whene delete module");
            }

            _moduleRepository.Remove(Module);
            var IsDeleted= await _moduleRepository.SaveChanges();

            if (!IsDeleted)
                return new ApiResponse<bool>(false, "En error whene delete module");

            return new ApiResponse<bool>(true, "Module deleted successfully");
        }

        public async Task<ApiResponse<bool>> EditModule(ModuleForEditDTO moduleForEdit)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync(); // بدء المعاملة

            try
            {
                // 1. جلب الـ Module الحالي من قاعدة البيانات باستخدام الـ ID
                var module = await _moduleRepository.GetFirstOrDefault(c=>c.Id== moduleForEdit.Id);
                if (module is null)
                {
                    return new ApiResponse<bool>(false, "Module not found.", false);
                }

                // 2. تحديث خصائص الـ Module بناءً على الـ DTO الجديد
                module.Name = moduleForEdit.ModuleName;
                module.TableFromName = moduleForEdit.TableFromName;
                module.TableToName = moduleForEdit.TableToName;
                module.ToPrimaryKeyName = moduleForEdit.ToPrimaryKeyName;
                module.fromPrimaryKeyName = moduleForEdit.FromPrimaryKeyName;
                module.LocalIdName = moduleForEdit.LocalIdName;
                module.CloudIdName = moduleForEdit.CloudIdName;
                module.ToDbId = moduleForEdit.ToDbId;
                module.FromDbId = moduleForEdit.FromDbId;
                module.ToInsertFlagName = moduleForEdit.ToInsertFlagName;
                module.ToUpdateFlagName = moduleForEdit.ToUpdateFlagName;
                module.FromInsertFlagName = moduleForEdit.FromInsertFlagName;
                module.FromUpdateFlagName = moduleForEdit.FromUpdateFlagName;
                module.condition = moduleForEdit.condition;
                // 3. إزالة الأعمدة القديمة (إذا كانت موجودة)
                var existingColumns = await _appDbContext.columnFroms.Where(c => c.ModuleId == moduleForEdit.Id).ToListAsync();
                _appDbContext.columnFroms.RemoveRange(existingColumns);

                // 4. إضافة الأعمدة الجديدة
                var newColumns = moduleForEdit.Columns.Select(c => new ColumnFrom()
                {
                    ColumnFromName = c.ColumnFrom,
                    ColumnToName = c.ColumnTo,
                    isReference = c.IsChecked,
                    TableReferenceName = c.Referance,
                    ModuleId = moduleForEdit.Id,
                }).ToList();

                await _appDbContext.columnFroms.AddRangeAsync(newColumns);

                // 5. إزالة المراجع القديمة
                var existingReferences = await _appDbContext.References.Where(r => r.ModuleId == moduleForEdit.Id).ToListAsync();
                _appDbContext.References.RemoveRange(existingReferences);

                // 6. إضافة المراجع الجديدة
                if (moduleForEdit.References.Any())
                {
                    var newReferences = moduleForEdit.References.Select(r => new TableReference()
                    {
                        LocalName = r.LocalName,
                        PrimaryName = r.PrimaryName,
                        TableFromName = r.TableFromName,
                        ModuleId = moduleForEdit.Id,
                    }).ToList();

                    await _appDbContext.References.AddRangeAsync(newReferences);
                }

                // 7. حفظ التغييرات في قاعدة البيانات
               var Result= await _appDbContext.SaveChangesAsync();

                if(Result<=0)
                    await transaction.RollbackAsync();


                // 8. تأكيد المعاملة
                await transaction.CommitAsync();

                return new ApiResponse<bool>(true, "Module updated successfully.", true);
            }
            catch (Exception ex)
            {
                // في حال حدوث خطأ، نقوم بإلغاء المعاملة
                await transaction.RollbackAsync();
                return new ApiResponse<bool>(false, "Error: " + ex.Message, false);
            }
        }

    }
}
