using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetMgr : MonoBehaviour {
	
	public string 	sIP = "119.29.146.115";  // ʵ�ʷ�����ip
	public int 		iPort = 11009;  // ʵ�ʷ������˿� firefly "net":{"netport":11009}

    private XTcpClient m_Client;
	//private Queue<byte[]> 		m_bufferManager;
	private System.Action       m_ConnectSuccessCallBack;  // �ɹ��Ļص�����
	private bool	m_bWarnLostConnect;
	
	void Awake (){
		//m_bufferManager = new Queue<byte[]>();
		_init();
	}
	
	void _init (){
		m_Client = new XTcpClient();
        // ���ص��������뵽ί����
		m_Client.OnConnected += HandleM_ClientOnConnected;
		m_Client.OnDisconnected += HandleM_ClientOnDisconnected;
		m_Client.OnError += HandleM_ClientOnError;
	}
	
	void HandleM_ClientOnError (object sender, DSCClientErrorEventArgs e)
	{
		Debug.LogWarning("::OnError");
	}

	void HandleM_ClientOnDisconnected (object sender, DSCClientConnectedEventArgs e)
	{
		Debug.LogWarning("::OnDisconnected");
	}
    // ֪ͨ�ɹ����ӵĻص�
	void HandleM_ClientOnConnected (object sender, DSCClientConnectedEventArgs e)
	{
		Debug.LogWarning("::OnConnected");
		if (Connected) {
			if (m_ConnectSuccessCallBack != null) {
				m_ConnectSuccessCallBack();  // ֪ͨ
				m_ConnectSuccessCallBack = null;
			}
		}
		else{
			m_bWarnLostConnect = true;
		}
	}
	
    // ���Ӷ�ʧ��ʱ�����
	void _ShowLostConnect (){
		Globals.It.HideWaiting();
		Globals.It.ShowWarn(2, 5, null);
	}

    // �� ȡ���� �ŵ�ÿһ�̶�֡����
    void FixedUpdate (){
		if (m_Client != null && m_Client.Connected) {
			Globals.It.ProcessMsg(m_Client.Loop());  // ȡ����
		}
		if (m_bWarnLostConnect) {  // �Ƿ����Ӷ�ʧ
			m_bWarnLostConnect = false;
			_ShowLostConnect();
		}
	}
	
    // ###################################################### ����Ϊ�ⲿ���÷���

	public void ReInit (){
		_init();
	}
	
	public void Connect ()  // �޻ص������ӷ���
	{
		m_Client.Connect(sIP, iPort);
	}
	
	public void Connect (System.Action callback)  // �гɹ��ص������ӷ���
    {
		m_ConnectSuccessCallBack = callback;
		m_Client.Connect(sIP, iPort);
	}
	
	public void Send (byte[] buffer) {  // �����õ� �������ݷ���
		if (buffer != null && Connected) {
			m_Client.Send(buffer);
		}
	}
	
	public void Close (){
		if (Connected){
			m_Client.Close();
		}
	}
	
	public bool Connected {
		get {
			return m_Client != null && m_Client.Connected;
		}
	}
}
