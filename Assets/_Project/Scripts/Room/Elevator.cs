using UnityEngine;
using DG.Tweening; 
using System.Collections.Generic;
public class Elevator : MonoBehaviour
{
    public Transform defaultPosition;
    public Transform thisRoomTargetPosition; 
    public float durationThisRoom = 2.0f;
    public float durationNextRoom = 2.0f;
public List<ElevatorDoor> Enterdoors;
public List<ElevatorDoor> Exitdoors;
    public DungeonManager dungeonManager;
    public DungeonManager.roomType nextRoomType;
    private GameObject nextRoom;
    private Stage nextStage;
    private bool isGoing = false;
    public bool isUsed = false;
    public GameObject barrier;
    [Header("Effects")]
    public Light elevatorLight; 
    public float flashDuration = 0.05f; 
    public int flashCount = 3; 
    [Header("Audio")]
    public AudioSource StartSound;
    public AudioSource moveAudio;    
    public AudioSource warpAudio;    
    public AudioSource arrivalAudio; 
    [Header("Distance Settings")]
    public float resetDistance = 5.0f; 
    private Transform playerTransform;
    void Update()
    {
        if (isUsed && !isGoing && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist > resetDistance)
            {
            Debug.Log("Player moved away. Auto-resetting...");
                if(nextStage.wall != null) nextStage.wall.SetActive(true);
                ResetStage(); 
                playerTransform = null; 
            }
        }
    }
    public void Go(GameObject player)
    {
        if (isGoing || isUsed) return;
        isGoing = true;
        nextRoom = dungeonManager.SelectRoom(nextRoomType);
        if (nextRoom != null)
        {
            nextStage = nextRoom.GetComponent<Stage>();
        }
        player.transform.SetParent(transform);
        EnterDoors_CloseDoors();
        barrier.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => {
            if (StartSound != null) StartSound.Play();
            if (moveAudio != null) 
            {
                moveAudio.volume = 1f; 
                moveAudio.Play();
            }
            nextStage.wall.SetActive(false);
        });
        seq.Append(transform.DOMove(thisRoomTargetPosition.position, durationThisRoom).SetEase(Ease.InQuad));
        seq.AppendCallback(() => {
            Camera.main.transform.DOShakePosition(0.2f, 0.1f);
            if (warpAudio != null) 
            {
                warpAudio.volume = 1f; 
                warpAudio.Play();
            }
            if (elevatorLight != null)
            {
                Sequence flashSeq = DOTween.Sequence();
                for (int i = 0; i < flashCount; i++)
                {
                    flashSeq.AppendCallback(() => elevatorLight.enabled = false);
                    flashSeq.AppendInterval(flashDuration);
                    flashSeq.AppendCallback(() => elevatorLight.enabled = true);
                    flashSeq.AppendInterval(flashDuration);
                }
                warpAudio.DOFade(0, 1.0f).OnComplete(() => warpAudio.Stop());
            }
            transform.position = nextStage.elevatorStartPos.position;
            transform.rotation = nextStage.elevatorStartPos.rotation;
        });
        seq.Append(transform.DOMove(nextStage.elevatorEndPos.position, durationNextRoom).SetEase(Ease.OutQuad));
        seq.OnComplete(() => {
            player.transform.SetParent(null);
            playerTransform = player.transform;
            isGoing = false;
            isUsed = true;
            if (moveAudio != null)
            {
                moveAudio.DOFade(0, 1.0f).OnComplete(() => moveAudio.Stop());
            }
            if (arrivalAudio != null) arrivalAudio.Play();
            barrier.SetActive(false);
            DOVirtual.DelayedCall(0.5f, () => ExitDoors_OpenDoors());
            nextStage.ResetStage();
            nextStage.StartStage();
        });
    }
    public void EnterDoors_OpenDoors() { foreach(var door in Enterdoors) door.Open(); }
    public void EnterDoors_CloseDoors() { foreach(var door in Enterdoors) door.Close(); }
    public void ExitDoors_OpenDoors() { foreach(var door in Exitdoors) door.Open(); }
    public void ExitDoors_CloseDoors() { foreach(var door in Exitdoors) door.Close(); }
    void Start()
    {
        EnterDoors_OpenDoors();
        nextRoom = dungeonManager.SelectRoom(nextRoomType);
        if (nextRoom != null)
        {
            nextStage = nextRoom.GetComponent<Stage>();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isGoing && !isUsed)
        {
            Go(other.gameObject);
        }
    }
    public void ResetStage()
    {
        DOVirtual.DelayedCall(1.0f, () => {
            transform.position = defaultPosition.position;
            transform.rotation = defaultPosition.rotation;
            ExitDoors_CloseDoors();
            EnterDoors_OpenDoors();
            nextRoom = dungeonManager.SelectRoom(nextRoomType);
            if (nextRoom != null)
                nextStage = nextRoom.GetComponent<Stage>();
            isUsed = false; 
            Debug.Log("Elevator is back to default and ready.");
        });
    }
}