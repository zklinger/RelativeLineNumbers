/**********************************************************************
 * 
 *  Copyright 2013 Zoltan Klinger <zoltan dot klinger at gmail dot com>
 * 
 *  This file is part of RelativeLineNumbers Visual Studio 2010 extension.
 *
 *  RelativeLineNumbers is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  RelativeLineNumbers is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with RelativeLineNumbers.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *********************************************************************/

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Classification;

namespace RelativeLineNumbers
{
	/// <summary>
	/// A class detailing the margin's visual definition including both size and content.
	/// </summary>
	class RelativeLineNumbers : Canvas, IWpfTextViewMargin //, IClassifier
	{
		#region Member Variables

		public const string MarginName = "RelativeLineNumbers";
		private IWpfTextView _textView;
		private bool _isDisposed = false;
		private Canvas _canvas;
		private double _lastPos = -1.00;
		private double _labelOffsetX = 6.0;
		private FontFamily _fontFamily = null;
		private double _fontEmSize = 12.00;
		private IEditorFormatMap _formatMap;

		#endregion

		#region Constructor
		/// <summary>
		/// Creates a <see cref="RelativeLineNumbers"/> for a given <see cref="IWpfTextView"/>.
		/// </summary>
		/// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
		public RelativeLineNumbers(IWpfTextView textView, IEditorFormatMap formatMap)
		{
			_textView = textView;
			_formatMap = formatMap;

			_canvas = new Canvas();
			this.Children.Add(_canvas);

			this.ClipToBounds = true;

			_fontFamily = _textView.FormattedLineSource.DefaultTextProperties.Typeface.FontFamily;
			_fontEmSize = _textView.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize;

			this.Width = GetMarginWidth(new Typeface(_fontFamily.Source), _fontEmSize) + 2 * _labelOffsetX;
			_textView.Caret.PositionChanged += new EventHandler<CaretPositionChangedEventArgs>(OnCaretPositionChanged);
			_textView.ViewportHeightChanged += (sender, args) => DrawLineNumbers();
			_textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(OnLayoutChanged);
			_formatMap.FormatMappingChanged += (sender, args) => DrawLineNumbers();

			this.ToolTip = "To customize Relative Line Numbers select:\n" +
			               "  Tools -> Options -> Fonts and Colors -> Relative Line Numbers";
		}

		#endregion

		#region Event Handlers

		private void OnCaretPositionChanged (object sender, CaretPositionChangedEventArgs e)
		{
			double pos = e.TextView.Caret.ContainingTextViewLine.TextTop;

			if (_lastPos != pos)
			{
				_lastPos = pos;
				DrawLineNumbers();
			}
		}

		private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			if (e.VerticalTranslation || e.NewOrReformattedLines.Count > 1)
			{
				DrawLineNumbers();
			}
		}

		#endregion

		#region DrawLineNumbers

		private void DrawLineNumbers()
		{
			// Get the index from the line collection where the cursor is currently sitting
			int cursorLineIndex = _textView.TextViewLines.GetIndexOfTextLine(_textView.Caret.ContainingTextViewLine);

			// Clear existing text boxes
			if (_canvas.Children.Count > 0)
			{
				_canvas.Children.Clear();
			}

			ResourceDictionary rd = _formatMap.GetProperties("Relative Line Numbers");
			SolidColorBrush fgBrush = (SolidColorBrush)rd[EditorFormatDefinition.ForegroundBrushId];
			FontWeight fontWeight = Convert.ToBoolean(rd[ClassificationFormatDefinition.IsBoldId]) ?
				                         FontWeights.Bold : FontWeights.Normal;
			this.Background = (SolidColorBrush)rd[EditorFormatDefinition.BackgroundBrushId];

			for (int i = 0; i < _textView.TextViewLines.Count; i++)
			{
				TextBlock tb = new TextBlock();
				tb.Text = string.Format("{0,2}", Math.Abs(cursorLineIndex - i));
				tb.FontFamily = _fontFamily;
				tb.FontSize = _fontEmSize;
				tb.Foreground = fgBrush;
				tb.FontWeight = fontWeight;
				Canvas.SetLeft(tb, _labelOffsetX);
				Canvas.SetTop(tb, _textView.TextViewLines[i].TextTop - _textView.ViewportTop);
				_canvas.Children.Add(tb);
			}
		}

		#endregion

		#region GetMarginWidth

		private double GetMarginWidth(Typeface fontTypeFace, double fontSize)
		{
			FormattedText formattedText = new FormattedText(
			"99",
			System.Globalization.CultureInfo.GetCultureInfo("en-us"),
			System.Windows.FlowDirection.LeftToRight,
			fontTypeFace,
			fontSize,
			Brushes.Black);

			return formattedText.MinWidth;
		}

		#endregion

		private void ThrowIfDisposed()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(MarginName);
		}

		#region IWpfTextViewMargin Members

		public System.Windows.FrameworkElement VisualElement
		{
			get
			{
				ThrowIfDisposed();
				return this;
			}
		}

		#endregion

		#region ITextViewMargin Members

		public double MarginSize
		{
			get
			{
				ThrowIfDisposed();
				return this.ActualWidth;
			}
		}

		public bool Enabled
		{
			get
			{
				ThrowIfDisposed();
				return true;
			}
		}

		public ITextViewMargin GetTextViewMargin(string marginName)
		{
			return (marginName == RelativeLineNumbers.MarginName) ? (IWpfTextViewMargin)this : null;
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				GC.SuppressFinalize(this);
				_isDisposed = true;
			}
		}

		#endregion
	}
}
