using System;

namespace ElderBlood
{
    // Bridge: run lifecycle (LevelManager / ECS) → run-end UI. Registered as a VContainer singleton.
    public sealed class RunChannel
    {
        public event Action RunWon;
        public event Action RunLost;

        public void RaiseWon() => RunWon?.Invoke();
        public void RaiseLost() => RunLost?.Invoke();
    }
}
