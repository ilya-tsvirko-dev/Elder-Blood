using ElderBlood.Guard.Container;

namespace ElderBlood.Guard.GuardBehavior
{
    public abstract class GuardBehaviorAbstract
    {
        protected readonly GuardBehaviorParametersContainer Container;

        protected GuardBehaviorAbstract(GuardBehaviorParametersContainer container)
        {
            Container = container;
        }
        
        public abstract void FixedUpdate();
        public abstract void Update();
        public abstract void Enter();
        public abstract void Exit();
    }
}