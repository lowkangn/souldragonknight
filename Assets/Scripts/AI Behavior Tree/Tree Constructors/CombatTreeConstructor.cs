using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CombatStates;
using AiBehaviorTreeNodes;

namespace AiBehaviorTrees
{
    public static class CombatTreeConstructor
    {
        public static BehaviorTree ConstructMeleeCombatTree(ActorController actor, Movement movement, Combat combat)
        {
            return new BehaviorTree(
                new GetVisibleCombatTargetNode(combat),
                new SequenceNode(new List<BehaviorNode>()
                {
                    // get visible combat target
                    new SelectorNode(new List<BehaviorNode>()
                    {
                        new GetVisibleCombatTargetNode(combat),
                        // failed to get visible combat target; fail out of tree
                        new InverterNode(new SucceederNode(new SequenceNode(new List<BehaviorNode>()
                        {
                            // exit state machine if readying attack
                            new IsStateMachineInStateNode(combat.ActionStateMachine, typeof(ReadyAttackState)),
                            new ExitCombatStateMachineNode(combat)
                        })))
                    }),
                    // found visible combat target
                    new SelectorNode(new List<BehaviorNode>()
                    {
                        // in combat state, engaging target
                        new SequenceNode(new List<BehaviorNode>()
                        {
                            // in ready-attack state
                            new IsStateMachineInStateNode(combat.ActionStateMachine, typeof(ReadyAttackState)),
                            new InverterNode(new SequenceNode(new List<BehaviorNode>()
                            {
                                // exit ready-attack state if target is no longer in range
                                new InverterNode(new IsCombatTargetInMeleeRangeNode(movement, combat, true)),
                                new ExitCombatStateMachineNode(combat)
                            }))
                        }),
                        new IsStateMachineInStateNode(combat.ActionStateMachine, typeof(ActionState)),
                        // chasing target
                        new SequenceNode(new List<BehaviorNode>()
                        {
                            new SetMeleePosNode(actor),
                            new SelectorNode(new List<BehaviorNode>()
                            {
                                new SequenceNode(new List<BehaviorNode>()
                                {
                                    // move to target and attack if in melee range
                                    new GoToNavTargetNode(actor),
                                    new StopMovingNode(actor),
                                    new FaceNavTargetNode(movement),
                                    new IsCombatTargetInMeleeRangeNode(movement, combat, false),
                                    new StartMeleeAttackNode(combat)
                                }),
                                new SequenceNode(new List<BehaviorNode>()
                                {
                                    // not in melee range and cannot reach target
                                    new StopMovingNode(actor),
                                    new FaceNavTargetNode(movement)
                                })
                            })
                        })
                    })
                }));
        }

        public static BehaviorTree ConstructRangedCombatTree(ActorController actor, Movement movement, Combat combat)
        {
            return new BehaviorTree(
                new SequenceNode(new List<BehaviorNode>()
                {
                    new GetVisibleCombatTargetNode(combat),
                    new AddListenerToCombatTargetRangedAttackNode(combat)
                }),
                new SequenceNode(new List<BehaviorNode>()
                {
                    // get visible combat target
                    new SelectorNode(new List<BehaviorNode>()
                    {
                        new GetVisibleCombatTargetNode(combat),
                        // failed to get visible combat target; fail out of tree
                        new InverterNode(new SucceederNode(new SequenceNode(new List<BehaviorNode>()
                        {
                            // exit state machine if readying attack
                            new IsStateMachineInStateNode(combat.ActionStateMachine, typeof(ReadyAttackState)),
                            new ExitCombatStateMachineNode(combat)
                        })))
                    }),
                    // found visible combat target
                    new SelectorNode(new List<BehaviorNode>()
                    {
                        // in combat state, engaging target
                        new SequenceNode(new List<BehaviorNode>()
                        {
                            // in ready-attack state
                            new IsStateMachineInStateNode(combat.ActionStateMachine, typeof(ReadyRangedAttackState)),
                            // face target while readying if target position not locked yet
                            new InverterNode(new HasLockedTargetPositionNode(combat)),
                            new SetCombatTargetPosNode(),
                            new FaceNavTargetNode(movement)
                        }),
                        new IsStateMachineInStateNode(combat.ActionStateMachine, typeof(ActionState)),
                        // chasing target
                        new SequenceNode(new List<BehaviorNode>()
                        {
                            new SetRangedAttackPosNode(actor),
                            new SelectorNode(new List<BehaviorNode>()
                            {
                                new SequenceNode(new List<BehaviorNode>()
                                {
                                    // can reach target
                                    new GoToNavTargetNode(actor),
                                    new StopMovingNode(actor),
                                    new StartRangedAttackNode(combat)
                                }),
                                new SequenceNode(new List<BehaviorNode>()
                                {
                                    // cannot reach target
                                    new StopMovingNode(actor),
                                    new FaceNavTargetNode(movement)
                                })
                            })
                        })
                    })
                }));
        }
    }
}
