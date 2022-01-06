
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;

namespace MComponents
{
    public sealed class Navigation : IDisposable
    {
        private const int MIN_HISTORY_SIZE = 256;
        private const int ADDITION_HISTORY_SIZE = 64;
        private const string SPECIAL_RELOAD_PAGE = "__reload_page__";

        private readonly NavigationManager mNavigationManager;
        private readonly List<string> mHistory;

        public string BaseUri => mNavigationManager.BaseUri;
        public string Uri => mNavigationManager.Uri;

        public Navigation(NavigationManager navigationManager)
        {
            mNavigationManager = navigationManager;
            mNavigationManager.LocationChanged += OnLocationChanged;

            mHistory = new List<string>(MIN_HISTORY_SIZE + ADDITION_HISTORY_SIZE);
            mHistory.Add(mNavigationManager.Uri);
        }

        public void NavigateTo(string url, bool forceLoad = false, bool replace = false)
        {
#if NET6_0_OR_GREATER
            mNavigationManager.NavigateTo(url, forceLoad, replace);
#else
            mNavigationManager.NavigateTo(url, forceLoad);
#endif
        }

        public bool CanNavigateBack => mHistory.Count >= 2;

        public void NavigateBack()
        {
            if (!CanNavigateBack)
                return;

            var backPageUrl = mHistory[^2];
            mHistory.RemoveRange(mHistory.Count - 2, 2);
            mNavigationManager.NavigateTo(backPageUrl);
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            if (e.Location != null && e.Location.EndsWith(SPECIAL_RELOAD_PAGE))
                return;

            EnsureSize();
            mHistory.Add(e.Location);
        }

        private void EnsureSize()
        {
            if (mHistory.Count < MIN_HISTORY_SIZE + ADDITION_HISTORY_SIZE)
                return;

            mHistory.RemoveRange(0, mHistory.Count - MIN_HISTORY_SIZE);
        }

        public Uri ToAbsoluteUri(string relativeUri)
        {
            return mNavigationManager.ToAbsoluteUri(relativeUri);
        }

        public void ReloadCurrentPage()
        {
            string url = mNavigationManager.Uri;

#if NET6_0_OR_GREATER
            mNavigationManager.NavigateTo("/" + SPECIAL_RELOAD_PAGE, false, true);
            mNavigationManager.NavigateTo(url, false, true);
#else
            mNavigationManager.NavigateTo("/" + SPECIAL_RELOAD_PAGE, false);
            mNavigationManager.NavigateTo(url, false);
#endif
        }
        public void Dispose()
        {
            mNavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
