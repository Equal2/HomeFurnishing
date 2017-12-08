using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Presentation.HtmlHelpers
{
    public static class GridHelper
    {
        public static Grid jqGrid(this HtmlHelper helper, string id)
        {
            return new Grid(id);
        }
    }

    #region Enumerations
    public enum Align
    {
        center,
        left,
        right
    }

    public enum CellSubmit
    {
        clientArray,
        remote
    }

    public enum DataType
    {
        json,
        xml,
        jsonp,
        clientSide,
        array,
        local
    }

    public enum Direction
    {
        vertical,
        horizontal
    }

    public enum EditType
    {
        text,
        textarea,
        checkbox,
        password,
        button,
        image,
        file,
        select,
        custom
    }

    public enum Formatters
    {
        integer,
        number,
        currency,
        date,
        email,
        link,
        showlink,
        checkbox,
        select
    }

    public enum LoadUi
    {
        enable,
        disable,
        block
    }

    public enum MultiKey
    {
        altKey,
        ctrlKey,
        shiftKey
    }

    public enum PagerPos
    {
        center,
        left,
        right
    }

    public enum PathType
    {
        relative,
        absolute
    }

    public enum RecordPos
    {
        center,
        left,
        right
    }

    public enum RequestType
    {
        get,
        post
    }

    public enum SearchType
    {
        text,
        select,
        datepicker
    }

    public enum SortOrder
    {
        asc,
        desc
    }

    public enum SortType
    {
        INT,
        FLOAT,
        DATE,
        TEXT
    }

    public enum ToolbarPosition
    {
        top,
        bottom,
        both
    }

    public enum TreeGridModel
    {
        nested,
        adjacency
    }
    #endregion Eum

    /// <summary>
    /// Grid class, used to render JqGrid
    /// </summary>
    public class Grid
    {
        #region Properties
        private string _parentId;
        private string _id;
        private string _altClass;
        private bool? _altRows;
        private bool? _autoEncode;
        private bool? _autoWidth;
        private string _caption;
        private bool? _cellEdit;
        private CellSubmit? _cellSubmit;
        private string _cellUrl;
        private bool? _cloneToTop;
        private List<Column> _columns = new List<Column>();
        private DataType _dataType = DataType.json;
        private string _editUrl;
        private string _emptyRecords;
        private bool? _footerRow;
        private bool? _forceFit;
        private bool? _gridView;
        private bool? _grouping;
        private string _groupingView;
        private bool? _headerTitles;
        private int? _height;
        private bool? _hiddenGrid;
        private bool? _hideGrid;
        private bool? _hoverRows;
        private string _imgPath;
        private bool? _loadOnce;
        private string _loadText;
        private LoadUi? _loadUi;
        private MultiKey? _multiKey;
        private bool? _multiBoxOnly;
        private bool? _multiSelect;
        private int? _multiSelectWidth;
        private int? _page;
        private string _pager;
        private PagerPos? _pagerPos;
        private PathType? _pathtype = PathType.relative;
        private bool? _pgButtons;
        private bool? _pgInput;
        private string _pgText;
        private RecordPos? _recordPos;
        private string _recordText;
        private RequestType? _requestType;
        private string _resizeClass;
        private int[] _rowList;
        private int? _rowNum;
        private bool? _rowNumbers;
        private int? _rowNumWidth;
        private bool? _scroll;
        private int? _scrollInt;
        private int? _scrollOffset;
        private bool? _scrollRows;
        private int? _scrollTimeout;
        private bool? _searchClearButton;
        private bool? _searchOnEnter;
        private bool? _searchToggleButton;
        private bool? _searchToolbar;
        private bool? _shrinkToFit;
        private bool? _showAllSortIcons;
        private Direction? _sortIconDirection;
        private SortOrder? _sortOrder;
        private string _sortName;
        private bool? _sortOnHeaderClick;
        private ToolbarPosition _toolbarPosition = ToolbarPosition.top;
        private bool? _treeGrid;
        private int? _tree_root_level;
        private TreeGridModel? _treeGridModel;
        private string _treeGridExpandColumn;
        private bool? _treeGridExpandColClick;
        private bool? _topPager;
        private bool? _toolbar;
        private string _url;
        private bool? _viewRecords;
        private int? _width;

        private string _onAfterInsertRow;
        private string _onBeforeRequest;
        private string _onBeforeSelectRow;
        private string _onGridComplete;
        private string _onLoadBeforeSend;
        private string _onLoadComplete;
        private string _onSubGridLoadComplete;
        private string _onLoadError_title = string.Empty;
        private string _onloadError_message = string.Empty;
        private string _onCellSelect;
        private string _onDblClickRow;
        private string _onHeaderClick;
        private string _onPaging;
        private string _onRightClickRow;
        private string _onSelectAll;
        private string _onSelectRow;
        private string _onSortCol;
        private string _onResizeStart;
        private string _onResizeStop;
        private string _onSerializeGridData;
        private string _onInitializeForm;

        private Grid _subGrid;
        private string _subGridOptions = "{plusicon:'ui-icon-plus', minusicon:'ui-icon-minus'}";
        private string _readerOptions;
        private bool _closeAfterEdit = true;
        private Grid _customGrid;
        private Grid _customGridJson;
        private bool? _hideHeader;
        private bool? _navEdit;
        private bool? _navAdd;
        private bool? _navDel;
        private bool _showError = false;
        private bool _excelExport = false;
        private string _excelExportUrl;
        private bool? _print;
        private string _customSubGridJsonBeforeExpandDataSuccessDelegate;
        private string _customSubGridJsonAfterExpandDataSuccessDelegate;
        private bool? _customSubGridJsonCacheExpandedData;
        private string _customSubGridJsonAltClass;
        private string _customSubGridJsonMouseOverClass;
        private string _customSubGridJsonSelectedRowClass;
        private string _customSubGridJsonChildHighlightClass;
        private string _customSubGridJsonChildExcludeHighlightClass;
        private string _rowStyle;
        private string _loadingImageSrc;
        private Grid _parentGridRef;
        private string _onDocumentReadyJavaScript;

        public Grid parentGridRef
        {
            get { return this._parentGridRef; }
            set { this._parentGridRef = value; }
        }
        private string _data;
        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id of grid</param>
        public Grid(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Id must contain a value to identify the grid");
            }
            this._id = id;
        }

        /// <summary>
        /// Adds columns to grid
        /// </summary>
        /// <param name="column">Colomn object</param>
        public Grid addColumn(Column column)
        {
            this._columns.Add(column);
            return this;
        }
        #endregion Constructor

        #region Set Methods
        public Grid setDocumentReadyJavaScript(string onDocReadyJscript)
        {
            this._onDocumentReadyJavaScript = onDocReadyJscript;
            return this;
        }

        /// <summary>
        ///  <para>Array that store the local data passed to the grid. You can directly point to 
        ///  <para>this variable in case you want to load a array data.</para>
        ///  <para>It can replace addRowData method which is slow on relative big data</para>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Grid setData(string data)
        {
            this._data = data;
            return this;
        }

        /// <summary>
        /// <para>The class that is used for alternate rows. You can construct your own class and replace this value.</para>
        /// <para>This option is valid only if altRows options is set to true (default: ui-priority-secondary)</para>
        /// </summary>
        /// <param name="altClass">Classname for alternate rows</param>
        public Grid setAltClass(string altClass)
        {
            this._altClass = altClass;
            return this;
        }

        /// <summary>
        /// Set a zebra-striped grid (default: false)
        /// </summary>
        /// <param name="altRows">Boolean indicating if zebra-striped grid is used</param>
        public Grid setAltRows(Boolean altRows)
        {
            this._altRows = altRows;
            return this;
        }

        /// <summary>
        /// <para>When set to true encodes (html encode) the incoming (from server) and posted</para> 
        /// <para>data (from editing modules). For example < will be converted to &lt (default: false)</para>
        /// </summary>
        /// <param name="autoEncode">Boolean indicating if autoencode is used</param>
        public Grid setAutoEncode(bool autoEncode)
        {
            this._autoEncode = autoEncode;
            return this;
        }

        /// <summary>
        /// <para>When set to true, the grid width is recalculated automatically to the width of the</para> 
        /// <para>parent element. This is done only initially when the grid is created. In order to</para> 
        /// <para>resize the grid when the parent element changes width you should apply custom code</para> 
        /// <para>and use a setGridWidth method for this purpose. (default: false)</para>
        /// </summary>
        /// <param name="autoWidth">Boolean indicating if autowidth is used</param>
        public Grid setAutoWidth(bool autoWidth)
        {
            this._autoWidth = autoWidth;
            return this;
        }

        /// <summary>
        /// <para>Defines the caption layer for the grid. This caption appears above the header layer.</para> 
        /// <para>If the string is empty the caption does not appear. (default: empty)</para>
        /// </summary>
        /// <param name="caption">Caption of grid</param>
        public Grid setCaption(string caption)
        {
            this._caption = caption;
            return this;
        }

        public Grid setCellEdit(bool cellEdit)
        {
            this._cellEdit = cellEdit;
            return this;
        }

        public Grid setCellSubmit(CellSubmit cellSubmit)
        {
            this._cellSubmit = cellSubmit;
            return this;
        }

        public Grid setCellUrl(string cellUrl)
        {
            this._cellUrl = cellUrl;
            return this;
        }

        /// <summary>
        /// <para>Clones all the actions from the bottom pager to the top pager if defined.</para> 
        /// <para>Note that the navGrid can be applied to the top pager only.</para> 
        /// <para>The id of the top pager is a combination of grid id and “_toppager”</para>
        /// </summary>
        /// <param name="cloneToTop"></param>
        /// <returns></returns>
        public Grid setCloneToTop(bool cloneToTop)
        {
            this._cloneToTop = cloneToTop;
            return this;
        }

        public Grid setCustomGrid(Grid customGrid)
        {
            this._customGrid = customGrid;
            this._customGrid._parentId = this._id;
            return this;
        }

        public Grid setCustomGridJson(Grid customGrid)
        {
            this._customGridJson = customGrid;
            this._customGridJson._parentId = this._id;
            customGrid.parentGridRef = this;
            return this;
        }

        /// <summary>
        /// <para>Defines what type of information to expect to represent data in the grid. Valid</para> 
        /// <para>options are json (default) and xml</para>
        /// </summary>
        /// <param name="dataType">Data type</param>
        public Grid setDataType(DataType dataType)
        {
            this._dataType = dataType;
            return this;
        }

        /// <summary>
        /// Defines the url for inline and form editing
        /// </summary>
        /// <param name="editUrl"></param>
        /// <returns></returns>
        public Grid setEditUrl(string editUrl)
        {
            this._editUrl = editUrl;
            return this;
        }

        /// <summary>
        /// <para>Displayed when the returned (or the current) number of records is zero.</para> 
        /// <para>This option is valid only if viewrecords option is set to true. (default value is</para> 
        /// <para>set in language file)</para>
        /// </summary>
        /// <param name="emptyRecords">Display string</param>
        public Grid setEmptyRecords(string emptyRecords)
        {
            this._emptyRecords = emptyRecords;
            return this;
        }

        /// <summary>
        /// <para>If set to true this will place a footer table with one row below the grid records</para> 
        /// <para>and above the pager. The number of columns equal to the number of columns in the colModel</para> 
        /// <para>(default: false)</para>
        /// </summary>
        /// <param name="footerRow">Boolean indicating whether footerrow is displayed</param>
        public Grid setFooterRow(bool footerRow)
        {
            this._footerRow = footerRow;
            return this;
        }

        /// <summary>
        /// <para>If set to true, when resizing the width of a column, the adjacent column (to the right)</para> 
        /// <para>will resize so that the overall grid width is maintained (e.g., reducing the width of</para> 
        /// <para>column 2 by 30px will increase the size of column 3 by 30px).</para> 
        /// <para>In this case there is no horizontal scrolbar.</para> 
        /// <para>Note: this option is not compatible with shrinkToFit option - i.e if</para> 
        /// <para>shrinkToFit is set to false, forceFit is ignored.</para>
        /// </summary>
        /// <param name="forceFit">Boolean indicating if forcefit is enforced</param>
        public Grid setForceFit(bool forceFit)
        {
            this._forceFit = forceFit;
            return this;
        }

        /// <summary>
        /// <para>In the previous versions of jqGrid including 3.4.X,reading relatively big data sets</para> 
        /// <para>(Rows >=100 ) caused speed problems. The reason for this was that as every cell was</para> 
        /// <para>inserted into the grid we applied about 5-6 jQuery calls to it. Now this problem has</para> 
        /// <para>been resolved; we now insert the entry row at once with a jQuery append. The result is</para> 
        /// <para>impressive - about 3-5 times faster. What will be the result if we insert all the</para>
        /// <para>data at once? Yes, this can be done with help of the gridview option. When set to true,</para> 
        /// <para>the result is a grid that is 5 to 10 times faster. Of course when this option is set</para> 
        /// <para>to true we have some limitations. If set to true we can not use treeGrid, subGrid,</para> 
        /// <para>or afterInsertRow event. If you do not use these three options in the grid you can</para> 
        /// <para>set this option to true and enjoy the speed. (default: false)</para>
        /// </summary>
        /// <param name="gridView">Boolean indicating gridview is enabled</param>
        public Grid setGridView(bool gridView)
        {
            this._gridView = gridView;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grouping"></param>
        /// <returns></returns>
        public Grid setGrouping(bool grouping)
        {
            this._grouping = grouping;
            return this;
        }

        /// <summary>
        /// <para>groupField : ['name'],</para>
        /// <para>groupSummary : [true],</para>
        /// <para>groupColumnShow : [true],</para>
        /// <para>roupText : ['<b>{0}</b>'],</para>
        /// <para>groupCollapse : false,</para>
        /// <para> groupOrder: ['asc']</para>
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public Grid setGroupingView(string view)
        {
            this._groupingView = view;
            return this;
        }

        /// <summary>
        /// If the option is set to true the title attribute is added to the column headers (default: false)
        /// </summary>
        /// <param name="headerTitles">Boolean indicating if headertitles are enabled</param>

        public Grid setHeaderTitles(bool headerTitles)
        {
            this._headerTitles = headerTitles;
            return this;
        }

        /// <summary>
        /// The height of the grid in pixels (default: 100%, which is the only acceptable percentage for jqGrid)
        /// </summary>
        /// <param name="height">Height in pixels</param>
        public Grid setHeight(int height)
        {
            this._height = height;
            return this;
        }

        /// <summary>
        /// <para>If set to true the grid initially is hidden. The data is not loaded (no request is sent) and only the</para> 
        /// <para>caption layer is shown. When the show/hide button is clicked for the first time to show the grid, the request</para> 
        /// <para>is sent to the server, the data is loaded, and the grid is shown. From this point on we have a regular grid.</para> 
        /// <para>This option has effect only if the caption property is not empty. (default: false)</para>
        /// </summary>
        /// <param name="hiddenGrid">Boolean indicating if hiddengrid is enforced</param>
        public Grid setHiddenGrid(bool hiddenGrid)
        {
            this._hiddenGrid = hiddenGrid;
            return this;
        }

        /// <summary>
        /// <para>Enables or disables the show/hide grid button,</para>
        /// <para>which appears on the right side of the caption layer</para>. 
        /// <para>Takes effect only if the caption property is not an empty string. (default: true)</para> 
        /// </summary>
        /// <param name="hideGrid">Boolean indicating if show/hide button is enabled</param>
        public Grid setHideGrid(bool hideGrid)
        {
            this._hideGrid = hideGrid;
            return this;
        }

        /// <summary>
        /// setHideHeader(true) hide column header row which appears on top of grid
        /// </summary>
        /// <param name="hideHeader"></param>
        /// <returns></returns>
        public Grid setHideHeader(bool hideHeader)
        {
            this._hideHeader = hideHeader;
            return this;
        }

        /// <summary>
        /// When set to false the mouse hovering is disabled in the grid data rows. (default: true)
        /// </summary>
        /// <param name="hoverRows">Indicates whether hoverrows is enabled</param>
        public Grid setHoverRows(bool hoverRows)
        {
            this._hoverRows = hoverRows;
            return this;
        }

        /// <summary>
        /// <para>Defines the path to the images that are used in the grid.</para>
        /// <para>Set this option without / at end</para>
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        public Grid setImgPath(string imgPath)
        {
            this._imgPath = imgPath;
            return this;
        }

        /// <summary>
        /// <para>If this flag is set to true, the grid loads the data from the server only once (using the</para> 
        /// <para>appropriate datatype). After the first request the datatype parameter is automatically</para> 
        /// <para>changed to local and all further manipulations are done on the client side. The functions</para> 
        /// <para>of the pager (if present) are disabled. (default: false)</para>
        /// </summary>
        /// <param name="loadOnce">Boolean indicating if loadonce is enforced</param>
        public Grid setLoadOnce(bool loadOnce)
        {
            this._loadOnce = loadOnce;
            return this;
        }

        /// <summary>
        /// <para>The text which appears when requesting and sorting data.</para> 
        /// <para>This parameter override the value located</para> 
        /// <para>in the language file</para>
        /// </summary>
        /// <param name="loadText">Loadtext</param>
        public Grid setLoadText(string loadText)
        {
            this._loadText = loadText;
            return this;
        }

        /// <summary>
        /// <para>This option controls what to do when an ajax operation is in progress.</para>
        /// <para>'disable' - disables the jqGrid progress indicator.</para>
        /// <para>This way you can use your own indicator.</para>
        /// <para>'enable' (default) - enables “Loading” message in the center of the grid.</para> 
        /// <para>'block' - enables the “Loading” message and blocks all actions in</para>
        /// <para>the grid until the ajax request</para> 
        /// <para>is finished. Note that this disables paging,</para>
        /// <para>sorting and all actions on toolbar, if any.</para>
        /// </summary>
        /// <param name="loadUi">Load ui mode</param>
        public Grid setLoadUi(LoadUi loadUi)
        {
            this._loadUi = loadUi;
            return this;
        }

        /// <summary>
        /// <para>This option works only when multiselect = true.</para>
        /// <para>When multiselect is set to true, clicking anywhere</para> 
        /// <para>on a row selects that row; when multiboxonly is also set to true,</para>
        /// <para>the multiselection is done only</para> 
        /// <para>when the checkbox is clicked (Yahoo style). Clicking in any other row</para>
        /// <para>(suppose the checkbox is not clicked) deselects all rows and</para> 
        /// <para>the current row is selected. (default: false)</para>
        /// </summary>
        /// <param name="multiBoxOnly">Boolean indicating if multiboxonly is enforced</param>
        public Grid setMultiBoxOnly(bool multiBoxOnly)
        {
            this._multiBoxOnly = multiBoxOnly;
            return this;
        }

        /// <summary>
        /// <para>This parameter makes sense only when multiselect option is set to true.</para> 
        /// <para>Defines the key which will be pressed</para> 
        /// <para>when we make a multiselection. The possible values are:</para> 
        /// <para>'shiftKey' - the user should press Shift Key</para> 
        /// <para>'altKey' - the user should press Alt Key</para>
        /// <para>'ctrlKey' - the user should press Ctrl Key</para>
        /// </summary>
        /// <param name="multiKey">Key to multiselect</param>
        public Grid setMultiKey(MultiKey multiKey)
        {
            this._multiKey = multiKey;
            return this;
        }

        /// <summary>
        /// <para>If this flag is set to true a multi selection of rows is enabled.</para>
        /// <para>A new column at the left side is added. Can be used with any datatype option.</para>
        /// <para>(default: false)</para>
        /// </summary>
        /// <param name="multiSelect">Boolean indicating if multiselect is enabled</param>
        public Grid setMultiSelect(bool multiSelect)
        {
            this._multiSelect = multiSelect;
            return this;
        }

        /// <summary>
        /// Determines the width of the multiselect column if multiselect is set to true. (default: 20)
        /// </summary>
        /// <param name="multiSelectWidth"></param>
        public Grid setMultiSelectWidth(int multiSelectWidth)
        {
            this._multiSelectWidth = multiSelectWidth;
            return this;
        }

        /// <summary>
        /// show add button on pager bar
        /// </summary>
        /// <param name="add"></param>
        /// <returns></returns>
        public Grid setNavAdd(bool add)
        {
            this._navAdd = add;
            return this;
        }

        /// <summary>
        /// show delete button on pager bar
        /// </summary>
        /// <param name="del"></param>
        /// <returns></returns>
        public Grid setNavDel(bool del)
        {
            this._navDel = del;
            return this;
        }

        /// <summary>
        /// show edit button on pager bar
        /// </summary>
        /// <param name="edit"></param>
        /// <returns></returns>
        public Grid setNavEdit(bool edit)
        {
            this._navEdit = edit;
            return this;
        }

        /// <summary>
        /// <para>Set the initial number of selected page when we make the request.</para>
        /// <para>This parameter is passed to the url</para> 
        /// <para>for use by the server routine retrieving the data (default: 1)</para>
        /// </summary>
        /// <param name="page">Number of page</param>
        public Grid setPage(int page)
        {
            this._page = page;
            return this;
        }

        /// <summary>
        /// If pagername is specified a pagerelement is automatically added to the grid 
        /// </summary>
        /// <param name="pager">Id/name of pager</param>
        public Grid setPager(string pager)
        {
            this._pager = pager;
            return this;
        }

        /// <summary>
        /// <para>Determines the position of the pager in the grid. By default the pager element</para>
        /// <para>when created is divided in 3 parts (one part for pager, one part for navigator</para> 
        /// <para>buttons and one part for record information) (default: center)</para>
        /// </summary>
        /// <param name="pagerPos">Position of pager</param>
        public Grid setPagerPos(PagerPos pagerPos)
        {
            this._pagerPos = pagerPos;
            return this;
        }

        public Grid setPathType(PathType pathType)
        {
            this._pathtype = pathType;
            return this;
        }

        /// <summary>
        /// <para>Determines if the pager buttons should be displayed if pager is available. Valid</para> 
        /// <para>only if pager is set correctly. The buttons are placed in the pager bar. (default: true)</para>
        /// </summary>
        /// <param name="pgButtons">Boolean indicating if pager buttons are displayed</param>
        public Grid setPgButtons(bool pgButtons)
        {
            this._pgButtons = pgButtons;
            return this;
        }

        /// <summary>
        /// <para>Determines if the input box, where the user can change the number of the</para>
        /// <para>requested page, should be available. The input box appears in the pager bar. (default: true)</para>
        /// </summary>
        /// <param name="pgInput">Boolean indicating if pager input is available</param>
        public Grid setPgInput(bool pgInput)
        {
            this._pgInput = pgInput;
            return this;
        }

        /// <summary>
        /// <para>Show information about current page status. The first value is the current loaded page.</para> 
        /// <para>The second value is the total number of pages (default is set in language file)</para>
        /// <para>Example: "Page {0} of {1}"</para>
        /// </summary>
        /// <param name="pgText">Current page status text</param>
        public Grid setPgText(string pgText)
        {
            this._pgText = pgText;
            return this;
        }

        /// <summary>
        /// <para>used to set the reader options jsonReader or XMLReader</para>
        /// <para>datatype: "json"</para>
        /// <para>readerOpions = "repeatitems: false, id: '0'"</para>
        /// <para>jsonReader: { repeatitems: false, id: '0' }</para>
        /// </summary>
        /// <param name="readerOprtions"></param>
        /// <returns></returns>
        public Grid setReaderOptions(string readerOpions)
        {
            this._readerOptions = readerOpions;
            return this;
        }

        /// <summary>
        /// <para>Determines the position of the record information in the pager. Can be left,</para>
        /// <para>center, right (default: right)</para>
        /// <para>Warning: When pagerpos en recordpos are equally set, pager is hidden. </para>       
        /// </summary>
        /// <param name="recordPos">Position of record information</param>
        public Grid setRecordPos(RecordPos recordPos)
        {
            this._recordPos = recordPos;
            return this;
        }

        /// <summary>
        /// <para>Represent information that can be shown in the pager. This option is valid if viewrecords</para> 
        /// <para>option is set to true. This text appears only if the total number of records is greater then zero.</para>
        /// <para>In order to show or hide information the items between {} mean the following: {0} the</para> 
        /// <para>start position of the records depending on page number and number of requested records;</para> 
        /// <para>{1} - the end position {2} - total records returned from the data (default defined in language file)</para>
        /// </summary>
        /// <param name="recordText">Record Text</param>
        public Grid setRecordText(string recordText)
        {
            this._recordText = recordText;
            return this;
        }

        /// <summary>
        /// Defines the type of request to make (“POST” or “GET”) (default: GET)
        /// </summary>
        /// <param name="requestType">Request type</param>
        public Grid setRequestType(RequestType requestType)
        {
            this._requestType = requestType;
            return this;
        }

        /// <summary>
        /// Assigns a class to columns that are resizable so that we can show a resize 
        /// handle (default: empty string)
        /// </summary>
        /// <param name="resizeClass"></param>
        /// <returns></returns>
        public Grid setResizeClass(string resizeClass)
        {
            this._resizeClass = resizeClass;
            return this;
        }

        /// <summary>
        /// <para>An array to construct a select box element in the pager in which we can change the number</para> 
        /// <para>of the visible rows. When changed during the execution, this parameter replaces the rowNum</para> 
        /// <para>parameter that is passed to the url. If the array is empty the element does not appear</para> 
        /// <para>in the pager. Typical you can set this like [10,20,30]. If the rowNum parameter is set to</para> 
        /// <para>30 then the selected value in the select box is 30.</para>
        /// </summary>
        /// <example>
        /// setRowList(new int[]{10,20,50})
        /// </example>
        /// <param name="rowList">List of rows per page</param>
        public Grid setRowList(int[] rowList)
        {
            this._rowList = rowList;
            return this;
        }

        /// <summary>
        /// <para>Sets how many records we want to view in the grid. This parameter is passed to the url</para> 
        /// <para>for use by the server routine retrieving the data. Note that if you set this parameter</para> 
        /// <para>to 10 (i.e. retrieve 10 records) and your server return 15 then only 10 records will be</para> 
        /// <para>loaded. Set this parameter to -1 (unlimited) to disable this checking. (default: 20)</para>
        /// </summary>
        /// <param name="rowNum">Number of rows per page</param>
        public Grid setRowNum(int rowNum)
        {
            this._rowNum = rowNum;
            return this;
        }

        /// <summary>
        /// <para>If this option is set to true, a new column at the leftside of the grid is added. The purpose of</para> 
        /// <para>this column is to count the number of available rows, beginning from 1. In this case</para> 
        /// <para>colModel is extended automatically with a new element with name - 'rn'. Also, be careful</para> 
        /// <para>not to use the name 'rn' in colModel</para>
        /// </summary>
        /// <param name="rowNumbers">Boolean indicating if rownumbers are enabled</param>
        public Grid setRowNumbers(bool rowNumbers)
        {
            this._rowNumbers = rowNumbers;
            return this;
        }

        /// <summary>
        /// Determines the width of the row number column if rownumbers option is set to true. (default: 25)
        /// </summary>
        /// <param name="rowNumWidth">Width of rownumbers column</param>
        public Grid setRowNumWidth(int rowNumWidth)
        {
            this._rowNumWidth = rowNumWidth;
            return this;
        }

        /// <summary>
        /// <para>Creates dynamic scrolling grids. When enabled, the pager elements are disabled and we can use the</para> 
        /// <para>vertical scrollbar to load data. When set to true the grid will always hold all the items from the</para> 
        /// <para>start through to the latest point ever visited.</para> 
        /// <para>When scroll is set to an integer value (eg 1), the grid will just hold the visible lines.</para>
        /// <para>This allow us to load the data at portions whitout to care about the memory leaks. (default: false)</para>
        /// </summary>
        /// <param name="scroll">Boolean indicating if scroll is enforced</param>
        public Grid setScroll(bool scroll)
        {
            this._scroll = scroll;
            if (this._scrollInt.HasValue)
            {
                throw new InvalidOperationException("You can't set scroll to both a boolean and an integer at the same time, please choose one.");
            }
            return this;
        }

        /// <summary>
        /// <para>Creates dynamic scrolling grids. When enabled, the pager elements are disabled and we can use the</para> 
        /// <para>vertical scrollbar to load data. When set to true the grid will always hold all the items from the</para> 
        /// <para>start through to the latest point ever visited.</para> 
        /// <para>When scroll is set to an integer value (eg 1), the grid will just hold the visible lines.</para>
        /// <para>This allow us to load the data at portions whitout to care about the memory leaks. (default: false)</para>
        /// </summary>
        /// <param name="scroll">When integer value is set (eg 1) scroll is enforced</param>
        public Grid setScroll(int scroll)
        {
            this._scrollInt = scroll;
            if (this._scroll.HasValue)
            {
                throw new InvalidOperationException("You can't set scroll to both a boolean and an integer at the same time, please choose one.");
            }
            return this;
        }

        /// <summary>
        /// <para>Determines the width of the vertical scrollbar. Since different browsers interpret this width</para> 
        /// <para>differently (and it is difficult to calculate it in all browsers) this can be changed. (default: 18)</para>
        /// </summary>
        /// <param name="scrollOffset">Scroll offset</param>
        public Grid setScrollOffset(int scrollOffset)
        {
            this._scrollOffset = scrollOffset;
            return this;
        }

        /// <summary>
        /// <para>When enabled, selecting a row with setSelection scrolls the grid so that the selected row is visible.</para> 
        /// <para>This is especially useful when we have a verticall scrolling grid and we use form editing with</para> 
        /// <para>navigation buttons (next or previous row). On navigating to a hidden row, the grid scrolls so the</para> 
        /// <para>selected row becomes visible. (default: false)</para>
        /// </summary>
        /// <param name="scrollRows">Boolean indicating if scrollrows is enabled</param>
        public Grid setScrollRows(bool scrollRows)
        {
            this._scrollRows = scrollRows;
            return this;
        }

        /// <summary>
        /// This controls the timeout handler when scroll is set to 1. (default: 200 milliseconds)
        /// </summary>
        /// <param name="scrollTimeout">Scroll timeout in milliseconds</param>
        /// <returns></returns>
        public Grid setScrollTimeout(int scrollTimeout)
        {
            this._scrollTimeout = scrollTimeout;
            return this;
        }

        /// <summary>
        /// When set to true adds clear button to clear all search entries (default: false)
        /// </summary>
        /// <param name="searchClearButton"></param>
        /// <returns></returns>
        public Grid setSearchClearButton(bool searchClearButton)
        {
            this._searchClearButton = searchClearButton;
            return this;
        }

        /// <summary>
        /// <para>Determines how the search should be applied. If this option is set to true search is started when</para> 
        /// <para>the user hits the enter key. If the option is false then the search is performed immediately after</para> 
        /// <para>the user presses some character. (default: true</para>
        /// </summary>
        /// <param name="searchOnEnter">Indicates if search is started on enter</param>
        public Grid setSearchOnEnter(bool searchOnEnter)
        {
            this._searchOnEnter = searchOnEnter;
            return this;
        }

        /// <summary>
        /// Enables toolbar searching / filtering
        /// </summary>
        /// <param name="searchToolbar">Indicates if toolbar searching is enabled</param>
        public Grid setSearchToolbar(bool searchToolbar)
        {
            this._searchToolbar = searchToolbar;
            return this;
        }

        /// <summary>
        /// When set to true adds toggle button to toggle search toolbar (default: false)
        /// </summary>
        /// <param name="searchToggleButton">Indicates if toggle button is displayed</param>
        public Grid setSearchToggleButton(bool searchToggleButton)
        {
            this._searchToggleButton = searchToggleButton;
            return this;
        }

        /// <summary>
        /// If enabled all sort icons are visible for all columns which are sortable (default: false)
        /// </summary>
        /// <param name="showAllSortIcons">Boolean indicating if all sorting icons should be displayed</param>
        public Grid setShowAllSortIcons(bool showAllSortIcons)
        {
            this._showAllSortIcons = showAllSortIcons;
            return this;
        }

        /// <summary>
        /// <para>This option describes the type of calculation of the initial width of each column</para> 
        /// <para>against the width of the grid. If the value is true and the value in width option</para> 
        /// <para>is set then: Every column width is scaled according to the defined option width.</para> 
        /// <para>Example: if we define two columns with a width of 80 and 120 pixels, but want the</para> 
        /// <para>grid to have a 300 pixels - then the columns are recalculated as follow:</para> 
        /// <para>1- column = 300(new width)/200(sum of all width)*80(column width) = 120 and 2 column = 300/200*120 = 180.</para> 
        /// <para>The grid width is 300px. If the value is false and the value in width option is set then: </para>
        /// <para>The width of the grid is the width set in option.</para> 
        /// <para>The column width are not recalculated and have the values defined in colModel. (default: true)</para>
        /// </summary>
        /// <param name="shrinkToFit">Boolean indicating if shrink to fit is enforced</param>
        public Grid setShrinkToFit(bool shrinkToFit)
        {
            this._shrinkToFit = shrinkToFit;
            return this;
        }

        /// <summary>
        /// Sets direction in which sort icons are displayed (default: vertical)
        /// </summary>
        /// <param name="sortIconDirection">Direction in which sort icons are displayed</param>
        public Grid setSortIconDirection(Direction sortIconDirection)
        {
            this._sortIconDirection = sortIconDirection;
            return this;
        }

        /// <summary>
        /// <para>If enabled columns are sorted when header is clicked (default: true)</para>
        /// <para>Warning, if disabled and setShowAllSortIcons is set to false, sorting will</para>
        /// <para>be effectively disabled</para>
        /// </summary>
        /// <param name="sortOnHeaderClick">Boolean indicating if columns will sort on headerclick</param>
        /// <returns></returns>
        public Grid setSortOnHeaderClick(bool sortOnHeaderClick)
        {
            this._sortOnHeaderClick = sortOnHeaderClick;
            return this;
        }

        /// <summary>
        /// <para>The initial sorting name when we use datatypes xml or json (data returned from server).</para>
        /// <para>This parameter is added to the url. If set and the index (name) matches the name from the</para>
        /// <para>colModel then by default an image sorting icon is added to the column, according to</para> 
        /// <para>the parameter sortorder.</para>
        /// </summary>
        /// <param name="sortName"></param>
        public Grid setSortName(string sortName)
        {
            this._sortName = sortName;
            return this;
        }

        /// <summary>
        /// <para>The initial sorting order when we use datatypes xml or json (data returned from server).</para>
        /// <para>This parameter is added to the url. Two possible values - asc or desc. (default: asc)</para>
        /// </summary>
        /// <param name="sortOrder">Sortorder</param>
        public Grid setSortOrder(SortOrder sortOrder)
        {
            this._sortOrder = sortOrder;
            return this;
        }

        public Grid setSubGrid(Grid subGrid)
        {
            this._subGrid = subGrid;
            return this;
        }

        /// <summary>
        /// <para>This option enabled the toolbar of the grid.  When we have two toolbars (can be set using setToolbarPosition)</para>
        /// <para>then two elements (div) are automatically created. The id of the top bar is constructed like</para> 
        /// <para>“t_”+id of the grid and the bottom toolbar the id is “tb_”+id of the grid. In case when</para> 
        /// <para>only one toolbar is created we have the id as “t_” + id of the grid, independent of where</para> 
        /// <para>this toolbar is created (top or bottom). You can use jquery to add elements to the toolbars.</para>
        /// </summary>
        /// <param name="toolbar">Boolean indicating if toolbar is enabled</param>
        public Grid setToolbar(bool toolbar)
        {
            this._toolbar = toolbar;
            return this;
        }

        /// <summary>
        /// Sets toolbarposition (default: top)
        /// </summary>
        /// <param name="toolbarPosition">Position of toolbar</param>
        public Grid setToolbarPosition(ToolbarPosition toolbarPosition)
        {
            this._toolbarPosition = toolbarPosition;
            return this;
        }

        /// <summary>
        /// <para>When enabled this option place a pager element at top of the grid below the caption</para> 
        /// <para>(if available). If another pager is defined both can coexists and are refreshed in sync.</para> 
        /// <para>(default: false)</para>
        /// </summary>
        /// <param name="topPager">Boolean indicating if toppager is enabled</param>
        public Grid setTopPager(bool topPager)
        {
            this._topPager = topPager;
            return this;
        }

        /// <summary>
        /// Enables (disables) the tree grid format
        /// </summary>
        /// <param name="treeGrid"></param>
        /// <returns></returns>
        public Grid setTreeGrid(bool treeGrid)
        {
            this._treeGrid = treeGrid;
            return this;
        }

        /// <summary>
        /// Enable tree buttons
        /// </summary>
        /// <param name="colClick"></param>
        /// <returns></returns>
        public Grid setTreeGridExpandColClick(bool colClick)
        {
            this._treeGridExpandColClick = colClick;
            return this;
        }

        /// <summary>
        /// <para>Indicates which column (name from colModel) should be used to expand the tree grid.</para> 
        /// <para>If not set the first one is used.</para> 
        /// <para>Valid only when treeGrid option is set to true.</para>
        /// </summary>
        /// <param name="treeGridExpandColumn"></param>
        /// <returns></returns>
        public Grid setTreeGridExpandColumn(string treeGridExpandColumn)
        {
            this._treeGridExpandColumn = treeGridExpandColumn;
            return this;
        }

        /// <summary>
        /// Tree Grid Model can be either "adjacency" or "nested"
        /// </summary>
        /// <param name="treeGridModel"></param>
        /// <returns></returns>
        public Grid setTreeGridModel(TreeGridModel treeGridModel)
        {
            this._treeGridModel = treeGridModel;
            return this;
        }

        /// <summary>
        /// Determines the level where the root element begins when treeGrid is enabled
        /// Default is 0
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public Grid setTreeRootLevel(int root)
        {
            this._tree_root_level = root;
            return this;
        }

        /// <summary>
        /// <para>The url of the file that holds the request</para>
        /// <para>setUrl("~/Home/GetData")</para>
        /// </summary>
        /// <param name="url">Data url</param>
        public Grid setUrl(string url)
        {
            this._url = url;
            return this;
        }

        /// <summary>
        /// <para>If true, jqGrid displays the beginning and ending record number in the grid,</para>
        /// <para>out of the total number of records in the query.</para> 
        /// <para>This information is shown in the pager bar (bottom right by default)in this format:</para> 
        /// <para>“View X to Y out of Z”.</para> 
        /// <para>If this value is true, there are other parameters that can be adjusted,</para> 
        /// <para>including 'emptyrecords' and 'recordtext'. (default: false)</para>
        /// </summary>
        /// <param name="viewRecords">Boolean indicating if recordnumbers are shown in grid</param>
        public Grid setViewRecords(bool viewRecords)
        {
            this._viewRecords = viewRecords;
            return this;
        }

        /// <summary>
        /// <para>If this option is not set, the width of the grid is a sum of the widths of the columns</para> 
        /// <para>defined in the colModel (in pixels). If this option is set, the initial width of each </para>
        /// <para>column is set according to the value of shrinkToFit option.</para>
        /// </summary>
        /// <param name="width">Width in pixels</param>
        public Grid setWidth(int width)
        {
            this._width = width;
            return this;
        }

        /// <summary>
        /// <para>export grid and data to excel spreadsheet</para>
        /// <para>create a post back in the controller</para>
        /// <para>excample: [HttpPost] public void ExportExcel(){ ... }</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Grid setExcelExport(string url)
        {
            this._excelExport = true;
            this._excelExportUrl = url;
            return this;
        }

        /// <summary>
        /// print griad and data to printer
        /// </summary>
        /// <param name="print"></param>
        /// <returns></returns>
        public Grid setPrint(bool print)
        {
            this._print = print;
            return this;
        }
        /// <summary>
        /// <para>If set to "true", allows to preserve previously expanded and then collapsed data on the UI and therefore</para>
        /// <para>when expanded again, avoid making new back end call. In this case, previously collapsed subgrid data is</para>
        /// <para>made visible again. Any data changes, made on the UI to the subgrid rows before collapsing, remain after expanding again.</para>
        /// </summary>
        /// <param name="isCacheData"></param>
        /// <returns></returns>
        public Grid setCustomSubGridJsonCacheExpandedData(bool isCacheData)
        {
            this._customSubGridJsonCacheExpandedData = isCacheData;
            return this;
        }

        public Grid setCustomSubGridJsonAltClass(string className)
        {
            this._customSubGridJsonAltClass = className;
            return this;
        }
        public Grid setCustomSubGridJsonMouseOverClass(string className)
        {
            this._customSubGridJsonMouseOverClass = className;
            return this;
        }
        public Grid setCustomSubGridJsonSelectedRowClass(string className)
        {
            this._customSubGridJsonSelectedRowClass = className;
            return this;
        }
        public Grid setRowStyle(string style)
        {
            this._rowStyle = style;
            return this;
        }
        public Grid setCustomSubGridChildHighlightClass(string className, string excludeHighlightClassName = "")
        {
            this._customSubGridJsonChildHighlightClass = className;
            this._customSubGridJsonChildExcludeHighlightClass = excludeHighlightClassName;
            return this;
        }
        public Grid setLoadingImageSrc(string imagesrc)
        {
            this._loadingImageSrc = imagesrc;
            return this;
        }
        /// <summary>
        /// <para>A set of options for the subgrid. Below are all the options with their default values</para>
        /// <para>{plusicon : “ui-icon-plus”,minusicon : “ui-icon-minus”,openicon: “ui-icon-carat-1-sw”,expandOnLoad: false,selectOnExpand : false,reloadOnExpand : true }</para> 
        /// <para>plusicon and minusicon defies the icons when the grid is collapsed/expanded. A valid name of icon from Theme Roller should be set.</para> 
        /// <para>openicon the icon bellow the minusicon when the subgrid row is expanded </para>
        /// <para>expandOnLoad when set to true make it so that all rows will be expanded automatically when a new set of data is loaded.</para> 
        /// <para>selectOnLoad when set to true the row is selected when a plusicon is clicked with the mouse.</para> 
        /// <para>reloadOnExpand If set to false the data in the subgrid is loaded only once and all other subsequent </para>
        /// <para>clicks just hide or show the data and no more ajax calls are made.</para>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public Grid setSubGridOptions(string options)
        {
            this._subGridOptions = options;
            return this;
        }
        #endregion Set Methods

        #region Grid Events
        /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <para>This event fires after each inserted row.</para>
        /// <para>Variables available in call:</para>
        /// <para>'rowid': Id of the inserted row</para> 
        /// <para>'rowdata': An array of the data to be inserted into the row. This is array of type name-</para> 
        /// <para>value, where the name is a name from colModel</para> 
        /// <para>'rowelem': The element from the response. If the data is xml this is the xml element of the row;</para> 
        /// <para>if the data is json this is array containing all the data for the row</para> 
        /// <para>Note: this event does not fire if gridview option is set to true</para>
        /// </summary>
        /// <param name="onAfterInsertRow">Script to be executed</param>
        public Grid onAfterInsertRow(string onAfterInsertRow)
        {
            this._onAfterInsertRow = onAfterInsertRow;
            return this;
        }

        /// <summary>
        /// <para>This event fires before requesting any data. Does not fire if datatype is function</para>
        /// <para>Variables available in call: None</para>
        /// </summary>
        /// <param name="onBeforeRequest">Script to be executed</param>
        public Grid onBeforeRequest(string onBeforeRequest)
        {
            this._onBeforeRequest = onBeforeRequest;
            return this;
        }

        /// <summary>
        /// <para>This event fires when the user clicks on the row, but before selecting it.</para>
        /// <para>Variables available in call:</para>
        /// <para>'rowid': The id of the row.</para> 
        /// <para>'e': The event object</para> 
        /// <para>This event should return boolean true or false. If the event returns true the selection</para> 
        /// <para>is done. If the event returns false the row is not selected and any other action if defined</para> 
        /// <para>does not occur.</para>
        /// </summary>
        /// <param name="onBeforeSelectRow">Script to be executed</param>
        public Grid onBeforeSelectRow(string onBeforeSelectRow)
        {
            this._onBeforeSelectRow = onBeforeSelectRow;
            return this;
        }

        /// <summary>
        /// <para>This fires after all the data is loaded into the grid and all the other processes are complete.</para> 
        /// <para>Also the event fires independent from the datatype parameter and after sorting paging and etc.</para>
        /// <para>Variables available in call: None</para>
        /// </summary>
        /// <param name="onGridComplete">Script to be executed</param>
        public Grid onGridComplete(string onGridComplete)
        {
            this._onGridComplete = onGridComplete;
            return this;
        }

        /// <summary>
        /// <para>A pre-callback to modify the XMLHttpRequest object (xhr) before it is sent. Use this to set</para> 
        /// <para>custom headers etc. The XMLHttpRequest is passed as the only argument.</para>
        /// <para>Variables available in call:</para>
        /// <para>'xhr': The XMLHttpRequest</para>
        /// </summary>
        /// <param name="onLoadBeforeSend">Script to be executed</param>
        public Grid onLoadBeforeSend(string onLoadBeforeSend)
        {
            this._onLoadBeforeSend = onLoadBeforeSend;
            return this;
        }

        /// <summary>
        /// <para>This event is executed immediately after every server request. </para>
        /// <para>Variables available in call:</para>
        /// <para>'xhr': The XMLHttpRequest</para>
        /// </summary>
        /// <param name="onLoadComplete">Script to be executed</param>
        public Grid onLoadComplete(string onLoadComplete)
        {
            this._onLoadComplete = onLoadComplete;
            return this;
        }

        /// <summary>
        /// <para>This function (onSubGridLoadComplete) is executed immediately after sub grid is loaded on client side.</para> 
        /// <para>Variables available in call:</para>
        /// <para>subgrid_id: subgrid id</para>
        /// <para>row_id: row id</para>
        /// </summary>
        /// <param name="onLoadComplete">Script to be executed</param>
        public Grid onSubGridLoadComplete(string onSubGridLoadComplete)
        {
            this._onSubGridLoadComplete = onSubGridLoadComplete;
            return this;
        }


        /// <summary>
        /// <para>A function to be called if the request fails.</para>
        /// <para>Variables available in call:</para>
        /// <para>title: popup title</para>
        /// <para>message: message to display</para>
        /// </summary>
        /// <param name="onLoadError">Script to be executed</param>
        public Grid onLoadError(string title, string message = "")
        {
            this._showError = true;
            this._onLoadError_title = title;
            this._onloadError_message = message;
            return this;
        }

        /// <summary>
        /// <para>Fires when we click on a particular cell in the grid.</para>
        /// <para>Variables available in call:</para>
        /// <para>'rowid': The id of the row</para> 
        /// <para>'iCol': The index of the cell,</para>
        /// <para>'cellcontent': The content of the cell,</para>
        /// <para>'e': The event object element where we click.</para>
        /// <para>(Note that this available when we are not using cell editing module</para> 
        /// <para>and is disabled when using cell editing).</para>
        /// </summary>
        /// <param name="onCellSelect">Script to be executed</param>
        public Grid onCellSelect(string onCellSelect)
        {
            this._onCellSelect = onCellSelect;
            return this;
        }

        /// <summary>
        /// <para>Raised immediately after row was double clicked.</para> 
        /// <para>Variables available in call:</para>
        /// <para>'rowid': The id of the row,</para> 
        /// <para>'iRow': The index of the row (do not mix this with the rowid),</para>
        /// <para>'iCol': The index of the cell.</para> 
        /// <para>'e': The event object</para>
        /// </summary>
        /// <param name="onDblClickRow">Script to be executed</param>
        public Grid onDblClickRow(string onDblClickRow)
        {
            this._onDblClickRow = onDblClickRow;
            return this;
        }

        /// <summary>
        /// <para>Fires after clicking hide or show grid (hidegrid:true)</para>
        /// <para>Variables available in call:</para>
        /// <para>'gridstate': The state of the grid - can have two values - visible or hidden</para>
        /// </summary>
        /// <param name="onHeaderClick">Script to be executed</param>
        public Grid onHeaderClick(string onHeaderClick)
        {
            this._onHeaderClick = onHeaderClick;
            return this;
        }

        /// <summary>
        /// <para>This event fires after click on [page button] and before populating the data.</para>
        /// <para>Also works when the user enters a new page number in the page input box</para>
        /// <para>(and presses [Enter]) and when the number of requested records is changed via</para>
        /// <para>the select box.</para>
        /// <para>If this event returns 'stop' the processing is stopped and you can define your</para>
        /// <para>own custom paging</para>
        /// <para>Variables available in call:</para>
        /// <para>'pgButton': first,last,prev,next in case of button click, records in case when</para>
        /// <para>a number of requested rows is changed and user when the user change the number of the requested page</para>
        /// </summary>
        /// <param name="onPaging">Script to be executed</param>
        public Grid onPaging(string onPaging)
        {
            this._onPaging = onPaging;
            return this;
        }

        /// <summary>
        /// <para>Raised immediately after row was right clicked.</para>
        /// <para>Variables available in call:</para>
        /// <para>'rowid': The id of the row,</para>
        /// <para>'iRow': The index of the row (do not mix this with the rowid),</para>
        /// <para>'iCol': The index of the cell.</para>
        /// <para>'e': The event object</para>
        /// <para>Note - this event does not work in Opera browsers, since Opera does not support oncontextmenu event</para>
        /// </summary>
        /// <param name="onRightClickRow">Script to be executed</param>
        public Grid onRightClickRow(string onRightClickRow)
        {
            this._onRightClickRow = onRightClickRow;
            return this;
        }

        /// <summary>
        /// <para>This event fires when multiselect option is true and you click on the header checkbox.</para>
        /// <para>Variables available in call:</para>
        /// <para>'aRowids':  Array of the selected rows (rowid's).</para>
        /// <para>'status': Boolean variable determining the status of the header check box - true if checked, false if not checked.</para>
        /// <para>Note that the aRowids alway contain the ids when header checkbox is checked or unchecked.</para>
        /// </summary>
        /// <param name="onSelectAll">Script to be executed</param>
        public Grid onSelectAll(string onSelectAll)
        {
            this._onSelectAll = onSelectAll;
            return this;
        }


        /// <summary>
        /// <para>Raised immediately when row is clicked.</para>
        /// <para>Variables available in function call:</para>
        /// <para>'rowid': The id of the row,</para>
        /// <para>'status': Tthe status of the selection. Can be used when multiselect is set to true.</para> 
        /// <para>true if the row is selected, false if the row is deselected.</para>
        /// <param name="onSelectRow">Script to be executed</param>
        /// </summary>
        public Grid onSelectRow(string onSelectRow)
        {
            this._onSelectRow = onSelectRow;
            return this;
        }

        /// <summary>
        /// <para>Raised immediately after sortable column was clicked and before sorting the data.</para>
        /// <para>Variables available in call:</para>
        /// <para>'index': The index name from colModel</para>
        /// <para>'iCol': The index of column, </para>
        /// <para>'sortorder': The new sorting order - can be 'asc' or 'desc'. </para>
        /// <para>If this event returns 'stop' the sort processing is stopped and you can define your own custom sorting</para>
        /// </summary>
        /// <param name="onSortCol">Script to be executed</param>
        public Grid onSortCol(string onSortCol)
        {
            this._onSortCol = onSortCol;
            return this;
        }

        /// <summary>
        /// <para>Event which is called when we start resizing a column.</para>
        /// <para>Variables available in call:</para>
        /// <para>'event':  The event object</para>
        /// <para>'index': The index of the column in colModel.</para>
        /// </summary>
        /// <param name="onResizeStart">Script to be executed</param>
        public Grid onResizeStart(string onResizeStart)
        {
            this._onResizeStart = onResizeStart;
            return this;
        }


        /// <summary>
        /// <para>Event which is called after the column is resized.</para>
        /// <para>Variables available in call:</para>
        /// <para>'newwidth': The new width of the column</para>
        /// <para>'index': The index of the column in colModel.</para>
        /// </summary>
        /// <param name="onResizeStop">Script to be executed</param>
        public Grid onResizeStop(string onResizeStop)
        {
            this._onResizeStop = onResizeStop;
            return this;
        }

        /// <summary>
        /// <para>If this event is set it can serialize the data passed to the ajax request.</param>
        /// <para>The function should return the serialized data. This event can be used when</param>
        /// <para>custom data should be passed to the server - e.g - JSON string, XML string and etc.</param> 
        /// <para>Variables available in call:</param>
        /// <para>'postData': Posted data</param>
        /// </summary>
        /// <param name="onSerializeGridData">Script to be executed</param>
        public Grid onSerializeGridData(string onSerializeGridData)
        {
            this._onSerializeGridData = onSerializeGridData;
            return this;
        }

        /// <summary>
        /// <para>NAME of javascript method, which will be called upon successful server data load when expanding</param>
        /// <para>parent level row BEFORE (!) server data is used to render subgrid. Allows to stop subgrid rendering when necessary,</param> 
        /// <para>and override with custom desired render logic. This is achieved with overrideResponseRender argument, see below.</param>
        /// <para>Passed-in javascript method delegate must be declared with one input parameter, similar to this: "MySuperMethod(args)". Once this method</param>
        /// <para>is invoked, the single input parameter object is passed, containing the following fields:</param>
        /// <para>expandedRowData:  expanded row data, </param>
        /// <para>expandedRowId:  expanded row ID, </param>
        /// <para>subgridParentRef: subgrid parent element reference, helpful when reading or updating parent element contents, i.e. custom rendering, </param>
        /// <para>serverResponseData:  the data, received from server when expanding parent row,</param> 
        /// <para>subgridId:  Id of the subgrid, </param>
        /// <para>subgridTableId: Id of subgrid table element,</param>
        /// <para>overrideResponseRender: default is "false" - used to override default data rendering logic. If javascript </param>
        /// <para>delegate method sets this field to "true", than default data rendering will not take place. This is used to define custom</param>
        /// <para>response handling rather than built-in data rendering logic.</param>
        /// </summary>
        /// <param name="delegateReference">Name of the javascript method, which will be invoked when row already received server data but BEFORE
        /// that received data was rendered in the grid.</param>
        /// <returns></returns>
        public Grid onCustomSubGridJsonBeforeExpandDataSuccessDelegate(string delegateReference)
        {
            this._customSubGridJsonBeforeExpandDataSuccessDelegate = delegateReference;
            return this;
        }

        /// <summary>
        /// <para>NAME of javascript method, which will be called upon successful server data load when expanding</param>
        /// <para>parent level row AFTER (!) server data is used to render subgrid. Allows to fine-tune default rendering logic, when desired.</param>
        /// <para>Passed-in javascript method delegate must be declared with one input parameter, similar to this: "MySuperMethod(args)". Once this method</param>
        /// <para>is invoked, the single input parameter object is passed, containing the following fields:</param>
        /// <para>expandedRowData:  expanded row data, </param>
        /// <para>expandedRowId:  expanded row ID, </param>
        /// <para>subgridParentRef: subgrid parent element reference, helpful when reading or updating parent element contents, i.e. custom rendering, </param>
        /// <para>serverResponseData:  the data, received from server when expanding parent row, </param>
        /// <para>subgridId:  Id of the subgrid, </param>
        /// <para>subgridTableId: Id of subgrid table element</param>
        /// </summary>
        /// <param name="delegateReference">Name of the javascript method, which will be invoked when row already expanded and AFTER data rendered.</param>
        /// <returns></returns>
        public Grid onCustomSubGridJsonAfterExpandDataSuccessDelegate(string delegateReference)
        {
            this._customSubGridJsonAfterExpandDataSuccessDelegate = delegateReference;
            return this;
        }

        public Grid onInitializeForm(string onInitializeForm)
        {
            this._onInitializeForm = onInitializeForm;
            return this;
        }

        public Grid closeAfterEdit(bool close)
        {
            this._closeAfterEdit = close;
            return this;
        }
        /////////////////////////////////////////////////////////////////////////////
        #endregion Grid Events

        #region ToString Methods
        /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates and returns javascript + required html elements to render grid
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool IsEditable = (from c in _columns where c.IsEditable == true select c).Count() > 0 ? true : false;

            // Create javascript
            StringBuilder script = new StringBuilder();

            // Start script
            script.AppendLine("<script type=\"text/javascript\">");

            if (IsEditable) script.AppendFormat("var lastsel{0};", this._id).AppendLine();
            if (this._treeGrid.HasValue) script.AppendLine("$.jgrid.useJSON = true;");

            script.AppendLine("jQuery(document).ready(function () {");
            script.AppendLine("jQuery('#" + this._id + "').jqGrid({");

            // Altrows
            if (this._altRows.HasValue) script.AppendFormat("altRows: {0},", this._altRows.Value.ToString().ToLower()).AppendLine();

            // Altclass
            if (!string.IsNullOrWhiteSpace(this._altClass)) script.AppendFormat("altclass: '{0}',", this._altClass).AppendLine();

            // Autoencode
            if (this._autoEncode.HasValue) script.AppendFormat("autoencode: {0},", this._autoEncode.Value.ToString().ToLower()).AppendLine();

            // Autowidth
            if (this._autoWidth.HasValue) script.AppendFormat("autowidth: {0},", this._autoWidth.Value.ToString().ToLower()).AppendLine();

            // Caption
            if (!string.IsNullOrWhiteSpace(this._caption)) script.AppendFormat("caption: '{0}',", this._caption).AppendLine();

            // CellEdit
            if (this._cellEdit.HasValue) script.AppendFormat("cellEdit: {0},", this._cellEdit.Value.ToString().ToLower()).AppendLine();

            // CellSubmit
            if (this._cellSubmit.HasValue) script.AppendFormat("cellsubmit: '{0}',", this._cellSubmit.Value.ToString()).AppendLine();

            // CellUrl
            if (!string.IsNullOrWhiteSpace(this._cellUrl)) script.AppendFormat("editurl: '{0}',", this._cellUrl).AppendLine();

            // Datatype
            script.AppendLine(string.Format("datatype:'{0}',", this._dataType.ToString()));
            if ((this._dataType == DataType.local) && (!string.IsNullOrWhiteSpace(this._data)))
            {
                script.AppendFormat("data: {0},", this._data);
            }

            // Emptyrecords
            if (!string.IsNullOrWhiteSpace(this._emptyRecords)) script.AppendFormat("emptyrecords: '{0}',", this._emptyRecords).AppendLine();

            // FooterRow
            if (this._footerRow.HasValue) script.AppendFormat("footerrow: {0},", this._footerRow.Value.ToString().ToLower()).AppendLine();

            // Forcefit
            if (this._forceFit.HasValue) script.AppendFormat("forceFit: {0},", this._forceFit.Value.ToString().ToLower()).AppendLine();

            // Gridview
            if (this._gridView.HasValue) script.AppendFormat("gridview: {0},", this._gridView.Value.ToString().ToLower()).AppendLine();

            // HeaderTitles
            if (this._headerTitles.HasValue) script.AppendFormat("headertitles: {0},", this._headerTitles.Value.ToString().ToLower()).AppendLine();

            // Height (set 100% if no value is specified except when scroll is set to true otherwise layout is not as it is supposed to be)
            if (!this._height.HasValue)
            {
                if ((!this._scroll.HasValue || this._scroll.Value == false) && !this._scrollInt.HasValue) script.AppendFormat("height: '{0}',", "100%").AppendLine();
            }
            else script.AppendFormat("height: {0},", this._height).AppendLine();

            // Hiddengrid
            if (this._hiddenGrid.HasValue) script.AppendFormat("hiddengrid: {0},", this._hiddenGrid.Value.ToString().ToLower()).AppendLine();

            // Hidegrid
            if (this._hideGrid.HasValue) script.AppendFormat("hidegrid: {0},", this._hideGrid.Value.ToString().ToLower()).AppendLine();

            // HoverRows
            if (this._hoverRows.HasValue) script.AppendFormat("hoverrows: {0},", this._hoverRows.Value.ToString().ToLower()).AppendLine();

            // Loadonce
            if (this._loadOnce.HasValue) script.AppendFormat("loadonce: {0},", this._loadOnce.Value.ToString().ToLower()).AppendLine();

            // Loadtext
            if (this._loadText != null) script.AppendFormat("loadtext: '{0}',", this._loadText).AppendLine();

            // LoadUi
            if (this._loadUi.HasValue) script.AppendFormat("loadui: '{0}',", this._loadUi.Value.ToString()).AppendLine();

            // MultiBoxOnly
            if (this._multiBoxOnly.HasValue) script.AppendFormat("multiboxonly: {0},", this._multiBoxOnly.Value.ToString().ToLower()).AppendLine();

            // MultiKey
            if (this._multiKey.HasValue) script.AppendFormat("multikey: '{0}',", this._multiKey.Value.ToString()).AppendLine();

            // MultiSelect
            if (this._multiSelect.HasValue) script.AppendFormat("multiselect: {0},", this._multiSelect.Value.ToString().ToLower()).AppendLine();

            // MultiSelectWidth
            if (this._multiSelectWidth.HasValue) script.AppendFormat("multiselectWidth: {0},", this._multiSelectWidth.Value.ToString()).AppendLine();

            // Page
            if (this._page.HasValue) script.AppendFormat("page:{0},", this._page.Value).AppendLine();

            // Pager
            if (!string.IsNullOrWhiteSpace(this._pager)) script.AppendFormat("pager:'#{0}',", this._pager).AppendLine();

            // PagerPos
            if (this._pagerPos.HasValue) script.AppendFormat("pagerpos: '{0}',", this._pagerPos.ToString()).AppendLine();

            // PgButtons
            if (this._pgButtons.HasValue) script.AppendFormat("pgbuttons:{0},", this._pgButtons.Value.ToString().ToLower()).AppendLine();

            // PgInput
            if (this._pgInput.HasValue) script.AppendFormat("pginput: {0},", this._pgInput.Value.ToString().ToLower()).AppendLine();

            // PGText
            if (!string.IsNullOrWhiteSpace(this._pgText)) script.AppendFormat("pgtext: '{0}',", this._pgText).AppendLine();

            // RecordPos
            if (this._recordPos.HasValue) script.AppendFormat("recordpos: '{0}',", this._recordPos.Value).AppendLine();

            // RecordText
            if (!string.IsNullOrWhiteSpace(this._recordText)) script.AppendFormat("recordtext: '{0}',", this._recordText).AppendLine();

            // Request Type
            if (this._requestType.HasValue) script.AppendFormat("mtype: '{0}',", this._requestType.ToString()).AppendLine();

            // ResizeClass
            if (!string.IsNullOrWhiteSpace(this._resizeClass)) script.AppendFormat("resizeclass: '{0}',", this._resizeClass).AppendLine();

            // Rowlist
            if (this._rowList != null) script.AppendFormat("rowList: [{0}],", string.Join(",", ((from p in this._rowList select p.ToString()).ToArray()))).AppendLine();

            // Rownum
            if (this._rowNum.HasValue) script.AppendFormat("rowNum:{0},", this._rowNum.Value).AppendLine();

            // Rownumbers
            if (this._rowNumbers.HasValue) script.AppendFormat("rownumbers: {0},", this._rowNumbers.Value.ToString().ToLower()).AppendLine();

            // RowNumWidth
            if (this._rowNumWidth.HasValue) script.AppendFormat("rownumWidth: {0},", this._rowNumWidth.Value.ToString()).AppendLine();

            // Scroll (setters make sure either scroll or scrollint is set, never both)
            if (this._scroll.HasValue) script.AppendFormat("scroll:{0},", this._scroll.ToString().ToLower()).AppendLine();
            if (this._scrollInt.HasValue) script.AppendFormat("scroll:{0},", this._scrollInt.Value).AppendLine();

            // ScrollOffset
            if (this._scrollOffset.HasValue) script.AppendFormat("scrollOffset: {0},", this._scrollOffset.Value).AppendLine();

            // ScrollRows
            if (this._scrollRows.HasValue) script.AppendFormat("scrollrows: {0},", this._scrollRows.ToString().ToLower()).AppendLine();

            // ScrollTimeout
            if (this._scrollTimeout.HasValue) script.AppendFormat("scrollTimeout: {0},", this._scrollTimeout.Value).AppendLine();

            // Sortname
            if (!string.IsNullOrWhiteSpace(this._sortName)) script.AppendFormat("sortname: '{0}',", this._sortName).AppendLine();

            // Sorticons
            if (this._showAllSortIcons.HasValue || this._sortIconDirection.HasValue || this._sortOnHeaderClick.HasValue)
            {
                // Set defaults
                if (!this._showAllSortIcons.HasValue) this._showAllSortIcons = false;
                if (!this._sortIconDirection.HasValue) this._sortIconDirection = Direction.vertical;
                if (!this._sortOnHeaderClick.HasValue) this._sortOnHeaderClick = true;

                script.AppendFormat("viewsortcols: [{0},'{1}',{2}],", this._showAllSortIcons.Value.ToString().ToLower(), this._sortIconDirection.ToString(), this._sortOnHeaderClick.Value.ToString().ToLower()).AppendLine();
            }

            // Shrink to fit
            if (this._shrinkToFit.HasValue) script.AppendFormat("shrinkToFit: {0},", this._shrinkToFit.Value.ToString().ToLower()).AppendLine();

            // Sortorder
            if (this._sortOrder.HasValue) script.AppendFormat("sortorder: '{0}',", this._sortOrder.Value.ToString()).AppendLine();

            // Toolbar
            if (_toolbar.HasValue) script.AppendFormat("toolbar: [{0},\"{1}\"],", this._toolbar.Value.ToString().ToLower(), this._toolbarPosition.ToString()).AppendLine();

            // Toppager
            if (this._topPager.HasValue) script.AppendFormat("toppager: {0},", this._topPager.Value.ToString().ToLower()).AppendLine();

            // CloneToTop
            if (this._cloneToTop.HasValue) script.AppendFormat("cloneToTop: {0},", this._cloneToTop.Value.ToString().ToLower()).AppendLine();

            // Url
            if (!string.IsNullOrWhiteSpace(this._url)) script.AppendFormat("url: '{0}',", ((this._pathtype.HasValue && this._pathtype.Value.Equals(PathType.relative) && this._url.IndexOf("http") < 0) ? VirtualPathUtility.ToAbsolute(this._url) : this._url)).AppendLine();

            // edit url
            if (!string.IsNullOrWhiteSpace(this._editUrl)) script.AppendFormat("editurl: '{0}',", VirtualPathUtility.ToAbsolute(this._editUrl)).AppendLine();

            // image path
            if (!string.IsNullOrWhiteSpace(this._imgPath)) script.AppendFormat("imgpath: '{0}',", VirtualPathUtility.ToAbsolute(this._imgPath)).AppendLine();

            // View records
            if (this._viewRecords.HasValue) script.AppendFormat("viewrecords: {0},", this._viewRecords.Value.ToString().ToLower()).AppendLine();

            // Width
            if (this._width.HasValue) script.AppendFormat("width: '{0}',", this._width.Value).AppendLine();

            //Reader
            if (this._dataType == DataType.json || this._dataType == DataType.jsonp || this._dataType == DataType.local)
                if (!string.IsNullOrWhiteSpace(this._readerOptions)) script.AppendFormat("jsonReader : {{{0}}},", this._readerOptions.ToString()).AppendLine();
                else if (this._dataType == DataType.xml)
                    if (!string.IsNullOrWhiteSpace(this._readerOptions)) script.AppendFormat("xmlReader : {{{0}}},", this._readerOptions.ToString()).AppendLine();

            // onAfterInsertRow
            if (!string.IsNullOrWhiteSpace(this._onAfterInsertRow)) script.AppendFormat("afterInsertRow: function(rowid, rowdata, rowelem) {{{0}}},", this._onAfterInsertRow).AppendLine();

            // onBeforeRequest
            if (!string.IsNullOrWhiteSpace(this._onBeforeRequest)) script.AppendFormat("beforeRequest: function() {{{0}}},", this._onBeforeRequest).AppendLine();

            // onBeforeSelectRow
            if (!string.IsNullOrWhiteSpace(this._onBeforeSelectRow)) script.AppendFormat("beforeSelectRow: function(rowid, e) {{{0}}},", this._onBeforeSelectRow).AppendLine();

            string rowStyle = string.Empty;
            if (!string.IsNullOrEmpty(this._rowStyle))
            {
                System.Text.StringBuilder sbStyle = new StringBuilder();
                sbStyle.Append("var grid = $('#");
                sbStyle.Append(this._id);
                sbStyle.Append(@"');var cells = grid.find('TD[aria-describedby]');
                     if (cells.length == 0)
                        return;
                    var newCells = $(cells).filter(function (index, value) { return $(value).html().length>0; });
                     if (newCells.length > 0) {
                         var styleList = '");
                sbStyle.Append(this._rowStyle);
                sbStyle.Append(@"'.split(';');                        
                        for (var i = 0; i < newCells.length; i++) {
                            for (var a = 0; a < styleList.length; a++) {
                                var oneStyle = styleList[a].split(':');
                                if (oneStyle.length < 2)
                                    continue;
                                 $(newCells[i]).css(oneStyle[0], oneStyle[1]);
                            }
                        }
                    }");
                rowStyle = sbStyle.ToString();
            }
            // onGridComplete
            if (!string.IsNullOrWhiteSpace(this._onGridComplete)) script.AppendFormat("gridComplete: function() {{{0} {1}}},", rowStyle, this._onGridComplete).AppendLine();
            else if (!string.IsNullOrEmpty(rowStyle))
                script.AppendFormat("gridComplete: function() {{{0}}},", rowStyle).AppendLine();

            // onLoadBeforeSend
            if (!string.IsNullOrWhiteSpace(this._onLoadBeforeSend)) script.AppendFormat("loadBeforeSend: function(xhr) {{{0}}},", this._onLoadBeforeSend).AppendLine();

            // onLoadComplete
            if (!string.IsNullOrWhiteSpace(this._onLoadComplete)) script.AppendFormat("loadComplete: function(xhr) {{{0}}},", this._onLoadComplete).AppendLine();


            // onLoadError            
            if (_showError)
            {
                script.AppendFormat(@"loadError: function(xhr, status, error) {{
                    if ($('#divErrPopup{0}').html(error))
                    {{
                        $('#divPupFrm').dialog('option', 'title', '{1}');
                        $('#divPupFrm').append('{2}');
                    }}
                    else
                    {{
                        $('#divErrPopup{0}').empty();
                        $('#divErrPopup{0}').append('<p>');
                        $('#divErrPopup{0}').append('{2}');
                        $('#divErrPopup{0}').append(error);
                        $('#divErrPopup{0}').append('</p>');
                        $('#divErrPopup{0}').dialog({{
                            modal: true,
                            title: '{1}'
                        }});
                    }}
                }},", this._id, this._onLoadError_title, this._onloadError_message).AppendLine();
            }

            // onCellSelect
            if (!string.IsNullOrWhiteSpace(this._onCellSelect)) script.AppendFormat("onCellSelect: function(rowid, iCol, cellcontent, e) {{{0}}},", this._onCellSelect).AppendLine();

            // onDblClickRow
            if (!string.IsNullOrWhiteSpace(this._onDblClickRow)) script.AppendFormat("ondblClickRow: function(rowid, iRow, iCol, e) {{{0}}},", this._onDblClickRow).AppendLine();

            // onHeaderClick
            if (!string.IsNullOrWhiteSpace(this._onHeaderClick)) script.AppendFormat("onHeaderClick: function(gridstate) {{{0}}},", this._onHeaderClick).AppendLine();

            // onPaging
            if (!string.IsNullOrWhiteSpace(this._onPaging)) script.AppendFormat("onPaging: function(pgButton) {{{0}}},", this._onPaging).AppendLine();

            // onRightClickRow
            if (!string.IsNullOrWhiteSpace(this._onRightClickRow)) script.AppendFormat("onRightClickRow: function(rowid, iRow, iCol, e) {{{0}}},", this._onRightClickRow).AppendLine();

            // onSelectAll
            if (!string.IsNullOrWhiteSpace(this._onSelectAll)) script.AppendFormat("onSelectAll: function(aRowids, status) {{{0}}},", this._onSelectAll).AppendLine();

            // onSelectRow event
            if (!string.IsNullOrWhiteSpace(this._onSelectRow)) script.AppendFormat("onSelectRow: function(rowid, status) {{{0}}},", this._onSelectRow).AppendLine();

            // onSortCol
            if (!string.IsNullOrWhiteSpace(this._onSortCol)) script.AppendFormat("onSortCol: function(index, iCol, sortorder) {{{0}}},", this._onSortCol).AppendLine();

            // onResizeStart
            if (!string.IsNullOrWhiteSpace(this._onResizeStart)) script.AppendFormat("resizeStart: function(event, index) {{{0}}},", this._onResizeStart).AppendLine();

            // onResizeStop
            if (!string.IsNullOrWhiteSpace(this._onResizeStop)) script.AppendFormat("resizeStop: function(newwidth, index) {{{0}}},", this._onResizeStop).AppendLine();

            // onSerializeGridData
            if (!string.IsNullOrWhiteSpace(this._onSerializeGridData)) script.AppendFormat("serializeGridData: function(postData) {{{0}}},", this._onSerializeGridData).AppendLine();

            // Colmodel
            script.AppendLine("colModel: [");
            string colModel = string.Join(",\r\n", ((from c in this._columns select c.ToString()).ToArray()));
            script.AppendLine(colModel);
            script.AppendLine("],");

            if (IsEditable)
            {
                script.AppendFormat(@"
                onSelectRow: function(id)
                {{
                    if (id && id !== lastsel{0})
                    {{
                        jQuery('#{0}').restoreRow(lastsel{0});
                        jQuery('#{0}').editRow(id,true);
                        lastsel{0}=id;
                    }}
                }},", this._id).AppendLine();
            }

            // tree grid
            if (this._treeGrid.HasValue) script.AppendFormat("treeGrid: {0},", this._treeGrid.Value.ToString().ToLower()).AppendLine();
            if (this._tree_root_level.HasValue) script.AppendFormat("tree_root_level: {0},", this._tree_root_level.Value.ToString()).AppendLine();
            if (this._treeGridModel.HasValue) script.AppendFormat("treeGridModel: '{0}',", this._treeGridModel.ToString()).AppendLine();
            if (!string.IsNullOrWhiteSpace(this._treeGridExpandColumn)) script.AppendFormat("ExpandColumn: '{0}',", this._treeGridExpandColumn).AppendLine();
            if (this._treeGridExpandColClick.HasValue) script.AppendFormat("ExpandColClick: {0},", this._treeGridExpandColClick.ToString().ToLower()).AppendLine();

            // grouping
            if (this._grouping.HasValue) script.AppendFormat("grouping: {0},", this._grouping.Value.ToString().ToLower()).AppendLine();
            if (!string.IsNullOrWhiteSpace(this._groupingView)) script.AppendFormat("groupingView : {{{0}}},", this._groupingView).AppendLine();

            // hide header row
            if (this._hideHeader.HasValue && this._hideHeader == true)
                script.AppendLine("gridComplete: function(){$('#gview_" + this._id + " tr.ui-jqgrid-labels').hide();},");

            // sub grid
            if (this._subGrid != null) script.AppendLine(this._subGrid.ToSubGridString());
            if (this._customGrid != null) script.AppendLine(this._customGrid.ToCustomGridString());
            if (this._customGridJson != null) script.AppendLine(this._customGridJson.ToCustomGridStringJson());

            // Remove last comma from string
            if (script.ToString().LastIndexOf(",\r\n") > -1) script = script.Remove(script.Length - 3, 1);

            // End jqGrid call
            script.AppendLine("});");

            if (IsEditable && !string.IsNullOrWhiteSpace(this._pager))
            {
                StringBuilder nav = new StringBuilder();
                if (this._navEdit != null) nav.AppendFormat("edit: {0},", this._navEdit.ToString().ToLower()); else nav.Append("edit: true,");
                if (this._navAdd != null) nav.AppendFormat("add: {0},", this._navAdd.ToString().ToLower()); else nav.Append("add: false,");
                if (this._navDel != null) nav.AppendFormat("del: {0}", this._navDel.ToString().ToLower()); else nav.Append("del: false");

                // form settings
                string initForm = string.Empty;
                if (string.IsNullOrWhiteSpace(this._onInitializeForm) == false)
                {
                    initForm = string.Format(@"
                       ,{{
                            closeAfterEdit: {0},
                            closeAfterAdd: {0},
                            recreateForm: true,
                            onInitializeForm: function(formid) {{
                                {1}
                            }} 
                        }}", this._closeAfterEdit.ToString().ToLower(), this._onInitializeForm);
                }

                script.AppendFormat("jQuery('#{0}').jqGrid('navGrid',\"#{1}\",{{{2}}}{3});",
                    this._id,
                    this._pager,
                    nav.ToString(),
                    initForm
                    );

                // script.AppendLine("jQuery('#" + this._id + "').jqGrid('navGrid',\"#" + this._pager + "\",{" + nav.ToString() + "});");
            }

            // Search clear button
            if (this._searchToolbar == true && this._searchClearButton.HasValue && !string.IsNullOrWhiteSpace(this._pager) && this._searchClearButton.Value == true)
            {
                if (!script.ToString().Contains("navGrid"))
                    script.AppendLine("jQuery('#" + this._id + "').jqGrid('navGrid',\"#" + this._pager + "\",{edit:false,add:false,del:false,search:false,refresh:false}); ");
                script.AppendLine("jQuery('#" + this._id + "').jqGrid('navButtonAdd',\"#" + this._pager + "\",{caption:\"Clear\",title:\"Clear Search\",buttonicon :'ui-icon-refresh', onClickButton:function(){mygrid[0].clearToolbar(); }}); ");
            }

            // Search toolbar
            if (this._searchToolbar == true)
            {
                script.Append("jQuery('#" + this._id + "').jqGrid('filterToolbar', {stringResult:true");
                if (this._searchOnEnter.HasValue) script.AppendFormat(", searchOnEnter:{0}", this._searchOnEnter.Value.ToString().ToLower());
                script.AppendLine("});");
            }

            // excel export button
            if (this._excelExport == true)
            {
                if (!script.ToString().Contains("navGrid"))
                    script.AppendLine("jQuery('#" + this._id + "').jqGrid('navGrid',\"#" + this._pager + "\",{edit:false,add:false,del:false,search:false,refresh:false}); ");
                script.AppendLine("jQuery('#" + this._id + "').jqGrid(\"navButtonAdd\",\"#" + this._pager + "\"," +
                string.Format(@"
                {{
                    caption:'', 
                    buttonicon: 'ui-icon-extlink',
                    title: 'Excel Export',
                    onClickButton: function () 
                    {{
                        var escaped = $('#gview_{0}').html();
                        var findReplace = [[/&/g, ""&amp;""], [/</g, ""&lt;""], [/>/g, ""&gt;""], [/""/g, ""&quot;""], [/Loading.../g, """"]];
                        for (i = 0; i < findReplace.length; i++) 
                        {{
                            escaped = escaped.replace(findReplace[i][0], findReplace[i][1]);
                        }}
                        $('#exportData{0}').val(escaped);
                        $('#frmPost{0}').submit();
                    }}
                }});", this._id));
            }

            // print button
            if (this._print.HasValue && (bool)this._print)
            {
                if (!script.ToString().Contains("navGrid"))
                    script.AppendLine("jQuery('#" + this._id + "').jqGrid('navGrid',\"#" + this._pager + "\",{edit:false,add:false,del:false,search:false,refresh:false}); ");
                script.AppendLine("jQuery('#" + this._id + "').jqGrid(\"navButtonAdd\",\"#" + this._pager + "\"," +
                string.Format(@"
                {{
                    caption:'', 
                    buttonicon: 'ui-icon-print',
                    title: 'Print',
                    onClickButton: function () 
                    {{
                        var html = $('#gview_{0}').html();
                        html = html.replace(/border=0/g, 'border=""1""');
                        html = html.replace(/border=""0""/g, 'border=""1""');
                        var win = window.open('', 'ExportWindow', 'width=1024,height=800');
                        win.document.open();
                        win.document.writeln(html);
                        win.document.close();
                        win.print();
                        win.close();
                    }}
                }});", this._id));
            }

            if (!string.IsNullOrEmpty(_onDocumentReadyJavaScript))
                script.AppendLine(this._onDocumentReadyJavaScript);
            script.AppendLine("$('table#" + _id + "').trigger('ready',this);");
            // End script
            script.AppendLine("});");
            script.AppendFormat(@"
                $('#{0}').ready(function () {{
                    $('#gview_{0} .s-ico').prepend('&nbsp;&nbsp;');
                    $('#gview_{0} .s-ico').append('&nbsp;&nbsp;');
                }});
            ", this._id).AppendLine();
            script.AppendLine("</script>");

            // Create table which is used to render grid
            var table = new StringBuilder();
            table.AppendFormat("<table id=\"{0}\"></table>", this._id);

            // Create pager element if is set
            var pager = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(this._pager))
            {
                pager.AppendFormat("<div id=\"{0}\"></div>", this._pager);
            }

            // Create toppager element if is set
            var topPager = new StringBuilder();
            if (this._topPager == true)
            {
                topPager.AppendFormat("<div id=\"{0}_toppager\"></div>", this._id);
            }

            // Create error popup
            string errorDialog = string.Empty;
            if (_showError)
            {
                // errorDialog = string.Format("<div id='divErrPopup{0}' class='ui-dialog ui-widget ui-widget-content ui-corner-all ui-draggable ui-resizable' style='display:none'></div>", this._id);
                errorDialog = string.Format("<div id='divErrPopup{0}' style='display:none'></div>", this._id);
            }

            string exportForm = string.Empty;
            if (this._excelExport)
            {
                exportForm = string.Format("<form id='frmPost{0}' method='post' action='{1}?exportData=exportData{0}'><input id='exportData{0}' type='hidden' value='' name='exportData{0}'></form>", this._id, VirtualPathUtility.ToAbsolute(this._excelExportUrl));
            }

            // Insert grid id where needed (in columns)
            script.Replace("##gridid##", this._id);

            // Return script + required elements
            return script.ToString() + table.ToString() + pager.ToString() + topPager.ToString() + errorDialog + exportForm;
        }

        public string ToSubGridString()
        {
            bool IsEditable = (from c in _columns where c.IsEditable == true select c).Count() > 0 ? true : false;
            StringBuilder script = new StringBuilder();
            script.AppendLine("subGrid: true,");
            script.AppendFormat("subGridOptions: {0},", this._subGridOptions).AppendLine();
            script.AppendFormat(@"subGridRowExpanded: function (subgrid_id, row_id) {{
                var subgrid_table_id;
                var lastsel{0};
                subgrid_table_id = subgrid_id + '_t';
                jQuery('#'+subgrid_id).html('<table id=""' + subgrid_table_id + '"" class=""scroll""></table>');
                jQuery('#'+subgrid_table_id).jqGrid({{
            ", this._id).AppendLine();

            // Altrows
            if (this._altRows.HasValue) script.AppendFormat("altRows: {0},", this._altRows.Value.ToString().ToLower()).AppendLine();

            // Altclass
            if (!string.IsNullOrWhiteSpace(this._altClass)) script.AppendFormat("altclass: '{0}',", this._altClass).AppendLine();

            // Autoencode
            if (this._autoEncode.HasValue) script.AppendFormat("autoencode: {0},", this._autoEncode.Value.ToString().ToLower()).AppendLine();

            // Autowidth
            if (this._autoWidth.HasValue) script.AppendFormat("autowidth: {0},", this._autoWidth.Value.ToString().ToLower()).AppendLine();

            // Caption
            if (!string.IsNullOrWhiteSpace(this._caption)) script.AppendFormat("caption: '{0}',", this._caption).AppendLine();

            // Datatype
            script.AppendLine(string.Format("datatype:'{0}',", this._dataType.ToString()));

            // Emptyrecords
            if (!string.IsNullOrWhiteSpace(this._emptyRecords)) script.AppendFormat("emptyrecords: '{0}',", this._emptyRecords).AppendLine();

            // FooterRowe
            if (this._footerRow.HasValue) script.AppendFormat("footerrow: {0},", this._footerRow.Value.ToString().ToLower()).AppendLine();

            // Forcefit
            if (this._forceFit.HasValue) script.AppendFormat("forceFit: {0},", this._forceFit.Value.ToString().ToLower()).AppendLine();

            // Gridview
            if (this._gridView.HasValue) script.AppendFormat("gridview: {0},", this._gridView.Value.ToString().ToLower()).AppendLine();

            // HeaderTitles
            if (this._headerTitles.HasValue) script.AppendFormat("headertitles: {0},", this._headerTitles.Value.ToString().ToLower()).AppendLine();

            // Height (set 100% if no value is specified except when scroll is set to true otherwise layout is not as it is supposed to be)
            if (!this._height.HasValue)
            {
                if ((!this._scroll.HasValue || this._scroll.Value == false) && !this._scrollInt.HasValue) script.AppendFormat("height: '{0}',", "100%").AppendLine();
            }
            else script.AppendFormat("height: {0},", this._height).AppendLine();

            // Hiddengrid
            if (this._hiddenGrid.HasValue) script.AppendFormat("hiddengrid: {0},", this._hiddenGrid.Value.ToString().ToLower()).AppendLine();

            // Hidegrid
            if (this._hideGrid.HasValue) script.AppendFormat("hidegrid: {0},", this._hideGrid.Value.ToString().ToLower()).AppendLine();

            // HoverRows
            if (this._hoverRows.HasValue) script.AppendFormat("hoverrows: {0},", this._hoverRows.Value.ToString().ToLower()).AppendLine();

            // Loadonce
            if (this._loadOnce.HasValue) script.AppendFormat("loadonce: {0},", this._loadOnce.Value.ToString().ToLower()).AppendLine();

            // Loadtext
            if (!string.IsNullOrWhiteSpace(this._loadText)) script.AppendFormat("loadtext: '{0}',", this._loadText).AppendLine();

            // LoadUi
            if (this._loadUi.HasValue) script.AppendFormat("loadui: '{0}',", this._loadUi.Value.ToString()).AppendLine();

            // MultiBoxOnly
            if (this._multiBoxOnly.HasValue) script.AppendFormat("multiboxonly: {0},", this._multiBoxOnly.Value.ToString().ToLower()).AppendLine();

            // MultiKey
            if (this._multiKey.HasValue) script.AppendFormat("multikey: '{0}',", this._multiKey.Value.ToString()).AppendLine();

            // MultiSelect
            if (this._multiSelect.HasValue) script.AppendFormat("multiselect: {0},", this._multiSelect.Value.ToString().ToLower()).AppendLine();

            // MultiSelectWidth
            if (this._multiSelectWidth.HasValue) script.AppendFormat("multiselectWidth: {0},", this._multiSelectWidth.Value.ToString()).AppendLine();

            // Page
            if (this._page.HasValue) script.AppendFormat("page:{0},", this._page.Value).AppendLine();

            // Pager
            if (!string.IsNullOrWhiteSpace(this._pager)) script.AppendFormat("pager:'#{0}',", this._pager).AppendLine();

            // PagerPos
            if (this._pagerPos.HasValue) script.AppendFormat("pagerpos: '{0}',", this._pagerPos.ToString()).AppendLine();

            // PgButtons
            if (this._pgButtons.HasValue) script.AppendFormat("pgbuttons:{0},", this._pgButtons.Value.ToString().ToLower()).AppendLine();

            // PgInput
            if (this._pgInput.HasValue) script.AppendFormat("pginput: {0},", this._pgInput.Value.ToString().ToLower()).AppendLine();

            // PGText
            if (!string.IsNullOrWhiteSpace(this._pgText)) script.AppendFormat("pgtext: '{0}',", this._pgText).AppendLine();

            // RecordPos
            if (this._recordPos.HasValue) script.AppendFormat("recordpos: '{0}',", this._recordPos.Value).AppendLine();

            // RecordText
            if (!string.IsNullOrWhiteSpace(this._recordText)) script.AppendFormat("recordtext: '{0}',", this._recordText).AppendLine();

            // Request Type
            if (this._requestType.HasValue) script.AppendFormat("mtype: '{0}',", this._requestType.ToString()).AppendLine();

            // ResizeClass
            if (!string.IsNullOrWhiteSpace(this._resizeClass)) script.AppendFormat("resizeclass: '{0}',", this._resizeClass).AppendLine();

            // Rowlist
            if (this._rowList != null) script.AppendFormat("rowList: [{0}],", string.Join(",", ((from p in this._rowList select p.ToString()).ToArray()))).AppendLine();

            // Rownum
            if (this._rowNum.HasValue) script.AppendFormat("rowNum:{0},", this._rowNum.Value).AppendLine();

            // Rownumbers
            if (this._rowNumbers.HasValue) script.AppendFormat("rownumbers: {0},", this._rowNumbers.Value.ToString().ToLower()).AppendLine();

            // RowNumWidth
            if (this._rowNumWidth.HasValue) script.AppendFormat("rownumWidth: {0},", this._rowNumWidth.Value.ToString()).AppendLine();

            //Reader
            if (this._dataType == DataType.json || this._dataType == DataType.jsonp)
                if (!string.IsNullOrWhiteSpace(this._readerOptions)) script.AppendFormat("jsonReader : {{{0}}},", this._readerOptions.ToString()).AppendLine();
                else if (this._dataType == DataType.xml)
                    if (!string.IsNullOrWhiteSpace(this._readerOptions)) script.AppendFormat("xmlReader : {{{0}}},", this._readerOptions.ToString()).AppendLine();

            // Scroll (setters make sure either scroll or scrollint is set, never both)
            if (this._scroll.HasValue) script.AppendFormat("scroll:{0},", this._scroll.ToString().ToLower()).AppendLine();
            if (this._scrollInt.HasValue) script.AppendFormat("scroll:{0},", this._scrollInt.Value).AppendLine();

            // ScrollOffset
            if (this._scrollOffset.HasValue) script.AppendFormat("scrollOffset: {0},", this._scrollOffset.Value).AppendLine();

            // ScrollRows
            if (this._scrollRows.HasValue) script.AppendFormat("scrollrows: {0},", this._scrollRows.ToString().ToLower()).AppendLine();

            // ScrollTimeout
            if (this._scrollTimeout.HasValue) script.AppendFormat("scrollTimeout: {0},", this._scrollTimeout.Value).AppendLine();

            // Sortname
            if (!string.IsNullOrWhiteSpace(this._sortName)) script.AppendFormat("sortname: '{0}',", this._sortName).AppendLine();

            // Sorticons
            if (this._showAllSortIcons.HasValue || this._sortIconDirection.HasValue || this._sortOnHeaderClick.HasValue)
            {
                // Set defaults
                if (!this._showAllSortIcons.HasValue) this._showAllSortIcons = false;
                if (!this._sortIconDirection.HasValue) this._sortIconDirection = Direction.vertical;
                if (!this._sortOnHeaderClick.HasValue) this._sortOnHeaderClick = true;

                script.AppendFormat("viewsortcols: [{0},'{1}',{2}],", this._showAllSortIcons.Value.ToString().ToLower(), this._sortIconDirection.ToString(), this._sortOnHeaderClick.Value.ToString().ToLower()).AppendLine();
            }

            // Shrink to fit
            if (this._shrinkToFit.HasValue) script.AppendFormat("shrinkToFit: {0},", this._shrinkToFit.Value.ToString().ToLower()).AppendLine();

            // Sortorder
            if (this._sortOrder.HasValue) script.AppendFormat("sortorder: '{0}',", this._sortOrder.Value.ToString()).AppendLine();

            // Toolbar
            if (_toolbar.HasValue) script.AppendFormat("toolbar: [{0},\"{1}\"],", this._toolbar.Value.ToString().ToLower(), this._toolbarPosition.ToString()).AppendLine();

            // Toppager
            if (this._topPager.HasValue) script.AppendFormat("toppager: {0},", this._topPager.Value.ToString().ToLower()).AppendLine();

            // Url
            string newurl = this._url;
            if (!string.IsNullOrWhiteSpace(newurl) && newurl.Contains("?"))
            {
                int index = newurl.IndexOf("?");
                newurl = newurl.Insert(index, "/'+row_id+'/");
                if (!string.IsNullOrWhiteSpace(newurl)) script.AppendFormat("url: '{0}',", ((this._pathtype.HasValue && this._pathtype.Value.Equals(PathType.relative)) ? VirtualPathUtility.ToAbsolute(newurl) : newurl)).AppendLine();
            }
            else
                if (!string.IsNullOrWhiteSpace(this._url)) script.AppendFormat("url: '{0}' + row_id,", ((this._pathtype.HasValue && this._pathtype.Value.Equals(PathType.relative)) ? VirtualPathUtility.ToAbsolute(this._url) : this._url)).AppendLine();

            // edit url
            if (!string.IsNullOrWhiteSpace(this._editUrl)) script.AppendFormat("editurl: '{0}',", VirtualPathUtility.ToAbsolute(this._editUrl)).AppendLine();

            // image path
            if (!string.IsNullOrWhiteSpace(this._imgPath)) script.AppendFormat("imgpath: '{0}',", VirtualPathUtility.ToAbsolute(this._imgPath)).AppendLine();

            // View records
            if (this._viewRecords.HasValue) script.AppendFormat("viewrecords: {0},", this._viewRecords.Value.ToString().ToLower()).AppendLine();

            // Width
            if (this._width.HasValue) script.AppendFormat("width: '{0}',", this._width.Value).AppendLine();

            // tree grid
            if (this._treeGrid.HasValue) script.AppendFormat("treeGrid: {0},", this._treeGrid.Value.ToString().ToLower()).AppendLine();
            if (this._treeGridModel.HasValue) script.AppendFormat("treeGridModel: '{0}',", this._treeGridModel.ToString()).AppendLine();
            if (!string.IsNullOrWhiteSpace(this._treeGridExpandColumn)) script.AppendFormat("ExpandColumn: '{0}',", this._treeGridExpandColumn).AppendLine();
            if (this._treeGridExpandColClick.HasValue) script.AppendFormat("ExpandColClick: {0},", this._treeGridExpandColClick.ToString().ToLower()).AppendLine();

            // onAfterInsertRow
            if (!string.IsNullOrWhiteSpace(this._onAfterInsertRow)) script.AppendFormat("afterInsertRow: function(rowid, rowdata, rowelem) {{{0}}},", this._onAfterInsertRow).AppendLine();

            // onBeforeRequest
            if (!string.IsNullOrWhiteSpace(this._onBeforeRequest)) script.AppendFormat("beforeRequest: function() {{{0}}},", this._onBeforeRequest).AppendLine();

            // onBeforeSelectRow
            if (!string.IsNullOrWhiteSpace(this._onBeforeSelectRow)) script.AppendFormat("beforeSelectRow: function(rowid, e) {{{0}}},", this._onBeforeSelectRow).AppendLine();

            // onGridComplete
            if (!string.IsNullOrWhiteSpace(this._onGridComplete)) script.AppendFormat("gridComplete: function() {{{0}}},", this._onGridComplete).AppendLine();

            // onLoadBeforeSend
            if (!string.IsNullOrWhiteSpace(this._onLoadBeforeSend)) script.AppendFormat("loadBeforeSend: function(xhr) {{{0}}},", this._onLoadBeforeSend).AppendLine();

            // onLoadComplete
            if (!string.IsNullOrWhiteSpace(this._onLoadComplete)) script.AppendFormat("loadComplete: function(xhr) {{{0}}},", this._onLoadComplete).AppendLine();

            // onLoadError
            if (this._showError)
            {
                script.AppendFormat(@"loadError: function(xhr, status, error) {{
                    if ($('#divErrPopup{0}').length == 0)
                        $('body').prepend($(""<div id='divErrPopup{0}' style='display:none'></div>""));                        
            
                    if ($('#divErrPopup{0}').html(error))
                    {{
                        $('#divPupFrm').dialog('option', 'title', '{1}');
                        $('#divPupFrm').append('{2}');
                    }}
                    else
                    {{
                        $('#divErrPopup{0}').empty();
                        $('#divErrPopup{0}').append('<p>');
                        $('#divErrPopup{0}').append('{2}');
                        $('#divErrPopup{0}').append(error);
                        $('#divErrPopup{0}').append('</p>');
                        $('#divErrPopup{0}').dialog({{
                            modal: true,
                            title: '{1}'
                        }});
                    }}
                }},", this._id, this._onLoadError_title, this._onloadError_message).AppendLine();
            }

            // onCellSelect
            if (!string.IsNullOrWhiteSpace(this._onCellSelect)) script.AppendFormat("onCellSelect: function(rowid, iCol, cellcontent, e) {{{0}}},", this._onCellSelect).AppendLine();

            // onDblClickRow
            if (!string.IsNullOrWhiteSpace(this._onDblClickRow)) script.AppendFormat("ondblClickRow: function(rowid, iRow, iCol, e) {{{0}}},", this._onDblClickRow).AppendLine();

            // onHeaderClick
            if (!string.IsNullOrWhiteSpace(this._onHeaderClick)) script.AppendFormat("onHeaderClick: function(gridstate) {{{0}}},", this._onHeaderClick).AppendLine();

            // onPaging
            if (!string.IsNullOrWhiteSpace(this._onPaging)) script.AppendFormat("onPaging: function(pgButton) {{{0}}},", this._onPaging).AppendLine();

            // onRightClickRow
            if (!string.IsNullOrWhiteSpace(this._onRightClickRow)) script.AppendFormat("onRightClickRow: function(rowid, iRow, iCol, e) {{{0}}},", this._onRightClickRow).AppendLine();

            // onSelectAll
            if (!string.IsNullOrWhiteSpace(this._onSelectAll)) script.AppendFormat("onSelectAll: function(aRowids, status) {{{0}}},", this._onSelectAll).AppendLine();

            // onSelectRow event
            if (!string.IsNullOrWhiteSpace(this._onSelectRow)) script.AppendFormat("onSelectRow: function(rowid, status) {{{0}}},", this._onSelectRow).AppendLine();

            // onSortCol
            if (!string.IsNullOrWhiteSpace(this._onSortCol)) script.AppendFormat("onSortCol: function(index, iCol, sortorder) {{{0}}},", this._onSortCol).AppendLine();

            // onResizeStart
            if (!string.IsNullOrWhiteSpace(this._onResizeStart)) script.AppendFormat("resizeStart: function(event, index) {{{0}}},", this._onResizeStart).AppendLine();

            // onResizeStop
            if (!string.IsNullOrWhiteSpace(this._onResizeStop)) script.AppendFormat("resizeStop: function(newwidth, index) {{{0}}},", this._onResizeStop).AppendLine();

            // onSerializeGridData
            if (!string.IsNullOrWhiteSpace(this._onSerializeGridData)) script.AppendFormat("serializeGridData: function(postData) {{{0}}},", this._onSerializeGridData).AppendLine();

            // Colmodel
            script.AppendLine("colModel: [");
            string colModel = string.Join(",\r\n", ((from c in this._columns select c.ToString()).ToArray()));
            script.AppendLine(colModel);
            script.AppendLine("],");

            if (IsEditable)
            {
                script.AppendFormat(@"
                onSelectRow: function(id)
                {{
                    if (id && id !== lastsel{0})
                    {{
                        jQuery('#{0}').restoreRow(lastsel{0});
                        jQuery('#{0}').editRow(id,true);
                        lastsel{0}=id;
                    }}
                }},", this._id).AppendLine();
            }

            // tree grid
            if (this._treeGrid.HasValue) script.AppendFormat("treeGrid: {0},", this._treeGrid.Value.ToString().ToLower()).AppendLine();
            if (this._treeGridModel.HasValue) script.AppendFormat("treeGridModel: '{0}',", this._treeGridModel.ToString()).AppendLine();
            if (!string.IsNullOrWhiteSpace(this._treeGridExpandColumn)) script.AppendFormat("ExpandColumn: '{0}',", this._treeGridExpandColumn).AppendLine();
            if (this._treeGridExpandColClick.HasValue) script.AppendFormat("ExpandColClick: {0},", this._treeGridExpandColClick.ToString().ToLower()).AppendLine();

            // grouping
            if (this._grouping.HasValue) script.AppendFormat("grouping: {0},", this._grouping.Value.ToString().ToLower()).AppendLine();
            if (!string.IsNullOrWhiteSpace(this._groupingView)) script.AppendFormat("groupingView : {{{0}}},", this._groupingView).AppendLine();

            // sub grid
            if (this._subGrid != null) script.AppendLine(this._subGrid.ToSubGridString());
            if (this._customGrid != null) script.AppendLine(this._customGrid.ToCustomGridString());
            if (this._customGridJson != null) script.AppendLine(this._customGridJson.ToCustomGridStringJson());

            // Remove last comma from string
            if (script.ToString().LastIndexOf(",\r\n") > -1) script = script.Remove(script.Length - 3, 1);

            // End jqGrid call
            script.AppendLine("});");

            if (IsEditable && !string.IsNullOrWhiteSpace(this._pager))
            {
                script.AppendLine("jQuery('#" + this._id + "').jqGrid('navGrid',\"#" + this._pager + "\",{edit: true, add: false, del: true});");
            }

            //Call the function once sub grid is loadded..
            if (string.IsNullOrEmpty(this._onSubGridLoadComplete) == false)
            {
                script.AppendLine(string.Format("{0}({1},{2});", this._onSubGridLoadComplete, "subgrid_id", "row_id"));
            }
            // End of function
            script.AppendLine("}");

            // Return script + required elements
            return script.ToString();
        }

        /// <summary>
        /// <param>Generates sub grid for JSONp back end call for each parent row. Allows for caching retrieved child data</param>
        /// <param>and therefore avoid unnecessary round trips when desired. Every row click sets custom property "selectedData" </param>
        /// <param>on parent grid, accessible in client-side with $container.jqGrid('getGridParam','selectedData') method.</param>
        /// <param>Such custom field on PARENT grid defines two object-typed fields: parent and child. If parent row had been clicked</param>
        /// <param>or collapsed, this field has data, related to the parent row only and the child field is null. If any of the children</param>
        /// <param>rows was clicked, than this grid-level property contains data for both related parent and child. These are the fields</param>
        /// <param>for both parent and child objects: rowData,rowId,rowElement. These are client-side events that this grid raises on the</param>
        /// <param>parent grid level: "parentRowSelected","parentRowBeforeExpand", "subGridItemClick", "parentRowAfterExpand", </param>
        /// <param>"parentRowCollapsed", and "error". For argument structure of each of these event please refer to this method's implementation.</param>
        /// <param>This subGrid allows passing-in two client-side event delegates: </param>
        /// <param>"setCustomSubGridJsonBeforeExpandDataSuccessDelegate" and "setCustomSubGridJsonAfterExpandDataSuccessDelegate".</param>
        /// <param>These allow running passed-in method delegate according to their named events.</param>
        /// </summary>
        /// <returns></returns>
        public string ToCustomGridStringJson()
        {/* ******************************************************************************************************
          * ************ Client-side events and their corresponding event arguments: *****************************
          * ------------------------------------------------------------------------------------------------------
          * parentRowSelected ->    {parent:{rowData,
          *                                 rowId,
          *                                 rowElement},
          *                          child:null
          *                         }
          * ------------------------------------------------------------------------------------------------------
          * parentRowBeforeExpand -> { expandedRowData,
                                      subGridId,
                                      subGridTableId,
                                      parentRowId}
          * ------------------------------------------------------------------------------------------------------
          * subGridItemClick ->      {    parent:{rowData,rowId,rowElement},
                                            child:{rowData,rowId,rowElement} 
          *                          }
          * ------------------------------------------------------------------------------------------------------
          * parentRowAfterExpand ->  {  expandedRowData,expandedRowRef,expandedRowId,childData}
          * ------------------------------------------------------------------------------------------------------
          * parentRowCollapsed ->   {  rowId,subGridId,collapsedRows}
          * ------------------------------------------------------------------------------------------------------
          * error ->                 {MessageList:<collection of server-side errors while extracting data>,
                                        expandedRowData, 
                                        expandedRowId,
                                        subgridParentRef, 
                                        serverResponseDat,
                                        subgridId, 
                                        subgridTableId}
          * *****************************************************************************************************
          ------- CUSTOM CLIENT-SIDE METHODS: ------------------------------------------------------------------------
          * ********************************************************************************************************
          * addChildData({rowId,html,aboveExisting[false]}) => adds new child elements to either empty parent or appends to existing child elements.
          *                               Does not return anything;
          * ----------------------------------------------------------------------------------------------------------
          * replaceChildData({rowId,html}) => replaces child elements for passed-in parent row ID with new passed-in 
          *                                   child elements. Does not return anything;
          * ----------------------------------------------------------------------------------------------------------
          * getChildData(parentRowId) => returns object with following structure: {childRows,parentRow, parentRowData} 
          *********************************************************************************************************/
            /* **************************** Applicable CSS classes: **********************************************
             * _customSubGridJsonSelectedRowClass
             * parentGridRef._altClass
             * _customSubGridJsonAltClass
             * _customSubGridJsonMouseOverClass             * 
             * ***************************************************************************************************/
            bool IsEditable = (from c in _columns where c.IsEditable == true select c).Count() > 0 ? true : false;
            string URL = NormalizedUrl;
            string loadingContent = string.Empty;
            string loadingClass = string.Empty;
            StringBuilder script = new StringBuilder();

            //*******************************************************************************************************************
            //************ BEGINNING OF "EXTENSION" OBJECT, DEFINING SUB-ITEM INITIALIZATION, STYLING AND EVENT MANAGEMENT; *************
            //********************************************************************************************************************
            script.AppendLine("var extensionParentGrid" + _parentId + @" = $('table#" + _parentId + "');");
            script.AppendLine("extensionParentGrid" + _parentId + @".bind('reloadGrid',function(event,reloadArgs){
                for (var prop in extensionParentGrid" + _parentId + @".data()) {
                if (extensionParentGrid" + _parentId + @".data().hasOwnProperty(prop) && (prop.indexOf('childLoaded') > -1 || prop.indexOf('isReplace') > -1))
                    delete extensionParentGrid" + _parentId + @".data()[prop];
                }
                extensionParentGrid" + _parentId + @".find('TR[isExpanded]').removeAttr('isExpanded');
            });");

            script.AppendLine(@"var extension = {
                            isChildLoaded:function(parentRowId){
                                var isLoaded = extensionParentGrid" + _parentId + @".data('childLoaded'+parentRowId);
                                if(!isLoaded)
                                    return false;
                                return true;
                            },
                            isRowExpanded:function(parentRowId){
                                var grd = extensionParentGrid" + _parentId + @";
                                var targetRowRef = grd.children().children('TR#'+parentRowId);
                                if(targetRowRef.length>0 && (targetRowRef.attr('isExpanded')==true || targetRowRef.attr('isExpanded')=='true'))
                                    return true;
                                return false;
                            },
                            getChildData:function(parentRowId){
                                return {childRows: extensionParentGrid" + _parentId + @".children().children('TR[parent='+parentRowId+']'),parentRow:extensionParentGrid" + _parentId + @".children().children('TR#'+parentRowId)[0],
                                    parentRowData:extensionParentGrid" + _parentId + @".jqGrid('getRowData',parentRowId)};
                            },

                            toggleChildRowHighlight:function(tgglChldArgs){");
            if (!string.IsNullOrEmpty(this._customSubGridJsonChildHighlightClass))
                script.AppendLine(@"
                                var targetRow = tgglChldArgs.childRef;
                                if(tgglChldArgs.isHighlight){
                                    targetRow.children('td')" + (_customSubGridJsonChildExcludeHighlightClass != "" ? @".not('." + _customSubGridJsonChildExcludeHighlightClass + "')" : @"") + @".addClass('" + this._customSubGridJsonChildHighlightClass + @"');
                                }
                                else {
                                    targetRow.children('td').removeClass('" + this._customSubGridJsonChildHighlightClass + @"');
                                    if(targetRow.data('OriginalClass'))
                                        targetRow.addClass(targetRow.data('OriginalClass'));
                                    else if(targetRow.attr('class') && targetRow.attr('class').indexOf('" + this._customSubGridJsonAltClass + @"')>-1){
                                        targetRow.addClass('" + this._customSubGridJsonAltClass + @"');
                                       targetRow.data('OriginalClass','" + this._customSubGridJsonAltClass + @"'); 
                                    }
                                }");
            script.AppendLine(@"
                            },//<-end of method 'toggleChildRowHighlight';

                            replaceChildData:function(args){
                                var childData = extensionParentGrid" + _parentId + @".children().children('TR[parent='+args.rowId+']');
                                if(childData.length==0)
                                    return;
                                childData.remove();
                                args.isReplace=true;
                                args.isQueue=true;
                                extensionParentGrid" + _parentId + @".jqGrid('addChildData',args);
                            },
                             addChildData:function(args){");
            script.AppendLine(@"var parentRowRef" + _parentId + " = extensionParentGrid" + _parentId + @".children().children('TR#'+args.rowId); 
                                var arguments = {
                                expandedRowData:extensionParentGrid" + _parentId + @".jqGrid('getRowData',args.rowId), 
                                expandedRowId:args.rowId,  
                                expandedRowRef:extensionParentGrid" + _parentId + @".children().children('TR#'+args.rowId),
                                serverResponseHtml:args.html,
                                clientModel:args.clientModel,
                                aboveExisting:args.aboveExisting,
                                addedChildData:args.addedChildData,
                                overrideResponseRender:false };");
            //script.AppendLine("alert(arguments.expandedRowRef.html());");
            //<-- Once back end server call is made and data retrieved, this allows calling passed-in javascript delegate
            // before retrieved data is actually rendered. If javascript delegate method returns "overrideResponseRender" field
            // set to "true", then following built-in data rendering is skipped and therefore custom rendering is defined 
            // externally in passed-in delegate method:
            if (!string.IsNullOrEmpty(_customSubGridJsonBeforeExpandDataSuccessDelegate))
            {
                script.Append(_customSubGridJsonBeforeExpandDataSuccessDelegate);
                script.Append("(arguments);if(arguments.overrideResponseRender)return;");
            }
            //<-- If passed-in client-side delegate does not override built-in data rendering, this will generate
            // HTML for retrieved data:
            script.Append("if(args.isQueue==true)arguments.expandedRowRef.attr('isExpanded',true);");
            script.Append(@" var isExpanded = extensionParentGrid" + _parentId + @".jqGrid('isRowExpanded',arguments.expandedRowId);                                    
                                if(args && args.isReplace){
                                     extensionParentGrid" + _parentId + @".data('isReplace'+args.rowId,true);
                                   extensionParentGrid" + _parentId + @".expandSubGridRow(args.rowId);
                                }
                             if(!isExpanded){
                                extensionParentGrid" + _parentId + @".expandSubGridRow(arguments.expandedRowId);
                                 var callAgain = window.setTimeout(function () {
                                    extensionParentGrid" + _parentId + @".jqGrid('addChildData',args);
                                    window.clearTimeout(callAgain);
                                 }, 100);
                                 return this;
                            }");
            script.Append("arguments.cancel=false;extensionParentGrid" + _parentId + @".trigger('beforeChildAdded',arguments);if(arguments.cancel==true)return this;");
            script.Append(@"var lastSibling = extensionParentGrid" + _parentId + @".children().children('TR[parent='+arguments.expandedRowId+']:last');
                             var alterStyle= true;
                             ");
            if (!string.IsNullOrEmpty(this._customSubGridJsonAltClass))
            {
                script.Append("if(lastSibling.length>0 && lastSibling.hasClass('");
                script.Append(this._customSubGridJsonAltClass);
                script.Append("') || lastSibling.data('OriginalClass')=='");
                script.Append(this._customSubGridJsonAltClass);
                script.Append("')alterStyle=false;");
            }
            if (!string.IsNullOrWhiteSpace(parentGridRef._altClass))
            {
                script.Append("if(lastSibling.length==0 && parentRowRef" + _parentId + ".hasClass('");
                script.Append(parentGridRef._altClass);
                script.Append("')) alterStyle = false;");
            }
            script.Append(@"var rows = $(document.createElement('div')).append($(arguments.serverResponseHtml)).children('TR');
                                        var updatedHtml = $(document.createElement('div'));");
            script.Append(@"
                                        $.each(rows,function(index,value){
                                            var $row = $(value);
                                            $row.attr('parent',args.rowId);");
            //<-- If alternate style is defined for this subgrid type, assign passed-in class name to the row: ----------
            if (!string.IsNullOrEmpty(_customSubGridJsonAltClass))
            {
                script.AppendLine("if(alterStyle){");
                script.Append("$row.addClass(\"");
                script.Append(_customSubGridJsonAltClass);
                script.Append("\");$row.data('OriginalClass','" + _customSubGridJsonAltClass + "');}alterStyle=!alterStyle;");
            }
            //<-- If MouseOver class is assigned, this generates highlight styles for mouse over and mouse out: -----------------------
            if (!string.IsNullOrEmpty(_customSubGridJsonMouseOverClass))
            {
                script.Append(@"
                                $row.hover(function(){");
                if (!string.IsNullOrEmpty(this._customSubGridJsonSelectedRowClass))
                {
                    script.Append("if($(this).hasClass('");
                    script.Append(this._customSubGridJsonSelectedRowClass);
                    script.Append("'))return;");
                }
                script.Append(@" $(this).removeClass(function(index,value){
                                if(value){
                                    $row.data('OriginalClass',value);
                                };})/*End of removing class logic;*/
                               .addClass('");
                script.Append(_customSubGridJsonMouseOverClass);
                script.Append(@"');},function(){");
                if (!string.IsNullOrEmpty(this._customSubGridJsonSelectedRowClass))
                {
                    script.Append("if($(this).hasClass('");
                    script.Append(this._customSubGridJsonSelectedRowClass);
                    script.Append("'))return;");
                }
                script.Append(" $(this).removeClass();if($(this).data('OriginalClass'))$row.addClass($row.data('OriginalClass'));});");
            }
            script.Append("$row.data('parentRowId',args.rowId);$row.data('parentRowData',extensionParentGrid" + _parentId + @".jqGrid('getRowData',args.rowId));$row.data('parentRowRef',extensionParentGrid" + _parentId + @".children().children('TR#'+args.rowId));");
            //<-- Raising text box change events for all child text boxes: -------------------------
            script.Append(@"$row.find('INPUT[type=text]')
                            .change(function(){$(this).trigger('textboxChange',
                                    {target:this,childRow:$row,parentRow:$row.data('parentRowRef'),
                                     parentRowData:$row.data('parentRowData'),
                                     parentRowId:$row.data('parentRowId')});})
                            .focus(function(){$row.click();}); ");
            //<-- This triggers client-side event "subGridItemClick" and passes arguments object: -----------------
            script.Append("$row.click(function(){");
            //<--First step is un-select all parent-level rows because the child is being selected:
            script.Append("extensionParentGrid" + _parentId + @".jqGrid('resetSelection');");
            //<-- If SelectedRowClass was assigned, than this injects appropriate javascript: -------------------------
            if (!string.IsNullOrEmpty(this._customSubGridJsonSelectedRowClass))
            {
                script.Append("$row.removeClass();");
                script.Append("var selectedRows = extensionParentGrid" + _parentId + @".children().children('.");
                script.Append(this._customSubGridJsonSelectedRowClass);
                script.Append(@"'); $.each(selectedRows,function(index,value){
                                $(value).removeClass().addClass($(value).data('OriginalClass'));
                            });");
                script.Append("$row.addClass('");
                script.Append(this._customSubGridJsonSelectedRowClass);
                script.Append("');");
            }
            //<-- Raising client side event "subGridItemClick" here: ------------------------------------
            script.Append(@" var selectedData={
                            parent:{rowData:$(this).data('parentRowData'),rowId:$(this).data('parentRowId'),rowElement:$(this).data('parentRowRef')},
                            child:{rowData:$row.children(),rowId:$row.attr('id'),rowElement:$row}};
                            extensionParentGrid" + _parentId + @".jqGrid('setGridParam',{selectedData:selectedData});
                        ");
            script.Append(@"extensionParentGrid" + _parentId + @".trigger('subGridItemClick',{
                                clickedRowData:$row,
                                parentRowId:$(this).data('parentRowId'),
                                parentRowRef:$(this).data('parentRowRef'),
                                parentRowData:$(this).data('parentRowData')
                        });");

            script.Append("});");
            //-------------------------------------------------------------------------------------------
            script.Append(@"updatedHtml.append(value);
                                        }); var appendTarget; if(lastSibling.length==0){
                                        appendTarget = $($('table#");
            script.Append(this._parentId);
            script.Append(@" tr#'+arguments.expandedRowId));}else{appendTarget = lastSibling;}
            if(args.aboveExisting){
                appendTarget.before(updatedHtml.children());                
                $(arguments.expandedRowRef).attr('childOnTop',false);
            }
            else
                appendTarget.after(updatedHtml.children());
            ");
            //<-- If client-side delegate was defined to be called after retrieved data is rendered, 
            // this will call passed-in javascript method:
            if (!string.IsNullOrEmpty(_customSubGridJsonAfterExpandDataSuccessDelegate))
            {
                script.Append(_customSubGridJsonAfterExpandDataSuccessDelegate);
                script.Append(@"(arguments);");
            }
            script.AppendLine("}};if(!extensionParentGrid" + _parentId + @".jqGrid('getGridParam','extended')){$.extend(extensionParentGrid" + _parentId + @".jqGrid,extension);extensionParentGrid" + _parentId + @".jqGrid('setGridParam',{extended:true});}");
            //*******************************************************************************************************************
            //************ END OF "EXTENSION" OBJECT, DEFINING SUB-ITEM INITIALIZATION, STYLING AND EVENT MANAGEMENT; *************
            //********************************************************************************************************************

            _parentGridRef._onDocumentReadyJavaScript += script.ToString();
            script.Clear();

            script.Append("onSelectRow:function(rowid,status){var grid = $('table#");
            script.Append(this._parentId);
            script.Append("'); var source = grid.jqGrid('getRowData',rowid); if(!source)return;");
            script.Append(" var selectedData={parent:{rowData:source,rowId:rowid,rowElement:grid.children().children('TR#'+rowid)},child:null};");
            script.Append("grid.jqGrid('setGridParam',{selectedData: selectedData});");
            script.Append("grid.trigger('parentRowSelected',selectedData);");
            if (!string.IsNullOrEmpty(this._customSubGridJsonSelectedRowClass))
            {
                script.Append("var selectedRows = grid.children().children('TR.");
                script.Append(this._customSubGridJsonSelectedRowClass);
                script.Append(@"'); $.each(selectedRows,function(index,value){
                    $(value).removeClass();$(value).addClass($(value).data('OriginalClass'));
                });");
            }
            script.Append("},");

            if (!string.IsNullOrWhiteSpace(this._loadingImageSrc))
            {
                loadingContent = string.Format(@"<img src=""{0}"" />", ((this._pathtype.HasValue && this._pathtype.Value.Equals(PathType.relative) && this._url.IndexOf("http") < 0) ? VirtualPathUtility.ToAbsolute(this._loadingImageSrc) : this._loadingImageSrc));
                loadingClass = "";
            }
            else
            {
                loadingContent = "Loading...";
                loadingClass = @"class=""i-state-default ui-state-active"" ";
            }

            script.AppendLine("subGrid: true,");
            script.AppendLine(@"subGridRowExpanded: function (subgrid_id, row_id) {{                
                var trId = new String(row_id);
                var subgridId = subgrid_id;
                var subgrid_table_id = subgrid_id + '_t';
                jQuery('#' + subgrid_id).html('<center><div " + loadingClass + @" style=""top: 45%;left: 45%;width:150px;z-index:101;padding: 6px; margin: 5px;text-align: center;font-weight: bold;border-width: 2px !important;"">" + loadingContent + @"</div></center>');
                var subgridParent=jQuery('#' + subgrid_id).parent().parent();
                $(subgridParent).children('td').css({'border':'0px'});
                var mainGrid = subgridParent.parent().parent();
                mainGrid.jqGrid('setSelection',trId);
                var expandRowData = mainGrid.jqGrid(""getRowData"",row_id);
                var parentRowRef = mainGrid.children().children('TR#'+row_id);");
            script.Append("var selectedData={parent:{rowData:expandRowData,rowId:row_id,rowElement:parentRowRef},child:null};");
            script.Append("mainGrid.jqGrid('setGridParam',{selectedData:selectedData});");
            //<-- If SelectedRowClass was assigned, than this injects appropriate javascript: -------------------------
            if (!string.IsNullOrEmpty(this._customSubGridJsonSelectedRowClass))
            {
                //script.Append("alert(mainGrid.children().children('."+this._customSubGridJsonSelectedRowClass+"').length);");
                script.Append("var selectedRowsToClear = mainGrid.children().children('.");
                script.Append(this._customSubGridJsonSelectedRowClass);
                script.Append(@"'); $.each(selectedRowsToClear,function(index,value){
                    $(value).removeClass().addClass($(value).data('OriginalClass'));
                });");
            }
            //<-- Raise client-side event "parentRowBeforeExpand" when parent row is clicked to expand, but before it actually expanded: ---------------------
            script.AppendLine(@"
                mainGrid.trigger('parentRowBeforeExpand',{
                    expandedRowData:expandRowData,
                    subGridId:subgridId,
                    clientModel:mainGrid.data('clientModel'+row_id),
                    isNewExpand:!mainGrid.jqGrid('isChildLoaded',row_id),
                    subGridTableId:subgrid_table_id,
                    parentRowId:trId});");
            //<-- If CacheExpandedData property is set to true, than previously expanded data is to be maintained across collapsing and expanding: -------------------
            if (_customSubGridJsonCacheExpandedData.HasValue && _customSubGridJsonCacheExpandedData.Value)
            {
                script.AppendLine(@"var rows = $('table#" + this._parentId + @" tr');
                    var collapsedRows = $(rows).filter(function(index,value){
                        return $(value).attr('parent')== row_id;
                    });
                    if(collapsedRows.length>0){
                    $('#' + subgridId).remove();
                    $('.ui-subgrid').remove();
                    $.each(collapsedRows,function(index,value){
                        $(value).toggle();});parentRowRef.attr('isExpanded',true);");
                script.AppendLine(@" mainGrid.trigger('parentRowAfterExpand',{
                expandedRowData:expandRowData,
                expandedRowRef:parentRowRef,
                expandedRowId:trId,
                clientModel:mainGrid.data('clientModel'+row_id),
                isNewExpand:false,
                childData:collapsedRows});");
                script.Append("}else if(mainGrid.data('isReplace'+row_id)==true){ ");
                script.Append(@"mainGrid.data('isReplace'+row_id,false);
                    $('#' + subgridId).remove();
                    $('.ui-subgrid').remove();");
                script.Append("}else{");
            }
            //<--- If getting data from server for expanded row is required, this will make a call to the 
            // action with JSONp technology and submit expanded row data: -------------
            script.Append(@"
                $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',");
            script.Append("url: '" + URL + "/?rowId='+row_id+'&jsonCallback=?',");
            script.AppendLine(@"dataType:'json',
                data:jQuery.toJSON({expandedRow:expandRowData}),");
            script.AppendLine(@" 
                success:function(viewResult,status, args)
                        {
                            $('#' + subgridId).remove();
                            $('.ui-subgrid').remove();");
            //<-- Handle data retrieval error by checking MessageList object and raising client-side "error" event:

            script.Append(@"if(!viewResult || viewResult.MessageList){
                    if(!viewResult)
                             viewResult = {MessageList:[{Message:'Server did not return any data',FieldName:'Subgrid ""'+subgridId+'""'}]};
                    viewResult.expandedRowData=expandRowData; 
                    viewResult.expandedRowId=row_id; 
                    viewResult.subgridParentRef=subgridParent; 
                    viewResult.serverResponseData=viewResult; 
                    viewResult.subgridId=subgridId; 
                    viewResult.subgridTableId=subgrid_table_id;
                    mainGrid.trigger('error',viewResult);
                    return;
            }");
            script.AppendLine(@"mainGrid.jqGrid('addChildData',{html:viewResult.html,clientModel:viewResult.data,rowId:row_id,aboveExisting:parentRowRef.attr('childOnTop'),isQueue:true});
            mainGrid.data('childLoaded'+row_id,true);
            mainGrid.data('clientModel'+row_id,viewResult.data);");
            //************************************************************************************************************************************************************************
            //<-- Raise client-side event "" when parent row is clicked to expand, but before it actually expanded: ---------------------
            script.AppendLine(@" mainGrid.trigger('parentRowAfterExpand',{
                expandedRowData:expandRowData,
                expandedRowRef:parentRowRef,
                expandedRowId:trId,
                clientModel:viewResult.data,
                isNewExpand:true,
                childData:$(document.createElement('div')).append(viewResult.html).children()
            });");
            script.Append("},");
            script.Append(CustomGridJsonClientErrorHandler);
            script.AppendLine(@"});}}");
            if (_customSubGridJsonCacheExpandedData.HasValue && _customSubGridJsonCacheExpandedData.Value)
                script.Append("}");
            script.Append(@",
                subGridRowColapsed: function(subgrid_id, row_id)
                { var rows = $('table#" + this._parentId + @" tr');
                    var collapseRows = $(rows).filter(function(index,value){
                        return $(value).attr('parent')== row_id;
                    });
                    var grid = $('table#" + this._parentId + @"');
                    grid.children().children('TR#'+row_id).attr('isExpanded',false);
                    grid.trigger('parentRowCollapsed',{rowId:row_id,subGridId:subgrid_id,collapsedRows:collapseRows});
                    $.each(collapseRows,function(index,value){");
            //<-- If client data caching is defined, this will hide previously expanded HTML rather than
            // removing it from the dome. This allows to preserve any data changes, made to subgrid data 
            // on the UI across multiple collapse/expand operations and save server round trip.
            if (_customSubGridJsonCacheExpandedData.HasValue && _customSubGridJsonCacheExpandedData.Value)
                script.Append("$(value).toggle();");
            else
                script.Append("$(value).remove();");
            script.AppendLine("}); },");
            script.AppendFormat("subGridOptions: {0}", this._subGridOptions).AppendLine();
            // Return script + required elements
            return script.ToString();
        }
        /// <summary>
        /// Return a View Of your Own instead of the table
        /// </summary>
        /// <returns></returns>
        public string ToCustomGridString()
        {
            bool IsEditable = (from c in _columns where c.IsEditable == true select c).Count() > 0 ? true : false;
            string URL = NormalizedUrl;
            string loadingContent = string.Empty;
            string loadingClass = string.Empty;
            StringBuilder script = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(this._loadingImageSrc))
            {
                loadingContent = string.Format(@"<img src=""{0}"" />", ((this._pathtype.HasValue && this._pathtype.Value.Equals(PathType.relative) && this._url.IndexOf("http") < 0) ? VirtualPathUtility.ToAbsolute(this._loadingImageSrc) : this._loadingImageSrc));
                loadingClass = "";
            }
            else
            {
                loadingContent = "Loading...";
                loadingClass = @"class=""i-state-default ui-state-active"" ";
            }

            script.AppendLine(@"
                subGrid: true,
                subGridRowExpanded: function (subgrid_id, row_id) 
                {
                    var trId = new String(row_id);
                    var subgridId = subgrid_id;
                    var subgrid_table_id = subgrid_id + '_t';
                    jQuery('#' + subgrid_id).html('<center><div " + loadingClass + @" style=""top: 45%;left: 45%;width:150px;z-index:101;padding: 6px; margin: 5px;text-align: center;font-weight: bold;border-width: 2px !important;"">" + loadingContent + @"</div></center>');
                    var subgridParent=jQuery('#' + subgrid_id).parent().parent();
                    $(subgridParent).children('td').css({'border':'0px'});
                    $.ajax(
                    {
                        type: 'get',
                        contentType: 'application/json; charset=utf-8',
                        url: '" + URL + @"'," + CustomGridClientSuccessHandler + ",");
                        script.AppendLine(CustomGridClientErrorHandler);
                        script.AppendLine(@"                        
                    });
                },
                subGridRowColapsed: function(subgrid_id, row_id)
                { 
                    row_id = new String(row_id);
                    var grid = $(this).jqGrid();
                    var getChildrenNode = function( row_id )
		            {
                        var result = [];
                        var children = $(grid).find('tr[parent=' + row_id + ']');
                        $(children).each(function (i) {
                            if ($(this).attr('isExpanded') == 'true') {
                                var chl = getChildrenNode(this.id);
                                $(chl).each(function (i) {
                                    result.push(this);
                                });
                            }
                            result.push(this);
                        });
		                return result;                        
                    };
                    var childern = getChildrenNode(row_id);
                    $.each(childern, function (index, value) { $(value).remove(); });
                },");
            script.AppendFormat("subGridOptions:{0}", this._subGridOptions).AppendLine();

            return script.ToString();
        }
        private string NormalizedUrl
        {
            get
            {
                string URL = ((this._pathtype.HasValue && this._pathtype.Value.Equals(PathType.relative) && this._url.IndexOf("http") < 0) ? VirtualPathUtility.ToAbsolute(this._url) : this._url);
                if (!string.IsNullOrEmpty(URL) && URL.EndsWith("/"))
                    URL = URL.Remove(URL.Length - 1, 1);
                return URL;
            }
        }
        private string CustomGridClientSuccessHandler
        {
            get
            {
                return string.Format(@"
                        success: function (data, textStatus) 
                        {{
                            $('#' + subgridId).remove();
                            $('.ui-subgrid').remove();
                            
                            var newTr = $(data);
                            $(newTr).each(function (i) 
                            {{
                                $(this).attr('isExpanded', false);
                                $(this).attr('parent', row_id);
                            }});

                            $($('#{0} tr#' + trId)).attr('isExpanded',true);
                            $($('#{0} tr#' + trId)).after(newTr);
                            
                        }}", this._parentId);
            }
        }
        private string CustomGridJsonClientErrorHandler
        {
            get
            {
                return @"error:function(jqXHR,textStatus,errorThrown)
                    {{
                        mainGrid.trigger('error',{ajaxError:{jqXHR:jqXHR,textStatus:textStatus,errorThrown:errorThrown}});
                    }}";
            }
        }
        private string CustomGridClientErrorHandler
        {
            get
            {
                return string.Format(@"
                    error: function (data, textStatus) 
                    {{
                        if ($('#divErrPopup{0}').html(error))
                        {{
                            $('#divPupFrm').dialog('option', 'title', '{1}');
                            $('#divPupFrm').append('{2}');
                        }}
                        else
                        {{
                            $('#divErrPopup{0}').empty();
                            $('#divErrPopup{0}').append('<p>');
                            $('#divErrPopup{0}').append('{2}');
                            $('#divErrPopup{0}').append(data);
                            $('#divErrPopup{0}').append('</p>');
                            $('#divErrPopup{0}').dialog({{
                                modal: true,
                                title: '{1}'
                            }});
                        }}
                    }}", this._id, this._onLoadError_title, this._onloadError_message);
            }
        }
        public HtmlString Render()
        {
            return new HtmlString(this.ToString());
        }
        #endregion ToString Methods
    }


}
