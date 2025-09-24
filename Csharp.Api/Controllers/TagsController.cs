using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Csharp.Api.Services;
using Csharp.Api.DTOs;

namespace Csharp.Api.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagsController : ControllerBase
    {
        private readonly ITagBleService _svc;

        public TagsController(ITagBleService svc)
        {
            _svc = svc;
        }

        /// <summary>Lista tags BLE com paginação e busca.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TagBleViewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? q = null)
        {
            var result = await _svc.GetAllAsync(page, pageSize, q);

            // adiciona links HATEOAS nos itens
            foreach (var t in result.Items)
            {
                t.Links = HateoasBuilder.BuildLinks(
                    Url,
                    getName: "GetTagById",
                    updateName: "UpdateTag",
                    deleteName: "DeleteTag",
                    id: t.Id
                );
            }

            Response.Headers["X-Pagination"] = result.MetaToJson();
            return Ok(result);
        }

        /// <summary>Obtém uma tag por ID.</summary>
        [HttpGet("{id:guid}", Name = "GetTagById")]
        [ProducesResponseType(typeof(TagBleViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var t = await _svc.GetByIdAsync(id);
            if (t is null) return NotFound();

            t.Links = HateoasBuilder.BuildLinks(Url, "GetTagById", "UpdateTag", "DeleteTag", id);
            return Ok(t);
        }

        /// <summary>Cria uma nova tag BLE.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(TagBleViewDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateTagBleDto dto)
        {
            var t = await _svc.CreateAsync(dto);
            t.Links = HateoasBuilder.BuildLinks(Url, "GetTagById", "UpdateTag", "DeleteTag", t.Id);

            return CreatedAtRoute("GetTagById", new { id = t.Id }, t);
        }

        /// <summary>Atualiza uma tag BLE.</summary>
        [HttpPut("{id:guid}", Name = "UpdateTag")]
        [ProducesResponseType(typeof(TagBleViewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagBleDto dto)
        {
            try
            {
                var t = await _svc.UpdateAsync(id, dto);
                t.Links = HateoasBuilder.BuildLinks(Url, "GetTagById", "UpdateTag", "DeleteTag", t.Id);
                return Ok(t);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>Remove uma tag BLE.</summary>
        [HttpDelete("{id:guid}", Name = "DeleteTag")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
