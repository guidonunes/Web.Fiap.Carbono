using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Dtos.MongoDb.Empresas;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Services.MongoDb.Interfaces;

public interface IMongoEmpresaService
{
    Task<EmpresaDocument> CreateAsync(
        CreateEmpresaRequest request,
        CancellationToken cancellationToken = default);

    Task<EmpresaDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmpresaDocument>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<EmpresaDocument> UpdateAsync(
        string id,
        UpdateEmpresaRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<DashboardEmpresaMongoResponse> GetDashboardAsync(
        string empresaId,
        CancellationToken cancellationToken = default);

}
