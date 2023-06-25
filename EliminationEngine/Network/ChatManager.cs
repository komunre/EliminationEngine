using EliminationEngine;
using EliminationEngine.GameObjects;

namespace EliminationEngine.Network
{
    public class ChatManager : EntitySystem
    {
        public List<string> ChatMessages = new List<string>();

        public ChatManager(Elimination e) : base(e) { }

        public void AddMessage(string message)
        {
            Logger.Info(Loc.Get("ADDED_MESSAGE") + message);
            ChatMessages.Add(message);
        }

        public void RemoveMessage(string message)
        {
            Logger.Info(Loc.Get("REMOVED_MESSAGE") + message);
            ChatMessages.Remove(message);
        }

        protected void LogMessageGet(int index, string msg)
        {
            Logger.Info(Loc.Get("GET_MESSAGE_AT") + index + Loc.Get("GET_MESSAGE_CONTENT") + msg);
        }

        public string GetMessageAt(int index)
        {
            LogMessageGet(index, ChatMessages[index]);
            return ChatMessages[index];
        }

        public int GetMessageIndex(string message)
        {
            LogMessageGet(-1, message);
            return ChatMessages.IndexOf(message);
        }

        public string[] GetLastMultipleMessages(int quant)
        {
            string[] messages = new string[quant];
            int a = 0;
            for (int i = ChatMessages.Count - 1; i > ChatMessages.Count - quant - 1; i--)
            {
                LogMessageGet(i, ChatMessages[i]);
                messages[a] = ChatMessages[i];
                a++;
            }
            return messages;
        }

        public string GetLastMessage()
        {
            LogMessageGet(ChatMessages.Count - 1, ChatMessages[ChatMessages.Count - 1]);
            return ChatMessages[ChatMessages.Count - 1];
        }
    }
}
