using Path = System.IO.Path;
using PdfForms.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using PdfForms.Models;
using PdfForms.Helpers;

namespace PdfForms.ViewModels
{
    [QueryProperty(nameof(FormId), nameof(FormId))]
    public partial class FormEditorViewModel : BaseViewModel
    {
        [ObservableProperty] private bool _isChecked;
        [ObservableProperty] private bool _isCheckField;
        [ObservableProperty] private bool _isTextField;
        [ObservableProperty] private bool _enableEdit;
        [ObservableProperty] private int _formId;

        private static readonly string outputFile = Path.Combine(FileSystem.CacheDirectory, "outputFile.pdf");
        private static readonly string sourceFile = Path.Combine(FileSystem.CacheDirectory, "sourceFile.pdf");
        //private static readonly string tempFile = Path.Combine(FileSystem.CacheDirectory, "tempFile.pdf");
        private Field _selectedField = new(); 
        private FieldType _selectedType = new();
        private int _index;

        public FieldType SelectedType
        { 
            get => _selectedType;
            set
            { 
                _selectedType = value;
                IsCheckField = _selectedType.FieldTypeId == 1;
                IsTextField = _selectedType.FieldTypeId == 3;
                OnPropertyChanged(nameof(SelectedType));
            }
        }
        
        public Field SelectedField
        {
            get => _selectedField;
            set
            {
                _selectedField = value;
                if (_selectedField.FieldId > 0) 
                {
                    SelectedType = FieldTypes.Where(x=> x.FieldTypeId == _selectedField.FieldType).First();
                    if (IsCheckField)
                        IsChecked = CheckState(_selectedField.FieldValue ?? string.Empty);
                }
                OnPropertyChanged(nameof(SelectedField));
            }
        }

        public async Task Appearing()
        {
            
            if (!await RefreshFieldTypesAsync())
            {
                await Shell.Current.DisplayAlert("Alert!", "Field types must be added to continue...", "Ok");
                return;
            }

            SelectedType = FieldTypes.First();

            if (!await RefreshFieldsAsync(FormId))
            {
                SetButtonText(false);
                EnableDelete = EnableEdit = false;
                SelectedField = new();
                return;
            }
            
            SelectedField = Fields.First();
            EnableDelete = EnableEdit = true;
        }

        [RelayCommand]
        public async Task EditSave(View view)
        {
            try
            {
                if (EditSaveButtonText == "Edit")
                {
                    EditButtonPressed();
                    return;
                }
                await SaveButtonPressed();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
            finally { view.Focus(); }
        }

        private void EditButtonPressed()
        {
            SetButtonText(true);
           _index = Fields.IndexOf(SelectedField);
        }

        private async Task SaveButtonPressed()
        {
            VerifyInput();

            SelectedField.FieldType = SelectedType.FieldTypeId;
            SelectedField.FormId = FormId;
            if (SelectedType.FieldTypeId == 1)
                SelectedField.FieldValue = CheckState(IsChecked);

            if (await Database.AddOrUpdateFieldAsync(SelectedField))
            {
                SetButtonText(false);
                EnableDelete = EnableEdit = await RefreshFieldsAsync(FormId);
                SelectedField = Fields.Where(x => x.FieldId == SelectedField.FieldId).First();
            }
        }

        [RelayCommand]
        public async Task AddCancel(View view)
        {
            try
            {
                if (AddCancelButtonText == "Add")
                {
                    await AddButtonPressed();
                    return;
                }
                await CancelButtonPressed();
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
            finally { view.Focus(); }
        }

        private async Task AddButtonPressed()
        {
            SetButtonText(true);
            _index = Fields.IndexOf(SelectedField);
            SelectedField = await CopyFields(); 
            EnableEdit = true;
        }
        
        private async Task CancelButtonPressed()
        {
            SetButtonText(false);
            EnableDelete = EnableEdit = await RefreshFieldsAsync(FormId);

            if (_index > -1)
                SelectedField = Fields[_index];
            _index = -1;
        }

        [RelayCommand]
        public async Task Export()
        {
            await Share.Default.RequestAsync(new ShareFileRequest("Export", new ShareFile(outputFile)));
        }
         
        
        [RelayCommand]
        public async Task Delete()
        {
            try
            {
                _index = Fields.IndexOf(SelectedField);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteFieldAsync(SelectedField);
                EnableDelete = EnableEdit = await RefreshFieldsAsync(FormId);

                if (Fields.Count == 0)
                {
                    SelectedField = new();
                    return;
                }

                if (_index > Fields.IndexOf(Fields.Last()))
                    SelectedField = Fields.Last();
                else
                    SelectedField = Fields[_index];

            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

//        private static async Task DisplayPdf(string FileName)
//        {
//#if ANDROID || IOS            
//            await Launcher.Default.OpenAsync(new OpenFileRequest("Open document...", new ReadOnlyFile(FileName)));
//#else            
//            //await Launcher.Default.OpenAsync(new OpenFileRequest("Open document...", new ReadOnlyFile(FileName)));
//            await Shell.Current.GoToAsync($"{nameof(ViewPage)}?FileName={FileName}");
//#endif
//        }

        //[RelayCommand]
        //public async Task PreviewField()
        //{
        //    try
        //    {
        //        PdfFieldHelper helper = new(sourceFile, tempfile);

        //        helper.OpenPdfDocument();

        //        if (SelectedType.FieldTypeId == 1)
        //            helper.CreateInteractiveCheckboxField(SelectedField.Rectangle, SelectedField.FieldName, CheckState(IsChecked), SelectedField.PageNumber);
        //        else if (SelectedType.FieldTypeId == 2)
        //            helper.CreateInteractiveSignatureField(SelectedField.Rectangle, SelectedField.FieldName, SelectedField.PageNumber);
        //        else if (SelectedType.FieldTypeId == 3)
        //            helper.CreateInteractiveTextField(SelectedField.Rectangle, SelectedField.FieldName, SelectedField.FieldValue ?? string.Empty, SelectedField.PageNumber);
        //        else return;

        //        if (helper.AcroDocument == null) return;

        //        helper.AcroDocument.Close();

        //        await DisplayPdf(tempfile);
        //    }
        //    catch (Exception ex) { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
        //}

        [RelayCommand]
        public async Task GenerateForm()
        {
            PdfFieldHelper helper = new(sourceFile, outputFile);

            if (!await RefreshFieldsAsync(FormId)) return;

            helper.OpenPdfDocument();

            foreach (var field in Fields)
            {
                if (field.FieldType == 1 && field.Rectangle != null)
                    helper.CreateInteractiveCheckboxField(field.Rectangle, field.FieldName, field.FieldValue ?? string.Empty, field.PageNumber);
                else if (field.FieldType == 2 && field.Rectangle != null)
                    helper.CreateInteractiveSignatureField(field.Rectangle, field.FieldName, field.PageNumber);
                else if (field.FieldType == 3 && field.Rectangle != null)
                    helper.CreateInteractiveTextField(field.Rectangle, field.FieldName, field.FieldValue ?? string.Empty, field.PageNumber);
                else
                    break;
            }

            if (helper.AcroDocument == null) return;
            helper.AcroDocument.Close();

            await Launcher.Default.OpenAsync(new OpenFileRequest("Open document...", new ReadOnlyFile(outputFile)));
        }
        
        private async Task<Field> CopyFields()
        {
            Field field = new()
            {
                Left = SelectedField.Left,
                Bottom = SelectedField.Bottom,
                Width = SelectedField.Width,
                Height = SelectedField.Height,
                PageNumber = SelectedField.PageNumber,
                TabIndex = await Database.NextFieldTabIndexIdAsync(FormId),
                FieldName = SelectedField.FieldName,
                FieldValue = SelectedField.FieldValue,
                FieldType = SelectedField.FieldType,
                FormId = FormId
            };
            return field;
        }

        private void VerifyInput()
        {
            if (string.IsNullOrWhiteSpace(SelectedField.FieldName)) throw new Exception("Field name can not be blank");
            
            var result = Fields.Where(x => x.FieldName == SelectedField.FieldName).Any();

            if (SelectedField.FieldId == 0 && result)
            {
                throw new Exception($"The field \"{SelectedField.FieldName}\" already exists.");
            }
        }

        private static string CheckState(bool isChecked)
        {
            if (isChecked)
                return "Yes";
            else
                return "Off";
        }
        
        private static bool CheckState(string isChecked)
            => isChecked == "Yes";
    }
}



