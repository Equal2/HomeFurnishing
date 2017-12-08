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
    public class Column
    {
        #region Properties
        private Align? _align;
        private string _cellAttr;
        private List<string> _classes = new List<string>();
        private string _columnName;
        private string _customFormatter;
        private string _dateFmt;
        private EditType? _editType;
        private bool? _editable;
        private string _editOptions;
        private string _editRules;
        private SortOrder? _firstSortOrder;
        private bool? _fixedWidth;
        private string _formatoptions;
        private KeyValuePair<Formatters, string>? _formatter;
        private bool? _hidden;
        private string _index;
        private bool? _key;
        private string _label;
        private bool? _resizeable;
        private bool? _search;
        private SearchType? _searchType;
        private string[] _searchTerms;
        private string _searchDateFormat;
        private bool _sortable = false;
        private SortType? _sortType;
        private bool? _title;
        private int? _width;
        #endregion Properties

        #region Methods
        public bool IsEditable
        {
            get
            {
                if (this._editable.HasValue)
                    return this._editable.Value;
                else
                    return false;

            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Name of column, cannot be blank or set to 'subgrid', 'cb', and 'rn'</param>
        public Column(string columnName)
        {
            // Make sure columnname is not left blank
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("No columnname specified");
            }

            // Make sure columnname is not part of the reserved names collection
            var reservedNames = new string[] { "subgrid", "cb", "rn" };

            if (reservedNames.Contains(columnName))
            {
                throw new ArgumentException("Columnname '" + columnName + "' is reserved");
            }

            // Set columnname
            this._columnName = columnName;

            // Set index equal to columnname by default, can be overriden by setter
            this._index = columnName;
        }

        /// <summary>
        /// <param>This option allow to add a class to to every cell on that column. In the grid css </param>
        /// <param>there is a predefined class ui-ellipsis which allow to attach ellipsis to a </param>
        /// <param>particular row. Also this will work in FireFox too.</param>
        /// <param>Multiple calls to this function are allowed to set multiple classes</param>
        /// </summary>
        /// <param name="className">Classname</param>
        public Column addClass(string className)
        {
            this._classes.Add(className);
            return this;
        }

        /// <summary>
        /// <param>Defines the alignment of the cell in the Body layer, not in header cell. </param>
        /// <param>Possible values: left, center, right. (default: left)</param>
        /// </summary>
        /// <param name="align">Alignment of column (center, right, left</param>
        public Column setAlign(Align align)
        {
            this._align = align;
            return this;
        }

        /// <summary>
        /// <param>Java script function which allow to set various properties for the cell dynamically</param>
        /// <param>function(rowId, tv, rawObject, cm, rdata)</param>
        /// <param>{</param>
        /// <param>    return ' colspan=2';</param>
        /// <param>}</param>
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public Column setCellAttr(string attr)
        {
            this._cellAttr = attr;
            return this;
        }

        /// <summary>
        /// <param>Sets custom formatter. Usually this is a function. When set in the formatter option </param>
        /// <param>this should not be enclosed in quotes and not entered with () - </param>
        /// <param>just specify the name of the function</param>
        /// <param>The following variables are passed to the function:</param>
        /// <param>'cellvalue': The value to be formated (pure text).</param>
        /// <param>'options': Object { rowId: rid, colModel: cm} where rowId - is the id of the row colModel is </param>
        /// <param>the object of the properties for this column getted from colModel array of jqGrid</param>
        /// <param>'rowobject': Row data represented in the format determined from datatype option. </param>
        /// <param>If we have datatype: xml/xmlstring - the rowObject is xml node,provided according to the rules </param>
        /// <param>from xmlReader If we have datatype: json/jsonstring - the rowObject is array, provided according to </param>
        /// <param>the rules from jsonReader</param>
        /// </summary>
        /// <param name="customFormatter"></param>
        /// <returns></returns>
        public Column setCustomFormatter(string customFormatter)
        {
            if (this._formatter.HasValue)
            {
                throw new Exception("You cannot set a formatter and a customformatter at the same time, please choose one.");
            }
            this._customFormatter = customFormatter;
            return this;
        }

        public Column setDateFmt(string dateFmt)
        {
            this._dateFmt = dateFmt;
            return this;
        }

        public Column setEditable(bool editable)
        {
            this._editable = editable;
            return this;
        }

        public Column setEditType(EditType editType)
        {
            this._editType = editType;
            return this;
        }

        /// <summary>
        /// <param>The option is used only for style select and defines the select options in the search dialogs.</param>
        /// <param>When set for stype select and dataUrl option is not set, the value can be a string or objec</param>
        /// <param>Examples: value:{1:'One',2:'Two'}</param>
        /// </summary>
        /// <param name="editOptions"></param>
        /// <returns></returns>
        public Column setEditOptions(string editOptions)
        {
            this._editOptions = editOptions;
            return this;
        }

        public Column setEditRules(string editRules)
        {
            this._editRules = editRules;
            return this;
        }

        /// <summary>
        /// <param>If set to asc or desc, the column will be sorted in that direction on first </param>
        /// <param>sort.Subsequent sorts of the column will toggle as usual (default: null)</param>
        /// </summary>
        /// <param name="firstSortOrder">First sort order</param>
        public Column setFirstSortOrder(SortOrder firstSortOrder)
        {
            this._firstSortOrder = firstSortOrder;
            return this;
        }

        /// <summary>
        /// <param>If set to true this option does not allow recalculation of the width of the </param>
        /// <param>column if shrinkToFit option is set to true. Also the width does not change </param>
        /// <param>if a setGridWidth method is used to change the grid width. (default: false)</param>
        /// </summary>
        /// <param name="fixedWidth">Indicates if width of column is fixed</param>
        public Column setFixed(bool fixedWidth)
        {
            this._fixedWidth = fixedWidth;
            return this;
        }

        /// <summary>
        /// <para>Sets formatter options</para>
        /// <para>decimalSeparator:'.', thousandsSeparator: ',', decimalPlaces: 2, prefix: '$ ', suffix: ' %'</para>
        /// <para>No braces ({}) needed</para>
        /// </summary>
        /// <param name="formatter">formatoptions</param>
        public Column setFormatOptions(string formatoptions)
        {
            this._formatoptions = formatoptions;
            return this;
        }

        /// <summary>
        /// Sets formatter with default formatoptions (as set in language file)
        /// </summary>
        /// <param name="formatter">Formatter</param>
        public Column setFormatter(Formatters formatter)
        {
            if (!string.IsNullOrWhiteSpace(this._customFormatter))
            {
                throw new Exception("You cannot set a formatter and a customformatter at the same time, please choose one.");
            }
            this._formatter = new KeyValuePair<Formatters, string>(formatter, "");
            return this;
        }

        /// <summary>
        /// Sets formatter with formatoptions (see jqGrid documentation for more info on formatoptions)
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <param name="formatOptions">Formatoptions</param>
        public Column setFormatter(Formatters formatter, string formatOptions)
        {
            if (!string.IsNullOrWhiteSpace(this._customFormatter))
            {
                throw new Exception("You cannot set a formatter and a customformatter at the same time, please choose one.");
            }
            this._formatter = new KeyValuePair<Formatters, string>(formatter, formatOptions);
            return this;
        }

        /// <summary>
        /// Defines if this column is hidden at initialization. (default: false)
        /// </summary>
        /// <param name="hidden">Boolean indicating if column is hidden</param>
        public Column setHidden(bool hidden)
        {
            this._hidden = hidden;
            return this;
        }

        /// <summary>
        /// Set the index name when sorting. Passed as sidx parameter. (default: Same as columnname)
        /// </summary>
        /// <param name="index">Name of index</param>
        public Column setIndex(string index)
        {
            this._index = index;
            return this;
        }

        /// <summary>
        /// <param>In case if there is no id from server, this can be set as as id for the unique row id. </param>
        /// <param>Only one column can have this property. If there are more than one key the grid finds </param>
        /// <param>the first one and the second is ignored. (default: false)</param>
        /// </summary>
        /// <param name="key">Indicates if key is set</param>
        public Column setKey(bool key)
        {
            this._key = key;
            return this;
        }

        /// <summary>
        /// Defines the heading for this column. If empty, the heading for this column comes from the name property.
        /// </summary>
        /// <param name="label">Label name of column</param>
        public Column setLabel(string label)
        {
            this._label = label;
            return this;
        }

        /// <summary>
        /// Defines if the column can be resized (default: true)
        /// </summary>
        /// <param name="resizeable">Indicates if the column is resizable</param>
        public Column setResizeable(bool resizeable)
        {
            this._resizeable = resizeable;
            return this;
        }

        /// <summary>
        /// When used in search modules, disables or enables searching on that column. (default: true)
        /// </summary>
        /// <param name="search">Indicates if searching for this column is enabled</param>
        public Column setSearch(bool search)
        {
            this._search = search;
            return this;
        }

        /// <summary>
        /// Set dateformat of datepicker when searchtype is set to datepicker (default: dd-mm-yy)
        /// </summary>
        /// <param name="searchDateFormat">Dateformat</param>
        public Column setSearchDateFormat(string searchDateFormat)
        {
            this._searchDateFormat = searchDateFormat;
            return this;
        }

        /// <summary>
        /// Set searchterms if search type of this column is set to type select
        /// </summary>
        /// <param name="searchTerms">Searchterm to add to dropdownlist</param>
        public Column setSearchTerms(string[] searchTerms)
        {
            this._searchTerms = searchTerms;
            return this;
        }

        /// <summary>
        /// <param>Sets the searchtype of this column (text, select or datepicker) (default: text)</param>
        /// <param>Note: To use datepicker jQueryUI javascript should be included</param>
        /// </summary>
        /// <param name="searchType">Search type</param>
        public Column setSearchType(SearchType searchType)
        {
            this._searchType = searchType;
            return this;
        }

        /// <summary>
        /// Indicates if column is sortable (default: true)
        /// </summary>
        /// <param name="sortable">Indicates if column is sortable</param>
        public Column setSortable(bool sortable)
        {
            this._sortable = sortable;
            return this;
        }

        /// <summary>
        /// <param>Indicates how column is sorted, default is TEXT</param>
        /// <param>INT - the data is interpreted as integer, </param>
        /// <param>FLOAT - the data is interpreted as decimal number </param>
        /// <param>DATE - the data is interpreted as data </param>
        /// <param>TEXT - the data is interpreted as text</param>
        /// </summary>
        /// <param name="sortType"></param>
        public Column setSortType(SortType sortType)
        {
            this._sortType = sortType;
            return this;
        }

        /// <summary>
        /// If this option is false the title is not displayed in that column when we hover over a cell (default: true)
        /// </summary>
        /// <param name="title">Indicates if title is displayed when hovering over cell</param>
        public Column setTitle(bool title)
        {
            this._title = title;
            return this;
        }

        /// <summary>
        /// Set the initial width of the column, in pixels. This value currently can not be set as percentage (default: 150)
        /// </summary>
        /// <param name="width">Width in pixels</param>
        public Column setWidth(int width)
        {
            this._width = width;
            return this;
        }
        /// <summary>
        /// Creates javascript string from column to be included in grid javascript
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var script = new StringBuilder();

            // Start column
            script.Append("{").AppendLine();

            // Align
            if (this._align.HasValue) script.AppendFormat("align: '{0}',", this._align).AppendLine();

            // Classes
            if (this._classes.Count > 0) script.AppendFormat("classes: '{0}',", string.Join(" ", (from c in _classes select c).ToArray())).AppendLine();

            // Columnname
            script.AppendFormat("name:'{0}',", this._columnName).AppendLine();

            // FirstSortOrder
            if (this._firstSortOrder.HasValue) script.AppendFormat("firstsortorder: '{0}',", this._firstSortOrder).AppendLine();

            // FixedWidth
            if (this._fixedWidth.HasValue) script.AppendFormat("fixed: {0},", this._fixedWidth.Value.ToString().ToLower()).AppendLine();

            // Formatters
            if (this._formatter.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(this._formatter.Value.Value))
                {
                    script.AppendFormat("formatter: '{0}',", this._formatter.Value.Key).AppendLine();
                    if (!string.IsNullOrWhiteSpace(this._formatoptions))
                    {
                        script.AppendFormat("formatoptions: {{{0}}},", this._formatoptions).AppendLine();
                    }
                    else
                    {
                        script.AppendFormat("formatoptions: {{{0}}},", this._formatter.Value.Value).AppendLine();
                    }
                }
                else if (this._formatter.Value.Key == Formatters.date)
                {
                    script.AppendFormat("formatter: '{0}',", this._formatter.Value.Key).AppendLine();
                    if (!string.IsNullOrWhiteSpace(this._formatoptions)) script.AppendFormat("formatoptions: {{{0}}},", _formatoptions).AppendLine();

                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this._formatter.Value.Value))
                        script.AppendFormat("formatter: '{0}',", this._formatter.Value.Key).AppendLine();

                    if (!string.IsNullOrWhiteSpace(this._formatoptions))
                        script.AppendFormat("formatoptions: {{{0}}},", this._formatoptions).AppendLine();
                }
            }

            // Custom formatter
            if (!string.IsNullOrWhiteSpace(this._customFormatter)) script.AppendFormat("formatter: {0},", this._customFormatter).AppendLine();

            // Hidden
            if (this._hidden.HasValue) script.AppendFormat("hidden: {0},", this._hidden.Value.ToString().ToLower()).AppendLine();

            // Key
            if (this._key.HasValue) script.AppendFormat("key: {0},", this._key.Value.ToString().ToLower()).AppendLine();

            // Label
            if (!string.IsNullOrWhiteSpace(this._label)) script.AppendFormat("label: '{0}',", this._label).AppendLine();

            // Resizable
            if (this._resizeable.HasValue) script.AppendFormat("resizable: {0},", this._resizeable.Value.ToString().ToLower()).AppendLine();

            // Search
            if (this._search.HasValue) script.AppendFormat("search: {0},", this._search.Value.ToString().ToLower()).AppendLine();

            // SearchType
            if (this._searchType.HasValue)
            {
                if (this._searchType.Value == SearchType.text) script.AppendLine("stype:'text',");
                if (this._searchType.Value == SearchType.select) script.AppendLine("stype:'select',");

            }

            // Searchoptions
            if (this._searchType == SearchType.select || this._searchType == SearchType.datepicker)
            {
                script.Append("searchoptions: {");

                // Searchtype select
                if (this._searchType == SearchType.select)
                {
                    if (this._searchTerms != null)
                    {
                        string emtpyOption = (this._searchTerms.Count() > 0) ? ":;" : ":";
                        script.AppendFormat("value: \"{0}{1}\"", emtpyOption, string.Join(";", from s in this._searchTerms select s + ":" + s));
                    }
                    else
                    {
                        script.Append("value: ':'");
                    }
                }

                // Searchtype datepicker
                if (this._searchType == SearchType.datepicker)
                {
                    // string fmt = System.Globalization.CultureInfo.CurrentCulture.GetFormat(typeof(DateTime)).ToString();
                    string format = string.IsNullOrWhiteSpace(this._searchDateFormat) ? "mm/dd/yy" : this._searchDateFormat;
                    script.AppendFormat(@"
                        dataInit: function (elem) 
                        {{
                            $(elem).datepicker(
                            {{
                                changeYear: true,
                                changeMonth: true,
                                showButtonPanel: true,
                                dateFormat:'{0}',
                                onSelect: function() 
                                {{
                                    if (this.id.substr(0, 3) === 'gs_') 
                                    {{
                                        // in case of searching toolbar
                                        setTimeout(function()
                                        {{
                                            var grid = $('###gridid##')[0];
                                            grid.triggerToolbar();
                                        }}, 50);
                                    }} 
                                    else 
                                    {{
                                        // refresh the filter in case of
                                        // searching dialog
                                        $(this).trigger('change');
                                    }}
                                }}
                            }});
                        }}", format);
                }
                script.AppendLine("},");
            }

            // Sortable
            script.AppendFormat("sortable: {0},", this._sortable.ToString().ToLower()).AppendLine();

            // SortType
            if (this._sortType.HasValue) script.AppendFormat("sorttype: '{0}',", this._sortType.Value.ToString().ToLower()).AppendLine();

            // Title
            if (this._title.HasValue) script.AppendFormat("title: {0},", this._title.Value.ToString().ToLower()).AppendLine();

            // Width
            if (this._width.HasValue) script.AppendFormat("width:{0},", this._width.Value).AppendLine();

            // Index
            script.AppendFormat("index:'{0}',", this._index).AppendLine();

            // CellAtr
            if (!string.IsNullOrWhiteSpace(this._cellAttr)) script.AppendFormat("cellattr: {0},", this._cellAttr).AppendLine();

            // DateFmt
            if (!string.IsNullOrWhiteSpace(this._dateFmt)) script.AppendFormat("datefmt: '{0}',", this._dateFmt.ToLower()).AppendLine();

            // Editable
            if (this._editable.HasValue) script.AppendFormat("editable: {0},", this._editable.Value.ToString().ToLower()).AppendLine();

            // Edit Type
            if (this._editType.HasValue) script.AppendFormat("edittype: '{0}',", this._editType.ToString().ToLower()).AppendLine();

            // Edit Options
            if (!string.IsNullOrWhiteSpace(this._editOptions)) script.AppendFormat("editoptions: {{{0}}},", this._editOptions).AppendLine();

            // Edit Rules
            if (!string.IsNullOrWhiteSpace(this._editRules)) script.AppendFormat("editrules: {{{0}}},", this._editRules.ToLower()).AppendLine();

            // Remove last comma from string
            if (script.ToString().LastIndexOf(",\r\n") > -1) script = script.Remove(script.Length - 3, 1);

            // End column
            script.Append("}");

            return script.ToString();
        }
        #endregion Methods
    }
}
