using System.Collections;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace LethalCharger.Patches
{
    [HarmonyPatch(typeof(ItemCharger))]
    public class ItemChargerPatch
    {
        
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool ItemChargerUpdatePrefix(ItemCharger __instance,
            ref float ___updateInterval, ref InteractTrigger ___triggerScript)
        {
            if (!NetworkManager.Singleton)
                return false;

            ___updateInterval += Time.deltaTime;
            if (___updateInterval <= 1f)
                return false;

            ___updateInterval = 0f;

            var networkManager = GameNetworkManager.Instance;
            if (networkManager?.localPlayerController.currentlyHeldObjectServer != null)
            {
                ___triggerScript.interactable = true;
            }

            return false;
        }

        [HarmonyPatch("ChargeItem")]
        [HarmonyPostfix]
        public static void ChargeItemPostfix(ItemCharger __instance)
        {
            var instance = GameNetworkManager.Instance;
            
            var playerController = instance?.localPlayerController;

            if (playerController == null)
            {
                Debug.LogError("Invalid player");
                return;
            }
            
            if (playerController.currentlyHeldObjectServer != null)
            {
                Debug.LogError(playerController.serverPlayerPosition);

                if (playerController.currentlyHeldObjectServer.itemProperties.requiresBattery)
                    return;

                Landmine.SpawnExplosion(explosionPosition: playerController.serverPlayerPosition + Vector3.up, true, 5.7f,
                    6f, 50, 0f, null, false);
                return;
            }

            if (playerController is IHittable playerHittable && playerController is IShockableWithGun shockableWithGun)
            {
                Debug.Log("Игрок сует пальцы в розетку - ебашим током");

                PlayAudio(playerController.waterBubblesAudio);
                playerHittable.Hit(1, Vector3.up, playerController, true);
                shockableWithGun.ShockWithGun(playerController);
                
                __instance.StartCoroutine(StopEffectAfterDelay(shockableWithGun, 3f));
            }
        }
        
        private static IEnumerator StopEffectAfterDelay(IShockableWithGun shockableWithGun, float delay)
        {
            yield return new WaitForSeconds(delay);
            shockableWithGun.StopShockingWithGun();
        }
        
        
        private static void PlayAudio(AudioSource audioSource)
        {
            var audioClip = audioSource.clip;
            audioSource.PlayOneShot(audioClip);
        }
    }
}