using DocumentFormat.OpenXml.Drawing.Charts;
using MComponents.ExportData;
using MComponents.MForm;
using MComponents.Shared.Attributes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MComponents.MGrid
{
    public class MGrid<T> : ComponentBase, IMGrid<T>
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public string Identifier { get; set; } = typeof(T).Name;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public Func<T> ModelFactory { get; set; }

        [Parameter]
        public IEnumerable<T> DataSource { get; set; }

        [Parameter]
        public IMGridDataAdapter<T> DataAdapter { get; set; }

        [Parameter]
        public IMGridObjectFormatter<T> Formatter { get; set; }

        [Parameter]
        public bool EnableAdding { get; set; }

        [Parameter]
        public bool EnableEditing { get; set; }

        [Parameter]
        public bool EnableDeleting { get; set; }

        [Parameter]
        public bool EnableUserSorting { get; set; }

        [Parameter]
        public bool EnableFilterRow { get; set; }

        [Parameter]
        public bool EnableExport { get; set; }

        [Parameter]
        public bool EnableImport { get; set; }

        [Parameter]
        public bool EnableSaveState { get; set; }

        [Parameter]
        public ToolbarItem ToolbarItems { get; set; }

        [Parameter]
        public MGridInitialState InitialState { get; set; }

        [Parameter]
        public string HtmlTableClass { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public MGridStateService StateService { get; set; }

        [Inject]
        public IStringLocalizer L { get; set; }

        internal List<SortInstruction> SortInstructions { get; set; } = new List<SortInstruction>();
        internal List<FilterInstruction> FilterInstructions { get; set; } = new List<FilterInstruction>();
        internal List<SortInstruction> GroupByInstructions { get; set; } = new List<SortInstruction>();

        internal List<(IDictionary<string, object>, T)> HiddenGroupByKeys = new List<(IDictionary<string, object>, T)>();

        protected object[] mLastGroupByKeys;

        internal Dictionary<IMGridPropertyColumn, IMPropertyInfo> PropertyInfos = new Dictionary<IMGridPropertyColumn, IMPropertyInfo>();

        public MGridPager Pager { get; set; }
        public MGridEvents<T> Events { get; set; }

        public List<IMGridColumn> ColumnsList { get; set; } = new List<IMGridColumn>();

        public T Selected;
        public T EditRow;

        public MForm<T> EditForm;

        public bool IsFilterRowVisible { get; internal set; }

        protected EditContext EditContext;

        internal T EditValue;
        internal T NewValue;

        protected ICollection<T> DataCache;
        protected long DataCountCache;
        protected long TotalDataCountCache;

        protected IEnumerable<IGrouping<object, T>> GroupedDataCache;

        protected object mFilterModel;

        protected ElementReference mTableReference;


        protected double[] mColumnsWidth;

        protected BoundingBox mFieldBoundingBox;
        protected double mTableBorderLeft;
        protected double mTableBorderTop;

        protected SorterBuilder<T> mSorter = new SorterBuilder<T>();
        protected FilterBuilder<T> mFilter = new FilterBuilder<T>();
        //  protected GroupByBuilder<T> mGrouper = new GroupByBuilder<T>();

        protected bool mHasActionColumn;

        public bool IsEditingRow => EditRow != null;

        public bool FixedColumns => (IsEditingRow || IsFilterRowVisible) && !UpdateColumnsWidthOnNextRender;

        public IMGridColumn[] VisibleColumns => ColumnsList.Where(c => c.ShouldRenderColumn).ToArray();

        public bool UpdateColumnsWidthOnNextRender;

        protected string mInputFileId = Guid.NewGuid().ToString();


        protected DotNetObjectReference<MGrid<T>> mObjReference;

        protected DotNetObjectReference<MGrid<T>> ObjReference
        {
            get
            {
                if (mObjReference == null)
                {
                    mObjReference = DotNetObjectReference.Create(this);
                }
                return mObjReference;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Formatter == null)
                Formatter = new MGridDefaultObjectFormatter<T>();

            Formatter.L = this.L;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if ((EnableAdding || EnableDeleting) && DataSource != null && DataSource is T[])
                throw new ArgumentException($"{DataSource} can not be an array. It must be a source which supports adding and deleting");

            ClearDataCache();
        }

        public void RegisterColumn(IMGridColumn pColumn)
        {
            if (ColumnsList.Any(c => c.Identifier == pColumn.Identifier))
                throw new ArgumentException($"A column with {nameof(IMGridColumn.Identifier)} {pColumn.Identifier} already exists. Please specify a custom value");

            ColumnsList.Add(pColumn);

            if (pColumn is MGridActionColumn<T>)
                mHasActionColumn = true;

            if (pColumn is IMGridPropertyColumn propc)
            {
                if (string.IsNullOrWhiteSpace(propc.Property))
                {
                    throw new ArgumentException($"Provide a {nameof(IMGridPropertyColumn.Property)} for column {propc.Identifier}");
                }

                var iprop = ReflectionHelper.GetIMPropertyInfo(typeof(T), propc.Property, propc.PropertyType);

                propc.PropertyType = iprop.PropertyType;

                if (propc.Attributes != null)
                {
                    iprop.SetAttributes(propc.Attributes);
                }

                PropertyInfos.Add(propc, iprop);

                if (pColumn is IMGridSortableColumn sc && sc.SortDirection != MSortDirection.None)
                {
                    SortInstructions.Add(new SortInstruction()
                    {
                        GridColumn = propc,
                        Direction = sc.SortDirection,
                        PropertyInfo = iprop,
                        Index = sc.SortIndex,
                        Comparer = pColumn.GetComparer()
                    });
                }
            }

            mFilterModel = null;
            ClearDataCache();
            StateHasChanged();
        }

        public void RegisterPagerSettings(MGridPager pPager)
        {
            Pager = pPager;
            StateHasChanged();
        }

        public void RegisterEvents(MGridEvents<T> pEvents)
        {
            Events = pEvents;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UpdateColumnsWidth();

                if (EnableSaveState)
                    StateService.RestoreGridState(this);
            }

            if (UpdateColumnsWidthOnNextRender)
            {
                await UpdateColumnsWidth();
                UpdateColumnsWidthOnNextRender = false;

                StateHasChanged();
            }

            await UpdateDataCacheIfDataAdapter();
        }

        [JSInvokable]
        public void JsInvokeKeyDown(string pKey)
        {
            if (pKey == "Escape")
            {
                InvokeAsync(() => _ = StopEditing(true, true));
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenRegion(this.GetHashCode());

            if (this.ChildContent != null)
            {
                RenderFragment child() =>
                        (builder2) =>
                        {
                            builder2.AddContent(56, this.ChildContent);
                        };

                builder.OpenComponent<CascadingValue<MGrid<T>>>(4);
                builder.AddAttribute(207, "Value", this);
                builder.AddAttribute(208, "ChildContent", child());
                builder.CloseComponent();
            }


            RenderFragment childMain() =>
                   (builder2) =>
                   {
                       builder2.OpenElement(216, "div");
                       builder2.AddAttribute(217, "class", "m-grid-container");
                       builder2.AddMultipleAttributes(218, AdditionalAttributes);


                       builder2.OpenElement(221, "div");
                       builder2.AddAttribute(222, "class", "m-btn-toolbar");
                       builder2.AddAttribute(223, "role", "toolbar");

                       if (ToolbarItems != ToolbarItem.None)
                       {
                           builder2.OpenElement(227, "div");
                           builder2.AddAttribute(228, "class", "m-btn-group mr-2");
                           builder2.AddAttribute(229, "role", "group");

                           if (EnableAdding && ToolbarItems.HasFlag(ToolbarItem.Add))
                           {
                               builder2.OpenElement(233, "button");
                               builder2.AddAttribute(234, "class", "m-btn m-btn-primary");
                               builder2.AddAttribute(235, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnToolbarAdd));
                               builder2.AddContent(236, (MarkupString)$"<i class=\"fa fa-plus\"></i> {L["Add"]}");
                               builder2.CloseElement(); // button
                           }

                           if (EnableEditing && ToolbarItems.HasFlag(ToolbarItem.Edit))
                           {
                               builder2.OpenElement(242, "button");
                               builder2.AddAttribute(243, "class", "m-btn m-btn-primary");
                               builder2.AddAttribute(244, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnToolbarEdit));
                               builder2.AddContent(245, (MarkupString)$"<i class=\"fa fa-edit\"></i> {L["Edit"]}");
                               builder2.CloseElement(); // button
                           }

                           if (EnableDeleting && ToolbarItems.HasFlag(ToolbarItem.Delete))
                           {
                               builder2.OpenElement(251, "button");
                               builder2.AddAttribute(252, "class", "m-btn m-btn-primary");
                               builder2.AddAttribute(253, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnToolbarRemove));
                               builder2.AddContent(254, (MarkupString)$"<i class=\"fa fa-trash-alt\"></i> {L["Delete"]}");
                               builder2.CloseElement(); // button
                           }

                           builder2.CloseElement(); // div
                       }

                       if (EnableFilterRow)
                       {
                           builder2.OpenElement(263, "button");
                           builder2.AddAttribute(264, "class", "m-btn m-btn-primary m-btn-sm");
                           builder2.AddAttribute(265, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnToggleFilter));
                           builder2.OpenElement(266, "i");
                           builder2.AddAttribute(267, "class", "fas fa-filter");
                           builder2.CloseElement(); //i
                           builder2.AddContent(269, L["Filter"]);
                           builder2.CloseElement(); //button
                       }


                       builder2.CloseElement(); // div


                       builder2.OpenElement(277, "table");


                       if (HtmlTableClass != null)
                       {
                           builder2.AddAttribute(282, "class", HtmlTableClass + (EnableEditing ? " m-clickable" : string.Empty) + (IsEditingRow ? " m-editing" : string.Empty));
                       }
                       else
                       {
                           builder2.AddAttribute(286, "class", "m-grid m-grid-striped m-grid-bordered m-grid-hover" + (EnableEditing ? " m-clickable" : string.Empty) + (IsEditingRow ? " m-editing" : string.Empty));
                       }

                       if (FixedColumns)
                       {
                           builder2.AddAttribute(291, "style", "table-layout: fixed;");
                       }

                       builder2.AddElementReferenceCapture(294, (__value) =>
                       {
                           mTableReference = __value;
                       });

                       builder2.OpenElement(299, "thead");
                       builder2.OpenElement(300, "tr");

                       for (int i = 0; i < ColumnsList.Count; i++)
                       {
                           IMGridColumn column = ColumnsList[i];

                           if (!column.ShouldRenderColumn)
                               continue;

                           builder2.OpenElement(309, "th");
                           builder2.AddAttribute(310, "data-identifier", column.Identifier);

                           if (column.AdditionalAttributes != null)
                               builder2.AddMultipleAttributes(312, column.AdditionalAttributes.Where(k => k.Key != "style"));

                           if (FixedColumns)
                           {
                               var width = GetColumnWidth(i);

                               string style = $"width: {width.ToString(CultureInfo.InvariantCulture)}px;";

                               if (column.AdditionalAttributes != null && column.AdditionalAttributes.ContainsKey("style"))
                               {
                                   style += column.AdditionalAttributes["style"].ToString();
                               }

                               builder2.AddAttribute(313, "style", style); //same sequence than below !!
                           }
                           else
                           {
                               if (column.AdditionalAttributes != null && column.AdditionalAttributes.ContainsKey("style"))
                               {
                                   builder2.AddAttribute(313, "style", column.AdditionalAttributes["style"].ToString());
                               }
                           }

                           builder2.AddAttribute(318, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, a => OnColumnHeaderClick(column, a)));
                           builder2.AddAttribute(319, "scope", "col");

                           builder2.AddContent(321, (MarkupString)column.HeaderText);

                           if (EnableUserSorting)
                           {
                               var sortInstr = SortInstructions.FirstOrDefault(si => si.GridColumn == column);
                               if (sortInstr != null)
                               {
                                   if (sortInstr.Direction == MSortDirection.Ascending)
                                       builder2.AddContent(329, (MarkupString)"<i class=\"fa fa-arrow-down m-grid-header-icon\"></i>");

                                   if (sortInstr.Direction == MSortDirection.Descending)
                                       builder2.AddContent(332, (MarkupString)"<i class=\"fa fa-arrow-up m-grid-header-icon\"></i>");
                               }
                           }

                           builder2.CloseElement(); //th
                       }

                       builder2.CloseElement(); //tr
                       builder2.CloseElement(); //thead

                       builder2.AddMarkupContent(342, "\r\n");
                       builder2.OpenElement(434, "tbody");

                       if (DataCache == null)
                       {
                           if (DataSource != null)
                           {
                               if (GroupByInstructions.Any())
                               {
                                   GroupedDataCache = FetchGroupByPartsAndUpdateDataCount();
                               }
                               else
                               {
                                   DataCache = GetIQueryable(DataSource, true).ToArray();
                               }
                           }
                           else if (DataAdapter == null)
                               throw new InvalidOperationException("Please provide a " + nameof(DataSource) + " or " + nameof(DataAdapter));
                       }

                       if (IsFilterRowVisible)
                       {
                           AddFilterRow(builder2);
                       }

                       if (GroupedDataCache != null)
                       {
                           var lastValues = new object[GroupByInstructions.Count];

                           foreach (var grouped in GroupedDataCache)
                           {
                               object key = grouped.Key;

                               int index = 0;
                               foreach (var prop in key.GetType().GetProperties())
                               {
                                   var keyValue = prop.GetValue(key);

                                   bool renderRow = lastValues[index] == null;

                                   if (lastValues[index] != null)
                                   {
                                       renderRow = !lastValues[index].Equals(keyValue);
                                   }

                                   for (int i = index; i >= 1; i--)
                                   {
                                       var keyValuesRow = MGridGroupByAnonymousTypeHelper.GetKeyValues(key, i);
                                       renderRow = !HiddenGroupByKeys.Any(h => Extensions.DictionaryEqual(h.Item1, keyValuesRow));
                                       if (renderRow == false)
                                           break;
                                   }

                                   if (renderRow)
                                   {
                                       var instr = GroupByInstructions[index];
                                       AddGroupByHeaderRow(builder2, instr, key, keyValue, index + 1, grouped.FirstOrDefault());
                                   }

                                   lastValues[index] = keyValue;
                                   index++;
                               }

                               foreach (var entry in grouped)
                               {
                                   AddContentRow(builder2, (T)entry, MGridAction.Edit);
                               }
                           }
                       }
                       else if (DataCache != null)
                       {
                           foreach (var entry in DataCache)
                           {
                               AddContentRow(builder2, entry, MGridAction.Edit);
                           }
                       }

                       if (NewValue != null)
                           AddContentRow(builder2, NewValue, MGridAction.Add);

                       builder2.AddMarkupContent(24, "\r\n");
                       builder2.CloseElement(); //tbody

                       builder2.CloseElement(); // table
                       builder2.AddMarkupContent(391, "\r\n");

                       if (Pager != null)
                       {
                           long dataCount = DataCountCache;

                           long pagecount = (long)Math.Ceiling(dataCount / (double)Pager.PageSize);

                           if (Pager.CurrentPage > pagecount && dataCount >= 0)
                           {
                               _ = OnPagerPageChanged(pagecount);
                           }

                           builder2.OpenComponent<MPager>(11);
                           builder2.AddAttribute(398, "CurrentPage", Pager.CurrentPage);
                           builder2.AddAttribute(399, "PageCount", pagecount);
                           builder2.AddAttribute(400, "OnPageChanged", EventCallback.Factory.Create<long>(this, OnPagerPageChanged));

                           builder2.AddAttribute(402, "ChildContent", (RenderFragment)((builder3) =>
                           {
                               builder3.AddMarkupContent(404, "\r\n\r\n    ");
                               builder3.OpenElement(405, "div");
                               builder3.AddAttribute(406, "class", "m-pagination-entry m-pagination-tools");
                               builder3.AddMarkupContent(407, "\r\n        ");

                               if (Pager.SelectablePageSizes != null)
                               {
                                   builder3.OpenElement(411, "select");
                                   builder3.AddAttribute(412, "class", "m-form-control");

                                   builder3.AddAttribute(413, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnPageSizeChange));

                                   foreach (var entry in Pager.SelectablePageSizes)
                                   {
                                       builder3.OpenElement(418, "option");
                                       builder3.AddAttribute(419, "value", entry);

                                       if (entry == Pager.PageSize)
                                       {
                                           builder3.AddAttribute(419, "selected", "selected");
                                       }

                                       builder3.AddContent(420, entry);
                                       builder3.CloseElement();
                                   }

                                   builder3.AddMarkupContent(424, "\r\n");
                                   builder3.CloseElement(); //select
                               }

                               builder3.AddMarkupContent(428, "\r\n");

                               var count = DataCache?.Count;
                               if (GroupedDataCache != null)
                               {
                                   count = GroupedDataCache.Sum(g => g.Count());
                               }

                               builder3.AddMarkupContent(429, $"<span class=\"m-pagination-descr\">{string.Format(L["{0} entries of {1}"], count, TotalDataCountCache)}</span>");

                               if (EnableImport)
                               {
                                   builder3.OpenElement(560, "button");
                                   builder3.AddAttribute(561, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                                   builder3.AddAttribute(562, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnBtnImportClicked));
                                   builder3.AddContent(563, (MarkupString)"<i class=\"fas fa-upload\"></i>");
                                   builder3.CloseElement(); // button

                                   builder3.OpenComponent<InputFile>(566);
                                   builder3.AddAttribute(567, "accept", ".xlsx");
                                   builder3.AddAttribute(568, "OnChange", EventCallback.Factory.Create<InputFileChangeEventArgs>(this, OnFileChange));
                                   builder3.AddAttribute(567, "style", "visibility: hidden; position: absolute; top: -1000px;");
                                   builder3.AddAttribute(567, "id", mInputFileId);
                                   builder3.CloseElement(); // InputFile
                               }
                               if (EnableExport)
                               {
                                   builder3.OpenElement(435, "button");
                                   builder3.AddAttribute(436, "class", "m-btn m-btn-secondary m-btn-icon m-btn-sm");
                                   builder3.AddAttribute(437, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ExportContent()));
                                   builder3.AddContent(438, (MarkupString)"<i class=\"fas fa-download\"></i>");
                                   builder3.CloseElement(); // button
                               }

                               builder3.CloseElement(); //div
                           }
                           ));

                           builder2.CloseComponent();
                       }


                       builder2.CloseElement(); //div 
                   };

            builder.AddContent(453, childMain());

            builder.CloseRegion();
        }

        private IQueryable<T> GetIQueryable(IEnumerable<T> pSource, bool pUpdateDataCount)
        {
            var data = pSource as IQueryable<T>;

            if (data == null)
                data = pSource.AsQueryable();

            if (pUpdateDataCount)
                TotalDataCountCache = data.LongCount();

            if (FilterInstructions.Count > 0)
            {
                data = mFilter.FilterBy(data, FilterInstructions);
            }

            if (pUpdateDataCount)
                DataCountCache = data.LongCount();

            if (GroupByInstructions.Count > 0)
            {
                data = mSorter.SortBy(data, GroupByInstructions);
            }

            if (SortInstructions.Count > 0)
            {
                data = mSorter.SortBy(data, SortInstructions);
            }

            if (Pager != null && GroupByInstructions.Count <= 0)
            {
                data = data.Skip((int)(Pager.PageSize * (Math.Max(0, Pager.CurrentPage - 1)))).Take(Pager.PageSize);
            }

            return data;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                if (InitialState == MGridInitialState.AddNewRow)
                {
                    _ = StartAdd(false);
                }
            }
        }

        private void GetNestedExpandoValue(string[] pProperties, object pValue, ref Dictionary<string, object> pParent)
        {
            if (pProperties.Length > 1)
            {
                var property = pProperties[0];

                var dict = new Dictionary<string, object>();
                pParent.Add(property, dict);

                GetNestedExpandoValue(pProperties.Skip(1).ToArray(), pValue, ref dict);
                return;
            }

            pParent.Add(pProperties[0], pValue);
        }

        private void AddFilterRow(RenderTreeBuilder pBuilder)
        {
            if (!FixedColumns)
                return;

            if (mFilterModel == null)
            {
                var fmodel = new ExpandoObject() as IDictionary<string, object>;

                foreach (var column in ColumnsList)
                {
                    if (column is IMGridPropertyColumn pc)
                    {
                        object value = null;
                        var instr = FilterInstructions.Where(f => f.GridColumn == pc).FirstOrDefault();

                        if (instr != null)
                            value = instr.Value;

                        if (!pc.Property.Contains("."))
                            fmodel.Add(pc.Property, value);
                        else
                        {
                            var properties = pc.Property.Split('.');
                            var dict = new Dictionary<string, object>();
                            GetNestedExpandoValue(properties.Skip(1).ToArray(), value, ref dict);
                            fmodel.Add(properties.First(), dict);
                        }
                    }
                }

                mFilterModel = fmodel;
            }

            pBuilder.OpenElement(487, "tr");
            pBuilder.AddAttribute(489, "class", "m-grid-row m-grid-edit-row m-grid-filter-row");

            AddInlineTrHeight(pBuilder);

            AddEditRow<ExpandoObject>(pBuilder, MGridAction.FilterRow, true, (ExpandoObject)mFilterModel);

            pBuilder.CloseElement();
        }

        protected void AddGroupByHeaderRow(RenderTreeBuilder pBuilder, SortInstruction pInstruction, object pKey, object pKeyValue, int pColumnIndex, T pFirstValue)
        {
            pBuilder.OpenElement(615, "tr");
            pBuilder.AddAttribute(616, "class", "m-grid-row");

            var keyValuesRow = MGridGroupByAnonymousTypeHelper.GetKeyValues(pKey, pColumnIndex);

            pBuilder.AddAttribute(617, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (a) =>
            {
                var existing = HiddenGroupByKeys.FirstOrDefault(p => Extensions.DictionaryEqual(p.Item1, keyValuesRow));

                if (existing.Item1 != null)
                {
                    HiddenGroupByKeys.Remove(existing);
                }
                else
                {
                    HiddenGroupByKeys.Add((keyValuesRow, pFirstValue));
                }

                Selected = default(T);
                await ResetRowsAndCache();
            }));

            AddInlineTrHeight(pBuilder);

            if (pColumnIndex > 1)
            {
                pBuilder.OpenElement(689, "td");
                pBuilder.AddAttribute(690, "colspan", pColumnIndex - 1);
                pBuilder.CloseElement(); //td
            }

            pBuilder.OpenElement(695, "td");

            var hidden = HiddenGroupByKeys.FirstOrDefault(p => Extensions.DictionaryEqual(p.Item1, keyValuesRow));

            T value = pFirstValue;

            if (hidden.Item1 != null)
            {
                value = hidden.Item2;
                pBuilder.AddMarkupContent(697, "<i class=\"fas fa-arrow-right\"></i>");
            }
            else
            {
                pBuilder.AddMarkupContent(697, "<i class=\"fas fa-arrow-down\"></i>");
            }

            if (value == null)
            {
                var hiddenChild = HiddenGroupByKeys.FirstOrDefault(p => Extensions.DictionaryEqualIfContains(p.Item1, keyValuesRow));

                if (hiddenChild.Item1 != null)
                {
                    value = hiddenChild.Item2;
                }
            }

            pBuilder.CloseElement(); //td

            pBuilder.OpenElement(577, "td");
            pBuilder.AddAttribute(618, "colspan", VisibleColumns.Length - pColumnIndex - 1);

            var pi = pInstruction.PropertyInfo;

            pBuilder.AddContent(625, pInstruction.GridColumn.HeaderText + ": ");
            RenderValueTdContent(pBuilder, value, pInstruction.GridColumn);

            pBuilder.CloseElement(); //td

            pBuilder.CloseElement(); //tr
        }

        private void AddContentRow(RenderTreeBuilder pBuilder, T pEntry, MGridAction pAction)
        {
            bool rowEdit = EditRow != null && EditRow.Equals(pEntry);

            pBuilder.AddMarkupContent(503, "\r\n");
            pBuilder.OpenElement(504, "tr");

            string cssClass = "m-grid-row";

            if (rowEdit)
            {
                cssClass += " m-grid-edit-row";
            }

            bool selected = Selected != null && Selected.Equals(pEntry);
            if (selected)
                cssClass += " m-grid-highlight";

            Formatter.AppendToTableRow(pBuilder, ref cssClass, pEntry, selected);

            pBuilder.AddAttribute(519, "class", cssClass);

            pBuilder.AddAttribute(522, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, (a) =>
            {
                OnRowClick(pEntry, a);
            }));

            pBuilder.AddEventStopPropagationAttribute(527, "onclick", true);

            /*
            pBuilder.AddAttribute(530, "ondblclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (a) =>
            {
                await StartEditRow(pEntry, a);
            }));
            */

            if (rowEdit)
            {
                EditValue = pEntry;
                AddEditRow(pBuilder, pAction, false, EditValue);
            }
            else
            {
                for (int i = 0; i < ColumnsList.Count; i++)
                {
                    IMGridColumn column = ColumnsList[i];

                    if (!column.ShouldRenderColumn)
                        continue;

                    pBuilder.OpenElement(550, "td");
                    RenderValueTdContent(pBuilder, pEntry, column);

                    pBuilder.CloseElement(); //td
                }
            }

            pBuilder.CloseElement(); //tr
        }

        private void RenderValueTdContent(RenderTreeBuilder pBuilder, T pEntry, IMGridColumn column)
        {
            Formatter.AppendToTableRowData(pBuilder, column, pEntry);

            if (column is IMGridColumnGenerator<T> generator)
            {
                pBuilder.AddContent(556, generator.GenerateContent(pEntry));
            }
            else if (column is IMGridPropertyColumn pc)
            {
                var iprop = PropertyInfos[pc];
                pBuilder.AddContent(561, Formatter.FormatPropertyColumnValue(pc, iprop, pEntry));
            }
        }

        private void AddEditRow<M>(RenderTreeBuilder pBuilder, MGridAction pAction, bool pIsFilterRow, M pValue)
        {
            AddInlineTrHeight(pBuilder);

            pBuilder.OpenElement(577, "td");
            pBuilder.AddAttribute(578, "colspan", VisibleColumns.Length);

            pBuilder.OpenElement(580, "table");
            pBuilder.OpenElement(581, "tbody");
            pBuilder.OpenElement(582, "tr");

            {

                pBuilder.OpenComponent<MForm<M>>(53);
                pBuilder.AddAttribute(587, nameof(MForm<M>.Model), pValue);
                pBuilder.AddAttribute(588, nameof(MForm<M>.IsInTableRow), true);
                pBuilder.AddAttribute(589, nameof(MForm<M>.Fields), (RenderFragment)((builder3) =>
                {
                    AddInlineFormFields(builder3, VisibleColumns, pIsFilterRow);
                }));

                pBuilder.AddAttribute(594, nameof(MForm<M>.MFormGridContext), new MFormGridContext()
                {
                    Action = pAction
                });

                if (pIsFilterRow)
                {
                    pBuilder.AddAttribute(601, nameof(MForm<M>.EnableValidation), false);
                    pBuilder.AddAttribute(602, "data-is-filterrow", true);

                    pBuilder.AddAttribute(604, nameof(MForm<M>.OnValueChanged),
                        EventCallback.Factory.Create<MFormValueChangedArgs<ExpandoObject>>(this, OnFilterValueChanged));
                }
                else
                {
                    //T == M here

                    pBuilder.AddAttribute(611, nameof(MForm<T>.OnValidSubmit), EventCallback.Factory.Create<MFormSubmitArgs>(this, async (a) =>
                    {
                        await OnFormSubmit(a);
                    }));

                    pBuilder.AddAttribute(616, nameof(MForm<T>.OnValueChanged), EventCallback.Factory.Create<MFormValueChangedArgs<T>>(this, OnEditValueChanged));

                    pBuilder.AddComponentReferenceCapture(618, (__value) =>
                    {
                        EditForm = (MForm<T>)__value;
                    });
                }

                pBuilder.CloseComponent();
            }

            pBuilder.CloseElement(); //tr
            pBuilder.CloseElement(); //tbody
            pBuilder.CloseElement(); //table

            pBuilder.CloseElement(); //td  
        }

        private void AddInlineTrHeight(RenderTreeBuilder pBuilder)
        {
            var inlineTrHeight = mFieldBoundingBox.Height;

            if (mFieldBoundingBox.BorderCollapse == "collapse")
            {
                inlineTrHeight += mFieldBoundingBox.BorderTop / 2 - mTableBorderTop / 2;
            }

            pBuilder.AddAttribute(643, "style", $"height: {(inlineTrHeight).ToString(CultureInfo.InvariantCulture)}px");
        }

        private void AddInlineFormFields(RenderTreeBuilder pBuilder, IMGridColumn[] pVisibleColumns, bool pIsFilterRow)
        {
            double left = 0;

            for (int i = 0; i < pVisibleColumns.Length; i++)
            {
                IMGridColumn column = pVisibleColumns[i];

                var columnWidth = GetColumnWidth(i);

                var bWidth = columnWidth;
                var bHeight = mFieldBoundingBox.Height;

                if (mFieldBoundingBox.BorderCollapse == "separate")
                {
                    bWidth -= mFieldBoundingBox.BorderRight;
                }
                else if (mFieldBoundingBox.BorderCollapse == "collapse")
                {
                    if (i == 0)
                    {
                        bWidth -= mFieldBoundingBox.BorderRight / 2;
                    }
                    else
                        bWidth -= mFieldBoundingBox.BorderRight;

                    if (i == 0 || i == pVisibleColumns.Length - 1)
                    {
                        bWidth -= mTableBorderLeft / 2;
                    }
                }

                BoundingBox box = new BoundingBox()
                {
                    BorderTop = 0,
                    BorderRight = mFieldBoundingBox.BorderRight,
                    BorderSpace = mFieldBoundingBox.BorderSpace,
                    BorderCollapse = mFieldBoundingBox.BorderCollapse,

                    Width = bWidth,
                    Height = bHeight,
                };

                AddMFormField(pBuilder, column, pIsFilterRow, left, box);

                if (mFieldBoundingBox.BorderCollapse == "separate")
                {
                    left += columnWidth + mFieldBoundingBox.BorderSpace;
                }
                else if (mFieldBoundingBox.BorderCollapse == "collapse")
                {
                    if (i == 0)
                    {
                        left += columnWidth + mFieldBoundingBox.BorderRight / 2 + mFieldBoundingBox.BorderSpace;
                        left -= mTableBorderLeft / 2;
                    }
                    else
                    {
                        left += columnWidth + mFieldBoundingBox.BorderSpace;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"border-collapse {mFieldBoundingBox.BorderCollapse} not supported!");
                }
            }
        }

        private void AddMFormField(RenderTreeBuilder pBuilder, IMGridColumn pColumn, bool pIsInFilterRow, double pLeftOffset, BoundingBox pBoundingBox)
        {
            if (pColumn is IMGridPropertyColumn pc)
            {
                var propertyType = PropertyInfos[pc].PropertyType;

                if (pIsInFilterRow)
                    propertyType = GetNullableTypeIfNeeded(propertyType);

                var method = typeof(MGrid<T>).GetMethod(nameof(MGrid<T>.AddPropertyField), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
                method.Invoke(this, new object[] { pBuilder, pColumn, pc, pIsInFilterRow, pLeftOffset, pBoundingBox });
            }
            else if (pColumn is IMGridEditFieldGenerator<T> fieldGenerator)
            {
                AddFieldGenerator(pBuilder, fieldGenerator, pIsInFilterRow, pLeftOffset, pBoundingBox);
            }
            else
            {
                pBuilder.OpenComponent<MField>(5);
                pBuilder.CloseComponent();
            }
        }

        private static Type GetNullableTypeIfNeeded(Type pPropertyType)
        {
            if (pPropertyType.IsValueType && (!pPropertyType.IsGenericType || pPropertyType.GetGenericTypeDefinition() != typeof(Nullable<>)))
            {
                pPropertyType = typeof(Nullable<>).MakeGenericType(pPropertyType);
            }

            return pPropertyType;
        }

        private void AddPropertyField<TProperty>(RenderTreeBuilder pBuilder, IMGridColumn pColumn, IMGridPropertyColumn pPropertyColumn, bool pIsInFilterRow, double pLeftOffset, BoundingBox pBoundingBox)
        {
            var attributes = PropertyInfos[pPropertyColumn].GetAttributes()?.ToList() ?? new List<Attribute>();

            if (pIsInFilterRow)
            {
                if (!pColumn.EnableFilter && !attributes.OfType<ReadOnlyAttribute>().Any())
                {
                    attributes.Add(new ReadOnlyAttribute());
                }
                if (pColumn.EnableFilter && attributes.Any(a => a is ReadOnlyAttribute))
                {
                    attributes = attributes.Where(a => !(a is ReadOnlyAttribute)).ToList();
                }
            }

            if (pColumn is IMGridComplexEditableColumn<TProperty> complex)
            {
                pBuilder.OpenComponent<MComplexPropertyField<TProperty>>(5);
                pBuilder.AddAttribute(776, nameof(MField.Property), pPropertyColumn.Property);
                pBuilder.AddAttribute(777, nameof(MField.PropertyType), typeof(TProperty));
                pBuilder.AddAttribute(778, nameof(MField.Attributes), attributes.ToArray());
                pBuilder.AddAttribute(779, nameof(IMGridColumn), pColumn);

                if (complex.FormTemplate != null && !pIsInFilterRow || (pIsInFilterRow && pColumn.EnableFilter))
                    pBuilder.AddAttribute(781, nameof(MComplexPropertyField<TProperty>.Template), complex.FormTemplate);

                pBuilder.AddStyleWithAttribute(784, Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE, pLeftOffset, pBoundingBox);

                pBuilder.CloseComponent();
            }
            else
            {
                pBuilder.OpenComponent<MField>(790);

                pBuilder.AddAttribute(792, nameof(MField.Property), pPropertyColumn.Property);
                pBuilder.AddAttribute(793, nameof(MField.PropertyType), typeof(TProperty));
                pBuilder.AddAttribute(794, nameof(MField.Attributes), attributes.ToArray());
                pBuilder.AddAttribute(795, nameof(IMGridColumn), pColumn);

                pBuilder.AddStyleWithAttribute(976, Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE, pLeftOffset, pBoundingBox);

                pBuilder.CloseComponent();
            }
        }

        private void AddFieldGenerator(RenderTreeBuilder pBuilder, IMGridEditFieldGenerator<T> pFieldGenerator, bool pIsInFilterRow, double pLeftOffset, BoundingBox pBoundingBox)
        {
            pBuilder.OpenElement(804, "td");
            pBuilder.AddStyleWithAttribute(805, "style", pLeftOffset, pBoundingBox);
            pBuilder.AddContent(807, pFieldGenerator.EditFieldTemplate(pLeftOffset, pBoundingBox, pIsInFilterRow));
            pBuilder.CloseElement();
        }

        protected async void OnRowClick(T pValue, MouseEventArgs pMouseArgs)
        {
            if (EditRow != null && EditRow.Equals(pValue))
                return;

            if (Events?.OnBeginRowSelect != null)
            {
                var args = new BeginRowSelectArgs<T>()
                {
                    Row = pValue,
                    MouseEventArgs = pMouseArgs
                };

                await Events.OnBeginRowSelect.InvokeAsync(args);

                if (args.Cancelled)
                    return;
            }

            if (Selected != null && Selected.Equals(pValue))
            {
                if (EditRow != null && !EditRow.Equals(Selected))
                {
                    await StopEditing(true, true);
                    await StartEdit(pValue, true, pMouseArgs);
                }
                else if (EditRow == null)
                {
                    await StartEdit(pValue, true, pMouseArgs);
                }
                return;
            }

            if (EditRow != null)
            {
                await StopEditing(true, true);
            }

            Selected = pValue;

            SaveCurrentState();
            StateHasChanged();
        }

        protected async Task<bool> StopEditing(bool pInvokeSubmit, bool pUserInteracted)
        {
            if (pInvokeSubmit && EditForm != null)
            {
                //We move the current focus away from the input so changed value event is called first and
                //Mform adds current value to the changedValues list
                await JsRuntime.InvokeVoidAsync("mcomponents.clearFocus");
                await Task.Delay(50);

                if (EditForm == null)
                    return false;

                var success = await EditForm.CallLocalSubmit(pUserInteracted);
                if (!success)
                    return false;
            }

            EditForm = null;
            NewValue = default(T);
            EditRow = default(T);

            if (mObjReference != null)
                _ = JsRuntime.InvokeVoidAsync("mcomponents.unRegisterKeyListener", Identifier);

            _ = InvokeAsync(StateHasChanged);
            return true;
        }

        public async Task StartEditRow(T pValue, MouseEventArgs pMouseEventArgs)
        {
            await StartEdit(pValue, true, pMouseEventArgs);
        }

        private async Task StartEdit(T pRow, bool pUserInteracted, MouseEventArgs pMouseEventArgs)
        {
            if (EditRow != null && EditRow.Equals(pRow))
                return;

            if (!EnableEditing)
                return;

            bool success = await StopEditing(true, pUserInteracted);

            if (!success)
                return;

            T toSelect = pRow ?? Selected;

            if (toSelect == null)
                return;

            if (Events?.OnBeginEdit != null)
            {
                var args = new BeginEditArgs<T>()
                {
                    Row = toSelect,
                    MouseEventArgs = pMouseEventArgs
                };

                await Events.OnBeginEdit.InvokeAsync(args);

                if (args.Cancelled)
                    return;
            }

            await UpdateColumnsWidth();

            EditRow = toSelect;
            Selected = EditRow;

            await JsRuntime.InvokeVoidAsync("mcomponents.registerKeyListener", Identifier, ObjReference);
            StateHasChanged();
        }

        private async Task UpdateColumnsWidth()
        {
            var values = await JsRuntime.InvokeAsync<string[]>("mcomponents.getColumnSizes", new object[] { mTableReference, ColumnsList.Select(c => c.Identifier).ToArray() });

            if (values == null || values.Length <= 0)
                return;

            mTableBorderTop = values[0].FromPixelToDouble();
            mTableBorderLeft = values[1].FromPixelToDouble();

            mFieldBoundingBox = new BoundingBox()
            {
                BorderRight = values[2].FromPixelToDouble(),
                BorderTop = values[3].FromPixelToDouble(),
                BorderSpace = values[4].FromPixelToDouble(),
                BorderCollapse = values[5],
                Height = values[6].FromPixelToDouble(),
            };

            mColumnsWidth = values.Skip(7).Select(v => v.FromPixelToDouble()).ToArray();
        }

        public async Task StartAdd(bool pUserInteracted)
        {
            var success = await StopEditing(true, pUserInteracted);

            if (!success)
                return;

            var newv = CreateNewT();

            if (Events?.OnBeginAdd != null)
            {
                var args = new BeginAddArgs<T>()
                {
                    Row = newv
                };

                await Events.OnBeginAdd.InvokeAsync(args);

                if (args.Cancelled)
                    return;
            }

            EditRow = newv;
            Selected = newv;
            NewValue = newv;

            await UpdateColumnsWidth();

            await JsRuntime.InvokeVoidAsync("mcomponents.registerKeyListener", Identifier, ObjReference);
            StateHasChanged();
        }

        protected async void OnToolbarAdd()
        {
            await StartAdd(true);
        }

        protected async void OnToolbarEdit(MouseEventArgs pMouseEventArgs)
        {
            if (EditRow != null)
            {
                await StopEditing(true, true);
                StateHasChanged();
                return;
            }

            await StartEdit(default(T), true, pMouseEventArgs);
        }

        protected async void OnToolbarRemove(MouseEventArgs pMouseEventArgs)
        {
            var success = await StopEditing(true, true);

            //stop editing failed, but force deletion
            if (!success && NewValue != null)
            {
                await StopEditing(false, false);
                return;
            }

            if (Selected == null)
                return;

            await StartDeleteRow(Selected, pMouseEventArgs);
        }

        protected async Task OnToggleFilter()
        {
            await SetFilterRowVisible(!IsFilterRowVisible);
        }

        public async Task SetFilterRowVisible(bool pVisible)
        {
            IsFilterRowVisible = pVisible;

            if (IsFilterRowVisible)
            {
                await UpdateColumnsWidth();
            }
            else
            {
                ClearFilterValues();
            }

            SaveCurrentState();
            InvokeStateHasChanged();
        }

        public async Task StartDeleteRow(T value, MouseEventArgs pMouseEventArgs)
        {
            if (!EnableDeleting)
                return;

            if (Events?.OnBeginDelete != null)
            {
                var args = new BeginDeleteArgs<T>()
                {
                    Row = value,
                    MouseEventArgs = pMouseEventArgs
                };

                await Events.OnBeginDelete.InvokeAsync(args);

                if (args.Cancelled)
                    return;
            }

            if (DataSource is ICollection<T> coll)
                coll.Remove(value);

            if (DataAdapter != null)
            {
                await DataAdapter.Remove(value);
            }

            if (Events?.OnAfterDelete != null)
            {
                await Events.OnAfterDelete.InvokeAsync(new AfterDeleteArgs<T>()
                {
                    Row = value,
                    MouseEventArgs = pMouseEventArgs
                });
            }

            Selected = default(T);
            ClearDataCache();
            await UpdateDataCacheIfDataAdapter();

            if (mObjReference != null)
                _ = JsRuntime.InvokeVoidAsync("mcomponents.unRegisterKeyListener", Identifier);

            InvokeStateHasChanged();
        }

        internal async Task OnFormSubmit(MFormSubmitArgs args)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////
            // WARNING: Code is Redundant because with DataAdapter ContinueWith is required !
            //////////////////////////////////////////////////////////////////////////////////////////////////

            // START


            if (args.ChangedValues.Count > 0 || args.UserInteracted)
            {
                if (NewValue == null)
                {
                    T value = (T)args.Model;

                    if (DataSource is Collection<T> ds) //update obversable collection
                    {
                        int index = ds.IndexOf(value);
                        ds[index] = value;
                    }

                    if (DataAdapter != null)
                    {
                        // WARNING: Code is Redundant because with DataAdapter ContinueWith is required !
                        _ = DataAdapter.Update(value).ContinueWith(async t =>
                        {
                            if (Events?.OnAfterEdit != null)
                            {
                                await Events.OnAfterEdit.InvokeAsync(new AfterEditArgs<T>()
                                {
                                    Row = value
                                });
                            }

                            ClearDataCache();
                            await UpdateDataCacheIfDataAdapter();
                        });
                        return;
                    }

                    if (Events?.OnAfterEdit != null)
                    {
                        await Events.OnAfterEdit.InvokeAsync(new AfterEditArgs<T>()
                        {
                            Row = value
                        });
                    }
                }
                else
                {
                    if (DataSource is ICollection<T> coll)
                        coll.Add(NewValue);

                    if (DataAdapter != null)
                    {
                        // WARNING: Code is Redundant because with DataAdapter ContinueWith is required !
                        _ = DataAdapter.Add(NewValue).ContinueWith(async t =>
                        {
                            if (Events?.OnAfterAdd != null)
                            {
                                await Events.OnAfterAdd.InvokeAsync(new AfterAddArgs<T>()
                                {
                                    Row = NewValue
                                });
                            }

                            ClearDataCache();
                            await UpdateDataCacheIfDataAdapter();
                            await StopEditing(false, false);
                        });
                        return;
                    }

                    if (Events?.OnAfterAdd != null)
                    {
                        await Events.OnAfterAdd.InvokeAsync(new AfterAddArgs<T>()
                        {
                            Row = NewValue
                        });
                    }
                }

                ClearDataCache();
            }

            await StopEditing(false, false);

            //END

            //////////////////////////////////////////////////////////////////////////////////////////////////
            // WARNING: Code is Redundant because with DataAdapter ContinueWith is required !
            //////////////////////////////////////////////////////////////////////////////////////////////////
        }

        protected async Task OnPagerPageChanged(long pPage)
        {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
            Pager.CurrentPage = pPage;
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

            await StopEditing(false, false);

            ClearDataCache();
            await UpdateDataCacheIfDataAdapter();

            SaveCurrentState();
            StateHasChanged();
        }

        protected async Task OnPageSizeChange(ChangeEventArgs pValue)
        {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
            Pager.PageSize = int.Parse(pValue.Value.ToString());
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

            await StopEditing(false, false);

            ClearDataCache();
            await UpdateDataCacheIfDataAdapter();

            SaveCurrentState();
            StateHasChanged();
        }

        protected async Task OnColumnHeaderClick(IMGridColumn pColumn, MouseEventArgs pArgs)
        {
            if (!EnableUserSorting)
                return;

            if (!(pColumn is IMGridSortableColumn sc) || !sc.EnableSorting)
                return;

            var propInfoColumn = pColumn as IMGridPropertyColumn;

            if (propInfoColumn == null)
                return; // other columns not supported yet

            var propInfo = PropertyInfos[propInfoColumn];

            object comparer = pColumn.GetComparer();

            if (pArgs.CtrlKey && pArgs.ShiftKey)
            {
                var groupByInstr = GroupByInstructions.FirstOrDefault(s => s.GridColumn == pColumn);

                if (groupByInstr == null)
                {
                    GroupByInstructions.Add(new SortInstruction()
                    {
                        GridColumn = propInfoColumn,
                        Direction = MSortDirection.Ascending,
                        PropertyInfo = propInfo,
                        Index = SortInstructions.Count,
                        Comparer = comparer
                    });

                    ColumnsList.Insert(0, new MGridGroupByColumn<T>()
                    {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
                        Identifier = "groupby_" + pColumn.Identifier + "_" + Guid.NewGuid(),
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
                        GridColumn = pColumn
                    });
                }
                else
                {
                    GroupByInstructions.Remove(groupByInstr);
                    ColumnsList.RemoveAll(r => r is MGridGroupByColumn<T> gc && gc.GridColumn == pColumn);
                }

                await ResetRowsAndCache();
            }
            else
            {
                var instr = SortInstructions.FirstOrDefault(s => s.GridColumn == pColumn);

                if (instr == null)
                {
                    if (!pArgs.ShiftKey)
                        SortInstructions.Clear();

                    SortInstructions.Add(new SortInstruction()
                    {
                        GridColumn = propInfoColumn,
                        Direction = MSortDirection.Ascending,
                        PropertyInfo = propInfo,
                        Index = SortInstructions.Count,
                        Comparer = comparer
                    });
                }
                else if (instr.Direction == MSortDirection.Ascending)
                {
                    instr.Direction = MSortDirection.Descending;
                }
                else if (instr.Direction == MSortDirection.Descending)
                {
                    SortInstructions.Remove(instr);
                }

                await StopEditing(false, false);

                ClearDataCache();
                await UpdateDataCacheIfDataAdapter();

                SaveCurrentState();
                StateHasChanged();
            }
        }

        protected async void OnEditValueChanged(MFormValueChangedArgs<T> pArgs)
        {
            if (Events?.OnFormValueChanged != null)
            {
                await Events.OnFormValueChanged.InvokeAsync(pArgs);
            }
        }

        protected async Task OnFilterValueChanged(MFormValueChangedArgs<ExpandoObject> pArgs)
        {
            var column = (IMGridColumn)pArgs.Field.AdditionalAttributes[nameof(IMGridColumn)];

            FilterInstructions.RemoveAll(f => f.GridColumn.Identifier == column.Identifier);

            var iprop = PropertyInfos.First(v => v.Value.GetFullName() == pArgs.PropertyInfo.GetFullName()).Value;

            if (pArgs.NewValue != null && !(pArgs.NewValue is string a && a == string.Empty)) //all values from filter are nullable
            {
                FilterInstructions.Add(new FilterInstruction()
                {
                    GridColumn = column,
                    PropertyInfo = iprop,
                    Value = pArgs.NewValue
                });
            }

            await ResetRowsAndCache();
        }

        public async Task ResetRowsAndCache()
        {
            await StopEditing(false, false);

            ClearDataCache();
            await UpdateDataCacheIfDataAdapter();

            UpdateColumnsWidthOnNextRender = true;

            SaveCurrentState();
            StateHasChanged();
        }

        public async void ExportContent(string pFileName = "Export.xlsx")
        {
            IEnumerable<T> dataForExport = DataSource;

            if (dataForExport == null && DataAdapter != null)
            {
                dataForExport = await DataAdapter.GetData(Enumerable.Empty<T>().AsQueryable());
            }

            var data = ExcelExportHelper.GetExcelSpreadsheet<T>(ColumnsList, PropertyInfos, dataForExport, Formatter);
            await FileUtil.SaveAs(JsRuntime, pFileName, data);
        }

        protected void OnBtnImportClicked()
        {
            _ = JsRuntime.InvokeVoidAsync("mcomponents.invokeClick", mInputFileId);
        }

        protected async Task OnFileChange(InputFileChangeEventArgs e)
        {
            var file = e.GetMultipleFiles(1)[0];

            if (file == null)
                return;

            try
            {
                using var stream = file.OpenReadStream(long.MaxValue);

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                stream.Close();

                ms.Position = 0;
                await ImportContent(ms);
                ms.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Notificator.InvokeNotification(true, "Error Importing file");
                Notificator.InvokeNotification(true, ex.Message);
            }
        }

        public async Task ImportContent(Stream pStream)
        {
            await ExcelImportHelper.ImportFile<T>(this, pStream);
        }

        internal T CreateNewT()
        {
            if (ModelFactory != null)
            {
                return ModelFactory();
            }

            return (T)Activator.CreateInstance(typeof(T));
        }

        protected void ClearDataCache()
        {
            DataCache = null;
            DataCountCache = -1;
            TotalDataCountCache = -1;
            GroupedDataCache = null;
        }

        protected async Task UpdateDataCacheIfDataAdapter()
        {
            if (DataCache == null && DataAdapter != null)
            {
                var queryable = GetIQueryable(Enumerable.Empty<T>(), false);

                await Task.Run(async () => await DataAdapter.GetData(queryable)).ContinueWith(async a =>
                {
                    DataCache = a.Result.ToArray();

                    await Task.Run(async () => await DataAdapter.GetDataCount(queryable)).ContinueWith(async a =>
                    {
                        DataCountCache = a.Result;

                        await Task.Run(async () => await DataAdapter.GetTotalDataCount()).ContinueWith(a =>
                        {
                            TotalDataCountCache = a.Result;
                            InvokeAsync(() => StateHasChanged());
                        });
                    });
                });
            }
        }

        public void Refresh()
        {
            ClearDataCache();
            InvokeStateHasChanged();
        }

        public void ClearColumns()
        {
            ColumnsList.Clear();
            PropertyInfos.Clear();
            ClearFilterValues();
            Refresh();
        }

        public void InvokeStateHasChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        public async Task<bool> SavePendingChanges(bool pUserInteracted)
        {
            return await StopEditing(true, pUserInteracted);
        }

        protected double GetColumnWidth(int pIndex)
        {
            if (pIndex < mColumnsWidth.Length)
            {
                return mColumnsWidth[pIndex];
            }

            throw new InvalidOperationException("Column width is unknown");
        }

        public void ClearFilterValues()
        {
            FilterInstructions.Clear();
            mFilterModel = null;

            SaveCurrentState();
            Refresh();
        }

        public void SaveCurrentState()
        {
            if (EnableSaveState)
                StateService.SaveGridState(this);
        }

        public List<IGrouping<object, T>> FetchGroupByPartsAndUpdateDataCount()
        {
            List<IGrouping<object, T>> data = new List<IGrouping<object, T>>();

            //TODO we could cache these values in the future and check if results matches the expected filter key
            //if the results does not match the calculated skip and take indexes, the collection changed in the database
            var keyCounts = MGridGroupByHelper.GetGroupKeyCounts<T>(GetIQueryable(DataSource, true), GroupByInstructions.Select(p => p.PropertyInfo));

            long skipvalues = 0;

            if (Pager != null)
                skipvalues = Pager.PageSize * (Math.Max(0, Pager.CurrentPage - 1));

            var hiddenDict = HiddenGroupByKeys.Select(h => h.Item1).ToArray();

            var keys = MGridGroupByHelper.GetKeys(keyCounts, skipvalues, Pager?.PageSize, hiddenDict);

            foreach (var entry in keys)
            {
                var keyObj = entry.DynamicKeyObj;

                T[] groupedPart;

                if (entry.Take > 0)
                {
                    var skip = entry.Offset;

                    var propInfos = keyObj.GetType().GetProperties().Select(p => (p, PropertyInfos.First(pp => pp.Value.Name == p.Name))).ToList();

                    var filterInstr = propInfos.Select(p => new FilterInstruction()
                    {
                        PropertyInfo = p.Item2.Value,
                        Value = p.p.GetValue(keyObj),
                        MatchExact = true
                    }).ToArray();

                    var filtered = mFilter.FilterBy(GetIQueryable(DataSource, false), filterInstr);

                    groupedPart = filtered.Skip((int)skip).Take((int)entry.Take).ToArray();
                }
                else
                {
                    groupedPart = Array.Empty<T>();
                }

                var part = new MGridGrouping<T>(keyObj, groupedPart);
                data.Add(part);
            }

            DataCountCache = MGridGroupByHelper.GetDataCount(keyCounts, hiddenDict);

            return data;
        }
    }
}
