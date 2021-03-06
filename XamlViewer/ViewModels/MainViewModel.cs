﻿using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System.ComponentModel;
using System.Windows;
using Utils.IO;
using XamlViewer.Models;
using XamlService.Commands;

namespace XamlViewer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private AppData _appData = null;

        public DelegateCommand ExpandOrCollapseCommand { get; private set; }
        public DelegateCommand ActivatedCommand { get; private set; }
        public DelegateCommand<CancelEventArgs> ClosingCommand { get; private set; }

        public DelegateCommand SwapCommand { get; private set; }
        public DelegateCommand HorSplitCommand { get; private set; }
        public DelegateCommand VerSplitCommand { get; private set; }

        public MainViewModel(IContainerExtension container, IApplicationCommands appCommands)
        {
            _appData = container.Resolve<AppData>();
            AppCommands = appCommands;

            InitCommand();
        }

        #region Init

        private void InitCommand()
        {
            ExpandOrCollapseCommand = new DelegateCommand(ExpandOrCollapse);
            ActivatedCommand = new DelegateCommand(Activated);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing);

            SwapCommand = new DelegateCommand(Swap);
            HorSplitCommand = new DelegateCommand(HorSplit);
            VerSplitCommand = new DelegateCommand(VerSplit);
        }

        #endregion

        #region Command

        private void ExpandOrCollapse()
        {
            ExpandOrCollapse(_isExpandSetting);
        }

        private void Activated()
        {
            if (_appCommands == null)
                return;

            _appCommands.RefreshCommand.Execute(null);
        }

        private void Closing(CancelEventArgs e)
        {
            if (_appData.CollectExistedFileAction != null)
                _appData.CollectExistedFileAction();

            FileHelper.SaveToJsonFile(ResourcesMap.LocationDic[Location.GlobalConfigFile], _appData.Config);
        }

        private void Swap()
        {
            if (DesignerRow == 0)
            {
                DesignerRow = 2;
                EditorRow = 0;
            }
            else
            {
                DesignerRow = 0;
                EditorRow = 2;
            }
        }

        private void HorSplit()
        {
            GridAngle = 0d;
            PaneAngle = 0d;
            HorSplitAngle = 0d;
            VerSplitAngle = 90d;
            CursorSource = @"./Assets/Cursors/Splitter_ud.cur";
        }

        private void VerSplit()
        {
            GridAngle = -90d;
            PaneAngle = 90d;
            HorSplitAngle = 90d;
            VerSplitAngle = 0d;
            CursorSource = @"./Assets/Cursors/Splitter_lr.cur";
        }

        #endregion

        private IApplicationCommands _appCommands;
        public IApplicationCommands AppCommands
        {
            get { return _appCommands; }
            set { SetProperty(ref _appCommands, value); }
        }

        private string _title = "XAML Viewer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private bool _isExpandSetting;
        public bool IsExpandSetting
        {
            get { return _isExpandSetting; }
            set { SetProperty(ref _isExpandSetting, value); }
        }

        private GridLength _settingRowHeight = new GridLength(0);
        public GridLength SettingRowHeight
        {
            get { return _settingRowHeight; }
            set { SetProperty(ref _settingRowHeight, value); }
        }

        private int _designerRow = 0;
        public int DesignerRow
        {
            get { return _designerRow; }
            set { SetProperty(ref _designerRow, value); }
        }

        private int _editorRow = 2;
        public int EditorRow
        {
            get { return _editorRow; }
            set { SetProperty(ref _editorRow, value); }
        }

        private string _cursorSource = @"./Assets/Cursors/Splitter_ud.cur";
        public string CursorSource
        {
            get { return _cursorSource; }
            set { SetProperty(ref _cursorSource, value); }
        }

        private double _gridAngle = 0d;
        public double GridAngle
        {
            get { return _gridAngle; }
            set { SetProperty(ref _gridAngle, value); }
        }

        private double _paneAngle = 0d;
        public double PaneAngle
        {
            get { return _paneAngle; }
            set { SetProperty(ref _paneAngle, value); }
        }

        private double _horSplitAngle = 0d;
        public double HorSplitAngle
        {
            get { return _horSplitAngle; }
            set { SetProperty(ref _horSplitAngle, value); }
        }

        private double _verSplitAngle = 90d;
        public double VerSplitAngle
        {
            get { return _verSplitAngle; }
            set { SetProperty(ref _verSplitAngle, value); }
        }

        #region Func

        private void ExpandOrCollapse(bool isExpand)
        {
            SettingRowHeight = isExpand ? GridLength.Auto : new GridLength(0);
        }

        #endregion
    }
}
