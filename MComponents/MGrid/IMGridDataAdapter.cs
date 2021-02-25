using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public interface IMGridDataAdapter<T>
    {
        Task<IEnumerable<T>> GetData(IQueryable<T> pQueryable);

        Task<long> GetDataCount(IQueryable<T> pQueryable);

        Task<long> GetTotalDataCount();

        Task Add(T pNewValue);

        Task Remove(T pValue);

        Task Update(T pValue);
    }
}
