using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Game;
    using UnityEngine;

    public class UIManager : MonoBehaviour
    {
        private Dictionary<String, Transform> _uiLayer = new();
        private Dictionary<String, Transform> _ui = new();
        
        public static UIManager Instance { get; private set; }

        public (Transform, string) FindChildRecursive(Transform parent, string names)
        {
            var splited = names.Split('.');
            Queue<string> newQueue = new Queue<string>();
            for (var i = 0; i < splited.Length; i++)
            {
                newQueue.Enqueue(splited[i]);
            }

            return _findChildRecursive(parent, newQueue);
        }
        
        private (Transform, string) _findChildRecursive(Transform parent, Queue<string> names)
        {
            if (names.TryDequeue(out string name))
            {
                foreach (Transform child in parent)
                {
                    if (child.name == name)
                    {
                        if (names.Count < 1) return (child, name);
                        return  _findChildRecursive(child, names);
                    }
                }
            }

            return (null, null);
        }

        void AddUILayer(string path)
        {
            (Transform, string) tr = FindChildRecursive( transform, path);
            if (tr.Item1 == null)
                return;
            
            _uiLayer.Add(tr.Item2, tr.Item1);
        }
        
        void AddUI(string path)
        {
            (Transform, string) tr = FindChildRecursive( transform, path);
            if (tr.Item1 == null)
                return;
            
            _ui.Add(path, tr.Item1);
        }

        Transform GetUI(string path)
        { 
            _ui.TryGetValue(path, out Transform tr);
            return tr;
        }

        private void OnEnable()
        {
            {
                if (_uiLayer.TryGetValue("Login", out Transform tr))
                {
                    var login = FindChildRecursive(tr, "LoginID");
                    login.Item1.GetComponent<TMPro.TMP_InputField>().onSubmit.AddListener((string id) =>
                    {
                        SuperManager.Instance.playerId = id;
                        TcpProtobufClient.Instance.SendLoginMessage(id);
                        SetUIState("Chat");
                    });
                }

            }
            {
                if (_uiLayer.TryGetValue("Chat", out Transform tr))
                {
                    var chat = FindChildRecursive(tr, "ChatContent");
                    chat.Item1.GetComponent<TMPro.TMP_InputField>().onSubmit.AddListener((string content) =>
                    {
                        TcpProtobufClient.Instance.SendChatMessage(SuperManager.Instance.playerId, content);
                    });
                }
            }
        }

        private void OnDisable()
        {
            {
                if (_uiLayer.TryGetValue("Login", out Transform tr))
                {
                    var login = FindChildRecursive(tr, "LoginID");
                    login.Item1.GetComponent<TMPro.TMP_InputField>().onSubmit.RemoveAllListeners();
                }
            }
            {
                if (_uiLayer.TryGetValue("Chat", out Transform tr))
                {
                    var chat = FindChildRecursive(tr, "ChatContent");
                    chat.Item1.GetComponent<TMPro.TMP_InputField>().onSubmit.RemoveAllListeners();
                }
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);

                AddUILayer("Login");
                AddUILayer("Chat");

                AddUI("Chat.View.Viewport.ChatWhiteBoard");
                
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            SetUIState("Login");
        }
        
        public void SetUIState(string stateName)
        {
            foreach (var keyValuePair in _uiLayer)
            {
                keyValuePair.Value.gameObject.SetActive(false);
            }

            if (_uiLayer.TryGetValue(stateName, out Transform tr))
            {
                tr.gameObject.SetActive(true);
            }
        }

        public void OnRecevieChatMsg(ChatMessage chatmsg)
        {   
            Transform chatBoardTransform = GetUI("Chat.View.Viewport.ChatWhiteBoard");
            TMPro.TextMeshProUGUI chatWhiteBoard = chatBoardTransform.GetComponent<TMPro.TextMeshProUGUI>();
            if (chatWhiteBoard)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(chatmsg.Sender);
                builder.Append(" : ");
                builder.Append(chatmsg.Content);
                builder.Append("\n");

                chatWhiteBoard.text += builder;
            }
        }
    }
