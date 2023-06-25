using EliminationEngine.GameObjects;
using Lidgren.Network;
using Newtonsoft.Json;
using OpenTK.Mathematics;

namespace EliminationEngine.Network
{
    public struct ObjectClientData
    {
        public string Name;
        public int Id;
        public Vector3 Position;
        public Vector3 Rotation;
        public object[] ObjectData;
        public EntityComponent[] Components;

        public ObjectClientData(string name, int id, Vector3 pos, Vector3 rot, object[] data, EntityComponent[] comps)
        {
            Name = name;
            Id = id;
            Position = pos;
            Rotation = rot;
            ObjectData = data;
            Components = comps;
        }
    }
    public class NetworkManager : EntitySystem
    {
        public static readonly Dictionary<string, string> BuiltinMessages = new Dictionary<string, string>() { 
            { "GET_OBJ", "get-obj" }, 
            { "RECV_OBJ", "recv-obj" }, 
            { "GET_NBY", "get-nearby" }, 
            { "RECV_NBY", "recv-nearby" } 
        };

        public bool IsServer = false;
        public bool IsRunning = false;
        public Dictionary<string, Func<NetIncomingMessage, bool>> MessageExec = new();
        public NetOutgoingMessage? ConnectionMessage = null;
        public NetPeerConfiguration DefaultConfig = new NetPeerConfiguration("industrial-macro-dream") { Port = 5889 };
        private NetClient? GClient = null;
        private NetServer? GServer = null;
        private NetPeer? GPeer = null;

        public List<ObjectClientData> UnprocessedData = new();
        public int[] KnownNearby = new int[0];
        private Dictionary<NetConnection, GameObject> AssignedObjects = new();
        public float NearbyDistance = 10f;

        public NetworkManager(Elimination e) : base(e)
        {

        }

        public NetOutgoingMessage? CreateMessage()
        {
            if (GPeer == null)
            {
                Logger.Error("NO NETWORK RUNNING.");
                return null;
            }
            return GPeer.CreateMessage();
        }

        public NetConnection? GetConnectionToServer()
        {
            if (IsServer)
            {
                Logger.Error("Can not get connection to itself.");
                return null;
            }
            if (GClient == null)
            {
                Logger.Error("NETWORK IS NOT RUNNING.");
                return null;
            }
            return GClient.ServerConnection;
        }

        public void SendMessage(NetConnection receiver, NetOutgoingMessage message)
        {
            if (GPeer == null)
            {
                Logger.Error("NETWORK IS NOT RUNNING. CAN NOT SEND MESSAGE.");
                return;
            }
            GPeer.SendMessage(message, receiver, NetDeliveryMethod.ReliableOrdered);
        }

        public void AssignToObject(NetConnection conn, GameObject obj)
        {
            if (!IsServer) return;
            if (AssignedObjects.ContainsKey(conn))
            {
                AssignedObjects[conn] = obj;
            }
            else
            {
                AssignedObjects.Add(conn, obj);
            }
        }

        public override void OnLoad()
        {
            base.OnLoad();

            //MessageExec.Add("get-obj", SendObject); // СЕРЬЁЗНАЯ УЯЗВИМОСТЬ. Клиент НЕ ДОЛЖЕН иметь возможность запросить данные объекта с сервера. Сервер должен самостоятельно решать, если отправка данных приемлима!
            MessageExec.Add("recv-obj", ReceiveObject);
            //MessageExec.Add("get-nby", SendNearby); // СЕРЬЁЗНАЯ УЯЗВИМОСТЬ. Клиент НЕ ДОЛЖЕН иметь возможность запросить данные объекта с сервера. Сервер должен самостоятельно решать, если отправка данных приемлима!
            MessageExec.Add("recv-nby", ReceiveNearby);

            if (Engine.ProcessedArgs.networked == 0) return;

            if (Engine.ProcessedArgs.server == 1)
            {
                StartServer(Engine.ProcessedArgs.port);
                Engine.window.MaxObjectId = 1000;
            }
            else
            {
                StartClient(Engine.ProcessedArgs.host, Engine.ProcessedArgs.port);
            }
        }

        private bool ReceiveNearby(NetIncomingMessage message)
        {
            var count = message.ReadInt32();
            KnownNearby = new int[count];
            int index = 0;
            for (var i = 0; i < count; i++)
            {
                KnownNearby[index] = message.ReadInt32();
            }
            return true;
        }

        private bool SendNearby(NetIncomingMessage message)
        {
            if (!CheckServer()) return false;

            if (!AssignedObjects.TryGetValue(message.SenderConnection, out var assigned))
            {
                var msg = GServer.CreateMessage();
                msg.Write(BuiltinMessages["RECV_NBY"]);
                msg.Write(0);
                GServer.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                return false;
            }

            int[] ids = new int[10000];
            int idCount = 0;
            foreach (var obj in Engine.GetAllObjects())
            {
                if ((obj.Position - assigned.Position).Length < NearbyDistance)
                {
                    ids[idCount] = obj.Id;
                    idCount++;

                    if (idCount >= 10000)
                    {
                        break;
                    }
                }
            }
            var msg2 = GServer.CreateMessage();
            msg2.Write(BuiltinMessages["RECV_NBY"]);
            msg2.Write(idCount);
            for (var i = 0; i < idCount; i++)
            {
                msg2.Write(ids[i]);
            }
            GServer.SendMessage(msg2, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);

            return true;
        }

        private bool SendObject(NetIncomingMessage message)
        {
            if (!CheckServer()) return false;
            var objId = message.ReadInt32();
            var obj = Engine.GetObjectById(objId);
            if (obj == GameObject.InvalidObject)
            {
                // Process invalid object id
            }
            var msg = GServer.CreateMessage();
            msg.Write(BuiltinMessages["RECV_OBJ"]);
            msg.Write(obj.Name);
            msg.Write(obj.Id);

            msg.Write(obj.Position.X);
            msg.Write(obj.Position.Y);
            msg.Write(obj.Position.Z);

            msg.Write(obj.DegreeRotation.X);
            msg.Write(obj.DegreeRotation.Y);
            msg.Write(obj.DegreeRotation.Z);

            msg.Write(obj.ObjectData.Count);
            foreach (var data in obj.ObjectData)
            {
                msg.Write(data);
            }

            var comps = obj.GetAllComponents();
            msg.Write(comps.Length);
            foreach (var comp in comps)
            {
                msg.Write(JsonConvert.SerializeObject(comp));
            }

            GServer.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
            return true;
        }

        private bool ReceiveObject(NetIncomingMessage message)
        {
            var name = message.ReadString();
            var id = message.ReadInt32();

            var posX = message.ReadFloat();
            var posY = message.ReadFloat();
            var posZ = message.ReadFloat();

            var rotX = message.ReadFloat();
            var rotY = message.ReadFloat();
            var rotZ = message.ReadFloat();

            var dataCount = message.ReadInt32();
            object[] dataArr = new object[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                var data = message.ReadString();
                var obj = JsonConvert.DeserializeObject(data);
                dataArr[i] = obj;
            }

            var compsCount = message.ReadInt32();
            var compsArr = new EntityComponent[compsCount];
            for (int i = 0; i < compsCount; i++)
            {
                var compStr = message.ReadString();
                var comp = (EntityComponent?)JsonConvert.DeserializeObject(compStr);
                if (comp == null)
                {
                    Logger.Error("Incorrect EntityComponent JSON value. Object: " + id + ", Component index: " + i);
                    continue;
                }
                compsArr[i] = comp;
            }

            var result = new ObjectClientData(name, id, new Vector3(posX, posY, posZ), new Vector3(rotX, rotY, rotZ), dataArr, compsArr);
            UnprocessedData.Add(result);

            return true;
        }

        public void ProcessUnreadyData()
        {
            foreach (var item in UnprocessedData )
            {
                if (Engine.window.GameObjects.TryGetValue(item.Id, out var obj)) {
                    obj.Position = item.Position;
                    obj.DegreeRotation = item.Rotation;
                    foreach (var comp in item.Components)
                    {
                        var type = comp.GetType();
                        var objType = obj.GetType();
                        var hasComponentMethod = objType.GetMethod("HasComponent").MakeGenericMethod(type);
                        if (!(bool)hasComponentMethod.Invoke(obj, new object[] { comp }))
                        {
                            obj.AddExistingComponent(comp);
                        }
                    }
                }
            }
            UnprocessedData.Clear();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!IsRunning) return;

            if (IsServer)
            {
                HandleServerMessages();
            }
            else
            {
                HandleClientMessags();
            }

            ProcessUnreadyData();
        }

        public void StartServer(int port)
        {
            DefaultConfig.Port = port;
            var server = new NetServer(DefaultConfig);
            server.Start();

            GServer = server;
            GPeer = server;
            IsServer = true;
            IsRunning = true;
        }

        public void StartClient(string host, int port)
        {
            DefaultConfig.Port = 0;

            var client = new NetClient(DefaultConfig);
            client.Start();
            client.Connect(host: host, port: port);

            GClient = client;
            GPeer = client;
            IsRunning = true;
        }

        private bool CheckServer()
        {
            if (GServer == null)
            {
                Logger.Error("THIS IS NOT A SERVER! CAN NOT PROCESS SERVER-SIDE MESSAGES.");
                return false;
            }
            return true;
        }

        private void HandleServerMessages()
        {
            NetIncomingMessage message;

            if (!CheckServer()) return;

            message = GServer.ReadMessage();
            if (message == null) return;
            switch (message.MessageType)
            {
                case NetIncomingMessageType.Data:
                    var data = message.ReadString();
                    if (MessageExec.TryGetValue(data, out var exec))
                    {
                        exec.Invoke(message);
                    }
                    break;
                case NetIncomingMessageType.StatusChanged:
                    // handle connection status messages
                    switch (message.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                            if (ConnectionMessage != null)
                            {
                                GServer.SendMessage(ConnectionMessage, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                            }
                            break;
                        case NetConnectionStatus.Disconnected:
                            // Disconnected
                            break;
                    }
                    break;

                case NetIncomingMessageType.DebugMessage:
                    // handle debug messages
                    // (only received when compiled in DEBUG mode)
                    Logger.Info(message.ReadString());
                    break;

                /* .. */
                default:
                    Logger.Warn("unhandled message with type: "
                        + message.MessageType);
                    break;
            }
        }

        private void HandleClientMessags()
        {
            NetIncomingMessage message;

            if (GClient == null)
            {
                Logger.Error("THIS IS NOT A CLIENT! CAN NOT PROCESS CLIENT-SIDE MESSAGES.");
                return;
            }

            message = GClient.ReadMessage();
            if (message == null) return;
            switch (message.MessageType)
            {
                case NetIncomingMessageType.Data:
                    var data = message.ReadString();
                    if (MessageExec.TryGetValue(data, out var exec))
                    {
                        exec.Invoke(message);
                    }
                    break;
                case NetIncomingMessageType.StatusChanged:
                    // handle connection status messages
                    switch (message.SenderConnection.Status)
                    {
                        case NetConnectionStatus.Connected:
                            if (ConnectionMessage != null)
                            {
                                GClient.SendMessage(ConnectionMessage, NetDeliveryMethod.ReliableOrdered);
                            }

                            break;
                        case NetConnectionStatus.Disconnected:
                            // Disconnected
                            break;
                    }
                    break;

                case NetIncomingMessageType.DebugMessage:
                    // handle debug messages
                    // (only received when compiled in DEBUG mode)
                    Logger.Info(message.ReadString());
                    break;

                /* .. */
                default:
                    Logger.Warn("unhandled message with type: "
                        + message.MessageType);
                    break;
            }
        }
    }
}
