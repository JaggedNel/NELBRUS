using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ScriptBuilderUtil.Helpers;
using ScriptBuilderUtil.Models;
using ScriptBuilderUtil.Building;

namespace ScriptBuilderUtil.GUI {

    public class ViewModel : Notified {

        public static ViewModel Instance { get; private set; }
        public static ViewModel GetInstance(
            TextBox resultTextBox,
            ListBox additionalInjectionsList,
            ComboBox compression,
            Label compressionDescription,
            CheckBox includeCommentsCheck) {
            return Instance ?? (Instance = new ViewModel(
                resultTextBox, 
                additionalInjectionsList, 
                compression, 
                compressionDescription,
                includeCommentsCheck
                ));
        }

        #region Properties

        public ResourceManager RM { get; protected set; }
        public ScriptBuilderModel ScriptBuilder { get; protected set; }

        public readonly TextBox ResultField;
        public readonly ListBox AdditionalInjectionsList;
        public readonly ComboBox CompressionComboBox;
        public readonly Label CompressionDescriptionLabel;
        public readonly CheckBox IncludeCommentsCheck;

        public bool IsScriptTextEmpty;
        public string ScriptText {
            get {
                return ResultField.Text; // TODO чистить текст от \r ?
            }
            set {
                IsScriptTextEmpty = string.IsNullOrWhiteSpace(value);
                ResultField.Text = value;
            }
        }

        #endregion Properties

        ViewModel(
            TextBox resultTextBox,
            ListBox additionalInjectionsList,
            ComboBox compression,
            Label compressionDescription,
            CheckBox includeCommentsCheck) {
            RM = new ResourceManager("ScriptBuilderUtil.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            ScriptBuilder = ScriptBuilderModel.GetInstance();

            ResultField = resultTextBox;
            AdditionalInjectionsList = additionalInjectionsList;
            CompressionComboBox = compression;
            CompressionDescriptionLabel = compressionDescription;
            IncludeCommentsCheck = includeCommentsCheck;
        }

        #region Methods

        public void Close(object sender, CancelEventArgs e) {
            string error;
            if (!ScriptBuilder.SaveParams(out error)) {
                Oink();
                switch(Oink(MessageBoxButton.YesNo, "ErrorSavingCaption", "ErrorSaving", error)) {
                    case MessageBoxResult.Yes:
                        e.Cancel |= true;
                        break;
                }
            }
        }

        public void ChooseRootFile() {
            ScriptBuilder.ChooseRootFile();
        }
        public void AddAdditional() {
            ScriptBuilder.AddAdditional();
        }
        public void RemAdditional() {
            int i;
            while ((i = AdditionalInjectionsList.SelectedIndex) != -1)
                ScriptBuilder.RemAdditional(i);
        }
        public void CompressionChanged() {
            string desc = "CompressinDescription";
            switch (CompressionComboBox.SelectedIndex) {
                case (int)BuilderParamsModel.Compressions.Build:
                    desc += "Build";
                    IncludeCommentsCheck.IsEnabled = true;
                    break;
                case (int)BuilderParamsModel.Compressions.Light:
                    desc += "Light";
                    IncludeCommentsCheck.IsEnabled = true;
                    break;
                case (int)BuilderParamsModel.Compressions.Hard:
                    desc += "Hard";
                    IncludeCommentsCheck.IsEnabled = false;
                    break;
                default:
                    Oink("ErrorUnexpectedComboBoxItem");
                    CompressionComboBox.SelectedIndex = (int)BuilderParamsModel.Compressions.Build;
                    goto case (int)BuilderParamsModel.Compressions.Build;
            }
            CompressionDescriptionLabel.Content = RM.GetString(desc);
        }
        public void CopyResult() {
            if (!IsScriptTextEmpty) {
                Clipboard.SetText(ScriptText);
                Asterisk();
            } else {
                Oink();
            }
        }
        public void SaveResultFile() {
            if (!IsScriptTextEmpty) {
                FileInfo info = FilesManager.SaveFile(BuilderParamsModel.CONST.TXT_FILE_FILTER);
                if (info != null) {
                    FilesManager.WriteFile(info, false, ScriptText);
                }
            } else {
                Oink();
            }
        }
        public void ClearResult() {
            //IsScriptTextEmpty = true;
            ResultField.Clear();
        }
        public void Build() {
            string result;
            string error;
            string[] errorArgs;

            result = Builder.BuildScript(ScriptBuilder.BuilderParams, out error, out errorArgs);
            if (!string.IsNullOrWhiteSpace(result) && string.IsNullOrWhiteSpace(error)) {
                ScriptText = result;
            } else {
                Oink(error, errorArgs);
            }
        }

        public string TryGetResource(string resouceName) {
            string resource = RM.GetString(resouceName);
            return string.IsNullOrWhiteSpace(resource) ? resouceName : resource;
        }
        public void Asterisk() {
            System.Media.SystemSounds.Asterisk.Play();
        }
        public void Oink(string errorMessage = null, params string[] errorArgs) {
            Oink(MessageBoxButton.OK, null, errorMessage, errorArgs);
        }
        public MessageBoxResult Oink(MessageBoxButton type, string caption, string errorMessage, params string[] errorArgs) {
            System.Media.SystemSounds.Hand.Play();
            if (!string.IsNullOrWhiteSpace(errorMessage)) {
                caption = string.IsNullOrWhiteSpace(caption) ? RM.GetString("ErrorLabel") : TryGetResource(caption);
                return MessageBox.Show(string.Format(errorMessage, errorArgs), caption, type);
            }
            return MessageBoxResult.None;
        }

        #endregion Methods

    }
}
