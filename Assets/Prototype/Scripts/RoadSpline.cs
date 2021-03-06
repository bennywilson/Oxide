using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SplineMesh {

    [ExecuteInEditMode]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class RoadSpline : MonoBehaviour {
        private GameObject generated;
        private Spline spline = null;
        private bool toUpdate = false;
        private bool bFullRebuild = false;

        /// <summary>
        /// A list of object that are storing data for each segment of the curve.
        /// </summary>
        public List<TrackSegment> segments = new List<TrackSegment>();

        /// <summary>
        /// If true, the generated content will be updated in play mode.
        /// If false, the content generated and saved to the scene will be used in playmode without modification.
        /// Usefull to preserve lightmaps baked for static objects.
        /// </summary>
        public bool updateInPlayMode;

        public void MarkDirty(bool doFullRebuild = false)
        {
            toUpdate = true;
            bFullRebuild = doFullRebuild;
        }

        private void OnEnable() {
            string generatedName = "generated by " + GetType().Name;
            var generatedTranform = transform.Find(generatedName);
            generated = generatedTranform != null ? generatedTranform.gameObject : UOUtility.Create(generatedName, gameObject);

            spline = GetComponentInParent<Spline>();

            // we listen changes in the spline's node list and we update the list of segment accordingly
            // this way, if we insert a node between two others, a segment will be inserted too and the data won't shift
            while (segments.Count < spline.nodes.Count) {
                segments.Add(new TrackSegment());
            }

            while (segments.Count > spline.nodes.Count) {
                segments.RemoveAt(segments.Count - 1);
            }

            spline.NodeListChanged += (s, e) => {
                switch (e.type) {
                    case ListChangeType.Add:
                        segments.Add(new TrackSegment());
                        break;
                    case ListChangeType.Remove:
                        segments.RemoveAt(e.removeIndex);
                        break;
                    case ListChangeType.Insert:
                        segments.Insert(e.insertIndex, new TrackSegment());
                        break;
                }
                toUpdate = true;
            };
            toUpdate = true;
        }

        private void OnValidate() {
            if (spline == null) return;
            toUpdate = true;
        }

        private void Update() {
            // we can prevent the generated content to be updated during playmode to preserve baked data saved in the scene
            if (!updateInPlayMode && Application.isPlaying) return;

            if (toUpdate) {
                toUpdate = false;

                CreateMeshes();
                bFullRebuild = false;
            }
        }

        public void CreateMeshes() {
            List<GameObject> used = new List<GameObject>();


            if (bFullRebuild)
            {
                var childTransform = generated.transform.Find("generated by RoadSpline");
                Object.DestroyImmediate(generated);
                generated = UOUtility.Create("generated by RoadSpline", gameObject);
            }

            for (int i = 0; i < spline.GetCurves().Count; i++) {

                var curve = spline.GetCurves()[i];
                TrackSegment CurSeg = segments[i];

                if (CurSeg.transformedMeshes.Count == 0)
                {
                    CurSeg = segments[0];
                }

                foreach (var tm in CurSeg.transformedMeshes) {
                    if (tm.mesh == null) {
                        // if there is no mesh specified for this segment, we ignore it.
                        continue;
                    }

   
                    // we try to find a game object previously generated. this avoids destroying/creating
                    // game objects at each update, wich is faster.
                    var childName = "segment " + i + " mesh " + CurSeg.transformedMeshes.IndexOf(tm);
                    var childTransform = generated.transform.Find(childName);
                    GameObject go;
                    if (childTransform == null || bFullRebuild) {
                        go = UOUtility.Create(childName,
                            generated,
                            typeof(MeshFilter),
                            typeof(MeshRenderer),
                            typeof(MeshBender));
                        go.isStatic = true;
                    } else {
                        go = childTransform.gameObject;
                    }
                    go.GetComponent<MeshRenderer>().material = tm.material;
                    // go.GetComponent<MeshCollider>().material = tm.physicMaterial;

                    // we update the data in the bender. It will decide itself if the bending must be recalculated.
                    MeshBender mb = go.GetComponent<MeshBender>();
                    mb.Source = SourceMesh.Build(tm.mesh)
                        .Translate(tm.translation)
                        .Rotate(Quaternion.Euler(tm.rotation))
                        .Scale(tm.scale);
                    mb.SetInterval(curve);
                    mb.ComputeIfNeeded();
                    used.Add(go);
                }
            }

            // finally, we destroy the unused objects
            foreach (var go in generated.transform
                .Cast<Transform>()
                .Select(child => child.gameObject).Except(used)) {
                UOUtility.Destroy(go);
            }
        }
    }
}

