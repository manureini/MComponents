using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using MComponents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public class MGridStateService
    {
        protected MLocalStorageService mPersistService;

        public MGridStateService(MLocalStorageService pPersistService)
        {
            mPersistService = pPersistService;
        }

        public void SaveGridState<T>(MGrid<T> pGrid)
        {
            var state = GetGridState(pGrid);
            mPersistService.SetValueAsync(pGrid, state);
        }

#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
        public void RestoreGridState<T>(MGrid<T> pGrid)
        {
            _ = Task.Run(() => mPersistService.GetValueAsync<MGridState>(pGrid))
                .ContinueWith(s =>
              {
                  try
                  {
                      var state = s.Result.Result;

                      if (state == null)
                          return;

                      pGrid.Selected = state.SelectedRow;

                      if (pGrid.Pager != null)
                      {
                          if (state.Page != null)
                              pGrid.Pager.CurrentPage = state.Page.Value;
                          if (state.PageSize != null)
                              pGrid.Pager.PageSize = state.PageSize.Value;
                      }

                      pGrid.FilterInstructions = state.FilterState.Select(f =>
                      {
                          var column = pGrid.ColumnsList.FirstOrDefault(c => c.Identifier == f.ColumnIdentifier);

                          if (column == null || !(column is IMGridPropertyColumn propc))
                              return null;

                          IMPropertyInfo pi = pGrid.PropertyInfos[propc];

                          if (f.Value == null || !(f.Value is JsonElement jsone))
                              return null;

                          object value = null;

                          try
                          {
                              value = jsone.ToObject(pi.PropertyType);
                          }
                          catch (Exception e)
                          {
                              Console.WriteLine(e.ToString());
                          }

                          if (value == null)
                              return null;

                          return new FilterInstruction()
                          {
                              Value = value,
                              GridColumn = column,
                              PropertyInfo = pi
                          };
                      }).Where(f => f != null).ToList();

                      pGrid.SortInstructions = state.SorterState.Select(s =>
                      {
                          var column = pGrid.ColumnsList.FirstOrDefault(c => c.Identifier == s.ColumnIdentifier);

                          if (column == null || !(column is IMGridPropertyColumn propc))
                              return null;

                          IMPropertyInfo pi = pGrid.PropertyInfos[propc];

                          return new SortInstruction()
                          {
                              GridColumn = column,
                              Direction = s.Direction,
                              Index = s.Index,
                              PropertyInfo = pi,
                              Comparer = column.GetComparer()
                          };
                      }).Where(s => s != null).ToList();

                      _ = pGrid.SetFilterRowVisible(state.IsFilterRowVisible).ContinueWith(t =>
                        {
                            pGrid.Refresh();
                        });
                  }
                  catch (Exception e)
                  {
                      Console.Write(e);
                  }
              });
        }
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

        public static MGridState GetGridState<T>(MGrid<T> pGrid)
        {
            return new MGridState()
            {
                IsFilterRowVisible = pGrid.IsFilterRowVisible,
                Page = pGrid.Pager?.CurrentPage,
                PageSize = pGrid.Pager?.PageSize,
                SelectedRow = pGrid.Selected,

                FilterState = pGrid.FilterInstructions.Select(f => new MGridFilterState()
                {
                    ColumnIdentifier = f.GridColumn.Identifier,
                    Value = f.Value
                }).Where(f => f != null).ToArray(),

                SorterState = pGrid.SortInstructions.Select(s => new MGridSorterState()
                {
                    ColumnIdentifier = s.GridColumn.Identifier,
                    Direction = s.Direction,
                    Index = s.Index
                }).Where(s => s != null).ToArray()
            };
        }
    }
}
