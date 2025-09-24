using Csharp.Api.DTOs;

namespace Csharp.Api.Services
{
    public interface ITagBleService
    {
        Task<PagedResult<TagBleViewDto>> GetAllAsync(int page, int pageSize, string? q);
        Task<TagBleViewDto?> GetByIdAsync(Guid id);
        Task<TagBleViewDto> CreateAsync(CreateTagBleDto dto);
        Task<TagBleViewDto> UpdateAsync(Guid id, UpdateTagBleDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
