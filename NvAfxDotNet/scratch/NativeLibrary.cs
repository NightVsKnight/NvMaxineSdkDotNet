using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NVAudioEffects
{
    internal class NativeLibrary : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string pathToDllFile);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        private IntPtr hLibrary = IntPtr.Zero;

        public NativeLibrary(string pathToDllFile)
        {
            if (String.IsNullOrEmpty(pathToDllFile)) throw new ArgumentNullException("pathToDllFile");
            hLibrary = LoadLibrary(pathToDllFile);
            if (hLibrary == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                throw new DllNotFoundException(pathToDllFile);
            }
        }

        public T GetMethod<T>(string methodName) where T : Delegate
        {
            var method = GetProcAddress(hLibrary, methodName);
            if (method == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException(methodName);
            }
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                if (hLibrary == IntPtr.Zero) return;
                FreeLibrary(hLibrary);
                hLibrary = IntPtr.Zero;

                disposedValue = true;
            }
        }

        ~NativeLibrary()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
