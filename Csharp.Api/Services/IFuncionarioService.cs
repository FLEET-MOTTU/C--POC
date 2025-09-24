using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IFuncionarioService
    {
        Task<PagedResult<FuncionarioViewDto>> GetAllAsync(int page, int pageSize, string? q);
        Task<FuncionarioViewDto?> GetByIdAsync(Guid id);
        Task<FuncionarioViewDto> CreateAsync(CreateFuncionarioDto dto);
        Task<FuncionarioViewDto> UpdateAsync(Guid id, UpdateFuncionarioDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
