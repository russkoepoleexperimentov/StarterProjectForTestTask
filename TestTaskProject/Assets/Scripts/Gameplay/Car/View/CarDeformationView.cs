using System.Collections.Generic;
using System.Linq;
using Gameplay.Car.Configs;
using UnityEngine;
using Zenject;

namespace Gameplay.Car.View
{
    // вершинная деформация визуальных мешей кузова. Физический коллайдер не меняется -
    // вмятины чисто косметические.
    public class CarDeformationView : MonoBehaviour
    {
        private class DeformTarget
        {
            public Transform Transform;
            public Mesh Mesh;
            public Vector3[] Original;
            public Vector3[] Current;
        }

        [SerializeField] private Transform _deformationRoot;
        [SerializeField] private MeshFilter[] _targets;

        private readonly List<DeformTarget> _deformTargets = new();

        private CarDamageConfig _config;

        [Inject]
        public void Init(CarDamageConfig config)
        {
            _config = config;
        }

        public void Deform(Vector3 worldPoint, Vector3 worldNormal, float force01)
        {
            if (_config == null || force01 <= 0f) return;

            foreach (var target in _deformTargets)
            {
                var localPoint = target.Transform.InverseTransformPoint(worldPoint);
                var localRadius = ToLocalRadius(target.Transform, _config.DeformationRadius);

                if (localRadius <= 0f) continue;
                if (target.Mesh.bounds.SqrDistance(localPoint) > localRadius * localRadius) continue;

                // вминаем против нормали поверхности, то есть внутрь кузова
                var localDirection = target.Transform.InverseTransformDirection(-worldNormal).normalized;
                var amount = _config.DeformationStrength * force01;

                var falloffCurve = _config.DeformationFalloff;
                var changed = false;

                for (var i = 0; i < target.Current.Length; i++)
                {
                    var distance = Vector3.Distance(target.Current[i], localPoint);

                    if (distance > localRadius) continue;

                    var t = distance / localRadius;
                    // пустая кривая в ассете не должна отключать деформацию целиком
                    var falloff = falloffCurve != null && falloffCurve.length > 0
                        ? falloffCurve.Evaluate(t)
                        : 1f - t;

                    if (falloff <= 0f) continue;

                    var offset = localDirection * (amount * falloff)
                                 + Random.insideUnitSphere * (_config.DeformationNoise * falloff);

                    var total = target.Current[i] + offset - target.Original[i];

                    // копить смещение без предела нельзя - иначе машину сминает в точку
                    if (total.magnitude > _config.MaxVertexOffset)
                        total = total.normalized * _config.MaxVertexOffset;

                    target.Current[i] = target.Original[i] + total;
                    changed = true;
                }

                if (!changed) continue;

                target.Mesh.SetVertices(target.Current);
                target.Mesh.RecalculateNormals();
                target.Mesh.RecalculateBounds();
            }
        }

        private void Awake()
        {
            if (_targets == null) return;

            foreach (var filter in _targets)
            {
                if (filter == null || filter.sharedMesh == null) continue;

                if (!filter.sharedMesh.isReadable)
                {
                    Debug.LogWarning($"Меш {filter.sharedMesh.name} не читается в рантайме — " +
                                     "включите Read/Write Enabled у модели, иначе деформации не будет.", filter);
                    continue;
                }

                // копия меша: общий ассет трогать нельзя, иначе повреждения переживут Play-mode
                var mesh = Instantiate(filter.sharedMesh);
                mesh.MarkDynamic();
                filter.mesh = mesh;

                var vertices = mesh.vertices;

                _deformTargets.Add(new DeformTarget
                {
                    Transform = filter.transform,
                    Mesh = mesh,
                    Original = vertices,
                    Current = (Vector3[])vertices.Clone()
                });
            }
        }

        private void OnDestroy()
        {
            foreach (var target in _deformTargets)
                Destroy(target.Mesh);

            _deformTargets.Clear();
        }

        private static float ToLocalRadius(Transform target, float worldRadius)
        {
            var scale = target.lossyScale;
            var maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

            return maxScale > Mathf.Epsilon ? worldRadius / maxScale : 0f;
        }

        private void Reset()
        {
            if (_deformationRoot == null) return;

            // колёса крутятся и переставляются скриптом - деформировать их незачем
            _targets = _deformationRoot.GetComponentsInChildren<MeshFilter>()
                .Where(filter => filter.sharedMesh != null)
                .Where(filter => !filter.name.ToLowerInvariant().Contains("wheel"))
                .ToArray();
        }
    }
}
