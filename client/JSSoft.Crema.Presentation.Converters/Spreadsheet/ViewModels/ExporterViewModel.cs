﻿// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Forked from https://github.com/NtreevSoft/Crema
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using JSSoft.Crema.Data;
using JSSoft.Crema.Presentation.Framework;
using JSSoft.Crema.Spreadsheet;
using JSSoft.Library;
using JSSoft.Library.IO;
using JSSoft.ModernUI.Framework;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace JSSoft.Crema.Presentation.Converters.Spreadsheet.ViewModels
{
    [Export(typeof(IExporter))]
    class ExporterViewModel : Screen, IExporter
    {
        private readonly ICremaAppHost cremaAppHost;
        private readonly IAppConfiguration configs;
        private string outputPath;
        private readonly ExporterSettings settings;

        [ImportingConstructor]
        public ExporterViewModel(ICremaAppHost cremaAppHost, IAppConfiguration configs)
        {
            this.cremaAppHost = cremaAppHost;
            this.configs = configs;
            this.settings = new ExporterSettings();
            this.settings.PropertyChanged += Settings_PropertyChanged;
            this.configs.Update(this.settings);
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(CanExport));
        }

        public void Export(string itemPath, CremaDataSet dataSet)
        {
            if (dataSet.Tables.Any() == false)
                return;

            var spreadsheetWriterSettings = new SpreadsheetWriterSettings()
            {
                OmitAttribute = this.settings.OmitAttribute,
                OmitSignatureDate = this.settings.OmitSignatureDate,
            };

            var writer = new SpreadsheetWriter(dataSet, spreadsheetWriterSettings);
            var filename = this.OutputPath;
            if (this.settings.IsSeparable == true)
            {
                var outputPath = itemPath.Trim(PathUtility.SeparatorChar).Replace(PathUtility.SeparatorChar, '.');
                if (outputPath == string.Empty)
                    outputPath = this.cremaAppHost.DataBaseName;
                if (this.settings.IsIncludeDate)
                {
                    //------------------------------------------------------
                    // TableName_2017-02-17_05_11.xlsx
                    string szDate = DateTime.Now.ToString($"{this.settings.OutputDateFormat}");
                    filename = FileUtility.Prepare(this.outputPath, $"{outputPath}_{szDate}.xlsx");
                }
                else
                    filename = FileUtility.Prepare(this.outputPath, outputPath + ".xlsx");
            }

            using (var stream = new FileStream(filename, FileMode.Create))
            {
                writer.Write(stream);
            }
            this.configs.Commit(this.settings);
            this.configs.Commit(this);
        }

        public void SelectPath()
        {
            if (this.settings.IsSeparable == true)
            {
                var dialog = new CommonOpenFileDialog()
                {
                    IsFolderPicker = true,
                };

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.OutputPath = dialog.FileName;
                }
            }
            else
            {
                var dialog = new CommonSaveFileDialog();
                dialog.Filters.Add(new CommonFileDialogFilter("excel file", "*.xlsx"));
                dialog.DefaultExtension = "xlsx";

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.OutputPath = dialog.FileName;
                }
            }
        }

        public string Name => "Excel Exporter";

        [ConfigurationProperty("outputPath")]
        public string OutputPath
        {
            get => this.outputPath;
            set
            {
                this.outputPath = value;
                this.NotifyOfPropertyChange(nameof(this.OutputPath));
                this.NotifyOfPropertyChange(nameof(this.CanExport));
            }
        }

        [ConfigurationProperty("omitSignatureDate")]
        public ExporterSettings Settings => this.settings;

        public bool CanExport
        {
            get
            {
                if (string.IsNullOrEmpty(this.OutputPath) == true)
                    return false;
                if (this.settings.IsSeparable == true)
                    return Directory.Exists(this.OutputPath);
                if (DirectoryUtility.IsDirectory(this.OutputPath) == true)
                    return false;
                return true;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            this.configs.Update(this);
        }
    }
}
