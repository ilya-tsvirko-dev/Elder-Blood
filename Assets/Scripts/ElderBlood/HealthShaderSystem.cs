using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HealthShaderSystem : ISystem 
    {
        private static readonly int Fill = Shader.PropertyToID("_FillAmount");
    
        private const float AlphaSpeed = 10f;
    
        public World World { get; set;}
    
        private Filter filter;
        private Stash<HealthComponent> healthStash;
        private Stash<HealthShaderComponent> healthShaderStash;

        public void OnAwake()
        {
            this.filter = this.World.Filter.With<HealthComponent>()
                .With<HealthShaderComponent>().Build();
            this.healthStash = this.World.GetStash<HealthComponent>();
            this.healthShaderStash = this.World.GetStash<HealthShaderComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in this.filter)
            {
                ref var healthComponent = ref healthStash.Get(entity);
                ref var healthShaderComponent = ref healthShaderStash.Get(entity);
                var fillAmount = (float)healthComponent.healthPoint / healthComponent.maxHealthPoint;
                healthShaderComponent.neededFillAmount = fillAmount;

                if (!Mathf.Approximately(healthShaderComponent.neededFillAmount, healthShaderComponent.fillAmount))
                {
                    healthShaderComponent.fillAmount = Mathf.Lerp(healthShaderComponent.neededFillAmount, healthShaderComponent.fillAmount, deltaTime * AlphaSpeed);
                    healthShaderComponent.material.SetFloat(Fill, healthShaderComponent.fillAmount);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}