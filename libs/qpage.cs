namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	* 18.09.20 | QetriX Page Class, platform dependent.
	* NuGet requirements: System.Runtime.Loader (4.3.0)
	*/

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Loader;
	using System.Runtime.Serialization;

	public sealed class QPage
	{
		private static QPage _instance; // Static reference to current QetriX Page
		private FileInfo _file;

		private Dictionary<string, string> _sessionData = new Dictionary<string, string>(); // HashMap<String, String>
		private Dictionary<string, DataStore> _dsList = new Dictionary<string, DataStore>(); // HashMap<String, DataStore>
		private QModuleStage _stage = QModuleStage.prod; // Better PROD as default
		private QLogStage _logStage = QLogStage.minor;

		private string _lang = "en"; // Current language, English as a default language, incl. internationalization and localization (g11n/i18n) settings: date format, time format, units in general, temperature...
		private List<string> _langs = new List<string>(new string[] { "en" }); // List of available languages
		private Dictionary<string, string> _lbl = new Dictionary<string, string>(); // Array of labels in current language
		private List<string> _messages = new List<string>(); // TODO: List<QMessage>
		private List<string> _log = new List<string>(); // TODO: List<QMessage>
		private Dictionary<string, string> _args = new Dictionary<string, string>(); // HashMap<String, String>
		private Dictionary<string, string> _data = new Dictionary<string, string> { // HashMap<String, String>
			{ "request_protocol", "" },
			{ "http_host", "" },
		};
		private Dictionary<string, string> _formData = null; // HashMap<String, String>; null = no form data
		private Dictionary<string, string> _config = new Dictionary<string, string>{
			{ "appName", ""}, // App Name. Recommended 1-16 chars, [a-z][0-9] only. Used in paths, keys, possibly ds conn etc.
			{ "text", ""}, // Language specific, appropriate to _name
			{ "siteName", ""},
			{ "outputFormat", "html" }, // In what format should the output be rendered (what set of converters it uses)
			{ "sessionTokenName", "_t"}, // Name of cookie, where session token is stored
			{ "uid", ""},
		};
		private Dictionary<string, string> _namespaces = new Dictionary<string, string>(); // TODO: What is the purpose?? Maybe autoloading?
		private Dictionary<string, string> _headers = new Dictionary<string, string>(); // Like "cookies", but for headers
		private int _statusCode = 200;

		private string _path; // Page path, e.g. abc/def/
		private string _pathX; // Base path
		private string _pathBase; // Base path for links, e.g. /qetrix/myapp/
		private string _pathRes; // Resource path, e.g. /qetrix/myapp/res/ (CDN ready)
		private string _pathResCommon; // Common resource path, e.g. /qetrix/common/res/ (CDN ready)
		private string _pathContent; // Content path, e.g.  /qetrix/myapp/content/ (CDN ready)

		private string _pathRoot; // Script root path, e.g. /var/www/qetrix/
		private string _pathApp; // App root path, e.g. /var/www/qetrix/apps/myapp/
		private string _pathAppCommon; // App root path, e.g. /var/www/qetrix/apps/common/
		private string _pathAppContent; // App content path, e.g. /var/www/qetrix/apps/myapp/content/
		private string _pathAppData; // App data path, e.g. /var/www/qetrix/apps/myapp/data/
		private string _pathAppVars; // App vars path, e.g. /var/www/qetrix/vars/myapp/

		private int _isMultiApp = 0; // Uses this QetriX multi-page mode? (looks for /apps/ subdir in QetriX root dir)
		private string defaultModule = ""; // What module should be used as default, when requested module wasn't found
		private List<string> _allowedCookies = new List<string> { "_c", "_f", "_l" };
		private List<string> _features = new List<string>();

		private string _uid;
		private string _beginTime;
		private string _beginMem;


		public QPage()
		{
			_config.Add("fwk", Assembly.GetEntryAssembly()?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName.Substring(4, 4).ToLowerInvariant());

			AppDomain.CurrentDomain.AssemblyResolve -= autoload_register;
			AppDomain.CurrentDomain.AssemblyResolve += autoload_register;
			init();
		}

		/* Returns instance of QPage */
		public static QPage getInstance(bool init = false)
		{
			if (_instance == null || init) _instance = new QPage();
			return _instance;
		}

		private void init()
		{
			_config["uid"] = getUID();
			_formData = null;
			_allowedCookies.Add(this.sessionTokenName());
		}

		private string getUID()
		{
			string str = "";
			var mt_rand = new Random();
			for (var i = 0; i < 6; i++) str += "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(mt_rand.Next(0, 61), 1);
			return str;
		}

		/* PHP Server QetriX root path */
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

		public QPage path(string value, bool isPermalink = false)
		{
			this.sessionManager(true); // Save the session right away, because exit is imminent.
			this.set("path", this.pathBase() + value); // TODO
			this.statusCode(isPermalink ? 301 : 303);
			_headers.Add("Location", value);
			return this;
		}

		/** HTML Client page base path */
		public string pathBase()
		{
			return this._pathBase;
		}

		public string pathFull(string value = null)
		{
			return string.Concat(this._data["request_protocol"], "://", this._data["http_host"], this.pathBase(), value == null ? this._path : value);
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
			return this._config["appName"];
		}

		public QPage appName(string value)
		{
			this._config["appName"] = value;
			return this;
		}

		public string text()
		{
			return this._config["text"];
		}

		public string outputFormat()
		{
			return this._config["outputFormat"];
		}

		public QPage outputFormat(string value)
		{
			this._config["outputFormat"] = value;
			return this;
		}

		public string lang()
		{
			return this._lang;
		}

		public string sessionTokenName()
		{
			return this._config["sessionTokenName"];
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
			key = key.ToLowerInvariant();
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
			return this._formData != null && this._formData.Count > 0;
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
				case "output":
					//echo value;
					break;
				default:
					if (key == this.sessionTokenName()) {
						this._data["cookie." + key] = value;
						//this.cookie(key, value, time() + 3600);
					} else {
						//if (this._allowedCookies.Contains(key)) this.cookie(key, value, time());
						//else header(key.": ".value);
					}
					break;
			}
			return this;
		}

		public int statusCode()
		{
			return this._statusCode;
		}

		public string statusCode(int code)
		{
			this._statusCode = code;
			switch (code) {
				case 401: return "401 Unauthorized";
				case 403: return "403 Forbidden";
				case 404: return "404 Not Found";
				case 500: return "500 Internal Server Error";
			}
			return code.ToString();
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

			if (this._config["appName"] == "") {
				this._config["appName"] = aPath[0];
				aPath.RemoveAt(0);
			}

			page.set("app", this._config["appName"]);

			if (aPath.Count > 0) {
				page.set(int.TryParse(aPath[0], out int n) ? "id" : "mod", aPath[0]);
				aPath.RemoveAt(0);
			} else {
				page.set("mod", this._config["appName"]);
			}

			if (aPath.Count > 0) {
				page.set(int.TryParse(aPath[0], out int n) ? "id" : "func", aPath[0]);
				aPath.RemoveAt(0);
			}

			if (aPath.Count > 0) {
				if (!page.has("id") && int.TryParse(aPath[0], out int n)) {
					page.set("id", aPath[0]);
					aPath.RemoveAt(0);
				} else if (!page.has("func")) {
					page.set("func", aPath[0]);
				}
			}

			if (!page.has("func")) {
				page.set("func", "main");
			}

			// TODO: Process App Config
			// ...

			this.sessionManager(); // Read session data (Must be AFTER config!)

			if (aPath.Count > 0) {
				this._args = aPath.Select((s, i) => new { s, i }).ToDictionary(x => x.i.ToString(), x => x.s);
				this._args.ToList().ForEach(x => this._data.Add(x.Key, x.Value));
				//this._args = aPath.Distinct().ToDictionary(x => x);
			}

			return page;
		}


		private Assembly autoload_register(object sender, ResolveEventArgs args)
		{
			var x = sender;

			string dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").TrimEnd('/') + "/";
			string name = args.Name.Split(',')[0] + ".dll";
			foreach (string subdir in new string[] { "libs/", "modules/", "datastores/", "converters/" }) {
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

		public void clone(string x)
			{
			}

		public string loadModule(string path, PathMode pathMode)
		{
			var page = this.parsePath(path);

			if (pathMode != PathMode.direct) {

				string mod = page.get("mod"), func = page.get("func");
				mod = mod.Length > 0 ? mod.Substring(0, 1).ToUpper() + mod.Substring(1) : appName();
				if (mod == "") mod = "qetrix";

				// Handle keywords, add trailing underscore; '/list' in URL => function list_(Dict $args)
				var keywords = new string[] { "abstract", "and", "as", "bool", "break", "case", "catch", "class", "const", "continue", "declare", "default", "die", "do", "echo", "else", "elseif", "empty", "enddeclare", "endfor", "endforeach", "endif", "endswitch", "endwhile", "eval", "exit", "extends", "false", "final", "float", "for", "foreach", "function", "global", "goto", "if", "implements", "include", "include_once", "int", "instanceof", "insteadof", "interface", "isset", "list", "mixed", "namespace", "new", "null", "numeric", "or", "print", "private", "protected", "public", "require", "require_once", "resource", "return", "scalar", "static", "string", "switch", "throw", "trait", "true", "try", "unset", "use", "var", "while", "xor" };
				if (keywords.Contains(mod)) mod = mod + "_";
				keywords = keywords.Union(new string[] { "ds", "init", "page", "qmodule", "qpage", "stage" }).ToArray();
				if (keywords.Contains(func)) func = func + "_";

				var myType = loadAssembly(AssemblyType.module, mod);
				string methodName = "";

				var args = new Dict(this._args);
				if (page.has("id")) args.set("id", page.get("id"));

				try {
					var modObject = Activator.CreateInstance(myType, this);
					MethodInfo method;

					method = myType.GetMethod("init");
					method.Invoke(modObject, new object[] { args });

					method = myType.GetMethod(func);
					if (method == null) {
						method = myType.GetMethod("main");
						args.set("0", func); // TODO: Will it be always "0"??
						func = "main";
					}
					if (method == null) throw new Exception(string.Format("Method {0} not found in module {1}", page.get("func"), mod));
					methodName = method.Name;

					var output = method.Invoke(modObject, new object[] { args });
					this.sessionManager(); // Write session data
					return output.ToString();
				} catch (TargetParameterCountException ex) {
					return string.Format("Err#xx: Method \"{0}\" must have following signature: public string {0}(Dict args);", methodName); ;
				} catch (Exception ex) {
					var exx = ex.InnerException == null ? ex : (ex.InnerException.InnerException == null ? ex.InnerException : ex.InnerException.InnerException);
					return "Exception: " + exx.Message + " at " + exx.Source + "\n" + String.Join("\n", exx.StackTrace);
				}
			}

			_statusCode = 404;
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


		public Type loadAssembly(string type, string name)
		{
			var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").TrimEnd('/') + "/";
			var file = string.Concat(dir, type, "s/", name, ".dll");

			if (!File.Exists(file)) throw new Exception(string.Format("{0} not found: {1}", type, file));
			Assembly myAssembly = _config["fwk"] == "fram" ? loadAssemblyNet(file) : loadAssemblyCore(file);

			if (myAssembly == null) throw new Exception(string.Format("Invalid {0} {1} in {2}", type, name, file));

			Type myType = null;
			var strType = string.Concat("com.qetrix.", type, "s.", myAssembly.ManifestModule.ScopeName.Substring(0, myAssembly.ManifestModule.ScopeName.Length - 4));
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


		/** Adds new feature into the page. "Feature" is just an indicator, that certain functionality is available (to avoid more "expensive" checks)
		 *
		 * @param string|array $feature Feature name
		 *
		 * @return QPage
		 */
		public QPage addFeature(string feature)
		{
			this._features.Add(feature.ToLowerInvariant());
			return this;
		}


		/** Checks, if page has given feature.
		 *
		 * @param string $featureName Feature name (e.g. "session")
		 *
		 * @return bool
		 */
		public bool hasFeature(string feature)
		{
			return this._features.Contains(feature.ToLowerInvariant());
		}


		/** Returns array of page's features */
		public List<string> features()
		{
			return _features;
		}


		/** Resources version, added to query-string for mostly CSS and JS files
		 *
		 * @param bool $addQM Add question mark at the beginning of the return string
		 *
		 * @return string
		 */
		public string resVersion(bool addQM = true)
		{
			switch (this._config["resVersion"]) {
				case "day": // Changes every day
					this._config["resVersion"] = DateTime.UtcNow.ToString("yyyyMMdd");
					break;
				case "sec": // Changes every second
					this._config["resVersion"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
					break;
			}

			return string.Concat((addQM ? "?" : ""), this._config["resVersion"]);
		}

		public void log(string message)
		{
			_log.Add(message);
		}

		public void log(string group, string message)
		{
			_log.Add(string.Concat(group, "\t", message));
		}

		public List<string> log()
		{
			return _log;
		}

		/* Helper function for working with cookies */
		private bool cookie(string name, string value, int expire = 0)
		{
			if (value == "") {
				expire = 1;
				//this._allowedCookies.Remove(name); // Good idea???
				this._data["cookie." + name] = null;
			} else {
				if (!this._allowedCookies.Contains(name)) this._allowedCookies.Add(name);
				this.set(name, value);
			}
			/*if (setcookie(name, value, expire, this.pathBase(), this.has("localhost") ? "" : str_replace("www.", "", this.get("server_name")))) {
				if (expire === 1 || value === "") unset(_COOKIE[name]); /// Del cookie
				return true;
			}*/
			return false;
		}

		public QPage args(string key, string value)
		{
			if (!_args.ContainsKey(key)) _args.Add(key, value);
			return this;
		}

		public QPage data(string key, string value)
		{
			if (!_data.ContainsKey(key)) _data.Add(key, value);
			return this;
		}

		public string data(string key)
		{
			return _data.ContainsKey(key) ? _data[key] : "";
		}

		public FileInfo file(byte[] data = null)
		{
			if (data != null) { }
			if (_file == null) _file = new FileInfo("asdf");

			return _file;
			//File.Move()
		}

		public Dictionary<string, string> headers()
		{
			return _headers;
		}

		public bool session()
		{
			return _sessionData.Count() > 0;
		}

		/** Get or set data from/to current session.
		 * session(): bool => is session active?
		 * session(string key): string => return string value for key
		 * session(array data): QPage => merge key-value data into _sessionData
		 * session(string key, null): QPage => removes value with key
		 * session(string key, string value): QPage => set value for key
		 * session(null): string => delete session
		 *
		 * @param string|array $key
		 * @param string|array $value
		 *
		 * @return QPage|string|bool|array
		 * @throws \Exception
		 */
		public string session(string key)
		{
			if (key == null) { // Logout or Delete session
				this._sessionData = null;
				this.log("session", "terminated");
				return "";
			}
			return _sessionData.ContainsKey(key) ? _sessionData[key] : "";
		}
		public QPage session(Dictionary<string, string> data)
		{
			// Maybe is faster Trim('_') instead of StartsWith + EndsWith (on the line below)? (x.Key.Trim('_') == x.Key)
			data.ToList().Where(x => x.Key.StartsWith("_") || x.Key.EndsWith("_")).ToList().ForEach(x => data.Remove(x.Key));  // Remove all keys beginning or ending with an underscore
			if (data.Count > 0) this._sessionData.ToList().ForEach(x => data[x.Key] = x.Value); // Merge arrays
			this._sessionData["_mod"] = "1"; // Mark the session as modified
			return this;
		}
		public QPage session(string key, string value)
		{
			if (value == null) {
				_sessionData.Remove(key);
			} else {
				if (key.Trim('_') != key) throw new Exception("Session key can't start or end with an underscore.");
				_sessionData[key] = value;
			}
			this._sessionData["_mod"] = "1"; // Mark the session as modified
			return this;
		}

		private bool sessionManager(bool regenToken = false)
		{
			if (this.path() == "http-basic-logout") this.path("");

			var token = this.get(this.sessionTokenName()); // Read token value
			var dir = string.Concat(this.pathAppVars(), "sessions", Path.DirectorySeparatorChar);
			var file = string.Concat(dir, token);

			bool emptySessionData;
			if (this._sessionData.ContainsKey("_mod") && this._sessionData.Where(x => !x.Key.StartsWith("_") && !x.Key.EndsWith("_")).ToArray().Count() == 0) {
				emptySessionData = true;
				this._sessionData = null;
			} else emptySessionData = this._sessionData.Count == 0;

			if (this._sessionData != null) { // Initial session read = empty array (not null!)
				if (token == "" && emptySessionData && (!this.has("php_auth_user") || this.get("php_auth_user") == ".") && !this.has("http_authorization")) return true; // No token and no data
				else if (token != "" && emptySessionData) { // Has token, but no data (read session data)
					if (File.Exists(file)) {
						using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) this._sessionData = (Dictionary<string, string>)new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream);
						this.log("session", string.Concat("read: ", token));
						if (this._sessionData.Count > 0 && this._sessionData.ContainsKey("_ua") && this._sessionData["_ua"] == this.get("http_user_agent")) { // If check is OK, return. Else goto end of the function.
							this.addFeature("session");
							return true;
						}
					}
					this.log("session", "file not found");
				} else if (!emptySessionData) { // Has data. For new data, token may not exist yet
					if (regenToken || this.hasFormData() || token == "") { // Create new token, if: forced, HTTP POST or empty.
						var newToken = Util.uuid(true);
						if (this.set(this.sessionTokenName(), newToken) != null) { // If the cookie set isn't happening (e.g. headers already sent), session file won't rename
							if (token != "" && File.Exists(file)) { // Existing session
								File.Move(file, dir + newToken);
							} else { // New session
								if (!Directory.Exists(dir)) Directory.CreateDirectory(dir); // This should be unnecessary...
								new Dictionary<string, string> {
									{ "_created", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") },
									{ "_ua", this.get("http_user_agent") },
									{ "_ip", this.get("remote_addr") },
									{ "_id", newToken },
									// TODO: Load user data to session
									// TODO: Session cleanup (preferably only from that user). Maybe on session delete, using _id?
								}.ToList().ForEach(x => this._sessionData[x.Key] = x.Value);
							}
							token = newToken;
							this._sessionData["_mod"] = "1";
							file = string.Concat(dir, token);
						}
					}
					if (this._sessionData.ContainsKey("_mod")) {
						this._sessionData.Remove("_mod");
						this.log("session", string.Concat("write: ", token));
						this._sessionData["_path"] = this.path(); // TODO previous version >>>, remove this comment if <<< works. | if (this._sessionData.ContainsKey("_path")) this._sessionData["_path"] = this.path(); else this._sessionData.Add("_path", this.path());
						using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, this._sessionData);
					}
					return true;
				} else this.log("session", "error" + (string.IsNullOrEmpty(token) ? ": " + token : ""));
			}

			// Something is wrong, delete session file and session cookie
			//Util::log(debug_backtrace(0), $token);Util::log($this->_sessionData, "delete session");die;
			if (emptySessionData) {
				this.log("session", "delete: " + token);
				this.cookie(this.sessionTokenName(), "");
				if (token != "" && File.Exists(file)) File.Delete(file);
				if (this.has("php_auth_user") || this.has("http_authorization")) this.path(string.Concat("http", (this.has("https") ? "s" : ""), "://.@", this.get("http_host"), this.pathBase(), "http-basic-logout")); // %2E is a dot
			}
			return false;
		}

	}

	/* QModuleStage enum, also used as log level
	 * @link https://www.quiky.net/QetriX/Docs/QModuleStage
	 */
	public enum QModuleStage
	{
		debug = 1, // Verbose debug info, enable only if something wents really wrong.
		dev = 2,   // Basic debug info, stack trace, default for localhost/dev env.
		test = 3,  // No debug info, prints warnings and errors. Like production, with DS mockups for sending e-mails, WS push requests etc. For staging env.
		prod = 4,  // Production, warnings/error messages are logged, not printed.
	}

	public enum QLogStage
	{
		off = 0,
		major = 1,
		minor = 2,
		dev = 3,
		debug = 4,
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
