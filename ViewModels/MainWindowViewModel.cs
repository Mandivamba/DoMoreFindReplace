using CsvHelper;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DoMoreFindReplace.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        private string _ProjectFile;
        private string _FindReplaceFile;
        private int _ProgressValue;
        private string _StatusMessage="Started";

        private string _Disclaimer =    "This software is provided by the copyright holders and contributors \" as is \" and any express or implied warranties, " +
                                        "Including, but not limited to, the implied warranties of merchantability and fitness for a particular purpose are Disclaimed." +
                                        " In no event shall the copyright holder or contributors be liable for any direct, indirect, incidental, Special, exemplary, or" +
                                        " consequential damages (including, but not limited to, procurement of substitute goods or Services; loss of use, data, or profits;" +
                                        " or business interruption) however caused and on any theory of liability, Whether in contract, strict liability, or tort (including" +
                                        " negligence or otherwise) arising in any way out of the use Of this software, even if advised of the possibility of such damage.";

        public string ProjectFile
        {
            get { return _ProjectFile; }
            set { SetProperty(ref _ProjectFile, value); }
        }

        
        public string FindReplaceFile
        {
            get { return _FindReplaceFile; }
            set { SetProperty(ref _FindReplaceFile, value); }
        }

        public string Disclaimer
        {
            get { return _Disclaimer; }
            set { SetProperty(ref _Disclaimer, value); }
        }

        public int ProgressValue
        {
            get { return _ProgressValue; }
            set { SetProperty(ref _ProgressValue, value); }
        }

        public string StatusMessage
        {
            get { return _StatusMessage; }
            set { SetProperty(ref _StatusMessage, value); }
        }

        public DelegateCommand ProjectFileCommand { get; private set; }

        public DelegateCommand FileReplaceCommand { get; private set; }

        public DelegateCommand RunCommand { get; private set; }

        public MainWindowViewModel()
        {
            RegisterCommands();
        }

       

        private void RegisterCommands()
        {
            ///Horible way of doing this,who has time for elegant solutions,this is working just fine, the user won't have a clue
           
            ProjectFileCommand = new DelegateCommand(SelectProjectFile);
            FileReplaceCommand = new DelegateCommand(SelectFileReplaceFile);
            RunCommand = new DelegateCommand(RunApp);

        }

        private void SelectProjectFile()
        {


            var thisapp = System.Windows.Application.Current;
            Dispatcher disp = thisapp.Dispatcher;

            disp.Invoke(() =>
            {

                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    Title = "Select Project Export",
                    InitialDirectory = System.IO.Path.GetTempPath(),
                    IsFolderPicker = false,
                    Multiselect = false
                };

                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }

                ProjectFile = dialog.FileName;

            });

            
        }

        // Multithread if necessary
        private void RunApp()
        {
            List<FindReplace> FindReplaceData;
            var tFile = new FileInfo(ProjectFile);

            var ResultsFile = Path.Combine(Directory.GetParent(tFile.FullName).FullName, Path.GetFileNameWithoutExtension(tFile.FullName)+"FindReplaceResults.txt");
            File.WriteAllText(ResultsFile, File.ReadAllText(tFile.FullName));

            using (var reader = new StreamReader(FindReplaceFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var data = csv.GetRecords<FindReplace>();
                FindReplaceData = data.ToList();
            }

            double count = FindReplaceData.Count();
            double current = 0;
            foreach (var item in FindReplaceData)
            {
                var tempimport = File.ReadAllText(ResultsFile);
                var results = tempimport.SafeReplace(item.Find_This, item.Replace_With, true);
                File.WriteAllText(ResultsFile, results);

                ProgressValue = (int)((current / count) * 100);
                StatusMessage = $"Replaced {item.Find_This} with {item.Replace_With}";
                current++;
            }


        }

        private class FindReplace
        {

            public string Find_This { get; set; }
            public string Replace_With { get; set; }
        }




        private void SelectFileReplaceFile()
        {
            var thisapp = System.Windows.Application.Current;
            Dispatcher disp = thisapp.Dispatcher;

            disp.Invoke(() =>
            {

                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    Title = "Select Find Replace File",
                    InitialDirectory = System.IO.Path.GetTempPath(),
                    IsFolderPicker = false,
                    Multiselect = false
                };

                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }

                FindReplaceFile = dialog.FileName;

            });

        }


    }

    public static class StringExtensions
    {
        public static string SafeReplace(this string input, string find, string replace, bool matchWholeWord)
        {
            string textToFind = matchWholeWord ? string.Format(@"\b{0}\b", find) : find;
            return Regex.Replace(input, textToFind, replace);
        }
    }



}
