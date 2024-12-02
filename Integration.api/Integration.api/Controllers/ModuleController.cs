using Integration.business.DTOs.ModuleDTOs;
using Integration.business.Services.Implementation;
using Integration.business.Services.Interfaces;
using Integration.data.Data;
using Integration.data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace Integration.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModuleController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpPost("CreateModule")]
        public async Task<IActionResult> CreateModule(ModuleForCreateDTO moduleForCreateDTO)
        {
            try
            {
                var result = await _moduleService.CreateModule(moduleForCreateDTO);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }


        [HttpGet("Sync")]
        public async Task<IActionResult> Sync(int ModuleId,SyncType syncType)
        {
            try
            {
                var Result = await _moduleService.Sync(ModuleId, syncType);
                if (!Result.Success)
                    return BadRequest(Result);

                return Ok(Result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }  
        [HttpPost("EditModule")]
        public async Task<IActionResult> EditModule(ModuleForEditDTO moduleForEditDTO)
        {
            try
            {
                var Result = await _moduleService.EditModule(moduleForEditDTO);
                if (!Result.Success)
                    return BadRequest(Result);

                return Ok(Result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpGet("Modules")]
        public async Task<IActionResult> GetModules()
        {
            return Ok(await _moduleService.GetModules());
        }
       
        [HttpGet("GetModule/{id}")]
        public async Task<IActionResult> GetModule(int id)
        {
            var Result=await _moduleService.GetModuleById(id);

            if(!Result.Success)
                return NotFound(Result);

            return Ok(Result);
        }  
        [HttpDelete("DeleteModule/{id}")]
        public async Task<IActionResult> DeleteModule(int id)
        {
            var Result=await _moduleService.DeleteModule(id);

            if(!Result.Success)
                return BadRequest(Result);

            return Ok(Result);
        }



    }
}