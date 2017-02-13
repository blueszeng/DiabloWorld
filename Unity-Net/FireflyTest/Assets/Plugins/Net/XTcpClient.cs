using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

// ����ί�� Connect�ɹ�����  Error��������  DataIn��Ϣ����  Disconnect�Ͽ�����
public delegate void DSCClientOnConnectedHandler(object sender, DSCClientConnectedEventArgs e);
public delegate void DSCClientOnErrorHandler(object sender, DSCClientErrorEventArgs e);
public delegate void DSCClientOnDataInHandler(object sender, DSCClientDataInEventArgs e);
public delegate void DSCClientOnDisconnectedHandler(object sender, DSCClientConnectedEventArgs e);

// Connected������Disconnected��
public class DSCClientConnectedEventArgs : EventArgs
{
    public Socket socket;

    public DSCClientConnectedEventArgs(Socket soc)
    {
        this.socket = soc;
    }
}
// Error����
public class DSCClientErrorEventArgs : EventArgs
{
    public SocketException exception;

    public DSCClientErrorEventArgs(SocketException e)
    {
        this.exception = e;
    }
}
// DataIn����
public class DSCClientDataInEventArgs : EventArgs
{
    public byte[] Data;
    public Socket socket;

    public DSCClientDataInEventArgs(Socket soc, byte[] datain)
    {
        this.socket = soc;
        this.Data = datain;  // ��Ϣ
    }
}

public class XTcpClient
{
    /*
         ManualResetEvent �����߳�ͨ�����źŻ���ͨ�š���ͨ������ͨ���漰һ���߳��������߳̽���֮ǰ������ɵ�����
         ��һ�������ƣ��߳̿�ʼһ������˻������ɺ������̲߳��ܿ�ʼ��ʱ�������� Reset �Խ� ManualResetEvent ���ڷ���ֹ״̬��
         ���� WaitOne ���߳̽����������ȴ��źš�
         �� �����߳� ��ɻʱ�������� Set �ȴ��߳̿��Լ������е��źţ����ͷ����еȴ��̡߳�
         һ��������ֹ��ManualResetEvent ��������ֹ״̬������ WaitOne �ĵ��õ��߳̽��������أ�������������ֱ�������ֶ����á�
    */
    private static ManualResetEvent connectDone = new ManualResetEvent(false);  // Ŀǰ��δʹ��...

    // ���������¼�
    public event DSCClientOnConnectedHandler OnConnected;
    public event DSCClientOnErrorHandler OnError;
    public event DSCClientOnDisconnectedHandler OnDisconnected;

    private Socket m_Socket;
    private IPEndPoint m_Remote;  // host
    private Thread m_SelectThread;
    private bool m_bStopRun;
    private ArrayList m_CheckRead, m_CheckSend, m_CheckError;
    private Queue<byte[]> m_SendBuff;  // ׼�����͵����� ����
    private Queue<MessageData> m_Datas;  // �Ѿ����յ����� ����
    private object _lock = new object();

    private void _Init()  // ��ʼ��
    {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_SelectThread = new Thread(new ThreadStart(_LoopRun));  // ����һ���µ����̣߳���ֹ������Ϸ���߳�
        m_SelectThread.IsBackground = true;  // ����
        m_bStopRun = false;
        m_CheckRead = new ArrayList();  // ���
        m_CheckSend = new ArrayList();
        m_CheckError = new ArrayList();
        m_SendBuff = new Queue<byte[]>();  // ���͵���Ϣ����
        m_Datas = new Queue<MessageData>();  // ���ܵ�����Ϣ����
    }

    private void _BeginConnect()  // ��ʼ�첽����
    {
        m_Socket.BeginConnect(m_Remote, new AsyncCallback(_EndConnect), m_Socket);

        // connectDone.WaitOne();  // ����������
    }

    private void _EndConnect(IAsyncResult async)  // �ɹ������Զ��ص�
    {
        if (Connected)
        {
            m_SelectThread.Start();
            m_Socket.EndConnect(async);
        }

        if (OnConnected != null)
        {
            this.OnConnected(this, new DSCClientConnectedEventArgs(m_Socket));
        }

        // connectDone.Set();  // ��Reset()��Ӧ
    }

    private void _LoopRun()  // ��ʼ�߳�
    {
        while (!m_bStopRun && Connected)  // ������ ������
        {
            m_CheckRead.Clear();
            m_CheckSend.Clear();
            m_CheckError.Clear();
            m_CheckRead.Add(m_Socket);
            m_CheckSend.Add(m_Socket);
            m_CheckError.Add(m_Socket);

            /*
                ʹ��socket �� selct ���ڼ�����׽��ֵ�״̬�������� ֱ�ӷ��أ�
                ��C#�У�select�����ڻ�ΪSocket��ľ�̬����֮һ��fd_setҲ�ø�Ϊ�����ArrayList��������
                ������Ҫ���׽��ֶ�������ArrayList��Ķ����У���ArrayList�����count�����жϸ�����
                �����������±���ʽ��������׽����Խ���I/O����������������ֻ��һ���׽��֣�
            */
            Socket.Select(m_CheckRead, null, null, 100);  // ���Ľ���
            if (m_CheckRead.Count > 0)
            {
                _OnRead();
            }

            Socket.Select(null, m_CheckSend, null, 100);  // ���ķ���
            if (m_CheckSend.Count > 0)
            {
                _OnSend();
            }

            Socket.Select(null, null, m_CheckError, 100);  // ���Ĵ���
            if (m_CheckError.Count > 0)
            {
                _OnError(null);
            }
        }
    }

    // ############# ����������Ҫ��ȡʱ������ �Զ� ������
    private void _OnRead()
    {
        /*
            socket.Available �Ǵ��Ѿ���������յġ��ɹ���ȡ�����ݵ��ֽ�����
            ���ֵ��ָ ������ ���ѽ������ݵ��ֽ���������ʵ�ʵ����ݴ�С��
            ��������������ӳ٣�Send֮�����϶�ȡAvailable���Բ�һ���ܶ�����ȷ��ֵ��
            ���Բ�������socket.Available���ж��ܹ�Ҫ���ܵ��ֽ�����
        */
        if (m_Socket.Available > 0)
        {
            try
            {
                byte[] buffer = new byte[13];
                m_Socket.Receive(buffer, 13, SocketFlags.Peek);  // ��ʼ��ȡ Peek ���ٲ鿴������Ϣ
                Message_Head head = MessageParse.UnParseHead(buffer);  // ������Ϣͷ
                if (head == null)
                {
                    _OnError(new SocketException((int)SocketError.ProtocolOption));
                    Close();
                }
                else {
                    // 13 = 5��Э��ͷ��+4���汾�ţ�+4����Ϣ�峤��Length��
                    // ��Ϣ�峤�� = command��int��4λ��+msg��byte[]��
                    int iLength = head.Length + 13;  // �ܳ���
                    if (iLength <= m_Socket.Available)  // ��Ϣȫ������
                    {
                        buffer = new byte[iLength];
                        m_Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);  // ��ȡ��Ϣ��buffer��
                        MessageData data = MessageParse.UnParse(buffer);  // ������Data
                        if (data != null)
                        {
                            lock (_lock)
                            {  // �����������̻߳ᱻ����������ֱ�������⿪
                                m_Datas.Enqueue(data);  // ���������
                            }
                        }
                        else {
                            _OnError(new SocketException((int)SocketError.ProtocolOption));  // 10042 Option unknown, or unsupported.
                            Close();
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                Close();
            }
            catch (SocketException ex)
            {
                _OnError(ex);
                Close();
            }
        }
    }

    // ############# ����������Ҫ����ʱ������ �Զ� ������
    private void _OnSend()
    {
        Monitor.Enter(m_SendBuff);
        while (m_SendBuff.Count > 0 && Connected)
        {
            byte[] buffer = m_SendBuff.Dequeue();
            m_Socket.Send(buffer);  // ��ʼ����
        }
        Monitor.Exit(m_SendBuff);
    }

    // ############# �����ִ���ʱ �Զ� ������
    private void _OnError(SocketException ex)
    {
        if (OnError != null)
        {
            this.OnError(this, new DSCClientErrorEventArgs(ex));
        }
    }

    #region Interface �ⲿ���Ե�������ķ���

    public void Connect(string ip, int port)  // ��������
    {
        _Init();
        m_Remote = new IPEndPoint(IPAddress.Parse(ip), port);  // ��÷�������host
        _BeginConnect();
    }

    public void Send(byte[] buffer)  // ����Ϣ
    {
        if (buffer != null)
        {
            Monitor.Enter(m_SendBuff);  // �÷�ͬ�����Loop�����������̶߳�������ͬʱ���������Moniter.Enter()����
            m_SendBuff.Enqueue(buffer);  // ��Ϣ���
            Monitor.Exit(m_SendBuff);
        }
    }

    public MessageData Loop() // ȡ����
    { 
        MessageData data = null;
        if (m_Datas.Count > 0)
        {
            /*
                Lock �� Moniter �ķ�װ
                    lock(obj){
                        //�����
                    } 
                        �͵�ͬ�� 
                    Monitor.Enter(obj)�� 
                    //�����
                    Monitor.Exit(obj)��
                
                Enter(Object) ��ָ�������ϻ�ȡ��������
��������         Exit(Object) �ͷ�ָ�������ϵ���������
                ������Կ���ʹ��try catch ��֤ enter��ʹʧ��Ҳ�ܳɹ� exit
	        */
            Monitor.Enter(m_Datas);  // ���������̶߳�������ͬʱ���������Moniter.Enter()����
            if (m_Datas.Count > 0)
            {
                data = m_Datas.Dequeue();  // ���ݳ�����
            }
            Monitor.Exit(m_Datas);
        }
        return data;
    }

    public void Close()  // �ر�����
    {
        if (OnDisconnected != null)
        {  // ֪ͨί�йرյ��¼�
            this.OnDisconnected(this, new DSCClientConnectedEventArgs(m_Socket));
        }
        if (Connected)
        {
            m_bStopRun = true;
            Thread.Sleep(10);
            m_Socket.Shutdown(SocketShutdown.Both);  // �ͻ��˷���� socket ���رգ����������رգ�
            m_Socket.Close();
        }
        m_Socket = null;
    }

    public void ReConnect()  // ��������
    {
        Close();  // �ȹر�
        _Init();  // ��ʼ��
        _BeginConnect();  // ������
    }

    public bool Connected  // �����Ƿ�����
    {
        get { return m_Socket != null && m_Socket.Connected; }
    }

    #endregion

}
