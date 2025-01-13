using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovementController movementController;
    private PlayerLookController lookController;
    private PlayerStatus status;

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
    }

    // 마우스 입력을 통한 캐릭터 회전을 담당
    private void UpdateRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        lookController.UpdateRotation(mouseX, mouseY);
    }

    // 키보드 입력을 통한 캐릭터 이동을 담당
    private void UpdateMove()
    {
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
        if (Input.GetKey(KeyCode.C) && !movementController.isCrouching) movementController.Crouch();            
        else if (!Input.GetKey(KeyCode.C) && movementController.isCrouching) movementController.UnCrouch();     


        // 최종 이동 방향 설정
        movementController.MoveTo(new Vector3(x, 0, z));
    }

    private void PerformInteraction()
    {
        // 카메라에서 바라보는 방향으로 Ray를 쏘아 감지되는 오브젝트가 IIteractable 인터페이스가 구현되었는지 여부를 확인
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1.0f))
        {
            if (hit.collider.gameObject.GetComponent<IInteractable>() != null)
            {
                // 만약 상호작용 가능한 오브젝트가 Ray에 감지될 시, 플레이어는 E키를 통해 해당 오브젝트와 상호작용이 가능하도록
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.gameObject.GetComponent<IInteractable>().Interact();
                }
            }
        }

        // For Debugging
        Debug.DrawRay(ray.origin, ray.direction * 1.0f, Color.red);

    }
}
