using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Logging;

namespace DesktopApp
{
    public partial class Form1 : Form
    {
        void AnyClick(object sender, EventArgs ea)
        {
            if (btnInfo == sender)
            {
                Logger.Info("Stop playing with loggers");
            }
            else if (btnWarn == sender)
            {
                Logger.Warn("This is a last warning!!!");
            }
            else if (btnError == sender)
            {
                try
                {
                    throw new ApplicationException("I told you already. No more playing with loggers");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }
        void FormInitialize()
        {
            btnInfo.Click += AnyClick;
            btnWarn.Click += AnyClick;
            btnError.Click += AnyClick;

            Logger.Add(new LogFileListener());
        }

        protected override void OnShown(EventArgs e)
        {
            if (!DesignMode)
                FormInitialize();
            base.OnShown(e);
        }


        public Form1()
        {
            InitializeComponent();
        }
    }
}
