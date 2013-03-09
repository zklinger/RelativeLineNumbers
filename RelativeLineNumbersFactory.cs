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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;

namespace RelativeLineNumbers
{
	#region RelativeLineNumbers Factory
	/// <summary>
	/// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor
	/// to use.
	/// </summary>
	[Export(typeof(IWpfTextViewMarginProvider))]
	[Name(RelativeLineNumbers.MarginName)]
	[Order(After = PredefinedMarginNames.LineNumber)]
	[MarginContainer(PredefinedMarginNames.Left)]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Interactive)]
	internal sealed class MarginFactory : IWpfTextViewMarginProvider
	{
		[Import]
		internal IEditorFormatMapService FormatMapService = null;

		public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
		{
			return new RelativeLineNumbers(textViewHost.TextView, FormatMapService.GetEditorFormatMap(textViewHost.TextView));
		}
	}
	#endregion
}
