using Scellecs.Morpeh;
namespace ElderBlood.Guard
{
    public struct DebugTriggerEvent : IEventData
    {
        public bool showDebugLineRenderer;

        public DebugTriggerEvent(bool showDebugLineRenderer)
        {
            this.showDebugLineRenderer = showDebugLineRenderer;
        }
    }
}