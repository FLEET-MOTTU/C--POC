using Csharp.Api.DTOs;
using Csharp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Csharp.Api.Controllers
{
    [ApiController]
    [Route("api/motos")]
    public class MotosController : ControllerBase
    {
        private readonly IMotoService _motoService;

        public MotosController(IMotoService motoService)
        {
            _motoService = motoService;
        }

        /// <summary>Lista motos com filtros e paginação.</summary>
        /// <param name="status">Status da moto (ex: Disponivel, Manutencao...)</param>
        /// <param name="placa">Filtro por placa (contém)</param>
        /// <param name="page">Página (1..N)</param>
        /// <param name="pageSize">Tamanho da página</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MotoViewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMotos(
            [FromQuery] string? status,
            [FromQuery] string? placa,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _motoService.GetAllMotosAsync(status, placa, page, pageSize);

            // HATEOAS por item:
            foreach (var m in result.Items)
            {
                m.Links = DTOs.HateoasBuilder.BuildLinks(
                    Url,
                    getName: "GetMotoById",
                    updateName: "UpdateMoto",
                    deleteName: "DeleteMoto",
                    id: m.Id
                );
            }

            // Metadados de paginação no header
            Response.Headers["X-Pagination"] = result.MetaToJson();
            return Ok(result);
        }

        /// <summary>Obtém uma moto por ID.</summary>
        [HttpGet("{id:guid}", Name = "GetMotoById")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMotoById(Guid id)
        {
            var moto = await _motoService.GetMotoByIdAsync(id);
            if (moto is null) return NotFound();

            moto.Links = DTOs.HateoasBuilder.BuildLinks(
                Url, "GetMotoById", "UpdateMoto", "DeleteMoto", id);

            return Ok(moto);
        }

        /// <summary>Cria uma nova moto.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMoto([FromBody] CreateMotoDto dto)
        {
            var moto = await _motoService.CreateMotoAsync(dto);
            moto.Links = DTOs.HateoasBuilder.BuildLinks(
                Url, "GetMotoById", "UpdateMoto", "DeleteMoto", moto.Id);

            return CreatedAtRoute("GetMotoById", new { id = moto.Id }, moto);
        }

        /// <summary>Atualiza uma moto existente.</summary>
        [HttpPut("{id:guid}", Name = "UpdateMoto")]
        [ProducesResponseType(typeof(MotoViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMoto(Guid id, [FromBody] UpdateMotoDto dto)
        {
            try
            {
                var moto = await _motoService.UpdateMotoAsync(id, dto);
                moto.Links = DTOs.HateoasBuilder.BuildLinks(
                    Url, "GetMotoById", "UpdateMoto", "DeleteMoto", moto.Id);

                return Ok(moto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>Remove uma moto.</summary>
        [HttpDelete("{id:guid}", Name = "DeleteMoto")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMoto(Guid id)
        {
            var ok = await _motoService.DeleteMotoAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
