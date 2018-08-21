namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	* 18.08.05 | QetriX Page Class, platform dependent.
	*/

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Loader;
	using System.Web;

	public sealed class QPage
	{
		private static QPage _instance; // Static reference to current QetriX Page

		private string _appName = ""; // App Name. Recommended 1-16 chars, [a-z][0-9] only. Used in paths, keys, possibly ds conn etc.
		private string _text = ""; // Language specific, appropriate to _name
		private string _authTokenName = "_t";
		private Dictionary<string, DataStore> _dsList = new Dictionary<string, DataStore>(); // HashMap<String, DataStore>
		private QModuleStage _stage;

		private string _outputFormat = "html"; // In what format should the output be rendered (what set of converters it uses)
		private string _lang = "en"; // Current language, English as a default language, incl. internationalization and localization (g11n/i18n) settings: date format, time format, units in general, temperature...
		private List<string> _langs = new List<string>(new string[] { "en" }); // List of available languages
		private Dictionary<string, string> _lbl = new Dictionary<string, string>(); // Array of labels in current language
		private List<string> _messages = new List<string>(); // TODO: List<QMessage>
		private List<string> _log = new List<string>(); // TODO: List<QMessage>
		private Dictionary<string, string> _args; // HashMap<String, String>
		private Dictionary<string, string> _data; // HashMap<String, String>
		private Dictionary<string, string> _formData = null; // HashMap<String, String>; null = no form data
		private Dictionary<string, string> _config = new Dictionary<string, string>{
			{ "appName", ""},
			{ "text", ""},
			{ "siteName", ""},
			{ "uid", ""},
		};
		private Dictionary<string, string> _namespaces = new Dictionary<string, string>();

		private string _path; // Page path, e.g. abc/def/
		private string _pathX; // Base path
		private string _pathBase; // Base path for links, e.g. /qetrix/myapp/
		private string _pathRes; // Resource path, e.g. /qetrix/myapp/res/ (CDN ready)
		private string _pathResCommon; // Common resource path, e.g. /qetrix/common/res/ (CDN ready)
		private string _pathContent; // Content path, e.g.  /qetrix/myapp/content/ (CDN ready)

		private string _pathRoot; // Script root path, e.g. /mnt/data/www/qetrix/
		private string _pathApp; // App root path, e.g. /mnt/data/www/qetrix/apps/myapp/
		private string _pathAppCommon; // App root path, e.g. /mnt/data/www/qetrix/apps/common/
		private string _pathAppContent; // App content path, e.g. /mnt/data/www/qetrix/apps/myapp/content/
		private string _pathAppData; // App data path, e.g. /mnt/data/www/qetrix/apps/myapp/data/
		private string _pathAppVars; // App vars path, e.g. /mnt/data/www/qetrix/vars/myapp/

		private int _isMultiApp = 0; // Uses this QetriX multi-page mode? (looks for /apps/ subdir in QetriX root dir)
		private string defaultModule = ""; // What module should be used as default, when requested module wasn't found
		private List<string> _allowedCookies = new List<string>(new string[] { "_c", "_f", "_l" });

		private string _uid;
		private string _beginTime;
		private string _beginMem;


		public QPage()
		{
			_config.Add("fwk", Assembly.GetEntryAssembly()?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName.Substring(4, 4).ToLower());

			AppDomain.CurrentDomain.AssemblyResolve += autoload_register;
			init();
		}

		/** Returns instance of QPage */
		public static QPage getInstance()
		{
			if (_instance == null) _instance = new QPage();
			return _instance;
		}

		private void init()
		{
			_config["uid"] = getUID();
		}

		private string getUID()
		{
			string str = "";
			var mt_rand = new Random();
			for (var i = 0; i < 6; i++) str += "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(mt_rand.Next(0, 61), 1);
			return str;
		}

		/** PHP Server QetriX root path */
		public string pathRoot()
		{
			return this._pathRoot;
		}

		/** PHP Server page path */
		public string pathApp()
		{
			return this._pathApp;
		}

		/** PHP Server common page path */
		public string pathAppCommon()
		{
			return this._pathAppCommon;
		}

		/** PHP Server data path */
		public string pathAppData()
		{
			return this._pathAppData;
		}

		/** PHP Server content path */
		public string pathAppContent()
		{
			return this._pathAppContent;
		}

		/** PHP Server vars path - containing /backups, /cache, /log, /sessions subdirectoris */
		public string pathAppVars()
		{
			return this._pathAppVars;
		}

		/** HTML Client page path */
		public string path()
		{
			return this._path;
		}
		public QPage path(string value)
		{
			this.set("path", this.pathBase() + value); // TODO
			return this;
		}

		/** HTML Client page base path */
		public string pathBase()
		{
			return this._pathBase;
		}

		/** HTML Client content path */
		public string pathContent()
		{
			return this._pathContent;
		}

		/** HTML Client page resources path */
		public string pathRes()
		{
			return this._pathRes;
		}

		/** HTML Client common resources path */
		public string pathResCommon()
		{
			return this._pathResCommon;
		}

		public string appName()
		{
			return this._appName;
		}

		public QPage appName(string value)
		{
			this._appName = value;
			return this;
		}

		public string text()
		{
			return this._text;
		}

		public string outputFormat()
		{
			return this._outputFormat;
		}

		public QPage outputFormat(string value)
		{
			this._outputFormat = value;
			return this;
		}

		public string lang()
		{
			return this._lang;
		}

		public string authTokenName()
		{
			return this._authTokenName;
		}

		public QModuleStage stage()
		{
			return this._stage;
		}

		/**
		 * @param string name
		 * @param DataStore set
		 *
		 * @return DataStore
		 */
		public DataStore ds()
		{
			return this.ds("ds");
		}

		public DataStore ds(string name)
		{
			return this._dsList.ContainsKey(name) ? this._dsList[name] : null;
		}

		public QPage ds(string name, DataStore set)
		{
			if (this._dsList.ContainsKey(name)) this._dsList[name] = set;
			else this._dsList.Add(name, set);
			return this;
		}

		/** @return bool (true/false). 0 = undefined, 1 = no, 2 = yes */
		public bool isMultiApp() /// Usage in HTTP DS and loadDataStore and loadModule and parsePath
		{
			return false; // Not supported
		}

		/** Read values from request
		 *
		 * @param string key Lower case key for requested value
		 * @param string defval Default value, if not found or empty
		 *
		 * @return string
		 */
		public string get(string key)
		{
			return get(key, "");
		}

		public string get(string key, string defval)
		{
			if (this.has(key)) return this._data[key];
			key = key.ToLower();
			return this.has(key) ? this._data[key] : defval;
		}

		public bool has(string key)
		{
			return this._data.ContainsKey(key);
		}

		public void loadFormData(Dictionary<string, string> data)
		{
			if (this._formData != null) return;
			_formData = data;
			// this.formData = array_merge(array_change_key_case(_POST, CASE_LOWER), array_change_key_case(_FILES, CASE_LOWER));
		}

		/** Load data submitted by QForm
		 *
		 * @param string key
		 *
		 * @return array|string - Map<String, String[]>
		 */
		public Dictionary<string, string> getFormData()
		{
			return this._formData;
		}

		public string getFormData(string key)
		{
			return this._formData.ContainsKey(key) ? this._formData[key] : "";
		}

		/** Check if there are submitted QForm data
		 *
		 * @param string key
		 *
		 * @return bool Return if there are new form data (POST)
		 */
		public bool hasFormData()
		{
			return this._formData.Count > 0;
		}

		public bool hasFormData(string key)
		{
			return this._formData.ContainsKey(key) && this._formData[key] != "";
		}

		/** Write values to response
		 *
		 * @param key
		 * @param value
		 *
		 * @return QPage
		 */
		public QPage set(string key, string value)
		{
			// Set header or cookie
			switch (key) {
				case "path":
				case "redirect": // Deprecated
				case "redir": // Deprecated
				case "location": // Deprecated
				case "goto": // Deprecated
							 //header("Location: ".value);
					break;
				case "status": // HTTP status
							   //header(this.get("server_protocol")." ".value);
					break;
				case "output":
					//echo value;
					break;
				default:

					if (key == this.authTokenName()) {
						//this.cookie(key, value, time() + 3600);
					} else {
						//if (this._allowedCookies.Contains(key)) this.cookie(key, value, time());
						//else header(key.": ".value);
					}
					break;
			}
			return this;
		}


		public Dict parsePath()
		{
			var path = "";
			return parsePath(path);
		}

		public Dict parsePath(string path)
		{
			var aPath = new List<string>(path.Trim(new char[] { '/' }).Split('/'));

			var page = new Dict();
			if (aPath.Count == 0) return page;

			if (_appName == "") {
				_appName = aPath[0];
				aPath.RemoveAt(0);
			}

			page.set("app", _appName);

			if (aPath.Count > 0) {
				page.set(int.TryParse(aPath[0], out int n) ? "id" : "mod", aPath[0]);
				aPath.RemoveAt(0);
			} else {
				page.set("mod", _appName);
			}

			if (aPath.Count > 0) {
				page.set(int.TryParse(aPath[0], out int n) ? "id" : "func", aPath[0]);
				aPath.RemoveAt(0);
			}

			if (aPath.Count > 0) {
				if (int.TryParse(aPath[0], out int n)) {
					page.set("id", aPath[0]);
					aPath.RemoveAt(0);
				} else if (!page.has("func")) {
					page.set("func", aPath[0]);
				}
			}

			if (!page.has("func")) {
				page.set("func", "main");
			}

			return page;
		}


		private Assembly autoload_register(object sender, ResolveEventArgs args)
		{
			var x = sender;

			string dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").TrimEnd('/') + "/";
			string name = args.Name.Split(',')[0] + ".dll";
			foreach (string subdir in new string[] { "libs/", "modules/", "datastores/", "converters/"}) {
				if (!File.Exists(dir + subdir + name)) continue;
				return _config["fwk"] == "fram" ? loadAssemblyNet(dir + subdir + name) : loadAssemblyCore(dir + subdir + name);
			}
			return null;
		}

		private Assembly loadAssemblyCore(string file)
		{
			return AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
		}

		private Assembly loadAssemblyNet(string file)
		{
			return Assembly.LoadFile(file);
		}

		public string loadModule(string path, PathMode pathMode)
		{
			var page = this.parsePath(path);

			if (pathMode != PathMode.direct) {
				string mod = page.get("mod");
				mod = mod.Length > 0 ? mod.Substring(0, 1).ToUpper() + mod.Substring(1) : appName();
				if (mod == "") mod = "qetrix";

				/*var modDir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").TrimEnd('/') + "/";
				// var modFile = modDir + "modules/" + mod + ".dll";
				var modFile = modDir + "modules/" + mod + ".dll";

				if (!File.Exists(modFile)) return "Module not found: " + modFile;
				Assembly myAssembly = null;
				try {
					myAssembly = loadAssemblyCore(modFile);
				} catch {
					myAssembly = loadAssemblyNet(modFile);
				}
				if (myAssembly == null) return "Invalid module " + mod + " in " + modFile;

				Type myType = null;
				myType = myAssembly.GetType("com.qetrix.modules." + mod);
				// if (myType == null) myType = myAssembly.GetType("com.qetrix.modules." + mod);
				if (myType == null) return "Unknown module: " + mod;*/

				var myType = loadAssembly(AssemblyType.module, mod);

				try {
					var myInstance = Activator.CreateInstance(myType, this);

					MethodInfo method = myType.GetMethod(page.get("func"));
					if (method == null) method = myType.GetMethod("main");
					if (method == null) throw new Exception("Function " + page.get("func") + " not found in module " + mod);

					var args = new Dict();
					if (page.has("id")) args.set("id", page.get("id"));
					var xout = method.Invoke(myInstance, new object[] { args });
					return xout.ToString();
				} catch (Exception ex) {
					var exx = ex.InnerException == null ? ex : (ex.InnerException.InnerException == null ? ex.InnerException : ex.InnerException.InnerException);
					return "Exception: " + exx.Message + " at " + exx.Source + "\n" + String.Join("\n", exx.StackTrace);
				}
			}

			return "404 Not Found";
		}

		public string loadModule(string path)
		{
			return loadModule(path, PathMode.regular);
		}

		public string loadModule(string[] args)
		{
			if (args.Length > 0) return loadModule(args[0], PathMode.regular);
			return loadModule("", PathMode.regular);
		}


		/*public string loadModule2(string path, PathMode pathMode)
		{
			var page = this.parsePath(path);

			if (pathMode != PathMode.direct) {
				var mod = page.get("mod");
				mod = mod.Substring(0, 1).ToUpper() + mod.Substring(1);

				var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
				var myAssembly = Assembly.LoadFile(dir + "/modules/" + mod + ".dll");

				Type myType = null;
				myType = myAssembly.GetType("com.qetrix.apps." + _appName + ".modules." + mod);
				if (myType == null) myType = myAssembly.GetType("com.qetrix.apps.shared.modules." + mod);
				var myInstance = Activator.CreateInstance(myType);

				MethodInfo method = myType.GetMethod(page.get("func"));
				if (method == null) method = myType.GetMethod("main");
				if (method == null) throw new Exception("Function " + page.get("func") + " not found in module " + mod);

				var args = new Dict();
				var xout = method.Invoke(myInstance, new object[] { args });
				return xout.ToString();
			}

			return "404 Not Found";
		}*/

		public Type loadAssembly(string type, string name)
		{
			var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").TrimEnd('/') + "/";
			var file = dir + type + "s/" + name + ".dll";

			if (!File.Exists(file)) throw new Exception(type + " not found: " + file);
			//Assembly myAssembly = loadAssembly(file);
			Assembly myAssembly = _config["fwk"] == "fram" ? loadAssemblyNet(file) : loadAssemblyCore(file);

			if (myAssembly == null) throw new Exception("Invalid " + type + " " + name + " in " + file);

			Type myType = null;
			var strType = "com.qetrix." + type + "s." + myAssembly.ManifestModule.ScopeName.Substring(0, myAssembly.ManifestModule.ScopeName.Length - 4);
			myType = myAssembly.GetType(strType);

			if (myType == null) {
				IEnumerable<Type> myTypes;
				try {
					myTypes = myAssembly.GetTypes();
				} catch (ReflectionTypeLoadException e) {
					myTypes = e.Types.Where(t => t != null);
				}
				throw new Exception("Unknown " + type + ": " + name + " in " + strType + ".\n\n" + (myTypes.Count<Type>() > 0 ? "Available types:\n- " + String.Join("\n- ", myTypes) : "No available types, try copy referenced DLLs to app root.") + "\n\n");
			}

			return myType;
		}

		/*public IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			// TODO: Argument validation
			try {
				return assembly.GetTypes();
			} catch (ReflectionTypeLoadException e) {
				return e.Types.Where(t => t != null);
			}
		}*/

		public DataStore loadDataStore(string alias, Type myType, string host, string scope = "", string prefix = "", string username = "", string password = "")
		{
			MethodInfo method = myType.GetMethod("conn");
			if (method == null) throw new Exception("Function conn not found in DataStore " + alias + " / " + myType.ToString());
			var dsInstance = Activator.CreateInstance(myType) as DataStore;
			dsInstance.page(this);
			myType.GetMethod("conn").Invoke(dsInstance, new object[] { host, scope, prefix, username, password });
			if (dsInstance != null) {
				if (this._dsList.ContainsKey(alias)) this._dsList[alias] = dsInstance;
				else this._dsList.Add(alias, dsInstance);
			}
			return dsInstance;
		}
		public DataStore loadDataStore(string alias, string name, string host, string scope = "", string prefix = "", string username = "", string password = "")
		{
			//Type myType = loadAssembly(AssemblyType.datastore, name);
			return loadDataStore(alias, loadAssembly(AssemblyType.datastore, name), host, scope, prefix, username, password);
		}

		public object loadConverter(string fromFormat, string toFormat)
		{
			return loadConverter(fromFormat, toFormat, "");
		}

		public Type loadConverter(string fromFormat, string toFormat, string toType)
		{
			var name = fromFormat + "_" + toFormat + (toType != "" ? "_" + toType : "");
			return loadAssembly(AssemblyType.converter, name);
		}

		public void log(string message)
		{
			_log.Add(message);
		}

		public List<string> log()
		{
			return _log;
		}

		/** Helper function for working with cookies */
		private bool cookie(string name, string value, int expire = 0)
		{
			if (value == "") expire = 1;
			/*if (setcookie(name, value, expire, this.pathBase(), this.has("localhost") ? "" : str_replace("www.", "", this.get("server_name")))) {
				if (expire === 1 || value === "") unset(_COOKIE[name]); /// Del cookie
				return true;
			}*/
			return false;
		}

	}

	public enum QModuleStage
	{
		debug = 0, // Verbose debug info, enable only if something wents really wrong
		dev = 1,   // Basic debug info, stack trace, default for localhost/dev env
		test = 2,  // No debug info, prints warnings and errors. Like production, with DS mockups for sending e-mails, WS push requests etc.
		prod = 3,  // Production, warnings/error messages are logged, not printed.
	}

	public enum PathMode
	{
		direct = 0,
		regular = 1,
		fallbacks = 2
	}

	public sealed class AssemblyType
	{
		public static string converter = "converter";
		public static string module = "module";
		public static string datastore = "datastore";
	}
}
