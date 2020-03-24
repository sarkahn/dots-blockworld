using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockGame.BlockWorld
{
    public class FlyingCameraController : MonoBehaviour
    {
        private Vector3 _angles;
        public float speed = 1.0f;
        public float fastSpeed = 2.0f;
        public float mouseSpeed = 4.0f;

        public bool captureMouse = true;

        [SerializeField]
        GameObject _lockedCameraUI = default;

        private void OnEnable()
        {
            _angles = transform.eulerAngles;
            if( captureMouse )
                Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable() 
        { 
            Cursor.lockState = CursorLockMode.None; 
        }

        void ToggleCursorLock()
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ?
                Cursor.lockState = CursorLockMode.None : CursorLockMode.Locked;

            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.visible = false;
            else
                Cursor.visible = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleCursorLock();

            if (Cursor.lockState == CursorLockMode.None)
            {
                _lockedCameraUI.gameObject.SetActive(false);
                return;
            }
            else
                _lockedCameraUI.gameObject.SetActive(true);

            _angles.x -= Input.GetAxis("Mouse Y") * mouseSpeed;
            _angles.y += Input.GetAxis("Mouse X") * mouseSpeed;
            transform.eulerAngles = _angles;

            float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : speed;

            float xVelocity = Input.GetAxisRaw("Horizontal") * moveSpeed;
            float zVelocity = Input.GetAxisRaw("Vertical") * moveSpeed;

            float yVelocity = 0;
            if (Input.GetButton("Jump"))
                yVelocity = moveSpeed * .5f;
            if (Input.GetButton("Fire1"))
                yVelocity = -moveSpeed * .5f;

            // Cancel y rotation during movement
            var forward = transform.forward;
            forward.y = 0;

            transform.position += forward.normalized * zVelocity * Time.deltaTime;
            transform.position += transform.right * xVelocity * Time.deltaTime;
            transform.position += Vector3.up * yVelocity * Time.deltaTime;
        }
    } 
}