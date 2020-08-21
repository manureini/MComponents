using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Services
{
    public class MPersistService
    {
        internal const string LOCAL_STORAGE_PREFIX = "mcomponents_";

        protected ILocalStorageService mLocalStorage;

        public MPersistService(ILocalStorageService pLocalStorageService)
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

        public static string GetKey(IIdentifyable pIdentifyable)
        {
            return LOCAL_STORAGE_PREFIX + pIdentifyable.GetType().Name + pIdentifyable.Identifier;
        }
    }
}
