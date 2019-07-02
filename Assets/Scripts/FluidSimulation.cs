using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace FI
{

    public enum StepToShow
    {
        Density,
        Divergence,
        Pressure,
        Velocity
    }

    public class FluidSimulation : MonoBehaviour
    {

        public Texture2D obstaclesTest;

        [SerializeField]
        private StepToShow stepToShow = StepToShow.Density;

        [Header("RTextures"), SerializeField]
        private int res = 128;

        [SerializeField]
        private RenderTextureFormat densityTextureFormat = RenderTextureFormat.ARGBHalf;

        [Header("Simulation Properties"), SerializeField, Range(0.9f, 1f)]
        private float densityDissipation = 0.97f;

        [SerializeField, Range(0.9f, 1f)]
        private float velocityDissipation = 0.98f;

        [SerializeField]
        private Gradient gradient = null;

        [SerializeField, Range(0.01f, 5f)]
        private float splatRadius = 0.1f;

        [SerializeField, Range(20, 100)]
        private int pressureIterations = 20;

        [SerializeField]
        private float speedMultiplier = 1f;

        private RenderTexture densityTex, densityToShowTex, velocityTex, pressureTex, divergenceTex;

        [NonSerialized]
        private Material advectionMat, divergenceMat, splatMat, gradientSubMat, pressureMat;

        [SerializeField]
        private Material matToShow = null;

        [SerializeField]
        private Transform rawImagesContainer = null;

        private Vector3 oldPos;

        private bool bIsPressed;

        private bool bIsPaused;

        // Start is called before the first frame update
        private void Start()
        {
            CreateMaterials();
            AssignRawTextures();
        }


        // -----------------
        private void OnGUI()
        {
            if (GUI.Button(new Rect(50, 10, 70, 50), "Reset"))
            {
                ResetSimulation();
            }

            if (GUI.Button(new Rect(50, 70, 70, 50), bIsPaused ? "Resume" : "Pause"))
            {
                bIsPaused = !bIsPaused;
            }

            if (GUI.Button(new Rect(50, 130, 70, 50), "Scene"))
            {
                if (SceneManager.GetActiveScene().buildIndex == 0)
                    SceneManager.LoadScene(1);
                else
                    SceneManager.LoadScene(0);
            }
        }

        // ---------------------------
        private void ResetSimulation()
        {
            RenderTexture[] allTextures = new RenderTexture[5];

            allTextures[0] = densityTex;
            allTextures[1] = densityToShowTex;
            allTextures[2] = velocityTex;
            allTextures[3] = pressureTex;
            allTextures[4] = divergenceTex;

            for (int i = 0; i < allTextures.Length; i++)
            {
                Graphics.SetRenderTarget(allTextures[i]);
                GL.Clear(true, true, Color.clear);
            }

            Graphics.SetRenderTarget(null);
        }

        // ---------------------------
        private void CreateMaterials()
        {
            advectionMat = new Material(Shader.Find("FI/Advection"));
            divergenceMat = new Material(Shader.Find("FI/Divergence"));
            gradientSubMat = new Material(Shader.Find("FI/GradientSubstraction"));
            pressureMat = new Material(Shader.Find("FI/Pressure"));
            splatMat = new Material(Shader.Find("FI/Splat"));
        }

        // -----------------------------
        private void AssignRawTextures()
        {
            if (!rawImagesContainer)
                return;

            rawImagesContainer.GetChild(0).GetComponent<RawImage>().texture = densityToShowTex;
            rawImagesContainer.GetChild(1).GetComponent<RawImage>().texture = velocityTex;
            rawImagesContainer.GetChild(2).GetComponent<RawImage>().texture = divergenceTex;
            rawImagesContainer.GetChild(3).GetComponent<RawImage>().texture = pressureTex;
        }

        // --------------------
        private void OnEnable()
        {
            CreateTextures();
        }

        // ---------------------
        private void OnDisable()
        {
            ReleaseTextures();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!bIsPaused)
                UpdateSimulation();

            UpdateSelectedTexture();
        }

        // ----------------------------
        private void UpdateSimulation()
        {
            // Update Global Floats
            Shader.SetGlobalFloat("_TexelSize", 1.0f / res);
            Shader.SetGlobalFloat("_SpeedMultiplier", speedMultiplier);

            // Update Global Textures
            Shader.SetGlobalTexture("_VelocityTex", velocityTex);
            Shader.SetGlobalTexture("_PressureTex", pressureTex);
            Shader.SetGlobalTexture("_DivergenceTex", divergenceTex);
            Shader.SetGlobalTexture("_ObstaclesTex", obstaclesTest);

            // CURL AND VORTICITY
            RenderTexture velTemp_1 = RenderTexture.GetTemporary(res, res, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
            velTemp_1.wrapMode = TextureWrapMode.Clamp;
            velTemp_1.filterMode = FilterMode.Bilinear;

            // DUMMY BLIT => Testing Purposes
            Graphics.Blit(velocityTex, velTemp_1);

            // VELOCITY ADVECTION
            Shader.SetGlobalTexture("_VelocityTex", velTemp_1);
            Shader.SetGlobalFloat("_Dissipation", velocityDissipation);

            Graphics.Blit(velTemp_1, velocityTex, advectionMat);

            Shader.SetGlobalTexture("_VelocityTex", velocityTex);

            // DIFFUSE ADVECTION
            RenderTexture denTemp_1 = RenderTexture.GetTemporary(res, res, 0, densityTextureFormat, RenderTextureReadWrite.Linear);
            denTemp_1.wrapMode = TextureWrapMode.Clamp;
            denTemp_1.filterMode = FilterMode.Bilinear;

            Shader.SetGlobalFloat("_Dissipation", densityDissipation);
            Graphics.Blit(densityTex, denTemp_1, advectionMat);
            Graphics.Blit(denTemp_1, densityTex);

            RenderTexture denTemp_2 = RenderTexture.GetTemporary(res << 1, res << 1, 0, densityTextureFormat, RenderTextureReadWrite.Linear);
            denTemp_2.wrapMode = TextureWrapMode.Clamp;
            denTemp_2.filterMode = FilterMode.Bilinear;

            Graphics.Blit(densityTex, denTemp_2);
            Graphics.Blit(denTemp_2, densityToShowTex);

            // DIVERGENCE
            Graphics.Blit(null, divergenceTex, divergenceMat);

            // COMPUTE PRESSURE
            RenderTexture pressTemp_1 = RenderTexture.GetTemporary(res, res, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            pressTemp_1.wrapMode = TextureWrapMode.Clamp;
            pressTemp_1.filterMode = FilterMode.Bilinear;

            Graphics.SetRenderTarget(pressureTex);
            GL.Clear(true, true, Color.clear);
            Graphics.SetRenderTarget(null);

            for (int i = 0; i < pressureIterations / 2; i++)
            {
                Graphics.Blit(pressureTex, pressTemp_1, pressureMat);
                Graphics.Blit(pressTemp_1, pressureTex, pressureMat);
            }

            // SUBSTRACT PRESSURE GRADIENT
            Graphics.Blit(null, velTemp_1, gradientSubMat);
            Graphics.Blit(velTemp_1, velocityTex);

            // Release Temp RT
            RenderTexture.ReleaseTemporary(velTemp_1);
            RenderTexture.ReleaseTemporary(denTemp_1);
            RenderTexture.ReleaseTemporary(denTemp_2);
            RenderTexture.ReleaseTemporary(pressTemp_1);
        }

        // ---------------------------------
        private void UpdateSelectedTexture()
        {
            // Update the RT to Show
            RenderTexture rt = null;

            switch (stepToShow)
            {
                case StepToShow.Density:
                    rt = densityToShowTex;
                    break;
                case StepToShow.Divergence:
                    rt = divergenceTex;
                    break;
                case StepToShow.Pressure:
                    rt = pressureTex;
                    break;
                default:
                    rt = velocityTex;
                    break;
            }

            matToShow.SetTexture("_MaskTex", rt);
        }

        // ----------------------------------------------------------------
        public void ProcessHit(RaycastHit hit, Vector2 dir, bool bIsMoving)
        {
            if (bIsPaused)
                return;

            if (bIsMoving)
                Shader.SetGlobalFloat("_Radius", splatRadius / 1000f);
            else
                Shader.SetGlobalFloat("_Radius", 0.1f * splatRadius / 1000f);

            splatMat.SetVector("_Pos", hit.textureCoord);

            RenderTexture temp_1 = RenderTexture.GetTemporary(res, res, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            temp_1.wrapMode = TextureWrapMode.Clamp;
            temp_1.filterMode = FilterMode.Bilinear;

            if (densityTextureFormat == RenderTextureFormat.ARGBHalf)
            {
                matToShow.SetFloat("_IsColorful", 1f);
                splatMat.SetVector("_Color", gradient.Evaluate(Time.time % 1f));
            }
            else
            {
                matToShow.SetFloat("_IsColorful", 0f);
                splatMat.SetVector("_Color", Color.white);
            }

            Graphics.Blit(densityTex, temp_1, splatMat);
            Graphics.Blit(temp_1, densityTex);

            if (bIsMoving)
            {
                splatMat.SetVector("_Color", dir);
                Graphics.Blit(velocityTex, temp_1, splatMat);
                Graphics.Blit(temp_1, velocityTex);
            }

            RenderTexture.ReleaseTemporary(temp_1);
        }

        // --------------------------
        private void CreateTextures()
        {
            // Selective Format - RGB / R
            densityTex = new RenderTexture(res, res, 0, densityTextureFormat, RenderTextureReadWrite.Linear);
            densityTex.wrapMode = TextureWrapMode.Clamp;
            densityTex.filterMode = FilterMode.Bilinear;

            // Selective Format - RGB / R
            densityToShowTex = new RenderTexture(res << 2, res << 2, 0, densityTextureFormat, RenderTextureReadWrite.Linear);
            densityToShowTex.wrapMode = TextureWrapMode.Clamp;
            densityToShowTex.filterMode = FilterMode.Bilinear;

            // RG
            velocityTex = new RenderTexture(res, res, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
            velocityTex.wrapMode = TextureWrapMode.Clamp;
            velocityTex.filterMode = FilterMode.Bilinear;

            // R
            divergenceTex = new RenderTexture(res, res, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            divergenceTex.wrapMode = TextureWrapMode.Clamp;
            divergenceTex.filterMode = FilterMode.Bilinear;

            // R
            pressureTex = new RenderTexture(res, res, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            pressureTex.wrapMode = TextureWrapMode.Clamp;
            pressureTex.filterMode = FilterMode.Bilinear;

            // R
            pressureTex = new RenderTexture(res, res, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            pressureTex.wrapMode = TextureWrapMode.Clamp;
            pressureTex.filterMode = FilterMode.Bilinear;
        }

        // ---------------------------
        private void ReleaseTextures()
        {
            if (densityTex)
            {
                densityTex.Release();
                Destroy(densityTex);
            }

            if (densityToShowTex)
            {
                densityToShowTex.Release();
                Destroy(densityToShowTex);
            }

            if (velocityTex)
            {
                velocityTex.Release();
                Destroy(velocityTex);
            }

            if (divergenceTex)
            {
                divergenceTex.Release();
                Destroy(divergenceTex);
            }

            if (pressureTex)
            {
                pressureTex.Release();
                Destroy(pressureTex);
            }
        }

    }

}