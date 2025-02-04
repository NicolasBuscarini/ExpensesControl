using CodeFlow.Data.Context.Package.Base.Repositories;
using ExpensesControl.Domain.Entities.AggregateRoot;

namespace ExpensesControl.Infrastructure.SqlServer.Repositories.Interface;

public interface IRevenueRepository : IBaseRepository<Revenue, int>
{
}
