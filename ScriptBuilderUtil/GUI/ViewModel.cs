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
            Label compressionDescription) {
            return Instance ?? (Instance = new ViewModel(
                resultTextBox, 
                additionalInjectionsList, 
                compression, 
                compressionDescription
                ));
        }

        #region Properties

        public ResourceManager RM { get; protected set; }
        public ScriptBuilderModel ScriptBuilder { get; protected set; }

        public readonly TextBox ResultField;
        public readonly ListBox AdditionalInjectionsList;
        public readonly ComboBox CompressionComboBox;
        public readonly Label CompressionDescriptionLabel;

        #endregion Properties

        ViewModel(
            TextBox resultTextBox,
            ListBox additionalInjectionsList,
            ComboBox compression,
            Label compressionDescription) {
            RM = new ResourceManager("ScriptBuilderUtil.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            ScriptBuilder = ScriptBuilderModel.GetInstance();

            ResultField = resultTextBox;
            AdditionalInjectionsList = additionalInjectionsList;
            CompressionComboBox = compression;
            CompressionDescriptionLabel = compressionDescription;            
        }

        #region Methods

        public void Close(object sender, CancelEventArgs e) {
            // TODO спрашивать хотим ли сохранить параметры если менялись
            string error;
            if (!ScriptBuilder.SaveParams(out error)) {
                switch (MessageBox.Show($"Parameters have been not saved. Do you want stay in application?\n{error}", "Saving error", MessageBoxButton.YesNo)) {
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
            switch (CompressionComboBox.SelectedIndex) {
                case (int)BuilderParamsModel.Compressions.Common:
                    CompressionDescriptionLabel.Content = "TODO 3212354";//RM.GetString("");
                    break;
                default:
                    Oink($"{RM.GetString("ErrorUnexpectedComboBoxItem")}\n#217: Compression combo box.");
                    break;
            }
        }
        public void CopyResult() {
            if (!string.IsNullOrEmpty(ResultField.Text)) {
                Clipboard.SetText(ResultField.Text);
                System.Media.SystemSounds.Asterisk.Play();
            } else {
                Oink();
            }
        }
        public void SaveResultFile() {
            if (!string.IsNullOrEmpty(ResultField.Text)) {
                FileInfo info = FilesManager.SaveFile(BuilderParamsModel.CONST.TXT_FILE_FILTER);
                if (info != null) {
                    FilesManager.WriteFile(info, false, ResultField.Text);
                }
            } else {
                Oink();
            }
        }
        public void ClearResult() {
            ResultField.Clear();
        }
        public void Run() {
            string result;
            string error;
            switch (CompressionComboBox.SelectedIndex) {
                case ((int)BuilderParamsModel.Compressions.Common):
                    if (!string.IsNullOrEmpty(result = Builder.BuildScript(ScriptBuilder.BuilderParams, out error))) {
                        //result = result.Replace("\r", "");
                        ResultField.Text = result;
                        MessageBox.Show($"TODO count: {result.Length} {result.Contains("\r")}");
                    } else {
                        Oink(error);
                    }
                    break;
                case ((int)BuilderParamsModel.Compressions.Maximum):
                    Oink("Maximum not implemented");
                    break;
                default:
                    throw new ArgumentException($"Unexpected selected compression type: {CompressionComboBox.SelectedItem}");
            }
        }

        public static void Oink(string errorMessage = null) {
            System.Media.SystemSounds.Hand.Play();
            if (!string.IsNullOrWhiteSpace(errorMessage)) {
                MessageBox.Show(errorMessage, Instance.RM.GetString("Error"));
            }
        }

        #endregion Methods

    }
}
