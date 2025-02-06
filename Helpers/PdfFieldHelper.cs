using iText.Forms.Fields.Properties;
using iText.Forms.Fields;
using iText.Forms;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Colors;

namespace PdfForms.Helpers
{
    public class PdfFieldHelper(string SourceFile, string OutputFile)
    {
        private static PdfAcroForm? AcroForm { get; set; }
        public PdfDocument? AcroDocument { get; set; }

        public void OpenPdfDocument()
        {
            AcroDocument = new(new PdfReader(SourceFile), new PdfWriter(OutputFile));
            AcroForm = PdfFormCreator.GetAcroForm(AcroDocument, true);
        }

        public void CreateInteractiveTextField(Rectangle rectangle, string fieldName, string fieldText, int pageNumber)
        {
            if (AcroDocument == null) throw new InvalidOperationException("PDF document is not initialized.");

            // Create a text form field
            PdfTextFormField field = new TextFormFieldBuilder(AcroDocument, fieldName)
                .SetWidgetRectangle(rectangle).CreateText();

            field.GetWidgets()[0].SetHighlightMode(PdfAnnotation.HIGHLIGHT_OUTLINE).SetFlags(PdfAnnotation.PRINT);
            field.SetValue(fieldText);

            if (AcroForm == null) throw new InvalidOperationException("Form is not initialized");
                AcroForm.AddField(field, AcroDocument.GetPage(pageNumber));
        }

        public void CreateInteractiveCheckboxField(Rectangle rectangle, string fieldName, string isChecked, int pageNumber)
        {
           if (AcroDocument == null) throw new InvalidOperationException("PDF document is not initialized.");

            // Create a checkbox form field
            PdfButtonFormField field = new CheckBoxFormFieldBuilder(AcroDocument, fieldName)
                .SetWidgetRectangle(rectangle).CreateCheckBox();

            field.GetWidgets()[0]
                .SetHighlightMode(PdfAnnotation.HIGHLIGHT_OUTLINE)
                .SetFlags(PdfAnnotation.PRINT);
            field.SetCheckType(CheckBoxType.CHECK);
            field.SetValue(isChecked);

            if (AcroForm == null) throw new InvalidOperationException("Form is not initialized");
                AcroForm.AddField(field, AcroDocument.GetPage(pageNumber));
        }

        public void CreateInteractiveSignatureField(Rectangle rectangle, string fieldName, int pageNumber)
        {
            if (AcroDocument == null) throw new InvalidOperationException("PDF document is not initialized.");

            // Create a signature form field
            PdfSignatureFormField field = new SignatureFormFieldBuilder(AcroDocument, fieldName)
                .SetWidgetRectangle(rectangle).CreateSignature();

            PdfFormXObject appearance = new (new Rectangle(rectangle.GetWidth(), rectangle.GetHeight()));
            PdfCanvas canvas = new(appearance, AcroDocument);
            canvas.SetStrokeColor(ColorConstants.BLUE);
            canvas.Rectangle(0, 0, rectangle.GetWidth(), rectangle.GetHeight());
            canvas.Stroke();

            field.GetWidgets()[0].SetAppearance(PdfName.N, appearance.GetPdfObject());

            field.GetWidgets()[0].SetHighlightMode(PdfAnnotation.HIGHLIGHT_OUTLINE).SetFlags(PdfAnnotation.PRINT);

            if (AcroForm == null) throw new InvalidOperationException("Form is not initialized");
                AcroForm.AddField(field, AcroDocument.GetPage(pageNumber));
        }
    }
}
