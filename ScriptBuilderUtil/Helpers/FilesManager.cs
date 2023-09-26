using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptBuilderUtil.Helpers {
    public static class FilesManager {

        public static FileInfo ChooseFile(string filter) {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = filter };
            return openFileDialog.ShowDialog() == true ?
                new FileInfo(openFileDialog.FileName) :
                null;
        }

        public static IEnumerable<FileInfo> ChooseFiles(string filter) {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = filter, Multiselect = true };
            return openFileDialog.ShowDialog() == true ?
                openFileDialog.FileNames.Select(n => new FileInfo(n)) :
                null;
        }

        public static FileInfo SaveFile(string filter, string fileName = null) {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = filter, FileName = fileName };
            return saveFileDialog.ShowDialog() == true ?
                new FileInfo(saveFileDialog.FileName) :
                null;
        }

        public static void WriteFile(FileInfo info, bool append, string content) {
            using (StreamWriter sw = new StreamWriter(info.FullName, append, Encoding.Default)) {
                sw.WriteLine(content);
            }
        }

    }
}
