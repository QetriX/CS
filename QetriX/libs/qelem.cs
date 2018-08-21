namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	* 17.09.05 | QetriX Type
	*/

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;

	public class QElem
	{
		/// Type properties
		protected string _id = ""; /// Primary Key, null = new (not in DS yet) => dsname
		protected string _ds; /// DataStore: engine.table
		protected string _name; /// Lang key for name
		protected string _desc = ""; /// Description / detail
		protected string _parent = ""; /// Parent type => parent dsname
		protected int tmo = 1; /// Max occurences in class. -1 = is ent, 127 = unlimited.
		protected int _order = 0; /// Order in class
		protected ElemType _valueType = ElemType.text; /// Value type
		protected string _valueUnit = ""; /// Value unit (SI, if possible)
		protected ElemMode _valueMode = ElemMode.hidden; /// Value mode (R/O, req, unique...)
		protected float? _valueMin = null; /// Min value (num) / min length (str)
		protected float? _valueMax = null; /// Max value (num) / max length (str)
		protected float? _valuePrecision = null; /// Value precision
		protected string _valueValidation = ""; /// Value validation
		protected string _valueDefault = ""; /// Default value
		//protected $trt = RelationType::none; /// Relation type
		//protected $trm = RelationMode::hidden; /// Relation mode
		protected int _relationList = 0; /// Relation list (domain - Particle ID or Class ID)
		//protected $tot = OrderType::none; /// Order type
		protected string _tag = ""; /// QetriX Tag for specific purposes (marked as parent, coords, user...)

		/// Value properties
		protected string _action = "";
		protected string _compname = ""; /// Parent component name
		protected List<QItem> _items = new List<QItem>(); /// List<QItem>
		protected string _label = ""; /// Control label or column heading
		protected string _style = "";
		protected string _text = "";
		protected string _value = ""; /// Current value

		public QElem()
		{
		}

		public QElem(int data)
		{
		}

		public QElem(Dictionary<string, string> data)
		{
			foreach (var x in data) {
				PropertyInfo prop = this.GetType().GetProperty(x.Key, BindingFlags.Public | BindingFlags.Instance);
				if (null != prop && prop.CanWrite) prop.SetValue(this, x.Value, null);
			}
		}

		public QElem(QItem data)
		{
			foreach (var x in data.toArray()) {
				PropertyInfo prop = this.GetType().GetProperty(x.Key, BindingFlags.Public | BindingFlags.Instance);
				if (null != prop && prop.CanWrite) prop.SetValue(this, x.Value, null);
			}
		}

		public QElem(ElemType type, string name, string label, string value)
		{
			this._name = name;
			this._valueType = type;
			/*if ((type > 99 && type < 200) || name.Substring(name.Length - 2, 2) == "_r") {
				this._ds = name.Substring(0, name.Length - 2);
				this.value(value);
			}*/
		}

		public QElem(ElemType type, string name, string label, string value, int order, List<Dictionary<string, string>> items)
		{
			this._name = name;
			this._valueType = type;
			/*if ((type > 99 && type < 200) || name.Substring(name.Length - 2, 2) == "_r") {
				this._ds = name.Substring(0, name.Length - 2);
				this.value(value);
			}*/
		}

		public string dsname()
		{
			return this._id;
		}

		public string ds()
		{
			return this._ds;
		}
		public QElem ds(string value)
		{
			this._ds = value;
			return this;
		}

		public string name()
		{
			return this._name;
		}
		public QElem name(string value)
		{
			this._name = value;
			return this;
		}

		public string compname()
		{
			return this._compname;
		}
		public QElem compname(string value)
		{
			this._compname = value;
			return this;
		}

		/** Parent control (defining lowest value or filter for multi-level enums or ID of enum - dual Maximo style) */
		public string parent()
		{
			return this._parent;
		}
		/** Parent control (defining lowest value or filter for multi-level enums or ID of enum - dual Maximo style) */
		public QElem parent(string value)
		{
			this._parent = value;
			return this;
		}

		public string value()
		{
			return this._value;
		}

		public QElem value(string[] value)
		{
			return this.value(String.Join("\t", value));
		}

		public QElem value(List<string> value)
		{
			return this.value(String.Join("\t", value));
		}

		public QElem value(int value)
		{
			return this.value((float)value);
		}

		public QElem value(float value)
		{
			if (this._valueMin.HasValue && value < this._valueMin) throw new ArgumentOutOfRangeException("value", "Value (" + value + ") can't be smaller, than " + this._valueMin);
			if (this._valueMax.HasValue && value > this._valueMax) throw new ArgumentOutOfRangeException("value", "Value (" + value + ") can't be greater, than " + this._valueMin);
			if (this._valuePrecision.HasValue) {
				// TODO
			}
			this._value = value.ToString();
			return this;
		}

		public QElem value(string value)
		{
			value = value.Trim();
			if (value.Length > 0) {
				if (this._valueMin.HasValue && value.Length < this._valueMin) throw new ArgumentOutOfRangeException("value", "Value (" + value + ") must be at lest " + this._valueMin + " chars long");
				if (this._valueMax.HasValue && value.Length > this._valueMax) throw new ArgumentOutOfRangeException("value", "Value (" + value + ") can't be longer, than " + this._valueMin + " chars");
				if (this._valuePrecision.HasValue) {
					// TODO
				}
			}
			this._value = value;
			return this;
		}

		public string defaultValue()
		{
			return this._valueDefault;
		}

		public ElemType type()
		{
			return this._valueType;
		}

		public QElem type(ElemType value)
		{
			this._valueType = value;
			return this;
		}

		public ElemMode mode()
		{
			return this._valueMode;
		}

		public QElem mode(ElemMode value)
		{
			this._valueMode = value;
			return this;
		}

		/// For string value: min length
		/// For numeric value: min value
		/// For date and datetime: earliest date yyyymmdd (incl. entered date)
		/// For time: min seconds
		public float? min()
		{
			return this._valueMin;
		}
		public QElem min(float value)
		{
			this._valueMin = value;
			return this;
		}

		/// For string value: max length
		/// For numeric value: max value
		/// For date and datetime: furthest date yyyymmdd (incl. entered date)
		/// For time: max seconds
		public float? max()
		{
			return this._valueMax;
		}

		public QElem max(float value)
		{
			this._valueMax = value;
			return this;
		}

		/** Value precision
		 * Number: 100=hundreads, 10=tens, 0.1=1/10, 0.01=1/100
		 * DateTime: 1=Century, 2=Decade, 3=Year, 4=Quarter, 5=Month, 6=Week, 7=Day, 8=Hour, 9=Minute, 10=Second
		 * Time (duration): In seconds; 86400 = day, 3600 = hour, 300 = 5 mins, 60 = min, 0.01 = 1/100 sec
		 */
		public float? precision()
		{
			return this._valuePrecision;
		}

		public QElem precision(float value)
		{
			if (value == 0) this._valuePrecision = null;
			else this._valuePrecision = value;
			return this;
		}

		/** Value validation, list or regexp */
		public string validation()
		{
			return this._valueValidation;
		}

		public QElem validation(string value)
		{
			this._valueValidation = value;
			return this;
		}

		public QElem validate()
		{
			throw new NotImplementedException("Validation not yet implemented");
		}

		/** For textbox it's a placeholder */
		public string text()
		{
			//if (value === null) return this.value() == "" ? "" : this._text; // No value = no text
			return this._text; // Because tableColQType.text() should return text
		}

		public QElem text(string value)
		{
			this._text = value;
			return this;
		}

		/** For textbox it's a placeholder */
		public string unit()
		{
			return this._valueUnit;
		}

		public QElem unit(string value)
		{
			this._valueUnit = value;
			return this;
		}

		/** For textbox it's help or value descriptoin */
		public string detail()
		{
			return this._desc;
		}

		public QElem detail(string value)
		{
			this._desc = value;
			return this;
		}

		/** UI interpretation or CSS class(es) in HTML */
		public string style()
		{
			return this._style;
		}

		public QElem style(string value)
		{
			this._style = value;
			return this;
		}

		public int order()
		{
			return this._order;
		}

		public QElem order(int value)
		{
			this._order = value;
			return this;
		}

		public List<QItem> items()
		{
			return this._items;
		}

		public bool hasItems()
		{
			return this._items.Count > 0;
		}

		public string label()
		{
			return this._label;
		}

		public QElem label(string value)
		{
			this._label = value;
			return this;
		}

		public string action()
		{
			return this._action;
		}

		public QElem action(string value)
		{
			this._action = value;
			//if (this.tvt != ValueType::plain && Util::isActionPath(value)) this.label("");
			return this;
		}

		/** Set visibility (idea: just bool to hide the control/column) */
		public bool visible()
		{
			return this.mode() != ElemMode.hidden;
		}


		/** Set visibility (idea: just bool to hide the control/column) */
		public QElem visible(bool value)
		{
			this.mode(value ? ElemMode.hidden : ElemMode.normal); // TODO: normal might replace previous mode setting!!
			return this;
		}

		public string convert(string to, string toType)
		{
			throw new NotImplementedException("Not implemented");
		}
	}



	public enum ElemType
	{
		class_ = 1,
		result = 3, /// Computed
		const_ = 4, /// Value is not copied into object
		enum_ = 6, /// Enum with elems or items

		button = -1,

		boolean = 10, /// 1/0 (as Yes/No) - checkbox
		number = 12, /// Integer or Decimal number (MAIN)

		longtext = 20,
		text = 24, /// Any value (MAIN)

		datetime = 30, /// YYYYMMDDhhmmss (MAIN)
		time = 36, /// duration in decimal seconds (s.sss; output: 1d 16h 31m)

		file = 50,

		relation = 110,
	}

	public enum DateTimePrecision
	{
		century = 1,
		decade = 2,
		year = 3,
		quarterYear = 4,
		month = 5,
		week = 6,
		day = 7,
		hour = 8,
		minute = 9,
		second = 10,
	}

	public enum ElemMode
	{
		hidden = 0, // Input type = hidden
		plain = 1, // Plain text, no control, not editable
		readonly_ = 2, // Not editable
		normal = 4,
		expected = 5, // Marked "+", must be filled to progress into next stage (like "validated")
		required = 6, // Marked "*", must be filled to process (typically save) the form
		unique = 8, // Unique value in the class it belongs to, it also is required (empty = not unique)
	}
}
