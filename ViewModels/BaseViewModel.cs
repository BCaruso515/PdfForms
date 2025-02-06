
using CommunityToolkit.Mvvm.ComponentModel;
using PdfForms.Data;
using PdfForms.Helpers;
using PdfForms.Models;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace PdfForms.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty] private static DataInterface database = new();
        [ObservableProperty] private static List<FieldType> fieldTypes = [];
        [ObservableProperty] private static List<Form> forms = [];
        [ObservableProperty] private static List<Field> fields = [];

        [ObservableProperty] private static string editSaveButtonText = string.Empty;
        [ObservableProperty] private static string addCancelButtonText = string.Empty;
        [ObservableProperty] private static string editSaveImage = string.Empty;
        [ObservableProperty] private static string addCancelImage = string.Empty;
        [ObservableProperty] private static bool isEnabled;
        [ObservableProperty] private static bool enableDelete;

        protected async Task Initialize()
        {
            await Database.CreateTablesAsync();
            await GetFieldTypesAsync();
        }

        private async Task GetFieldTypesAsync()
        {
            if (! await RefreshFieldTypesAsync())
                await AddFieldTypesAsync();
        }

        protected async Task<bool> RefreshFieldTypesAsync()
        {
            FieldTypes = await Database.RefreshFieldTypeAsync();
            FieldTypes = [..FieldTypes.OrderBy(x=> x.TypeName)];
            return FieldTypes.Count != 0;
        }

        protected async Task<bool> RefreshFormsAsync()
        {
            Forms = await Database.RefreshFormAsync();
            Forms = [.. Forms.OrderBy(x => x.FormName)];
            return Forms.Count != 0;
        }

        protected async Task<bool> RefreshFieldsAsync(int formId)
        {
            Fields = await Database.RefreshFieldAsync();
            Fields = [.. Fields.Where(x => x.FormId == formId).OrderBy(x => x.TabIndex)];
            return Fields.Count != 0;
        }

        private async Task AddFieldTypesAsync()
        {
            await Database.AddOrUpdateFieldTypeAsync(new FieldType { TypeName = "Checkbox Field", FieldTypeId = 1 });
            await Database.AddOrUpdateFieldTypeAsync(new FieldType { TypeName = "Signature Field", FieldTypeId = 2 });
            await Database.AddOrUpdateFieldTypeAsync(new FieldType { TypeName = "Text Field", FieldTypeId = 3 });

            await RefreshFieldTypesAsync();
        }

        protected void SetButtonText(bool editing)
        {
            IsEnabled = editing;
            EnableDelete = !editing;
            if (editing)
            {
                EditSaveButtonText = "Save";
                EditSaveImage = FontAwesomeHelper.FloppyDisk;
                AddCancelButtonText = "Cancel";
                AddCancelImage = FontAwesomeHelper.Xmark;
            }
            else
            {
                EditSaveButtonText = "Edit";
                EditSaveImage = FontAwesomeHelper.Pencil;
                AddCancelButtonText = "Add";
                AddCancelImage = FontAwesomeHelper.Plus;
            }
        }
    }
}
