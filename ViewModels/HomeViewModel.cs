
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PdfForms.Models;

namespace PdfForms.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        [ObservableProperty] private Form _selectedForm = new();
        [ObservableProperty] bool _enableEdit;

        private static readonly string sourceFile = Path.Combine(FileSystem.CacheDirectory, "sourceFile.pdf");
        private int _index = -1;

        public HomeViewModel()
        {
            Task.Run(async () =>
            {
                await Initialize();
            });
        }

        [RelayCommand]
        public async Task Appearing()
        {
            SetButtonText(false);

            if (!await RefreshFormsAsync())
            {
                EnableDelete = EnableEdit = false;
                return;
            }

            SelectedForm = Forms.First();
            EnableDelete = EnableEdit = true;
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
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
                await CancelButtonPressed();
            }
            finally { view.Focus(); }
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
            catch (Exception ex) 
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }
            finally { view.Focus(); }
        }

        private async Task AddButtonPressed()
        {
            SetButtonText(true);
            _index = Forms.IndexOf(SelectedForm);
            SelectedForm = new();
            var result = await SelectPdfFileAsync();
            
            if (result == null)
            {
                await CancelButtonPressed();
                return;
            }

            AddSourceForm(result);
            EnableEdit = true;
        }

        private async Task CancelButtonPressed()
        {
            SetButtonText(false);
            EnableDelete = EnableEdit = await RefreshFormsAsync();

            if (_index > -1)
                SelectedForm = Forms[_index];
        }

        private void EditButtonPressed()
        {
            SetButtonText(true);
            _index = Forms.IndexOf(SelectedForm);
        }

        private async Task SaveButtonPressed()
        {
            VerifyInput();

            if (!await Database.AddOrUpdateFormAsync(SelectedForm))return;
            
            SetButtonText(false);
            EnableDelete = EnableEdit = await RefreshFormsAsync();
            SelectedForm = Forms.Where(x => x.FormId == SelectedForm.FormId).First();
        }

        [RelayCommand]
        public async Task Delete()
        {
            try
            {
                _index = Forms.IndexOf(SelectedForm);

                if (!await Shell.Current.DisplayAlert("WARNING!",
                    "Are you sure you want to proceed? This action can not be undone.",
                    "Yes", "No")) return;

                await Database.DeleteFormAsync(SelectedForm);
                EnableDelete = EnableEdit = await RefreshFormsAsync();

                if (Forms.Count == 0)
                {
                    SelectedForm = new();
                    return;
                }                    

                if (_index > Forms.IndexOf(Forms.Last()))
                    SelectedForm = Forms.Last();
                else
                    SelectedForm = Forms[_index];

            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok"); }

            finally
            { _index = -1; }
        }

        private void VerifyInput()
        {
            if (string.IsNullOrWhiteSpace(SelectedForm.FormName)) throw new Exception("Name can not be blank");

            var result = Forms.Where(x => x.FormName == SelectedForm.FormName).Any();

            if (SelectedForm.FormId == 0 && result)
            {
                throw new Exception($"The form \"{SelectedForm.FormName}\" already exists. " +
                                    $"Form names must be unique.");
            }
        }
        
        public async Task FormEditorPage()
            => await Shell.Current.GoToAsync($"{nameof(FormEditorPage)}?FormId={SelectedForm.FormId}");

        [RelayCommand]
        private async Task OpenSourceForm()
        {
            //Retrieve saved form from database and writes to cache directory
            var forms = await Database.RefreshFormAsync();
            var form = forms.Where(x => x.FormId == SelectedForm.FormId).First();

            if (form.FormData == null) throw new Exception("Stored form has no data");
            await File.WriteAllBytesAsync(sourceFile, form.FormData);
            await FormEditorPage();
        }

        private async Task<FileResult?> SelectPdfFileAsync()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select pdf file *.pdf",
                FileTypes = FilePickerFileType.Pdf
            });
            return result;
        }
        
        private void AddSourceForm(FileResult result)
        {
            var filename = Path.GetFileNameWithoutExtension(result.FileName);
            var extension = Path.GetExtension(result.FileName);

            SelectedForm.FormName = $"{filename}({DateTime.Now}){extension}";
            SelectedForm.FormData = ConvertToByteArray(result.OpenReadAsync().Result);
            OnPropertyChanged(nameof(SelectedForm));
        }

        private static byte[] ConvertToByteArray(Stream stream)
        {
            using MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
