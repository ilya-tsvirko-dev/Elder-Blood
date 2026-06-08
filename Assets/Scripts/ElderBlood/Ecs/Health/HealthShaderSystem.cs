using ElderBlood.Common;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Health
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HealthShaderSystem : ISystem 
    {
        private const float AlphaSpeed = 10f;
        private const float BaseAlpha = 1f;
        private const float FadedAlpha = 0.6f;
    
        public World World { get; set;}
    
        private Filter filter;
        private Stash<HealthComponent> healthStash;
        private Stash<HealthShaderComponent> healthShaderStash;

        public void OnAwake()
        {
            filter = World.Filter.With<HealthComponent>()
                .With<HealthShaderComponent>().Build();
            healthStash = World.GetStash<HealthComponent>();
            healthShaderStash = World.GetStash<HealthShaderComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in filter)
            {
                ref var healthComponent = ref healthStash.Get(entity);
                ref var healthShaderComponent = ref healthShaderStash.Get(entity);
                var fillAmount = (float)healthComponent.healthPoint / healthComponent.maxHealthPoint;
                healthShaderComponent.neededFillAmount = fillAmount;

                if (!Mathf.Approximately(healthShaderComponent.neededFillAmount, healthShaderComponent.fillAmount))
                {
                    healthShaderComponent.fillAmount = Mathf.Lerp(healthShaderComponent.neededFillAmount, healthShaderComponent.fillAmount, deltaTime * AlphaSpeed);
                    healthShaderComponent.material.SetFloat(ShaderIds.Fill, healthShaderComponent.fillAmount);
                }

                var targetAlpha = healthShaderComponent.isFaded ? FadedAlpha : BaseAlpha;
                if (!Mathf.Approximately(targetAlpha, healthShaderComponent.alpha))
                {
                    healthShaderComponent.alpha = Mathf.Lerp(targetAlpha, healthShaderComponent.alpha, deltaTime * AlphaSpeed);
                    healthShaderComponent.material.SetFloat(ShaderIds.Alpha, healthShaderComponent.alpha);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}