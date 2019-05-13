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
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace RelativeLineNumbers
{
    #region Format definition
    [Export(typeof(EditorFormatDefinition))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ClassificationType(ClassificationTypeNames = "Relative Line Numbers")]
    [Name("Relative Line Numbers")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class RelativeLineNumberLook : ClassificationFormatDefinition
    {
        public RelativeLineNumberLook()
        {
            this.DisplayName = "Relative Line Numbers";
            this.ForegroundColor = Colors.LimeGreen;
            this.BackgroundColor = Colors.Black;
            this.IsBold = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ClassificationType(ClassificationTypeNames = "Relative Line Numbers Cursor Line")]
    [Name("Relative Line Numbers Cursor Line")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class RelativeLineNumberMarkerLook : ClassificationFormatDefinition
    {
        public RelativeLineNumberMarkerLook()
        {
            this.DisplayName = "Relative Line Numbers Cursor Line";
            this.ForegroundColor = Colors.HotPink;
            this.BackgroundColor = Colors.Black;
            this.IsBold = true;
        }
    }

    #endregion //Format definition
}
