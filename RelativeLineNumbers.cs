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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

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
		private Dictionary<int, int> _lineMap;

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
			_lineMap = new Dictionary<int, int>();

			_canvas = new Canvas();
			this.Children.Add(_canvas);

			this.ClipToBounds = true;

			_fontFamily = _textView.FormattedLineSource.DefaultTextProperties.Typeface.FontFamily;
			_fontEmSize = _textView.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize;

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
			DrawLineNumbers();
		}

		#endregion

		#region DrawLineNumbers

		private void DrawLineNumbers()
		{
			int lineCount = _textView.TextViewLines.Count;
			int notFoundVal = Int32.MaxValue;

			List<int> rlnList = new List<int>();

			// Get the index from the line collection where the cursor is currently sitting
			int cursorLineIndex = _textView.TextViewLines.GetIndexOfTextLine(_textView.Caret.ContainingTextViewLine);

			if (cursorLineIndex > -1)
			{
				_lineMap.Clear();
				for (int i = 0; i < lineCount; i++)
				{
					int relLineNr = cursorLineIndex - i;
					rlnList.Add(relLineNr);
					_lineMap[GetLineNumber(i)] = relLineNr;
				}
			}
			else
			{
				// Cursor is off the screen. Extrapolate relative line numbers.
				for (int i = 0; i < lineCount; i++)
				{
					int relLineNr = 0;

					// Try to get relative line number value for this line from the map.
					if (!_lineMap.TryGetValue(GetLineNumber(i), out relLineNr))
					{
						relLineNr = notFoundVal;
					}
					rlnList.Add(relLineNr);
				}

				// Extrapolate missing relative line number values
				for (int i = 0; i < lineCount; i++)
				{
					if (rlnList[0] != notFoundVal)
					{
						rlnList[i] = rlnList[0] - i;
					}
					else if (rlnList[rlnList.Count - 1] != notFoundVal)
					{
						rlnList[lineCount - 1 - i] = rlnList[lineCount - 1] + i;
					}
				}
			}

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

			string notFoundTxt = "~ ";

			// The amount of digits in the amount of lines is used to calculate the width of the line
			// numbers. For documents that have less than 9999 lines, this is static, which prevents the
			// margin's width from being adjusted while typing (adding a tenth line would otherwise
			// cause the margin's width to change, making the document 'jump' sideways).
			int documentLineCount = _textView.TextSnapshot.LineCount;
			int maxDigits = CalculateNumDigits(Math.Max(documentLineCount, 1000));
			
			for (int i = 0; i < lineCount; i++)
			{
				int relLineNumber = rlnList[i];
				_lineMap[GetLineNumber(i)] = relLineNumber;

				TextBlock tb = new TextBlock();
				tb.Text = relLineNumber == notFoundVal ? notFoundTxt : Math.Abs(relLineNumber).ToString();
				// Padding the text causes it to be right-aligned. Setting the TextAlignment property does
				// not properly do this.
				tb.Text = tb.Text.PadLeft(maxDigits);
				tb.FontFamily = _fontFamily;
				tb.FontSize = _fontEmSize;
				tb.Foreground = fgBrush;
				tb.FontWeight = fontWeight;
				Canvas.SetLeft(tb, _labelOffsetX);
				Canvas.SetTop(tb, _textView.TextViewLines[i].TextTop - _textView.ViewportTop);
				_canvas.Children.Add(tb);
			}

			string sample = new string('-', maxDigits);
			this.Width = GetMarginWidth(new Typeface(_fontFamily.Source), _fontEmSize, sample) + 2 * _labelOffsetX;
		}

		private static int CalculateNumDigits(int num)
		{
			if (num == 0)
			{
				return 1;
			}
			return (int)Math.Floor(Math.Log10(Math.Abs(num))) + 1;
		}

		private int GetLineNumber(int index)
		{
			int position = _textView.TextViewLines[index].Start.Position;
			return _textView.TextViewLines[index].Start.Snapshot.GetLineNumberFromPosition(position) + 1;
		}

		#endregion

		#region GetMarginWidth

		private double GetMarginWidth(Typeface fontTypeFace, double fontSize, string txt)
		{
			FormattedText formattedText = new FormattedText(
			txt,
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
