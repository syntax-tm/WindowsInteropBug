using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace WindowsInteropBug
{
    public partial class MainWindow : IWin32Window, IDisposable
    {
        private bool _disposed;

        public IntPtr Handle { get; }

        public MainWindow()
        {
            InitializeComponent();

            var interopHelper = new WindowInteropHelper(this);
            Handle = interopHelper.EnsureHandle();
        }
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                CloseHandle(Handle);
            }
            
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void BtnCreateForm_OnClick(object sender, RoutedEventArgs e)
        {
            var frmTest = (Form) Activator.CreateInstance(typeof(Form));

            Debug.Assert(frmTest is not null);

            var windowInteropHelper = new WindowInteropHelper(this);
            windowInteropHelper.Owner = frmTest.Handle;

            //  suspend drawing and layout operations
            frmTest.SuspendLayout();
            
            frmTest.TopLevel = false;
            frmTest.FormBorderStyle = FormBorderStyle.None;
            
            frmTest.Shown += (sender, args) =>
            {
                var ctrl = sender as Control;
                var frm = ctrl?.FindForm();

                var frmDialog = new Form();
                frmDialog.ShowDialog();
            };

            frmHost.Child = frmTest;
            
            //  resume drawing and layout operations
            frmTest.ResumeLayout();
        }
    }
}
