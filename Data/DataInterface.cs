using SQLite;
using PdfForms.Models;

namespace PdfForms.Data
{
    public class DataInterface
    {
        protected SQLiteAsyncConnection Connection = new(Path.Combine(FileSystem.Current.AppDataDirectory, "FormData.db3"));

        public async Task CreateTablesAsync()
        {
            await Connection.CreateTableAsync<FieldType>();
            await Connection.CreateTableAsync<Form>();
            await CreateFieldAsync();

            await Connection.ExecuteAsync("VACUUM;");
        }

        //Form
        public async Task<List<Form>> RefreshFormAsync()
            => await Connection.Table<Form>().ToListAsync();

        public async Task<bool> AddOrUpdateFormAsync(Form form)
        {
            if (form.FormId == 0)
                return Convert.ToBoolean(await Connection.InsertAsync(form));
            else
                return Convert.ToBoolean(await Connection.UpdateAsync(form));
        }

        public async Task<bool> DeleteFormAsync(Form form)
        {
            var sql = "PRAGMA foreign_keys = ON;";
            await Connection.ExecuteAsync(sql);
            return Convert.ToBoolean(await Connection.DeleteAsync(form));
        }

        //FieldType
        public async Task<List<FieldType>> RefreshFieldTypeAsync()
            => await Connection.Table<FieldType>().ToListAsync();

        public async Task AddOrUpdateFieldTypeAsync(FieldType fieldType)
        {
            if (fieldType.FieldTypeId == 0)
                return;
            await Connection.InsertAsync(fieldType);
        }

        public async Task<bool> DeleteFieldTypeAsync(FieldType fieldType)
        {
            var sql = "PRAGMA foreign_keys = ON;";
            await Connection.ExecuteAsync(sql);
            return Convert.ToBoolean(await Connection.DeleteAsync(fieldType));
        }

        //Field
        private async Task<bool> CreateFieldAsync()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS Field(
                      FieldId integer PRIMARY KEY AUTOINCREMENT,
                      Left float, Bottom float, Width float, Height float,
                      PageNumber integer, TabIndex integer,
                      FieldName text, FieldValue text,
	                  FieldType integer,
	                  FormId integer,
                      FOREIGN KEY (FormId)
                      REFERENCES Form (FormId)
                      ON UPDATE CASCADE
                      ON DELETE CASCADE
                      FOREIGN KEY (FieldType)
                      REFERENCES FieldType (FieldTypeId)
                      ON UPDATE CASCADE
                      ON DELETE CASCADE)";

            return Convert.ToBoolean(await Connection.ExecuteAsync(sql));
        }

        public async Task<List<Field>> RefreshFieldAsync()
            => await Connection.Table<Field>().ToListAsync();

        public async Task<bool> AddOrUpdateFieldAsync(Field field)
        {
            if (field.FieldId == 0)
                return Convert.ToBoolean(await Connection.InsertAsync(field));
            else 
                return Convert.ToBoolean(await Connection.UpdateAsync(field));
        }

        public async Task<bool> DeleteFieldAsync(Field field)
            => Convert.ToBoolean(await Connection.DeleteAsync(field));

        public async Task<int> NextFieldTabIndexIdAsync(int formId)
        {
            return await Connection.ExecuteScalarAsync<int>($"SELECT MAX (TabIndex) FROM Field WHERE Field.FormId = {formId}") + 1;
        }
    }
}
