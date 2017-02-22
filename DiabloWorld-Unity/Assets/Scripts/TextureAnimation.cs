using UnityEngine;
using System.Collections;
// ͼƬ���� ������ת
public class TextureAnimation : MonoBehaviour {
	
	public UISprite 	target;
	public float		fWaitSecond = 0.15f;
	public bool 		bRepeart;
	public string 		sFormat;
	public int 			iStart;  // 0
	public int 			iCount;  // �������
	
	public GameObject 	eventReceiver;
	public string 		finishEvent;
		
	private int 		m_iCur = 0;
	
	void Start (){
		if (Globals.It.bTestView) {
			runAction();
		}
	}
	
	public void runAction () {
		if (target != null)
			StartCoroutine("_runAction");
	}

	IEnumerator _runAction (){  // ��С����Ч��   ������Կ���ʹ��dotween��Ĳ��
		while (bRepeart || (!bRepeart && m_iCur <= iCount + iStart)) {
			if(m_iCur < iCount + iStart) {
				string sName = string.Format(sFormat, m_iCur);
				target.spriteName = sName;
				target.MakePixelPerfect();  // ������С
				m_iCur++;  // ���Ӵ���  m_iCur < iCount + iStart
                yield return new WaitForSeconds(fWaitSecond / Time.timeScale);  // ����
			}
			else{
				if (bRepeart) m_iCur = iStart;
				else {
					if (eventReceiver != null && !string.IsNullOrEmpty(finishEvent)) {
						eventReceiver.SendMessage(finishEvent, SendMessageOptions.DontRequireReceiver);
					}
					break;
				}
			}
		}
		//yield return null;
	}
}
