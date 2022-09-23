using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;
using Unity.Collections;

partial class CameraRenderer
{
    // Start is called before the first frame update
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CommandBuffer _commandBuffer;
    private const string bufferName = "Camera Render";
    private CullingResults _cullingResult;
    private static readonly List<ShaderTagId> drawingShaderTagIds = new List<ShaderTagId>{new ShaderTagId("SRPDefaultUnlit"),};

    const int maxVisibleLights = 16;

    static int visibleLightColorsId =
        Shader.PropertyToID("_VisibleLightColors");
    static int visibleLightDirectionsOrPositionsId =
        Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
    static int visibleLightAttenuationsId =
        Shader.PropertyToID("_VisibleLightAttenuations");
    static int visibleLightSpotDirectionsId =
        Shader.PropertyToID("_VisibleLightSpotDirections");
    static int lightIndicesOffsetAndCountID =
        Shader.PropertyToID("unity_LightIndicesOffsetAndCount");

    Vector4[] visibleLightColors = new Vector4[maxVisibleLights];
    Vector4[] visibleLightDirectionsOrPositions = new Vector4[maxVisibleLights];
    Vector4[] visibleLightAttenuations = new Vector4[maxVisibleLights];
    Vector4[] visibleLightSpotDirections = new Vector4[maxVisibleLights];

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _camera = camera;
        _context = context;

        if (!Cull(out var parameters))
        {
            return;
        }
        PrepareForSceneWindow();
        Settings(parameters);
        DrawVisible();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();

        if (_cullingResult.visibleLights.Length > 0)
        {
            ConfigureLights();
        }
        else
        {
            _commandBuffer.SetGlobalVector(
                lightIndicesOffsetAndCountID, Vector4.zero
            );
        }

        _commandBuffer.SetGlobalVectorArray(
        visibleLightColorsId, visibleLightColors
        );
        _commandBuffer.SetGlobalVectorArray(
            visibleLightDirectionsOrPositionsId, visibleLightDirectionsOrPositions
        );
        _commandBuffer.SetGlobalVectorArray(
            visibleLightAttenuationsId, visibleLightAttenuations
        );
        _commandBuffer.SetGlobalVectorArray(
            visibleLightSpotDirectionsId, visibleLightSpotDirections
        );
        context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();

    }

    private void Submit()
    {
        _commandBuffer.EndSample(bufferName);
        ExecuteCommandBuffer();
        _context.Submit();
    }



    private void DrawVisible()
    {
        var drawingSettings =
            CreateDrawingSettings(drawingShaderTagIds, SortingCriteria.CommonOpaque, out var sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        _context.DrawRenderers(_cullingResult, ref drawingSettings, ref filteringSettings);

        _context.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        _context.DrawRenderers(_cullingResult, ref drawingSettings, ref filteringSettings);
    }


    private void Settings(ScriptableCullingParameters parameters)
    {
        _commandBuffer = new CommandBuffer
        { name = _camera.name };
        _cullingResult = _context.Cull(ref parameters);
        _context.SetupCameraProperties(_camera);
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);
        _commandBuffer.BeginSample(bufferName);
        ExecuteCommandBuffer();


    }
    private DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTags, SortingCriteria sortingCriteria, out SortingSettings sortingSettings)
    {
        sortingSettings = new SortingSettings(_camera)
        {
            criteria = sortingCriteria,
        };

        var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);
        for (var i = 1; i < shaderTags.Count; i++)
        {
            drawingSettings.SetShaderPassName(i, shaderTags[i]);
        }

        return drawingSettings;
    }


    private bool Cull(out ScriptableCullingParameters parameters)
    {
        return _camera.TryGetCullingParameters(out parameters);
    }

    private void ExecuteCommandBuffer()
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    void ConfigureLights()
    {
        for (int i = 0; i < _cullingResult.visibleLights.Length; i++)
        {
            if (i == maxVisibleLights)
            {
                break;
            }
            VisibleLight light = _cullingResult.visibleLights[i];
            visibleLightColors[i] = light.finalColor;
            Vector4 attenuation = Vector4.zero;
            attenuation.w = 1f;

            if (light.lightType == LightType.Directional)
            {
                Vector4 v = light.localToWorldMatrix.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;
                visibleLightDirectionsOrPositions[i] = v;
            }
            else
            {
                visibleLightDirectionsOrPositions[i] =
                    light.localToWorldMatrix.GetColumn(3);
                attenuation.x = 1f /
                    Mathf.Max(light.range * light.range, 0.00001f);

                if (light.lightType == LightType.Spot)
                {
                    Vector4 v = light.localToWorldMatrix.GetColumn(2);
                    v.x = -v.x;
                    v.y = -v.y;
                    v.z = -v.z;
                    visibleLightSpotDirections[i] = v;

                    float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                    float outerCos = Mathf.Cos(outerRad);
                    float outerTan = Mathf.Tan(outerRad);
                    float innerCos =
                        Mathf.Cos(Mathf.Atan((46f / 64f) * outerTan));
                    float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                    attenuation.z = 1f / angleRange;
                    attenuation.w = -outerCos * attenuation.z;
                }
            }

            visibleLightAttenuations[i] = attenuation;
        }

        if (_cullingResult.visibleLights.Length > maxVisibleLights)
        {
            NativeArray<int> lightIndices = _cullingResult.GetLightIndexMap(Allocator.Persistent);
            for (int i = maxVisibleLights; i < _cullingResult.visibleLights.Length; i++)
            {
                lightIndices[i] = -1;
            }
            _cullingResult.SetLightIndexMap(lightIndices);
        }
    }

}

