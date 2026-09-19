using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using System.Collections.Generic;
using System.Text;

namespace Kamgam.PowerPivot
{
    partial class PowerPivotTool
    {
        public enum CursorUpdateCause { Unknown, UndoPerformed, PivotRotationChanged, PivotModeChanged, SelectionChanged, ToolChanged }

        // Cursor position:
        // We cache one position per selection. This way the user does not have to
        // reposition the cursor every time the selection changes.
        protected Dictionary<string, Vector3> cursorPositionCache = new Dictionary<string, Vector3>();
        protected Dictionary<string, Vector3> cursorLocalPositionCache = new Dictionary<string, Vector3>();
        
        // Maybe in a future update.
        // protected Dictionary<string, Quaternion> cursorRotationCache = new Dictionary<string, Quaternion>();
        // protected Dictionary<string, Quaternion> cursorLocalRotationCache = new Dictionary<string, Quaternion>();
        
        protected StringBuilder cursorCacheKeyBuilder = new StringBuilder();
        protected string cursorCacheKey;

        /// <summary>
        /// Global cursor position.
        /// </summary>
        protected Vector3 cursorPosition
        {
            get
            {
                initCursorCacheKeyIfneeded();
                var pos = getCursorPositionFromCache(cursorCacheKey);
                if (pos.HasValue)
                {
                    return pos.Value;
                }
                else
                {
                    return Vector3.zero;
                }
            }

            set
            {
                initCursorCacheKeyIfneeded();
                setCursorPositionInCache(cursorCacheKey, value);
            }
        }

        /// <summary>
        /// Global cursor rotation.
        /// </summary>
        protected Quaternion cursorRotation;
        
        // Maybe in a future update. For now rotation changes are limited to pivot editing.
        // {
        //     get
        //     {
        //         initCursorCacheKeyIfneeded();
        //         var rot = getCursorRotationFromCache(cursorCacheKey);
        //         if (rot.HasValue)
        //         {
        //             return rot.Value;
        //         }
        //         else
        //         {
        //             return Quaternion.identity;
        //         }
        //     }
        // 
        //     set
        //     {
        //         initCursorCacheKeyIfneeded();
        //         setCursorRotationInCache(cursorCacheKey, value);
        //     }
        // }

        // Used for continuous input of rotation (avoid gimbal lock)
        protected Vector3 cursorEulerRotationInput;

        /// <summary>
        /// The cursors relative position to the last selected object (or the reference object if multiple have been selected).
        /// This is used to restore the cursor position on Undo.
        /// </summary>
        protected Vector3? cursorRelativePositionLocal = null;

        protected Vector3? cursorRelativePositionWorld
        {
            get
            {
                if (!cursorRelativePositionLocal.HasValue)
                    return null;

                if (Selection.activeGameObject == null)
                    return null;

                return Selection.activeGameObject.transform.TransformPoint(cursorRelativePositionLocal.Value);
            }
        }
        
        /// <summary>
        /// The cursors relative rotation to the last selected object (or the reference object if multiple have been selected).
        /// This is used to restore the cursor Rotation on Undo.
        /// </summary>
        protected Quaternion? cursorRelativeRotationLocal = null;

        protected Quaternion? cursorRelativeRotationWorld
        {
            get
            {
                if (!cursorRelativeRotationLocal.HasValue)
                    return null;

                if (Selection.activeGameObject == null)
                    return null;

                return Selection.activeGameObject.transform.TransformRotation(cursorRelativeRotationLocal.Value);
            }
        }

        public void ClearCursorCache()
        {
            cursorPositionCache.Clear();
            cursorLocalPositionCache.Clear();
            
            // Maybe in a future update.
            // cursorRotationCache.Clear();
            // cursorLocalRotationCache.Clear();
        }

        protected void initCursorCacheKeyIfneeded()
        {
            if (cursorCacheKey == null)
            {
                cursorCacheKey = getCursorCacheKey(Selection.gameObjects);
            }
        }

        protected string getCursorCacheKey(IEnumerable<GameObject> gameObjects)
        {
            cursorCacheKeyBuilder.Clear();
            foreach (var go in gameObjects)
            {
#if UNITY_6000_5_OR_NEWER
                cursorCacheKeyBuilder.Append(EntityId.ToULong(go.GetEntityId()));
#else
                cursorCacheKeyBuilder.Append(go.GetInstanceID());
#endif
                
            }

            // In case we want to cache different cursor positions for mode/rotations.
            // objectsKeyBuilder.Append(Tools.pivotMode == PivotMode.Center ? "c" : "p");
            // objectsKeyBuilder.Append(Tools.pivotRotation == PivotRotation.Local ? "l" : "g");

            // avoid long keys by hashing if necessary
            if (cursorCacheKeyBuilder.Length <= 40)
                return cursorCacheKeyBuilder.ToString();
            else
                return UtilsHash.SHA1(cursorCacheKeyBuilder.ToString());
        }

        protected Vector3? getCursorPositionFromCache(string key)
        {
            if (cursorPositionCache.ContainsKey(key))
                return cursorPositionCache[key];
            else
                return null;
        }

        protected Vector3? getCursorLocalPositionFromCache(string key)
        {
            if (cursorLocalPositionCache.ContainsKey(key))
                return cursorLocalPositionCache[key];
            else
                return null;
        }

        protected void setCursorPositionInCache(string key, Vector3 position)
        {
            // store as local position in cache
            var go = target as GameObject;
            if (go != null)
            {
                var localPosition = go.transform.InverseTransformPoint(position);
                cursorLocalPositionCache[key] = localPosition;
            }

            // store in cache
            cursorPositionCache[key] = position;
        }

        protected void removeCursorPositionInCache(string key)
        {
            if (cursorPositionCache.ContainsKey(key))
                cursorPositionCache.Remove(key);

            if (cursorLocalPositionCache.ContainsKey(key))
                cursorLocalPositionCache.Remove(key);
        }
        
        // Maybe in a future update
        //protected Quaternion? getCursorRotationFromCache(string key)
        //{
        //    if (cursorRotationCache.ContainsKey(key))
        //        return cursorRotationCache[key];
        //    else
        //        return null;
        //}
        //
        //protected Quaternion? getCursorLocalRotationFromCache(string key)
        //{
        //    if (cursorLocalRotationCache.ContainsKey(key))
        //        return cursorLocalRotationCache[key];
        //    else
        //        return null;
        //}
        //
        //protected void setCursorRotationInCache(string key, Quaternion rotation)
        //{
        //    // store as local rotation in cache
        //    var go = target as GameObject;
        //    if (go != null)
        //    {
        //        var localRotation = go.transform.InverseTransformRotation(rotation);
        //        cursorLocalRotationCache[key] = localRotation;
        //    }
        //
        //    // store in cache
        //    cursorRotationCache[key] = rotation;
        //}
        //
        //protected void removeCursorRotationInCache(string key)
        //{
        //    if (cursorRotationCache.ContainsKey(key))
        //        cursorRotationCache.Remove(key);
        //
        //    if (cursorLocalRotationCache.ContainsKey(key))
        //        cursorLocalRotationCache.Remove(key);
        //}

        // Called from within OnGUI
        protected void drawCursorGizmo(SceneView sceneView)
        {
            var settings = PowerPivotSettings.GetOrCreateSettings();
            var colorA = settings.GizmoColorA;
            var colorB = settings.GizmoColorB;
            colorA.a = settings.GizmoOpacity;
            colorB.a = settings.GizmoOpacity;

            // draw cursor indicator
            Quaternion rot = Quaternion.identity;
            if ((sceneView.camera.transform.position - cursorPosition).sqrMagnitude > 0)
                rot = Quaternion.LookRotation(sceneView.camera.transform.position - cursorPosition);
            var size = HandleUtility.GetHandleSize(cursorPosition);

            // dot
            drawCircle(cursorPosition, rot, size * 0.01f, colorB, colorB, 3);
            // circles
            if (currentTool != PTool.Cursor)
            {
                drawCircle(cursorPosition, rot, size * 0.20f, colorA, colorB, 10);
                drawCircle(cursorPosition, rot, size * 0.19f, colorA, colorB, 10);
                drawCircle(cursorPosition, rot, size * 0.18f, colorA, colorB, 10);
            }
        }

        // Called from within OnGUI
        protected void drawOriginalCursorGizmo(Transform transform)
        {
            // don't draw if cursor pos is too close
            if (currentTool != PTool.Rotate && currentTool != PTool.Cursor && Vector3.SqrMagnitude(transform.position - cursorPosition) < 0.01f)
                return;

            // draw cursor indicator
            var size = HandleUtility.GetHandleSize(transform.position);

            var up = transform.TransformDirection(Vector3.up).normalized;
            var right = transform.TransformDirection(Vector3.right).normalized;
            var forward = transform.TransformDirection(Vector3.forward).normalized;

            Handles.color = Color.green;
            Handles.DrawDottedLine(transform.position, transform.position + up * size * 0.33f, 1f);

            Handles.color = Color.red;
            Handles.DrawDottedLine(transform.position, transform.position + right * size * 0.33f, 1f);

            Handles.color = Color.blue;
            Handles.DrawDottedLine(transform.position, transform.position + forward * size * 0.33f, 1f);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="duration">Set to 0 in Editor to keep it for one frame.</param>
        /// <param name="circleSegments"></param>
        void drawCircle(Vector3 center, Quaternion rot, float radius, Color colorA, Color colorB,  int circleSegments = 10)
        {
            var col = Handles.color;

            // getCirclePoints
            var angleInRad = 0f;  // angle that will be increased each loop
            var step = Mathf.PI * 2f / circleSegments;
            Vector3 point = Vector3.zero;
            Vector3 previousPoint = Vector3.zero;
            Color color;
            int i;
            for (int x = 0; x <= circleSegments; ++x)
            {
                i = x % circleSegments;
                color = (i % 2 == 0 ? colorA : colorB);
                Handles.color = color;
                point.x = radius * Mathf.Cos(angleInRad);
                point.y = radius * Mathf.Sin(angleInRad);
                point = rot * point;
                angleInRad += step;
                if (x > 0)
                {
                    Handles.DrawLine(center + previousPoint, center + point);
                }
                previousPoint = point;
            }

            Handles.color = col;
        }

        void updateCursor(CursorUpdateCause reason = CursorUpdateCause.Unknown, bool forceRefresh = false)
        {
            // restore from local cache
            if (reason == CursorUpdateCause.ToolChanged || reason == CursorUpdateCause.SelectionChanged)
            {
                restoreCursorFromLocalPosition();
            }

            // Cache key
            cursorCacheKey = getCursorCacheKey(Selection.gameObjects);
            
            // update cursor position (or read from cache)
            var cachedPos = getCursorPositionFromCache(cursorCacheKey);
            if (cachedPos.HasValue && !forceRefresh)
            {
                cursorPosition = cachedPos.Value;
            }
            else
            {
                cursorPosition = calculateDefaultHandlePivotPosition();
                setCursorPositionInCache(cursorCacheKey, cursorPosition);
            }

            // rotation
            // rotation
            if (Selection.gameObjects.Length > 0 && Selection.activeGameObject != null)
                cursorRotation = Tools.pivotRotation == PivotRotation.Local ? Selection.activeGameObject.transform.rotation : Quaternion.identity;
            else
                cursorRotation = Quaternion.identity;
            
            // Maybe in a future update.
            // var cachedRot = getCursorRotationFromCache(cursorCacheKey);
            // if (cachedRot.HasValue && !forceRefresh)
            // {
            //     cursorRotation = cachedRot.Value;
            // }
            // else
            // {
            //     cursorRotation = calculateDefaultHandlePivotRotation();
            //     setCursorRotationInCache(cursorCacheKey, cursorRotation);
            // }

            if (reason == CursorUpdateCause.SelectionChanged)
            {
                updateCursorRelativePosition();
                ClearCursorUndo();
            }
            // Restore gizmo position after undo
            else if (reason == CursorUpdateCause.UndoPerformed)
            {
                if (cursorRelativePositionWorld.HasValue)
                {
                    cursorPosition = cursorRelativePositionWorld.Value;
                }
            }
        }

        /// <summary>
        /// Whenever cursorPosition is set we also store that position as a localPosition relative
        /// to the current target GameObject. Now this method takes that localPosition, converts
        /// it back to a global position and set cursorPosition to that position.
        /// </summary>
        void restoreCursorFromLocalPosition()
        {
            var settings = PowerPivotSettings.GetOrCreateSettings();

            if (settings.UpdateCursorWithObject == PowerPivotSettings.UpdateCursorOptions.Never)
                return;

            // do not restore if the current rotation mode is global
            if (Tools.pivotRotation == PivotRotation.Global && settings.UpdateCursorWithObject == PowerPivotSettings.UpdateCursorOptions.OnlyIfLocal)
                return;

            string key = getCursorCacheKey(Selection.gameObjects);
            var localPos = getCursorLocalPositionFromCache(key);

            if (!localPos.HasValue)
                return;

            var go = target as GameObject;

            if (UtilsEditor.IsNotInScene(go))
                return;

            setCursorPositionInCache(key, go.transform.TransformPoint(localPos.Value));
        }

        void updateCursorRelativePosition()
        {
            if (Selection.gameObjects.Length == 0 || Selection.activeGameObject == null)
            {
                cursorRelativePositionLocal = null;
            }
            else
            {
                cursorRelativePositionLocal = Selection.activeGameObject.transform.InverseTransformPoint(cursorPosition);
            }
        }
        
        void updateCursorRelativeRotation()
        {
            if (Selection.gameObjects.Length == 0 || Selection.activeGameObject == null)
            {
                cursorRelativeRotationLocal = null;
            }
            else
            {
                cursorRelativeRotationLocal = Selection.activeGameObject.transform.InverseTransformRotation(cursorRotation);
            }
        }

        /// <summary>
        /// Mirrors the default behaviour of Unity pivots.
        /// </summary>
        /// <returns></returns>
        Vector3 calculateDefaultHandlePivotPosition()
        {
            Vector3 result = Vector3.zero;

            var gameObjects = Selection.gameObjects;
            if (gameObjects.Length >= 1)
            {
                if (Tools.pivotMode == PivotMode.Pivot)
                {
                    // pivot of last selected (from target or selection)
                    var targetGo = target as GameObject;
                    if (targetGo != null)
                        result = targetGo.transform.position;
                    else if (Selection.activeGameObject != null)
                        result = Selection.activeGameObject.transform.position;
                }
                else
                {
                    // center of all selected objects
                    // TODO: expand bounds and then take the center instead of avg.
                    var avgPos = Vector3.zero;
                    int numOfObjects = 0;
                    MeshRenderer meshRenderer;
                    foreach (var go in Selection.gameObjects)
                    {
                        numOfObjects++;
                        if (go.TryGetComponent(out meshRenderer))
                        {
                            avgPos += meshRenderer.bounds.center;
                        }
                        else
                        {
                            avgPos += go.transform.position;
                        }
                    }
                    avgPos /= numOfObjects;
                    result = avgPos;
                }
            }

            return result;
        }
        
        Vector3 calculatePivotPositionBoundBoxPosition(Vector3 currentPivot, Vector3 relativePos, bool isLocal)
        {
            Vector3 result = currentPivot;

            var gameObjects = Selection.gameObjects;
            if (gameObjects.Length >= 1 && Selection.activeGameObject != null)
            {
                var targetTransform = Selection.activeGameObject.transform;
                
                Bounds combinedBounds = new Bounds();
                bool boundsInitialized = false;

                foreach (var go in gameObjects)
                {
                    var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in meshRenderers)
                    {
                        var bounds = isLocal ? CalculateLocalBounds(meshRenderer, targetTransform) : meshRenderer.bounds;
                        if (!boundsInitialized)
                        {
                            combinedBounds = bounds;
                            boundsInitialized = true;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(bounds);
                        }
                    }
                    
                    var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (var renderer in skinnedMeshRenderers)
                    {
                        var bounds = isLocal ? CalculateLocalBounds(renderer, targetTransform) : renderer.bounds;
                        if (!boundsInitialized)
                        {
                            combinedBounds = bounds;
                            boundsInitialized = true;
                        }
                        else
                        {
                            combinedBounds.Encapsulate(bounds);
                        }
                    }
                }

                // Calculate the position within the bounds using relativePos
                if (boundsInitialized)
                {
                    if (isLocal)
                    {
                        var globalCenter = targetTransform.TransformPoint(combinedBounds.center);
                        var globalVector = targetTransform.TransformVector(new Vector3(
                            relativePos.x * combinedBounds.extents.x,
                            relativePos.y * combinedBounds.extents.y,
                            relativePos.z * combinedBounds.extents.z
                        ));
                        result = globalCenter + globalVector;
                    }
                    else
                    {
                        result = combinedBounds.center + new Vector3(
                            relativePos.x * combinedBounds.extents.x,
                            relativePos.y * combinedBounds.extents.y,
                            relativePos.z * combinedBounds.extents.z
                        );
                    }
                }
            }

            return result;
        }
        
        static Vector3[] s_tmpCorners = new Vector3[8];
        
        public static Bounds CalculateLocalBounds(Renderer renderer, Transform relativeTo)
        {
            Bounds localBounds = renderer.localBounds;

            // Get the 8 corners of the bounds in world space
            Matrix4x4 meshTransformMatrix = renderer.transform.localToWorldMatrix;
            s_tmpCorners[0] = renderer.transform.TransformPoint(localBounds.min);
            s_tmpCorners[1] = renderer.transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z));
            s_tmpCorners[2] = renderer.transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z));
            s_tmpCorners[3] = renderer.transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z));
            s_tmpCorners[4] = renderer.transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z));
            s_tmpCorners[5] = renderer.transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z));
            s_tmpCorners[6] = renderer.transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z));
            s_tmpCorners[7] = renderer.transform.TransformPoint(localBounds.max);
            
            for (int i = 0; i < 8; i++)
            {
                s_tmpCorners[i] = relativeTo.InverseTransformPoint(s_tmpCorners[i]);
            }

            // Calculate the new local bounds
            Bounds newLocalBounds = new Bounds(s_tmpCorners[0], Vector3.zero);
            foreach (Vector3 corner in s_tmpCorners)
            {
                newLocalBounds.Encapsulate(corner);
            }

            return newLocalBounds;
        }
        
        // Maybe in a future update.
        /// <summary>
        /// Mirrors the default behaviour of Unity pivots.
        /// </summary>
        /// <returns></returns>
        //Quaternion calculateDefaultHandlePivotRotation()
        //{
        //    Quaternion result = Quaternion.identity;
        //
        //    var gameObjects = Selection.gameObjects;
        //    if (gameObjects.Length >= 1)
        //    {
        //        // No matter the pivot setting (center or pivot) the rotation is always that
        //        // of the first selected object.
        //        if (Tools.pivotMode == PivotMode.Pivot)
        //        {
        //            // Rotation of last selected (from target or selection)
        //            var targetGo = target as GameObject;
        //            if (targetGo != null)
        //                result = Tools.pivotRotation == PivotRotation.Local ? targetGo.transform.rotation : Quaternion.identity;
        //            else if (Selection.activeGameObject != null)
        //                result = Tools.pivotRotation == PivotRotation.Local ? Selection.activeGameObject.transform.rotation : Quaternion.identity;
        //        }
        //    }
        //
        //    return result;
        //}

        void MoveCursor(SceneView sceneView)
        {
            var settings = PowerPivotSettings.GetOrCreateSettings();

            // draw small scale handle 
            var matrix = Handles.matrix;
            Handles.matrix = Matrix4x4.Scale(Vector3.one / 1.1f) * matrix;
            var newCursorPosition = Handles.PositionHandle(cursorPosition * 1.1f, cursorRotation) / 1.1f;
            Handles.matrix = matrix;

            if (Vector3.Magnitude(newCursorPosition - cursorPosition) > 0.0001f)
            {
                cursorPosition = newCursorPosition;

                updateCursorRelativePosition();
                GlobalTransformObserver.SetTransform(Selection.activeGameObject.transform);
            }

            // draw deco gimzos
            var rot = Quaternion.LookRotation(sceneView.camera.transform.position - cursorPosition);
            var size = HandleUtility.GetHandleSize(cursorPosition);
            if ((sceneView.camera.transform.position - cursorPosition).sqrMagnitude > 0)
                rot = Quaternion.LookRotation(sceneView.camera.transform.position - cursorPosition);
            var cA = settings.GizmoCursorColor;
            var cB = settings.GizmoColorB;
            var a = settings.GizmoOpacity;
            drawCircle(cursorPosition, rot, 0.17f * size, new Color(cA.r, cA.g, cA.b, cA.a * a * 0.8f), new Color(cB.r, cB.g, cB.b, a * 1.0f), 10);
            drawCircle(cursorPosition, rot, 0.16f * size, new Color(cA.r, cA.g, cA.b, cA.a * a * 0.8f), new Color(cB.r, cB.g, cB.b, a * 0.8f), 10);
            drawCircle(cursorPosition, rot, 0.14f * size, new Color(cA.r, cA.g, cA.b, cA.a * a * 0.6f), new Color(cB.r, cB.g, cB.b, a * 0.6f), 10);
            drawCircle(cursorPosition, rot, 0.12f * size, new Color(cA.r, cA.g, cA.b, cA.a * a * 0.4f), new Color(cB.r, cB.g, cB.b, a * 0.4f), 10);
            drawCircle(cursorPosition, rot, 0.10f * size, new Color(cA.r, cA.g, cA.b, cA.a * a * 0.2f), new Color(cB.r, cB.g, cB.b, a * 0.2f), 10);
        }

        #region Undo Stack
        /// <summary>
        /// We have to maintain our own undo/redo stacks for the cursor position because there is no object to register in undo.
        /// </summary>
        protected Stack<(Vector3, Quaternion)> cursorHistoryUndoStack = new ();
        protected Stack<(Vector3, Quaternion)> cursorHistoryRedoStack = new ();

        protected double lastCursorUndoRegistrationTime = 0;

        protected void ClearCursorUndo()
        {
            cursorHistoryUndoStack.Clear();
            cursorHistoryRedoStack.Clear();
        }

        /// <summary>
        /// Register (or update) a new undo action.
        /// </summary>
        /// <param name="cursorPosition"></param>
        /// <param name="minTimeDelta">Register a new undo if the last one is older than # seconds, otherwise update the current one.</param>
        /// <param name="forceNewGroup"></param>
        protected void RegisterCursorUndo(Vector3 cursorPosition, Quaternion cursorRotation, double minTimeDelta = 0d, bool forceNewGroup = false)
        {
            // abort if the new cursor pos is identical to the old one
            bool posIsIdentical = cursorHistoryUndoStack.Count > 0 &&
                                  (cursorHistoryUndoStack.Peek().Item1 - cursorPosition).magnitude < 0.0001f;
            bool rotIsIdentical = cursorHistoryUndoStack.Count > 0 &&
                                  Mathf.Abs(Quaternion.Angle(cursorHistoryUndoStack.Peek().Item2, cursorRotation)) < 0.0001f;
            
            if (posIsIdentical && rotIsIdentical)
                return;

            if (forceNewGroup)
            {
                cursorHistoryUndoStack.Push((cursorPosition, cursorRotation));
            }
            else
            {
                // Register a new undo if the last one is older than # seconds, otherwise update the current one.
                if (EditorApplication.timeSinceStartup - lastCursorUndoRegistrationTime < minTimeDelta)
                {
                    if (cursorHistoryUndoStack.Count > 0)
                        cursorHistoryUndoStack.Pop();
                }
                cursorHistoryUndoStack.Push((cursorPosition, cursorRotation));
            }
            lastCursorUndoRegistrationTime = EditorApplication.timeSinceStartup;
            cursorHistoryRedoStack.Clear();
        }

        protected bool HasCursorUndoActions()
        {
            return cursorHistoryUndoStack.Count > 0;
        }

        protected bool HasCursorRedoActions()
        {
            return cursorHistoryRedoStack.Count > 0;
        }

        protected void UndoCursor()
        {
            if (cursorHistoryUndoStack.Count > 0)
            {
                cursorHistoryRedoStack.Push((cursorPosition, cursorRotation));
                var v = cursorHistoryUndoStack.Pop();
                cursorPosition = v.Item1;
                cursorRotation = v.Item2;
            }
        }

        protected void RedoCursor()
        {
            if (cursorHistoryRedoStack.Count > 0)
            {
                cursorHistoryUndoStack.Push((cursorPosition, cursorRotation));
                var v = cursorHistoryRedoStack.Pop();
                cursorPosition = v.Item1;
                cursorRotation = v.Item2;
            }
        }
        #endregion
    }
}
