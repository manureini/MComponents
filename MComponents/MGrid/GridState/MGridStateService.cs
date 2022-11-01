using MComponents.Services;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public class MGridStateService
    {
        private static readonly MethodInfo FirstOrDefaultMethod =
              typeof(Queryable).GetMethods()
              .Where(method => method.Name == "FirstOrDefault")
              .Where(method => method.GetParameters().Length == 2)
              .First();

        protected MLocalStorageService mPersistService;

        public MGridStateService(MLocalStorageService pPersistService)
        {
            mPersistService = pPersistService;
        }

        public void SaveGridState<T>(MGrid<T> pGrid)
        {
            var state = GetGridState(pGrid);
            _ = mPersistService.SetValueAsync(pGrid, state);
        }

#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
        public async Task RestoreGridState<T>(MGrid<T> pGrid)
        {
            try
            {
                var state = await mPersistService.GetValueAsync<MGridState>(pGrid);

                if (state == null)
                    return;

                pGrid.SelectRow(state.SelectedRow);

                if (pGrid.Pager != null)
                {
                    if (state.Page != null)
                        pGrid.Pager.CurrentPage = Math.Max(1, state.Page.Value);
                    if (state.PageSize != null)
                        pGrid.Pager.PageSize = Math.Max(1, state.PageSize.Value);
                }

                pGrid.FilterInstructions = state.FilterState.Select(filterState =>
                {
                    var column = pGrid.ColumnsList.FirstOrDefault(c => c.Identifier == filterState.ColumnIdentifier);

                    if (column == null || !(column is IMGridPropertyColumn propc))
                        return null;

                    IMPropertyInfo pi = pGrid.PropertyInfos[propc];

                    object value = null;

                    if (filterState.ReferencedId != null && column.GetType().Name == typeof(MGridComplexPropertyColumn<object, object>).Name)
                    {
                        value = GetReferencedValue(pGrid, column, propc.PropertyType, filterState.ReferencedId);
                    }
                    else
                    {
                        var jsone = filterState.Value as JsonElement?;

                        try
                        {
                            value = jsone?.ToObject(pi.PropertyType);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
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
                        GridColumn = propc,
                        Direction = s.Direction,
                        Index = s.Index,
                        PropertyInfo = pi,
                        Comparer = column.GetComparer()
                    };
                }).Where(s => s != null).ToList();

                await pGrid.SetFilterRowVisible(state.IsFilterRowVisible);

                pGrid.ClearDataCache();
                pGrid.InvokeStateHasChanged();
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        protected object GetReferencedValue<T>(MGrid<T> pGrid, IMGridColumn column, Type pPropertyType, string pReferencedId)
        {
            var propInfo = column.GetType().GetProperty(nameof(MGridComplexPropertyColumn<object, object>.ReferencedValues));

            var referencedValues = propInfo.GetValue(column) as IEnumerable;
            var enumerator = referencedValues.GetEnumerator();
            if (!enumerator.MoveNext())
                return null;

            var firstValue = enumerator.Current;

            var pi = pGrid.GetIdentifierProperty(firstValue);
            //no idict support right now

            var refIdValue = ReflectionHelper.ChangeType(pReferencedId, pi.PropertyType);

            var queryable = referencedValues.AsQueryable();

            var param = Expression.Parameter(pPropertyType, "p");
            var property = Expression.Property(param, pi.Name);
            var exprEqual = Expression.Equal(property, Expression.Constant(refIdValue));

            var lambda = Expression.Lambda(exprEqual, param);

            var method = FirstOrDefaultMethod.MakeGenericMethod(new[] { pPropertyType });
            var val = method.Invoke(null, new object[] { queryable, lambda });
            return val;
        }



#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

        public MGridState GetGridState<T>(MGrid<T> pGrid)
        {
            return new MGridState()
            {
                IsFilterRowVisible = pGrid.IsFilterRowVisible,
                Page = pGrid.Pager?.CurrentPage,
                PageSize = pGrid.Pager?.PageSize,
                SelectedRow = pGrid.GetIdentifierValue(pGrid.Selected),

                FilterState = pGrid.FilterInstructions.Select(f =>
                {
                    if (f.GridColumn.GetType().Name == typeof(MGridComplexPropertyColumn<object, object>).Name)
                    {
                        return new MGridFilterState()
                        {
                            ColumnIdentifier = f.GridColumn.Identifier,
                            ReferencedId = pGrid.GetIdentifierValue(f.Value)
                        };
                    }

                    return new MGridFilterState()
                    {
                        ColumnIdentifier = f.GridColumn.Identifier,
                        Value = f.Value
                    };
                }
                ).Where(f => f != null).ToArray(),

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
