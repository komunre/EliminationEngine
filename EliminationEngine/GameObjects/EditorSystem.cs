using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using ImGuiNET;
using EliminationEngine.Render;
using System.Text;
using System.Globalization;

namespace EliminationEngine.GameObjects
{
    public struct ObjectPreset {
        public string PresetName;
        public string Name;
        public int Id;
        public Type[] Components;
        public string ModelPath;
        public TexturePreset Texture = new TexturePreset();
        public float DisplaceDepth = 0.3f;
        
        public ObjectPreset()
        {
            PresetName = "[INVALID]";
            Name = "[INVALID]";
            Id = 0;
            Components = new Type[0];
            ModelPath = "[INVALID]";
        }
    }

    public struct TexturePreset {
        public string Name = "placeholder";
        public string DiffusePath = "displacedef.png";
        public string NormalPath = "displacedef.png";
        public string DisplacePath = "normaldef.png";

        public int ID = 0;

        public TexturePreset()
        {
        }
    }

    public struct TexturePresetLoaded
    {
        public string Name = "invalid";
        public int ID = 0;
        public TextureData Diffuse;
        public TextureData Normal;
        public TextureData Displace;

        public TexturePresetLoaded(TextureData diff, TextureData normal, TextureData displace)
        {
            Diffuse = diff;
            Normal = normal;
            Displace = displace;
        }
    }

    public enum PresetParseState
    {
        START,
        NAME,
        COMPONENTS,
        ID,
        DIFFUSE,
        NORMAL,
        DISPLACE,
        TEXNAME,
        TEXID,
        MODEL,
        DEPTH,
        UNKNOWN,
    }

    public class EditorSystem : EntitySystem
    {
        public string LoadedMap = "UNLOADED";
        public string CurrentMapPath = "UNLOADED";
        public bool EditorActive = false;
        public float EditorCameraSpeed = 4.5f;

        public static float MouseSensitivity = 25f;

        protected CameraComponent EditorCamera;
        protected bool _lastActive = false;

        protected CameraComponent? _lastActiveCamera = null;

        private List<string> _objectPresets = new();
        private int _selectedPreset = 0;
        private bool _placeObjects = false;

        public List<ObjectPreset> LoadedPresets = new();
        public Dictionary<int, TexturePresetLoaded> LoadedTextures = new();
        private Dictionary<GameObject, ObjectPreset> AddedPresets = new();

        public EditorSystem(Elimination e) : base(e)
        {
            RunsWhilePaused = true;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            var editCamObj = new GameObject();
            EditorCamera = editCamObj.AddComponent<CameraComponent>();
            var light = editCamObj.AddComponent<LightComponent>();
            light.Diffuse = 0.005f;

            if (!Directory.Exists("res/presets/")) return;
            
            foreach (var file in Directory.GetFiles("res/presets/"))
            {
                _objectPresets.Add(file);
            }
        }

        public ObjectPreset ParsePreset(string path)
        {
            var lines = File.ReadLines(path);

            var preset = new ObjectPreset();

            var comps = new List<Type>();

            var state = PresetParseState.START;
            foreach (var l in lines)
            {
                var line = l;
                line = line.Replace("\n", "");
                line = line.Replace("\t", "");
                if (line == "" || line == " ") continue;
                if (line[0] == '*')
                {
                    var category = line.Substring(1);
                    switch (category) {
                        case "name":
                            state = PresetParseState.NAME;
                            break;
                        case "model":
                            state = PresetParseState.MODEL;
                            break;
                        case "components":
                            state = PresetParseState.COMPONENTS;
                            break;
                        case "id":
                            state = PresetParseState.ID;
                            break;
                        case "diffuse":
                            state = PresetParseState.DIFFUSE;
                            break;
                        case "normal":
                            state = PresetParseState.NORMAL;
                            break;
                        case "displace":
                            state = PresetParseState.DISPLACE;
                            break;
                        case "depth":
                            state = PresetParseState.DEPTH;
                            break;
                        case "texname":
                            state = PresetParseState.TEXNAME;
                            break;
                        case "texid":
                            state = PresetParseState.TEXID;
                            break;
                        default:
                            state = PresetParseState.UNKNOWN;
                            Logger.Warn("Unknown category. Parser is set to UNKNOWN state.");
                            break;
                    }
                    continue;
                }

                switch (state)
                {
                    case PresetParseState.NAME:
                        preset.Name = line;
                        break;
                    case PresetParseState.ID:
                        preset.Id = int.Parse(line);
                        break;
                    case PresetParseState.MODEL:
                        preset.ModelPath = line;
                        break;
                    case PresetParseState.DIFFUSE:
                        preset.Texture.DiffusePath = line;
                        break;
                    case PresetParseState.NORMAL:
                        preset.Texture.NormalPath = line;
                        break;
                    case PresetParseState.DISPLACE:
                        preset.Texture.DisplacePath = line;
                        break;
                    case PresetParseState.TEXNAME:
                        preset.Texture.Name = line;
                        break;
                    case PresetParseState.TEXID:
                        preset.Texture.ID = int.Parse(line);
                        break;
                    case PresetParseState.DEPTH:
                        preset.DisplaceDepth = float.Parse(line, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
                        break;
                    case PresetParseState.UNKNOWN:
                        Logger.Warn("Line at UNKNOWN state of parser: " + line);
                        break;
                    case PresetParseState.COMPONENTS:
                        var c = Type.GetType(line);
                        if (c == null)
                        {
                            Logger.Warn("Unknwon component: " + line);
                            continue;
                        }
                        comps.Add(c);
                        break;
                }
            }
            preset.PresetName = path;
            preset.Components = comps.ToArray();
            return preset;
        }

        public void LoadAllPresets()
        {
            foreach (var preset in _objectPresets)
            {
                var p = ParsePreset(preset);
                if (!LoadedTextures.ContainsKey(p.Texture.ID))
                {
                    var texpreset = new TexturePresetLoaded();
                    texpreset.ID = p.Texture.ID;
                    texpreset.Name = p.Texture.Name;
                    texpreset.Diffuse = ImageLoader.CreateTextureFromImage(p.Texture.DiffusePath, ImageFilter.Linear);
                    texpreset.Normal = ImageLoader.CreateTextureFromImage(p.Texture.NormalPath, ImageFilter.Linear);
                    texpreset.Displace = ImageLoader.CreateTextureFromImage(p.Texture.DisplacePath, ImageFilter.Linear);
                    LoadedTextures.Add(texpreset.ID, texpreset);
                }
                LoadedPresets.Add(p);
            }
        }

        public override void OnDraw()
        {
            base.OnDraw();

            if (!EditorActive) return;

            ImGui.Begin("Editor");
            ImGui.InputText("Scene path", ref CurrentMapPath, 2000);
            if (ImGui.Button("Save scene"))
            {
                var sceneFile = File.OpenWrite(CurrentMapPath);
                var presetStr = Encoding.ASCII.GetBytes("*preset\n");
                var posStr = Encoding.ASCII.GetBytes("*pos\n");
                var rotStr = Encoding.ASCII.GetBytes("*rot\n");
                foreach (var obj in Engine.GetAllObjects())
                {
                    if (!AddedPresets.ContainsKey(obj)) continue;
                    sceneFile.Write(presetStr, 0, presetStr.Length);
                    var presetName = Encoding.ASCII.GetBytes(AddedPresets[obj].PresetName + "\n");
                    sceneFile.Write(presetName);
                    sceneFile.Write(posStr);
                    var posEnc = Encoding.ASCII.GetBytes(obj.Position.X + ":" + obj.Position.Y + ":" + obj.Position.Z + "\n");
                    sceneFile.Write(posEnc);
                    sceneFile.Write(rotStr);
                    var rotEnc = Encoding.ASCII.GetBytes(obj.DegreeRotation.X + ":" + obj.DegreeRotation.Y + ":" + obj.DegreeRotation.Z + "\n");
                    sceneFile.Write(rotEnc);
                    sceneFile.Flush();
                }
                sceneFile.Close();
            }
            if (ImGui.CollapsingHeader("Objects"))
            {
                foreach (var preset in _objectPresets)
                {
                    if (ImGui.RadioButton(preset, false))
                    {
                        var p = ParsePreset(preset);
                        var obj = new GameObject();
                        foreach (var comp in p.Components)
                        {
                            var method = comp.GetType().GetMethod("AddComponent");
                            var methodRef = method.MakeGenericMethod(comp);
                            methodRef.Invoke(obj, null);
                        }
                        obj.Name = p.Name;
                        var model = ModelParser.ParseGLTFExternal(p.ModelPath);
                        ModelHelper.AddGLTFMeshToObject(model, ref obj);
                        foreach (var mesh in obj.GetComponent<MeshGroupComponent>().Meshes)
                        {
                            if (!LoadedTextures.ContainsKey(p.Texture.ID))
                            {
                                var texpreset = new TexturePresetLoaded();
                                texpreset.ID = p.Texture.ID;
                                texpreset.Name = p.Texture.Name;
                                texpreset.Diffuse = ImageLoader.CreateTextureFromImage(p.Texture.DiffusePath, ImageFilter.Linear);
                                texpreset.Normal = ImageLoader.CreateTextureFromImage(p.Texture.NormalPath, ImageFilter.Linear);
                                texpreset.Displace = ImageLoader.CreateTextureFromImage(p.Texture.DisplacePath, ImageFilter.Linear);
                                LoadedTextures.Add(texpreset.ID, texpreset);
                            }
                            var tex = LoadedTextures[p.Texture.ID];
                            mesh._shader = MeshSystem.DefaultTexturedShader;
                            mesh._tex = tex.Diffuse.TextureID;
                            mesh._normalTex = tex.Normal.TextureID;
                            mesh._displacementTex = tex.Displace.TextureID;
                            mesh.DisplaceValue = p.DisplaceDepth;
                        }
                        Engine.AddGameObject(obj);
                        AddedPresets.Add(obj, p);
                    }
                }
            }
            ImGui.End();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Engine.KeyState.IsKeyDown(Keys.V))
            {
                Engine.ToggleCursor();
            }

            if (_lastActive != EditorActive)
            {
                _lastActive = EditorActive;
                if (EditorActive)
                {
                    foreach (var cam in Engine.GetObjectsOfType<CameraComponent>())
                    {
                        if (cam.Active)
                        {
                            _lastActiveCamera = cam;
                            _lastActiveCamera.Active = false;
                            break;
                        }
                    }
                    Engine.AddGameObject(EditorCamera.Owner);
                    EditorCamera.Active = true;
                }
                else
                {
                    EditorCamera.Active = false;
                    Engine.RemoveGameObject(EditorCamera.Owner);
                    if (_lastActiveCamera != null)
                    {
                        _lastActiveCamera.Active = true;
                    }
                }
            }

            if (!EditorActive) return;

            var moveVector = Vector3.Zero;

            var playerDirections = EditorCamera.Owner.GetDirections();

            if (Engine.KeyState.IsKeyDown(Keys.A))
            {
                moveVector += -playerDirections[1];
            }
            if (Engine.KeyState.IsKeyDown(Keys.D))
            {
                moveVector += playerDirections[1];
            }
            if (Engine.KeyState.IsKeyDown(Keys.W))
            {
                moveVector += playerDirections[0];
            }
            if (Engine.KeyState.IsKeyDown(Keys.S))
            {
                moveVector += -playerDirections[0];
            }
            if (Engine.KeyState.IsKeyDown(Keys.Q))
            {
                moveVector += -playerDirections[2];
            }
            if (Engine.KeyState.IsKeyDown(Keys.E))
            {
                moveVector += playerDirections[2];
            }

            EditorCamera.Owner.Position += moveVector * EditorCameraSpeed * Engine.DeltaTime;

            var mouseDelta = Engine.MouseState.Delta;

            var rotation = new Vector3(-mouseDelta.Y, mouseDelta.X, 0) * MouseSensitivity * Engine.DeltaTime;

            if (Engine.GetCursorLockState())
            {
                var newRot = EditorCamera.Owner.DegreeRotation + rotation;
                newRot.X = Math.Clamp(newRot.X, -75, 75);
                EditorCamera.Owner.DegreeRotation = newRot;
            }
        }
    }
}
