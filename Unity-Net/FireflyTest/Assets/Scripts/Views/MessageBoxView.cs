using UnityEngine;
using System.Collections;
// ��Ϣ��Ļ
public class MessageBoxView : MonoBehaviour {
	
	public UILabel		labelTitle, labelText;
	
	private System.Action	m_BtnCallback;
	// ��ʾ ���� ���ûص�����
	public void show (string strTitle, string sText, System.Action callback){
		m_BtnCallback = callback;
		labelTitle.text = strTitle.Trim();
		labelText.text = sText.Trim();
	}
	
	public void onClick (){
		if (m_BtnCallback != null) {
			m_BtnCallback();
		}
		Globals.It.HideWarn();
	}
}
