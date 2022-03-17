using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CombatStates
{
    public class BlockHitState : CombatState
    {
        private readonly float duration;
        private readonly BlockState.Direction direction;
        private readonly UnityEvent blockHitEvent;

        private float startTime;
        private bool willReturnToBlock;

        public bool WillReturnToBlock
        {
            get => willReturnToBlock;
            set
            {
                willReturnToBlock = value;
                owner.Animator.SetBool("isBlocking", value);
                if (direction == BlockState.Direction.UPWARDS)
                {
                    owner.Animator.SetBool("isBlockingUpwards", value);
                }
            }
        }

        public BlockHitState(Combat owner, float duration, BlockState.Direction direction, UnityEvent blockHitEvent) : base(owner)
        {
            this.duration = duration;
            this.direction = direction;
            this.blockHitEvent = blockHitEvent;
        }

        public override void Enter()
        {
            startTime = Time.time;
            WillReturnToBlock = true;
            owner.Animator.SetBool("isBlockingHit", true);
            blockHitEvent.Invoke();
        }

        public override void Execute()
        {
            if (Time.time - startTime > duration)
            {
                // switch to block state or exit combat state machine
                if (WillReturnToBlock)
                {
                    owner.CombatStateMachine.ChangeState(new BlockState(owner, duration, direction, blockHitEvent));
                }
                else
                {
                    owner.CombatStateMachine.Exit();
                }
            }
        }

        public override void Exit()
        {
            owner.Animator.SetBool("isBlockingHit", false);
        }
    }
}