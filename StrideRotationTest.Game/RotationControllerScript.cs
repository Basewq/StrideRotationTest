using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.UI.Controls;
using StrideRotationTest.UI;
using System.Diagnostics;

namespace StrideRotationTest
{
    public class RotationControllerScript : SyncScript
    {
        private static readonly UIElementKey<EditTextExt> XNumberEdit = new("XNumberEdit");
        private static readonly UIElementKey<EditTextExt> YNumberEdit = new("YNumberEdit");
        private static readonly UIElementKey<EditTextExt> ZNumberEdit = new("ZNumberEdit");
        private static readonly UIElementKey<TextBlock> CurrentOrderLabel = new("CurrentOrderLabel");
        private static readonly UIElementKey<ButtonExt> OrderYXZButton = new("OrderYXZButton");
        private static readonly UIElementKey<ButtonExt> OrderZXYButton = new("OrderZXYButton");
        private static readonly UIElementKey<ButtonExt> OrderXYZButton = new("OrderXYZButton");
        private static readonly UIElementKey<ButtonExt> OrderZYXButton = new("OrderZYXButton");
        private static readonly UIElementKey<ButtonExt> UseMatrixButton = new("UseMatrixButton");
        private static readonly UIElementKey<ButtonExt> UseQuaternionButton = new("UseQuaternionButton");

        private RotationOrder _currentRotationOrder = RotationOrder.None;
        private bool _isUseMatrix = true;

        public Entity RotationTarget { get; set; }

        public override void Start()
        {
            var uiComp = Entity.Get<UIComponent>();
            Debug.Assert(uiComp != null);

            uiComp.GetUI(XNumberEdit).TextCommitted += (sender, ev) => ApplyRotation();
            uiComp.GetUI(XNumberEdit).NavigatedOut += (sender, ev) => ApplyRotation();
            uiComp.GetUI(YNumberEdit).TextCommitted += (sender, ev) => ApplyRotation();
            uiComp.GetUI(YNumberEdit).NavigatedOut += (sender, ev) => ApplyRotation();
            uiComp.GetUI(ZNumberEdit).TextCommitted += (sender, ev) => ApplyRotation();
            uiComp.GetUI(ZNumberEdit).NavigatedOut += (sender, ev) => ApplyRotation();

            uiComp.GetUI(OrderYXZButton).Click += (sender, ev) => ApplyRotation(RotationOrder.YXZ);
            uiComp.GetUI(OrderZXYButton).Click += (sender, ev) => ApplyRotation(RotationOrder.ZXY);
            uiComp.GetUI(OrderXYZButton).Click += (sender, ev) => ApplyRotation(RotationOrder.XYZ);
            uiComp.GetUI(OrderZYXButton).Click += (sender, ev) => ApplyRotation(RotationOrder.ZYX);
            uiComp.GetUI(UseMatrixButton).Click += (sender, ev) => ApplyRotation(isUseMatrix: true);
            uiComp.GetUI(UseQuaternionButton).Click += (sender, ev) => ApplyRotation(isUseMatrix: false);

            ApplyRotation(RotationOrder.YXZ);
        }

        private void ApplyRotation(RotationOrder? rotationOrder = null, bool? isUseMatrix = null)
        {
            if (RotationTarget == null) return;

            _currentRotationOrder = rotationOrder ?? _currentRotationOrder;
            _isUseMatrix = isUseMatrix ?? _isUseMatrix;

            var uiComp = Entity.Get<UIComponent>();
            var anglesRadians = GetAngles(uiComp);

            var quatX = Quaternion.RotationX(anglesRadians.X);
            var quatY = Quaternion.RotationY(anglesRadians.Y);
            var quatZ = Quaternion.RotationZ(anglesRadians.Z);

            var matX = Matrix.RotationX(anglesRadians.X);
            var matY = Matrix.RotationY(anglesRadians.Y);
            var matZ = Matrix.RotationZ(anglesRadians.Z);

            var rotQuat = Quaternion.Identity;
            var rotMat = Matrix.Identity;
            switch (_currentRotationOrder)
            {
                case RotationOrder.YXZ:
                    rotQuat = quatY * quatX * quatZ;
                    rotMat = matY * matX * matZ;
                    break;
                case RotationOrder.ZXY:
                    rotQuat = quatZ * quatX * quatY;
                    rotMat = matZ * matX * matY;
                    break;
                case RotationOrder.XYZ:
                    rotQuat = quatX * quatY * quatZ;
                    rotMat = matX * matY * matZ;
                    break;
                case RotationOrder.ZYX:
                    rotQuat = quatZ * quatY * quatX;
                    rotMat = matZ * matY * matX;
                    break;
            }
            // Proving there's no (major) difference between data types
            if (_isUseMatrix)
            {
                RotationTarget.Transform.Rotation = Quaternion.RotationMatrix(rotMat);
            }
            else
            {
                RotationTarget.Transform.Rotation = rotQuat;
            }
            uiComp.GetUI(CurrentOrderLabel).Text = $"Current Order: {_currentRotationOrder} {(_isUseMatrix ? "(Matrix)" : "(Quaternion)")}";
        }

        private Vector3 GetAngles(UIComponent uiComp)
        {
            var anglesDegrees = Vector3.Zero;
            float.TryParse(uiComp.GetUI(XNumberEdit)?.Text, out anglesDegrees.X);
            float.TryParse(uiComp.GetUI(YNumberEdit)?.Text, out anglesDegrees.Y);
            float.TryParse(uiComp.GetUI(ZNumberEdit)?.Text, out anglesDegrees.Z);

            var anglesRadians = anglesDegrees;
            anglesRadians.X = MathUtil.DegreesToRadians(anglesRadians.X);
            anglesRadians.Y = MathUtil.DegreesToRadians(anglesRadians.Y);
            anglesRadians.Z = MathUtil.DegreesToRadians(anglesRadians.Z);
            return anglesRadians;
        }

        public override void Update()
        {
            if (Input.HasKeyboard)
            {
                EditTextExt activeInput = null;
                int valueIncrement = 10;
                //if (Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift))
                //{
                //    valueIncrement = 10;
                //}
                if (Input.IsKeyPressed(Keys.Up) && TryGetActiveInput(out activeInput))
                {
                    UpdateValue(activeInput, valueIncrement);
                }
                if (Input.IsKeyPressed(Keys.Down) && TryGetActiveInput(out activeInput))
                {
                    UpdateValue(activeInput, -valueIncrement);
                }
            }

            bool TryGetActiveInput(out EditTextExt input)
            {
                var uiComp = Entity.Get<UIComponent>();

                var xInput = uiComp.GetUI(XNumberEdit);
                if (xInput.IsSelectionActive)
                {
                    input = xInput;
                    return true;
                }

                var yInput = uiComp.GetUI(YNumberEdit);
                if (yInput.IsSelectionActive)
                {
                    input = yInput; return true;
                }

                var zInput = uiComp.GetUI(ZNumberEdit);
                if (zInput.IsSelectionActive)
                {
                    input = zInput;
                    return true;
                }

                input = null;
                return false;
            }

            void UpdateValue(EditTextExt activeInput, int incrementValue)
            {
                if (float.TryParse(activeInput.Text, out float currentValue))
                {
                    currentValue += incrementValue;
                    activeInput.Text = currentValue.ToString();
                    if (_currentRotationOrder > RotationOrder.None)
                    {
                        ApplyRotation(_currentRotationOrder);
                    }
                }
            }
        }

        private enum RotationOrder
        {
            None,
            YXZ,
            ZXY,
            XYZ,
            ZYX,
        }
    }
}
