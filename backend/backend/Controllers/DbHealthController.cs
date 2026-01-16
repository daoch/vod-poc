using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace backend.Controllers
{
    [Route("api/dbhealth")]
    [ApiController]
    public class DbHealthController : ControllerBase
    {
        private readonly string _oracleConnString;

        public DbHealthController(IConfiguration config)
        {
            _oracleConnString = config.GetConnectionString("Oracle")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:Oracle en appsettings.json");
        }

        [HttpGet("oracle")]
        public async Task<IActionResult> Oracle(CancellationToken ct)
        {
            var started = DateTime.UtcNow;

            try
            {
                var csb = new OracleConnectionStringBuilder(_oracleConnString)
                {
                    ConnectionTimeout = 5
                };

                await using var conn = new OracleConnection(csb.ConnectionString);
                await conn.OpenAsync(ct);

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM DUAL";
                cmd.CommandType = CommandType.Text;

                var result = await cmd.ExecuteScalarAsync(ct);

                return Ok(new
                {
                    ok = true,
                    db = "oracle",
                    result,
                    latencyMs = (int)(DateTime.UtcNow - started).TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    ok = false,
                    db = "oracle",
                    error = ex.Message,
                    latencyMs = (int)(DateTime.UtcNow - started).TotalMilliseconds
                });
            }
        }
    }
}
