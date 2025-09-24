using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Services
{
    public class TagBleService : ITagBleService
    {
        private readonly AppDbContext _ctx;
        private readonly ILogger<TagBleService> _logger;

        public TagBleService(AppDbContext ctx, ILogger<TagBleService> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<PagedResult<TagBleViewDto>> GetAllAsync(int page, int pageSize, string? q)
        {
            var query = _ctx.TagsBle.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.CodigoUnicoTag.Contains(q));

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(t => t.CodigoUnicoTag)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = data.Select(t => new TagBleViewDto
            {
                Id = t.Id,
                CodigoUnicoTag = t.CodigoUnicoTag,
                NivelBateria = t.NivelBateria
            }).ToList();

            return new PagedResult<TagBleViewDto>(items, total, page, pageSize);
        }

        public async Task<TagBleViewDto?> GetByIdAsync(Guid id)
        {
            var t = await _ctx.TagsBle.FindAsync(id);
            return t is null ? null : new TagBleViewDto
            {
                Id = t.Id,
                CodigoUnicoTag = t.CodigoUnicoTag,
                NivelBateria = t.NivelBateria
            };
        }

        public async Task<TagBleViewDto> CreateAsync(CreateTagBleDto dto)
        {
            var t = new TagBle
            {
                Id = Guid.NewGuid(),
                CodigoUnicoTag = dto.CodigoUnicoTag,
                NivelBateria = dto.NivelBateria
            };

            _ctx.TagsBle.Add(t);
            await _ctx.SaveChangesAsync();

            return new TagBleViewDto
            {
                Id = t.Id,
                CodigoUnicoTag = t.CodigoUnicoTag,
                NivelBateria = t.NivelBateria
            };
        }

        public async Task<TagBleViewDto> UpdateAsync(Guid id, UpdateTagBleDto dto)
        {
            var t = await _ctx.TagsBle.FindAsync(id)
                ?? throw new KeyNotFoundException("Tag BLE não encontrada");

            t.CodigoUnicoTag = dto.CodigoUnicoTag;
            t.NivelBateria = dto.NivelBateria;

            await _ctx.SaveChangesAsync();

            return new TagBleViewDto
            {
                Id = t.Id,
                CodigoUnicoTag = t.CodigoUnicoTag,
                NivelBateria = t.NivelBateria
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var t = await _ctx.TagsBle.FindAsync(id);
            if (t is null) return false;

            _ctx.TagsBle.Remove(t);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
