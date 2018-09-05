namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	* 18.08.22 | QetriX Module C# class
	*/
	using components;


	public class QModule
	{
		protected DataStore _ds; // Primary DataStore
		protected DataStore varDS; // Variable files DataStore (logs, temps)
		protected DataStore dataDS; // Data DataStore
		protected DataStore contentDS; // Content DataStore (for PHP always FileSystem)
		public QModuleStage _stage;
		protected QPage _page;
		protected string heading = "";

		/** Class Constructor */
		public QModule(QPage page)
		{
			this._page = page;
			this._ds = this.page().ds();
			this._stage = this.page().stage();
		}

		public string main(Dict args)
		{
			return "It works! Now please add your custom implementation of method " + this.GetType().Name + ".main(Dict args):string.";
		}

		public DataStore ds()
		{
			return this._ds;
		}

		public QPage page()
		{
			return this._page;
		}

		protected object QPage(Dict args, string content, string heading, string style)
		{
			var page = new QView();
			page.heading(heading == "" ? this.heading : heading);
			page.add(content);
			return page.convert("page");
		}

		public QModuleStage stage()
		{
			return this._stage;
		}

		public Dict init(Dict args)
		{
			return args;
		}
	}
}
