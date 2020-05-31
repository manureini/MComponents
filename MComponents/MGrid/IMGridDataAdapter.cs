using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public interface IMGridDataAdapter<T>
    {
        Task<IEnumerable<T>> GetData(IQueryable<T> pQueryable);

        Task<long> GetDataCount(IQueryable<T> pQueryable);

        Task<long> GetTotalDataCount();

        Task Add(Guid pId, T pNewValue);

        Task Remove(Guid pId, T pValue);

        Task Update(Guid pId, T pValue);
    }
}
