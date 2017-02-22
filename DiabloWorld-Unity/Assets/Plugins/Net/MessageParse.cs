using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ��Ϣͷ
public class Message_Head {
	public byte HEAD_0 { get; set; }
	public byte HEAD_1 { get; set; }
	public byte HEAD_2 { get; set; }
	public byte HEAD_3 { get; set; }
	public byte ProtoVersion  { get; set; }
	public int ServerVersion  { get; set; }
	public int Length 		  { get; set; }	
}
// ��Ϣ��
public class Message_Body {
	public int iCommand { get; set; }
	public byte[] body 	{ get; set; }
}
// ��Ϣ = ��Ϣͷ + ��Ϣ��
public class MessageData {
	public Message_Head head { get; set; }
	public Message_Body body { get; set; }
}

public class MessageParse {
	// ����Ϣͷ��������
	public static Message_Head UnParseHead (byte[] buffer) {
		if (buffer.Length >= 13) {
			Message_Head head = new Message_Head();
			head.HEAD_0 = buffer[0];
			head.HEAD_1 = buffer[1];
			head.HEAD_2 = buffer[2];
			head.HEAD_3 = buffer[3];
			head.ProtoVersion = buffer[4];
            // ���϶���byte����

            // ��5 6 7 8�� ��9 10 11 12�� ��4λ��ת������������ת����C#��С����        
            // System.Array.Reverse(buffer, 5, 8);
            System.Array.Reverse(buffer, 5, 4);
            System.Array.Reverse(buffer, 9, 4);
            // ע�� reverse(5,8) ���Ǵ���ģ���������˼version �� length �պ��෴

            // ToInt32 => (value startIndex)
            head.ServerVersion = System.BitConverter.ToInt32(buffer, 5);  // ServerVersion 4λ 
			head.Length = System.BitConverter.ToInt32(buffer, 9);  // Length 4λ
            return head;
		}
		return null;
	}
    // ����Ϣ���������
    public static MessageData UnParse (byte[] buffer) {
		int iHead = 13;
		{
			Message_Head head = UnParseHead(buffer);
			if (head != null && head.Length <= (buffer.Length - iHead)) {
				Message_Body body = new Message_Body();
				System.Array.Reverse(buffer, 13, 4);
                // 13 14 15 16 ��4λ��ת������������ת����C#��С����
                body.iCommand = System.BitConverter.ToInt32(buffer, 13);
				body.body = new byte[head.Length - 4];  // ��Ϣ�峤��Length = command��int��4λ�� + msg���ȣ�byte[]��
                System.Array.Copy(buffer, iHead + 4, body.body, 0, body.body.Length);  // ���msg
				MessageData data = new MessageData();
				data.head = head;
				data.body = body;
				return data;
			}
		}
		return null;
	}
    // �����Ϣͷ
    public static byte[] ParseHead (int iVersion, int iByteLength){
		byte[] arrBytes = new byte[13];
        // 78,37,38,48,9,0
        arrBytes[0] = 78;  //�⼸����������ν��ʵ���Ͽ������ڷ���˿ͻ���֮�����֤
		arrBytes[1] = 37;
		arrBytes[2] = 38;
		arrBytes[3] = 48;
		arrBytes[4] = 9;  // ???? Э���

        // Copy => (source sourceIndex) (dest destIndex) (length)
		System.Array.Copy(System.BitConverter.GetBytes(iVersion),0,arrBytes,5, 4);
        System.Array.Copy(System.BitConverter.GetBytes(iByteLength),0,arrBytes,9, 4);

        // array index length
        // System.Array.Reverse(arrBytes, 5, 8);
        System.Array.Reverse(arrBytes, 5, 4);
        System.Array.Reverse(arrBytes, 9, 4);
        // ��5 6 7 8�� ��9 10 11 12����ת
        // ע�� reverse(5,8) ���Ǵ���ģ���������˼version �� length �պ��෴
        return arrBytes;
	}
    // �����Ϣ��
    public static byte[] ParseBody (int iCommand, string sJson){
		
		if(!string.IsNullOrEmpty(sJson)){
			byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(sJson);
			byte[] arrBytes = new byte[4+jsonBytes.Length];  // command���� + msg����
			System.Array.Copy(System.BitConverter.GetBytes(iCommand),0,arrBytes,0, 4);
			System.Array.Reverse(arrBytes, 0, 4);  // int�ͷ�ת
			System.Array.Copy(jsonBytes, 0, arrBytes, 4, jsonBytes.Length);  // byte�����跭ת
			return arrBytes;
		}
		
		return null;
		
	}
	
	public static byte[] Parse (int iVersion, int iCommand, string sJson){
		byte[] bodyBytes = ParseBody(iCommand, sJson);
		if (bodyBytes != null) {  // �����Ϣ���ǿյľ����跢��
			byte[] headBytes = ParseHead(iVersion, bodyBytes.Length);
			byte[] allBytes = new byte[headBytes.Length + bodyBytes.Length];  // ��Ϣ�ܳ���
			System.Array.Copy(headBytes,0,allBytes,0, headBytes.Length);
			System.Array.Copy(bodyBytes,0,allBytes,headBytes.Length, bodyBytes.Length);
			return allBytes;
		}
		return null;
	}
}
