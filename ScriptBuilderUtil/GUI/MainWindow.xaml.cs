﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScriptBuilderUtil.GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();

            DataContext = ViewModel.GetInstance(
                ResultViewer,
                AdditionalInjectionsList,
                CompressionList,
                CompressionDescriptionField,
                IncludeCommentsLabelCheck,
                ResultLengthLabel
                );
        }

        #region Events

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            ViewModel.Instance.Close(sender, e);
        }
        private void BuildButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.Build();
        }
        private void RootDirectorySelection_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.ChooseRootFile();
        }
        private void AddAdditionalInjectionButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.AddAdditional();
        }
        private void RemAdditionalInjectionButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.RemAdditional();
        }
        private void CompressionList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ViewModel.Instance.CompressionChanged();
        }
        private void CopyButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.CopyResult();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.SaveResultFile();
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.ClearResult();
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.Instance.ResetTags();
        }
        private void IncludeCommentsLabelCheck_Checked(object sender, RoutedEventArgs e) {
            IncludeFirstMainCommentCheck.IsEnabled = false;
            IncludeFirstMainCommentCheck.IsChecked = true;
        }
        private void IncludeCommentsLabelCheck_Unchecked(object sender, RoutedEventArgs e) {
            IncludeFirstMainCommentCheck.IsEnabled = true;
        }

        #endregion Events

    }
}
