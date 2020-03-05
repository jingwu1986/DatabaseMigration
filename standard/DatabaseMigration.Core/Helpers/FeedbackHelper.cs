using System;

namespace DatabaseMigration.Core
{
    public class FeedbackHelper
    {
        public static bool EnableLog { get; set; }


        public static void Feedback(IObserver<FeedbackInfo> observer, FeedbackInfo info, bool enableLog = true)
        {
            if (observer != null)
            {
                observer.OnNext(info);
            }

            if (EnableLog && enableLog)
            {
                string prefix = "";
                if (info.Owner != null)
                {
                    prefix = info.Owner.GetType().Name + ":";
                }

                LogHelper.LogInfo(prefix + info.Message);
            }
        }
    }
}
