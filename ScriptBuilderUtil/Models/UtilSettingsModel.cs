using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptBuilderUtil.GUI;

namespace ScriptBuilderUtil.Models {

    public class UtilSettingsModel : Notified {

        public static class CONST {
            public const string SETTINGS_FILE_PATH = "Settings.xml";
            public const string PARAMS_FILE_PATH = "Params.xml";
        }

        public string ParamsFilePath { get; set; }

        public static UtilSettingsModel Standart {
            get => new UtilSettingsModel() {
                ParamsFilePath = CONST.PARAMS_FILE_PATH,
            };
        }

        /// <summary>
        /// This constructor required for deserialization
        /// </summary>
        public UtilSettingsModel() { }

    }
}
