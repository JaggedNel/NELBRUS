using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ScriptBuilderUtil.Helpers;
using ScriptBuilderUtil.Models;

namespace ScriptBuilderUtil.GUI {
    public class ScriptBuilderModel : Notified {

        public static ScriptBuilderModel Instance { get; private set; }
        public static ScriptBuilderModel GetInstance() {
            return Instance ?? (Instance = new ScriptBuilderModel());
        }

        #region Properties

        UtilSettingsModel _utilSettings;
        public UtilSettingsModel UtilSettings {
            get => _utilSettings;
            protected set {
                _utilSettings = value;
                OnPropertyChanged("UtilSettings");
            }
        }
        BuilderParamsModel _builderParams;
        public BuilderParamsModel BuilderParams {
            get => _builderParams;
            protected set {
                _builderParams = value;
                OnPropertyChanged("BuilderParams");
            }
        }

        #endregion Properties

        ScriptBuilderModel() {
            Exception exception;
            UtilSettings = MySerializer.DeserializeObject<UtilSettingsModel>(
                UtilSettingsModel.CONST.SETTINGS_FILE_PATH,
                out exception);
            if (exception != null) {
                UtilSettings = UtilSettingsModel.Standart;
                BuilderParams = BuilderParamsModel.Standart;
            } else {
                BuilderParams = MySerializer.DeserializeObject<BuilderParamsModel>(
                    UtilSettings.ParamsFilePath, out exception);
                if (exception != null) {
                    BuilderParams = BuilderParamsModel.Standart;
                }
            }
        }

        #region Methods

        /// <returns> True if params are fine </returns>
        public bool SaveParams(out string error) {
            Exception exception1, exception2;
            MySerializer.SerializeObject(UtilSettings, UtilSettingsModel.CONST.SETTINGS_FILE_PATH, out exception1);
            MySerializer.SerializeObject(BuilderParams, UtilSettings.ParamsFilePath, out exception2);
            error = string.Join("\n", (new Exception[] { exception1, exception2 }).Where(e => e != null));

            return string.IsNullOrEmpty(error);
        }

        public void ChooseRootFile() {
            FileInfo info = FilesManager.ChooseFile(BuilderParamsModel.CONST.CS_FILE_FILTER);
            if (info != null) {
                BuilderParams.RootDirectoryPath = info.FullName;
            }
        }
        public void AddAdditional() {
            FileInfo[] info = FilesManager.ChooseFiles(BuilderParamsModel.CONST.CS_FILE_FILTER)?.ToArray();
            foreach (var i in info) {
                if (!BuilderParams.AdditionsCollection.Select(x => x.Path).Contains(i.FullName))
                    BuilderParams.AdditionsCollection.Add(new SavedPathModel(i.FullName));
            }
        }
        public void RemAdditional(int removingId) {
            BuilderParams.AdditionsCollection.RemoveAt(removingId);
        }
        public void Run() {

        }

        #endregion Methods
    }
}
