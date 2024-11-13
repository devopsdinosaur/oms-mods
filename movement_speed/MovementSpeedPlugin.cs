using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public static class PluginInfo {

	public const string TITLE = "Movement Speed";
	public const string NAME = "movement_speed";
	public const string SHORT_DESCRIPTION = "Change movement speed using configurable multipliers for all movement modes (walking, running, swimming, etc).";

	public const string VERSION = "0.0.1";

	public const string AUTHOR = "devopsdinosaur";
	public const string GAME_TITLE = "Old Market Simulator";
	public const string GAME = "oms";
	public const string GUID = AUTHOR + "." + GAME + "." + NAME;
	public const string REPO = "oms-mods";

	public static Dictionary<string, string> to_dict() {
		Dictionary<string, string> info = new Dictionary<string, string>();
		foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
			info[field.Name.ToLower()] = (string) field.GetValue(null);
		}
		return info;
	}
}

[BepInPlugin(PluginInfo.GUID, PluginInfo.TITLE, PluginInfo.VERSION)]
public class TestingPlugin : DDPlugin {
	private Harmony m_harmony = new Harmony(PluginInfo.GUID);

	private void Awake() {
		logger = this.Logger;
		try {
            this.m_plugin_info = PluginInfo.to_dict();
            Settings.Instance.load(this);
            DDPlugin.set_log_level(Settings.m_log_level.Value);
            this.create_nexus_page();
            this.m_harmony.PatchAll();
            logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
        } catch (Exception e) {
			logger.LogError("** Awake FATAL - " + e);
		}
	}

    [HarmonyPatch(typeof(CustomCharacterController), "UpdateVelocity")]
    class HarmonyPatch_CustomCharacterController_UpdateVelocity {
        private static bool Prefix(CustomCharacterController __instance, ref Vector3 currentVelocity, float deltaTime, Vector3 ____moveInputVector, bool ____sprintInputIsHeld, ref bool ____jumpedThisFrame, ref float ____timeSinceJumpRequested, ref bool ____jumpRequested, ref bool ____jumpConsumed, ref bool ____doubleJumpConsumed, ref bool ____canWallJump, ref float ____timeSinceLastAbleToJump, Vector3 ____wallJumpNormal, ref Vector3 ____internalVelocityAdd, bool ____jumpInputIsHeld, bool ____crouchInputIsHeld, Collider ____waterZone) {
            try {
                switch (__instance.CurrentCharacterState) {
                    case CharacterState.Default: {
                        Vector3 zero = Vector3.zero;
                        if (__instance.Motor.GroundingStatus.IsStableOnGround) {
                            currentVelocity = __instance.Motor.GetDirectionTangentToSurface(currentVelocity, __instance.Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                            Vector3 rhs = Vector3.Cross(____moveInputVector, __instance.Motor.CharacterUp);
                            Vector3 vector4 = Vector3.Cross(__instance.Motor.GroundingStatus.GroundNormal, rhs).normalized * ____moveInputVector.magnitude;
                            Vector2 vector5 = InputManager.Instance.inputMaster.Player.Move.ReadValue<Vector2>();
                            zero = vector4 * __instance.MaxStableMoveSpeed * Settings.m_normal_speed_multiplier.Value * ((____sprintInputIsHeld && vector5.y > 0f) ? Settings.m_sprint_speed_multiplier.Value : 1f);
                            currentVelocity = Vector3.Lerp(currentVelocity, zero, 1f - Mathf.Exp((0f - __instance.StableMovementSharpness) * deltaTime));
                        } else {
                            if (____moveInputVector.sqrMagnitude > 0f) {
                                zero = ____moveInputVector * __instance.MaxAirMoveSpeed;
                                if (__instance.Motor.GroundingStatus.FoundAnyGround) {
                                    Vector3 normalized2 = Vector3.Cross(Vector3.Cross(__instance.Motor.CharacterUp, __instance.Motor.GroundingStatus.GroundNormal), __instance.Motor.CharacterUp).normalized;
                                    zero = Vector3.ProjectOnPlane(zero, normalized2);
                                }
                                Vector3.ProjectOnPlane(zero - currentVelocity, __instance.Gravity);
                                currentVelocity += zero * __instance.AirAccelerationSpeed * deltaTime;
                            }
                            currentVelocity += __instance.Gravity * deltaTime;
                            currentVelocity *= 1f / (1f + __instance.Drag * deltaTime);
                        }
                        ____jumpedThisFrame = false;
                        ____timeSinceJumpRequested += deltaTime;
                        if (____jumpRequested) {
                            if (__instance.AllowDoubleJump && ____jumpConsumed && !____doubleJumpConsumed && (__instance.AllowJumpingWhenSliding ? (!__instance.Motor.GroundingStatus.FoundAnyGround) : (!__instance.Motor.GroundingStatus.IsStableOnGround))) {
                                __instance.Motor.ForceUnground();
                                currentVelocity += __instance.Motor.CharacterUp * __instance.JumpSpeed * Settings.m_jump_speed_multiplier.Value - Vector3.Project(currentVelocity, __instance.Motor.CharacterUp);
                                ____jumpRequested = false;
                                ____doubleJumpConsumed = true;
                                ____jumpedThisFrame = true;
                            }
                            if (____canWallJump || (!____jumpConsumed && ((__instance.AllowJumpingWhenSliding ? __instance.Motor.GroundingStatus.FoundAnyGround : __instance.Motor.GroundingStatus.IsStableOnGround) || ____timeSinceLastAbleToJump <= __instance.JumpPostGroundingGraceTime))) {
                                Vector3 vector6 = __instance.Motor.CharacterUp;
                                if (____canWallJump) {
                                    vector6 = ____wallJumpNormal;
                                } else if (__instance.Motor.GroundingStatus.FoundAnyGround && !__instance.Motor.GroundingStatus.IsStableOnGround) {
                                    vector6 = __instance.Motor.GroundingStatus.GroundNormal;
                                }
                                __instance.Motor.ForceUnground();
                                currentVelocity += vector6 * __instance.JumpSpeed * Settings.m_jump_speed_multiplier.Value - Vector3.Project(currentVelocity, __instance.Motor.CharacterUp);
                                ____jumpRequested = false;
                                ____jumpConsumed = true;
                                ____jumpedThisFrame = true;
                            }
                        }
                        ____canWallJump = false;
                        if (____internalVelocityAdd.sqrMagnitude > 0f) {
                            currentVelocity += ____internalVelocityAdd;
                            ____internalVelocityAdd = Vector3.zero;
                        }
                        break;
                    }
                    case CharacterState.Swimming: {
                        float num = 0f + (____jumpInputIsHeld ? 1f : 0f) + (____crouchInputIsHeld ? (-1f) : 0f);
                        Vector3 b = (____moveInputVector + __instance.Motor.CharacterUp * num).normalized * __instance.SwimmingSpeed * Settings.m_swim_speed_multiplier.Value;
                        Vector3 vector = Vector3.Lerp(currentVelocity, b, 1f - Mathf.Exp((0f - __instance.SwimmingMovementSharpness) * deltaTime));
                        Vector3 vector2 = __instance.Motor.TransientPosition + vector * deltaTime + (__instance.SwimmingReferencePoint.position - __instance.Motor.TransientPosition);
                        Vector3 vector3 = Physics.ClosestPoint(vector2, ____waterZone, ____waterZone.transform.position, ____waterZone.transform.rotation);
                        if (vector3 != vector2) {
                            Vector3 normalized = (vector2 - vector3).normalized;
                            vector = Vector3.ProjectOnPlane(vector, normalized);
                            if (____jumpRequested) {
                                vector += __instance.Motor.CharacterUp * __instance.JumpSpeed * Settings.m_jump_speed_multiplier.Value - Vector3.Project(currentVelocity, __instance.Motor.CharacterUp);
                            }
                        }
                        currentVelocity = vector;
                        break;
                    }
                }
                return false;
            } catch (Exception e) {
                DDPlugin._error_log("** HarmonyPatch_CustomCharacterController_UpdateVelocity.Prefix ERROR - " + e);
            }
            return true;
        }
    }
}
