using Microsoft.EntityFrameworkCore;
using Csharp.Api.Data;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;

namespace Csharp.Api.Services
{
    public class FuncionarioService : IFuncionarioService
    {
        private readonly AppDbContext _ctx;
        private readonly ILogger<FuncionarioService> _log;

        public FuncionarioService(AppDbContext ctx, ILogger<FuncionarioService> log)
        {
            _ctx = ctx; _log = log;
        }

        public async Task<PagedResult<FuncionarioViewDto>> GetAllAsync(int page, int pageSize, string? q)
        {
            var query = _ctx.Funcionarios.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(f => f.Nome.Contains(q) || f.Email.Contains(q));

            var total = await query.CountAsync();
            var itens = await query
                .OrderBy(f => f.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FuncionarioViewDto { Id=f.Id, Nome=f.Nome, Email=f.Email, Cargo=f.Cargo })
                .ToListAsync();

            return new PagedResult<FuncionarioViewDto>(itens, total, page, pageSize);
        }

        public async Task<FuncionarioViewDto?> GetByIdAsync(Guid id)
        {
            var f = await _ctx.Funcionarios.FindAsync(id);
            return f == null ? null : new FuncionarioViewDto { Id=f.Id, Nome=f.Nome, Email=f.Email, Cargo=f.Cargo };
        }

        public async Task<FuncionarioViewDto> CreateAsync(CreateFuncionarioDto dto)
        {
            var f = new Funcionario { Nome = dto.Nome, Email = dto.Email, Cargo = dto.Cargo };
            _ctx.Funcionarios.Add(f);
            await _ctx.SaveChangesAsync();
            return new FuncionarioViewDto { Id=f.Id, Nome=f.Nome, Email=f.Email, Cargo=f.Cargo };
        }

        public async Task<FuncionarioViewDto> UpdateAsync(Guid id, UpdateFuncionarioDto dto)
        {
            var f = await _ctx.Funcionarios.FindAsync(id) ?? throw new KeyNotFoundException("Funcionário não encontrado");
            f.Nome = dto.Nome; f.Email = dto.Email; f.Cargo = dto.Cargo;
            await _ctx.SaveChangesAsync();
            return new FuncionarioViewDto { Id=f.Id, Nome=f.Nome, Email=f.Email, Cargo=f.Cargo };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var f = await _ctx.Funcionarios.FindAsync(id);
            if (f == null) return false;
            _ctx.Funcionarios.Remove(f);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
