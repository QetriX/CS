namespace com.qetrix.libs
{
	/* Copyright (c) QetriX.com. Licensed under MIT License, see /LICENSE.txt file.
	 * 16.01.09 | QType
	 */

	using System;
	using com.qetrix.libs;

	/// <summary>
	/// Description of QType.
	/// </summary>
	public class QType
	{
		// Class data
		protected Integer t_pk = null; // Primary Key, null = new (not in DS yet)
		protected string tg_fk; // Lang key for name
		protected Integer tt_fk = null; // Parent type
		protected int tmo = 1; // Max occurences in class. -1 = is ent, 127 = unlimited.
		protected int toc = 0; // Order in class
		protected string tds; // DataStore: engine.particletable OR engine.table.column
		protected ValueType tvt = ValueType.anything; // Value type
		protected string tvu = null; // Value unit (SI, if possible)
		protected Integer tvn = null; // Min value (num) / min length (str)
		protected Integer tvx = null; // Max value (num) / max length (str)
		protected Integer tvp = null; // Value precision
		protected string tvv = null; // Value validation
		protected int tvm = 3; // Value mode (R/O, req, unique...)
		protected RelationType trt = RelationType.none; // Relation type
		protected OrderType tot = OrderType.none; // Order type
		protected int tod = 1; // Default order for new particle

		public Integer id()
		{
			return this.t_pk;
		}
		
		public string name()
		{
			return this.tg_fk;
		}
		
		public QType name(string value)
		{
			this.tg_fk = value;
			return this;
		}
		
		public QType valueType(ValueType value)
		{
			this.tvt = value;
			return this;
		}

		public ValueType valueType()
		{
			return this.tvt;
		}

		#region ENUMs
		public enum ValueType {
			none = 0,
			system = 1,
			anythingTN = 2,
			wikiText = 3,
			anythingN = 4,
			htmlText = 5,
			anything = 6,
			number = 7,
			yorn = 9,
			url = 10,
			email = 11,
			geoPoint = 12,
			dateTime = 13,
			date = 14,
			time = 15,
			duration = 16,
			color = 17,
			password = 18
		}

		public enum RelationType {
			none = 0,
			system = 1,
			searchSuggest = 2,
			searchSuggestReq = 3,
			searchSuggestTt = 4,
			searchSuggestTtReq = 5,
			suggest = 6,
			suggestReq = 7,
			suggestTt = 8,
			suggestTtReq = 9,
			table = 10,
			tableReq = 11,
			tableTt = 12,
			tableTtReq = 13
		}

		public enum OrderType {
			none = 0,
			system = 1,
			typeOrder = 2,
			numericOrder = 4,
			dateNoValidation = 10,
			date = 12,
			dateTimeNoValidation = 14,
			dateTime = 16
		}
		#endregion
	}
}
