using Lidgren.Network;

namespace EliminationEngine.Network
{
    public interface INetworkSerialize
    {
        public void Serialize(NetOutgoingMessage msg);
    }
}
