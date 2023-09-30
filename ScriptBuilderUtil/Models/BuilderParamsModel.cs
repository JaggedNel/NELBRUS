using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptBuilderUtil.GUI;

namespace ScriptBuilderUtil.Models {

    public class BuilderParamsModel : Notified {

        public enum Compressions {
            Build,
            Light,
            Hard,
            Rebuild,
        }

        public static class CONST {
            public const string RESULT_DIRECTORY_PATH = "Result.txt";
            public const string SCRIPT_BEGINNING_TAG = "//======-SCRIPT BEGINNING-======";
            public const string SCRIPT_ENDING_TAG = "//======-SCRIPT ENDING-======";
            public const string INJECTION_TAG = "#INSERT";
            public const bool INCLUDE_COMMENTS = true;

            public const string CS_FILE_FILTER = "C# files|*.cs|All files|*.*";
            public const string TXT_FILE_FILTER = "Text files|*.txt|All files|*.*";
        }

        #region Properties

        /// <summary>
        /// Common program params
        /// </summary>
        public static BuilderParamsModel Standart {
            get => new BuilderParamsModel() {
                ScriptBeginningTag = CONST.SCRIPT_BEGINNING_TAG,
                ScriptEndingTag = CONST.SCRIPT_ENDING_TAG,
                InjectionTag = CONST.INJECTION_TAG,
                IncludeComments = CONST.INCLUDE_COMMENTS,
                AdditionsCollection = new ObservableCollection<SavedPathModel>(),
            };
        }

        public string FilePath { get; set; }
        // List<string> _additionalInjectionsPath;

        string _rootDirectoryPath;
        public string RootDirectoryPath {
            get => _rootDirectoryPath;
            set {
                _rootDirectoryPath = value;
                OnPropertyChanged("RootDirectoryPath");
            }
        }
        string _scriptBeginningTag;
        public string ScriptBeginningTag {
            get => _scriptBeginningTag;
            set {
                _scriptBeginningTag = value;
                OnPropertyChanged("ScriptBeginningTag");
            }
        }
        string _scriptEndingTag;
        public string ScriptEndingTag {
            get => _scriptEndingTag;
            set {
                _scriptEndingTag = value;
                OnPropertyChanged("ScriptEndingTag");
            }
        }
        string _injectionTag;
        public string InjectionTag {
            get => _injectionTag;
            set {
                _injectionTag = value;
                OnPropertyChanged("InjectionTag");
            }
        }
        bool _addTagsToRes;
        public bool AddTagsToRes {
            get => _addTagsToRes;
            set {
                _addTagsToRes = value;
                OnPropertyChanged("AddTagsToRes");
            }
        }
        int _compression;
        public int Compression {
            get => _compression;
            set {
                _compression = value;
                OnPropertyChanged("Compression");
            }
        }
        bool _includeComments;
        public bool IncludeComments {
            get => _includeComments;
            set {
                _includeComments = value;
                OnPropertyChanged("IncludeComments");
            }
        }
        bool _includeFirstMainComment;
        public bool IncludeFirstMainComment {
            get => _includeFirstMainComment;
            set {
                _includeFirstMainComment = value;
                OnPropertyChanged("IncludeFirstMainComment");
            }
        }
        string _resultText;
        public string ResultText {
            get => _resultText;
            set {
                _resultText = value;
                OnPropertyChanged("ResultText");
            }
        }

        ObservableCollection<SavedPathModel> _additionsCollection;
        public ObservableCollection<SavedPathModel> AdditionsCollection {
            get => _additionsCollection;
            set {
                _additionsCollection = value;
                OnPropertyChanged("AdditionsCollection");
            }
        }

        #endregion Properties

        public void ResetTags() {
            ScriptBeginningTag = CONST.SCRIPT_BEGINNING_TAG;
            ScriptEndingTag = CONST.SCRIPT_ENDING_TAG;
            InjectionTag = CONST.INJECTION_TAG;
        }

        /// <summary> 
        /// This constructor required for deserialization
        /// </summary>
        public BuilderParamsModel() { }
    }
}
