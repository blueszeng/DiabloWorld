using UnityEngine;
using System.Collections;
// ��¼������
public class LogicMgr  {
	
	public static void OnLogin (Data_UserLogin_R data){
		Globals.It.HideWaiting();
		if(data != null) {
			if (data.result) {
				Globals.It.MainPlayer.proMain.SetLogin(data.data);
				if (data.data.hasRole) {
					// TODO �Ѿ����ڽ�ɫ
				}
				else {
				    // TODO ��һ�δ���
				}
			}
			else {
				Globals.It.ShowWarn(Const_ITextID.Msg_Tishi, data.message, null);
			}
		}
	}
	
}
