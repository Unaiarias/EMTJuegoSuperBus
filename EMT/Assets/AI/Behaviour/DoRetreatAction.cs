using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Retreat",
    story: "[Agent] Retreats from [Target]",
    category: "Action",
    id: "do-retreat-action-001")]
public partial class DoRetreatAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    private EnemyRetreat retreat;
    private bool retreatStarted = false;

    protected override Status OnStart()
    {
        // Validaciones
        if (Self == null || Self.Value == null || Target == null || Target.Value == null)
        {
            return Status.Failure;
        }

        // Obtener o cachear el componente
        if (retreat == null)
            retreat = Self.Value.GetComponent<EnemyRetreat>();

        if (retreat == null)
        {
            Debug.LogError($"EnemyRetreat component missing on {Self.Value.name}");
            return Status.Failure;
        }

        // Intentar iniciar la retirada
        if (retreat.TryStartRetreat(Target.Value))
        {
            retreatStarted = true;
            return Status.Running;
        }

        // No se pudo iniciar (en cooldown o no está lo suficientemente cerca)
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (!retreatStarted || retreat == null)
            return Status.Failure;

        // Esperar a que termine la retirada
        return retreat.IsRetreatFinished ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        retreatStarted = false;
    }
}