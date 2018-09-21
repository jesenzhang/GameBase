using System;

namespace GameBase
{
    /// <summary>
    /// Defines a specific listener so we can add/remove listeners
    /// outside of the core processing loop
    /// </summary>
    public class MessageListenerDefinition
    {
        /// <summary>
        /// Type of message to listen for
        /// </summary>
        public int MessageType;

        /// <summary>
        /// Filter for the messages
        /// </summary>
        public int Filter;

        /// <summary>
        /// Handler for the listener
        /// </summary>
        public MessageHandler Handler;

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
       // private static ObjectPool<MessageListenerDefinition> sPool = new ObjectPool<MessageListenerDefinition>(40, 10);

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static MessageListenerDefinition Allocate()
        {
            // Grab the next available object
            MessageListenerDefinition lInstance = new MessageListenerDefinition();
                //sPool.Allocate();
            lInstance.MessageType = Message.FilterTypeNothing;
            lInstance.Filter = Message.FilterTypeNothing;
            lInstance.Handler = null;

            // For this type, guarentee we have something
            // to hand back tot he caller
            if (lInstance == null) { lInstance = new MessageListenerDefinition(); }
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        //public static void Release(MessageListenerDefinition rInstance)
        //{
        //    if (rInstance == null) { return; }

        //    // We should never release an instance unless we're
        //    // sure we're done with it. So clearing here is fine
        //    rInstance.MessageType = Message.FilterTypeNothing;
        //    rInstance.Filter = Message.FilterTypeNothing;
        //    rInstance.Handler = null;

        //   // sPool.Release(rInstance);
        //}
    }
}