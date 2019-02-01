using log4net;
using System;

namespace SimplifiedCommon
{
    public static class Util
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Util));

        public static void DisposeObject(object obj)
        {
            if (obj == null)
                return;
            try
            {
                var disposable = obj as IDisposable;
                disposable?.Dispose();
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
            }
        }
    }
}
