using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface IMotoService
    {
        Task<PagedResult<MotoViewDto>> GetAllMotosAsync(string? status, string? placa, int page, int pageSize);
        Task<MotoViewDto?> GetMotoByIdAsync(Guid id);

        Task<MotoViewDto> CreateMotoAsync(CreateMotoDto dto);
        Task<MotoViewDto> UpdateMotoAsync(Guid id, UpdateMotoDto dto);
        Task<bool> DeleteMotoAsync(Guid id);
    }
}
