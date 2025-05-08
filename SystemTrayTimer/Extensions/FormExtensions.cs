using System;
using System.Windows.Forms;

namespace SystemTrayTimer.Extensions
{
    public static class FormExtensions
    {
        public static void SafeInvoke(this Form form, Action<Form> action)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            if (form.InvokeRequired)
            {
                form.Invoke(new Action(() => action(form)));
            }
            else
            {
                action(form);
            }
        }
    }
}
