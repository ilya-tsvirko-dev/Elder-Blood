using Scellecs.Morpeh;

namespace ElderBlood.Configs.Guard
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