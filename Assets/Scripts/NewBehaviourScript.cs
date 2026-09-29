using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("技能参数")]
    public float moveSpeed = 5f;              // 技能移动速度
    public Vector3 targetPosition;            // 技能目标位置（在Inspector设置）
    public float stopDistance = 0.1f;         // 到达判定距离

    private bool isCastingSkill = false;      // 是否正在释放技能

    void Update()
    {
        // 检测玩家按F键释放技能
        if (Input.GetKeyDown(KeyCode.F) && !isCastingSkill )
        {
            Debug.Log("释放技能！开始移动到目标位置");
            isCastingSkill = true;
        }

        // 如果正在释放技能，执行插值移动
        if (isCastingSkill && Input.GetKeyUp(KeyCode.F))
        {
            MoveToTarget();
        }
    }

    // 使用Vector3.Lerp插值移动到目标位置
    void MoveToTarget()
    {
        // 计算到目标的距离
        float distance = Vector3.Distance(transform.position, targetPosition);

        // 判断是否到达目标
        if (distance <= stopDistance)
        {
            transform.position = targetPosition;  // 精确定位
            isCastingSkill = false;
            Debug.Log("技能移动完成！到达目标位置");
            return;
        }

        // 使用Vector3.Lerp插值移动
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}
