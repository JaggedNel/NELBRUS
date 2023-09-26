using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptBuilderUtil.Models;

namespace ScriptBuilderUtil.Building {
    public static class Builder {

        public static string BuildScript(BuilderParamsModel param, out string error, out string[] errorArgs) {
            StringBuilder result = new StringBuilder();
            List<FileInfo> injections = param.AdditionsCollection
                .Select(m => new FileInfo(m.Path)).ToList();
            errorArgs = null;
            
            // Process root file and injections
            if (!BuildScriptFile(
                new FileInfo(param.RootDirectoryPath), 0, param, injections, out result, out error
                )) {
                return null;
            }

            if (injections.Any()) {
                // Processing additions and return
                List<string> errorList = new List<string>();
                return string.Join("\n\n\n", GetScript(result),
                    string.Join("\n\n\n", injections.Where(i => File.Exists(i.FullName)).Select(i => {
                        StringBuilder res = new StringBuilder();
                        string e;
                        if (BuildScriptFile(i, 0, param, injections, out res, out e)) {
                            return GetScript(res);
                        } else {
                            errorList.Add(e);
                            return null;
                        }
                    }).Where(s => !string.IsNullOrWhiteSpace(s))));
            } else {
                return GetScript(result);
            }
        }

        static string GetScript(StringBuilder script) {
            return script.ToString().Replace("\n", "").Trim('\n', '\t');
        }

        /// <summary>
        /// Build the script from file
        /// </summary>
        /// <param name="info"> Processed file </param>
        /// <param name="tab"> Accumulated tabulation </param>
        /// <param name="result"></param>
        /// <returns> True if build succesful </returns>
        static bool BuildScriptFile(
            FileInfo info, 
            int tab, 
            BuilderParamsModel param,
            List<FileInfo> additions, 
            out StringBuilder result,
            out string error) {
            error = null;
            result = new StringBuilder();
            int lineNumber = 0;
            try {
                using (var sr = new StreamReader(info.FullName)) {
                    int fileTab = 0;
                    string line; // Current string line
                    int index;
                    string temp;

                    // Gathering global indent by beginning tag place
                    while (!sr.EndOfStream) {
                        lineNumber++;
                        if ((line = sr.ReadLine()).IndexOf(param.ScriptBeginningTag) >= 0) {
                            fileTab = GetIndent(line);
                            break;
                        }
                    }

                    // If script beginning not found -> error
                    if (sr.EndOfStream) {
                        error = $"The beggining mark of the script {info.Name} not found";
                        return false;
                    }

                    while (!sr.EndOfStream && !(line = sr.ReadLine()).Contains(param.ScriptEndingTag)) {
                        if (line.Contains("ENDING")) {
                            error = line;
                            return false;
                        }
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) {
                            // Write empty line
                            result.AppendLine("");
                        } else {
                            if ((index = line.IndexOf(param.InjectionTag)) >= 0) {
                                // Injection
                                temp = line.Substring(index + param.InjectionTag.Length + 1).Trim(' '); // File name
                                FileInfo path;
                                // Checking file in the current directory
                                string meaningPath = $@"{info.Directory.FullName}\{temp}.cs";
                                if (!File.Exists(meaningPath)) {
                                    // Checking file in additions directoryes
                                    if ((path = additions.FirstOrDefault(i => i.Name == temp)) != default(FileInfo)) {
                                        if (File.Exists(path.FullName)) {
                                            additions.Remove(path);
                                        } else {
                                            error = $"Directory not exists: {path.FullName}";
                                            return false;
                                        }
                                    } else {
                                        error = $"Additions not contains path: {meaningPath}";
                                        return false;
                                    }
                                } else {
                                    path = new FileInfo(meaningPath);
                                }

                                StringBuilder localRes;
                                if (!BuildScriptFile(path, fileTab + tab, param, additions, out localRes, out error)) {
                                    return false;
                                }
                                result.AppendLine(localRes.ToString());
                            } else {
                                // Regular line
                                if (line.Length > fileTab && string.IsNullOrWhiteSpace(line.Remove(fileTab))) {
                                    // Tabulation is coorect
                                    line = line.Substring(fileTab);
                                    result.AppendLine(line.PadLeft(line.Length + tab));
                                } else {
                                    // Repair and append
                                    result.AppendLine((line = line.TrimStart(' ')).PadLeft(line.Length + tab));
                                }
                            }
                        }
                    }
                    if (sr.EndOfStream) {
                        error = $"Script ending mark in file {info.Name} not found. {lineNumber}";
                        return false;
                    }
                }
            } catch (IOException e) {
                error = $"Inner IOException: {e.Message}\n{e.InnerException}\n{e.StackTrace}";
                return false;
            } catch (Exception e) {
                error = $"Inner Exception (line {lineNumber}): {e.Message}\n{e.InnerException}\n{e.StackTrace}";
                return false;
            }
            return true;
        }

        static int GetIndent(string line) {
            for (int i = 0; i < line.Length; i++) {
                if (line[i] != ' ')
                    return i;
            }
            return -1;
        }
    }
}
