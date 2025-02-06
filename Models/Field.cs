using iText.Kernel.Geom;
using SQLite;   

namespace PdfForms.Models
{
    public partial class Field
    {
        [PrimaryKey, AutoIncrement]
        public int FieldId { get; set; }
        public float Left { get; set; }
        public float Bottom { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int PageNumber { get; set; }
        public int TabIndex { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? FieldValue { get; set; } = string.Empty;
        public int FieldType { get; set; } //Foreign Key
        public int FormId { get; set; } //Foreign Key
        public Rectangle Rectangle => new(Left * 72, Bottom * 72, Width * 72, Height * 72);
    }
}
