using System;

namespace VorratsUebersicht
{
	public class table_info
	{
        public int RecNo {get; set;}
        public int cid {get; set;}
        public string name {get; set;}

        public string type {get; set;}
        public int notnull {get; set;}
        public object dflt_value {get; set;}
        public bool pk {get; set;}
	}
}