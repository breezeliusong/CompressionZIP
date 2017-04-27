using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CompressionZIP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ZipHelper("hello.zip");
        }

        public async Task<string> ZipHelper(string zipFileName)
        {
            try
            {
                string appFolderPath = ApplicationData.Current.LocalFolder.Path;
                StorageFolder destnFolder = await StorageFolder.GetFolderFromPathAsync(appFolderPath + "\\destn");
                StorageFolder sourceFolder = await StorageFolder.GetFolderFromPathAsync(appFolderPath + "\\src");

                StorageFile zipFile = await destnFolder.CreateFileAsync(zipFileName, CreationCollisionOption.ReplaceExisting);
                Stream zipToCreate = await zipFile.OpenStreamForWriteAsync();
                ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Update);

                await ZipFolderContents(sourceFolder, archive, sourceFolder.Path);
                archive.Dispose();
                return "success";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("failed");
                return "fail";
            }
        }

        public async Task ZipFolderContents(StorageFolder sourceFolder, ZipArchive archive, string baseDirPath)
        {
            IReadOnlyList<StorageFile> files = await sourceFolder.GetFilesAsync();

            foreach (StorageFile file in files)
            {
                ZipArchiveEntry readmeEntry = archive.CreateEntry(file.Path.Remove(0, baseDirPath.Length));
                IBuffer FileBuffer = await FileIO.ReadBufferAsync(file);
                if (FileBuffer.Length != 0)
                {
                    //byte[] buffer=FileBuffer.ToArray();
                    byte[] buffer = WindowsRuntimeBufferExtensions.ToArray(FileBuffer);
                    using (Stream entryStream = readmeEntry.Open())
                    {
                        await entryStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }

            IReadOnlyList<StorageFolder> subFolders = await sourceFolder.GetFoldersAsync();

            if (subFolders.Count() == 0) return;

            foreach (StorageFolder subfolder in subFolders)
                await ZipFolderContents(subfolder, archive, baseDirPath);
        }

        private async void De_Button_Click(object sender, RoutedEventArgs e)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            //var file = await localFolder.GetFileAsync("file.json");
            //await file.DeleteAsync();
            var folder = await localFolder.GetFolderAsync("destn");
            var archive = await folder.GetFileAsync("hello.zip");
            await archive.RenameAsync("second.zip");
            var zipFolder = await folder.CreateFolderAsync("zip", CreationCollisionOption.ReplaceExisting);
            ZipFile.ExtractToDirectory(archive.Path, archive.Path);

        }
    }
}
