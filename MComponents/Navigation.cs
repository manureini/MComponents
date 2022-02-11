﻿
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MComponents
{
    public sealed class Navigation : IDisposable
    {
        private const int MIN_HISTORY_SIZE = 256;
        private const int ADDITION_HISTORY_SIZE = 64;
        private const string SPECIAL_RELOAD_PAGE = "__reload_page__";

        private readonly NavigationManager mNavigationManager;
        private readonly List<string> mHistory;

        private int mSkipAddingHistory = 0;

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

            if (replace)
            {
                mSkipAddingHistory++;
            }

            mNavigationManager.NavigateTo(url, forceLoad, replace);
#else
            mNavigationManager.NavigateTo(url, forceLoad);
#endif
        }

        public void NavigateTo<T>(bool forceLoad = false, bool replace = false)
        {
            var routeAttribute = typeof(T).GetCustomAttribute<RouteAttribute>();

            if (routeAttribute == null)
                throw new ArgumentException($"{typeof(T).FullName} does not have {nameof(RouteAttribute)}");

            NavigateTo(routeAttribute.Template, forceLoad, replace);
        }

        public void NavigateTo<T>(Guid pId, bool forceLoad = false, bool replace = false)
        {
            var routeAttribute = typeof(T).GetCustomAttributes<RouteAttribute>().FirstOrDefault(r => r.Template.Contains("{Id:guid}", StringComparison.InvariantCultureIgnoreCase));

            if (routeAttribute == null)
                throw new ArgumentException($"{typeof(T).FullName} does not have {nameof(RouteAttribute)} with template contains {{Id:guid}}");

            NavigateTo(routeAttribute.Template.Replace("{Id:guid}", pId.ToString(), StringComparison.InvariantCultureIgnoreCase), forceLoad, replace);
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

            if (mSkipAddingHistory > 0)
            {
                mSkipAddingHistory--;
                return;
            }

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
            NavigateTo(url, false, true);
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
