using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace SplineMesh {

    [CustomEditor(typeof(RoadSpline))]
    public class RoadSplineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RoadSpline rs = serializedObject.targetObject as RoadSpline;
            EditorGUILayout.LabelField("Segments");
            SerializedProperty serializedSegments = serializedObject.FindProperty("segments");

            List<int> selectedIndex = new List<int>();

            bool ReplacePrefabs = false;
            if (GUILayout.Button("Match segments to First"))
            {
                Undo.RecordObject(rs, "Match segments to First");
                ReplacePrefabs = true;
            }

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var name = Selection.objects[i].name.Substring(8, Selection.objects[i].name.Length - 8);
                if (name == String.Empty)
                {
                    continue;
                }

                int lastIdx = name.IndexOf(' ');
                if (lastIdx < 0)
                {
                    continue;
                }

                name = name.Substring(0, lastIdx);
                int outInt;
                Int32.TryParse(name, out outInt);
                selectedIndex.Add(outInt);
            }
            
            int[] segmentToControlID = new int[rs.segments.Count];
            TrackSegment firstSegment = null;

            for (int i = 0; i < rs.segments.Count; i++)
            {
                Color oldColor = GUI.color;
                if (selectedIndex.Contains(i))
                {
                    if (firstSegment == null)
                    {
                        GUI.color = new Color(1.0f, 2.0f, 1.0f);
                        firstSegment = rs.segments[i];
                    }
                    else
                    {
                        GUI.color = new Color(1.0f, 1.0f, 2.0f);
                        if (ReplacePrefabs)
                        {
                            firstSegment.CopySegment(rs.segments[i]);
                        }
                    }
                }

                var customLabel = new GUIContent("Index[" + i + "]");
                var controlId = EditorGUIUtility.GetControlID(customLabel, FocusType.Passive);
                segmentToControlID[i] = controlId;
                SerializedProperty curSegment = serializedSegments.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(curSegment, customLabel);
                GUI.color = oldColor;
            }
            if (ReplacePrefabs)
            {
                rs.MarkDirty(true);
                serializedObject.Update();
                EditorUtility.SetDirty(target);
            }
        }
    }
        [CustomEditor(typeof(Spline))]
    public class SplineEditor : Editor {

        // todo: Move to utility file?
        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public readonly Vector3 UpEpsilon = new Vector3(0.0f, 0.01f, 0.0f);
        // end

        private const int QUAD_SIZE = 12;
        private static Color CURVE_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color CURVE_BUTTON_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color DIRECTION_COLOR = Color.red;
        private static Color DIRECTION_BUTTON_COLOR = Color.red;
        private static Color UP_BUTTON_COLOR = Color.green;

        private static bool showUpVector = false;

        private enum SelectionType {
            Node,
            Direction,
            InverseDirection,
            Up
        }


        private SplineNode firstSelection;
        private SelectionType selectionType;

        private SplineNode lastSelection;

        private bool mustCreateNewNode = false;
        private SerializedProperty nodesProp { get { return serializedObject.FindProperty("nodes"); } }
        private Spline spline { get { return (Spline)serializedObject.targetObject; } }

        private GUIStyle nodeButtonStyle, directionButtonStyle, upButtonStyle, frontNodeButtonStyle, lastSelectedNodeButtonStyle;

        private void OnEnable() {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, CURVE_BUTTON_COLOR);
            t.Apply();
            nodeButtonStyle = new GUIStyle();
            nodeButtonStyle.normal.background = t;

            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, Color.green);
            t.Apply();
            frontNodeButtonStyle = new GUIStyle();
            frontNodeButtonStyle.normal.background = t;

            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, new Color(0.0f, 0.0f, 1.0f));
            t.Apply();
            lastSelectedNodeButtonStyle = new GUIStyle();
            lastSelectedNodeButtonStyle.normal.background = t;

            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, DIRECTION_BUTTON_COLOR);
            t.Apply();
            directionButtonStyle = new GUIStyle();
            directionButtonStyle.normal.background = t;

            t = new Texture2D(1, 1);
            t.SetPixel(0, 0, UP_BUTTON_COLOR);
            t.Apply();
            upButtonStyle = new GUIStyle();
            upButtonStyle.normal.background = t;
            firstSelection = null;
            lastSelection = null;

            Undo.undoRedoPerformed -= spline.RefreshCurves;
            Undo.undoRedoPerformed += spline.RefreshCurves;
        }

        SplineNode AddClonedNode(SplineNode node) {
            int index = spline.nodes.IndexOf(node);
            SplineNode res = new SplineNode(node.Position, node.Direction);
            if (index == spline.nodes.Count - 1) {
                spline.AddNode(res);
            } else {
                spline.InsertNode(index + 1, res);
            }
            return res;
        }

        void OnSceneGUI() {

            // disable game object transform gyzmo
            // if the spline script is active
            if (Selection.activeGameObject == spline.gameObject) {
                if (!spline.enabled) {
                    Tools.current = Tool.Move;
                } else {
                    Tools.current = Tool.None;
                    if (firstSelection == null && spline.nodes.Count > 0)
                        firstSelection = spline.nodes[0];
                }
            }

            // draw a bezier curve for each curve in the spline
            foreach (CubicBezierCurve curve in spline.GetCurves()) {
                Handles.DrawBezier(spline.transform.TransformPoint(curve.n1.Position),
                    spline.transform.TransformPoint(curve.n2.Position),
                    spline.transform.TransformPoint(curve.n1.Direction),
                    spline.transform.TransformPoint(curve.GetInverseDirection()),
                    CURVE_COLOR,
                    null,
                    3);
            }

            if (!spline.enabled)
                return;

            if (firstSelection != null)
            {
                // draw the selection handles
                switch (selectionType)
                {
                    case SelectionType.Node:
                        // place a handle on the node and manage position change

                        // TODO place the handle depending on user params (local or world)
                        Vector3 newPosition = spline.transform.InverseTransformPoint(Handles.PositionHandle(spline.transform.TransformPoint(firstSelection.Position), spline.transform.rotation));
                        if (newPosition != firstSelection.Position)
                        {
                            // position handle has been moved
                            if (mustCreateNewNode)
                            {
                                mustCreateNewNode = false;
                                firstSelection = AddClonedNode(firstSelection);
                                firstSelection.Direction += newPosition - firstSelection.Position;
                                firstSelection.Position = newPosition;
                            }
                            else
                            {
                                firstSelection.Direction += newPosition - firstSelection.Position;
                                firstSelection.Position = newPosition;
                            }
                        }
                        break;
                    case SelectionType.Direction:
                        var result = Handles.PositionHandle(spline.transform.TransformPoint(firstSelection.Direction), Quaternion.identity);
                        firstSelection.Direction = spline.transform.InverseTransformPoint(result);
                        break;
                    case SelectionType.InverseDirection:
                        result = Handles.PositionHandle(2 * spline.transform.TransformPoint(firstSelection.Position) - spline.transform.TransformPoint(firstSelection.Direction), Quaternion.identity);
                        firstSelection.Direction = 2 * firstSelection.Position - spline.transform.InverseTransformPoint(result);
                        break;
                    case SelectionType.Up:
                        result = Handles.PositionHandle(spline.transform.TransformPoint(firstSelection.Position + firstSelection.Up), Quaternion.LookRotation(firstSelection.Direction - firstSelection.Position));
                        firstSelection.Up = (spline.transform.InverseTransformPoint(result) - firstSelection.Position).normalized;
                        break;
                }
            }

            // draw the handles of all nodes, and manage selection motion
            Handles.BeginGUI();
            foreach (SplineNode n in spline.nodes) {
                var dir = spline.transform.TransformPoint(n.Direction);
                var pos = spline.transform.TransformPoint(n.Position);
                var invDir = spline.transform.TransformPoint(2 * n.Position - n.Direction);
                var up = spline.transform.TransformPoint(n.Position + n.Up);
                // first we check if at least one thing is in the camera field of view
                if (!(CameraUtility.IsOnScreen(pos) ||
                    CameraUtility.IsOnScreen(dir) ||
                    CameraUtility.IsOnScreen(invDir) ||
                    (showUpVector && CameraUtility.IsOnScreen(up)))) {
                    continue;
                }

                Vector3 guiPos = HandleUtility.WorldToGUIPoint(pos);
                if (n == firstSelection) {
                    Vector3 guiDir = HandleUtility.WorldToGUIPoint(dir);
                    Vector3 guiInvDir = HandleUtility.WorldToGUIPoint(invDir);
                    Vector3 guiUp = HandleUtility.WorldToGUIPoint(up);

                    // for the selected node, we also draw a line and place two buttons for directions
                    Handles.color = DIRECTION_COLOR;
                    Handles.DrawLine(guiDir, guiInvDir);

                    // draw quads direction and inverse direction if they are not selected
                    if (selectionType != SelectionType.Node) {
                        if (Button(guiPos, directionButtonStyle) > 0) {
                            selectionType = SelectionType.Node;
                        }
                    }
                    if (selectionType != SelectionType.Direction) {
                        if (Button(guiDir, directionButtonStyle) > 0) {
                            selectionType = SelectionType.Direction;
                        }
                    }
                    if (selectionType != SelectionType.InverseDirection) {
                        if (Button(guiInvDir, directionButtonStyle) > 0) {
                            selectionType = SelectionType.InverseDirection;
                        }
                    }
                    if (showUpVector) {
                        Handles.color = Color.green;
                        Handles.DrawLine(guiPos, guiUp);
                        if (selectionType != SelectionType.Up) {
                            if (Button(guiUp, upButtonStyle) > 0) {
                                selectionType = SelectionType.Up;
                            }
                        }
                    }
                } else {
                    if (n == spline.nodes[0])
                    {
                        int buttonClick = Button(guiPos, frontNodeButtonStyle);
                        if (buttonClick == 1)
                        {
                            firstSelection = n;
                            selectionType = SelectionType.Node;
                        }
                        else if (buttonClick == 2)
                        {
                            lastSelection = n;
                        }
                    }
                    else
                    {
                        int buttonClick = Button(guiPos, (n == lastSelection) ? (lastSelectedNodeButtonStyle) : (nodeButtonStyle));
                        if (buttonClick == 1)
                        {
                            firstSelection = n;
                            selectionType = SelectionType.Node;
                        }
                        else if (buttonClick == 2)
                        {
                            lastSelection = n;
                        }
                    }
                }
            }
            Handles.EndGUI();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        int Button(Vector2 position, GUIStyle style) {
            if (GUI.Button(new Rect(position - new Vector2(QUAD_SIZE / 2, QUAD_SIZE / 2), new Vector2(QUAD_SIZE, QUAD_SIZE)), GUIContent.none, style))
            {
                if (Event.current.button == 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }

            return 0;
        }

        List<TrackSegment> a;

        public override void OnInspectorGUI() {
            serializedObject.Update();


    
            if (spline.nodes.IndexOf(firstSelection) < 0) {
                firstSelection = null;
            }

            // add button
            if (firstSelection == null) {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Add node after selected")) {
                Undo.RecordObject(spline, "add spline node");
                SplineNode newNode = new SplineNode(firstSelection.Direction, firstSelection.Direction + firstSelection.Direction - firstSelection.Position);
                var index = spline.nodes.IndexOf(firstSelection);
    
                if(index == spline.nodes.Count - 1) {
                    spline.AddNode(newNode);
                } else {
                    spline.InsertNode(index + 1, newNode);
                }
                firstSelection = newNode;
                serializedObject.Update();
            }
            GUI.enabled = true;

            // delete button
            if (firstSelection == null || spline.nodes.Count <= 2) {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Delete selected node")) {
                Undo.RecordObject(spline, "delete spline node");
                spline.RemoveNode(firstSelection);
                firstSelection = null;
                serializedObject.Update();
            }
            GUI.enabled = true;

            if (firstSelection != null && lastSelection != null)
            {
                int firstIndex = spline.nodes.IndexOf(firstSelection);
                int lastIndex = spline.nodes.IndexOf(lastSelection);

                if (lastIndex < firstIndex)
                {
                    Swap(ref firstIndex, ref lastIndex);
                }

                Vector3 FirstPosition = firstSelection.Position;
                Vector3 FirstDirection = (lastSelection.Position - firstSelection.Position).normalized;
                float distBetweenNodes = ((firstSelection.Position - lastSelection.Position).magnitude) / (lastIndex - firstIndex);

                bool bSplineUpdated = false;
                GameObject WorldBuilder = GameObject.Find("WorldBuilder");
                Transform roadPiece = WorldBuilder.transform.Find("Generated World Objects/ProceduralRoadPiece(Clone)");
                RoadSpline roadSpline = roadPiece.GetComponent<RoadSpline>();
                if (GUILayout.Button("Place selected objects along road"))
                {
                    if (Selection.activeGameObject != null)
                    {
                        if (WorldBuilder != null)
                        {
                            WorldBuilder.GetComponent<WorldBuilder>().PlacePropAlongSpline(firstIndex, lastIndex, Selection.activeGameObject);
                        }
                    }
                }
                else if (GUILayout.Button("Replace range with selected prefab"))
                {
                    var so = new SerializedObject(roadSpline);
                    var segs = so.FindProperty("segments");

                    for (int i = firstIndex + 1; i < lastIndex; i++)
                    {
                        roadSpline.segments[firstIndex].CopySegment(roadSpline.segments[i]);
                    }

                    roadSpline.MarkDirty();

                    bSplineUpdated = true;
                }
                else if (GUILayout.Button("Align range to first"))
                {
                    Undo.RecordObject(spline, "Align range to first");

                    for (int i = firstIndex + 1; i < lastIndex; i++)
                    {
                        SplineNode curNode = spline.nodes[i];
                        curNode.Position = FirstPosition + FirstDirection * (i - firstIndex) * distBetweenNodes;
                    }

                    bSplineUpdated = true;
                }
                else if (GUILayout.Button("Align to ground"))
                {
                    Undo.RecordObject(spline, "Align to ground");
                    Debug.Log("Last Index = " + lastIndex);
                    for (int i = firstIndex; i <= lastIndex && i < spline.nodes.Count; i++)
                    {
                        Vector3 rayStart = spline.nodes[i].Position + new Vector3(0.0f, 20.0f, 0.0f);
                        RaycastHit hitInfo = new RaycastHit();
                        if (Physics.Linecast(rayStart, spline.nodes[i].Position - new Vector3(0.0f, 20.0f, 0.0f), out hitInfo))
                        {
                            spline.nodes[i].Position = hitInfo.point + UpEpsilon;
                        }
                    }

                    bSplineUpdated = true;
                }
                else
                {
                    if (GUILayout.Button("Align range Along X"))
                    {
                        Undo.RecordObject(spline, "Align range Along X");

                        for (int i = firstIndex + 1; i <= lastIndex && i < spline.nodes.Count; i++)
                        {
                            SplineNode curNode = spline.nodes[i];
                            curNode.Position = new Vector3(FirstPosition.x, curNode.Position.y, curNode.Position.z);
                        }

                        bSplineUpdated = true;
                    }
                    else if (GUILayout.Button("Align range Along Y"))
                    {
                        Undo.RecordObject(spline, "Align range Along Y");

                        for (int i = firstIndex + 1; i <= lastIndex && i < spline.nodes.Count; i++)
                        {
                            SplineNode curNode = spline.nodes[i];
                            curNode.Position = new Vector3(curNode.Position.x, FirstPosition.y, curNode.Position.z);
                        }

                        bSplineUpdated = true;
                    }
                }

                if (bSplineUpdated)
                {
                    for (int i = firstIndex; i <= lastIndex && i < spline.nodes.Count; i++)
                    {
                        SplineNode curNode = spline.nodes[i];
                        if (i < lastIndex - 1)
                        {
                            curNode.Direction = curNode.Position + (spline.nodes[i + 1].Position - spline.nodes[i].Position).normalized;
                        }
                        else
                        {
                            curNode.Direction = curNode.Position + (spline.nodes[i].Position - spline.nodes[i - 1].Position).normalized;
                        }
                    }

                    serializedObject.Update();
                    EditorUtility.SetDirty(target);
                }
            }

            showUpVector = GUILayout.Toggle(showUpVector, "Show up vector");
            spline.IsLoop = GUILayout.Toggle(spline.IsLoop, "Is loop");

            // nodes
            GUI.enabled = false;
            EditorGUILayout.PropertyField(nodesProp);
            GUI.enabled = true;

            if (firstSelection != null) {
                int index = spline.nodes.IndexOf(firstSelection);
                SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(index);

                EditorGUILayout.LabelField("Selected node (node " + index + ")");

                EditorGUI.indentLevel++;
                DrawNodeData(nodeProp, firstSelection);
                EditorGUI.indentLevel--;
            } else {
                EditorGUILayout.LabelField("No selected node");
            }
        }

        private void DrawNodeData(SerializedProperty nodeProperty, SplineNode node) {
            var positionProp = nodeProperty.FindPropertyRelative("position");
            var directionProp = nodeProperty.FindPropertyRelative("direction");
            var upProp = nodeProperty.FindPropertyRelative("up");
            var scaleProp = nodeProperty.FindPropertyRelative("scale");
            var rollProp = nodeProperty.FindPropertyRelative("roll");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(positionProp, new GUIContent("Position"));
            EditorGUILayout.PropertyField(directionProp, new GUIContent("Direction"));
            EditorGUILayout.PropertyField(upProp, new GUIContent("Up"));
            EditorGUILayout.PropertyField(scaleProp, new GUIContent("Scale"));
            EditorGUILayout.PropertyField(rollProp, new GUIContent("Roll"));

            if (EditorGUI.EndChangeCheck()) {
                node.Position = positionProp.vector3Value;
                node.Direction = directionProp.vector3Value;
                node.Up = upProp.vector3Value;
                node.Scale = scaleProp.vector2Value;
                node.Roll = rollProp.floatValue;
                serializedObject.Update();
            }
        }

        [MenuItem("GameObject/3D Object/Spline")]
        public static void CreateSpline() {
            new GameObject("Spline", typeof(Spline));
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        static void DisplayUnselected(Spline spline, GizmoType gizmoType) {
            foreach (CubicBezierCurve curve in spline.GetCurves()) {
                Handles.DrawBezier(spline.transform.TransformPoint(curve.n1.Position),
                    spline.transform.TransformPoint(curve.n2.Position),
                    spline.transform.TransformPoint(curve.n1.Direction),
                    spline.transform.TransformPoint(curve.GetInverseDirection()),
                    CURVE_COLOR,
                    null,
                    3);
            }
        }
    }
}
