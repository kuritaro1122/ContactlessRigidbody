using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KuriKit.Rigidbodys {
    public class ContactlessRigidbody : MonoBehaviour {
        [SerializeField, Min(0f), Tooltip("未実装")] float mass = 1;
        [SerializeField, Min(0f), Tooltip("未実装")] float drag = 0;
        [SerializeField, Min(0f), Tooltip("未実装")] float angularDrag = 0.05f;
        [SerializeField] public bool useGravity = false;
        [SerializeField, Min(0f)] float gravitiationalAccleleration = 9.8f;
        // positionはグローバル, rotationはローカル
        public RigidbodyConstraints constraints { get; set; } = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX;
        
        // velocity
        public Vector3 velocity { get; set; } = Vector3.zero;
        // torque
        public Vector3 localTorque {
            get { return this._localTorque; }
            set {
                this._localTorque = value;
                this._localTorqueMagnitude = this._localTorque.magnitude;
            }
        }
        private Vector3 _localTorque;
        private float _localTorqueMagnitude;
        // angularVelocity
        // centerOfMass
        // worldOfMass
        // ineriaTensor
        // inertiaTensorRotation
        // sleepThreshold

        public void AddForce(Vector3 force/*, ForceMode mode = ForceMode.Force*/) {
            this.velocity += force;
        }
        //public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force) {

        //}

        public void AddRelativeForce(Vector3 force/*, ForceMode mode = ForceMode.Force*/) {
            this.velocity = this.transform.TransformVector(force);
        }
        public void AddRelativeTorque(Vector3 torque/*, ForceMode mode = ForceMode.Force*/) {
            this.localTorque += torque;
        }
        public void AddTorque(Vector3 torque/*, ForceMode mode = ForceMode.Force*/) {
            this.localTorque += this.transform.InverseTransformVector(torque);
        }

        public void MovePosition(Vector3 position) {
            this.transform.position = position;
        }
        public void MoveRotation(Quaternion rotation) {
            this.transform.rotation = rotation;
        }

        /// <summary>
        /// ワールド座標における、Rigidbody オブジェクトの速度を取得します
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPointVelocity() {
            return Vector3.zero;
        }

        /// <summary>
        /// ローカル座標における、Rigidbody オブジェクトの相対的速度を取得します
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRelativePointVelocity() {
            return Vector3.zero;
        }

        // Update is called once per frame
        void Update() {
            UpdatePosition(Time.deltaTime);
            UpdateRotation(Time.deltaTime);
        }
        void OnDrawGizmosSelected() {
            Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.TransformVector(this.localTorque));
        }

        private void UpdatePosition(float deltaTime) {
            if (this.useGravity) this.velocity += this.gravitiationalAccleleration * Vector3.down * deltaTime;
            Vector3 _velocity = this.velocity;
            if (FlagIsOn(this.constraints, RigidbodyConstraints.FreezePositionX)) _velocity.x = 0f;
            if (FlagIsOn(this.constraints, RigidbodyConstraints.FreezePositionY)) _velocity.y = 0f;
            if (FlagIsOn(this.constraints, RigidbodyConstraints.FreezePositionZ)) _velocity.z = 0f;
            this.velocity = _velocity;
            this.transform.position += this.velocity * deltaTime;
        }
        private void UpdateRotation(float deltaTime) {
            Vector3 __localTorque = this.localTorque;
            if (FlagIsOn(this.constraints, RigidbodyConstraints.FreezeRotationX)) __localTorque.x = 0f;
            if (FlagIsOn(this.constraints, RigidbodyConstraints.FreezeRotationY)) __localTorque.y = 0f;
            if (FlagIsOn(this.constraints, RigidbodyConstraints.FreezeRotationZ)) __localTorque.z = 0f;
            this.localTorque = __localTorque;
            //Debug.Log($"{localTorque} {_localTorque} {_localTorqueMagnitude} {__localTorque}");
            this.transform.localRotation *= Quaternion.AngleAxis(this._localTorqueMagnitude * deltaTime * 90f, this._localTorque);
        }
        private static bool FlagIsOn(System.Enum bit, System.Enum flag) => FlagIsOn((int)(object)bit, (int)(object)flag);
        private static bool FlagIsOn(int bit, int flag) => (bit & flag) == flag;
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ContactlessRigidbody))]
    class ContactlessRigitbodyEditor : Editor {
        private Vector3 velocity = Vector3.zero;
        private Vector3 torque = Vector3.zero;
        private bool openConstraints = false;
        private bool openInfo = false;
        private bool openController = false;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var _target = target as ContactlessRigidbody;
            {
                this.openConstraints = EditorGUILayout.Foldout(this.openConstraints, "Constraints");
                if (this.openConstraints) {
                    void EditorConstraints(RigidbodyConstraints constraints) {
                        bool _flag = (_target.constraints & constraints) == constraints;
                        _flag = EditorGUILayout.Toggle(_flag);
                        if (_flag) _target.constraints |= constraints;
                        else _target.constraints &= ~constraints;
                    }
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Freeze Position");
                        EditorConstraints(RigidbodyConstraints.FreezePositionX);
                        EditorConstraints(RigidbodyConstraints.FreezePositionY);
                        EditorConstraints(RigidbodyConstraints.FreezePositionZ);
                        GUILayout.EndHorizontal();
                    }
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Freeze Rotation");
                        EditorConstraints(RigidbodyConstraints.FreezeRotationX);
                        EditorConstraints(RigidbodyConstraints.FreezeRotationY);
                        EditorConstraints(RigidbodyConstraints.FreezeRotationZ);
                        GUILayout.EndHorizontal();
                    }
                }
            }
            {
                this.openInfo = EditorGUILayout.Foldout(this.openInfo, "Info");
                if (this.openInfo) {

                }
            }
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("~~~ Editor ~~~");
            {
                string message = "";
                message += "RigidBodyはisKinematic=true推奨";
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }
            this.openController = EditorGUILayout.Foldout(this.openController, "Controller");
            if (this.openController) {
                this.velocity = EditorGUILayout.Vector3Field("Velocity", this.velocity);
                if (GUILayout.Button("Set")) {
                    _target.velocity = this.velocity;
                }
                if (GUILayout.Button("Add")) {
                    _target.AddForce(this.velocity);
                }
                if (GUILayout.Button("Add Relative")) {
                    _target.AddRelativeForce(this.velocity);
                }
                this.torque = EditorGUILayout.Vector3Field("Torque", this.torque);
                if (GUILayout.Button("Set")) {
                    _target.localTorque = torque;
                }
                if (GUILayout.Button("Add")) {
                    _target.AddTorque(this.torque);
                }
                if (GUILayout.Button("Add Relative")) {
                    _target.AddRelativeTorque(this.torque);
                }
            }
        }
    }
#endif
}