using Microsoft.AspNetCore.Mvc;
using Csharp.Api.DTOs;
using Csharp.Api.Services;

namespace Csharp.Api.Controllers
{
    [ApiController]
    [Route("api/funcionarios")]
    public class FuncionariosController : ControllerBase
    {
        private readonly IFuncionarioService _svc;
        public FuncionariosController(IFuncionarioService svc) => _svc = svc;

        /// <summary>Lista funcionários com paginação e busca.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<FuncionarioViewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? q = null)
        {
            var result = await _svc.GetAllAsync(page, pageSize, q);
            // HATEOAS para cada item:
            foreach (var f in result.Items)
            {
                f.Links = HateoasBuilder.BuildLinks(Url, "GetFuncionarioById", "UpdateFuncionario", "DeleteFuncionario", f.Id);
            }
            Response.Headers["X-Pagination"] = result.MetaToJson();
            return Ok(result);
        }

        /// <summary>Obtém um funcionário por ID.</summary>
        [HttpGet("{id:guid}", Name = "GetFuncionarioById")]
        [ProducesResponseType(typeof(FuncionarioViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var f = await _svc.GetByIdAsync(id);
            if (f == null) return NotFound();
            f.Links = HateoasBuilder.BuildLinks(Url, "GetFuncionarioById", "UpdateFuncionario", "DeleteFuncionario", id);
            return Ok(f);
        }

        /// <summary>Cria um funcionário.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(FuncionarioViewDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateFuncionarioDto dto)
        {
            var f = await _svc.CreateAsync(dto);
            return CreatedAtRoute("GetFuncionarioById", new { id = f.Id }, f);
        }

        /// <summary>Atualiza um funcionário.</summary>
        [HttpPut("{id:guid}", Name = "UpdateFuncionario")]
        [ProducesResponseType(typeof(FuncionarioViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFuncionarioDto dto)
        {
            try { return Ok(await _svc.UpdateAsync(id, dto)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        /// <summary>Remove um funcionário.</summary>
        [HttpDelete("{id:guid}", Name = "DeleteFuncionario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
