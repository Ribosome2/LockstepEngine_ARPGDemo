using Lockstep.Math;
using UnityEngine;
using UnityEngine.UI;

namespace Lockstep.Logic {
    public partial class PlayerView : MonoBehaviour, IPlayerView {
        public SkillBoxConfig skillConfig;
        public Player owner;
        public int currentHealth => owner.currentHealth;

        [HideInInspector] public AudioClip deathClip;


        [Header("Movement")] CAnimation anim;
        AudioSource playerAudio;

        public int shootableMask {
            get => owner.SkillShoot.shootableMask;
            set => owner.SkillShoot.shootableMask = value;
        }

        [Header("Shoot")] ParticleSystem gunParticles;
        LineRenderer gunLine;
        AudioSource gunAudio;
        Light gunLight;
        [HideInInspector] public Light faceLight;
        private Transform shootTrans;


        public static LFloat MinRunSpd = new LFloat(1);
        public static LFloat MinFastRunSpd = new LFloat(7);

        public void Animating(bool isIdle){
            string animNames = AnimDefine.Walk;
            if (isIdle) {
                animNames = AnimDefine.Idle;
            }
            else {
                if (owner.speed > MinFastRunSpd) {
                    animNames = AnimDefine.RunFast;
                }
                else if (owner.speed > MinRunSpd) {
                    animNames = AnimDefine.Run;
                }
            }

            anim.SetTrigger(animNames, true);
        }

        public void BindEntity(BaseEntity entity){
            owner = entity as Player;
            if (owner == null)
            {
                Debug.LogError("Null owner ");
            }
            var config = ResourceManager.Instance.GetPlayerConfig(owner.PrefabId);
            shootTrans = transform.Find(config.attackTransName);
            var go = transform.Find(config.attackTransName);
            //init shooting
            shootableMask = LayerMask.GetMask("Shootable");
            gunParticles = go.GetComponent<ParticleSystem>();
            gunLine = go.GetComponent<LineRenderer>();
            gunAudio = go.GetComponent<AudioSource>();
            gunLight = go.GetComponent<Light>();
            faceLight = transform.Find(config.faceLightName).GetComponent<Light>();
            transform.position = owner.transform.Pos3.ToVector3();

            owner.eventHandler = this;
        }

        void Awake(){
            anim = GetComponent<CAnimation>();
            playerAudio = GetComponent<AudioSource>();
        }

        private void Update(){
            if (owner == null)
            {
                return;
            }
            var pos = owner.transform.Pos3.ToVector3();
            transform.position = Vector3.Lerp(transform.position, pos, 0.3f);
            //todo:一直摁着一个方向，有时都会出现y角度 晃动明显的情况，待优化
            var deg = owner.transform.deg.ToFloat();
            transform.rotation = Quaternion.Euler(0, deg, 0);
            
            // deg = Mathf.Lerp(transform.rotation.eulerAngles.y, deg, 0.3f);
            // Quaternion.Lerp(transform.rotation,Quaternion.Euler(0, deg, 0),0.3f) ;
        }

        public void TakeDamage(int amount, LVector3 hitPoint){
            playerAudio.Play();
        }

        public void OnDead(){
            DisableEffects();
            anim.SetTrigger(AnimDefine.Died);
            playerAudio.clip = deathClip;
            playerAudio.Play();
        }

        public void SetLinePosition(int index, LVector3 position){
            gunLine.SetPosition(index, position.ToVector3());
        }

        public void Shoot(){
            gunAudio.Play();

            gunLight.enabled = true;
            faceLight.enabled = true;

            gunParticles.Stop();
            gunParticles.Play();

            gunLine.enabled = true;
            gunLine.SetPosition(0, gunLine.transform.position);
        }

        public void DisableEffects(){
            gunLine.enabled = false;
            faceLight.enabled = false;
            gunLight.enabled = false;
        }
    }
}