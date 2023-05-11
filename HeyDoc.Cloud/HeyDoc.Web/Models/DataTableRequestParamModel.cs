namespace HeyDoc.Web.Models
{
    public struct DataTableOrderOptions
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public struct DataTableColumnSearchOptions
    {
        public string value { get; set; }
        public bool regex { get; set; }
    }

    public struct DataTableColumnProps
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public DataTableColumnSearchOptions search { get; set; }
    }
}