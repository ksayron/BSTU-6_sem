using System.Data;
using Microsoft.AspNetCore.Mvc;
using TDWA06_01.Models;

namespace TDWA06_01.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CelebritiesController : ControllerBase
{
    private readonly IDbConnection _db;

    public CelebritiesController(IDbConnection db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Celebrity>>> GetAll()
    {
        await EnsureOpenAsync();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT id, Fullname, Nationality, ReqPhotoPath FROM dbo.Celebrities ORDER BY id";

        var list = new List<Celebrity>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Celebrity
            {
                Id = reader.GetInt32(0),
                Fullname = reader.GetString(1),
                Nationality = reader.GetString(2),
                ReqPhotoPath = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Celebrity>> GetById(int id)
    {
        await EnsureOpenAsync();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT id, Fullname, Nationality, ReqPhotoPath FROM dbo.Celebrities WHERE id = @id";
        AddParameter(cmd, "@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return NotFound();
        }

        var entity = new Celebrity
        {
            Id = reader.GetInt32(0),
            Fullname = reader.GetString(1),
            Nationality = reader.GetString(2),
            ReqPhotoPath = reader.IsDBNull(3) ? null : reader.GetString(3)
        };

        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Celebrity>> Create([FromBody] Celebrity model)
    {
        await EnsureOpenAsync();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.Celebrities (Fullname, Nationality, ReqPhotoPath)
VALUES (@fullname, @nationality, @photoPath);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        AddParameter(cmd, "@fullname", model.Fullname);
        AddParameter(cmd, "@nationality", model.Nationality);
        AddParameter(cmd, "@photoPath", model.ReqPhotoPath ?? (object)DBNull.Value);

        var newId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        model.Id = newId;

        return CreatedAtAction(nameof(GetById), new { id = newId }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Celebrity model)
    {
        await EnsureOpenAsync();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = @"
UPDATE dbo.Celebrities
SET Fullname = @fullname,
    Nationality = @nationality,
    ReqPhotoPath = @photoPath
WHERE id = @id;";

        AddParameter(cmd, "@id", id);
        AddParameter(cmd, "@fullname", model.Fullname);
        AddParameter(cmd, "@nationality", model.Nationality);
        AddParameter(cmd, "@photoPath", model.ReqPhotoPath ?? (object)DBNull.Value);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await EnsureOpenAsync();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.Celebrities WHERE id = @id";
        AddParameter(cmd, "@id", id);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    private async Task EnsureOpenAsync()
    {
        if (_db.State != ConnectionState.Open)
        {
            await ((System.Data.Common.DbConnection)_db).OpenAsync();
        }
    }

    private static void AddParameter(IDbCommand cmd, string name, object value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        cmd.Parameters.Add(parameter);
    }
}
