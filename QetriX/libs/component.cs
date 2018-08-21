namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	 * 16.09.13 | QetriX Component PHP class
	 */
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class Component
	{
		protected DataStore datastore = null;
		protected string _name = "";
		protected string _heading = "";
		protected string _action = "";

		protected List<string> features = new List<string>();

		protected string _style = "";
		protected QPage _page = null;

		public Component()
		{
			this._page = QPage.getInstance();
		}

		public Component(string name)
		{
			this._page = QPage.getInstance();
			this._name = name;
		}

		public Component(string name, string heading)
		{
			this._page = QPage.getInstance();
			this._name = name;
			this._heading = heading;
		}

		/** Get name
		 * @return string
		 */
		public string name()
		{
			return this._name;
		}

		/** Get or set heading
		 * @param null value
		 *
		 * @return this|null
		 */
		public string heading()
		{
			return this._heading;
		}

		public Component heading(string value)
		{
			this._heading = value;
			return this;
		}

		/** Get or set a style (in HTML it's "class" attribute, not "style" attribute!) */
		public string style()
		{
			return this._style;
		}
		public Component style(string value)
		{
			this._style = value;
			return this;
		}

		public string action()
		{
			return this._action;
		}

		public Component action(string value, QModule mod, Dict args, string but)
		{
			this._action = value;
			return this;
		}

		/** Get the page, used often by converters
		 * @return QPage
		 */
		public QPage page()
		{
			return this._page;
		}

		/** Convert the component to something else (HTML, JSON, XML...) */
		public string convert()
		{
			return "";
		}
		public object convert(string toType)
		{
			return convert(QPage.getInstance().outputFormat(), toType, "", new Dict());
		}
		public object convert(string toFormat, string toType)
		{
			return convert(toFormat, toType, "", new Dict());
		}
		public object convert(string fromFormat, string toFormat, string toType, Dict args)
		{
			return Util.convert(this, fromFormat, toFormat, toType, args);
		}

		public string formatValue(string value, ElemType type)
		{
			switch (type) {
				case ElemType.datetime:
				return Util.formatDateTime(value);
				case ElemType.number:
				return Util.formatNumber(value);
				//return date("", strToTime(value));
			}
			return value;
		}

		public Component addFeature(string feature)
		{
			this.features.Add(feature.ToLower());
			return this;
		}

		public Component addFeatures(string[] feature)
		{
			this.features.Concat(feature);
			return this;
		}

		public bool hasFeature(string feature)
		{
			return this.features.Contains(feature.ToLower());;
		}
	}
}
