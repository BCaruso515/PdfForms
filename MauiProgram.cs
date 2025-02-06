using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PdfForms.Views;
using PdfForms.ViewModels;
using PdfForms.Data;

namespace PdfForms
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("brands-regular-400.otf", "BrandsRegular");
                    fonts.AddFont("free-regular-400.otf", "FreeRegular");
                    fonts.AddFont("free-solid-900.otf", "FreeSolid");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<FormEditorPage>();
            builder.Services.AddTransient<FormEditorViewModel>();
            builder.Services.AddSingleton<DataInterface>();
            return builder.Build();
        }
    }
}
