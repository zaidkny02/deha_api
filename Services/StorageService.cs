namespace deha_api_exam.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _userContentFolder;
        private const string USER_CONTENT_FOLDER_NAME = "user-content";
        public StorageService(IWebHostEnvironment webHostEnvironment)
        {
            _userContentFolder = Path.Combine("Uploads", USER_CONTENT_FOLDER_NAME);
        }
        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_userContentFolder, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
        public string GetFileUrl(string fileName)
        {
            return $"/{USER_CONTENT_FOLDER_NAME}/{fileName}";
        }

        public async Task SaveFileAsync(Stream mediaBinaryStream, string fileName)
        {
            if (!Directory.Exists(_userContentFolder))
                Directory.CreateDirectory(_userContentFolder);

            var filePath = Path.Combine(_userContentFolder, fileName);
            using var output = new FileStream(filePath, FileMode.Create);
            await mediaBinaryStream.CopyToAsync(output);
        }
    }
}
