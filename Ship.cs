/**
 * Jonah Wallschlaeger
 * 
 * Ship.cs - creates spaceship instance and applies movement transformations to the ship and camera view 
 * Uses OpenTK and wings3D because that's all that was available for graphics
 * programming for students
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Prog4
{

    public class Ship
    {
        private Vector3 cameraPosition;
        private Vector3 cameraFront;
        private Vector3 cameraRight;
        private Vector3 cameraUp;
        private float rotatedX, rotatedY, rotatedZ;
        private static Figure pod;

        private const float MAX_VELOCITY = 2.0f;
        private const float THROTTLE_INC = 0.3f;
        private float velocity = 0.0f;

        private const float ROLL_INC = 0.2f;
        private const float MAX_ROLL_SPEED = 4.5f;
        private float roll = 0.0f;

        private const float PITCH_INC = 0.15f;
        private const float MAX_PITCH_SPEED = 3.5f;
        private float pitch = 0.0f;

        private const float YAW_INC = 0.1f;
        private const float MAX_YAW_SPEED = 1.0f;
        private float yaw = 0.0f;

        private const float TRANSLATE_INC = 0.05f;
        private const float MAX_TRANSLATE_SPEED = 1.5f;
        private float translateVertical = 0.0f;
        private float translateHorizontal = 0.0f;
        private float strafe = 0.0f;

        private static Ship _instance = null;   // Singleton instance – must be private

		/// <summary>
        /// Creates new instance of ship if there is none
        /// </summary>
        public static Ship Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Ship();
                return _instance;
            }
        }

		/// <summary>
        /// Constructor for the ship object
        /// </summary>
        public Ship()
        {
            cameraPosition = new Vector3(0.0f, 25.0f, 0.0f);
            cameraFront = new Vector3(-1.0f, 0.0f, 0.0f);
            cameraRight = new Vector3(0.0f, 0.0f, 1.0f);
            cameraUp = new Vector3(0.0f, 1.0f, 0.0f);


            pod = new Figure();
            if (!pod.Load("Pod.wrl", 0.1f, 0, 1, 3)) ; ///Check if cockpit view object loaded
            pod.RotateY(MathHelper.DegreesToRadians(90.0f)); //sets direction for spawn
        }

		/// <summary>
        /// Getters for camera attritutes
        /// </summary>
        public Vector3 getCameraFront()
        {
            return cameraFront;
        }

        public Vector3 getCameraPos()
        {
            return cameraPosition;
        }

		/// <summary>
        /// current and max velocity getters for comparison
        /// </summary>
        public float CurrentThrottle
        {
            get { return velocity; }
        }

        public float MaxThrottle
        {
            get { return MAX_VELOCITY; }
        }

		/// <summary>
        /// Methods for rotating the camera and, in turn, player view
        /// </summary>
        private void MoveForward(float distance)
        {
            cameraPosition += distance * cameraFront.Normalized();
        }

        public void MoveUp(float distance)
        {
            cameraPosition += distance * cameraUp.Normalized();
        }

        public void StrafeRight(float distance)
        {
            cameraPosition -= distance * cameraRight.Normalized();
        }

        private void PitchUp(float amount)
        {
            cameraFront = Vector3.Multiply(cameraFront, (float)Math.Cos(MathHelper.DegreesToRadians(-amount))) - Vector3.Multiply(cameraUp, (float)Math.Sin(MathHelper.DegreesToRadians(-amount)));
            cameraFront.Normalize();
            cameraUp = Vector3.Cross(cameraFront, cameraRight);
            cameraUp.Normalize();

            pod.Rotate(CreateRotationMatrix(cameraRight, amount));
        }

        private void YawRight(float amount)
        {
            cameraFront = Vector3.Multiply(cameraFront, (float)Math.Cos(MathHelper.DegreesToRadians(amount))) - Vector3.Multiply(cameraRight, (float)Math.Sin(MathHelper.DegreesToRadians(amount)));
            cameraFront.Normalize();
            cameraRight = Vector3.Cross(cameraFront, cameraUp) * -1;
            cameraRight.Normalize();

            pod.Rotate(CreateRotationMatrix(cameraUp, amount));
        }

        private void RollRight(float amount)
        {
            cameraRight = Vector3.Multiply(cameraRight, (float)Math.Cos(MathHelper.DegreesToRadians(-amount))) - Vector3.Multiply(cameraUp, (float)Math.Sin(MathHelper.DegreesToRadians(-amount)));
            cameraRight.Normalize();
            cameraUp = Vector3.Cross(cameraFront, cameraRight);
            cameraUp.Normalize();

            pod.Rotate(CreateRotationMatrix(cameraFront, -amount));
        }

        public void Show(Matrix4 lookat)
        {
            Vector3 podCenter = pod.CurrentCenter;
            pod.Translate(cameraPosition.X - podCenter.X, cameraPosition.Y - podCenter.Y, cameraPosition.Z - podCenter.Z);

            pod.Show(lookat);
        }

		/// <summary>
        /// Total Move method. Called at every frame and applies all ship movements
        /// </summary>
        public void Move()
        {
            this.MoveForward(velocity);
            ApplyRoll();
            ApplyPitch();
            ApplyYaw();

            ApplyStrafe();
            ApplyTranslateVertical();
            ApplyTranslateHorizontal();
        }

        public Matrix4 LookAt()
        {
            return Matrix4.LookAt(cameraPosition, cameraPosition + cameraFront, cameraUp);
        }

		/// <summary>
        /// When applying movement, ship has a movement decay once the button input is released, because momentum 
		/// is a thing.
        /// </summary>
        private void ApplyRoll()
        {
            float newRollRight = roll + ROLL_INC;
            float newRollLeft = roll - ROLL_INC;
            //If not rolling, decay the roll.
            if (!rollingRight && !rollingLeft)
            {
                if (roll < 0)
                    roll = newRollRight > 0 ? 0 : newRollRight;
                if (roll > 0)
                    roll = newRollLeft < 0 ? 0 : newRollLeft;
            }
            else
            {
                if (rollingLeft)
                    roll = newRollLeft < -MAX_ROLL_SPEED ? -MAX_PITCH_SPEED : newRollLeft;
                if (rollingRight)
                    roll = newRollRight > MAX_PITCH_SPEED ? MAX_PITCH_SPEED : newRollRight;
            }
            RollRight(roll);
        }

        private void ApplyPitch()
        {
            float newPitchUp = pitch + PITCH_INC;
            float newPitchDown = pitch - PITCH_INC;
            //If not pitching, decay the pitch.
            if (!pitchingUp && !pitchingDown)
            {
                if (pitch < 0)
                    pitch = newPitchUp > 0 ? 0 : newPitchUp ;
                if (pitch > 0)
                    pitch = newPitchDown < 0 ? 0 : newPitchDown ;
            }
            else
            {
                if (pitchingUp)
                    pitch = newPitchUp > MAX_PITCH_SPEED ? MAX_PITCH_SPEED : newPitchUp;
                if (pitchingDown)
                    pitch = newPitchDown < -MAX_PITCH_SPEED ? -MAX_PITCH_SPEED : newPitchDown;
            }
            PitchUp(pitch);
        }

        private void ApplyYaw()
        {
            float newYawRight = yaw + YAW_INC;
            float newYawLeft = yaw - YAW_INC;
            //If not yawing, decay the yaw.
            if (!yawingRight && !yawingLeft)
            {
                if (yaw < 0)
                    yaw = newYawRight > 0 ? 0 : newYawRight;
                if (yaw > 0)
                    yaw = newYawLeft < 0 ? 0 : newYawLeft;
            }
            else
            {
                if (yawingLeft)
                    yaw = newYawLeft < -MAX_YAW_SPEED ? -MAX_PITCH_SPEED : newYawLeft;
                if (yawingRight)
                    yaw = newYawRight > MAX_PITCH_SPEED ? MAX_PITCH_SPEED : newYawRight;
            }
            YawRight(yaw);
        }

        private void ApplyStrafe()
        {
            float newStrafeRight = strafe + TRANSLATE_INC;
            float newStrafeLeft = strafe - TRANSLATE_INC;
            //If not strafing, decay the strafe.
            if (!strafingRight && !strafingLeft)
            {
                if (strafe < 0)
                    strafe = newStrafeRight > 0 ? 0 : newStrafeRight;
                if (strafe > 0)
                    strafe = newStrafeLeft < 0 ? 0 : newStrafeLeft;
            }
            else
            {
                if (strafingLeft)
                    strafe = newStrafeLeft < -MAX_TRANSLATE_SPEED ? -MAX_TRANSLATE_SPEED : newStrafeLeft;
                if (strafingRight)
                    strafe = newStrafeRight > MAX_TRANSLATE_SPEED ? MAX_TRANSLATE_SPEED : newStrafeRight;
            }
            StrafeRight(strafe);
        }

        private void ApplyTranslateVertical()
        {
            float newTranslateUp = translateVertical + TRANSLATE_INC;
            float newTranslateDown = translateVertical - TRANSLATE_INC;
            //If not translating Vertically, decay the translateVertical.
            if (!translatingUp && !translatingDown)
            {
                if (translateVertical < 0)
                    translateVertical = newTranslateUp > 0 ? 0 : newTranslateUp;
                if (translateVertical > 0)
                    translateVertical = newTranslateDown < 0 ? 0 : newTranslateDown;
            }
            else
            {
                if (translatingUp)
                    translateVertical = newTranslateUp > MAX_TRANSLATE_SPEED ? MAX_TRANSLATE_SPEED : newTranslateUp;
                if (translatingDown)
                    translateVertical = newTranslateDown < -MAX_TRANSLATE_SPEED ? -MAX_TRANSLATE_SPEED : newTranslateDown;
            }
            MoveUp(translateVertical);
        }

        private void ApplyTranslateHorizontal()
        {
            float newTranslateFoward = translateHorizontal + TRANSLATE_INC;
            float newTranslateBackward = translateHorizontal - TRANSLATE_INC;
            //If not translating horizontally, decay the translateHorizontal.
            if (!translatingForward && !translatingBackward)
            {
                if (translateHorizontal < 0)
                    translateHorizontal = newTranslateFoward > 0 ? 0 : newTranslateFoward;
                if (translateHorizontal > 0)
                    translateHorizontal = newTranslateBackward < 0 ? 0 : newTranslateBackward;
            }
            else
            {
                if (translatingForward)
                    translateHorizontal = newTranslateFoward > MAX_TRANSLATE_SPEED ? MAX_TRANSLATE_SPEED : newTranslateFoward;
                if (translatingBackward)
                    translateHorizontal = newTranslateBackward < -MAX_TRANSLATE_SPEED ? -MAX_TRANSLATE_SPEED : newTranslateBackward;
            }
            MoveForward(translateHorizontal);
        }

        /// <summary>
        /// Create rotation matrix to rotate around abritary axis.
        /// </summary>
        /// <param name="axisToRotateAbout">determines the axis the matrix will rotate about</param>
        /// <param name="angle">the amount of rotation</param>
        /// <returns></returns>
        private Matrix3 CreateRotationMatrix(Vector3 axisToRotateAbout, float angle)
        {
            float cosPheta = (float)Math.Cos(MathHelper.DegreesToRadians(angle));
            float sinPheta = (float)Math.Sin(MathHelper.DegreesToRadians(angle));
            float uX = axisToRotateAbout.X;
            float uY = axisToRotateAbout.Y;
            float uZ = axisToRotateAbout.Z;

            Matrix3 rotationMatrix = new Matrix3(cosPheta + uX * uX * (1 - cosPheta), uX * uY * (1 - cosPheta) - uZ * sinPheta, uX * uZ * (1 - cosPheta) + uY * sinPheta,
                uY * uX * (1 - cosPheta) + uZ * sinPheta, cosPheta + uY * uY * (1 - cosPheta), uY * uZ * (1 - cosPheta) - uX * sinPheta,
                uZ * uX * (1 - cosPheta) - uY * sinPheta, uZ * uY * (1 - cosPheta) + uX * sinPheta, cosPheta + uZ * uZ * (1 - cosPheta));

            return rotationMatrix;
        }

		/// <summary>
        /// Sets forward velocity, ensures ship can't reverse or go faster than max velocity 
		/// (Einstein hasn't been broken yet in this game)
        /// </summary>
        public void ThrottleUp()
        {
            velocity += THROTTLE_INC;
            if (velocity > MAX_VELOCITY)
                velocity = MAX_VELOCITY;
        }

        public void ThrottleDown()
        {
            velocity -= THROTTLE_INC;
            if (velocity < 0)
                velocity = 0;
        }

        private bool rollingLeft;
        public bool RollingLeft
        {
            set { rollingLeft = value; }
        }

        private bool rollingRight;
        public bool RollingRight
        {
            set { rollingRight = value; }
        }

        private bool pitchingUp;
        public bool PitchingUp
        {
            set { pitchingUp = value; }
        }

        private bool pitchingDown;
        public bool PitchingDown
        {
            set { pitchingDown = value; }
        }

        private bool yawingLeft;
        public bool YawingLeft
        {
            set { yawingLeft = value; }
        }

        private bool yawingRight;
        public bool YawingRight
        {
            set { yawingRight = value; }
        }

        private bool strafingLeft;
        public bool StrafingLeft
        {
            set { strafingLeft = value; }
        }

        private bool strafingRight;
        public bool StrafingRight
        {
            set { strafingRight = value; }
        }

        private bool translatingUp;
        public bool TranslatingUp
        {
            set { translatingUp = value; }
        }

        private bool translatingDown;
        public bool TranslatingDown
        {
            set { translatingDown = value; }
        }

        private bool translatingForward;
        public bool TranslatingForward
        {
            set { translatingForward = value; }
        }

        private bool translatingBackward;
        public bool TranslatingBackward
        {
            set { translatingBackward = value; }
        } 
    }
}
