using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptBuilderUtil.Models;

namespace ScriptBuilderUtil.Building {
    public static class Builder {

        /// <summary>
        /// White space charecters
        /// </summary>
        readonly static char[] WSC = new char[] { ' ', '\t', '\n', '\r', '\0' };

        public static string BuildScript(BuilderParamsModel args, out string error, out string[] errorArgs) {
            StringBuilder result = new StringBuilder();
            List<FileInfo> injections = args.AdditionsCollection
                .Where(m => m.Choosen)
                .Select(m => new FileInfo(m.Path)).ToList();
            error = null;
            errorArgs = null;
            hardLineLength = 0;

            // Process root file and injections
            if (!BuildScriptFile(new FileInfo(args.RootDirectoryPath), 0, args, injections, false, ref result, out error, out errorArgs)) {
                return null;
            }

            // Processing additions
            foreach (var i in injections) {
                if (!File.Exists(i.FullName)) {
                    error = "ErrorBuildAdditionFileNotFound";
                    errorArgs = new string[] { i.FullName };
                    return null;
                }

                result.Append("\n\n");
                if (!BuildScriptFile(i, 0, args, injections, false, ref result, out error, out errorArgs)) {
                    return null;
                }
            }

            string res = result.ToString();
            while (res.Contains("\r\n\r\n\r\n\r\n")) {
                res = res.Replace("\r\n\r\n\r\n\r\n", "\r\n\r\n\r\n");
            }

            return res;
        }

        static int hardLineLength;
        /// <summary>
        /// Build the script from file
        /// </summary>
        /// <param name="info"> Processed file </param>
        /// <param name="tab"> Accumulated tabulation </param>
        /// <param name="result"></param>
        /// <returns> True if build succesful </returns>
        static bool BuildScriptFile(FileInfo info, int tab, BuilderParamsModel args, List<FileInfo> additions, bool isInjection,
            ref StringBuilder result, out string error, out string[] errorArgs) {
            error = null;
            errorArgs = null;
            int lineNumber = -1;
            try {
                using (var sr = new StreamReader(info.FullName)) {
                    int fileTab = 0;
                    string line; // Current string line
                    int index;
                    string temp;
                    StringBuilder locRes = new StringBuilder();
                    lineNumber = 0;

                    // Gathering global indent by beginning tag place
                    while (!sr.EndOfStream) {
                        lineNumber++;
                        if ((line = sr.ReadLine()).IndexOf(args.ScriptBeginningTag) >= 0) {
                            fileTab = GetIndent(line);
                            break;
                        }
                    }

                    // If script beginning not found -> error
                    if (sr.EndOfStream) {
                        error = "ErrorTagBeggining";
                        errorArgs = new string[] { info.FullName };
                        return false;
                    }

                    lineNumber++;

                    bool isBigComment = false;
                    // Check for root comment
                    bool isComment = false;
                    bool isSummaryComment = false;
                    do {
                        line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line) && !(temp = line.Trim(WSC)).StartsWith("#")) {
                            if (args.IncludeComments || !isInjection && args.IncludeFirstMainComment) {
                                if (isSummaryComment) {
                                    if (temp.StartsWith("///")) {
                                        result.AppendLine(temp);
                                    } else {
                                        break;
                                    }
                                } else if (isComment) {
                                    if (temp.StartsWith("//")) {
                                        result.AppendLine(temp);
                                    } else {
                                        break;
                                    }
                                } else if (isBigComment) {
                                    if (temp.EndsWith("*/")) {
                                        line = sr.ReadLine();
                                        break;
                                    }
                                } else {
                                    if (temp.StartsWith("//")) {
                                        if (temp.StartsWith("///")) {
                                            isSummaryComment = true;
                                        } else {
                                            isComment = true;
                                        }
                                        result.AppendLine(temp);
                                    } else if (temp.StartsWith("/*")) {
                                        isBigComment = true;
                                        result.AppendLine(temp);
                                    } else {
                                        break;
                                    }
                                }
                            } else {
                                break;
                            }
                        }
                    } while (!sr.EndOfStream);
                    if (sr.EndOfStream) {
                        error = "ErrorTagEnding";
                        errorArgs = new string[] { info.FullName };
                        return false;
                    }

                    if (args.Compression != (int)BuilderParamsModel.Compressions.Build) {
                        fileTab = 0;
                    }
                    do {
                        lineNumber++;

                        if (line.TrimStart(WSC).StartsWith("#"))
                            continue;

                        if (isBigComment) {
                            if ((index = line.IndexOf("*/")) < 0) {
                                continue;
                            } else {
                                temp = line.Substring(index + 2);
                                line = line.Remove(index = line.Length - line.TrimStart(WSC).Length, line.Length - temp.Length - index);
                                if (string.IsNullOrWhiteSpace(line)) {
                                    continue;
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(line)) {
                            if (args.Compression <= (int)BuilderParamsModel.Compressions.Light) {
                                // Write empty line
                                result.AppendLine("");
                            }
                        } else {
                            // Prepairing line
                            if (args.Compression == (int)BuilderParamsModel.Compressions.Build) {
                                int t = -1;
                                while (line[++t] == '\t') { }
                                if (t > 0) {
                                    line = line.Substring(t).PadLeft(line.Length + t * 4);
                                }
                            } else {
                                line = line.Trim(WSC);
                            }

                            // Processing line
                            if ((index = line.IndexOf(args.InjectionTag)) >= 0) {
                                // Injection
                                temp = line.Substring(index + args.InjectionTag.Length + 1).Trim(' '); // File name
                                FileInfo path;
                                // Checking file in the current directory
                                string meaningPath = $@"{info.Directory.FullName}\{temp}.cs";
                                if (!File.Exists(meaningPath)) {
                                    // Checking file in additions directoryes
                                    path = args.AdditionsCollection
                                        .Select(m => new FileInfo(m.Path))
                                        .FirstOrDefault(i => i.Name == temp);
                                    if (path != default(FileInfo)) {
                                        if (File.Exists(path.FullName)) {
                                            //additions.Remove(path);
                                        } else {
                                            error = "ErrorBuildFileNotExists";
                                            errorArgs = new string[] { lineNumber.ToString("N0"), info.FullName, path.FullName };
                                            return false;
                                        }
                                    } else {
                                        error = "ErrorBuildFileNotFound";
                                        errorArgs = new string[] { lineNumber.ToString("N0"), info.FullName, meaningPath };
                                        return false;
                                    }
                                } else {
                                    path = new FileInfo(meaningPath);
                                }

                                if (!BuildScriptFile(path, fileTab + tab, args, additions, true, ref result, out error, out errorArgs)) {
                                    return false;
                                }
                            } else {
                                // Regular line
                                if (args.Compression == (int)BuilderParamsModel.Compressions.Build) {
                                    // Bake tabulation
                                    int i = 0;
                                    while (i < fileTab && line[i] == ' ')
                                        i++;
                                    line = line.Substring(i);
                                }

                                // Clear comments
                                if (!args.IncludeComments || args.Compression >= (int)BuilderParamsModel.Compressions.Hard) {
                                    while (isBigComment && line.StartsWith("*/") || !isBigComment && line.StartsWith("/*")) {
                                        isBigComment = !isBigComment;
                                        line = line.Substring(2);
                                    }
                                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") && (isBigComment && !line.Contains("*/") || !isBigComment))
                                        continue;
                                    locRes.Clear();
                                    locRes.Append(line[0]);
                                    if (line.Length > 1) {
                                        bool isString = line.StartsWith("\"");
                                        bool isChar = line.StartsWith("'");
                                        for (int i = 1; i < line.Length; i++) {
                                            if (!isBigComment) {
                                                // Determine whether it is a string
                                                if (line[i] == '"' && !isChar &&
                                                    (isString && (i - line.Substring(0, i).TrimEnd('\\').Length % 2 == 0) || !isString)) {
                                                    isString = !isString;
                                                } else if (line[i] == '\'' && !isString &&
                                                    (isChar && line.Length > i + 1 && line[i + 1] == '\'' || !isChar)) {
                                                    isChar = !isChar;
                                                } else
                                                // Check if comment
                                                if (line[i] == '/' && !isString && !isChar && line.Length > i + 1) {
                                                    if (line[i + 1] == '/') {
                                                        break;
                                                    } else if (line[i + 1] == '*') {
                                                        isBigComment = true;
                                                        i++;
                                                        continue;
                                                    }
                                                }
                                                // Append code charrecter
                                                locRes.Append(line[i]);
                                            } else if (line[i] == '*' && line.Length > i + 1 && line[i + 1] == '/') {
                                                isBigComment = false;
                                                i++;
                                            }
                                        }
                                    }
                                    line = locRes.ToString();
                                    if (string.IsNullOrWhiteSpace(line))
                                        continue;
                                    line = line.TrimEnd(WSC);
                                }

                                // TODO Rebuild

                                // Append line
                                if (args.Compression < (int)BuilderParamsModel.Compressions.Hard) {
                                    result.AppendLine(line);
                                } else {
                                    // Hard compressinog
                                    if (string.IsNullOrWhiteSpace(line))
                                        continue;
                                    locRes.Clear();
                                    locRes.Append(line[0]);
                                    if (line.Length == 2) {
                                        locRes.Append(line[1]);
                                    } else if (line.Length > 1) {
                                        bool isString = line.StartsWith("\"");
                                        for (int i = 1; i < line.Length; i++) {
                                            if (line[i] == '"') {
                                                if (line[i - 1] != '\\') {
                                                    isString = !isString;
                                                }
                                                locRes.Append(line[i]);
                                            } else if (isString || line[i] != ' ' || line[i] == ' ' &&
                                                (char.IsLetterOrDigit(line[i - 1]) || line[i - 1] == '_') &&
                                                (char.IsLetterOrDigit(line[i + 1]) || line[i + 1] == '_')) {
                                                locRes.Append(line[i]);
                                            }
                                        }
                                    }
                                    line = locRes.ToString();

                                    // Appending
                                    if (!string.IsNullOrWhiteSpace(line)) {
                                        if (hardLineLength >= 1000) {
                                            hardLineLength = 0;
                                            result.AppendLine(line);
                                        } else {
                                            if (result.Length > 0 && (char.IsLetterOrDigit(result[result.Length - 1]) || result[result.Length - 1] == '_') &&
                                                (char.IsLetterOrDigit(line[0]) || line[0] == '_')) {
                                                result.Append(" " + line);
                                                hardLineLength += line.Length + 1;
                                            } else {
                                                result.Append(line);
                                                hardLineLength += line.Length;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } while (!sr.EndOfStream && !(line = sr.ReadLine()).Contains(args.ScriptEndingTag));
                    if (sr.EndOfStream) {
                        error = "ErrorTagEnding";
                        errorArgs = new string[] { info.FullName };
                        return false;
                    }
                }
            } catch (IOException e) {
                error = "ErrorIOException";
                errorArgs = new string[] { info.FullName, e.Message, e.StackTrace };
                return false;

            } catch (Exception e) {
                error = "ErrorUnhandledOnBuild";
                errorArgs = new string[] { e.Message, lineNumber.ToString("N0"), info.FullName, e.StackTrace };
                return false;
            }
            return true;
        }

        static int GetIndent(string line) {
            line = line.Replace("\t", "    ");
            for (int i = 0; i < line.Length; i++) {
                if (line[i] != ' ')
                    return i;
            }
            return -1;
        }
    }
}
