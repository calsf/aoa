using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePopUp : MonoBehaviour
{
    [SerializeField] private PlayerStateObject playerState;
    [SerializeField] private Image icon;
    [SerializeField] private Text textName;
    [SerializeField] private Text textDesc;

    private Queue<UpgradeInfo> upgradeQueue { get; set; }
    private Animator anim;
    private bool isActive;

    public struct UpgradeInfo
    {
        public Sprite upgradeIcon;
        public string upgradeName;
        public string upgradeDesc;

        public UpgradeInfo(PlayerStateObject.Stat stat)
        {
            this.upgradeIcon = stat.statIcon;
            this.upgradeName = stat.statName;
            this.upgradeDesc = stat.statDesc;
        }

        public UpgradeInfo(PlayerStateObject.Power power)
        {
            this.upgradeIcon = power.powerIcon;
            this.upgradeName = power.powerName;
            this.upgradeDesc = power.powerDesc;
        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();

        upgradeQueue = new Queue<UpgradeInfo>();
    }

    void OnEnable()
    {
        playerState.OnUpgradePower.AddListener(QueueUpgrade);
        playerState.OnUpgradeStat.AddListener(QueueUpgrade);
    }

    void OnDisable()
    {
        playerState.OnUpgradePower.AddListener(QueueUpgrade);
        playerState.OnUpgradeStat.RemoveListener(QueueUpgrade);
    }

    void Update()
    {
        if (upgradeQueue.Count > 0 && !isActive)
        {
            // Remove next upgrade info from queue to display
            UpgradeInfo info = upgradeQueue.Dequeue();

            icon.sprite = info.upgradeIcon;
            textName.text = info.upgradeName;
            textDesc.text = info.upgradeDesc;

            isActive = true; // Set isActive to true, should be deactivated by animation event so next upgrade info can be shown
            anim.Play("UpgradePopUpShow");
        }
    }

    // Animation event
    public void OnShowFinish()
    {
        isActive = false;
    }

    // Get stat and convert to upgrade info and add to queue
    private void QueueUpgrade(PlayerStateObject.Stat stat)
    {
        UpgradeInfo newInfo = new UpgradeInfo(stat);
        upgradeQueue.Enqueue(newInfo);
    }

    // Get power and convert to upgrade info and add to queue
    private void QueueUpgrade(PlayerStateObject.Power power)
    {
        UpgradeInfo newInfo = new UpgradeInfo(power);
        upgradeQueue.Enqueue(newInfo);
    }
}
