using Colyseus;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VEServicesClient
{
    public class BaseRoomManager<T>
    {
        public string SessionId { get; private set; }
        public Dictionary<string, object> Options { get; private set; }
        public string RoomName { get; private set; }
        public ColyseusRoom<T> Room { get; private set; }
        public bool IsConnected
        {
            get { return Room != null && Room.colyseusConnection != null && Room.colyseusConnection.IsOpen; }
        }

        private ColyseusClient _client;
        public ColyseusClient Client
        {
            get
            {
                if (_client == null)
                    _client = new ColyseusClient(ClientInstance.Instance.GetWsAddress());
                _client.Settings.useSecureProtocol = ClientInstance.Instance.secured;
                return _client;
            }
        }

        public BaseRoomManager(string roomName, Dictionary<string, object> options)
        {
            if (options == null)
                options = new Dictionary<string, object>();
            RoomName = roomName;
            Options = options;
        }

        public virtual async Task<bool> Join()
        {
            try
            {
                Room = await Client.JoinOrCreate<T>(RoomName, Options);
                Room.OnLeave += Room_OnLeave;
                SessionId = Room.SessionId;
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                Room = null;
                return false;
            }
        }

        public virtual async Task<bool> JoinById(string id)
        {
            try
            {
                Room = await Client.JoinById<T>(id, Options);
                Room.OnLeave += Room_OnLeave;
                SessionId = Room.SessionId;
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                Room = null;
                return false;
            }
        }

        private async void Room_OnLeave(int code)
        {
            if (code != 1000)
            {
                while (!IsConnected)
                {
                    Debug.Log($"Reconnecting {RoomName}");
                    await Task.Delay(1000);
                    await Join();
                    await Task.Delay(5000);
                }
            }
        }

        public virtual async Task Leave(bool consented = true)
        {
            try
            {
                if (Room != null)
                    await Room.Leave(consented);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                Room = null;
            }
        }

        protected virtual async void OnApplicationQuit()
        {
            await Leave();
        }
    }
}
