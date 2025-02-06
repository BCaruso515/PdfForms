using PdfForms.Views;

namespace PdfForms
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(FormEditorPage), typeof(FormEditorPage));
            Routing.RegisterRoute(nameof(ViewPage), typeof(ViewPage));
        }
    }
}
