using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace RelativeLineNumbers
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideOptionPage(typeof(OptionPageGrid),
    "Relative Line Numbers", "Cursor Line Number", 0, 0, true)]
    public sealed class RelativeLineNumbersPackage : Package
    {
        public RelativeLineNumbersPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }
    }


    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class OptionPageGrid : DialogPage
    {

        private bool optionValue = false;

        [Category("Cursor Line Number")]
        [DisplayName("Show absolute line number")]
        [Description("Configure what number to display for the line the cursor is currently on.\nSelect True to show the actual line number or False to show 0.")]
        public bool OptionBool
        {
            get { return optionValue; }
            set { optionValue = value; }
        }
    }
}
