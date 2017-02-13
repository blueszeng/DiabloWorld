using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// ս������ʱ����ʾ�������ͼ
public class BattleRecordView : MonoBehaviour {
	
	public GameObject	 	battlePlayer;
	public Transform[] 		upTrans = new Transform[9];
	public Transform[] 		downTrans = new Transform[9];
	
	private Data_MapBattle_R 	m_battleData;
	private Dictionary<int, BattlePlayer> 	m_players;
	private TextureAnimation 	m_startEffect;
	
	public void show (Data_MapBattle_R data) {
		m_battleData = data;
		_startData();
		_showStartEffect();
	}
	// ���������������  ��ʼ��ʾ���������ͼ
	void end (){
		Globals.It.ShowBattleRecordResultView(m_battleData.data.battleResult == 1, m_battleData.data.setData);
	}
	// 
	private void _startData(){
		m_players = new Dictionary<int, BattlePlayer>();
		int iCount = m_battleData.data.startData.Length;
		for (int i = 0; i < iCount; i++) {
			Data_MapBattle_R.StartData character = m_battleData.data.startData[i];
			Transform posT = character.chaDirection == 1 ? downTrans[character.chaPos-1] : upTrans[character.chaPos-1];
			GameObject playerObject = (GameObject)GameObject.Instantiate(battlePlayer);
			BattlePlayer player = playerObject.GetComponent<BattlePlayer> ();
			player.show(character);
			NGUIUtility.SetParent(posT, playerObject.transform);
			m_players.Add(character.chaBattleId, player);
		}
		
		Player mainPlayer = Globals.It.MainPlayer;
		mainPlayer.proMain.UpdateFromBattle(m_battleData.data);
	}
	// ��ʼ����
	void _showStartEffect (){
		System.Action<Object> handler = (asset) => {
			if(asset != null) {
				GameObject effectObject = (GameObject)GameObject.Instantiate(asset);
				TextureAnimation anima = effectObject.GetComponent<TextureAnimation>();
				anima.eventReceiver = gameObject;
				anima.finishEvent = "_startEffectEnd";
				NGUIUtility.SetParent(transform, effectObject.transform);
				m_startEffect = anima;
			}			
		};
		StartCoroutine(Globals.It.BundleMgr.CreateObject(kResource.Effect, "StartEffect", "StartEffect", handler));
		
	}
	// ��������  ����Э�� _step
	void _startEffectEnd (){
		if (m_startEffect != null) {
			GameObject.DestroyImmediate(m_startEffect.gameObject);
		}
		StartCoroutine("_step");
	}
	// ����   �ٿ���Э�� _playStep
	IEnumerator _step (){
		if (m_battleData.data.stepData != null && m_battleData.data.stepData.Length > 0) {
			Data_MapBattle_R.StepData[] datas = m_battleData.data.stepData;
			int iCount = datas.Length;
			for (int i = 0; i < iCount; i++) {
				Data_MapBattle_R.StepData stepdata = datas[i];
				yield return StartCoroutine("_playStep", stepdata);
			}
			yield return new WaitForSeconds(0.5f);
			end();  // end ���ý���
		}
	}
	
    // ���ǹ���  �ٶ�10 + 10֡��
	IEnumerator _playStep (Data_MapBattle_R.StepData stepdata){
		BattlePlayer battler = m_players[stepdata.chaBattleId];
		if (stepdata.skill == 61010) {  // ����1
			Animation attackAnim = battler.gameObject.GetComponent<Animation>();
			ActiveAnimation.Play(attackAnim, "attack", AnimationOrTween.Direction.Forward);
			int iFrameCount = playSkillEffect(battler.transform, 61010);  //
			yield return new WaitForSeconds(Time.deltaTime * iFrameCount);
			Data_MapBattle_R.StepData_Enemy[] enemys = stepdata.enemyChaArr;
			BattlePlayer enemy = null;
			for (int i = 0; i < enemys.Length; i++) {  // ��������
				enemy = m_players[enemys[i].enemyBattleId];
				enemy.ChangeHP(enemys[i].enemyChangeHp);
				iFrameCount = playSkillEffect(enemy.transform, 61011);  // 
			}
			yield return new WaitForSeconds(Time.deltaTime * iFrameCount);
		}
		else {  // ��������
			SkillJson skill = Globals.It.SkillJsonMgr.GetSkill(stepdata.skill);
			int iFrameCount = playSkillEffect(battler.transform, skill.releaseEffect);  //
			yield return new WaitForSeconds(Time.deltaTime * iFrameCount);
			
			Data_MapBattle_R.StepData_Enemy[] enemys = stepdata.enemyChaArr;
			BattlePlayer enemy = null;
			int iTempCount = 0;
			iFrameCount = 0;
			for (int i = 0; i < enemys.Length; i++) {
				enemy = m_players[enemys[i].enemyBattleId];
				enemy.ChangeHP(enemys[i].enemyChangeHp);
				iTempCount = playSkillEffect(enemy.transform, skill.effect);  // 
				if (iTempCount > iFrameCount) iFrameCount = iTempCount;
			}
			yield return new WaitForSeconds(Time.deltaTime * iFrameCount);
		}
	}
	
	int playSkillEffect (Transform parentT, int effectid){
		return 10;
	}
}
