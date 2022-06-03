using System.Collections.Generic;
using UnityEngine;
public class FreeCameraLogic : MonoBehaviour
{

    private Transform m_currentTarget = null;
    //private float m_distance = 2f;
    //private float m_height = 1;
    //private float m_lookAtAroundAngle = 180;
    public float distanceUp = 2f;

    public float distanceAway = 5f;

    public float smooth = 4f;//位置平滑移动值

    //public float camDepthSmooth = 5f;
    //static float previous_angle = 0;
    [SerializeField] private List<Transform> m_targets = null;
    private int m_currentIndex = 0;
    private void Start()
    {
        if (m_targets.Count > 0)
        {
            m_currentIndex = 0;
            m_currentTarget = m_targets[m_currentIndex];
        }
    }

    private void SwitchTarget(int step)
    {
        if (m_targets.Count == 0) { return; }
        m_currentIndex += step;
        if (m_currentIndex > m_targets.Count - 1) { m_currentIndex = 0; }
        if (m_currentIndex < 0) { m_currentIndex = m_targets.Count - 1; }
        m_currentTarget = m_targets[m_currentIndex];
    }

    public void NextTarget() { SwitchTarget(1); }
    public void PreviousTarget() { SwitchTarget(-1); }

    private void Update()
    {
        if (m_targets.Count == 0) { return; }
    }

    private void FixedUpdate()
    {
        /*if (m_currentTarget == null) { return; }

        float targetHeight = m_currentTarget.position.y + m_height;
        float currentRotationAngle = m_lookAtAroundAngle;

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        Vector3 position = m_currentTarget.position;
        position -= currentRotation * Vector3.forward * m_distance;
        position.y = targetHeight;

        transform.position = position;
        transform.LookAt(m_currentTarget.position + new Vector3(0, m_height, 0));*/
        if (m_currentTarget == null) { return; }
        //y軸向上移動distanceUp,目標當前位置向後移動distanceAway
        Vector3 position = m_currentTarget.position+Vector3.up*distanceUp-m_currentTarget.forward*distanceAway;
        //平滑的轉向
        transform.position = Vector3.Slerp(transform.position, position, smooth);
        //看向target
        transform.LookAt(m_currentTarget.position);


    }
}
