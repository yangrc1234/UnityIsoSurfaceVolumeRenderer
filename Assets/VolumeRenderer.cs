using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Rendering;
public class VolumeRenderer : MonoBehaviour {
    [SerializeField]
    public Material sliceVolumeMaterial;
    public int minSampleCount;
    private RenderTexture lightBuffer;
    public Camera lightCamera;
    
    private RenderBuffer[] raycastRT;

    //let it show in gizmos
    private Quaternion sliceRotation;
    private void OnRenderObject() {
        SliceRender();
    }
    
    //how do we get light in?
    private void SliceRender() {
        if (Camera.current.cameraType != CameraType.Game|| Camera.current == lightCamera)
            return;

        if (minSampleCount < 1)
            return;

        //get bounding volume edges in worldSpace.
        var edges = getEdges();

        var cameraPos = Camera.current.transform.position;
        //transform edges into slice view space.

        Vector3 cameraDirection = Camera.current.transform.rotation * Vector3.forward;
        Vector3 lightDirection = lightCamera.transform.rotation * Vector3.forward;

        var deltaAngle = Vector3.Angle(cameraDirection, lightDirection);
        
        if (deltaAngle > 90) {
            sliceRotation = Quaternion.FromToRotation(Vector3.forward, (-cameraDirection + lightDirection) / 2);
        } else {
            sliceRotation = Quaternion.FromToRotation(Vector3.forward, (cameraDirection + lightDirection) / 2);
        }
        
        var sliceviewWorldMatrix = Matrix4x4.TRS(transform.position, sliceRotation, Vector3.one);
        var wolrdSliceviewMatrix = sliceviewWorldMatrix.inverse;

        if (deltaAngle > 90) {
            sliceVolumeMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
            sliceVolumeMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        } else {
            sliceVolumeMaterial.SetInt("_SrcBlend", (int)BlendMode.OneMinusDstAlpha);
            sliceVolumeMaterial.SetInt("_DstBlend", (int)BlendMode.One);
        }

        float planeDistance = 1.0f / minSampleCount;
        var sampleStep = Mathf.Cos(Mathf.Deg2Rad * ((90 - Mathf.Abs(deltaAngle - 90)) / 2)) * planeDistance;
        sliceVolumeMaterial.SetFloat("_SampleStep", sampleStep);

        edges = edges.Select(edge => {
            Edge res;
            res.start = wolrdSliceviewMatrix.MultiplyPoint(edge.start);
            res.end = wolrdSliceviewMatrix.MultiplyPoint(edge.end);
            return res;
        });

        //calculate the nearest vertex and farest one.
        float minZ = edges.Min(e => Mathf.Min(e.start.z, e.end.z));
        float maxZ = edges.Max(e => Mathf.Max(e.start.z, e.end.z));

        lightBuffer = RenderTexture.GetTemporary(Screen.width, Screen.height);
        sliceVolumeMaterial.SetTexture("_LightBuffer", lightBuffer);
        sliceVolumeMaterial.SetVector("_VolumeLightDir",lightCamera.transform.forward );
        RenderTexture.active = lightBuffer;
        GL.Clear(true, true, new Color(0,0,0,0));
        RenderTexture.active = null;
        //for each sample plane, get the polygon mesh, render it.
        //we render proxy front to back.
        for(float samplePlane = minZ;samplePlane<=maxZ;samplePlane += planeDistance) {
            Mesh mesh = new Mesh();
            var intersectPoints = getIntersects(edges, samplePlane);
            intersectPoints = intersectPoints.OrderByDescending(pt => {
                var sign = Mathf.Sign(Vector3.up.x * pt.y - Vector3.up.y * pt.x);
                return Vector3.Angle(pt, Vector3.up) * sign;
            });
            if (intersectPoints.Count() < 3)
                continue;
            Vector3 centerPoint = intersectPoints.Aggregate((p1, p2) => p1 + p2) / intersectPoints.Count();
            intersectPoints = new Vector3[1] { centerPoint }.Concat(intersectPoints);

            var objectSpaceIntersectPoints = intersectPoints
                .Select(pt => sliceviewWorldMatrix.MultiplyPoint(pt))            // to world space
                .Select(pt => transform.InverseTransformPoint(pt)).ToArray(); //to object space

            //form a mesh.

            var triangles = new int[(objectSpaceIntersectPoints.Length - 1) * 3];
            
            for(int i = 0; i < objectSpaceIntersectPoints.Length - 1; i++) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % (objectSpaceIntersectPoints.Length - 1)+1;
            }
            mesh.vertices = objectSpaceIntersectPoints;
            mesh.triangles = triangles;

            Matrix4x4 lightMatrix = lightCamera.projectionMatrix * lightCamera.worldToCameraMatrix * transform.localToWorldMatrix;
            Shader.SetGlobalMatrix("ObjectToLightClipPos", lightMatrix);

            //pass1 draw eye buffer.
            sliceVolumeMaterial.SetPass(0);
            Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale));

            //pass2 draw light buffer
            sliceVolumeMaterial.SetPass(1);
            RenderTexture.active = lightBuffer;
            Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale));
            RenderTexture.active = null;
        }
        lightBuffer.Release();
    }

    private void OnDrawGizmos() {
        Gizmos.DrawCube(transform.position, transform.lossyScale);
        Gizmos.DrawLine(transform.position, transform.position + sliceRotation * Vector3.forward);
    }

    private IEnumerable<Vector3> getIntersects(IEnumerable<Edge> edges, float samplePlane) {
        return edges.Where(edge => {
            return
            ((edge.start.z - samplePlane) * (edge.end.z - samplePlane)) < 0;
        }).Select(
            e => {
                return
                ((samplePlane - e.start.z) / (e.end.z - e.start.z)) * (e.end - e.start) + e.start;
            });
    }

    private static Edge[] boundingBox = new Edge[12] {
            new Edge(Vector3.zero,Vector3.right),
            new Edge(Vector3.zero,Vector3.up),
            new Edge(Vector3.up,Vector3.up + Vector3.right),
            new Edge(Vector3.right,Vector3.up+Vector3.right),

            new Edge(Vector3.zero,Vector3.forward),
            new Edge(Vector3.up,Vector3.up + Vector3.forward ),
            new Edge(Vector3.right,Vector3.right+ Vector3.forward),
            new Edge(Vector3.right + Vector3.up,Vector3.right + Vector3.up + Vector3.forward),

            new Edge(Vector3.forward,Vector3.right+ Vector3.forward),
            new Edge(Vector3.forward,Vector3.up+ Vector3.forward),
            new Edge(Vector3.up+ Vector3.forward,Vector3.up + Vector3.right+ Vector3.forward),
            new Edge(Vector3.right+ Vector3.forward,Vector3.up+Vector3.right+ Vector3.forward),
        };
    private IEnumerable<Edge> getEdges() {
        return boundingBox.Select(
            edge => {
                return new Edge(
                    transform.TransformPoint(edge.start - Vector3.one/2),
                    transform.TransformPoint(edge.end - Vector3.one/2));
            });
    }

    struct Edge {
        public Edge(Vector3 start,Vector3 end) {
            this.start = start;
            this.end = end;
        }
        public Vector3 start;
        public Vector3 end;
    }
}
