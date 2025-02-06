using SQLite;

namespace PdfForms.Models
{
    public partial class Form
    {
        [PrimaryKey, AutoIncrement]
        public int FormId { get; set; }
        public string FormName { get; set; } = string.Empty;
        public byte[]? FormData { get; set; }
    }
}
