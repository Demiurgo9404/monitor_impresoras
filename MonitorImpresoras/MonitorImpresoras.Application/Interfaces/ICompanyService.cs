using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestión de empresas multi-tenant
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// Obtiene todas las empresas del tenant actual con paginación
        /// </summary>
        Task<CompanyListDto> GetCompaniesAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);

        /// <summary>
        /// Obtiene una empresa por ID
        /// </summary>
        Task<CompanyDto?> GetCompanyByIdAsync(Guid id);

        /// <summary>
        /// Crea una nueva empresa
        /// </summary>
        Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto createDto);

        /// <summary>
        /// Actualiza una empresa existente
        /// </summary>
        Task<CompanyDto?> UpdateCompanyAsync(Guid id, UpdateCompanyDto updateDto);

        /// <summary>
        /// Elimina una empresa (soft delete)
        /// </summary>
        Task<bool> DeleteCompanyAsync(Guid id);

        /// <summary>
        /// Obtiene estadísticas de una empresa
        /// </summary>
        Task<CompanyStatsDto?> GetCompanyStatsAsync(Guid id);

        /// <summary>
        /// Verifica si el usuario actual tiene acceso a la empresa
        /// </summary>
        Task<bool> HasAccessToCompanyAsync(Guid companyId, string userId);

        /// <summary>
        /// Obtiene empresas del usuario actual
        /// </summary>
        Task<List<CompanyDto>> GetUserCompaniesAsync(string userId);
    }
}
