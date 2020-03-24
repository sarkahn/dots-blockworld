using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace BlockGame.BlockWorld
{
    public class MapGenerationSettingsUI : MonoBehaviour
    {
        [SerializeField]
        Slider _iterationsSlider = default;

        [SerializeField]
        Slider _maxHeightSlider = default;

        [SerializeField]
        Slider _scaleSlider = default;

        [SerializeField]
        Slider _persistenceSlider = default;

        [SerializeField]
        Slider _rangeSlider = default;

        GenerateRegionHeightMapSettings _settings = GenerateRegionHeightMapSettings.Default;
        public GenerateRegionHeightMapSettings Settings => _settings;

        public int _range = 15;

        EntityQuery _regionsQuery;

        bool _regenerate;

        private void OnEnable()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _regionsQuery = em.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(Region) },
                },
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(Region), typeof(Disabled) },
                },
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(ChunkBlockType) },
                },
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(ChunkBlockType), typeof(Disabled) },
                });

            _iterationsSlider.onValueChanged.AddListener((f) =>
            {
                _regenerate = true;
                _settings.iterations = (int)f;
            });
            _iterationsSlider.value = _settings.iterations;

            _maxHeightSlider.onValueChanged.AddListener((f) =>
            {
                _regenerate = true;
                _settings.maxHeight = (int)f;
            });
            _maxHeightSlider.value = _settings.maxHeight;

            _scaleSlider.onValueChanged.AddListener((f) =>
            {
                _regenerate = true;
                _settings.scale = f;
            });
            _scaleSlider.value = _settings.scale;

            _persistenceSlider.onValueChanged.AddListener((f) =>
            {
                _regenerate = true;
                _settings.persistence = f;
            });
            _persistenceSlider.value = _settings.persistence;

            _rangeSlider.onValueChanged.AddListener((f) =>
            {
                _regenerate = true;
                _range = (int)f;
            });

        }

        private void Update()
        {
            if( _regenerate )
            {
                Debug.Log($"REGENERATING {_regionsQuery.CalculateEntityCount()} entities...");
                _regenerate = false;

                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                em.CompleteAllJobs();
                em.DestroyEntity(_regionsQuery);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _regenerate = true;
        }
#endif

    } 
}
