using Unity.Netcode;

public class ButtonClickedPacket : INetworkSerializable
{
    public ButtonType buttonType;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref buttonType);
    }
}
