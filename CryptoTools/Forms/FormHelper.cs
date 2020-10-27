using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoTools.Forms
{
    public static class FormHelper
    {
        internal static void DoubleBuffered(this DataGridView grid, bool Vakue)
        {
            if (!SystemInformation.TerminalServerSession)
            {
                Type dgvType = grid.GetType();
                PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(grid, true, null);
            }
        }

    }
}
