using Newtonsoft.Json;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WebSocketServer
{
    public class Server
    {
        private SuperWebSocket.WebSocketServer _server = new SuperWebSocket.WebSocketServer();

        // クライアントセッションを格納
        private List<WebSocketSession> _clinets = new List<WebSocketSession>();

        // ルームリスト
        private List<string> _rooms = new List<string>();

        public Server()
        {
            //コンフィグオブジェクト作成
            RootConfig rootConfig = new RootConfig();
            ServerConfig serverConfig = new ServerConfig()
            {
                Port = 2012,
                Ip = "Any",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Tcp,
                Name = "Test WebSocket Server"
            };

            //サーバーオブジェクト作成＆初期化
            _server.Setup(rootConfig, serverConfig);

            //イベントハンドラの設定
            //接続
            _server.NewSessionConnected += server_NewSessionConnected;
            //メッセージ受信
            _server.NewMessageReceived += server_NewMessageReceived;
            //切断        
            _server.SessionClosed += server_SessionClosed;
        }

        public void Start()
        {
            //サーバー起動
            Console.WriteLine("Start!");
            _server.Start();
        }

        public void Stop()
        {
            //サーバー起動
            Console.WriteLine("Stop!");
            _server.Stop();
        }

        void server_NewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine("Connected!!");

            // 新クライアントとして格納
            _clinets.Add(session);
        }

        void server_NewMessageReceived(WebSocketSession session, string value)
        {
            Console.WriteLine("Received [" + value + "]"); // {"eventName":"join_room","data":{"room":""}}

            //// 全ユーザに送信
            //Parallel.ForEach(_clinets, p => p.Send(value));

            XmlDictionaryReader xmlReader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(value), XmlDictionaryReaderQuotas.Max);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JoinRoom));
            JoinRoom aaa = (JoinRoom)serializer.ReadObject(xmlReader);
            if (aaa.eventName == "join_room")
            {
                //rtc.on('join_room', function(data, socket) {
                Console.WriteLine("join_room");

                //var connectionsId = [];
                //var roomList = rtc.rooms[data.room] || [];
                //roomList.push(socket.id);
                //rtc.rooms[data.room] = roomList;
                Console.WriteLine("\tsession id [" + session.SessionID + "]");
                _rooms.Add(session.SessionID);

                List<string> ids = new List<string>();
                foreach (string room in _rooms)
                {// (var i = 0; i < roomList.length; i++) {
                    if (room == session.SessionID)
                    {
                        continue;
                    }
                    else
                    {
                        ids.Add(room);
                        NewPeerConnected msg = new NewPeerConnected();
                        Dictionary<string, List<string>> data = new Dictionary<string,List<string>>();
                        data["socketId"] = ids;
                        msg.data = data;
                        string json = msg.ToJson();
                        Console.WriteLine("\tnew_peer_connected [" + json + "]");
                        session.Send("new_peer_connected", json);
                    }
                }

                GetPeers msg2 = new GetPeers();
                GetPeers.InnerData data2 = new GetPeers.InnerData();
                data2.connections = ids;
                data2.you = session.SessionID;
                msg2.data = data2;
                string json2 = msg2.ToJson();
                Console.WriteLine("\tget_peers [" + json2 + "]");
                session.Send("get_peers", json2);
            }
        }

        void server_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            Console.WriteLine("Closed...");

            // 該当クライアントの除外
            _clinets.Remove(session);
        }
    }
}
