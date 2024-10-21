using System;
using System.Collections.Generic;
using Lockstep.Logic;
using Lockstep.Collision2D;
using Lockstep.Math;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;

public class CAnimation : MonoBehaviour, IView {
    [Header("shaderDebug")] public float shaderDuration;
    public float shaderFadeInTime;
    public float shaderFadeOutTime;
    public float shadersSampleDist;
    public float shadersSampleStrength;

    public bool useAnimator = false;
    
    [Header("Comps")] 
    public Animation animComp;
    public Animator animatorComp;
    public Transform rootTrans;
    public AnimatorConfig config;

    [Header("other")] public List<string> animNames = new List<string>();
    public string CurAnimName;
    public int curAnimIdx = -1;
    [HideInInspector] public List<AnimInfo> animInfos => config.anims;

    [HideInInspector] public AnimationState animState;
    [HideInInspector] public LFloat animLen;
    [HideInInspector] public LFloat timer;
    [HideInInspector] public AnimInfo CurAnimInfo;
    [HideInInspector] public AnimBindInfo CurAnimBindInfo;

    private LVector3 intiPos;
    public LFloat debugTimer = LFloat.zero;


    void Start(){
        if (useAnimator == false)
        {
            if (animComp == null) {
                animComp = GetComponent<Animation>();
                if (animComp == null) {
                    animComp = GetComponentInChildren<Animation>();
                }
            }
        }
       

        animNames.Clear();
        foreach (var info in animInfos) {
            animNames.Add(info.name);
        }

        intiPos = transform.position.ToLVector3();
        Play(AnimDefine.Idle);
    }

    public void SetTrigger(string name, bool isCrossfade = false){
        Play(name, isCrossfade); //TODO
    }

    public void Play(string name, bool isCrossfade = false){
        if (CurAnimName == name)
            return;
        var idx = animNames.IndexOf(name);
        if (idx == -1) {
            UnityEngine.Debug.LogError("miss animation " + name);
            return;
        }

        if (owner!=null)
        {
            Debug.Trace($"{owner.EntityId} PlayAnim {name} rawName {CurAnimName}");
            UnityEngine.Debug.Log($"{owner.EntityId} PlayAnim {name} rawName {CurAnimName}");
        }
        var hasChangedAnim = CurAnimName != name;
        CurAnimName = name;
        if (useAnimator)
        {
            animatorComp.GetCurrentAnimatorStateInfo(0);
        }
        else
        {
            animState = animComp[CurAnimName];
        }
        CurAnimInfo = animInfos[idx];
        CurAnimBindInfo = config.events.Find((a) => a.name == name);
        if (CurAnimBindInfo == null) CurAnimBindInfo = AnimBindInfo.Empty;
        if (hasChangedAnim) {
            //owner.TakeDamage(0, owner.transform2D.Pos3);
            ResetAnim();
        }

        TryPlayAnim(isCrossfade);
    }

    private void TryPlayAnim(bool isCrossfade)
    {
        if (useAnimator)
        {
            if (isCrossfade) {
                animatorComp.CrossFade(CurAnimName,0.3f);
            }
            else {
                animatorComp.Play(CurAnimName);
            }
        }
        else
        {
            var state = animComp[CurAnimName];
            if (state != null) {
                if (isCrossfade) {
                    animComp.CrossFade(CurAnimName);
                }
                else {
                    animComp.Play(CurAnimName);
                }
            }
            else
            {
                UnityEngine.Debug.Log($"No Animation State  {CurAnimName} on {animComp} ");
            }
        }
    }

    private BaseEntity owner;

    public void BindEntity(BaseEntity entity){
        owner = entity;
    }

    public void SetTime(LFloat timer){
        if (owner!=null)
        {
            var idx = GetTimeIdx(timer);
            intiPos = owner.transform.Pos3 - CurAnimInfo[idx].pos;
            Debug.Trace(
                $"{owner.EntityId} SetTime  idx:{idx} intiPos {owner.transform.Pos3}",
                true);
        }

        this.timer = timer;
    }

    public void LateUpdate(){
        if (CurAnimBindInfo != null && CurAnimBindInfo.isMoveByAnim) {
            rootTrans.localPosition = Vector3.zero;
        }
    }

    public void DoLateUpdate(LFloat deltaTime){
        animLen = CurAnimInfo.length;
        timer += deltaTime;
        if (timer > animLen) {
            ResetAnim();
        }

        if (!Application.isPlaying) {
            Sample(timer);
        }

        UpdateTrans();
    }

    void ResetAnim(){
        timer = LFloat.zero;
        SetTime(LFloat.zero);
    }

    public void Sample(LFloat time){
        if(animState == null) return;
        if (!Application.isPlaying) {
            animComp.Play();
        }

        animState.enabled = true;
        animState.weight = 1;
        animState.time = time.ToFloat();
        animComp.Sample();
        if (!Application.isPlaying) {
            animState.enabled = false;
        }
    }


    private void UpdateTrans(){
        var idx = GetTimeIdx(timer);
        if (CurAnimBindInfo.isMoveByAnim) {
            var animOffset = CurAnimInfo[idx].pos;
            var pos = owner.transform.TransformDirection(animOffset.ToLVector2XZ());
            owner.transform.Pos3 = (intiPos + pos.ToLVector3XZ(animOffset.y));
            rootTrans.localPosition = Vector3.zero;
        }
    }


    int GetTimeIdx(LFloat timer){
        var idx = (int) (timer / AnimatorConfig.FrameInterval);
        idx = Math.Min(CurAnimInfo.OffsetCount - 1, idx);
        return idx;
    }
}