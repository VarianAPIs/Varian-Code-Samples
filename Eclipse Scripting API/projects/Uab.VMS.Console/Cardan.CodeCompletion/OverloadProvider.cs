using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor;
using Cardan.CodeCompletion.Extensions;
using Microsoft.Practices.Prism.Mvvm;

namespace Cardan.CodeCompletion
{
    public class OverloadProvider : BindableBase, IOverloadProvider
    {
        private readonly SignatureHelpItems _signatureHelp;
        private StackPanel _panel;

        public OverloadProvider(SignatureHelpItems signatureHelp)
        {
            _signatureHelp = signatureHelp;
            SelectItem();
        }

        private int _selectedIndex;
        private SignatureHelpItem _item;
        private object _currentHeader;
        private object _currentContent;
        private string _currentIndexText;

        public event PropertyChangedEventHandler PropertyChanged;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (SetProperty(ref _selectedIndex, value))
                {
                    SelectItem();
                }
            }
        }

        private void SelectItem()
        {
            _item = _signatureHelp.Items[_selectedIndex];
            if (_panel != null) { _panel.Children.Clear(); }
            else
            {
                _panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };
            }
            _panel.Children.Add(ToTextBlock(_item.PrefixDisplayParts));

            if (_item.Parameters != null)
            {
                for (int index = 0; index < _item.Parameters.Length; index++)
                {
                    var param = _item.Parameters[index];
                    _panel.Children.Add(ToTextBlock(param.DisplayParts));
                    if (index != _item.Parameters.Length - 1)
                    {
                        _panel.Children.Add(ToTextBlock(_item.SeparatorDisplayParts));
                    }
                }
            }
            _panel.Children.Add(ToTextBlock(_item.SuffixDisplayParts));
            CurrentHeader = _panel;
            this.OnPropertyChanged(()=>CurrentHeader);
            CurrentContent = ToTextBlock(_item.DocumenationFactory(CancellationToken.None));
        }

        private TextBlock ToTextBlock(IEnumerable<SymbolDisplayPart> parts)
        {
            if (parts == null) return new TextBlock();
            return parts.ToTextBlock();
        }

        public int Count
        {
            get { return _signatureHelp.Items.Count; }
        }

        // ReSharper disable once UnusedMember.Local
        public string CurrentIndexText
        {
            get { return _currentIndexText; }
            private set
            {
                SetProperty(ref _currentIndexText, value);
            }
        }

        public object CurrentHeader
        {
            get { return _currentHeader; }
            private set
            {
                SetProperty(ref _currentHeader, value);
            }
        }

        public object CurrentContent
        {
            get { return _currentContent; }
            private set
            {
                SetProperty(ref _currentContent, value);
            }
        }
    }
}
