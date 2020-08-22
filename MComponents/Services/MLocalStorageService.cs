using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Services
{
    public class MLocalStorageService
    {
        internal const string LOCAL_STORAGE_PREFIX = "mcomponents_";

        protected ILocalStorageService mLocalStorage;

        public MLocalStorageService(ILocalStorageService pLocalStorageService)
        {
            mLocalStorage = pLocalStorageService;
        }

        public ValueTask<T> GetValueAsync<T>(IIdentifyable pIdentifyable)
        {
            return mLocalStorage.GetItemAsync<T>(GetKey(pIdentifyable));
        }

        public ValueTask SetValueAsync<T>(IIdentifyable pIdentifyable, T pValue)
        {
            return mLocalStorage.SetItemAsync(GetKey(pIdentifyable), pValue);
        }

        public async ValueTask<ICollection<string>> GetAllMComponentKeys()
        {
            int length = await mLocalStorage.LengthAsync();

            var keys = new List<string>();

            for (int i = 0; i < length; i++)
            {
                var key = await mLocalStorage.KeyAsync(i);

                if (!key.StartsWith(LOCAL_STORAGE_PREFIX))
                    continue;

                keys.Add(key);
            }

            return keys;
        }

        public async ValueTask<Dictionary<string, object>> GetAllMComponentValuesDict()
        {
            var keys = await GetAllMComponentKeys();

            var values = new Dictionary<string, object>();

            foreach (var key in keys)
            {
                var value = await mLocalStorage.GetItemAsync<object>(key);
                values.Add(key, value);
            }

            return values;
        }

        public async Task RemoveAllMComponentValues()
        {
            var keys = await GetAllMComponentKeys();

            foreach (var key in keys)
            {
                _ = mLocalStorage.RemoveItemAsync(key);
            }
        }

        public static string GetKey(IIdentifyable pIdentifyable)
        {
            return LOCAL_STORAGE_PREFIX + pIdentifyable.GetType().Name + pIdentifyable.Identifier;
        }
    }
}
