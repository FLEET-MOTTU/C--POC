using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Api.Services
{
    public class MotoService : IMotoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MotoService> _logger;

        public MotoService(AppDbContext context, ILogger<MotoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // LISTAGEM COM FILTROS + PAGINAÇÃO
        public async Task<PagedResult<MotoViewDto>> GetAllMotosAsync(string? status, string? placa, int page, int pageSize)
        {
            var query = _context.Motos
                .AsNoTracking()
                .Include(m => m.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<TipoStatusMoto>(status, true, out var parsed))
            {
                query = query.Where(m => m.StatusMoto == parsed);
            }

            if (!string.IsNullOrWhiteSpace(placa))
                query = query.Where(m => m.Placa != null && m.Placa.Contains(placa.ToUpper()));

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(m => m.DataCriacaoRegistro)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = data.Select(MapMotoToViewDto).ToList();

            return new PagedResult<MotoViewDto>(items, total, page, pageSize);
        }

        public async Task<MotoViewDto?> GetMotoByIdAsync(Guid id)
        {
            var m = await _context.Motos
                .Include(x => x.Tag)
                .FirstOrDefaultAsync(x => x.Id == id);

            return m is null ? null : MapMotoToViewDto(m);
        }

        public async Task<MotoViewDto> CreateMotoAsync(CreateMotoDto dto)
        {
            var entity = new Moto
            {
                Id = Guid.NewGuid(),
                Placa = dto.Placa?.ToUpper(),
                Modelo = dto.Modelo,
                StatusMoto = dto.StatusMoto,         
                DataCriacaoRegistro = DateTime.UtcNow
            };

            _context.Motos.Add(entity);
            await _context.SaveChangesAsync();

            return MapMotoToViewDto(entity);
        }

        public async Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto dto)
        {
            var entity = await _context.Motos.FindAsync(id)
                ?? throw new KeyNotFoundException("Moto não encontrada");

            entity.Placa = dto.Placa?.ToUpper();
            entity.Modelo = dto.Modelo;
            entity.StatusMoto = dto.StatusMoto;

            await _context.SaveChangesAsync();
            return MapMotoToViewDto(entity);
        }

        public async Task<bool> DeleteMotoAsync(Guid id)
        {
            var entity = await _context.Motos.FindAsync(id);
            if (entity is null) return false;

            _context.Motos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ====== MAPEAMENTO ======
        private static MotoViewDto MapMotoToViewDto(Moto m) => new()
        {
            Id = m.Id,
            Placa = m.Placa,
            Modelo = m.Modelo,
            StatusMoto = m.StatusMoto,
            DataCriacaoRegistro = m.DataCriacaoRegistro,
            Tag = m.Tag is null ? null : new TagBleViewDto
            {
                Id = m.Tag.Id,
                CodigoUnicoTag = m.Tag.CodigoUnicoTag,
                NivelBateria = m.Tag.NivelBateria
            }
            // Links (HATEOAS) serão preenchidos no Controller
        };
    }
}
