using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace GDD4500.LAB02
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private InputField _NameInputField;
        [SerializeField] private InputField _HostInputField;
        [SerializeField] private InputField _PortInputField;
        [SerializeField] private Button _SubmitButton;

        private string _Name;
        private string _Host;
        private int _Port;

        private static readonly Encoding Utf8 = new UTF8Encoding(false);


        private void Start()
        {
            _SubmitButton.onClick.AddListener(OnSubmitButtonClicked);
        }

        private void OnSubmitButtonClicked()
        {
            _Name = _NameInputField.text;
            _Host = _HostInputField.text;
            _Port = int.Parse(_PortInputField.text);

            _ = SendOnceAsync(_Name);
        }

        private async Task SendOnceAsync(string text)
        {
            using var udpSender = new UdpClient();
            byte[] payload = Utf8.GetBytes(text);

            try
            {
                int bytesSent = await udpSender.SendAsync(payload, payload.Length, _Host, _Port);
                Debug.Log(bytesSent);
            }
            catch
            {

            }
        }
    }
}
