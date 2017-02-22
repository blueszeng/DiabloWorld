using UnityEngine;
using System.Collections;
// ��¼�ɹ���Ĳ���
public class DengluProtocol : IProtocol {
	
	#region IProtocol implementation
	public void Process (Message_Body info)
	{
		Data_UserLogin_R data = Globals.ToObject<Data_UserLogin_R>(info.body);
		if(data != null) {
			if (data.result) {
				Globals.It.DestoryDengluView();
				Globals.It.MainPlayer.proMain.SetLogin(data.data);  // ���浽Gloabsȥ
				if (!data.data.hasRole) {  // ��һ�δ�����ɫ
					Globals.It.ShowKaiChangGifView();  // ������
					Globals.It.HideWaiting();
				}
				else {
					Globals.It.ShowEnterGameView();
				}
			}
			else {
				Globals.It.HideWaiting();
				Globals.It.ShowWarn(Const_ITextID.Msg_Tishi, 14, null);
			}
		}
	}

	public int iCommand {
		get {
			return Const_ICommand.UserLogin;
		}
	}
	#endregion
	
}
