﻿namespace AutomaticGPTPupTest1
{
    public static class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T> handler, object sender, T args)
            where T : EventArgs
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        public static void Raise(this EventHandler handler, object sender,
            EventArgs args)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }
    }
}
