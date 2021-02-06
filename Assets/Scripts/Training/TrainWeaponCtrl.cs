using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainWeaponCtrl : MonoBehaviour
{
    [Header("Player")]
    public TrainPlayerCtrl m_Player;
    Animator m_PlayerAnim = null;
    public PlayerAudioCtrl m_PlayerAudio;

    //소리
    [Header("Audio")]
    public AudioSource audioSource;

    //Prefabs
    [Header("Prefabs")]
    public GameObject m_BulletChasingPrefab;
    public GameObject m_HitSparkPrefab;
    public GameObject m_HitHolePrefab;

    //Fire
    [Header("Fire")]
    public string m_WeaponName;
    public int m_bulletsPerMag;
    public int m_bulletsTotal;
    public int m_currentBullets;
    public float m_Range;
    public float m_FireRate;
    public float m_Damage;
    public float m_FireTimer;

    Transform m_ShootPoint;
    public Transform m_BulletCashingPoint;
    public ParticleSystem m_MuzzleFlash;
    public AudioClip m_ShootSound;

    public Text m_BulletsTxt;

    //--------반동 변수
    public Transform m_camRecoil;
    public float m_RecoilAim;
    public float m_RecoilKickback;
    public float m_RecoilAmount;
    //--------반동 변수

    private void Awake()
    {
        if (m_WeaponName == "AKM")
        {
            TrainingMgr.Inst.m_Assault57 = this.gameObject;
            TrainingMgr.Inst.m_WeaponCtrl = this.GetComponent<TrainWeaponCtrl>();

            if (PlayerPrefs.HasKey("MyAKMSkinName"))
                GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("WpMaterial/" + PlayerPrefs.GetString("MyAKMSkinName", ""));
        }
        else
        {
            TrainingMgr.Inst.m_Pistol = this.gameObject;
            this.gameObject.SetActive(false);

            if (PlayerPrefs.HasKey("MyPistolSkinName"))
                GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("WpMaterial/" + PlayerPrefs.GetString("MyPistolSkinName", ""));
        }
        m_camRecoil = Camera.main.transform;
        m_ShootPoint = Camera.main.transform;
        m_currentBullets = m_bulletsPerMag;

        m_Player = FindObjectOfType<TrainPlayerCtrl>();
        m_PlayerAnim = m_Player.GetComponentInChildren<Animator>();

        m_PlayerAudio = FindObjectOfType<PlayerAudioCtrl>();

        m_BulletsTxt = GameObject.Find("BulletsTxt").GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GameObject.Find("Weapon Positon").GetComponent<AudioSource>();
    }

    public string a_ReloadStr = "";
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (TrainingMgr.Inst.m_TrainingState != TrainingState.Play)
                return;

            if (m_currentBullets > 0)
            {
                Fire(); //발사
            }
        }

        if (m_FireTimer < m_FireRate)
            m_FireTimer += Time.deltaTime;

        if (m_BulletsTxt != null)
            m_BulletsTxt.text = m_currentBullets + " / " + m_bulletsTotal;

        //is_Reload
        AnimatorStateInfo animatorState;
        animatorState = m_PlayerAnim.GetCurrentAnimatorStateInfo(0);

        m_Player.m_isReload = animatorState.IsName(a_ReloadStr);
        //is_Reload

        AimDownSight();

        if (!m_Player.m_Prone)
            RecoilBack();
    }

    void Fire()
    {
        if (m_FireTimer < m_FireRate || m_Player.m_isReload)
            return;
        m_Player.m_isFire = true;

        RaycastHit hit;

        Vector2 reboundRay = Random.insideUnitCircle * m_RecoilAim;    //반동이 있다면 반동만큼의 크기(0.3f)를 가진 원의 범위 내에서 랜덤값을 가진다.
        Vector2 AimRay = Vector2.zero;  //조준하면 정확도가 높아진다.

        Vector2 Ray;

        if (!m_Player.m_isAiming)
            Ray = reboundRay;
        else
            Ray = AimRay;

        if (Physics.Raycast(m_ShootPoint.position, m_ShootPoint.TransformDirection(new Vector3(Ray.x, Ray.y, 15)), out hit, m_Range))
        {
            GameObject hitSpark = (GameObject)Instantiate(m_HitSparkPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            Destroy(hitSpark, 0.2f);
            hitSpark.transform.SetParent(hit.transform);
            GameObject hitHole = (GameObject)Instantiate(m_HitHolePrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            Destroy(hitHole, 3f);
            hitHole.transform.SetParent(hit.transform);

            TargetCtrl a_Target = hit.transform.GetComponent<TargetCtrl>();
            if(a_Target)
            {
                a_Target.Hit(); //타겟 넘어트리기
            }
        }

        m_currentBullets--;
        m_FireTimer = 0.0f;
        Recoil();
        EffectPlay();
    }

    void EffectPlay()
    {
        audioSource.PlayOneShot(m_ShootSound);
        m_MuzzleFlash.Play();
        BulletEffect();
    }

    public void BulletEffect()//탄피 날아가는 효과
    {
        Quaternion RandomQuaternion = new Quaternion(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f), 1);
        GameObject a_Bullet = Instantiate(m_BulletChasingPrefab, m_BulletCashingPoint);
        a_Bullet.transform.localRotation = RandomQuaternion;
        a_Bullet.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(Random.Range(50f, 100f), Random.Range(50f, 100f), Random.Range(-30f, 30f)));
        Destroy(a_Bullet, 3f);
    }


    public void AimDownSight()
    {
        if (m_Player.m_Prone)//누워있을땐 조준할 수 없다.
            return;
        if (m_PlayerAnim != null)
        {
            if (Input.GetMouseButton(1) && !m_Player.m_isReload)
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 40f, Time.deltaTime * 8f);
                m_Player.m_isAiming = true;
                m_PlayerAnim.SetBool("Aim", true);

            }
            else
            {
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f, Time.deltaTime * 8f);
                m_Player.m_isAiming = false;
                m_PlayerAnim.SetBool("Aim", false);
            }

            if (Input.GetMouseButtonDown(1))
                m_PlayerAudio.Aim();
        }

    }

    public void Reload()
    {
        int a_bulletsToReLoad = m_bulletsPerMag - m_currentBullets;

        if (a_bulletsToReLoad > m_bulletsTotal)//남아있는 총알이 재장전할 총알보다 적다면
        {
            a_bulletsToReLoad = m_bulletsTotal;
        }

        m_currentBullets += a_bulletsToReLoad;
        m_bulletsTotal -= a_bulletsToReLoad;
    }

    public void Recoil()
    {
        float recoilX = Random.Range(-m_RecoilKickback, m_RecoilKickback);
        Vector3 a_recoilCamVector = new Vector3(0, recoilX * 200f, 0);

        m_Player.Y = Mathf.Lerp(m_Player.Y, m_Player.Y + 1f, m_RecoilAmount * 500f);//카메라를 위로 올려줌
        m_camRecoil.localRotation = Quaternion.Slerp(m_camRecoil.localRotation, Quaternion.Euler(m_camRecoil.localEulerAngles + a_recoilCamVector), m_RecoilAmount);//카메라의 작은 회적으로 흔들림 표현
    }

    Vector3 a_EndVec = new Vector3(-96.35f, 90f, 0);
    public void RecoilBack()
    {
        if (m_camRecoil.localRotation != Quaternion.Euler(a_EndVec))
            m_camRecoil.localRotation = Quaternion.Slerp(m_camRecoil.localRotation, Quaternion.Euler(a_EndVec), Time.deltaTime * 2f);
    }
}
