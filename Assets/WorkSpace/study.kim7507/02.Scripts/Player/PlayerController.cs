using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovementController movementController;
    private PlayerLookController lookController;
    private PlayerStatus status;

    // 플레이어 인벤토리 상호작용 관련
    public InventorySystem inventory;
    [HideInInspector] public bool isOpenInventory;
    [HideInInspector] public bool isOpenItemDetailViewer;

    // 손전등
    [SerializeField] PlayerFlashlight flashlight;

    // 드래그 관련
    private Draggable draggable = null;

    // 손
    public Transform rightHand;

    private void Start()
    {
        // 마우스 커서를 보이지 않게 설정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        movementController = GetComponent<PlayerMovementController>(); 
        lookController = GetComponent<PlayerLookController>();
        status = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        UpdateRotation();
        UpdateMove();
        PerformInteraction();
        ManageFlashlight();
        ManageInventory();
        TakeAPhoto();
        DropItemInRightHand();
    }

    // 마우스 입력을 통한 캐릭터 회전을 담당
    private void UpdateRotation()
    {
        if (isOpenInventory) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        lookController.UpdateRotation(mouseX, mouseY);
    }

    // 키보드 입력을 통한 캐릭터 이동을 담당
    private void UpdateMove()
    {
        if (isOpenInventory)
        {
            movementController.Idle();
            movementController.MoveTo(new Vector3(0, 0, 0));
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 이동 입력 (느리게 걷기, 걷기, 달리기)
        if (x != 0 || z != 0)
        {
            if (Input.GetKey(KeyCode.LeftControl)) movementController.SlowWalk();
            else if (Input.GetKey(KeyCode.LeftShift)) movementController.Run();
            else movementController.Walk();
        }
        else movementController.Idle();

        // 점프 입력
        if (Input.GetKey(KeyCode.Space)) movementController.Jump();

        // 앉기 입력
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (movementController.isCrouching) movementController.UnCrouch();
            else movementController.Crouch();
        }

        // 최종 이동 방향 설정
        movementController.MoveTo(new Vector3(x, 0, z));
    }

    private void PerformInteraction()
    {
        if (isOpenInventory) return;

        // 카메라에서 바라보는 방향으로 Ray를 쏘아 감지되는 오브젝트가 IIteractable 인터페이스가 구현되었는지 여부를 확인
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2.0f))
        {
            // 레이의 시작점과 충돌 지점 사이에 디버그 라인 그리기
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            if (hit.collider.gameObject.GetComponent<IInteractable>() != null)
            {
                // 만약 상호작용 가능한 오브젝트가 Ray에 감지될 시, 플레이어는 E키를 통해 해당 오브젝트와 상호작용이 가능하도록
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.gameObject.GetComponent<IInteractable>().Interact();
                }
            }
            if (hit.collider.gameObject.GetComponent<Pickable>() != null)
            {
                // 만약 인벤토리에 저장 가능한 오브젝트가 Ray에 감지될 시, 플레이어는 G키를 통해 해당 오브젝트를 인벤토리에 저장하도록
                if (Input.GetKeyDown(KeyCode.G))
                {
                    inventory.AddToInventory(hit.collider.gameObject);
                }
            }
            if (hit.collider.gameObject.GetComponent<Draggable>() != null)
            {   
                // 드래그하여 플레이어가 옮길 수 있는 오브젝트가 Ray에 감지될 시, 플레이어는 마우스 좌클릭을 통해 오브젝트를 옮길 수 있도록
                if (draggable == null)
                {
                    Draggable currentDraggable = hit.collider.gameObject.GetComponent<Draggable>();

                    if (!currentDraggable.isDragging && Input.GetMouseButton(0))
                    {
                        currentDraggable.BeginDrag();
                        draggable = currentDraggable;
                    }
                }
                else
                {
                    if (draggable.isDragging && !Input.GetMouseButton(0))
                    {
                        draggable.EndDrag();
                        draggable = null;
                    }
                }
            }
        }
    }

    private void ManageFlashlight()
    {
        if (isOpenInventory) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            flashlight.ToggleFlashlight();
        }
    }

    private void ManageInventory()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpenItemDetailViewer) return;

            if (!isOpenInventory) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Locked;

            isOpenInventory = !isOpenInventory;
            Cursor.visible = !Cursor.visible;

            inventory.ToggleInventory();
        }
    }
    
    private void TakeAPhoto()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            gameObject.GetComponent<TakePhoto>().Capture();
        }
    }

    public void EquipItemInRightHand(GameObject item)
    {
        isOpenItemDetailViewer = !isOpenItemDetailViewer;
        isOpenInventory = !isOpenInventory;

        if (!isOpenInventory) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !Cursor.visible;

        // 현재 오른손에 가지고 있는 오브젝트 버리기
        DropItemInRightHand();
        item.transform.SetParent(rightHand);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        item.transform.localRotation = Quaternion.identity;
    }

    private void DropItemInRightHand()
    {
        // 손에 아무런 오브젝트가 없는 경우 바로 종료
        if (rightHand.childCount <= 0) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            // TODO: 아이템 버리는 로직 구현 필요
            Debug.Log("아이템을 버립니다.");

            Vector3 dropPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;

            rightHand.GetChild(0).transform.position = dropPosition;
            rightHand.GetChild(0).transform.SetParent(null);
        }
    }
}
