using MComponents.MGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MComponents.DataAdapter
{
    public class CreateIfNeededDataAdapter<T> : IMGridDataAdapter<T>
    {
        protected IMGridDataAdapter<T> mDataAdapter;
        protected ICollection<T> mNotCreatedData;
        protected Func<IEnumerable<T>, IEnumerable<T>> mAdditionalDataProcessor;
        protected Func<T, bool> mShouldValueBeCreatedCheck;

        protected HashSet<T> mCreatedValues = new HashSet<T>();

        public CreateIfNeededDataAdapter(IMGridDataAdapter<T> pDataAdapter, ICollection<T> pNotCreatedData, Func<T, bool> pShouldValueBeCreatedCheck, Func<IEnumerable<T>, IEnumerable<T>> pAdditionalDataProcessor = null)
        {
            mDataAdapter = pDataAdapter;
            mNotCreatedData = pNotCreatedData;
            mAdditionalDataProcessor = pAdditionalDataProcessor;
            mShouldValueBeCreatedCheck = pShouldValueBeCreatedCheck;
        }

        public async Task<IEnumerable<T>> GetData(IQueryable<T> pQueryable)
        {
            var data = await mDataAdapter.GetData(pQueryable);
            data = data.Concat(mNotCreatedData);
            data = mAdditionalDataProcessor(data);
            return data;
        }

        public async Task<long> GetDataCount(IQueryable<T> pQueryable)
        {
            var count = await mDataAdapter.GetDataCount(pQueryable);
            count += mNotCreatedData.Count; //filter with queryable is missing here
            return count;
        }

        public async Task<long> GetTotalDataCount()
        {
            var count = await mDataAdapter.GetTotalDataCount();
            count += mNotCreatedData.Count;
            return count;
        }

        public async Task<T> Add(T pNewValue)
        {
            if (!ShouldValueBeCreated(pNewValue))
                return pNewValue;

            var newValue = await mDataAdapter.Add(pNewValue);

            mNotCreatedData.Remove(pNewValue);
            mCreatedValues.Add(newValue);

            return newValue;
        }

        public async Task Remove(T pValue)
        {
            await mDataAdapter.Remove(pValue);

            mCreatedValues.Remove(pValue);
            mNotCreatedData.Add(pValue);
        }

        public async Task Update(T pValue)
        {
            if (ShouldValueBeCreated(pValue))
            {
                if (!ValueExistsOnServer(pValue))
                {
                    await Add(pValue);
                    return;
                }

                await mDataAdapter.Update(pValue);
            }
            else
            {
                if (ValueExistsOnServer(pValue))
                {
                    await Remove(pValue);
                }
            }
        }

        public bool ValueExistsOnServer(T pValue)
        {
            if (mCreatedValues.Contains(pValue))
                return true;

            if (mNotCreatedData.Contains(pValue))
                return false;

            return true;
        }

        public virtual bool ShouldValueBeCreated(T pValue)
        {
            return mShouldValueBeCreatedCheck(pValue);
        }
    }
}
