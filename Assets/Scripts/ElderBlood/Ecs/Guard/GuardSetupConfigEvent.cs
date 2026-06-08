using ElderBlood.Configs.Guard;
using Scellecs.Morpeh;

namespace ElderBlood.Ecs.Guard
{
    public struct GuardSetupConfigEvent : IEventData
    {
        public GuardConfig Config;
        
        public GuardSetupConfigEvent(GuardConfig config)
        {
            Config = config;
        }
    }
}