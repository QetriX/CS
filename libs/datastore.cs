namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	 * 18.07.29 | DataStore Class
	 */

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class DataStore : IDisposable
	{
		private QPage _page;
		protected object _data;

		protected object _conn;
		protected List<string> _features = new List<string>();
		protected string _host = "";
		protected string _username = "";
		protected string _password = "";
		protected string _prefix = "";
		protected string _scope = ""; /// Mostly table or file
		protected List<DataStore> _ccDS; /// Datastore to replicate "set" DS requests

		protected string _dateFormat = ""; /// Date format, which is default for the DS (e.g. YYYY-MM-DD)
		protected string _dateTimeFormat = ""; /// DateTime format, which is default for the DS (e.g. yyyy-mm-ddTHH:mm:ss)
		protected string _stringChar = "'"; /// Character for strings, usually single or double quotes
		protected bool _disposed = false;

		public void page(QPage page)
		{
			if (_page != null) throw new Exception("Error: page is already set in \"" + this.GetType() + "\" DataStore");
			_page = page;
		}

		/// <summary>
		/// Connect to DataStore - database or directory
		/// </summary>
		/// <param name="host">Address of a database server, or path to a data directory, e.g. "localhost"</param>
		/// <param name="scope">Database name, SID, directory or file name, e.g. "qetrix"</param>
		/// <param name="prefix">Table prefix (allowing multiple apps in single database) or subdirectory, e.g. "" (empty string = no prefix)</param>
		/// <param name="username">User Name, Schema Name or subdirectory, e.g. "qetrix"</param>
		/// <param name="password">Password for $username, e.g. "******" :-)</param>
		/// <returns>DataStore object</returns>
		public DataStore conn(string host, string scope, string prefix, string username, string password)
		{
			this._prefix = prefix;
			return this;
		}

		public DataStore activate(string scope)
		{
			this._scope = scope;
			return this;
		}

		public DataStore addFeature(string feature)
		{
			this._features.Add(feature.ToLower());
			return this;
		}

		public DataStore addFeatures(List<string> features)
		{
			this._features = this._features.Concat(features).ToList();
			return this;
		}

		public bool hasFeature(string featureName)
		{
			return this._features.Contains(featureName.ToLower());
		}

		public QPage page()
		{
			return _page;
		}

		public string file(string name)
		{
			throw new NotImplementedException();
		}

		/*public object run(string value, Dict args)
		{
			Type thisType = this.GetType();
			MethodInfo theMethod = thisType.GetMethod(value);
			if (args == null) return theMethod.Invoke(this, null);
			return theMethod.Invoke(this, new object[] { args });
		}*/

		/// <summary>
		/// Destructor for DataStores
		/// @link https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
		/// </summary>
		~DataStore()
		{
			Dispose(false);
		}

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			this._conn = null;
		}
	}
}
