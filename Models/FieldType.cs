using SQLite;

namespace PdfForms.Models
{
    public partial class FieldType
    {
        [PrimaryKey]
        public int FieldTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }
}
