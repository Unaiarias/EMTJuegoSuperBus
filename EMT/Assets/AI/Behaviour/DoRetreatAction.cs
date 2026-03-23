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
    // OJO: estos nombres deben coincidir con lo que te creó Unity
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    EnemyRetreat retreat;
    bool started;

    protected override Status OnStart()
    {
        // Comprobamos que Agent y Target existen
        if (Self == null || Self.Value == null)
        {
            Debug.LogWarning("DoRetreatAction: Agent es null");
            return Status.Failure;
        }

        if (Target == null || Target.Value == null)
        {
            Debug.LogWarning("DoRetreatAction: Target es null");
            return Status.Failure;
        }

        // Cogemos el componente EnemyRetreat del Agent
        if (retreat == null)
            retreat = Self.Value.GetComponent<EnemyRetreat>();

        if (retreat == null)
        {
            Debug.LogWarning("DoRetreatAction: Agent no tiene EnemyRetreat");
            return Status.Failure;
        }

        // Iniciamos la retirada
        retreat.StartRetreat(Target.Value);
        started = true;

        // IMPORTANTE: empezamos en Running, no Success
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Si algo va mal, fallamos
        if (!started || retreat == null)
            return Status.Failure;

        // Mientras siga retirándose → Running
        if (!retreat.IsRetreatFinished)
            return Status.Running;

        // Cuando termine la retirada → Success
        return Status.Success;
    }

    protected override void OnEnd()
    {
        started = false;
    }
}

