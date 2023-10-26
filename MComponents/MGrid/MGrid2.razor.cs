using MComponents.ExportData;
using MComponents.MForm;
using MComponents.Notifications;
using MComponents.Services;
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
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MComponents.Resources;

namespace MComponents.MGrid
{
    public partial class MGrid2<T> : IMGrid<T>
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
        public bool EnableAdding { get; set; } = MGridSettings.Instance.EnableAdding;

        [Parameter]
        public bool EnableEditing { get; set; } = MGridSettings.Instance.EnableEditing;

        [Parameter]
        public bool EnableDeleting { get; set; } = MGridSettings.Instance.EnableDeleting;

        [Parameter]
        public bool EnableUserSorting { get; set; } = MGridSettings.Instance.EnableUserSorting;

        [Parameter]
        public bool EnableFilterRow { get; set; } = MGridSettings.Instance.EnableFilterRow;

        [Parameter]
        public bool EnableGrouping { get; set; } = MGridSettings.Instance.EnableGrouping;

        [Parameter]
        public bool EnableExport { get; set; } = MGridSettings.Instance.EnableExport;

        [Parameter]
        public bool EnableImport { get; set; } = MGridSettings.Instance.EnableImport;

        [Parameter]
        public bool EnableSaveState { get; set; } = MGridSettings.Instance.EnableSaveState;

        [Parameter]
        public ToolbarItem ToolbarItems { get; set; } = MGridSettings.Instance.ToolbarItems;

        [Parameter]
        public MGridInitialState InitialState { get; set; } = MGridSettings.Instance.InitialState;

        [Parameter]
        public string HtmlTableClass { get; set; } = MGridSettings.Instance.HtmlTableClass;

        [Parameter]
        public bool UseStaticLayoutMode { get; set; } = MGridSettings.Instance.UseStaticLayoutMode;

        [Parameter]
        public string NoDataDescription { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public MGridStateService StateService { get; set; }

        [Inject]
        public MComponentSettings MComponentSettings { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        internal List<SortInstruction> SortInstructions { get; set; } = new List<SortInstruction>();
        internal List<FilterInstruction> FilterInstructions { get; set; } = new List<FilterInstruction>();
        internal List<SortInstruction> GroupByInstructions { get; set; } = new List<SortInstruction>();

        internal List<(IDictionary<string, object>, T)> HiddenGroupByKeys = new List<(IDictionary<string, object>, T)>();

        protected object[] mLastGroupByKeys;

        internal Dictionary<IMGridPropertyColumn, IMPropertyInfo> PropertyInfos = new Dictionary<IMGridPropertyColumn, IMPropertyInfo>();

        public MGridPager Pager { get; set; }
        public MGridEvents<T> Events { get; set; }

        public List<IMGridColumn> ColumnsList { get; set; } = new List<IMGridColumn>();

        internal string mSelectedRowId;
        protected bool mFirstRender = true;
        protected bool mFirstRenderUpdateColumnWidths = true;

        private T mSelected;
        public T Selected
        {
            get => mSelected;
            set
            {
                mSelected = value;
                mSelectedRowId = null;
            }
        }

        public T EditRow { get; set; }

        public object EditFormSetter
        {
            set
            {
                EditForm = (MForm<T>)value;
            }
        }

        public MForm<T> EditForm;

        public bool IsFilterRowVisible { get; internal set; }
        public bool IsGroupingVisible { get; internal set; }

        protected EditContext EditContext;

        internal T EditValue;
        internal T NewValue;

        protected ICollection<T> DataCache;
        protected long DataCountCache = -1;
        protected long TotalDataCountCache;

        protected IEnumerable<IGrouping<object, T>> GroupedDataCache;

        protected object mFilterModel;

        protected ElementReference mTableReference;

        protected double[] mColumnsWidth;
        protected double mTableContainerWidth;

        internal BoundingBox mFieldBoundingBox;
        internal double mTableBorderLeft;
        internal double mTableBorderTop;

        protected SorterBuilder<T> mSorter = new SorterBuilder<T>();
        protected FilterBuilder<T> mFilter = new FilterBuilder<T>();

        protected SemaphoreSlim mDataAdapterSemaphore = new SemaphoreSlim(1, 1);
        //  protected GroupByBuilder<T> mGrouper = new GroupByBuilder<T>();

        public bool IsEditingRow => EditRow != null;

        public bool FixedColumns => (IsEditingRow || IsFilterRowVisible) && !UpdateColumnsWidthOnNextRender;

        public IMGridColumn[] VisibleColumns => ColumnsList.Where(c => c.ShouldRenderColumn).ToArray();

        public bool UpdateColumnsWidthOnNextRender { get; set; }

        protected bool mUpdateDataCacheOnNextRender;
        internal bool mIsLoading;

        protected string mInputFileId = Guid.NewGuid().ToString();
        protected IMPropertyInfo mIdentifierProperty;

        protected DotNetObjectReference<MGrid2<T>> mObjReference;

        protected DotNetObjectReference<MGrid2<T>> ObjReference
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
            {
                if (MGridSettings.Instance.FormatterFactory != null)
                {
                    Formatter = MGridSettings.Instance.FormatterFactory.GetFormatter<T>();
                }
                else
                {
                    Formatter = new MGridDefaultObjectFormatter<T>();
                }
            }

            Formatter.L = this.L;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if ((EnableAdding || EnableDeleting) && DataSource != null && DataSource is T[])
                throw new ArgumentException($"{DataSource} can not be an array. It must be a source which supports adding and deleting");

            if (DataSource != null)
            {
                ClearDataCache();
            }

            if (DataAdapter != null)
            {
                mUpdateDataCacheOnNextRender = true;
            }

            StateHasChanged();
        }

        public void RegisterColumn(IMGridColumn pColumn)
        {
            if (ColumnsList.Any(c => c.Identifier == pColumn.Identifier))
                throw new ArgumentException($"A column with {nameof(IMGridColumn.Identifier)} {pColumn.Identifier} already exists. Please specify a custom value");

            ColumnsList.Add(pColumn);

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
                    if (propc.ExtendAttributes)
                    {
                        iprop.SetAttributes(iprop.GetAttributes().Concat(propc.Attributes).ToArray());
                    }
                    else
                    {
                        iprop.SetAttributes(propc.Attributes);
                    }
                }

                if (propc.HeaderText == null)
                {
                    var displayAttribute = iprop.GetCustomAttribute<DisplayAttribute>();
                    if (displayAttribute != null)
                    {
                        propc.HeaderText = displayAttribute.GetName();
                    }
                    else
                    {
                        propc.HeaderText = propc.Property;
                    }
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

        //todo
        /*
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (EnableSaveState)
                {
                    await StateService.RestoreGridState(this);
                }

                await UpdateDataCacheIfDataAdapter();

                mFirstRender = false;
                StateHasChanged();
            }

            if (UpdateColumnsWidthOnNextRender)
            {
                UpdateColumnsWidthOnNextRender = false;
                await UpdateColumnsWidth();
                StateHasChanged();
            }

            if (mUpdateDataCacheOnNextRender)
            {
                mUpdateDataCacheOnNextRender = false;
                await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();
            }
        }
        */

        [JSInvokable]
        public void JsInvokeKeyDown(string pKey)
        {
            if (pKey == "Escape")
            {
                InvokeAsync(() => _ = StopEditing(true, true));
            }
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

            var sortInstructions = GroupByInstructions.Concat(SortInstructions).ToArray();

            if (sortInstructions.Length > 0)
            {
                data = mSorter.SortBy(data, sortInstructions);
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

        //does this really work for complrex nested structures?
        private void GetNestedExpandoValue(string[] pProperties, object pValue, ref Dictionary<string, object> pParent)
        {
            var property = pProperties[0];

            if (pProperties.Length > 1)
            {
                var dict = new Dictionary<string, object>();

                if (!pParent.ContainsKey(property))
                    pParent.Add(property, dict);

                GetNestedExpandoValue(pProperties.Skip(1).ToArray(), pValue, ref dict);
                return;
            }

            if (!pParent.ContainsKey(property))
                pParent.Add(property, pValue);
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
                            var firstProp = properties.First();

                            var dict = new Dictionary<string, object>();

                            if (fmodel.ContainsKey(firstProp))
                            {
                                dict = (Dictionary<string, object>)fmodel[firstProp];
                            }

                            GetNestedExpandoValue(properties.Skip(1).ToArray(), value, ref dict);

                            if (!fmodel.ContainsKey(firstProp))
                            {
                                fmodel.Add(firstProp, dict);
                            }
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
 

        private void AddEditRow<M>(RenderTreeBuilder pBuilder, MGridAction pAction, bool pIsFilterRow, M pValue)
        {
            AddInlineTrHeight(pBuilder);

            if (UseStaticLayoutMode)
            {
                pBuilder.OpenElement(577, "td");
                pBuilder.AddAttribute(578, "colspan", VisibleColumns.Length);

                pBuilder.OpenElement(580, "table");
                pBuilder.OpenElement(581, "tbody");
                pBuilder.OpenElement(582, "tr");
            }

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

            if (UseStaticLayoutMode)
            {
                pBuilder.CloseElement(); //tr
                pBuilder.CloseElement(); //tbody
                pBuilder.CloseElement(); //table

                pBuilder.CloseElement(); //td  
            }
        }

        private void AddInlineTrHeight(RenderTreeBuilder pBuilder)
        {
            if (UseStaticLayoutMode && mFieldBoundingBox != null)
            {
                var inlineTrHeight = mFieldBoundingBox.Height;

                if (mFieldBoundingBox.BorderCollapse == "collapse")
                {
                    inlineTrHeight += mFieldBoundingBox.BorderTop / 2 - mTableBorderTop / 2;
                }

                pBuilder.AddAttribute(643, "style", $"height: {(inlineTrHeight).ToString(CultureInfo.InvariantCulture)}px");
            }
        }

        private void AddInlineFormFields(RenderTreeBuilder pBuilder, IMGridColumn[] pVisibleColumns, bool pIsFilterRow)
        {
            double left = 0;

            for (int i = 0; i < pVisibleColumns.Length; i++)
            {
                IMGridColumn column = pVisibleColumns[i];

                BoundingBox box = null;

                double columnWidth = 0;

                if (mFieldBoundingBox != null)
                {
                    columnWidth = GetColumnWidth(i);

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

                    box = new BoundingBox()
                    {
                        BorderTop = 0,
                        BorderRight = mFieldBoundingBox.BorderRight,
                        BorderSpace = mFieldBoundingBox.BorderSpace,
                        BorderCollapse = mFieldBoundingBox.BorderCollapse,

                        Width = bWidth,
                        Height = bHeight,
                    };
                }

                AddMFormField(pBuilder, column, pIsFilterRow, left, box);

                if (mFieldBoundingBox != null)
                {
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
        }

        private void AddMFormField(RenderTreeBuilder pBuilder, IMGridColumn pColumn, bool pIsInFilterRow, double pLeftOffset, BoundingBox pBoundingBox)
        {
            if (pColumn is IMGridPropertyColumn pc)
            {
                var propertyType = PropertyInfos[pc].PropertyType;

                if (pIsInFilterRow)
                    propertyType = GetNullableTypeIfNeeded(propertyType);

                var method = typeof(MGrid2<T>).GetMethod(nameof(MGrid2<T>.AddPropertyField), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(propertyType);
                method.Invoke(this, new object[] { pBuilder, pColumn, pc, pIsInFilterRow, pLeftOffset, pBoundingBox });
            }
            else if (pColumn is IMGridEditFieldGenerator<T> fieldGenerator)
            {
                AddFieldGenerator(pBuilder, pColumn, fieldGenerator, pIsInFilterRow, pLeftOffset, pBoundingBox);
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

                if (UseStaticLayoutMode)
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

                if (UseStaticLayoutMode)
                    pBuilder.AddStyleWithAttribute(976, Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE, pLeftOffset, pBoundingBox);

                pBuilder.CloseComponent();
            }
        }

        private void AddFieldGenerator(RenderTreeBuilder pBuilder, IMGridColumn pColumn, IMGridEditFieldGenerator<T> pFieldGenerator, bool pIsInFilterRow, double pLeftOffset, BoundingBox pBoundingBox)
        {
            pBuilder.OpenComponent<MComplexPropertyField<int>>(5);
            pBuilder.AddAttribute(1138, nameof(MField.Property), (string)null);
            pBuilder.AddAttribute(1139, nameof(MField.PropertyType), typeof(int));

            pBuilder.AddAttribute(1141, nameof(IMGridColumn), pColumn);

            var fieldGenerator = pFieldGenerator;

            RenderFragment<MComplexPropertyFieldContext<int>> template = context =>
            {
                return b =>
                {
                    b.AddContent(1147, fieldGenerator.EditFieldTemplate(pIsInFilterRow));
                };
            };

            pBuilder.AddAttribute(1151, nameof(MComplexPropertyField<int>.Template), template);

            if (UseStaticLayoutMode)
                pBuilder.AddStyleWithAttribute(1152, Extensions.MFORM_IN_TABLE_ROW_TD_STYLE_ATTRIBUTE, pLeftOffset, pBoundingBox);

            pBuilder.CloseComponent();
        }

        internal async void OnRowClick(T pValue, MouseEventArgs pMouseArgs)
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
            SaveCurrentState();
            StateHasChanged();
        }

        private async Task UpdateColumnsWidth()
        {
            var json = await JsRuntime.InvokeAsync<string>("mcomponents.getColumnSizes", new object[] { mTableReference, ColumnsList.Select(c => c.Identifier).ToArray() });

            if (string.IsNullOrWhiteSpace(json))
                return;

            var values = JsonSerializer.Deserialize<string[]>(json);

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

            mTableContainerWidth = values[7].FromPixelToDouble();
            mColumnsWidth = values.Skip(8).Select(v => v.FromPixelToDouble()).ToArray();

            InvokeStateHasChanged();
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

        protected void OnToggleGrouping()
        {
            IsGroupingVisible = !IsGroupingVisible;
            InvokeStateHasChanged();
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

            var useConfirmAlert = MComponentSettings.UseDeleteConfirmationWithAlert;

            if (Events?.OnBeginDelete != null)
            {
                var args = new BeginDeleteArgs<T>()
                {
                    Row = value,
                    MouseEventArgs = pMouseEventArgs,
                    UseDeleteConfirmationWithAlert = useConfirmAlert
                };

                await Events.OnBeginDelete.InvokeAsync(args);

                if (args.Cancelled)
                    return;

                useConfirmAlert = args.UseDeleteConfirmationWithAlert;
            }

            if (useConfirmAlert)
            {
                Formatter.ClearRowMetadata();
                Formatter.AddRowMetadata(value, MGridDefaultObjectFormatter<T>.ROW_DELETE_METADATA);

                InvokeStateHasChanged();
                await Task.Delay(150);

                bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", L[nameof(MComponentsLocalization.AreYouSure)].ToString());

                Formatter.ClearRowMetadata();
                InvokeStateHasChanged();

                if (!confirmed)
                    return;
            }

            if (DataSource is ICollection<T> coll)
                coll.Remove(value);

            if (DataAdapter != null)
            {
                try
                {
                    await DataAdapter.Remove(value);
                }
                catch
                {
                    Notificator.InvokeNotification(ServiceProvider, true, L[nameof(MComponentsLocalization.DeleteFailed)]);
                    await ResetRowsAndCache();
                    return;
                }
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
            await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();

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
                            if (t.Exception != null)
                            {
                                Notificator.InvokeNotification(ServiceProvider, true, L[nameof(MComponentsLocalization.UpdateFailed)]);
                                await ResetRowsAndCache();
                                return;
                            }

                            if (Events?.OnAfterEdit != null)
                            {
                                await Events.OnAfterEdit.InvokeAsync(new AfterEditArgs<T>()
                                {
                                    Row = value
                                });
                            }

                            await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();
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
                            if (t.Exception != null)
                            {
                                Notificator.InvokeNotification(ServiceProvider, true, L[nameof(MComponentsLocalization.CreateFailed)]);
                                await ResetRowsAndCache();
                                return;
                            }

                            if (Events?.OnAfterAdd != null)
                            {
                                await Events.OnAfterAdd.InvokeAsync(new AfterAddArgs<T>()
                                {
                                    Row = NewValue
                                });
                            }

                            await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();
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
            await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();

            SaveCurrentState();
            StateHasChanged();
        }

        protected async Task OnPageSizeChange(ChangeEventArgs pValue)
        {
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
            Pager.PageSize = int.Parse(pValue.Value.ToString());
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.

            await StopEditing(false, false);
            await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();

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

            if (EnableGrouping && pArgs.CtrlKey && pArgs.ShiftKey)
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
                    HiddenGroupByKeys.RemoveAll(r => r.Item1.ContainsKey(propInfo.Name));
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
                await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();

                SaveCurrentState();
                StateHasChanged();
            }
        }

        internal async void OnEditValueChanged(MFormValueChangedArgs<T> pArgs)
        {
            if (Events?.OnFormValueChanged != null)
            {
                await Events.OnFormValueChanged.InvokeAsync(pArgs);
            }
        }

        internal async Task OnFilterValueChanged(MFormValueChangedArgs<ExpandoObject> pArgs)
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
            await ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();

            UpdateColumnsWidthOnNextRender = true;

            SaveCurrentState();
            StateHasChanged();
        }

        public async Task ExportContent(string pFileName = "Export.xlsx")
        {
            IEnumerable<T> dataForExport = DataSource;

            if (dataForExport == null && DataAdapter != null)
            {
                dataForExport = await DataAdapter.GetData(Enumerable.Empty<T>().AsQueryable());
            }

            if (MComponentSettings.EnsureAssemblyIsLoaded != null)
            {
                await MComponentSettings.EnsureAssemblyIsLoaded(ServiceProvider, "DocumentFormat.OpenXml.dll");
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
                Notificator.InvokeNotification(ServiceProvider, true, ex.Message);
            }
        }

        public async Task ImportContent(Stream pStream)
        {
            //   await ExcelImportHelper.ImportFile<T>(this, pStream); //todo
        }

        internal T CreateNewT()
        {
            if (ModelFactory != null)
            {
                return ModelFactory();
            }

            return (T)Activator.CreateInstance(typeof(T));
        }

        internal void ClearDataCache()
        {
            DataCache = null;
            DataCountCache = -1;
            TotalDataCountCache = -1;
            GroupedDataCache = null;
        }

        protected async Task UpdateDataCacheIfDataAdapter(bool pForce = false)
        {
            bool semaphoreAcquired = false;

            try
            {
                if ((pForce || DataCache == null) && DataAdapter != null)
                {
                    await mDataAdapterSemaphore.WaitAsync();
                    semaphoreAcquired = true;

                    mIsLoading = true;
                    InvokeStateHasChanged();

                    var queryable = GetIQueryable(Enumerable.Empty<T>(), false);

                    var totalDataCount = await DataAdapter.GetTotalDataCount();
                    var dataCount = await DataAdapter.GetDataCount(queryable);
                    var dataCache = (await DataAdapter.GetData(queryable)).ToArray();

                    TotalDataCountCache = totalDataCount;
                    DataCountCache = dataCount;
                    DataCache = dataCache;

                    if (pForce)
                    {
                        GroupedDataCache = null; //if force act like ClearDataCache()
                    }

                    mIsLoading = false;

                    InvokeStateHasChanged();
                }
            }
            finally
            {
                if (semaphoreAcquired)
                    mDataAdapterSemaphore.Release();
            }
        }

        protected async Task ClearDataCacheIfDataSourceOrUpdateIfDataAdapter()
        {
            if (DataSource != null)
            {
                ClearDataCache();
                StateHasChanged();
                return;
            }

            await UpdateDataCacheIfDataAdapter(true);
        }

        public void Refresh()
        {
            _ = ClearDataCacheIfDataSourceOrUpdateIfDataAdapter();
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
            if (mColumnsWidth == null)
                return 200;

            if (pIndex < mColumnsWidth.Length)
            {
                return mColumnsWidth[pIndex];
            }

            throw new InvalidOperationException("Column width is unknown");
        }

        public void ClearFilterValues()
        {
            bool refresh = false;

            if (FilterInstructions.Count > 0)
            {
                refresh = true;
                FilterInstructions.Clear();
            }

            mFilterModel = null;

            SaveCurrentState();

            if (refresh)
                Refresh();
        }

        public void SaveCurrentState()
        {
            //todo
            //    if (EnableSaveState)
            //        StateService.SaveGridState(this);
        }

        public IMPropertyInfo GetIdentifierProperty(object pValue) //supports identifier of table property or other properties
        {
            if (pValue == null)
                return null;

            if (pValue.GetType() == typeof(T) && mIdentifierProperty != null)
                return mIdentifierProperty;

            var prop = ReflectionHelper.GetProperties(pValue)?.FirstOrDefault(p => p.Name.ToLowerInvariant() == "id");

            if (pValue.GetType() == typeof(T))
                mIdentifierProperty = prop;

            return prop;
        }

        public string GetIdentifierValue(object pValue)
        {
            if (pValue == null)
                return null;

            return GetIdentifierProperty(pValue)?.GetValue(pValue)?.ToString();
        }

        public void SelectRow(string pIdentifier)
        {
            mSelectedRowId = pIdentifier;
            InvokeStateHasChanged();
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
