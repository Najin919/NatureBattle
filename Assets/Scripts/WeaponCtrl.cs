using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public enum weaponState
{
    Active,
    Deactive,
}

public class WeaponCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Player")]
    public PlayerCtrl m_Player;
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
    public GameObject m_BloodPrefab;

    //Fire
    [Header("Fire")]
    public weaponState m_weaponState = weaponState.Active;
    public string m_WeaponName;
    public int m_bulletsPerMag;
    public int m_bulletsTotal;
    public int m_currentBullets;
    public float m_Range;
    public float m_FireRate;
    public float m_Damage;
    public float m_FireTimer;
    public GameObject m_Bullet;

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

    public PhotonView pv = null;

    public string m_AKMSkinName = "";
    public string a_AKMSkinName = "";
    public string m_PistolSkinName = "";
    public string a_PistolSkinName = "";

    [HideInInspector]
    public int AttackerId = -1; //방 밖으로 나갔다는 뜻

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        m_AKMSkinName = PlayerPrefs.GetString("MyAKMSkinName", "");
        m_PistolSkinName = PlayerPrefs.GetString("MyPistolSkinName", "");

        if (pv.IsMine)
        {
            if (m_WeaponName == "AKM")
            {
                m_weaponState = weaponState.Active;
                GameManager.Inst.m_Assault57 = this.gameObject;
                m_Player.m_WeaponCtrl = this.GetComponent<WeaponCtrl>();

                pv.RPC("AKMSetting", RpcTarget.All, m_AKMSkinName);
            }
            else
            {
                m_weaponState = weaponState.Deactive;
                GameManager.Inst.m_Pistol = this.gameObject;

                pv.RPC("PistolSetting", RpcTarget.All, m_PistolSkinName);
            }
            m_camRecoil = Camera.main.transform;
            m_ShootPoint = Camera.main.transform;
            m_currentBullets = m_bulletsPerMag;

            m_PlayerAnim = m_Player.GetComponentInChildren<Animator>();

            m_BulletsTxt = GameObject.Find("BulletsTxt").GetComponent<Text>();
        }
        else
        {
            m_AKMSkinName = a_AKMSkinName;
            m_PistolSkinName = a_PistolSkinName;
        }
    }

    [PunRPC]
    void AKMSetting(string SkinName)
    {
        if (PlayerPrefs.HasKey("MyAKMSkinName"))
            GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("WpMaterial/" + SkinName/* PlayerPrefs.GetString("MyAKMSkinName", "")*/);
    }

    [PunRPC]
    void PistolSetting(string SkinName)
    {
        if (PlayerPrefs.HasKey("MyPistolSkinName"))
            GetComponentInChildren<MeshRenderer>().material = Resources.Load<Material>("WpMaterial/" + SkinName/* PlayerPrefs.GetString("MyPistolSkinName", "")*/);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!pv.IsMine)
            return;
    }

    public string a_ReloadStr = "";
    // Update is called once per frame
    void Update()
    {
        if (m_weaponState != weaponState.Active)
            return;

        if (pv.IsMine)
        {
            if (Input.GetMouseButton(0))
            {
                if (GameManager.Inst.m_GameState != GameState.Start)
                    return;

                if (m_currentBullets > 0)
                {
                    Fire(); //발사
                    pv.RPC("Fire", RpcTarget.Others, null);
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
    }

    RaycastHit a_hit;
    [PunRPC]
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

        AttackerId = pv.Owner.ActorNumber;  //ownerId;

        if (Physics.Raycast(m_ShootPoint.position, m_ShootPoint.TransformDirection(new Vector3(Ray.x, Ray.y, 15)), out hit, m_Range))
        {
            GameObject Bullet = PhotonNetwork.Instantiate(m_Bullet.name, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            Bullet.GetComponent<BulletCtrl>().a_hit = hit;

            PlayerDamage a_Enemy = hit.transform.GetComponent<PlayerDamage>();
            if (a_Enemy && !hit.transform.GetComponent<PhotonView>().IsMine)
            {
                Bullet.GetComponent<BulletCtrl>().Enemy = a_Enemy;
                a_Enemy.TakeDamage(m_Damage, AttackerId);  //공격
            }
        }

        m_currentBullets--;
        m_FireTimer = 0.0f;
        Recoil();
        pv.RPC("EffectPlay", RpcTarget.All, null);
    }


    [PunRPC]
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
        if (GameManager.Inst.m_GameState != GameState.Start)
            return;

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

            if(Input.GetMouseButtonDown(1))
                m_PlayerAudio.Aim();

        }

    }

    public void Reload()
    {
        int a_bulletsToReLoad = m_bulletsPerMag - m_currentBullets;

        if(a_bulletsToReLoad > m_bulletsTotal)//남아있는 총알이 재장전할 총알보다 적다면
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
        m_camRecoil.localRotation =Quaternion.Slerp(m_camRecoil.localRotation, Quaternion.Euler(m_camRecoil.localEulerAngles + a_recoilCamVector), m_RecoilAmount);//카메라의 작은 회적으로 흔들림 표현
    }

    Vector3 a_EndVec = new Vector3(-96.35f, 90f, 0);
    public void RecoilBack()
    {
        if(m_camRecoil.localRotation != Quaternion.Euler(a_EndVec))
        m_camRecoil.localRotation = Quaternion.Slerp(m_camRecoil.localRotation, Quaternion.Euler(a_EndVec), Time.deltaTime * 2f);
    }

    [PunRPC]
    public void AddBullet(int Addbullets)
    {
        if (pv.IsMine == false) 
            return;
        m_bulletsTotal += Addbullets;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {       
        if (stream.IsWriting)
        {          
            stream.SendNext(m_currentBullets);
            stream.SendNext(m_bulletsTotal);
            stream.SendNext(m_AKMSkinName);
            stream.SendNext(m_PistolSkinName);
        }
        else //원격 플레이어의 위치 정보 수신
        {
            m_currentBullets = (int)stream.ReceiveNext();
            m_bulletsTotal = (int)stream.ReceiveNext();
            a_AKMSkinName = (string)stream.ReceiveNext();
            a_PistolSkinName = (string)stream.ReceiveNext();
        }
    }
   
}
