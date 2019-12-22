﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using XamlTheme.Datas;

namespace XamlTheme.Controls
{
    [TemplatePart(Name = TextEditorTemplateName, Type = typeof(TextEditor))]
    public class TextEditorEx : Control
    {
        private static readonly Type _typeofSelf = typeof(TextEditorEx);

        private const string TextEditorTemplateName = "PART_TextEditor";

        private TextEditor _partTextEditor = null;
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null;
        private CompletionWindow _completionWindow = null;

        private DispatcherTimer _timer = null;
        private bool _disabledTimer = false;

        public string Text
        {
            get
            {
                if (_partTextEditor != null)
                    return _partTextEditor.Text;

                return string.Empty;
            }
            set
            {
                if (_partTextEditor != null)
                {
                    _disabledTimer = true;
                    _partTextEditor.Text = value;
                }
            }
        }

        public bool CanRedo
        {
            get { return _partTextEditor.CanRedo; }
        }

        public bool CanUndo
        {
            get { return _partTextEditor.CanUndo; }
        }

        static TextEditorEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public TextEditorEx()
        {
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Max(1, Delay)) };
            _timer.Tick += _timer_Tick;
        }

        #region RouteEvent

        public static readonly RoutedEvent DelayArrivedEvent = EventManager.RegisterRoutedEvent("DelayArrived", RoutingStrategy.Bubble, typeof(RoutedEventArgs), _typeofSelf);
        public event RoutedEventHandler DelayArrived
        {
            add { AddHandler(DelayArrivedEvent, value); }
            remove { RemoveHandler(DelayArrivedEvent, value); }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty LinePositionProperty = DependencyProperty.Register("LinePosition", typeof(int), _typeofSelf, new PropertyMetadata(OnLinePositionPropertyChanged));
        public int LinePosition
        {
            get { return (int)GetValue(LinePositionProperty); }
            set { SetValue(LinePositionProperty, value); }
        }

        private static void OnLinePositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var pos = (int)e.NewValue;

            ctrl._partTextEditor.TextArea.Caret.Column = pos;
        }

        public static readonly DependencyProperty LineNumberProperty = DependencyProperty.Register("LineNumber", typeof(int), _typeofSelf, new PropertyMetadata(OnLineNumberPropertyChanged));
        public int LineNumber
        {
            get { return (int)GetValue(LineNumberProperty); }
            set { SetValue(LineNumberProperty, value); }
        }

        private static void OnLineNumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var line = (int)e.NewValue;

            ctrl._partTextEditor.TextArea.Caret.Line = line;
        }

        public static readonly DependencyProperty IsModifiedProperty = TextEditor.IsModifiedProperty.AddOwner(_typeofSelf);
        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        public static readonly DependencyProperty WordWrapProperty = TextEditor.WordWrapProperty.AddOwner(_typeofSelf);
        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        public static readonly DependencyProperty ShowLineNumbersProperty = TextEditor.ShowLineNumbersProperty.AddOwner(_typeofSelf);
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = TextEditor.IsReadOnlyProperty.AddOwner(_typeofSelf);
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsCodeCompletionProperty = DependencyProperty.Register("IsCodeCompletion", typeof(bool), _typeofSelf);
        public bool IsCodeCompletion
        {
            get { return (bool)GetValue(IsCodeCompletionProperty); }
            set { SetValue(IsCodeCompletionProperty, value); }
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnDelayPropertyChanged));
        public double Delay
        {
            get { return (double)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        private static void OnDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var delay = (double)e.NewValue;

            if (ctrl._timer != null)
                ctrl._timer.Interval = TimeSpan.FromSeconds(Math.Max(1, delay));
        }

        public static readonly DependencyProperty GenerateCompletionDataProperty =
           DependencyProperty.Register("GenerateCompletionData", typeof(Func<string, List<string>>), _typeofSelf);
        public Func<string, List<string>> GenerateCompletionData
        {
            get { return (Func<string, List<string>>)GetValue(GenerateCompletionDataProperty); }
            set { SetValue(GenerateCompletionDataProperty, value); }
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged -= _partTextEditor_TextChanged;

                _partTextEditor.TextArea.TextEntering -= TextArea_TextEntering;
                _partTextEditor.TextArea.TextEntered -= TextArea_TextEntered;
                _partTextEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
            }

            _partTextEditor = GetTemplateChild(TextEditorTemplateName) as TextEditor;

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged += _partTextEditor_TextChanged;

                _partTextEditor.TextArea.TextEntering += TextArea_TextEntering;
                _partTextEditor.TextArea.TextEntered += TextArea_TextEntered;
                _partTextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

                _partTextEditor.TextArea.SelectionCornerRadius = 0;
                _partTextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
                _partTextEditor.TextArea.SelectionBorder = null;
                _partTextEditor.TextArea.SelectionForeground = null;
            }
        }

        #endregion

        #region Event

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            RaiseEvent(new RoutedEventArgs(DelayArrivedEvent));
        }

        private void _partTextEditor_TextChanged(object sender, EventArgs e)
        {
            RefreshFoldings();

            if (_disabledTimer)
            {
                _disabledTimer = false;
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (_partTextEditor == null)
                return;

            LineNumber = _partTextEditor.TextArea.Caret.Location.Line;
            LinePosition = _partTextEditor.TextArea.Caret.Location.Column;
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (IsReadOnly || !IsCodeCompletion || GenerateCompletionData == null)
                return;

            switch (e.Text)
            {
                case ".":
                case " ":
                case "<":
                    {
                        var finalDatas = GenerateCompletionData("");
                        if (finalDatas == null || finalDatas.Count == 0)
                            return;

                        _completionWindow = new CompletionWindow(_partTextEditor.TextArea);
                        _completionWindow.Resources = Resources;

                        var datas = _completionWindow.CompletionList.CompletionData;
                        finalDatas.ForEach(d => datas.Add(new EditorCompletionData(d)));

                        _completionWindow.Closed += delegate
                        {
                            _completionWindow.Resources = null;
                            _completionWindow = null;
                        };

                        _completionWindow.Show();
                    }
                    break;

                case "{":
                    InsertPairChar("}");
                    break;

                case "=":
                    InsertPairChar("\"\"");
                    break;

                case "\"":
                    InsertPairChar("\"");
                    break;
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (!IsCodeCompletion)
                return;

            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        #endregion

        #region Func

        private void InsertPairChar(string chars, bool caretFallBack = true, int fallbackTimes = 1)
        {
            _partTextEditor.TextArea.Document.Insert(_partTextEditor.TextArea.Caret.Offset, chars);

            if (caretFallBack)
                _partTextEditor.TextArea.Caret.Column = _partTextEditor.TextArea.Caret.Column - fallbackTimes;
        }


        public void Redo()
        {
            if (_partTextEditor == null)
                return;

            _partTextEditor.Redo();
        }

        public void Undo()
        {
            if (_partTextEditor == null)
                return;

            _partTextEditor.Undo();
        }

        private void RefreshFoldings()
        {
            if (_partTextEditor == null)
                return;

            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(_partTextEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, _partTextEditor.Document);
        }

        #endregion
    }
}
