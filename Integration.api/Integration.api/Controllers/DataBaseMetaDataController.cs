using Integration.business.Services.Interfaces;
using Integration.data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Integration.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataBaseMetaDataController : ControllerBase
    {
        private readonly IDataBaseMetaDataService _dataBaseMetaDataService;

        public DataBaseMetaDataController(IDataBaseMetaDataService dataBaseMetaDataService)
        {
            _dataBaseMetaDataService = dataBaseMetaDataService;
        }

        // Check if connected to the database
        [HttpGet("check-connection")]
        public async Task<IActionResult> CheckConnection(int DataBaseId)
        {
            var Result=await _dataBaseMetaDataService.CanConnectAsync(DataBaseId);

            if(!Result) 
                return NotFound();

            return Ok(Result);
        }

        // Example endpoint for getting all tables
        [HttpGet("tables")]
        public async Task<IActionResult> GetAllTables(int DataBaseId)
        {
            var tables = await _dataBaseMetaDataService.GetAllTablesAsync(DataBaseId);
            return Ok(tables);
        }


        // Example endpoint for getting all columns from a table
        [HttpGet("columns")]
        public async Task<IActionResult> GetAllColumns([FromQuery] int DataBaseId, [FromQuery] string tableName)
        {
            var tables = await _dataBaseMetaDataService.GetAllColumnsAsync(DataBaseId,tableName);
            return Ok(tables);
        }
    }
}
