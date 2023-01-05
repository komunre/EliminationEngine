using EliminationEngine.Tools;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace EliminationEngine.GameObjects
{
    public struct LineData
    {
        public int Buffer;
        public float[] Positions;

        public LineData(int buff, float[] pos)
        {
            Buffer = buff;
            Positions = pos;
        }
    }

    public class DebugRenderSystem : EntitySystem
    {
        public bool DebugActive = false;
        private List<LineData> _lines = new();

        private int _vertexArr;

        private List<GameObject> _debugObjects = new();
        private bool _lastKnownActive = false;

        private ModelParser.GLTFData _placeholder;
        public Vector3 PlaceHolderScale = Vector3.One;

        public DebugRenderSystem(Elimination e) : base(e)
        {
            RunsWhilePaused = true;
        }

        public override void OnLoad()
        {
            base.OnLoad();
            _vertexArr = GL.GenVertexArray();
            _placeholder = ModelParser.ParseGLTFExternal("res/cube-placeholder.glb");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            /*if (_lastKnownActive != DebugActive)
            {
                if (_lastKnownActive == false)
                {
                    GenDebugData();
                }
                else
                {
                    ClearDebugData();
                }
                _lastKnownActive = DebugActive;
            }*/
        }

        public override void OnDraw()
        {
            base.OnDraw();

            if (!DebugActive) return;

            ImGui.Begin("ACCESS");
            if (ImGui.CollapsingHeader("OBJECTS"))
            {
                foreach (var obj in Engine.GetAllObjects())
                {
                    if (ImGui.CollapsingHeader(obj.Id + " - " + obj.Name))
                    {
                        ImGui.Text("Position: " + obj.GlobalPosition.X + ":" + obj.GlobalPosition.Y + ":" + obj.GlobalPosition.Z);
                        ImGui.Text("Rotation: " + obj.DegreeRotation.X + ":" + obj.DegreeRotation.Y + ":" + obj.DegreeRotation.Z);
                        if (ImGui.CollapsingHeader("Directions"))
                        {
                            var dir = obj.GetDirections();
                            ImGui.Text("Forward: " + dir[0].X + ":" + dir[0].Y + ":" + dir[0].Z);
                            ImGui.Text("Right: " + dir[1].X + ":" + dir[1].Y + ":" + dir[1].Z);
                            ImGui.Text("Up: " + dir[2].X + ":" + dir[2].Y + ":" + dir[2].Z);
                        }
                        foreach (var comp in obj.GetAllComponents())
                        {
                            var type = comp.GetType();
                            if (ImGui.CollapsingHeader(type.Name))
                            {
                                var fieldInfo = type.GetFields();
                                foreach (var field in fieldInfo)
                                {
                                    var value = field.GetValue(comp);
                                    var text = value != null ? value.ToString() : "null";
                                    ImGui.Text(field.Name + ": " + text);
                                }
                            }
                        }
                    }
                }
            }
            if (ImGui.CollapsingHeader("SYSTEMS"))
            {
                foreach (var sys in Engine.GetAllSystems())
                {
                    var type = sys.GetType();
                    if (ImGui.CollapsingHeader(type.Name))
                    {
                        var fieldInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
                        foreach (var field in fieldInfo)
                        {
                            var value = field.GetValue(sys);
                            var text = value != null ? value.ToString() : "null";
                            ImGui.Text(field.Name + ": " + text);
                        }
                    }
                }
            }
            ImGui.End();

            /*foreach (var line in _lines)
            {
                GL.BindVertexArray(_vertexArr);
                GL.BindBuffer(BufferTarget.ArrayBuffer, line.Buffer);

                GL.DrawArrays(PrimitiveType.Lines, 0, 2); // TODO: Change to DrawElements, related to next TODO
            }*/
        }

        // TODO: Change it to render by triangles, so create triangles and etc
        public void AddLine(float[] positions)
        {
            var vertBuffer = GL.GenBuffer();
            GL.BindVertexArray(_vertexArr);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, positions.Length * sizeof(float), positions, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _lines.Add(new LineData(vertBuffer, positions));

            //ClearDebugData();
            //GenDebugData();
        }

        public void GenDebugData()
        {
            foreach (var hitbox in Engine.GetObjectsOfType<HitBox>())
            {
                var ownerPos = hitbox.Owner.Position;
                foreach (var box in hitbox.GetBoxes())
                {
                    var boxObj = new GameObject();
                    ModelHelper.AddGLTFMeshToObject(_placeholder, ref boxObj);
                    boxObj.Position = box.Bounds.Center + ownerPos;
                    boxObj.Scale = box.Bounds.Size;
                    Engine.AddGameObject(boxObj);
                    _debugObjects.Add(boxObj);
                }
            }

            foreach (var line in _lines)
            {
                if (line.Positions.Length < 6) break;
                var lineObj = new GameObject();
                ModelHelper.AddGLTFMeshToObject(_placeholder, ref lineObj);
                var line1 = new Vector3(line.Positions[0], line.Positions[1], line.Positions[2]);
                var line2 = new Vector3(line.Positions[3], line.Positions[4], line.Positions[5]);
                var differ = line2 - line1;
                var direction = differ.Normalized();
                var distance = differ.Length;

                lineObj.Position = differ;
                lineObj.Scale = new Vector3(Math.Abs(direction.X), Math.Abs(direction.Y), Math.Abs(direction.Z)) * 100f;
                Engine.AddGameObject(lineObj);
            }
        }

        public void ClearDebugData()
        {
            foreach (var obj in _debugObjects)
            {
                Engine.RemoveGameObject(obj);
            }
            _debugObjects.Clear();
        }
    }
}